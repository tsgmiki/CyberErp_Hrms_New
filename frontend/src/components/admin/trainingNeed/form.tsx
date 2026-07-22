"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, ClipboardList, Lightbulb, Plus } from "lucide-react";
import type { TrainingNeedModel, TrainingNeedSuggestionModel } from "@/models";
import { getTrainingNeed, saveTrainingNeed, getTrainingNeedSuggestions } from "@/services/admin/trainingNeed";
import getAllTrainingCourse from "@/services/admin/trainingCourse/getAll";
import EmployeePicker from "@/components/common/employeePicker";
import { trainingNeedTypeOptions, trainingNeedPriorityOptions } from "@/constants/orgStructure";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none disabled:opacity-60";
const LABEL = "block text-xs font-medium text-muted mb-1";

const SOURCE_TONE: Record<string, string> = {
  CompetencyGap: "bg-warning/15 text-warning",
  Appraisal: "bg-error/15 text-error",
  Goal: "bg-info/15 text-info",
};

const NEW_DEFAULTS: TrainingNeedModel = { needType: "Local", priority: "Medium", source: "Manual" };

function TrainingNeedForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<TrainingNeedModel>({ ...NEW_DEFAULTS });
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["trainingNeed", id],
    queryFn: () => getTrainingNeed(id),
    enabled: id !== "",
  });

  const [courseParam] = useState({ ...parameterInitialData, take: 200, status: "true" });
  const { data: courses } = useQuery({ queryKey: ["trainingCourses", courseParam], queryFn: () => getAllTrainingCourse(courseParam) });

  // HC189 — performance-driven suggestions load once the employee is chosen (new requests only).
  const { data: suggestions } = useQuery({
    queryKey: ["trainingNeedSuggestions", meta.employeeId],
    queryFn: () => getTrainingNeedSuggestions(meta.employeeId!),
    enabled: !id && !!meta.employeeId,
    staleTime: 30_000,
  });

  useEffect(() => {
    if (record) setMeta({ ...record, neededBy: record.neededBy?.slice(0, 10) });
  }, [record]);

  const set = (name: keyof TrainingNeedModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));
  const editable = !id || meta.status === "Pending";

  const applySuggestion = (s: TrainingNeedSuggestionModel) => {
    setMeta((p) => ({
      ...p,
      topic: s.title,
      justification: s.rationale,
      source: s.source,
      competencyId: s.competencyId,
      competencyName: s.competencyName,
    }));
  };

  const submit = async () => {
    setIsSaving(true);
    const result = await saveTrainingNeed({
      id: meta.id,
      employeeId: meta.employeeId,
      trainingCourseId: meta.trainingCourseId || undefined,
      topic: meta.topic,
      needType: meta.needType,
      justification: meta.justification,
      priority: meta.priority,
      source: meta.source || "Manual",
      competencyId: meta.competencyId || undefined,
      estimatedCost: meta.estimatedCost != null && String(meta.estimatedCost) !== "" ? Number(meta.estimatedCost) : undefined,
      neededBy: meta.neededBy || undefined,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["trainingNeeds"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  const canSave = editable && !!meta.employeeId && !!meta.topic && !!meta.justification;

  return (
    <div className="space-y-4 text-foreground">
      {id && meta.status && meta.status !== "Pending" && (
        <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
          {t("This request is")} <span className="font-semibold text-foreground">{meta.status}</span> — {t("it can no longer be edited here.")}
        </p>
      )}

      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 flex items-center gap-2 text-sm font-semibold">
          <ClipboardList size={16} className="text-primary" /> {t("Training Need")}
        </h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Employee")} *</label>
            {/* Role-scoped: HR = all, manager = subtree, employee = locked to self (HC187). */}
            <EmployeePicker
              value={meta.employeeId}
              displayValue={meta.employeeName}
              disabled={!editable || !!id}
              onSelect={(eid, name) => setMeta((p) => ({ ...p, employeeId: eid, employeeName: name }))}
            />
          </div>
          <div>
            <label className={LABEL}>{t("Catalog Course")}</label>
            <select className={INPUT} disabled={!editable} value={meta.trainingCourseId ?? ""}
              onChange={(e) => {
                const c = (courses?.data ?? []).find((x) => x.id === e.target.value);
                setMeta((p) => ({ ...p, trainingCourseId: e.target.value || undefined, topic: c?.name ?? p.topic }));
              }}>
              <option value="">{t("None — free-text topic")}</option>
              {(courses?.data ?? []).map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
              {meta.trainingCourseId && !(courses?.data ?? []).some((c) => c.id === meta.trainingCourseId) && (
                <option value={meta.trainingCourseId}>{meta.courseName}</option>
              )}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Topic")} *</label>
            <input type="text" className={INPUT} disabled={!editable} placeholder={t("What training is needed?")}
              value={meta.topic ?? ""} onChange={(e) => set("topic", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Delivery")} *</label>
            <select className={INPUT} disabled={!editable} value={meta.needType ?? "Local"} onChange={(e) => set("needType", e.target.value)}>
              {trainingNeedTypeOptions.map((o) => (
                <option key={o.id} value={o.id}>{o.name}</option>
              ))}
            </select>
            <p className="mt-1 text-xs text-muted">{t("Abroad requests route through the extended approval chain.")}</p>
          </div>
          <div>
            <label className={LABEL}>{t("Priority")}</label>
            <select className={INPUT} disabled={!editable} value={meta.priority ?? "Medium"} onChange={(e) => set("priority", e.target.value)}>
              {trainingNeedPriorityOptions.map((o) => (
                <option key={o.id} value={o.id}>{o.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Needed By")}</label>
            <input type="date" className={INPUT} disabled={!editable} value={meta.neededBy ?? ""} onChange={(e) => set("neededBy", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Estimated Cost")}</label>
            <input type="number" min={0} className={INPUT} disabled={!editable} placeholder={t("Counted against the training budget")}
              value={meta.estimatedCost ?? ""} onChange={(e) => set("estimatedCost", e.target.value)} />
          </div>
          {meta.competencyName && (
            <div>
              <label className={LABEL}>{t("Linked Competency")}</label>
              <p className="rounded-md border border-border bg-secondary/20 px-2.5 py-1.5 text-sm">{meta.competencyName}</p>
            </div>
          )}
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Justification")} *</label>
            <textarea className={INPUT} rows={3} disabled={!editable}
              placeholder={t("Why is this training needed?")}
              value={meta.justification ?? ""} onChange={(e) => set("justification", e.target.value)} />
          </div>
        </div>
        {editable && (
          <div className="mt-4 flex justify-end">
            <button
              type="button"
              disabled={!canSave || isSaving}
              onClick={submit}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
            >
              <Save size={14} /> {isSaving ? t("Saving…") : t("Submit Request")}
            </button>
          </div>
        )}
      </div>

      {/* HC189 — one-click suggestions from performance data. */}
      {!id && meta.employeeId && (suggestions?.length ?? 0) > 0 && (
        <div className="rounded-lg border border-border bg-card p-4">
          <h3 className="mb-1 flex items-center gap-2 text-sm font-semibold">
            <Lightbulb size={16} className="text-warning" /> {t("Suggested by performance data")}
          </h3>
          <p className="mb-3 text-xs text-muted">
            {t("Derived from low-scored competencies, appraisal results and active goals — click to prefill the request.")}
          </p>
          <div className="space-y-2">
            {(suggestions ?? []).map((s, i) => (
              <div key={i} className="flex items-start justify-between gap-3 rounded-md border border-border/70 bg-secondary/10 px-3 py-2">
                <div className="min-w-0">
                  <p className="flex items-center gap-2 text-sm font-medium">
                    <span className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${SOURCE_TONE[s.source ?? ""] ?? "bg-secondary/40"}`}>{s.source}</span>
                    <span className="truncate">{s.title}</span>
                  </p>
                  <p className="mt-0.5 text-xs text-muted">{s.rationale}</p>
                </div>
                <button
                  type="button"
                  onClick={() => applySuggestion(s)}
                  className="inline-flex shrink-0 items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-foreground hover:bg-secondary/40"
                >
                  <Plus size={12} /> {t("Use")}
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(TrainingNeedForm);
