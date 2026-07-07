"use client";
import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { Network } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { OrgUnitTreeNode } from "@/models";
import getOrganizationTree from "@/services/admin/organizationUnit/getTree";
import Loading from "@/components/common/loader/loader";
import TreeView, { type TreeViewNode } from "@/components/common/tree/treeView";

interface OrgTreeProps {
  selectedId?: string;
  onSelect: (node: OrgUnitTreeNode | null) => void;
}

function toTreeNodes(nodes: OrgUnitTreeNode[]): TreeViewNode[] {
  return nodes.map((n) => ({
    id: n.id,
    label: n.name,
    badge: n.unitType,
    children: n.children ? toTreeNodes(n.children) : undefined,
  }));
}

/**
 * Organization hierarchy tree — a thin, data-loading wrapper around the reusable
 * {@link TreeView}. Selecting a node returns the original {@link OrgUnitTreeNode}.
 */
function OrgTree({ selectedId, onSelect }: OrgTreeProps) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["organizationTree"],
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
    />
  );
}

export default OrgTree;
