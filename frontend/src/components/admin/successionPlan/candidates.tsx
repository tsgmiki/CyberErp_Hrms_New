"use client";
import { memo, useCallback, useMemo, useState } from "react";
import { Target, Gauge, UserSearch, UserRound, GraduationCap, BookOpen, ListChecks } from "lucide-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Modal from "@/components/common/modal";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import ChildManager, { type ChildColumn } from "@/components/admin/employee/childManager";
import { StatusMessage } from "@/components/common/statusMessage/status";
import { RepeatHeader, RepeatRow, RowRemoveButton, SectionAddButton, EmptyHint } from "@/components/common/formSection";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import { FORM_INPUT_CLASS } from "@/components/ui/fieldStyles";
import type { SuccessionCandidateModel, SuccessionDevelopmentActionModel, KnowledgeTransferModel, CompetencyGapModel, SuccessionCandidateProfileModel } from "@/models";
import getAllSuccessionCandidate from "@/services/admin/successionCandidate/getAll";
import getSuccessionCandidate from "@/services/admin/successionCandidate/get";
import { saveSuccessionCandidate } from "@/services/admin/successionCandidate/save";
import deleteSuccessionCandidate from "@/services/admin/successionCandidate/delete";
import { getCompetencyGap } from "@/services/admin/successionCandidate/gap";
import { createDevelopmentPlanFromSuccession } from "@/services/admin/successionCandidate/developmentPlan";
import { computeReadiness } from "@/services/admin/successionCandidate/readiness";
import { getCandidateProfile } from "@/services/admin/successionCandidate/profile";
import getAllEmployee from "@/services/admin/employee/getAll";
import { parameterInitialData } from "@/constants/initialization";
import {
  readinessLevelOptions, successionActionTypeOptions, successionActionStatusOptions, knowledgeTransferStatusOptions,
} from "@/constants/careerDevelopment";

const empName = (e: any) => `${e.fullName ?? `${e.firstName ?? ""} ${e.grandFatherName ?? ""}`.trim()}${e.employeeNumber ? ` (${e.employeeNumber})` : ""}`;
const label = (opts: { id: string; name: string }[], v?: string) => opts.find((o) => o.id === v)?.name ?? (v ?? "");

function Candidates({ planId }: { planId: string }) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<SuccessionCandidateModel>({});
  const [actions, setActions] = useState<SuccessionDevelopmentActionModel[]>([]);
  const [transfers, setTransfers] = useState<KnowledgeTransferModel[]>([]);
  const [gap, setGap] = useState<CompetencyGapModel | null>(null);
  const [profile, setProfile] = useState<SuccessionCandidateProfileModel | null>(null);
  const [computing, setComputing] = useState(false);
  const [planMsg, setPlanMsg] = useState<string>("");
  const [saving, setSaving] = useState(false);
  const [state, setState] = useState<any>({});

  const { data: list, isLoading } = useQuery({
    queryKey: ["successionCandidates", planId],
    queryFn: () => getAllSuccessionCandidate({ ...parameterInitialData, take: 200, parentId: planId }),
    enabled: !!planId,
  });
  const { data: employees } = useQuery({
    queryKey: ["employees", "candidatePicker"],
    queryFn: () => getAllEmployee({ ...parameterInitialData, take: 500 }),
    staleTime: 60_000, enabled: open,
  });
  const employeeOptions = useMemo(() => (employees?.data ?? []).map((e) => ({ id: e.id!, name: empName(e) })), [employees]);
  const eName = (id?: string) => employeeOptions.find((o) => o.id === id)?.name ?? "";
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["successionCandidates", planId] });
    queryClient.invalidateQueries({ queryKey: ["successionPlan", planId, "chart"] });
  };

  const { mutate: remove } = useMutation({ mutationFn: (id: string) => deleteSuccessionCandidate(id), onSuccess: invalidate });

  const openAdd = () => { setForm({ rank: (list?.data?.length ?? 0) + 1, readiness: "NotReady" }); setActions([]); setTransfers([]); setGap(null); setProfile(null); setState({}); setOpen(true); };
  const openEdit = async (row: SuccessionCandidateModel) => {
    const full = await getSuccessionCandidate(row.id!);
    setForm(full ?? row); setActions(full?.developmentActions ?? []); setTransfers(full?.knowledgeTransfers ?? []); setGap(null); setProfile(null); setState({}); setOpen(true);
  };
  const set = useCallback((name: string, value: unknown) => setForm((p) => ({ ...p, [name]: value })), []);

  const submit = async () => {
    setSaving(true);
    const payload: SuccessionCandidateModel = {
      ...form, successionPlanId: planId, rank: Number(form.rank ?? 0), readiness: form.readiness ?? "NotReady",
      readinessScore: form.readinessScore != null && String(form.readinessScore) !== "" ? Number(form.readinessScore) : undefined,
      developmentActions: actions.filter((a) => a.description).map((a) => ({ type: a.type ?? "Training", description: a.description, status: a.status ?? "Planned", dueDate: a.dueDate || undefined, mentorEmployeeId: a.mentorEmployeeId || undefined })),
      knowledgeTransfers: transfers.filter((k) => k.topic).map((k) => ({ topic: k.topic, status: k.status ?? "NotStarted", fromEmployeeId: k.fromEmployeeId || undefined, targetDate: k.targetDate || undefined })),
    };
    const result = await saveSuccessionCandidate(payload);
    setState(result); setSaving(false);
    if (result.status === "success") { invalidate(); setOpen(false); }
  };

  const showGap = async () => { if (form.id) setGap(await getCompetencyGap(form.id)); };
  const showProfile = async () => { if (form.id) setProfile(await getCandidateProfile(form.id)); };
  const makePlan = async () => {
    if (!form.id) return;
    const r = await createDevelopmentPlanFromSuccession(form.id);
    setPlanMsg(`Individual Development Plan created with ${r.actionCount} action(s) — view it under Performance › Development Plans.`);
  };
  const doCompute = async () => {
    if (!form.id) return;
    setComputing(true);
    const r = await computeReadiness(form.id);
    setComputing(false);
    setForm((p) => ({ ...p, readiness: r.readiness, readinessScore: r.readinessScore }));
    invalidate();
  };

  const columns: ChildColumn<SuccessionCandidateModel>[] = [
    { name: "rank", label: "Rank", render: (v) => `#${v}` },
    { name: "employeeName", label: "Successor" },
    { name: "readiness", label: "Readiness", render: (v) => label(readinessLevelOptions, v as string) },
    { name: "readinessScore", label: "Score", render: (v) => (v != null ? `${Number(v).toFixed(0)}%` : "—") },
  ];

  return (
    <>
      <ChildManager<SuccessionCandidateModel>
        title="Successors" addLabel="Add Successor" columns={columns}
        rows={list?.data} isLoading={isLoading} onAdd={openAdd} onEdit={openEdit} onDelete={(id) => remove(id)}
      />
      {open && (
        <Modal visible size="lg" title={form.id ? "Edit Successor" : "Add Successor"}
          description="Rank a potential successor and plan their development toward the role."
          onClose={() => setOpen(false)}
          footer={
            <>
              <button type="button" onClick={() => setOpen(false)} className="rounded-lg border border-border px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-secondary">Cancel</button>
              <button type="button" disabled={saving || !form.employeeId} onClick={submit} className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">{saving ? "Saving…" : "Save Successor"}</button>
            </>
          }>
          <div className="space-y-4">
            <EntityFormTabs
              hasId={!!form.id}
              disabledHint="Save the successor to compute insights."
              tabs={[
                {
                  key: "successor",
                  label: "Successor",
                  Icon: UserRound,
                  keepMounted: true,
                  content: (
                    <div className="grid min-h-[15rem] grid-cols-1 gap-4 sm:grid-cols-2">
                      <FormUtility component={{ name: "employeeId", label: "Successor", required: true, type: "dropDown", layout: "auth", value: form.employeeId, displayValue: eName(form.employeeId), data: employeeOptions as never, onSelect: (_n, r: any) => set("employeeId", r.id) }} />
                      <FormUtility component={{ name: "readiness", label: "Readiness", type: "dropDown", layout: "auth", value: form.readiness, displayValue: label(readinessLevelOptions, form.readiness), data: readinessLevelOptions as never, onSelect: (_n, r: any) => set("readiness", r.id) }} />
                      <FormUtility component={{ name: "rank", label: "Rank", type: "text", inputType: "number", layout: "auth", value: form.rank, onChange: (e) => set("rank", e.target.value) }} />
                      <FormUtility component={{ name: "readinessScore", label: "Readiness Score (0–100)", type: "text", inputType: "number", layout: "auth", value: form.readinessScore, onChange: (e) => set("readinessScore", e.target.value) }} />
                      <div className="sm:col-span-2">
                        <FormUtility component={{ name: "gapSummary", label: "Gap Summary", type: "textarea", layout: "auth", value: form.gapSummary, onChange: (e) => set("gapSummary", e.target.value) }} />
                      </div>
                    </div>
                  ),
                },
                {
                  key: "actions",
                  label: "Development Actions",
                  Icon: GraduationCap,
                  keepMounted: true,
                  content: (
                    <div className="min-h-[15rem]">
                      <div className="mb-3 flex items-center justify-between gap-2">
                        <p className="text-xs text-muted">Mentorship, training or rotations to close the gap (HC156).</p>
                        <SectionAddButton onClick={() => setActions((a) => [...a, { type: "Training", status: "Planned", description: "" }])} label="Add action" />
                      </div>
                      {actions.length === 0 ? (
                        <EmptyHint>No development actions yet.</EmptyHint>
                      ) : (
                        <div className="space-y-2">
                          <RepeatHeader cols={["Type", "Description", "Status", ""]} className="grid-cols-[150px_1fr_140px_36px]" />
                          {actions.map((a, i) => (
                            <RepeatRow key={i} className="grid-cols-[150px_1fr_140px_36px]">
                              <FormUtility component={{ name: `at-${i}`, label: "", type: "dropDown", value: a.type, displayValue: label(successionActionTypeOptions, a.type), data: successionActionTypeOptions as never, onSelect: (_n, x: any) => setActions((arr) => arr.map((y, j) => (j === i ? { ...y, type: x.id } : y))) }} />
                              <input className={FORM_INPUT_CLASS} placeholder="Description" value={a.description ?? ""} onChange={(e) => setActions((arr) => arr.map((y, j) => (j === i ? { ...y, description: e.target.value } : y)))} />
                              <FormUtility component={{ name: `as-${i}`, label: "", type: "dropDown", value: a.status, displayValue: label(successionActionStatusOptions, a.status), data: successionActionStatusOptions as never, onSelect: (_n, x: any) => setActions((arr) => arr.map((y, j) => (j === i ? { ...y, status: x.id } : y))) }} />
                              <RowRemoveButton onClick={() => setActions((arr) => arr.filter((_, j) => j !== i))} />
                            </RepeatRow>
                          ))}
                        </div>
                      )}
                    </div>
                  ),
                },
                {
                  key: "transfer",
                  label: "Knowledge Transfer",
                  Icon: BookOpen,
                  keepMounted: true,
                  content: (
                    <div className="min-h-[15rem]">
                      <div className="mb-3 flex items-center justify-between gap-2">
                        <p className="text-xs text-muted">Critical know-how to hand over before transition (HC160).</p>
                        <SectionAddButton onClick={() => setTransfers((t) => [...t, { topic: "", status: "NotStarted" }])} label="Add topic" />
                      </div>
                      {transfers.length === 0 ? (
                        <EmptyHint>No knowledge-transfer topics yet.</EmptyHint>
                      ) : (
                        <div className="space-y-2">
                          <RepeatHeader cols={["Topic", "Status", ""]} className="grid-cols-[1fr_160px_36px]" />
                          {transfers.map((k, i) => (
                            <RepeatRow key={i} className="grid-cols-[1fr_160px_36px]">
                              <input className={FORM_INPUT_CLASS} placeholder="Topic" value={k.topic ?? ""} onChange={(e) => setTransfers((arr) => arr.map((y, j) => (j === i ? { ...y, topic: e.target.value } : y)))} />
                              <FormUtility component={{ name: `ks-${i}`, label: "", type: "dropDown", value: k.status, displayValue: label(knowledgeTransferStatusOptions, k.status), data: knowledgeTransferStatusOptions as never, onSelect: (_n, x: any) => setTransfers((arr) => arr.map((y, j) => (j === i ? { ...y, status: x.id } : y))) }} />
                              <RowRemoveButton onClick={() => setTransfers((arr) => arr.filter((_, j) => j !== i))} />
                            </RepeatRow>
                          ))}
                        </div>
                      )}
                    </div>
                  ),
                },
                {
                  key: "insights",
                  label: "Insights",
                  Icon: Gauge,
                  needsId: true,
                  content: (
                    <div className="min-h-[15rem]">
                      <div className="mb-3 flex flex-wrap items-center gap-2">
                        <p className="mr-auto text-xs text-muted">Readiness, performance &amp; competency gap — computed on demand (HC153/155/158).</p>
                        <button type="button" onClick={doCompute} disabled={computing} className="inline-flex items-center gap-1 rounded-lg bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50"><Gauge size={12} /> {computing ? "Computing…" : "Compute readiness"}</button>
                        <button type="button" onClick={showProfile} className="inline-flex items-center gap-1 rounded-lg border border-border px-2.5 py-1.5 text-xs font-medium text-foreground transition-colors hover:border-primary hover:text-primary"><UserSearch size={12} /> Profile</button>
                        <button type="button" onClick={showGap} className="inline-flex items-center gap-1 rounded-lg border border-border px-2.5 py-1.5 text-xs font-medium text-foreground transition-colors hover:border-primary hover:text-primary"><Target size={12} /> Gap</button>
                        <button type="button" onClick={makePlan} className="inline-flex items-center gap-1 rounded-lg border border-primary/40 bg-primary/10 px-2.5 py-1.5 text-xs font-semibold text-primary transition-colors hover:bg-primary/15"><ListChecks size={12} /> Development plan</button>
                      </div>
                      {planMsg && <p className="mb-2 text-xs text-success">{planMsg}</p>}
                      {!profile && !gap && (
                        <p className="text-xs text-muted">Use the actions above to compute readiness, view the performance profile, or analyze the competency gap.</p>
                      )}
                      {profile && (
                        <div className="grid grid-cols-2 gap-2 rounded-lg border border-border/60 bg-background/50 p-3 text-xs sm:grid-cols-4">
                          <Stat label="Latest appraisal" value={profile.performance?.latestAppraisal?.overallScore != null ? String(profile.performance.latestAppraisal.overallScore) : "—"} />
                          <Stat label="Active goals" value={String(profile.performance?.activeGoals ?? 0)} />
                          <Stat label="Achievements" value={String(profile.performance?.achievementsCount ?? 0)} />
                          <Stat label="Recognitions" value={String(profile.performance?.recognitionsCount ?? 0)} />
                        </div>
                      )}
                      {gap && (
                        <div className="mt-2 text-xs">
                          <p className="mb-1.5 text-muted">{gap.metCount} of {gap.requiredCount} required competencies met.</p>
                          {gap.gaps.length === 0 ? (
                            <p className="text-success">No gaps — the successor's current role covers all required competencies.</p>
                          ) : (
                            <div className="flex flex-wrap gap-1.5">
                              {gap.gaps.map((g) => (
                                <span key={g.competencyId} className="rounded-full border border-warning/40 bg-warning/10 px-2 py-0.5 text-warning">{g.name}</span>
                              ))}
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  ),
                },
              ]}
            />
            <StatusMessage formState={state} status={state?.status} message={state?.message} />
          </div>
        </Modal>
      )}
    </>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col">
      <span className="text-[10px] uppercase tracking-wide text-muted">{label}</span>
      <span className="text-sm font-semibold text-foreground">{value}</span>
    </div>
  );
}

export default memo(Candidates);
