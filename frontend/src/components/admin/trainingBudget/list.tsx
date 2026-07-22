"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Gauge } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllTrainingBudgets, deleteTrainingBudget, getBudgetUtilization } from "@/services/admin/trainingBudget";
import type { TrainingBudgetModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const money = (v?: number) => Number(v ?? 0).toLocaleString();

/** HC190 — live utilization for one envelope (actual session costs + committed need estimates). */
function UtilizationCard({ budget, onClose }: { budget: TrainingBudgetModel; onClose: () => void }) {
  const { t } = useTranslation();
  const { data } = useQuery({
    queryKey: ["budgetUtilization", budget.fiscalYear, budget.organizationUnitId],
    queryFn: () => getBudgetUtilization(budget.fiscalYear!, budget.organizationUnitId),
  });
  const percent = data && data.budgetAmount > 0 ? Math.min(100, Math.round((data.utilized / data.budgetAmount) * 100)) : 0;
  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-2 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-foreground">
          {t("Utilization")} — {budget.fiscalYear} {budget.organizationUnitName ? `· ${budget.organizationUnitName}` : `· ${t("Org-wide")}`}
        </h3>
        <button type="button" onClick={onClose} className="text-xs text-muted hover:text-foreground">{t("Close")}</button>
      </div>
      {!data ? (
        <p className="text-xs text-muted">{t("Loading…")}</p>
      ) : (
        <>
          <div className="mb-2 h-2 overflow-hidden rounded-full bg-secondary/40">
            <div className={`h-full ${percent >= 90 ? "bg-error" : percent >= 70 ? "bg-warning" : "bg-success"}`} style={{ width: `${percent}%` }} />
          </div>
          <div className="grid grid-cols-2 gap-2 text-xs sm:grid-cols-4">
            <p><span className="block text-muted">{t("Budget")}</span><span className="font-semibold">{money(data.budgetAmount)}</span></p>
            <p><span className="block text-muted">{t("Session Costs")}</span><span className="font-semibold">{money(data.sessionCosts)}</span></p>
            <p><span className="block text-muted">{t("Committed Needs")}</span><span className="font-semibold">{money(data.committedNeedEstimates)}</span></p>
            <p><span className="block text-muted">{t("Remaining")}</span><span className={`font-semibold ${data.remaining < 0 ? "text-error" : "text-success"}`}>{money(data.remaining)}</span></p>
          </div>
        </>
      )}
    </div>
  );
}

function TrainingBudgetList({ editHandler }: Props) {
  const { t } = useTranslation();
  const [utilFor, setUtilFor] = useState<TrainingBudgetModel | null>(null);

  const list = useEntityList({
    queryKey: "trainingBudgets",
    fetchPage: getAllTrainingBudgets,
    deleteById: deleteTrainingBudget,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "fiscalYear",
          label: "Fiscal Year",
          sort: true,
          render: (v: number, record: TrainingBudgetModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {v}
            </button>
          ),
        },
        { name: "organizationUnitName", label: "Scope", render: (v: string) => v || "Org-wide" },
        { name: "amount", label: "Amount", sort: true, render: (v: number) => <span className="font-semibold">{money(v)}</span> },
        { name: "notes", label: "Notes", render: (v: string) => v || "—" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: TrainingBudgetModel) => (
            <span className="flex items-center gap-1.5">
              <button type="button" title={t("Utilization")} onClick={() => setUtilFor(record)} className="rounded p-1 text-muted hover:text-primary">
                <Gauge size={15} />
              </button>
              <GridAction
                id={record.id || ""}
                record={record}
                showAdd={false}
                showEdit
                showDelete
                editHandler={editHandler}
                deleteHandler={() => record.id && list.deleteRecord(record.id)}
              />
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="space-y-3">
      {utilFor && <UtilizationCard budget={utilFor} onClose={() => setUtilFor(null)} />}
      <EntityListShell listKey="trainingBudgets" listLabel="Training Budgets" columns={columns} {...list} />
    </div>
  );
}

export default TrainingBudgetList;
