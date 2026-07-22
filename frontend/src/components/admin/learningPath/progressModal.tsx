"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { TrendingUp, X, CheckCircle2, Circle } from "lucide-react";
import { getLearningPathProgress } from "@/services/admin/learningPath";
import EmployeePicker from "@/components/common/employeePicker";
import type { LearningPathModel } from "@/models";

/** HC193 — one employee's completion progress along the path (a step completes via a completed enrollment). */
function ProgressModal({ path, onClose }: { path: LearningPathModel; onClose: () => void }) {
  const { t } = useTranslation();
  const [employeeId, setEmployeeId] = useState("");
  const [employeeName, setEmployeeName] = useState("");

  const { data, error } = useQuery({
    queryKey: ["learningPathProgress", path.id, employeeId],
    queryFn: () => getLearningPathProgress(path.id!, employeeId),
    enabled: !!employeeId && !!path.id,
    retry: false,
  });

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="flex max-h-[90vh] w-full max-w-lg flex-col rounded-lg border border-border bg-background shadow-xl">
        <div className="flex items-center justify-between border-b border-border px-4 py-3">
          <h3 className="flex items-center gap-2 text-sm font-semibold text-foreground">
            <TrendingUp size={16} /> {path.name} — {t("Progress")}
          </h3>
          <button type="button" onClick={onClose} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
        </div>

        <div className="border-b border-border px-4 py-3">
          <label className="mb-1 block text-xs font-medium text-muted">{t("Employee")}</label>
          <EmployeePicker value={employeeId} displayValue={employeeName}
            onSelect={(id, name) => { setEmployeeId(id); setEmployeeName(name); }} />
        </div>

        <div className="min-h-0 flex-1 overflow-auto p-4">
          {!employeeId ? (
            <p className="py-6 text-center text-sm text-muted">{t("Pick an employee to see their progress.")}</p>
          ) : error ? (
            <p className="py-6 text-center text-sm text-error">{t("You do not have access to this employee's progress.")}</p>
          ) : !data ? (
            <p className="py-6 text-center text-sm text-muted">{t("Loading…")}</p>
          ) : (
            <>
              <div className="mb-1 flex items-center justify-between text-xs text-muted">
                <span>{data.completedSteps} / {data.totalSteps} {t("courses")} · {t("required")} {data.completedRequiredSteps}/{data.requiredSteps}</span>
                <span className="font-semibold text-foreground">{data.progressPercent}%</span>
              </div>
              <div className="mb-4 h-2 overflow-hidden rounded-full bg-secondary/40">
                <div className="h-full bg-primary" style={{ width: `${data.progressPercent}%` }} />
              </div>
              <ol className="space-y-1.5">
                {data.steps.map((s) => (
                  <li key={s.trainingCourseId} className="flex items-center gap-2 rounded-md border border-border/70 px-3 py-1.5">
                    {s.completed
                      ? <CheckCircle2 size={15} className="shrink-0 text-success" />
                      : <Circle size={15} className="shrink-0 text-muted" />}
                    <span className={`min-w-0 flex-1 truncate text-sm ${s.completed ? "" : "text-muted"}`}>{s.courseName}</span>
                    {s.isRequired && <span className="rounded-full bg-warning/15 px-2 py-0.5 text-[11px] font-semibold text-warning">{t("Required")}</span>}
                  </li>
                ))}
              </ol>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default memo(ProgressModal);
