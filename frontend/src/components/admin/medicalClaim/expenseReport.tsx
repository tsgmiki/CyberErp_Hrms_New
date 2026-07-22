"use client";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import DateField from "@/components/ui/dateField";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { getMedicalExpenseReport } from "@/services/admin/medical";
import { money } from "./shared";

/** HC246 — medical expense report grouped by beneficiary category, over an optional date range. */
function MedicalExpenseReport() {
  const { t } = useTranslation();
  const [range, setRange] = useState({ fromDate: "", toDate: "" });
  const { data: report } = useQuery({
    queryKey: ["medicalExpenseReport", range],
    queryFn: () => getMedicalExpenseReport(range.fromDate || undefined, range.toDate || undefined),
  });

  const columns = useMemo(
    () =>
      [
        { name: "category", label: "Beneficiary category", render: (text: string) => t(text ?? "") },
        { name: "claimCount", label: "Claims", render: (_t: unknown, r: any) => <span className="tabular-nums">{r.claimCount}</span> },
        { name: "totalClaimed", label: "Total claimed", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.totalClaimed)}</span> },
        { name: "totalApproved", label: "Total approved", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.totalApproved)}</span> },
      ] as DataTableColumnModel[],
    [t],
  );

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-3">
      <div className="flex flex-wrap items-end gap-3">
        <div className="w-44"><DateField type="date" label="From" name="fromDate" value={range.fromDate} onChange={(e: any) => setRange((p) => ({ ...p, fromDate: e.target.value }))} /></div>
        <div className="w-44"><DateField type="date" label="To" name="toDate" value={range.toDate} onChange={(e: any) => setRange((p) => ({ ...p, toDate: e.target.value }))} /></div>
        {(report?.rows?.length ?? 0) > 0 && (
          <div className="ml-auto flex flex-wrap gap-6 rounded-lg border border-border bg-card px-4 py-2 text-sm">
            <span className="text-muted">{t("Total claims")}: <b className="tabular-nums text-foreground">{report?.totalClaims}</b></span>
            <span className="text-muted">{t("Total claimed")}: <b className="tabular-nums text-foreground">{money(report?.grandTotalClaimed)}</b></span>
            <span className="text-muted">{t("Total approved")}: <b className="tabular-nums text-primary">{money(report?.grandTotalApproved)}</b></span>
          </div>
        )}
      </div>
      <DataTableProvider dataTable={{ columns, data: report?.rows ?? [], count: report?.rows?.length ?? 0, pagination: "None", search: "None", key: "category" }} />
    </div>
  );
}

export default MedicalExpenseReport;
