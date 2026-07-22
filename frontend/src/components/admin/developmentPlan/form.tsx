"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save, GraduationCap, ListChecks } from "lucide-react";
import type { DevelopmentPlanModel, DevelopmentActionModel } from "@/models";
import { getDevelopmentPlan, saveDevelopmentPlan } from "@/services/admin/developmentPlan";
import EmployeePicker from "@/components/common/employeePicker";
import getAllCompetency from "@/services/admin/competency/getAll";
import { developmentPlanStatusOptions, developmentActionStatusOptions, learningInterventionOptions } from "@/constants/performance";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface EditableAction extends DevelopmentActionModel {
  _key: number;
}

const numOr0 = (v: unknown): number => {
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
};
const d10 = (v?: string) => (v ? v.slice(0, 10) : "");
const NEW_DEFAULTS: DevelopmentPlanModel = { status: "Active" };

function DevelopmentPlanForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [meta, setMeta] = useState<DevelopmentPlanModel>({ ...NEW_DEFAULTS });
  const [actions, setActions] = useState<EditableAction[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["developmentPlan", id],
    queryFn: () => getDevelopmentPlan(id),
    enabled: id !== "",
  });

  const [compParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: competencies } = useQuery({ queryKey: ["competencies", compParam], queryFn: () => getAllCompetency(compParam) });

  useEffect(() => {
    if (record) {
      setMeta({ ...record, startDate: d10(record.startDate), endDate: d10(record.endDate) });
      setActions((record.actions ?? []).map((a) => ({ ...a, targetDate: d10(a.targetDate), _key: nextKey() })));
    }
  }, [record]);

  const setMetaField = (name: keyof DevelopmentPlanModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));
  const addAction = () => setActions((p) => [...p, { _key: nextKey(), description: "", status: "Planned", progressPercent: 0, sortOrder: p.length }]);
  const updateAction = (key: number, patch: Partial<EditableAction>) => setActions((p) => p.map((a) => (a._key === key ? { ...a, ...patch } : a)));
  const removeAction = (key: number) => setActions((p) => p.filter((a) => a._key !== key));

  const submit = async () => {
    setIsSaving(true);
    const payload: DevelopmentPlanModel = {
      ...meta,
      appraisalId: meta.appraisalId || undefined,
      actions: actions.map(({ _key, ...a }, i) => ({
        ...a,
        competencyId: a.competencyId || undefined,
        targetDate: a.targetDate || undefined,
        progressPercent: numOr0(a.progressPercent),
        sortOrder: i,
      })),
    };
    const result = await saveDevelopmentPlan(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["developmentPlans"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  const canSave = !!meta.employeeId && !!meta.title && !!meta.startDate && !!meta.endDate;

  return (
    <div className="space-y-4 text-foreground">
      <EntityFormTabs
        hasId={!!id}
        tabs={[
          {
            key: "details",
            label: "Plan Details",
            Icon: GraduationCap,
            description: "Who the plan is for and the period it covers",
            keepMounted: true,
            content: (
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div>
                  <label className={LABEL}>{t("Employee")} *</label>
                  {/* Server-search picker — no bulk employee load (10k+ scale). */}
                  <EmployeePicker
                    value={meta.employeeId}
                    displayValue={meta.employeeName}
                    onSelect={(eid, name) => setMeta((p) => ({ ...p, employeeId: eid, employeeName: name }))}
                  />
                </div>
                <div>
                  <label className={LABEL}>{t("Status")}</label>
                  <select className={INPUT} value={meta.status ?? "Active"} onChange={(e) => setMetaField("status", e.target.value)}>
                    {developmentPlanStatusOptions.map((o) => (
                      <option key={o.id} value={o.id}>{t(o.name)}</option>
                    ))}
                  </select>
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Title")} *</label>
                  <input className={INPUT} value={meta.title ?? ""} onChange={(e) => setMetaField("title", e.target.value)} />
                </div>
                <div>
                  <label className={LABEL}>{t("Start Date")} *</label>
                  <input type="date" className={INPUT} value={meta.startDate ?? ""} onChange={(e) => setMetaField("startDate", e.target.value)} />
                </div>
                <div>
                  <label className={LABEL}>{t("End Date")} *</label>
                  <input type="date" className={INPUT} value={meta.endDate ?? ""} onChange={(e) => setMetaField("endDate", e.target.value)} />
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Description")}</label>
                  <input className={INPUT} value={meta.description ?? ""} onChange={(e) => setMetaField("description", e.target.value)} />
                </div>
              </div>
            ),
          },
          {
            key: "actions",
            label: "Development Actions",
            Icon: ListChecks,
            description: "Learning interventions that close competency gaps",
            keepMounted: true,
            content: (
              <div className="space-y-3">
                <div className="flex justify-end">
                  <button type="button" onClick={addAction} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
                    <Plus className="h-3.5 w-3.5" /> {t("Add Action")}
                  </button>
                </div>
                {actions.length === 0 ? (
                  <p className="py-6 text-center text-sm text-muted">{t("No actions yet.")}</p>
                ) : (
                  <div className="space-y-2">
                    {actions.map((a) => (
                      <div key={a._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_140px_130px_130px_90px_auto]">
                        <div>
                          <label className={LABEL}>{t("Description")} *</label>
                          <input className={INPUT} value={a.description ?? ""} onChange={(e) => updateAction(a._key, { description: e.target.value })} />
                        </div>
                        <div>
                          <label className={LABEL}>{t("Competency Gap")}</label>
                          <select className={INPUT} value={a.competencyId ?? ""} onChange={(e) => updateAction(a._key, { competencyId: e.target.value })}>
                            <option value="">{t("None")}</option>
                            {(competencies?.data ?? []).map((c) => (
                              <option key={c.id} value={c.id}>{c.name}</option>
                            ))}
                          </select>
                        </div>
                        <div>
                          <label className={LABEL}>{t("Intervention")}</label>
                          <select className={INPUT} value={a.learningIntervention ?? ""} onChange={(e) => updateAction(a._key, { learningIntervention: e.target.value })}>
                            <option value="">—</option>
                            {learningInterventionOptions.map((o) => (
                              <option key={o.id} value={o.id}>{t(o.name)}</option>
                            ))}
                          </select>
                        </div>
                        <div>
                          <label className={LABEL}>{t("Status")}</label>
                          <select className={INPUT} value={a.status ?? "Planned"} onChange={(e) => updateAction(a._key, { status: e.target.value })}>
                            {developmentActionStatusOptions.map((o) => (
                              <option key={o.id} value={o.id}>{t(o.name)}</option>
                            ))}
                          </select>
                        </div>
                        <div>
                          <label className={LABEL}>{t("Progress")}</label>
                          <input type="number" className={INPUT} value={a.progressPercent ?? 0} onChange={(e) => updateAction(a._key, { progressPercent: e.target.value as never })} />
                        </div>
                        <div className="flex items-center pb-1">
                          <button type="button" onClick={() => removeAction(a._key)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                            <Trash2 className="h-4 w-4" />
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ),
          },
        ]}
      />

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Persistent save bar — details + actions save as one payload from any tab. */}
      <div className="flex items-center justify-end gap-2 border-t border-border pt-3">
        <button type="button" disabled={isSaving || !canSave} onClick={submit} className="inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">
          <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Save Plan")}
        </button>
      </div>
    </div>
  );
}

export default memo(DevelopmentPlanForm);
