"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Send } from "lucide-react";
import { submitExitInterview } from "@/services/admin/employee/exitManagement";
import type { ExitInterviewModel } from "@/models";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

/** HC219 — answers a pending exit interview (leaver self-service or HR recording). */
function ExitInterviewForm({ interview, onSubmitted }: { interview: ExitInterviewModel; onSubmitted: (msg: string) => void }) {
  const { t } = useTranslation();
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const set = (key: string, value: string) => setAnswers((p) => ({ ...p, [key]: value }));
  const questions = interview.questions ?? [];
  const missingRequired = questions.some((q) => q.required !== false && !answers[q.key ?? ""]?.trim());

  const submit = async () => {
    if (!interview.id) return;
    setBusy(true);
    setError("");
    const res = await submitExitInterview(interview.id, answers);
    setBusy(false);
    if (res.ok) onSubmitted(t("Thank you — the interview has been recorded."));
    else setError(res.message);
  };

  return (
    <div className="space-y-3">
      {questions.map((q) => (
        <div key={q.key}>
          <label className="mb-1 block text-xs font-medium text-muted">
            {q.text} {q.required !== false && "*"}
          </label>
          {q.type === "Rating" ? (
            <select className={INPUT} value={answers[q.key ?? ""] ?? ""} onChange={(e) => set(q.key ?? "", e.target.value)}>
              <option value="">{t("Select a rating")}</option>
              {[5, 4, 3, 2, 1].map((n) => <option key={n} value={n}>{n}</option>)}
            </select>
          ) : q.type === "Choice" ? (
            <select className={INPUT} value={answers[q.key ?? ""] ?? ""} onChange={(e) => set(q.key ?? "", e.target.value)}>
              <option value="">{t("Select an option")}</option>
              {(q.options ?? []).map((o) => <option key={o} value={o}>{o}</option>)}
            </select>
          ) : (
            <textarea className={INPUT} rows={2} value={answers[q.key ?? ""] ?? ""} onChange={(e) => set(q.key ?? "", e.target.value)} />
          )}
        </div>
      ))}
      {error && <p className="text-xs text-error">{error}</p>}
      <div className="flex justify-end">
        <button type="button" disabled={busy || missingRequired} onClick={submit}
          className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
          <Send size={14} /> {busy ? t("Submitting…") : t("Submit Interview")}
        </button>
      </div>
    </div>
  );
}

export default memo(ExitInterviewForm);
