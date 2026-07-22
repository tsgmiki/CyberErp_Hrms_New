"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, Route, Plus, Trash2, ArrowUp, ArrowDown } from "lucide-react";
import type { LearningPathModel, LearningPathStepModel } from "@/models";
import { getLearningPath, saveLearningPath } from "@/services/admin/learningPath";
import getAllTrainingCourse from "@/services/admin/trainingCourse/getAll";
import getAllPosition from "@/services/admin/position/getAll";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface StepDraft {
  trainingCourseId: string;
  courseName?: string;
  isRequired: boolean;
}

function LearningPathForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<LearningPathModel>({ isActive: true });
  const [steps, setSteps] = useState<StepDraft[]>([]);
  const [pickCourse, setPickCourse] = useState("");
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["learningPath", id],
    queryFn: () => getLearningPath(id),
    enabled: id !== "",
  });

  const [courseParam] = useState({ ...parameterInitialData, take: 200, status: "true" });
  const { data: courses } = useQuery({ queryKey: ["trainingCourses", courseParam], queryFn: () => getAllTrainingCourse(courseParam) });
  const [posParam] = useState({ ...parameterInitialData, take: 300 });
  const { data: positions } = useQuery({ queryKey: ["positions", posParam], queryFn: () => getAllPosition(posParam) });

  useEffect(() => {
    if (record) {
      setMeta(record);
      setSteps((record.steps ?? []).map((s: LearningPathStepModel) => ({
        trainingCourseId: s.trainingCourseId!,
        courseName: s.courseName,
        isRequired: s.isRequired !== false,
      })));
    }
  }, [record]);

  const set = (name: keyof LearningPathModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));

  const addStep = () => {
    if (!pickCourse || steps.some((s) => s.trainingCourseId === pickCourse)) return;
    const c = (courses?.data ?? []).find((x) => x.id === pickCourse);
    setSteps((p) => [...p, { trainingCourseId: pickCourse, courseName: c?.name, isRequired: true }]);
    setPickCourse("");
  };
  const move = (i: number, dir: -1 | 1) =>
    setSteps((p) => {
      const next = [...p];
      const j = i + dir;
      if (j < 0 || j >= next.length) return p;
      [next[i], next[j]] = [next[j], next[i]];
      return next;
    });

  const submit = async () => {
    setIsSaving(true);
    const result = await saveLearningPath({
      id: meta.id,
      name: meta.name,
      description: meta.description || undefined,
      targetPositionId: meta.targetPositionId || undefined,
      isActive: meta.isActive !== false,
      steps: steps.map((s) => ({ trainingCourseId: s.trainingCourseId, isRequired: s.isRequired })),
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["learningPaths"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  const canSave = !!meta.name && steps.length > 0;

  return (
    <div className="space-y-4 text-foreground">
      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 flex items-center gap-2 text-sm font-semibold">
          <Route size={16} className="text-primary" /> {t("Learning Path")}
        </h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Name")} *</label>
            <input type="text" className={INPUT} placeholder={t("e.g. Data Engineer Track")} value={meta.name ?? ""} onChange={(e) => set("name", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Target Position (career alignment)")}</label>
            <select className={INPUT} value={meta.targetPositionId ?? ""} onChange={(e) => set("targetPositionId", e.target.value || undefined)}>
              <option value="">{t("None")}</option>
              {(positions?.data ?? []).map((p: any) => (
                <option key={p.id} value={p.id}>{p.code} {p.title ? `— ${p.title}` : ""}</option>
              ))}
              {meta.targetPositionId && !(positions?.data ?? []).some((p: any) => p.id === meta.targetPositionId) && (
                <option value={meta.targetPositionId}>{meta.targetPositionName}</option>
              )}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Status")}</label>
            <select className={INPUT} value={meta.isActive === false ? "false" : "true"} onChange={(e) => set("isActive", e.target.value === "true")}>
              <option value="true">{t("Active")}</option>
              <option value="false">{t("Inactive")}</option>
            </select>
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Description")}</label>
            <textarea className={INPUT} rows={2} value={meta.description ?? ""} onChange={(e) => set("description", e.target.value)} />
          </div>
        </div>
      </div>

      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("Course Sequence")} ({steps.length})</h3>
        <div className="mb-3 flex items-end gap-2">
          <div className="min-w-[220px] flex-1">
            <label className={LABEL}>{t("Add course")}</label>
            <select className={INPUT} value={pickCourse} onChange={(e) => setPickCourse(e.target.value)}>
              <option value="">{t("Select a course")}</option>
              {(courses?.data ?? [])
                .filter((c) => !steps.some((s) => s.trainingCourseId === c.id))
                .map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
            </select>
          </div>
          <button type="button" disabled={!pickCourse} onClick={addStep}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-2 text-sm font-semibold text-foreground hover:bg-secondary/40 disabled:opacity-50">
            <Plus size={14} /> {t("Add")}
          </button>
        </div>
        {steps.length === 0 ? (
          <p className="rounded-md border border-dashed border-border p-4 text-center text-xs text-muted">{t("Add at least one course to the path.")}</p>
        ) : (
          <ol className="space-y-1.5">
            {steps.map((s, i) => (
              <li key={s.trainingCourseId} className="flex items-center gap-2 rounded-md border border-border/70 bg-secondary/10 px-3 py-1.5">
                <span className="w-5 text-center text-xs font-bold text-muted">{i + 1}</span>
                <span className="min-w-0 flex-1 truncate text-sm">{s.courseName}</span>
                <label className="flex items-center gap-1 text-xs text-muted">
                  <input type="checkbox" checked={s.isRequired}
                    onChange={(e) => setSteps((p) => p.map((x, j) => (j === i ? { ...x, isRequired: e.target.checked } : x)))} />
                  {t("Required")}
                </label>
                <button type="button" onClick={() => move(i, -1)} className="rounded p-1 text-muted hover:text-primary"><ArrowUp size={13} /></button>
                <button type="button" onClick={() => move(i, 1)} className="rounded p-1 text-muted hover:text-primary"><ArrowDown size={13} /></button>
                <button type="button" onClick={() => setSteps((p) => p.filter((_x, j) => j !== i))} className="rounded p-1 text-muted hover:text-error"><Trash2 size={13} /></button>
              </li>
            ))}
          </ol>
        )}
        <div className="mt-4 flex justify-end">
          <button
            type="button"
            disabled={!canSave || isSaving}
            onClick={submit}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Save size={14} /> {isSaving ? t("Saving…") : t("Save Path")}
          </button>
        </div>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(LearningPathForm);
