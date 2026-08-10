"use client";
import { useEffect, useMemo, useState, type ReactNode } from "react";
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
}

function TreeNode({ node, depth, selectedId, collapsed, toggle, onSelect, query }: NodeProps) {
  const hasChildren = !!node.children && node.children.length > 0;
  const isOpen = !collapsed.has(node.id);
  const isSelected = node.id === selectedId;
  const selectable = node.selectable !== false;
  // A grouping-only row (selectable=false) toggles its children on click instead of selecting.
  const activate = () => (selectable ? onSelect(node) : hasChildren && toggle(node.id));

  return (
    <div>
      <div
        role="button"
        tabIndex={0}
        onClick={activate}
        onKeyDown={(e) => e.key === "Enter" && activate()}
        // w-max lets a deep row grow past the panel so the container can scroll to it; min-w-full
        // keeps short rows full-width so the hover/selected background and the right-aligned badge
        // still span the panel.
        className={`group flex w-max min-w-full cursor-pointer items-center gap-1 rounded-md px-2 py-1.5 text-sm transition-colors ${
          isSelected
            ? "bg-primary/15 font-semibold text-primary"
            : "text-sidebar-foreground hover:bg-secondary"
        }`}
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
          <div
            role="button"
            tabIndex={0}
            onClick={() => onSelect(null)}
            onKeyDown={(e) => e.key === "Enter" && onSelect(null)}
            className={`mb-1 w-max min-w-full cursor-pointer whitespace-nowrap rounded-md px-2 py-1.5 text-sm transition-colors ${
              !selectedId
                ? "bg-primary/15 font-semibold text-primary"
                : "text-sidebar-foreground hover:bg-secondary"
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
          />
        ))}
      </div>
    </div>
  );
}

export default TreeView;
