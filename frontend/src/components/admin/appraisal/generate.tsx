"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Sparkles } from "lucide-react";
import { generateAppraisal } from "@/services/admin/appraisal";
import EmployeePicker from "@/components/common/employeePicker";
import getAllReviewCycle from "@/services/admin/reviewCycle/getAll";
import getAllAppraisalTemplate from "@/services/admin/appraisalTemplate/getAll";
import { StatusMessage } from "../../common/statusMessage/status";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

/** Generate a new appraisal, then hand its id up so the workspace opens in scoring mode. */
function AppraisalGenerate({ onGenerated }: { onGenerated: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [employeeId, setEmployeeId] = useState("");
  const [employeeName, setEmployeeName] = useState("");
  const [reviewCycleId, setReviewCycleId] = useState("");
  const [appraisalTemplateId, setAppraisalTemplateId] = useState("");
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const [cycleParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: cycles } = useQuery({ queryKey: ["reviewCycles", cycleParam], queryFn: () => getAllReviewCycle(cycleParam) });
  const [tplParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: templates } = useQuery({ queryKey: ["appraisalTemplates", tplParam], queryFn: () => getAllAppraisalTemplate(tplParam) });

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const result = await generateAppraisal({
      employeeId,
      reviewCycleId,
      appraisalTemplateId: appraisalTemplateId || undefined,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success" && result.id) {
      queryClient.invalidateQueries({ queryKey: ["appraisals"] });
      onGenerated(result.id);
    }
  };

  return (
    <form onSubmit={submit} className="space-y-5 p-1 text-foreground">
      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("Generate Appraisal")}</h3>
        <p className="mb-3 text-xs text-muted">
          {t("Pulls the employee's goals and position competencies into a scorable appraisal.")}
        </p>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Employee")} *</label>
            {/* Role-scoped searchable picker: HR = all, manager = unit subtree, employee = locked to self. */}
            <EmployeePicker value={employeeId} displayValue={employeeName} onSelect={(id, name) => { setEmployeeId(id); setEmployeeName(name); }} />
          </div>
          <div>
            <label className={LABEL}>{t("Review Cycle")} *</label>
            <select className={INPUT} value={reviewCycleId} onChange={(e) => setReviewCycleId(e.target.value)} required>
              <option value="">{t("Select cycle")}</option>
              {(cycles?.data ?? []).map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Appraisal Template")}</label>
            <select className={INPUT} value={appraisalTemplateId} onChange={(e) => setAppraisalTemplateId(e.target.value)}>
              <option value="">{t("Default 50 / 50")}</option>
              {(templates?.data ?? []).map((tp) => (
                <option key={tp.id} value={tp.id}>{tp.name} ({tp.goalsWeight}/{tp.competenciesWeight})</option>
              ))}
            </select>
          </div>
        </div>
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <button type="submit" disabled={isSaving || !employeeId} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
          <Sparkles className="h-4 w-4" /> {isSaving ? t("Generating…") : t("Generate Appraisal")}
        </button>
      </div>
    </form>
  );
}

export default memo(AppraisalGenerate);
