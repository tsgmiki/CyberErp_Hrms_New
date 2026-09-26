"use client";
import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import {
  ChevronRight,
  ChevronDown,
  Folder,
  FolderOpen,
  PanelLeftClose,
  PanelLeftOpen,
  ChevronsDownUp,
  ChevronsUpDown,
} from "lucide-react";
import { useTranslation } from "react-i18next";
import SearchBar from "@/components/common/searchBar/searchBar";

/** Generic node the tree renders. Map any domain model into this shape. */
export interface TreeViewNode {
  id: string;
  label: string;
  /** Optional right-aligned tag (e.g. a type/status). */
  badge?: string;
  children?: TreeViewNode[];
  /** Optional leading icon; overrides the default yellow folder (inherits the row's text colour). */
  icon?: ReactNode;
  /** Optional right-aligned control revealed on row hover (e.g. a delete button). */
  action?: ReactNode;
  /** When false the row can't be selected — clicking it just expands/collapses its children
   * (use for pure grouping headers). Defaults to true. */
  selectable?: boolean;
}

/** Where a dragged row was released, relative to the row under the cursor. */
type DropBand = "before" | "into" | "after";

/** One completed drag, in the shape the server's move endpoint takes. */
export interface TreeMove {
  /** The node that was dragged. */
  id: string;
  /** Its new parent; `null` makes it a root node. */
  parentId: string | null;
  /**
   * The sibling it should sit immediately AFTER; `null` puts it first.
   *
   * ⚠️ An anchor, not an index. An index stops meaning anything the moment the tree the user
   * dragged on differs from the tree on the server — "after this specific node" survives that.
   */
  afterId: string | null;
}

/** Flattened parent/child lookups, rebuilt whenever `nodes` changes. */
interface TreeIndex {
  parentOf: Map<string, string | null>;
  /** Ordered child ids, keyed by parent id — ROOT_KEY for the top level. */
  childIds: Map<string, string[]>;
}

/** Map key standing in for "no parent"; "" can never collide with a real id. */
const ROOT_KEY = "";

function buildIndex(nodes: TreeViewNode[]): TreeIndex {
  const parentOf = new Map<string, string | null>();
  const childIds = new Map<string, string[]>();
  const walk = (list: TreeViewNode[], parent: string | null) => {
    childIds.set(parent ?? ROOT_KEY, list.map((n) => n.id));
    for (const n of list) {
      parentOf.set(n.id, parent);
      if (n.children && n.children.length > 0) walk(n.children, n.id);
      else childIds.set(n.id, []);
    }
  };
  walk(nodes, null);
  return { parentOf, childIds };
}

/** Is `candidateId` somewhere beneath `ancestorId`? Iterative — a deep tree must not blow the stack. */
function isDescendant(idx: TreeIndex, ancestorId: string, candidateId: string): boolean {
  const stack = [...(idx.childIds.get(ancestorId) ?? [])];
  while (stack.length > 0) {
    const id = stack.pop()!;
    if (id === candidateId) return true;
    const kids = idx.childIds.get(id);
    if (kids) stack.push(...kids);
  }
  return false;
}

/**
 * ⚠️ THE RULE THAT PROTECTS THE TREE. Dropping a node inside its own subtree detaches that whole
 * branch: every row still has a parent and nothing in the data is malformed, it simply stops being
 * reachable from any root and disappears. A mouse can do it in one gesture, so the drag is refused
 * outright — the row shows a "no entry" cursor rather than accepting a drop that would delete a
 * branch from view. The server checks the same thing; this is the half that can say so instantly.
 */
function canDropOn(idx: TreeIndex, dragId: string, targetId: string): boolean {
  return dragId !== targetId && !isDescendant(idx, dragId, targetId);
}

/** Where `dragId` actually sits now, so a drag that changes nothing can be dropped silently. */
function currentPosition(idx: TreeIndex, dragId: string): { parentId: string | null; afterId: string | null } {
  const parentId = idx.parentOf.get(dragId) ?? null;
  const sibs = idx.childIds.get(parentId ?? ROOT_KEY) ?? [];
  const i = sibs.indexOf(dragId);
  return { parentId, afterId: i > 0 ? sibs[i - 1] : null };
}

/** Turn "dropped on node X, in band B" into a move — or null when it would change nothing. */
function resolveMove(idx: TreeIndex, dragId: string, targetId: string | null, band: DropBand): TreeMove | null {
  let parentId: string | null;
  let afterId: string | null;

  if (targetId === null) {
    // The root row: promote to the top level, at the end of it.
    parentId = null;
    const roots = (idx.childIds.get(ROOT_KEY) ?? []).filter((r) => r !== dragId);
    afterId = roots.length > 0 ? roots[roots.length - 1] : null;
  } else if (band === "into") {
    parentId = targetId;
    const kids = (idx.childIds.get(targetId) ?? []).filter((k) => k !== dragId);
    afterId = kids.length > 0 ? kids[kids.length - 1] : null;
  } else {
    parentId = idx.parentOf.get(targetId) ?? null;
    const sibs = (idx.childIds.get(parentId ?? ROOT_KEY) ?? []).filter((s) => s !== dragId);
    const i = sibs.indexOf(targetId);
    afterId = band === "after" ? targetId : i > 0 ? sibs[i - 1] : null;
  }

  const now = currentPosition(idx, dragId);
  if (now.parentId === parentId && now.afterId === afterId) return null;
  return { id: dragId, parentId, afterId };
}

/** Which third of the row the pointer is in. The middle band is widest — "into" is the common intent. */
function bandFor(e: { clientY: number }, rect: DOMRect): DropBand {
  const y = (e.clientY - rect.top) / rect.height;
  if (y < 0.3) return "before";
  if (y > 0.7) return "after";
  return "into";
}

/** Everything the rows need to take part in a drag. Assembled once by TreeView. */
interface DndState {
  dragId: string | null;
  hover: { id: string | null; band: DropBand } | null;
  index: TreeIndex;
  start: (id: string) => void;
  over: (id: string | null, band: DropBand) => void;
  drop: (id: string | null, band: DropBand) => void;
  end: () => void;
  /** Auto-expands a collapsed row the pointer has hovered over mid-drag. */
  hoverExpand: (id: string) => void;
}

export interface TreeViewProps {
  nodes: TreeViewNode[];
  selectedId?: string;
  /** Fires with the node, or `null` when the root ("all") row is chosen. */
  onSelect: (node: TreeViewNode | null) => void;
  /** Header title (already translated). Omit to hide the header. */
  title?: string;
  titleIcon?: ReactNode;
  isLoading?: boolean;
  loader?: ReactNode;
  emptyMessage?: string;
  /** When set, a top "select all / clear" row is shown; selecting it calls onSelect(null). */
  rootLabel?: string;
  /** Sidebar-style collapse of the whole panel to a rail. Default true. */
  collapsible?: boolean;
  /** Expand-all / collapse-all control in the header. Default true. */
  showExpandAll?: boolean;
  /** Search box in the header that filters nodes by label/badge. Default true. */
  searchable?: boolean;
  /** Placeholder for the search box (already translated). */
  searchPlaceholder?: string;
  /**
   * Node ids to start COLLAPSED (applied once, when first non-empty — safe for async-loaded
   * trees). Omit for the default all-expanded behaviour; the user can still toggle freely after.
   */
  defaultCollapsedIds?: string[];
  /**
   * Enable drag-and-drop reordering and reparenting. OFF by default, so every other tree in the
   * app keeps its read-only behaviour untouched.
   */
  draggable?: boolean;
  /**
   * Fires once a drop resolves to a real change. A drag that lands back where it started, or on an
   * illegal target, never reaches here.
   */
  onMove?: (move: TreeMove) => void;
  /** Extra classes for the expanded panel container. */
  className?: string;
}

interface NodeProps {
  node: TreeViewNode;
  depth: number;
  selectedId?: string;
  collapsed: Set<string>;
  toggle: (id: string) => void;
  onSelect: (node: TreeViewNode) => void;
  /** Lower-cased active search term, for highlighting. Empty when not searching. */
  query: string;
  /** Present only when drag-and-drop is enabled. */
  dnd?: DndState;
}

function TreeNode({ node, depth, selectedId, collapsed, toggle, onSelect, query, dnd }: NodeProps) {
  const hasChildren = !!node.children && node.children.length > 0;
  const isOpen = !collapsed.has(node.id);
  const isSelected = node.id === selectedId;
  const selectable = node.selectable !== false;
  // A grouping-only row (selectable=false) toggles its children on click instead of selecting.
  const activate = () => (selectable ? onSelect(node) : hasChildren && toggle(node.id));

  const isDragging = dnd?.dragId === node.id;
  const hovered = dnd?.hover?.id === node.id ? dnd.hover.band : null;
  // An illegal target is shown as illegal rather than silently ignoring the drop.
  const allowed = dnd?.dragId ? canDropOn(dnd.index, dnd.dragId, node.id) : true;

  const dragProps = dnd
    ? {
        draggable: true,
        onDragStart: (e: React.DragEvent) => {
          e.stopPropagation();
          // Required for Firefox to start a drag at all, and it makes the row's label the
          // drag image's accessible text.
          e.dataTransfer.setData("text/plain", node.id);
          e.dataTransfer.effectAllowed = "move";
          dnd.start(node.id);
        },
        onDragOver: (e: React.DragEvent) => {
          if (!dnd.dragId) return;
          const band = bandFor(e, e.currentTarget.getBoundingClientRect());
          if (!allowed) {
            e.dataTransfer.dropEffect = "none";
            return;
          }
          // preventDefault is what actually marks this a valid drop target in HTML5 DnD —
          // without it the browser refuses the drop and fires nothing.
          e.preventDefault();
          e.stopPropagation();
          e.dataTransfer.dropEffect = "move";
          dnd.over(node.id, band);
          // Dropping into a collapsed branch is impossible unless it opens, so hovering opens it.
          if (band === "into" && hasChildren && !isOpen) dnd.hoverExpand(node.id);
        },
        onDrop: (e: React.DragEvent) => {
          if (!dnd.dragId || !allowed) return;
          e.preventDefault();
          e.stopPropagation();
          dnd.drop(node.id, bandFor(e, e.currentTarget.getBoundingClientRect()));
        },
        onDragEnd: (e: React.DragEvent) => {
          e.stopPropagation();
          dnd.end();
        },
      }
    : {};

  return (
    <div>
      <div
        role="button"
        tabIndex={0}
        onClick={activate}
        onKeyDown={(e) => e.key === "Enter" && activate()}
        {...dragProps}
        // w-max lets a deep row grow past the panel so the container can scroll to it; min-w-full
        // keeps short rows full-width so the hover/selected background and the right-aligned badge
        // still span the panel.
        // The three drop bands read differently on purpose: a LINE means "between these two rows",
        // a filled ring means "inside this one". Without that distinction the two outcomes of a
        // drag look identical right up until the tree rearranges itself.
        className={`group flex w-max min-w-full cursor-pointer items-center gap-1 rounded-md px-2 py-1.5 text-sm transition-colors ${
          isSelected
            ? "bg-primary/15 font-semibold text-primary"
            : "text-sidebar-foreground hover:bg-secondary"
        } ${isDragging ? "opacity-40" : ""} ${
          hovered === "into" ? "bg-primary/20 ring-2 ring-inset ring-primary" : ""
        } ${hovered === "before" ? "border-t-2 border-t-primary" : ""} ${
          hovered === "after" ? "border-b-2 border-b-primary" : ""
        } ${dnd?.dragId && !allowed ? "cursor-no-drop" : ""}`}
        style={{ paddingLeft: depth * 16 + 8 }}
      >
        {hasChildren ? (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              toggle(node.id);
            }}
            className="shrink-0 rounded p-0.5 hover:bg-black/10"
            aria-label={isOpen ? "Collapse" : "Expand"}
          >
            {isOpen ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
          </button>
        ) : (
          <span className="w-[18px] shrink-0" />
        )}
        {node.icon ? (
          <span className="flex shrink-0 items-center">{node.icon}</span>
        ) : hasChildren && isOpen ? (
          <FolderOpen size={15} className="shrink-0 fill-amber-300 text-amber-500" />
        ) : (
          <Folder size={15} className="shrink-0 fill-amber-300 text-amber-500" />
        )}
        {/* Not `truncate`: ellipsising made deep names unreadable with no way to reveal them.
            The row scrolls horizontally instead. */}
        <span className="whitespace-nowrap">{highlight(node.label, query)}</span>
        {node.badge && (
          <span className={`shrink-0 pl-2 text-[10px] uppercase tracking-wide text-muted opacity-70 ${node.action ? "" : "ml-auto"}`}>
            {node.badge}
          </span>
        )}
        {node.action && (
          <span
            className={`shrink-0 ${node.badge ? "pl-1" : "ml-auto pl-2"}`}
            onClick={(e) => e.stopPropagation()}
          >
            {node.action}
          </span>
        )}
      </div>
      {hasChildren && isOpen && (
        <div>
          {node.children!.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              depth={depth + 1}
              selectedId={selectedId}
              collapsed={collapsed}
              toggle={toggle}
              onSelect={onSelect}
              query={query}
              dnd={dnd}
            />
          ))}
        </div>
      )}
    </div>
  );
}

const matches = (node: TreeViewNode, q: string) =>
  node.label.toLowerCase().includes(q) || (node.badge ?? "").toLowerCase().includes(q);

/**
 * Keep a node when it matches OR any descendant does — a hit five levels down is useless if its
 * ancestors are filtered away, so branches leading to a match survive. A node that matches keeps its
 * whole subtree, so selecting it still shows what it contains.
 */
function filterNodes(nodes: TreeViewNode[], q: string): TreeViewNode[] {
  const out: TreeViewNode[] = [];
  for (const n of nodes) {
    if (matches(n, q)) {
      out.push(n);
      continue;
    }
    const kids = n.children ? filterNodes(n.children, q) : [];
    if (kids.length > 0) out.push({ ...n, children: kids });
  }
  return out;
}

/** Split a label around the matched run so it can be highlighted. */
function highlight(label: string, q: string): ReactNode {
  if (!q) return label;
  const i = label.toLowerCase().indexOf(q);
  if (i < 0) return label;
  return (
    <>
      {label.slice(0, i)}
      <mark className="rounded-sm bg-amber-300/60 text-inherit">{label.slice(i, i + q.length)}</mark>
      {label.slice(i + q.length)}
    </>
  );
}

/**
 * Which branches to keep SHUT while searching: the ones that matched in their own right. Their
 * children are along for the ride (a match keeps its subtree so you can still drill in), and
 * force-expanding them buries the actual hits — searching "directorate" would re-render most of the
 * tree. Branches that only survived because a descendant matched stay open, so the hit is on screen.
 */
function collapsedDuringSearch(nodes: TreeViewNode[], q: string, acc: string[] = []): string[] {
  for (const n of nodes) {
    if (!n.children || n.children.length === 0) continue;
    if (matches(n, q)) acc.push(n.id);
    else collapsedDuringSearch(n.children, q, acc);
  }
  return acc;
}

/** Collect ids of every node that has children (for expand-all / collapse-all). */
function collectParentIds(nodes: TreeViewNode[], acc: string[] = []): string[] {
  for (const n of nodes) {
    if (n.children && n.children.length > 0) {
      acc.push(n.id);
      collectParentIds(n.children, acc);
    }
  }
  return acc;
}

/**
 * Reusable hierarchy tree: selectable nodes, per-node + panel collapse, expand/collapse-all and
 * yellow folder icons. Feed it `nodes` (any data mapped to {@link TreeViewNode}) and it stays
 * domain-agnostic — pair it with a thin data-loading wrapper per feature.
 */
function TreeView({
  nodes,
  selectedId,
  onSelect,
  title,
  titleIcon,
  isLoading = false,
  loader,
  emptyMessage,
  rootLabel,
  collapsible = true,
  showExpandAll = true,
  searchable = true,
  searchPlaceholder,
  defaultCollapsedIds,
  draggable = false,
  onMove,
  className = "",
}: TreeViewProps) {
  const { t } = useTranslation();
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set());
  const [panelCollapsed, setPanelCollapsed] = useState(false);
  const [search, setSearch] = useState("");

  // Apply the initial-collapse default ONCE when it first becomes available (trees usually load
  // async, so the mount-time state can't see it). After that the user's toggles are untouched.
  const [defaultApplied, setDefaultApplied] = useState(false);
  useEffect(() => {
    if (defaultApplied || !defaultCollapsedIds || defaultCollapsedIds.length === 0) return;
    setDefaultApplied(true);
    setCollapsed(new Set(defaultCollapsedIds));
  }, [defaultApplied, defaultCollapsedIds]);

  const toggle = (id: string) =>
    setCollapsed((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  // ---- drag and drop -------------------------------------------------------
  const [dragId, setDragId] = useState<string | null>(null);
  const [hover, setHover] = useState<{ id: string | null; band: DropBand } | null>(null);
  const expandTimer = useRef<{ id: string; timer: number } | null>(null);

  // Built from the FULL tree, never the filtered one — see the searching guard below.
  const index = useMemo(() => buildIndex(nodes), [nodes]);

  const clearExpandTimer = useCallback(() => {
    if (expandTimer.current) {
      window.clearTimeout(expandTimer.current.timer);
      expandTimer.current = null;
    }
  }, []);

  const endDrag = useCallback(() => {
    setDragId(null);
    setHover(null);
    clearExpandTimer();
  }, [clearExpandTimer]);

  const dnd: DndState | undefined = useMemo(() => {
    if (!draggable || !onMove) return undefined;
    return {
      dragId,
      hover,
      index,
      start: (id: string) => setDragId(id),
      over: (id: string | null, band: DropBand) =>
        // Only write state when the target actually changed: dragover fires continuously, and
        // setting state on every event re-renders the whole tree many times a second.
        setHover((prev) => (prev && prev.id === id && prev.band === band ? prev : { id, band })),
      drop: (id: string | null, band: DropBand) => {
        if (!dragId) return;
        const move = resolveMove(index, dragId, id, band);
        endDrag();
        // resolveMove returns null when the node landed exactly where it already was — a very
        // common way to end a drag, and not something to send to the server or toast about.
        if (move) onMove(move);
      },
      end: endDrag,
      hoverExpand: (id: string) => {
        if (expandTimer.current?.id === id) return;
        clearExpandTimer();
        expandTimer.current = {
          id,
          timer: window.setTimeout(() => {
            setCollapsed((prev) => {
              if (!prev.has(id)) return prev;
              const next = new Set(prev);
              next.delete(id);
              return next;
            });
            expandTimer.current = null;
          }, 600),
        };
      },
    };
  }, [draggable, onMove, dragId, hover, index, endDrag, clearExpandTimer]);

  // Drop the timer if the component goes away mid-drag.
  useEffect(() => clearExpandTimer, [clearExpandTimer]);

  const query = search.trim().toLowerCase();
  const visibleNodes = useMemo(
    () => (query ? filterNodes(nodes, query) : nodes),
    [nodes, query],
  );

  const parentIds = useMemo(() => collectParentIds(visibleNodes), [visibleNodes]);
  const allExpanded = collapsed.size === 0;
  const toggleAll = () => setCollapsed(allExpanded ? new Set(parentIds) : new Set());

  // While searching, open the branches that LEAD to a hit (otherwise a match hidden inside a
  // collapsed parent reads as no result) but keep the ones that matched themselves shut. The user's
  // own collapse state is untouched and comes back when the box is cleared.
  const searchCollapsed = useMemo(
    () => (query ? new Set(collapsedDuringSearch(visibleNodes, query)) : null),
    [visibleNodes, query],
  );
  const effectiveCollapsed = searchCollapsed ?? collapsed;

  // ⚠️ NO DRAGGING WHILE SEARCHING. A filtered tree is not the tree: its rows are the survivors of
  // a match, so the node visually above another may not be its real neighbour and whole branches
  // are missing. "Drop after this row" would resolve against siblings the user cannot see, and
  // land the unit somewhere they never pointed at. Clear the box to rearrange.
  const activeDnd = query ? undefined : dnd;

  // Collapsed rail — mirrors the app sidebar's collapse behaviour.
  if (collapsible && panelCollapsed) {
    return (
      <div className="flex h-full w-11 min-h-0 flex-col items-center gap-3 rounded-lg border border-border bg-card py-3">
        <button
          type="button"
          onClick={() => setPanelCollapsed(false)}
          title={t("Expand tree")}
          aria-label={t("Expand tree")}
          className="rounded-md p-1.5 text-muted transition-colors hover:bg-secondary hover:text-foreground"
        >
          <PanelLeftOpen size={18} />
        </button>
        {titleIcon}
      </div>
    );
  }

  return (
    <div
      className={`flex h-full w-full min-h-0 flex-col rounded-lg border border-border bg-card md:w-[336px] ${className}`}
    >
      {(title || collapsible || showExpandAll || searchable) && (
        <div className="shrink-0 border-b border-border">
        <div className="flex items-center gap-2 px-3 py-2 text-sm font-semibold text-foreground">
          {titleIcon}
          {title && <span className="truncate">{title}</span>}
          <div className="ml-auto flex items-center gap-0.5">
            {showExpandAll && parentIds.length > 0 && (
              <button
                type="button"
                onClick={toggleAll}
                title={allExpanded ? t("Collapse all") : t("Expand all")}
                aria-label={allExpanded ? t("Collapse all") : t("Expand all")}
                className="rounded-md p-1 text-muted transition-colors hover:bg-secondary hover:text-foreground"
              >
                {allExpanded ? <ChevronsDownUp size={16} /> : <ChevronsUpDown size={16} />}
              </button>
            )}
            {collapsible && (
              <button
                type="button"
                onClick={() => setPanelCollapsed(true)}
                title={t("Collapse tree")}
                aria-label={t("Collapse tree")}
                className="rounded-md p-1 text-muted transition-colors hover:bg-secondary hover:text-foreground"
              >
                <PanelLeftClose size={16} />
              </button>
            )}
          </div>
        </div>
        {searchable && (
          <div className="px-3 pb-2">
            <SearchBar
              value={search}
              onChange={setSearch}
              onClear={() => setSearch("")}
              placeholder={searchPlaceholder ?? t("Search")}
              className="max-w-none"
            />
          </div>
        )}
        </div>
      )}
      <div className="min-h-0 flex-1 overflow-auto p-2">
        {rootLabel && (
          // Doubles as the "make this a top-level node" drop target. Without it a node dragged out
          // of a branch has nowhere to go: every other target is inside somebody's subtree, so
          // promoting to the root would be the one rearrangement a mouse could not express.
          <div
            role="button"
            tabIndex={0}
            onClick={() => onSelect(null)}
            onKeyDown={(e) => e.key === "Enter" && onSelect(null)}
            onDragOver={
              activeDnd?.dragId
                ? (e) => {
                    e.preventDefault();
                    e.dataTransfer.dropEffect = "move";
                    activeDnd.over(null, "into");
                  }
                : undefined
            }
            onDrop={
              activeDnd?.dragId
                ? (e) => {
                    e.preventDefault();
                    activeDnd.drop(null, "into");
                  }
                : undefined
            }
            className={`mb-1 w-max min-w-full cursor-pointer whitespace-nowrap rounded-md px-2 py-1.5 text-sm transition-colors ${
              !selectedId
                ? "bg-primary/15 font-semibold text-primary"
                : "text-sidebar-foreground hover:bg-secondary"
            } ${
              activeDnd?.hover?.id === null && activeDnd?.dragId
                ? "bg-primary/20 ring-2 ring-inset ring-primary"
                : ""
            }`}
          >
            {rootLabel}
          </div>
        )}
        {isLoading && (loader ?? null)}
        {/* "Nothing here" and "nothing MATCHED" are different answers — saying "no units yet" to
            someone who just mistyped a search would be misleading. */}
        {!isLoading && nodes.length === 0 && emptyMessage && (
          <p className="px-2 py-4 text-center text-xs text-muted">{emptyMessage}</p>
        )}
        {!isLoading && nodes.length > 0 && visibleNodes.length === 0 && (
          <p className="px-2 py-4 text-center text-xs text-muted">
            {t("No matches for")} “{search.trim()}”
          </p>
        )}
        {visibleNodes.map((node) => (
          <TreeNode
            key={node.id}
            node={node}
            depth={0}
            selectedId={selectedId}
            collapsed={effectiveCollapsed}
            toggle={toggle}
            onSelect={onSelect}
            query={query}
            dnd={activeDnd}
          />
        ))}
      </div>
    </div>
  );
}

export default TreeView;
