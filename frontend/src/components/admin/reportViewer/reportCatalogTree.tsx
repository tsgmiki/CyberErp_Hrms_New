"use client";
import { useMemo, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { FolderTree, FileBarChart, Bookmark, X } from "lucide-react";
import TreeView, { type TreeViewNode } from "@/components/common/tree/treeView";
import Loading from "@/components/common/loader/loader";
import type { ReportCatalogGroupModel, ReportCatalogItemModel } from "@/models";

interface ReportCatalogTreeProps {
  groups: ReportCatalogGroupModel[];
  loading?: boolean;
  /** reportKey of the currently open report (highlights its node). */
  selectedKey?: string;
  onSelectReport: (report: ReportCatalogItemModel) => void;
  onLoadFilter: (filterId: string, report: ReportCatalogItemModel) => void;
  onRemoveFilter: (filterId: string) => void;
  className?: string;
}

// Node-id prefixes let onSelect (which only gets the node) dispatch to the right callback.
const GRP = "grp:", RPT = "rpt:", FLT = "flt:";

/**
 * Report catalog rail — a thin data-loading wrapper over the shared, reusable {@link TreeView}
 * (same tree the Org/Employee/Position screens use), so the report catalog gets the app's standard
 * professional tree look: collapsible group folders → reports → saved filters, expand-all, and a
 * collapse-to-rail. Groups are grouping-only headers (click to expand); reports select; saved filters
 * load, with a hover-reveal delete. Mirrors `organizationUnit/orgTree.tsx`.
 */
function ReportCatalogTree({
  groups, loading, selectedKey, onSelectReport, onLoadFilter, onRemoveFilter, className,
}: ReportCatalogTreeProps) {
  const { t } = useTranslation();

  const { nodes, reportByKey, filterById } = useMemo(() => {
    const reportByKey = new Map<string, ReportCatalogItemModel>();
    const filterById = new Map<string, ReportCatalogItemModel>();
    const nodes: TreeViewNode[] = groups.map((g) => ({
      id: `${GRP}${g.grouping}`,
      label: g.grouping,
      badge: String(g.reports.length),
      selectable: false, // grouping header — click toggles its reports
      children: g.reports.map((r) => {
        reportByKey.set(r.reportKey, r);
        return {
          id: `${RPT}${r.reportKey}`,
          label: r.reportName,
          icon: <FileBarChart size={14} />,
          children: (r.savedFilters ?? []).map((sf): TreeViewNode => {
            filterById.set(sf.id, r);
            return {
              id: `${FLT}${sf.id}`,
              label: sf.name,
              icon: <Bookmark size={12} />,
              action: (
                <button
                  type="button"
                  onClick={() => onRemoveFilter(sf.id)}
                  className="rounded p-0.5 text-muted opacity-0 transition-opacity hover:text-error group-hover:opacity-100"
                  aria-label={t("Delete saved filter") ?? "Delete saved filter"}
                >
                  <X size={12} />
                </button>
              ),
            };
          }),
        } as TreeViewNode;
      }),
    }));
    return { nodes, reportByKey, filterById };
  }, [groups, onRemoveFilter, t]);

  const handleSelect = (node: TreeViewNode | null) => {
    if (!node) return;
    if (node.id.startsWith(RPT)) {
      const r = reportByKey.get(node.id.slice(RPT.length));
      if (r) onSelectReport(r);
    } else if (node.id.startsWith(FLT)) {
      const r = filterById.get(node.id.slice(FLT.length));
      if (r) onLoadFilter(node.id.slice(FLT.length), r);
    }
  };

  return (
    <TreeView
      nodes={nodes}
      selectedId={selectedKey ? `${RPT}${selectedKey}` : undefined}
      onSelect={handleSelect}
      title={t("Report Catalog")}
      titleIcon={<FolderTree size={16} className="text-primary" /> as ReactNode}
      isLoading={loading}
      loader={<Loading />}
      emptyMessage={t("No reports in the catalog.") ?? undefined}
      className={className}
    />
  );
}

export default ReportCatalogTree;
