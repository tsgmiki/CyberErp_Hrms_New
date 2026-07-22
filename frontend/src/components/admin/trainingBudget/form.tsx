"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, Wallet } from "lucide-react";
import type { TrainingBudgetModel } from "@/models";
import { getAllTrainingBudgets, saveTrainingBudget } from "@/services/admin/trainingBudget";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import { StatusMessage } from "../../common/statusMessage/status";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

function TrainingBudgetForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<TrainingBudgetModel>({ fiscalYear: new Date().getFullYear() });
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  // The paged list already carries every envelope — resolve the edited row from it.
  const [listParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: budgets } = useQuery({ queryKey: ["trainingBudgets", listParam], queryFn: () => getAllTrainingBudgets(listParam) });

  const [unitParam] = useState({ ...parameterInitialData, take: 300 });
  const { data: units } = useQuery({ queryKey: ["organizationUnits", unitParam], queryFn: () => getAllOrganizationUnit(unitParam) });

  useEffect(() => {
    if (id) {
      const record = (budgets?.data ?? []).find((b) => b.id === id);
      if (record) setMeta(record);
    } else {
      setMeta({ fiscalYear: new Date().getFullYear() });
    }
  }, [id, budgets]);

  const set = (name: keyof TrainingBudgetModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));

  const submit = async () => {
    setIsSaving(true);
    const result = await saveTrainingBudget({
      id: meta.id,
      fiscalYear: Number(meta.fiscalYear),
      organizationUnitId: meta.organizationUnitId || undefined,
      amount: Number(meta.amount ?? 0),
      notes: meta.notes || undefined,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["trainingBudgets"] });
      setId("");
    }
  };

  const canSave = !!meta.fiscalYear && meta.amount != null && String(meta.amount) !== "";

  return (
    <div className="space-y-4 text-foreground">
      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 flex items-center gap-2 text-sm font-semibold">
          <Wallet size={16} className="text-primary" /> {t("Budget Envelope")}
        </h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Fiscal Year")} *</label>
            <input type="number" min={2000} max={2100} className={INPUT} value={meta.fiscalYear ?? ""} onChange={(e) => set("fiscalYear", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Amount")} *</label>
            <input type="number" min={0} className={INPUT} value={meta.amount ?? ""} onChange={(e) => set("amount", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Scope")}</label>
            <select className={INPUT} value={meta.organizationUnitId ?? ""} onChange={(e) => set("organizationUnitId", e.target.value || undefined)}>
              <option value="">{t("Org-wide")}</option>
              {(units?.data ?? []).map((u: any) => (
                <option key={u.id} value={u.id}>{u.name}</option>
              ))}
            </select>
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Notes")}</label>
            <textarea className={INPUT} rows={2} value={meta.notes ?? ""} onChange={(e) => set("notes", e.target.value)} />
          </div>
        </div>
        <div className="mt-4 flex justify-end">
          <button
            type="button"
            disabled={!canSave || isSaving}
            onClick={submit}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Save size={14} /> {isSaving ? t("Saving…") : t("Save Budget")}
          </button>
        </div>
      </div>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(TrainingBudgetForm);
