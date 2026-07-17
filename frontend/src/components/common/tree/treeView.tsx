"use client";
import { useMemo, useState, type ReactNode } from "react";
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
}

function TreeNode({ node, depth, selectedId, collapsed, toggle, onSelect }: NodeProps) {
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
        className={`group flex cursor-pointer items-center gap-1 rounded-md px-2 py-1.5 text-sm transition-colors ${
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
        <span className="truncate">{node.label}</span>
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
            />
          ))}
        </div>
      )}
    </div>
  );
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
  className = "",
}: TreeViewProps) {
  const { t } = useTranslation();
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set());
  const [panelCollapsed, setPanelCollapsed] = useState(false);

  const toggle = (id: string) =>
    setCollapsed((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const parentIds = useMemo(() => collectParentIds(nodes), [nodes]);
  const allExpanded = collapsed.size === 0;
  const toggleAll = () => setCollapsed(allExpanded ? new Set(parentIds) : new Set());

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
      {(title || collapsible || showExpandAll) && (
        <div className="flex items-center gap-2 border-b border-border px-3 py-2 text-sm font-semibold text-foreground">
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
      )}
      <div className="min-h-0 flex-1 overflow-auto p-2">
        {rootLabel && (
          <div
            role="button"
            tabIndex={0}
            onClick={() => onSelect(null)}
            onKeyDown={(e) => e.key === "Enter" && onSelect(null)}
            className={`mb-1 cursor-pointer rounded-md px-2 py-1.5 text-sm transition-colors ${
              !selectedId
                ? "bg-primary/15 font-semibold text-primary"
                : "text-sidebar-foreground hover:bg-secondary"
            }`}
          >
            {rootLabel}
          </div>
        )}
        {isLoading && (loader ?? null)}
        {!isLoading && nodes.length === 0 && emptyMessage && (
          <p className="px-2 py-4 text-center text-xs text-muted">{emptyMessage}</p>
        )}
        {nodes.map((node) => (
          <TreeNode
            key={node.id}
            node={node}
            depth={0}
            selectedId={selectedId}
            collapsed={collapsed}
            toggle={toggle}
            onSelect={onSelect}
          />
        ))}
      </div>
    </div>
  );
}

export default TreeView;
