"use client";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import GridDataTableProvider from "@/components/common/dataTableProvider/gridDataTableProvider";
import { useListPage } from "@/components/common/dataTableProvider/useListPage";
import type { ListDisplayMode } from "@/components/common/dataTableProvider/listViewToolbar";
import { parameterInitialData } from "@/constants/initialization";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type ParameterModel from "@/models/ParameterModel";
import type { ReportColumnModel, ReportResultModel } from "@/models";
import { generateReport } from "@/services/admin/report";
import Loading from "@/components/common/loader/loader";

const fmtCell = (value: unknown, type: string): string => {
  if (value === null || value === undefined) return "";
  switch (type) {
    case "date": return String(value).slice(0, 10);
    case "datetime": return String(value).replace("T", " ").slice(0, 16);
    case "currency": return Number(value).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    case "number": return Number(value).toLocaleString();
    case "boolean": return value === true || value === 1 || String(value).toLowerCase() === "true" ? "Yes" : "No";
    default: return String(value);
  }
};
const isNumeric = (t: string) => t === "currency" || t === "number";

/**
 * Generated report page, opened IN A NEW TAB (reference _GenerateReport.cshtml via window.open) as a
 * DEDICATED FULL-SCREEN grid — no app shell. Uses the project's STANDARD list grid (DataTableProvider +
 * useListPage) so it gets the same search bar, column selector, Excel/CSV export, and list/grid toggle
 * as every other list in the app. The data is client-side (already generated), so search / sort /
 * paging are applied here off the shared `param` the standard chrome drives. Payload via sessionStorage.
 */
function ReportResult() {
  const { t } = useTranslation();
  const [result, setResult] = useState<ReportResultModel | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [displayMode, setDisplayMode] = useState<ListDisplayMode>("list");
  const [param, setParam] = useState<ParameterModel>(
    () => ({ ...parameterInitialData, take: 25, sortCol: "", dir: "asc" }) as ParameterModel,
  );
  // Pivot / grouping — the effective group-by columns for this run (report default or user override).
  const [groupFields, setGroupFields] = useState<string[]>([]);

  useEffect(() => {
    (async () => {
      const raw = sessionStorage.getItem("reportRun");
      if (!raw) { setError("No report request found — generate from the Reports page."); return; }
      const { reportKey, values, outputFields, groupBy } = JSON.parse(raw);
      setGroupFields(Array.isArray(groupBy) ? groupBy.filter(Boolean) : []);
      const res = await generateReport(reportKey, values, outputFields);
      if (!res.ok) { setError(res.message || "Failed to generate report"); return; }
      setResult(res.result!);
    })();
  }, []);

  const cols = useMemo(() => result?.columns ?? [], [result]);
  const colByField = useMemo(() => {
    const m: Record<string, ReportColumnModel> = {};
    for (const c of cols) m[c.field] = c;
    return m;
  }, [cols]);

  // Report columns → standard grid columns (typed formatting + drill-down links preserved).
  const columns = useMemo(
    () => cols.map((c) => ({
      name: c.field,
      label: c.label,
      sort: true,
      render: (_t: unknown, row: Record<string, unknown>) => (
        <span className={isNumeric(c.type) ? "block text-right tabular-nums" : undefined}>
          {c.linkPage && c.linkPageValue
            ? <a href={`/${c.linkPage}${row[c.linkPageValue] ?? ""}`} target="_blank" rel="noreferrer" className="text-primary underline">{fmtCell(row[c.field], c.type)}</a>
            : fmtCell(row[c.field], c.type)}
        </span>
      ),
    })) as DataTableColumnModel[],
    [cols],
  );

  // Client-side search + sort driven by the standard chrome's `param`.
  const filtered = useMemo(() => {
    let rows = (result?.rows ?? []) as Record<string, unknown>[];
    const q = (param.searchText ?? "").trim().toLowerCase();
    if (q) rows = rows.filter((r) => cols.some((c) => fmtCell(r[c.field], c.type).toLowerCase().includes(q)));
    if (param.sortCol) {
      const type = colByField[param.sortCol]?.type ?? "string";
      const dir = String(param.dir).toUpperCase() === "DESC" ? -1 : 1;
      rows = [...rows].sort((a, b) => {
        const av = a[param.sortCol], bv = b[param.sortCol];
        const cmp = isNumeric(type) ? (Number(av) || 0) - (Number(bv) || 0) : String(av ?? "").localeCompare(String(bv ?? ""));
        return dir * cmp;
      });
    }
    return rows;
  }, [result, param.searchText, param.sortCol, param.dir, cols, colByField]);

  const pageRows = useMemo(() => filtered.slice(param.skip, param.skip + param.take), [filtered, param.skip, param.take]);

  // Pivot mode: bucket rows by a COMPOSITE key of the group-by columns (the standard grid groups by one
  // field, so multi-level grouping collapses to one hierarchical key) and render every row (no paging,
  // so groups stay whole). The group header shows the field labels + values + row count.
  const groupedRows = useMemo(() => {
    if (groupFields.length === 0) return null;
    return filtered.map((r) => ({
      ...r,
      __grp: groupFields.map((f) => fmtCell(r[f], colByField[f]?.type ?? "string")).join("  ▸  "),
    }));
  }, [filtered, groupFields, colByField]);
  const grouped = groupedRows !== null;

  // PIVOT SUBTOTALS: the grouping SP's 3rd result set — one row per leaf group with server-computed
  // GroupCount + SalaryTotal. Keyed by the SAME composite as the grid's group buckets so each collapsible
  // group header can show its server-side subtotal (the T-SQL analogue of legacy ReportGroupedExportBuilder).
  const summaryByKey = useMemo(() => {
    const m = new Map<string, { count: number; salaryTotal: number | null }>();
    for (const s of result?.summaries ?? []) {
      const key = groupFields.map((f) => fmtCell(s[f], colByField[f]?.type ?? "string")).join("  ▸  ");
      const salary = s.SalaryTotal;
      m.set(key, {
        count: Number(s.GroupCount ?? 0),
        salaryTotal: salary === null || salary === undefined ? null : Number(salary),
      });
    }
    return m;
  }, [result, groupFields, colByField]);

  const groupLabel = (_key: string, groupRows: Record<string, unknown>[]) => {
    const label = groupFields
      .map((f) => `${colByField[f]?.label ?? f}: ${fmtCell(groupRows[0]?.[f], colByField[f]?.type ?? "string")}`)
      .join("     •     ");
    const s = summaryByKey.get(_key);
    if (!s) return label;
    const totals = [`${s.count} ${t("rows")}`];
    if (s.salaryTotal !== null) totals.push(`${t("Salary")}: ${fmtCell(s.salaryTotal, "currency")}`);
    return `${label}     —     ${totals.join("  •  ")}`;
  };

  // The standard toolbar: column selector + Excel/CSV/PDF export + list/grid toggle.
  const { displayColumns, toolbarEnd } = useListPage({
    listKey: `report-result:${result?.reportKey ?? "x"}`,
    listLabel: result?.reportName ?? "Report",
    columns,
    data: filtered,
    displayMode,
    onDisplayModeChange: setDisplayMode,
    totalCount: filtered.length,
    fetchAllData: async () => filtered,
  });

  const dataTable = grouped
    ? {
        columns: displayColumns as never,
        data: groupedRows as never,
        count: filtered.length,
        param, setParam,
        pagination: "None" as const,
        search: "Visible" as const,
        showSort: true,
        groupBy: "__grp",
        getGroupLabel: groupLabel,
        toolbarEnd,
        key: `report-grp-${result?.reportKey ?? ""}`,
      }
    : {
        columns: displayColumns as never,
        data: pageRows as never,
        count: filtered.length,
        param, setParam,
        pagination: "Visible" as const,
        search: "Visible" as const,
        showSort: true,
        toolbarEnd,
        key: `report-${result?.reportKey ?? ""}`,
      };

  if (error) return <div className="p-6 text-sm text-error">{error}</div>;
  if (!result) return <Loading />;

  return (
    <div className="flex h-screen min-h-0 flex-col gap-3 bg-background p-4 text-foreground">
      <div className="shrink-0">
        <h2 className="text-base font-semibold">
          {result.reportName} <span className="text-sm font-normal text-muted">({result.total} {t("rows")})</span>
        </h2>
        {grouped && (
          <p className="mt-0.5 text-xs text-muted">
            {t("Grouped by")}: {groupFields.map((f) => colByField[f]?.label ?? f).join(" ▸ ")}
            {(result.summaries?.length ?? 0) > 0 && (
              <span className="ml-2 text-muted">
                · {result.summaries!.length} {t("groups")}
                {(() => {
                  const grand = (result.summaries ?? []).reduce((a, s) => a + (Number(s.SalaryTotal) || 0), 0);
                  return grand ? ` · ${t("Salary")}: ${fmtCell(grand, "currency")}` : "";
                })()}
              </span>
            )}
          </p>
        )}
      </div>
      {displayMode === "grid"
        ? <GridDataTableProvider dataTable={dataTable} />
        : <DataTableProvider dataTable={dataTable} />}
    </div>
  );
}

export default ReportResult;
