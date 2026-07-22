"use client";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Sparkles, GraduationCap, Target, UserRoundCog, ListChecks } from "lucide-react";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import { FORM_INPUT_CLASS } from "@/components/ui/fieldStyles";
import type { EmployeeCareerPathModel, CareerPathSuggestionModel, DevelopmentRecommendationModel } from "@/models";
import { saveEmployeeCareerPath } from "@/services/admin/employeeCareerPath/save";
import getEmployeeCareerPath from "@/services/admin/employeeCareerPath/get";
import { getCareerPathSuggestions } from "@/services/admin/careerPath/suggestions";
import { getRecommendations, createDevelopmentGoals } from "@/services/admin/employeeCareerPath/recommendations";
import { createDevelopmentPlanFromCareerPath } from "@/services/admin/employeeCareerPath/developmentPlan";
import getAllOrganizationalObjective from "@/services/admin/organizationalObjective/getAll";
import getAllCareerPath from "@/services/admin/careerPath/getAll";
import getAllCareerPathStep from "@/services/admin/careerPathStep/getAll";
import EmployeePicker from "@/components/common/employeePicker";
import { parameterInitialData } from "@/constants/initialization";
import { careerStepProgressStatusOptions, employeeCareerPathStatusOptions } from "@/constants/careerDevelopment";

const label = (opts: { id: string; name: string }[], v?: string) => opts.find((o) => o.id === v)?.name ?? (v ?? "");
type StepStatus = { status?: string; completedDate?: string };

function EmployeeCareerPathForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [form, setForm] = useState<EmployeeCareerPathModel>({ status: "Active" });
  const [statusMap, setStatusMap] = useState<Record<string, StepStatus>>({});
  const [saving, setSaving] = useState(false);
  const [state, setState] = useState<any>({});
  const [suggestions, setSuggestions] = useState<CareerPathSuggestionModel[] | null>(null);
  const [recos, setRecos] = useState<DevelopmentRecommendationModel | null>(null);
  const [devMsg, setDevMsg] = useState<string>("");
  const [objectiveId, setObjectiveId] = useState<string>("");
  const [busy, setBusy] = useState(false);
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["employeeCareerPath", id],
    queryFn: () => getEmployeeCareerPath(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: paths } = useQuery({ queryKey: ["careerPaths", "assignPicker"], queryFn: () => getAllCareerPath({ ...parameterInitialData, take: 300 }), staleTime: 60_000 });
  const { data: steps } = useQuery({
    queryKey: ["careerPathSteps", form.careerPathId, "assign"],
    queryFn: () => getAllCareerPathStep({ ...parameterInitialData, take: 200, parentId: form.careerPathId! }),
    enabled: !!form.careerPathId,
  });

  const { data: objectives } = useQuery({
    queryKey: ["organizationalObjectives", "goalAlign"],
    queryFn: () => getAllOrganizationalObjective({ ...parameterInitialData, take: 300 }),
    staleTime: 60_000, enabled: !!id,
  });

  const pathOptions = useMemo(() => (paths?.data ?? []).map((p) => ({ id: p.id!, name: `${p.name} (${p.code})` })), [paths]);
  const objectiveOptions = useMemo(() => (objectives?.data ?? []).map((o) => ({ id: o.id!, name: o.title ?? "" })), [objectives]);
  const stepOptions = useMemo(() => (steps?.data ?? []).map((s) => ({ id: s.id!, name: `#${s.stepOrder} ${s.name}` })), [steps]);
  const pName = (v?: string) => pathOptions.find((o) => o.id === v)?.name ?? "";

  useEffect(() => {
    if (record) {
      setForm(record);
      const map: Record<string, StepStatus> = {};
      (record.stepProgress ?? []).forEach((p) => { if (p.careerPathStepId) map[p.careerPathStepId] = { status: p.status, completedDate: p.completedDate }; });
      setStatusMap(map);
    }
  }, [record]);

  const set = useCallback((name: string, value: unknown) => setForm((p) => ({ ...p, [name]: value })), []);
  const setStep = (stepId: string, patch: StepStatus) => setStatusMap((m) => ({ ...m, [stepId]: { ...m[stepId], ...patch } }));

  const submit = async () => {
    setSaving(true);
    const stepList = steps?.data ?? [];
    const payload: EmployeeCareerPathModel = {
      ...form,
      status: form.status ?? "Active",
      currentStepId: form.currentStepId || undefined,
      stepProgress: stepList.map((s) => ({
        careerPathStepId: s.id,
        status: statusMap[s.id!]?.status ?? "NotStarted",
        completedDate: statusMap[s.id!]?.completedDate || undefined,
      })),
    };
    const result = await saveEmployeeCareerPath(payload);
    setState(result); setSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["employeeCareerPaths"] });
      if (result.id && !id) setId(result.id);
    }
  };

  const suggest = async () => {
    if (!form.employeeId) return;
    setBusy(true);
    setSuggestions(await getCareerPathSuggestions(form.employeeId));
    setBusy(false);
  };
  const showRecos = async () => { if (form.id) setRecos(await getRecommendations(form.id)); };
  const makeGoals = async () => {
    if (!form.id) return;
    setBusy(true);
    const r = await createDevelopmentGoals(form.id, undefined, objectiveId || undefined);
    setBusy(false);
    const aligned = r.organizationalObjectiveTitle ? ` — aligned to "${r.organizationalObjectiveTitle}"` : "";
    setDevMsg(`${r.created} development goal(s) created${r.skipped ? `, ${r.skipped} already existed` : ""}${aligned}.`);
  };
  const makePlan = async () => {
    if (!form.id) return;
    setBusy(true);
    const r = await createDevelopmentPlanFromCareerPath(form.id);
    setBusy(false);
    setDevMsg(`Individual Development Plan created with ${r.actionCount} action(s) — view it under Performance › Development Plans.`);
  };

  const completed = Object.values(statusMap).filter((s) => s.status === "Completed").length;
  const total = steps?.data?.length ?? 0;
  const livePercent = total > 0 ? Math.round((completed / total) * 100) : 0;

  return (
    <div className="space-y-4">
      {pending && <Loading />}
      <EntityFormTabs
        hasId={!!id}
        disabledHint="Save the assignment to view development recommendations."
        tabs={[
          {
            key: "assignment",
            label: "Assignment",
            Icon: UserRoundCog,
            keepMounted: true,
            content: (
              <div className="min-h-[15rem] space-y-4">
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  {/* Server-search picker — the employee table is never bulk-loaded (10k+ scale). */}
                  <div>
                    <label className="mb-1 block text-xs font-medium text-muted">Employee *</label>
                    <EmployeePicker
                      value={form.employeeId}
                      displayValue={form.employeeName}
                      onSelect={(eid, name) => { set("employeeId", eid); set("employeeName", name); }}
                    />
                  </div>
                  <FormUtility component={{ name: "careerPathId", label: "Career Path", required: true, type: "dropDown", layout: "auth", value: form.careerPathId, displayValue: form.careerPathName ?? pName(form.careerPathId), data: pathOptions as never, onSelect: (_n, r: any) => { set("careerPathId", r.id); set("careerPathName", r.name); set("currentStepId", undefined); } }} />
                  <FormUtility component={{ name: "currentStepId", label: "Current Step", type: "dropDown", layout: "auth", value: form.currentStepId, displayValue: label(stepOptions, form.currentStepId), data: stepOptions as never, onSelect: (_n, r: any) => set("currentStepId", r.id) }} />
                  <FormUtility component={{ name: "status", label: "Status", type: "dropDown", layout: "auth", value: form.status, displayValue: label(employeeCareerPathStatusOptions, form.status), data: employeeCareerPathStatusOptions as never, onSelect: (_n, r: any) => set("status", r.id) }} />
                  <FormUtility component={{ name: "assignedBy", label: "Assigned By", type: "text", layout: "auth", value: form.assignedBy, onChange: (e) => set("assignedBy", e.target.value) }} />
                  <div className="sm:col-span-2">
                    <FormUtility component={{ name: "notes", label: "Notes", type: "textarea", layout: "auth", value: form.notes, onChange: (e) => set("notes", e.target.value) }} />
                  </div>
                </div>

                {form.employeeId && (
                  <div className="flex justify-end">
                    <button type="button" disabled={busy} onClick={suggest} className="inline-flex items-center gap-1.5 rounded-lg border border-border px-3 py-1.5 text-sm font-medium text-foreground transition-colors hover:border-primary hover:text-primary disabled:opacity-50"><Sparkles size={13} /> Suggest paths</button>
                  </div>
                )}

                {/* HC163 — suggested paths for the chosen employee, ranked by competency match */}
                {suggestions && (
                  <div className="rounded-lg border border-border bg-secondary/20 p-3">
                    <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted">Suggested paths (fit = competency match + performance)</p>
                    {suggestions.length === 0 ? (
                      <p className="text-xs text-muted">No active career paths to suggest.</p>
                    ) : (
                      <div className="space-y-1.5">
                        {suggestions.slice(0, 6).map((s) => (
                          <button key={s.careerPathId} type="button" onClick={() => { set("careerPathId", s.careerPathId); set("careerPathName", s.careerPathName); }}
                            className="flex w-full items-center gap-2 rounded-lg border border-border bg-card px-3 py-2 text-left text-sm transition-colors hover:border-primary">
                            <span className="min-w-0 flex-1 truncate">
                              {s.careerPathName} <span className="text-xs text-muted">({s.code})</span>
                              <span className="block text-[10px] text-muted">match {Number(s.matchPercent).toFixed(0)}%{s.performanceScore != null ? ` · perf ${Number(s.performanceScore).toFixed(0)}%` : ""}</span>
                            </span>
                            {s.alreadyAssigned && <span className="rounded bg-muted/30 px-1.5 py-0.5 text-[10px] text-muted">assigned</span>}
                            <span className="shrink-0 text-right text-sm font-semibold tabular-nums text-primary" title="Overall fit">{Number(s.fitScore).toFixed(0)}%</span>
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            ),
          },
          {
            key: "progress",
            label: "Step Progress",
            Icon: ListChecks,
            keepMounted: true,
            content: (
              <div className="min-h-[15rem]">
                {!form.careerPathId ? (
                  <p className="rounded-lg border border-dashed border-border/70 bg-card/40 px-3 py-5 text-center text-xs text-muted">Select a career path on the Assignment tab to track step progress.</p>
                ) : (
                  <>
                    <div className="mb-3 flex items-center gap-2">
                      <p className="mr-auto text-xs text-muted">Milestone status per step (HC165).</p>
                      <span className="text-xs text-muted">{completed}/{total} completed</span>
                      <div className="h-1.5 w-24 overflow-hidden rounded-full bg-secondary"><div className="h-full rounded-full bg-primary" style={{ width: `${livePercent}%` }} /></div>
                      <span className="text-xs font-semibold tabular-nums text-foreground">{livePercent}%</span>
                    </div>
                    {total === 0 ? (
                      <p className="rounded-lg border border-dashed border-border/70 bg-card/40 px-3 py-5 text-center text-xs text-muted">This career path has no steps yet.</p>
                    ) : (
                      <div className="space-y-2">
                        {(steps?.data ?? []).map((s) => (
                          <div key={s.id} className="grid grid-cols-[1fr_180px_170px] items-center gap-2 rounded-lg border border-border/70 bg-background/50 p-2">
                            <span className="truncate text-sm text-foreground"><span className="font-semibold">#{s.stepOrder}</span> {s.name}</span>
                            <FormUtility component={{ name: `st-${s.id}`, label: "", type: "dropDown", value: statusMap[s.id!]?.status ?? "NotStarted", displayValue: label(careerStepProgressStatusOptions, statusMap[s.id!]?.status ?? "NotStarted"), data: careerStepProgressStatusOptions as never, onSelect: (_n, r: any) => setStep(s.id!, { status: r.id }) }} />
                            <input type="date" className={FORM_INPUT_CLASS} value={statusMap[s.id!]?.completedDate?.slice(0, 10) ?? ""} onChange={(e) => setStep(s.id!, { completedDate: e.target.value })} />
                          </div>
                        ))}
                        <p className="text-xs text-muted">Progress % is recomputed from these step statuses when you Save.</p>
                      </div>
                    )}
                  </>
                )}
              </div>
            ),
          },
          {
            key: "development",
            label: "Development",
            Icon: GraduationCap,
            needsId: true,
            content: (
              <div className="min-h-[15rem]">
                <div className="mb-3 flex flex-wrap items-center gap-2">
                  <p className="mr-auto text-xs text-muted">Competency gaps for the next step + development goals (HC164/HC167).</p>
                  <div className="w-56">
                    <FormUtility component={{ name: "alignObjective", label: "", placeholder: "Align to objective (auto)", type: "dropDown", value: objectiveId, displayValue: objectiveOptions.find((o) => o.id === objectiveId)?.name, data: objectiveOptions as never, onSelect: (_n, r: any) => setObjectiveId(r.id) }} />
                  </div>
                  <button type="button" onClick={showRecos} className="inline-flex items-center gap-1 rounded-lg border border-border px-2.5 py-1.5 text-xs font-medium text-foreground transition-colors hover:border-primary hover:text-primary"><Target size={12} /> Recommendations</button>
                  <button type="button" disabled={busy} onClick={makeGoals} className="inline-flex items-center gap-1 rounded-lg bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50"><GraduationCap size={12} /> Create development goals</button>
                  <button type="button" disabled={busy} onClick={makePlan} className="inline-flex items-center gap-1 rounded-lg border border-primary/40 bg-primary/10 px-2.5 py-1.5 text-xs font-semibold text-primary transition-colors hover:bg-primary/15 disabled:opacity-50"><ListChecks size={12} /> Create development plan</button>
                </div>
                {!recos && !devMsg && (
                  <p className="text-xs text-muted">Use the actions above to see the competency gap for the next step, or turn it into development goals.</p>
                )}
                {recos && (
                  <div className="text-xs">
                    <p className="mb-1.5 text-muted">Next step: <span className="font-semibold text-foreground">{recos.targetStepName ?? "—"}</span> — {recos.gapCount} competency gap(s) to develop.</p>
                    {recos.recommendations.length === 0 ? (
                      <p className="text-success">No gaps — the employee meets the next step's requirements.</p>
                    ) : (
                      <div className="flex flex-wrap gap-1.5">
                        {recos.recommendations.map((r) => (
                          <span key={r.competencyId} className="rounded-full border border-warning/40 bg-warning/10 px-2 py-0.5 text-warning">{r.name} · {r.suggestedAction}</span>
                        ))}
                      </div>
                    )}
                  </div>
                )}
                {devMsg && <p className="mt-2 text-xs text-success">{devMsg}</p>}
              </div>
            ),
          },
        ]}
      />

      {/* Persistent save bar — the assignment + step progress save as one payload from any tab. */}
      <div className="flex items-center justify-end gap-2 border-t border-border pt-3">
        <button type="button" disabled={saving || !form.employeeId || !form.careerPathId} onClick={submit} className="rounded-lg bg-primary px-5 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">{saving ? "Saving…" : "Save Assignment"}</button>
      </div>
      <StatusMessage formState={state} status={state?.status} message={state?.message} />
    </div>
  );
}

export default memo(EmployeeCareerPathForm);
