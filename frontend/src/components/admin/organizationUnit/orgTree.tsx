"use client";
import { useCallback, useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Network } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { OrgUnitTreeNode } from "@/models";
import getOrganizationTree from "@/services/admin/organizationUnit/getTree";
import moveOrganizationUnit from "@/services/admin/organizationUnit/move";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";
import TreeView, { type TreeMove, type TreeViewNode } from "@/components/common/tree/treeView";

interface OrgTreeProps {
  selectedId?: string;
  onSelect: (node: OrgUnitTreeNode | null) => void;
  /**
   * Allow rearranging the hierarchy by dragging. Defaults to true — this screen IS the structure
   * editor. Pass false wherever the tree is only a filter.
   */
  canRearrange?: boolean;
}

const TREE_KEY = ["organizationTree"];

function toTreeNodes(nodes: OrgUnitTreeNode[]): TreeViewNode[] {
  return nodes.map((n) => ({
    id: n.id,
    label: n.name,
    badge: n.unitType,
    children: n.children ? toTreeNodes(n.children) : undefined,
  }));
}

/**
 * Apply a move to the cached tree, so the row lands where it was dropped immediately.
 *
 * <p>The server is the authority and the query is refetched afterwards regardless — this only
 * removes the pause between releasing the mouse and the tree catching up. Without it the row
 * springs back to where it came from and then jumps again a moment later, which reads as a failed
 * drag.</p>
 *
 * <p>Pure: it rebuilds the arrays rather than splicing the cached ones, because React Query hands
 * out the cached object itself and mutating it would change what a rollback restores.</p>
 */
function applyMove(nodes: OrgUnitTreeNode[], move: TreeMove): OrgUnitTreeNode[] {
  let moved: OrgUnitTreeNode | undefined;

  // 1. Lift the node out of wherever it currently is.
  const lift = (list: OrgUnitTreeNode[]): OrgUnitTreeNode[] =>
    list.reduce<OrgUnitTreeNode[]>((acc, n) => {
      if (n.id === move.id) {
        moved = n;
        return acc;
      }
      acc.push(n.children ? { ...n, children: lift(n.children) } : n);
      return acc;
    }, []);

  const without = lift(nodes);
  if (!moved) return nodes;
  const node = moved;

  // 2. Drop it back in, after the anchor (or first when there is none).
  const insert = (list: OrgUnitTreeNode[]): OrgUnitTreeNode[] => {
    if (move.afterId === null) return [node, ...list];
    const i = list.findIndex((n) => n.id === move.afterId);
    // An anchor that is not here means the cache is out of step with what was dragged; append
    // rather than silently dropping the node, and let the refetch settle the truth.
    if (i < 0) return [...list, node];
    return [...list.slice(0, i + 1), node, ...list.slice(i + 1)];
  };

  if (move.parentId === null) return insert(without);

  const intoParent = (list: OrgUnitTreeNode[]): OrgUnitTreeNode[] =>
    list.map((n) =>
      n.id === move.parentId
        ? { ...n, children: insert(n.children ?? []) }
        : n.children
          ? { ...n, children: intoParent(n.children) }
          : n,
    );

  return intoParent(without);
}

/**
 * Organization hierarchy tree — a thin, data-loading wrapper around the reusable
 * {@link TreeView}. Selecting a node returns the original {@link OrgUnitTreeNode}; dragging one
 * reparents or reorders it (HC001–HC003).
 */
function OrgTree({ selectedId, onSelect, canRearrange = true }: OrgTreeProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: TREE_KEY,
    queryFn: getOrganizationTree,
  });

  const nodes = useMemo(() => toTreeNodes(data ?? []), [data]);

  const byId = useMemo(() => {
    const map = new Map<string, OrgUnitTreeNode>();
    const walk = (list: OrgUnitTreeNode[]) =>
      list.forEach((n) => {
        map.set(n.id, n);
        if (n.children) walk(n.children);
      });
    walk(data ?? []);
    return map;
  }, [data]);

  const { mutate: move } = useMutation({
    mutationFn: moveOrganizationUnit,
    onMutate: async (variables) => {
      // Stop an in-flight refetch from landing on top of the optimistic tree and undoing it.
      await queryClient.cancelQueries({ queryKey: TREE_KEY });
      const previous = queryClient.getQueryData<OrgUnitTreeNode[]>(TREE_KEY);
      if (previous) queryClient.setQueryData(TREE_KEY, applyMove(previous, variables));
      return { previous };
    },
    onError: (error, _variables, context) => {
      // Put the tree back exactly as it was. The server refuses cycles, stale anchors and
      // out-of-scope units, and every one of those must leave the screen truthful.
      if (context?.previous) queryClient.setQueryData(TREE_KEY, context.previous);
      toast.error(error instanceof Error ? error.message : t("Could not move that unit."));
    },
    onSuccess: () => {
      toast.success(t("Structure updated"));
    },
    onSettled: () => {
      // Always reconcile: the server resequences the whole level, and its numbering is the one
      // the next drag has to reason about.
      queryClient.invalidateQueries({ queryKey: TREE_KEY });
      // A move changes each unit's parent, which the grid shows.
      queryClient.invalidateQueries({ queryKey: ["organizationUnits"], refetchType: "none" });
    },
  });

  const onMove = useCallback((m: TreeMove) => move(m), [move]);

  return (
    <TreeView
      nodes={nodes}
      selectedId={selectedId}
      onSelect={(tn) => onSelect(tn ? byId.get(tn.id) ?? null : null)}
      title={t("Organization Structure")}
      titleIcon={<Network size={15} className="text-primary" />}
      isLoading={isLoading}
      loader={<Loading />}
      rootLabel={t("All Units")}
      emptyMessage={t("No organization units yet. Use Add to create a root unit.")}
      draggable={canRearrange}
      onMove={canRearrange ? onMove : undefined}
    />
  );
}

export default OrgTree;
