"use client";

import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Scale, BriefcaseBusiness } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import {
  getAllWorkforcePlans,
  deleteWorkforcePlan,
  compareWorkforcePlans,
  getApprovedDemand,
} from "@/services/admin/workforcePlan";
import type { WorkforcePlanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import { planHorizonLabel } from "@/constants/orgStructure";

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  Submitted: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
  Archived: "bg-info/15 text-info",
};

const fmtMoney = (v?: number) =>
  (v ?? 0).toLocaleString(undefined, { maximumFractionDigits: 0 });

/** Side-by-side scenario comparison of the selected plans (HC068). */
function CompareModal({ ids, onClose }: { ids: string[]; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["workforcePlanCompare", ids],
    queryFn: () => compareWorkforcePlans(ids),
  });

  const rows: { label: string; render: (c: NonNullable<typeof data>[number]) => React.ReactNode }[] = [
    { label: t("Scenario"), render: (c) => t(c.scenario) },
    { label: t("Status"), render: (c) => `${t(c.status)} (v${c.version})` },
    { label: t("End Headcount"), render: (c) => c.totalEndHeadcount.toLocaleString() },
    { label: t("Hiring Demand"), render: (c) => c.totalDemand.toLocaleString() },
    { label: t("Separations"), render: (c) => c.totalSeparations.toLocaleString() },
    { label: t("Headcount Gap"), render: (c) => c.totalGap.toLocaleString() },
    { label: t("Critical Roles"), render: (c) => c.criticalRoles.toLocaleString() },
    { label: t("Projected Cost"), render: (c) => fmtMoney(c.projectedCost) },
    { label: t("Budget"), render: (c) => fmtMoney(c.totalBudget) },
    {
      label: t("Budget Variance"),
      render: (c) => (
        <span className={c.budgetVariance < 0 ? "font-semibold text-error" : "text-success"}>
          {fmtMoney(c.budgetVariance)}
        </span>
      ),
    },
  ];

  return (
    <Modal
      visible
      size="xl"
      title={t("Scenario Comparison")}
      onClose={onClose}
      footer={
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
        >
          {t("Close")}
        </button>
      }
    >
      {isLoading && <Loading />}
      {!isLoading && data && (
        <div className="overflow-x-auto">
          <table className="w-full text-[13px]">
            <thead>
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                <th className="px-3 py-2 font-semibold">{t("Measure")}</th>
                {data.map((c) => (
                  <th key={c.planId} className="px-3 py-2 font-semibold">{c.name}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.label} className="border-b border-border/60">
                  <td className="px-3 py-2 font-medium text-muted">{r.label}</td>
                  {data.map((c) => (
                    <td key={c.planId} className="px-3 py-2 tabular-nums text-foreground">{r.render(c)}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Modal>
  );
}

/** Outstanding approved hiring demand — what recruitment will consume (HC075/HC081). */
function DemandModal({ onClose }: { onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["workforcePlanApprovedDemand"],
    queryFn: getApprovedDemand,
  });

  return (
    <Modal
      visible
      size="xl"
      title={t("Approved Hiring Demand")}
      description={t("Outstanding demand from approved workforce plans — feeds recruitment requisitions.")}
      onClose={onClose}
      footer={
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
        >
          {t("Close")}
        </button>
      }
    >
      {isLoading && <Loading />}
      {!isLoading && (data ?? []).length === 0 && (
        <p className="py-6 text-center text-sm text-muted">{t("No approved hiring demand.")}</p>
      )}
      {!isLoading && (data ?? []).length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full text-[13px]">
            <thead>
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                <th className="px-3 py-2 font-semibold">{t("Unit")}</th>
                <th className="px-3 py-2 font-semibold">{t("Role")}</th>
                <th className="px-3 py-2 font-semibold">{t("Type")}</th>
                <th className="px-3 py-2 font-semibold">{t("Year")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("New")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("Repl")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("Temp")}</th>
                <th className="px-3 py-2 font-semibold">{t("Plan")}</th>
              </tr>
            </thead>
            <tbody>
              {(data ?? []).map((r, i) => (
                <tr key={i} className="border-b border-border/60">
                  <td className="px-3 py-2 text-foreground">{r.organizationUnitName}</td>
                  <td className="px-3 py-2 text-foreground">
                    {r.positionClassTitle}
                    {r.isCriticalRole && (
                      <span className="ml-1.5 rounded bg-error/15 px-1.5 py-0.5 text-[10px] font-semibold text-error">
                        {t("CRITICAL")}
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-2 text-muted">{t(r.employmentType)}</td>
                  <td className="px-3 py-2 text-muted">{r.periodIndex + 1}</td>
                  <td className="px-3 py-2 text-right tabular-nums">{r.newHires}</td>
                  <td className="px-3 py-2 text-right tabular-nums">{r.replacements}</td>
                  <td className="px-3 py-2 text-right tabular-nums">{r.temporaryStaff}</td>
                  <td className="px-3 py-2 text-xs text-muted">{r.planName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Modal>
  );
}

interface Props {
  editHandler: (id: string) => void;
}

function WorkforcePlanList({ editHandler }: Props) {
  const { t } = useTranslation();
  const [selected, setSelected] = useState<string[]>([]);
  const [compareIds, setCompareIds] = useState<string[] | null>(null);
  const [showDemand, setShowDemand] = useState(false);

  const list = useEntityList({
    queryKey: "workforcePlans",
    fetchPage: getAllWorkforcePlans,
    deleteById: deleteWorkforcePlan,
  });

  const toggleSelect = (id: string) =>
    setSelected((p) => (p.includes(id) ? p.filter((x) => x !== id) : [...p, id]));

  const columns = useMemo(
    () =>
      [
        {
          name: "compare",
          label: "",
          render: (_t: unknown, record: WorkforcePlanModel) => (
            <input
              type="checkbox"
              checked={!!record.id && selected.includes(record.id)}
              onChange={() => record.id && toggleSelect(record.id)}
              title={t("Select for comparison")}
            />
          ),
        },
        {
          name: "name",
          label: "Plan",
          sort: true,
          render: (text: string, record: WorkforcePlanModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left"
            >
              <span className="block font-semibold">
                {text} <span className="font-normal text-muted">v{record.version}</span>
              </span>
              <span className="block text-xs text-muted">
                {t(record.scenario ?? "")} · {planHorizonLabel(record.horizon)}
                {record.organizationUnitName ? ` · ${record.organizationUnitName}` : ` · ${t("Organization-wide")}`}
              </span>
            </button>
          ),
        },
        {
          name: "startFiscalYearName",
          label: "From FY",
          render: (text: string, r: WorkforcePlanModel) =>
            `${text ?? ""}${(r.periodCount ?? 1) > 1 ? ` (+${(r.periodCount ?? 1) - 1})` : ""}`,
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[text] ?? ""}`}>
              {t(text)}
            </span>
          ),
        },
        {
          name: "projectedCost",
          label: "Projected Cost",
          render: (v: number) => <span className="tabular-nums">{fmtMoney(v)}</span>,
        },
        {
          name: "budgetVariance",
          label: "Budget Variance",
          render: (v: number) => (
            <span className={`tabular-nums ${v < 0 ? "font-semibold text-error" : "text-success"}`}>
              {fmtMoney(v)}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: WorkforcePlanModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete={record.status === "Draft" || record.status === "Rejected"}
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, selected, t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="mb-2 flex items-center justify-end gap-2 px-1">
        <button
          type="button"
          onClick={() => setShowDemand(true)}
          className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-medium text-foreground hover:border-primary hover:text-primary"
        >
          <BriefcaseBusiness size={14} /> {t("Hiring Demand")}
        </button>
        <button
          type="button"
          disabled={selected.length < 2 || selected.length > 5}
          onClick={() => setCompareIds(selected)}
          title={t("Select 2–5 plans to compare")}
          className="inline-flex items-center gap-1.5 rounded-md border border-primary/40 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary hover:bg-primary/20 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <Scale size={14} /> {t("Compare")} {selected.length > 0 ? `(${selected.length})` : ""}
        </button>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="workforcePlans"
          listLabel="Workforce Plans"
          columns={columns}
          {...list}
        />
      </div>

      {compareIds && <CompareModal ids={compareIds} onClose={() => setCompareIds(null)} />}
      {showDemand && <DemandModal onClose={() => setShowDemand(false)} />}
    </div>
  );
}

export default WorkforcePlanList;
