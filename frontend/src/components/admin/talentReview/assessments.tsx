"use client";
import { memo, useCallback, useMemo, useState } from "react";
import { UserRound, Star } from "lucide-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Modal from "@/components/common/modal";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import ChildManager, { type ChildColumn } from "@/components/admin/employee/childManager";
import { StatusMessage } from "@/components/common/statusMessage/status";
import { RepeatHeader, RepeatRow, RowRemoveButton, SectionAddButton, EmptyHint } from "@/components/common/formSection";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import { FORM_INPUT_CLASS, FORM_CHECKBOX_CLASS } from "@/components/ui/fieldStyles";
import type { TalentAssessmentModel, TalentRatingModel } from "@/models";
import getAllTalentAssessment from "@/services/admin/talentAssessment/getAll";
import getTalentAssessment from "@/services/admin/talentAssessment/get";
import { saveTalentAssessment } from "@/services/admin/talentAssessment/save";
import deleteTalentAssessment from "@/services/admin/talentAssessment/delete";
import getAllEmployee from "@/services/admin/employee/getAll";
import { identifyHipos } from "@/services/admin/talentReview/identifyHipos";
import { toast } from "@/components/common/toast";
import { Sparkles } from "lucide-react";
import { parameterInitialData } from "@/constants/initialization";
import { bandOptions, bandLabel, readinessLevelOptions } from "@/constants/careerDevelopment";

const empName = (e: any) => `${e.fullName ?? `${e.firstName ?? ""} ${e.grandFatherName ?? ""}`.trim()}${e.employeeNumber ? ` (${e.employeeNumber})` : ""}`;

function Assessments({ reviewId }: { reviewId: string }) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<TalentAssessmentModel>({});
  const [ratings, setRatings] = useState<TalentRatingModel[]>([]);
  const [saving, setSaving] = useState(false);
  const [state, setState] = useState<any>({});

  const { data: list, isLoading } = useQuery({
    queryKey: ["talentAssessments", reviewId],
    queryFn: () => getAllTalentAssessment({ ...parameterInitialData, take: 200, parentId: reviewId }),
    enabled: !!reviewId,
  });
  const { data: employees } = useQuery({
    queryKey: ["employees", "assessmentPicker"],
    queryFn: () => getAllEmployee({ ...parameterInitialData, take: 500 }),
    staleTime: 60_000,
    enabled: open,
  });
  const employeeOptions = useMemo(() => (employees?.data ?? []).map((e) => ({ id: e.id!, name: empName(e) })), [employees]);
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["talentAssessments", reviewId] });
    queryClient.invalidateQueries({ queryKey: ["talentReview", reviewId, "nine-box"] });
  };

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteTalentAssessment(id),
    onSuccess: invalidate,
  });

  const openAdd = () => { setForm({ performanceBand: 2, potentialBand: 2, readiness: "NotReady", isHiPo: false }); setRatings([]); setState({}); setOpen(true); };
  const openEdit = async (row: TalentAssessmentModel) => {
    const full = await getTalentAssessment(row.id!);
    setForm(full ?? row);
    setRatings(full?.ratings ?? []);
    setState({}); setOpen(true);
  };

  const set = useCallback((name: string, value: unknown) => setForm((p) => ({ ...p, [name]: value })), []);
  const addRating = () => setRatings((r) => [...r, { raterEmployeeId: "", raterRole: "", performanceScore: undefined, potentialScore: undefined }]);
  const setRating = (i: number, patch: Partial<TalentRatingModel>) => setRatings((r) => r.map((x, j) => (j === i ? { ...x, ...patch } : x)));
  const removeRating = (i: number) => setRatings((r) => r.filter((_, j) => j !== i));

  const submit = async () => {
    setSaving(true);
    const payload: TalentAssessmentModel = {
      ...form, talentReviewId: reviewId,
      performanceBand: Number(form.performanceBand ?? 2), potentialBand: Number(form.potentialBand ?? 2),
      isHiPo: !!form.isHiPo, readiness: form.readiness ?? "NotReady",
      ratings: ratings.filter((r) => r.raterEmployeeId).map((r) => ({
        raterEmployeeId: r.raterEmployeeId, raterRole: r.raterRole,
        performanceScore: r.performanceScore != null && r.performanceScore !== ("" as never) ? Number(r.performanceScore) : undefined,
        potentialScore: r.potentialScore != null && r.potentialScore !== ("" as never) ? Number(r.potentialScore) : undefined,
        comment: r.comment,
      })),
    };
    const result = await saveTalentAssessment(payload);
    setState(result); setSaving(false);
    if (result.status === "success") { invalidate(); setOpen(false); }
  };

  const columns: ChildColumn<TalentAssessmentModel>[] = [
    { name: "employeeName", label: "Employee" },
    { name: "performanceBand", label: "Performance", render: (v) => bandLabel(Number(v)) },
    { name: "potentialBand", label: "Potential", render: (v) => bandLabel(Number(v)) },
    { name: "isHiPo", label: "HiPo", render: (v) => (v ? "★" : "—") },
    { name: "readiness", label: "Readiness", render: (v) => readinessLevelOptions.find((o) => o.id === v)?.name ?? String(v ?? "") },
  ];

  const eName = (id?: string) => employeeOptions.find((o) => o.id === id)?.name ?? "";

  const runIdentifyHiPos = async () => {
    const r = await identifyHipos(reviewId);
    invalidate();
    toast.success(`Flagged ${r.flagged} new high-potential${r.flagged === 1 ? "" : "s"} (${r.totalHiPo} total).`);
  };

  return (
    <>
      <div className="mb-2 flex justify-end">
        <button type="button" onClick={runIdentifyHiPos}
          className="inline-flex items-center gap-1.5 rounded-md border border-primary/40 bg-primary/10 px-2.5 py-1.5 text-xs font-semibold text-primary hover:bg-primary/15">
          <Sparkles size={13} /> Identify HiPos
        </button>
      </div>
      <ChildManager<TalentAssessmentModel>
        title="Assessments (9-box placements)"
        addLabel="Add Assessment"
        columns={columns}
        rows={list?.data}
        isLoading={isLoading}
        onAdd={openAdd}
        onEdit={openEdit}
        onDelete={(id) => remove(id)}
      />
      {open && (
        <Modal visible size="lg" title={form.id ? "Edit Assessment" : "Add Assessment"}
          description="Place an employee on the 9-box grid and capture multi-rater input."
          onClose={() => setOpen(false)}
          footer={
            <>
              <button type="button" onClick={() => setOpen(false)} className="rounded-lg border border-border px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-secondary">Cancel</button>
              <button type="button" disabled={saving || !form.employeeId} onClick={submit} className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">{saving ? "Saving…" : "Save Assessment"}</button>
            </>
          }>
          <div className="space-y-4">
            <EntityFormTabs
              hasId
              tabs={[
                {
                  key: "placement",
                  label: "Placement",
                  Icon: UserRound,
                  keepMounted: true,
                  content: (
                    <div className="grid min-h-[15rem] grid-cols-1 gap-4 sm:grid-cols-2">
                      <FormUtility component={{ name: "employeeId", label: "Employee", required: true, type: "dropDown", layout: "auth", value: form.employeeId, displayValue: eName(form.employeeId), data: employeeOptions as never, onSelect: (_n, r: any) => set("employeeId", r.id) }} />
                      <FormUtility component={{ name: "readiness", label: "Readiness", type: "dropDown", layout: "auth", value: form.readiness, displayValue: readinessLevelOptions.find((o) => o.id === form.readiness)?.name, data: readinessLevelOptions as never, onSelect: (_n, r: any) => set("readiness", r.id) }} />
                      <FormUtility component={{ name: "performanceBand", label: "Performance", type: "dropDown", layout: "auth", value: String(form.performanceBand ?? 2), displayValue: bandLabel(Number(form.performanceBand ?? 2)), data: bandOptions as never, onSelect: (_n, r: any) => set("performanceBand", Number(r.id)) }} />
                      <FormUtility component={{ name: "potentialBand", label: "Potential", type: "dropDown", layout: "auth", value: String(form.potentialBand ?? 2), displayValue: bandLabel(Number(form.potentialBand ?? 2)), data: bandOptions as never, onSelect: (_n, r: any) => set("potentialBand", Number(r.id)) }} />
                      <label className="flex cursor-pointer items-center gap-2.5 rounded-lg border border-border bg-background/50 px-3 py-2.5 text-sm sm:col-span-2">
                        <input type="checkbox" className={FORM_CHECKBOX_CLASS} checked={!!form.isHiPo} onChange={(e) => set("isHiPo", e.target.checked)} />
                        <Star size={15} className={form.isHiPo ? "text-warning" : "text-muted"} />
                        <span className="font-medium text-foreground">High-potential (HiPo)</span>
                        <span className="ml-auto text-xs text-muted">Flag as a top-talent successor</span>
                      </label>
                      <div className="sm:col-span-2">
                        <FormUtility component={{ name: "notes", label: "Notes", type: "textarea", layout: "auth", value: form.notes, onChange: (e) => set("notes", e.target.value) }} />
                      </div>
                    </div>
                  ),
                },
                {
                  key: "raters",
                  label: "Rater Inputs",
                  Icon: Star,
                  keepMounted: true,
                  content: (
                    <div className="min-h-[15rem]">
                      <div className="mb-3 flex items-center justify-between gap-2">
                        <p className="text-xs text-muted">Optional per-rater performance / potential scores (HC149).</p>
                        <SectionAddButton onClick={addRating} label="Add rater" />
                      </div>
                      {ratings.length === 0 ? (
                        <EmptyHint>No rater inputs yet.</EmptyHint>
                      ) : (
                        <div className="space-y-2">
                          <RepeatHeader cols={["Rater", "Role", "Perf.", "Pot.", ""]} className="grid-cols-[1fr_120px_80px_80px_36px]" />
                          {ratings.map((r, i) => (
                            <RepeatRow key={i} className="grid-cols-[1fr_120px_80px_80px_36px]">
                              <FormUtility component={{ name: `rater-${i}`, label: "", placeholder: "Select rater…", type: "dropDown", value: r.raterEmployeeId, displayValue: eName(r.raterEmployeeId), data: employeeOptions as never, onSelect: (_n, x: any) => setRating(i, { raterEmployeeId: x.id }) }} />
                              <input className={FORM_INPUT_CLASS} placeholder="Role" value={r.raterRole ?? ""} onChange={(e) => setRating(i, { raterRole: e.target.value })} />
                              <input type="number" step="0.1" className={FORM_INPUT_CLASS} placeholder="0.0" value={r.performanceScore ?? ""} onChange={(e) => setRating(i, { performanceScore: e.target.value as never })} />
                              <input type="number" step="0.1" className={FORM_INPUT_CLASS} placeholder="0.0" value={r.potentialScore ?? ""} onChange={(e) => setRating(i, { potentialScore: e.target.value as never })} />
                              <RowRemoveButton onClick={() => removeRating(i)} />
                            </RepeatRow>
                          ))}
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

export default memo(Assessments);
