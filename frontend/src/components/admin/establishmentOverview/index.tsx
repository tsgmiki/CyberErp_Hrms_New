"use client";

import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { LayoutGrid, Users, UserCheck, UserX, FileSpreadsheet } from "lucide-react";
import DropDownField from "@/components/ui/dropDownField";
import Loading from "@/components/common/loader/loader";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { getEstablishment } from "@/services/admin/workforcePlan";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import { parameterInitialData } from "@/constants/initialization";

/** Flat column set for the Excel export (HC074 — management/regulatory reporting). */
const EXPORT_COLUMNS: DataTableColumnModel[] = [
  { name: "organizationUnitName", label: "Unit" },
  { name: "positionClassTitle", label: "Role" },
  { name: "jobGradeName", label: "Grade" },
  { name: "jobCategoryName", label: "Job Family" },
  { name: "authorized", label: "Authorized" },
  { name: "filled", label: "Filled" },
  { name: "vacant", label: "Vacant" },
  { name: "avgVacantDays", label: "Avg Days Vacant" },
] as DataTableColumnModel[];

/**
 * Establishment Overview (HC056/HC073): authorized / filled / vacant seats per unit × role with a
 * vacancy-aging approximation — the live baseline every workforce plan anchors to.
 */
function EstablishmentOverview() {
  const { t } = useTranslation();
  const [unitId, setUnitId] = useState("");
  const [unitName, setUnitName] = useState("");
  const [unitParam, setUnitParam] = useState({ ...parameterInitialData, take: 10 });

  const { data: rows, isLoading } = useQuery({
    queryKey: ["establishmentOverview", unitId],
    queryFn: () => getEstablishment(unitId || undefined),
  });
  const { data: units, isLoading: unitsLoading } = useQuery({
    queryKey: ["organizationUnits", unitParam],
    queryFn: () => getAllOrganizationUnit(unitParam),
  });

  const totals = useMemo(() => {
    const r = rows ?? [];
    return {
      authorized: r.reduce((s, x) => s + x.authorized, 0),
      filled: r.reduce((s, x) => s + x.filled, 0),
      vacant: r.reduce((s, x) => s + x.vacant, 0),
    };
  }, [rows]);
  const fillRate = totals.authorized > 0 ? Math.round((totals.filled / totals.authorized) * 100) : 0;

  return (
    <div className="m-1 flex h-full min-h-0 flex-col gap-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h1 className="flex items-center gap-2 text-lg font-bold text-foreground">
          <LayoutGrid className="h-5 w-5 text-primary" />
          {t("Establishment Overview")}
          <span className="text-xs font-normal text-muted">
            — {t("authorized / filled / vacant seats by unit and role")}
          </span>
        </h1>
        <div className="flex items-center gap-2">
          <div className="w-72">
            <DropDownField
              name="unitFilter"
              type="dropDown"
              label=""
              placeholder={t("Filter by unit (incl. sub-units)…")}
              value={unitId}
              displayValue={unitName}
              isLoading={unitsLoading}
              param={unitParam}
              setParam={setUnitParam as never}
              data={(units?.data ?? []).map((u) => ({ id: u.id, name: u.name })) as never}
              onSelect={(_n, item: { id: string; name: string }) => {
                setUnitId(item.id);
                setUnitName(item.name);
              }}
            />
          </div>
          <button
            type="button"
            disabled={(rows ?? []).length === 0}
            onClick={async () => {
              // Export engine loads on demand (keeps xlsx out of the screen's initial bundle).
              const { exportListToExcel } = await import("@/components/common/dataTableProvider/listExport");
              exportListToExcel({
                title: unitName ? `Establishment - ${unitName}` : "Establishment Overview",
                data: (rows ?? []) as unknown as Record<string, unknown>[],
                columns: EXPORT_COLUMNS,
                labelFor: (key) => t(key),
              });
            }}
            title={t("Export for management and regulatory reporting (HC074)")}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-medium text-foreground hover:border-primary hover:text-primary disabled:opacity-50"
          >
            <FileSpreadsheet size={14} /> {t("Export")}
          </button>
        </div>
      </div>

      {/* Headline tiles */}
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        {[
          { label: t("Authorized Seats"), value: totals.authorized, Icon: Users, tone: "text-primary" },
          { label: t("Filled"), value: totals.filled, Icon: UserCheck, tone: "text-success" },
          { label: t("Vacant"), value: totals.vacant, Icon: UserX, tone: "text-warning" },
          { label: t("Fill Rate"), value: `${fillRate}%`, Icon: LayoutGrid, tone: "text-info" },
        ].map(({ label, value, Icon, tone }) => (
          <div key={label} className="rounded-lg border border-border bg-card px-4 py-3">
            <p className="flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-wide text-muted">
              <Icon size={13} className={tone} /> {label}
            </p>
            <p className="text-xl font-bold tabular-nums text-foreground">{value.toLocaleString?.() ?? value}</p>
          </div>
        ))}
      </div>

      {/* Establishment grid */}
      <div className="min-h-0 flex-1 overflow-auto rounded-lg border border-border bg-card">
        {isLoading && <Loading />}
        {!isLoading && (rows ?? []).length === 0 && (
          <p className="px-4 py-8 text-center text-sm text-muted">
            {t("No positions in this scope.")}
          </p>
        )}
        {!isLoading && (rows ?? []).length > 0 && (
          <table className="w-full text-[13px]">
            <thead className="sticky top-0 bg-card">
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                <th className="px-3 py-2 font-semibold">{t("Unit")}</th>
                <th className="px-3 py-2 font-semibold">{t("Role")}</th>
                <th className="px-3 py-2 font-semibold">{t("Grade")}</th>
                <th className="px-3 py-2 font-semibold">{t("Job Family")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("Auth")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("Filled")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("Vacant")}</th>
                <th className="px-3 py-2 font-semibold">{t("Occupancy")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("Avg Days Vacant")}</th>
              </tr>
            </thead>
            <tbody>
              {(rows ?? []).map((r, i) => {
                const pct = r.authorized > 0 ? Math.round((r.filled / r.authorized) * 100) : 0;
                return (
                  <tr key={i} className="border-b border-border/60">
                    <td className="px-3 py-2 font-medium text-foreground">{r.organizationUnitName}</td>
                    <td className="px-3 py-2 text-foreground">{r.positionClassTitle}</td>
                    <td className="px-3 py-2 text-muted">{r.jobGradeName ?? "—"}</td>
                    <td className="px-3 py-2 text-muted">{r.jobCategoryName ?? "—"}</td>
                    <td className="px-3 py-2 text-right tabular-nums">{r.authorized}</td>
                    <td className="px-3 py-2 text-right tabular-nums text-success">{r.filled}</td>
                    <td className={`px-3 py-2 text-right tabular-nums ${r.vacant > 0 ? "font-semibold text-warning" : "text-muted"}`}>
                      {r.vacant}
                    </td>
                    <td className="px-3 py-2">
                      <span className="flex items-center gap-1.5">
                        <span className="h-1.5 w-24 overflow-hidden rounded-full bg-muted/25">
                          <span className="block h-full bg-success" style={{ width: `${pct}%` }} />
                        </span>
                        <span className="text-[11px] tabular-nums text-muted">{pct}%</span>
                      </span>
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums text-muted">
                      {r.avgVacantDays ?? "—"}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default EstablishmentOverview;
