"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save, CheckCircle2, Flag, ClipboardList, ListChecks } from "lucide-react";
import type { ImprovementPlanModel, PipObjectiveModel } from "@/models";
import { getImprovementPlan, saveImprovementPlan, recordImprovementPlanOutcome } from "@/services/admin/improvementPlan";
import EmployeePicker from "@/components/common/employeePicker";
import { pipStatusOptions, pipObjectiveStatusOptions, pipOutcomeOptions } from "@/constants/performance";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none disabled:opacity-60";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface EditableObjective extends PipObjectiveModel {
  _key: number;
}

const numOr0 = (v: unknown): number => {
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
};
const d10 = (v?: string) => (v ? v.slice(0, 10) : "");
const NEW_DEFAULTS: ImprovementPlanModel = { status: "Active" };

function ImprovementPlanForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [meta, setMeta] = useState<ImprovementPlanModel>({ ...NEW_DEFAULTS });
  const [objectives, setObjectives] = useState<EditableObjective[]>([]);
  const [outcome, setOutcome] = useState("Successful");
  const [outcomeNotes, setOutcomeNotes] = useState("");
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["improvementPlan", id],
    queryFn: () => getImprovementPlan(id),
    enabled: id !== "",
  });

  useEffect(() => {
    if (record) {
      setMeta({ ...record, startDate: d10(record.startDate), endDate: d10(record.endDate) });
      setObjectives((record.objectives ?? []).map((o) => ({ ...o, targetDate: d10(o.targetDate), _key: nextKey() })));
    }
  }, [record]);

  const completed = meta.status === "Completed";

  const setMetaField = (name: keyof ImprovementPlanModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));
  const addObjective = () => setObjectives((p) => [...p, { _key: nextKey(), description: "", status: "NotStarted", progressPercent: 0, sortOrder: p.length }]);
  const updateObjective = (key: number, patch: Partial<EditableObjective>) => setObjectives((p) => p.map((o) => (o._key === key ? { ...o, ...patch } : o)));
  const removeObjective = (key: number) => setObjectives((p) => p.filter((o) => o._key !== key));

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["improvementPlans"] });
    queryClient.invalidateQueries({ queryKey: ["improvementPlan", id] });
  };

  const submit = async () => {
    setIsSaving(true);
    const payload: ImprovementPlanModel = {
      ...meta,
      appraisalId: meta.appraisalId || undefined,
      objectives: objectives.map(({ _key, ...o }, i) => ({
        ...o,
        targetDate: o.targetDate || undefined,
        progressPercent: numOr0(o.progressPercent),
        sortOrder: i,
      })),
    };
    const result = await saveImprovementPlan(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["improvementPlans"] });
      setId("");
    }
  };

  const record_ = async () => {
    setIsSaving(true);
    const result = await recordImprovementPlanOutcome({ id, outcome, notes: outcomeNotes || undefined });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") refresh();
  };

  if (isLoading) return <Loading />;

  const canSave = !!meta.employeeId && !!meta.title && !!meta.reason && !!meta.startDate && !!meta.endDate;

  return (
    <div className="space-y-4 text-foreground">
      <EntityFormTabs
        hasId={!!id}
        disabledHint="Save the plan to record its outcome."
        tabs={[
          {
            key: "details",
            label: "Plan Details",
            Icon: ClipboardList,
            description: "The employee, the reason, and the improvement period",
            keepMounted: true,
            content: (
              <div className="space-y-3">
                {completed && (
                  <div className="flex justify-end">
                    <span className="inline-flex items-center gap-1 rounded-full bg-secondary/60 px-3 py-1 text-xs font-semibold text-muted">
                      <CheckCircle2 className="h-3 w-3" /> {t("Outcome")}: {meta.outcome}
                    </span>
                  </div>
                )}
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                  <div>
                    <label className={LABEL}>{t("Employee")} *</label>
                    {/* Server-search picker — no bulk employee load (10k+ scale). */}
                    <EmployeePicker
                      value={meta.employeeId}
                      displayValue={meta.employeeName}
                      disabled={completed}
                      onSelect={(eid, name) => setMeta((p) => ({ ...p, employeeId: eid, employeeName: name }))}
                    />
                  </div>
                  <div>
                    <label className={LABEL}>{t("Status")}</label>
                    <select className={INPUT} value={meta.status ?? "Active"} disabled={completed} onChange={(e) => setMetaField("status", e.target.value)}>
                      {pipStatusOptions.map((o) => (
                        <option key={o.id} value={o.id}>{t(o.name)}</option>
                      ))}
                    </select>
                  </div>
                  <div className="sm:col-span-2">
                    <label className={LABEL}>{t("Title")} *</label>
                    <input className={INPUT} value={meta.title ?? ""} disabled={completed} onChange={(e) => setMetaField("title", e.target.value)} />
                  </div>
                  <div className="sm:col-span-2">
                    <label className={LABEL}>{t("Reason")} *</label>
                    <input className={INPUT} value={meta.reason ?? ""} disabled={completed} onChange={(e) => setMetaField("reason", e.target.value)} />
                  </div>
                  <div>
                    <label className={LABEL}>{t("Start Date")} *</label>
                    <input type="date" className={INPUT} value={meta.startDate ?? ""} disabled={completed} onChange={(e) => setMetaField("startDate", e.target.value)} />
                  </div>
                  <div>
                    <label className={LABEL}>{t("End Date")} *</label>
                    <input type="date" className={INPUT} value={meta.endDate ?? ""} disabled={completed} onChange={(e) => setMetaField("endDate", e.target.value)} />
                  </div>
                </div>
              </div>
            ),
          },
          {
            key: "objectives",
            label: "Objectives",
            Icon: ListChecks,
            description: "Concrete objectives the employee must meet",
            keepMounted: true,
            content: (
              <div className="space-y-3">
                {!completed && (
                  <div className="flex justify-end">
                    <button type="button" onClick={addObjective} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
                      <Plus className="h-3.5 w-3.5" /> {t("Add Objective")}
                    </button>
                  </div>
                )}
                {objectives.length === 0 ? (
                  <p className="py-6 text-center text-sm text-muted">{t("No objectives yet.")}</p>
                ) : (
                  <div className="space-y-2">
                    {objectives.map((o) => (
                      <div key={o._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_140px_130px_90px_auto]">
                        <div>
                          <label className={LABEL}>{t("Description")} *</label>
                          <input className={INPUT} value={o.description ?? ""} disabled={completed} onChange={(e) => updateObjective(o._key, { description: e.target.value })} />
                        </div>
                        <div>
                          <label className={LABEL}>{t("Target Date")}</label>
                          <input type="date" className={INPUT} value={o.targetDate ?? ""} disabled={completed} onChange={(e) => updateObjective(o._key, { targetDate: e.target.value })} />
                        </div>
                        <div>
                          <label className={LABEL}>{t("Status")}</label>
                          <select className={INPUT} value={o.status ?? "NotStarted"} disabled={completed} onChange={(e) => updateObjective(o._key, { status: e.target.value })}>
                            {pipObjectiveStatusOptions.map((s) => (
                              <option key={s.id} value={s.id}>{t(s.name)}</option>
                            ))}
                          </select>
                        </div>
                        <div>
                          <label className={LABEL}>{t("Progress")}</label>
                          <input type="number" className={INPUT} value={o.progressPercent ?? 0} disabled={completed} onChange={(e) => updateObjective(o._key, { progressPercent: e.target.value as never })} />
                        </div>
                        {!completed && (
                          <div className="flex items-center pb-1">
                            <button type="button" onClick={() => removeObjective(o._key)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                              <Trash2 className="h-4 w-4" />
                            </button>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ),
          },
          {
            key: "outcome",
            label: "Outcome",
            Icon: Flag,
            description: "Close the plan by recording its result",
            needsId: true,
            content: completed ? (
              <div className="flex items-center gap-2 rounded-md border border-primary/30 bg-primary/10 px-3 py-2 text-sm">
                <CheckCircle2 className="h-4 w-4 text-primary" />
                <span className="font-semibold">{t("Outcome")}: {meta.outcome}</span>
                <span className="text-muted">· {t("This plan is completed and locked.")}</span>
              </div>
            ) : (
              <div className="flex flex-wrap items-end gap-2">
                <div>
                  <label className={LABEL}>{t("Outcome")}</label>
                  <select className={INPUT} value={outcome} onChange={(e) => setOutcome(e.target.value)}>
                    {pipOutcomeOptions.map((o) => (
                      <option key={o.id} value={o.id}>{t(o.name)}</option>
                    ))}
                  </select>
                </div>
                <div className="min-w-[200px] flex-1">
                  <label className={LABEL}>{t("Notes")}</label>
                  <input className={INPUT} value={outcomeNotes} onChange={(e) => setOutcomeNotes(e.target.value)} />
                </div>
                <button type="button" disabled={isSaving} onClick={record_} className="inline-flex items-center gap-2 rounded-md border border-border px-4 py-2 text-sm font-semibold hover:bg-secondary/40 disabled:opacity-50">
                  <Flag className="h-4 w-4" /> {t("Record Outcome")}
                </button>
              </div>
            ),
          },
        ]}
      />

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Persistent save bar — details + objectives save as one payload from any tab. */}
      {!completed && (
        <div className="flex items-center justify-end gap-2 border-t border-border pt-3">
          <button type="button" disabled={isSaving || !canSave} onClick={submit} className="inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">
            <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Save Plan")}
          </button>
        </div>
      )}
    </div>
  );
}

export default memo(ImprovementPlanForm);
