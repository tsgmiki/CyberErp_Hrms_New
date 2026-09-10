"use client";
import { memo, useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ClipboardCheck, Save, Download } from "lucide-react";
import {
  getAssessment, saveAssessment, setAssessmentQuestions, importQuestions,
} from "@/services/admin/assessment";
import getAllQuestionBank from "@/services/admin/questionBank/getAll";
import type { AssessmentModel, QuestionModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import QuestionEditor from "../questionBank/questionEditor";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";

const DEFAULTS = {
  title: "",
  instructions: "",
  passMark: 70,
  maxAttempts: 3 as number | null,
  timeLimitMinutes: null as number | null,
  shuffleQuestions: false,
  revealAnswers: true,
};

/**
 * The quiz on a Quiz module.
 *
 * <p>It hangs off the MODULE, not the course, so it freezes exactly when the course version does —
 * a published quiz cannot have its pass mark quietly lowered under the people who already failed it.
 * The screen only appears while the version is a draft.</p>
 *
 * <p>Two saves, because they are two different things: the settings row must exist before questions
 * can attach to it, exactly as a course must exist before its content can.</p>
 */
function QuizSection({ moduleId, moduleTitle }: { moduleId?: string; moduleTitle: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const [settings, setSettings] = useState({ ...DEFAULTS });
  const [questions, setQuestions] = useState<QuestionModel[]>([]);
  const [settingsDirty, setSettingsDirty] = useState(false);
  const [questionsDirty, setQuestionsDirty] = useState(false);
  const [busy, setBusy] = useState(false);
  const [bankId, setBankId] = useState("");

  const enabled = !!moduleId;

  const { data: assessment, isLoading } = useQuery({
    queryKey: ["assessment", moduleId],
    queryFn: () => getAssessment(moduleId as string),
    enabled,
  });

  const { data: banks } = useQuery({
    queryKey: ["questionBanks", "quiz-import"],
    queryFn: () => getAllQuestionBank({ ...parameterInitialData, take: 200 } as never),
    enabled,
  });

  useEffect(() => {
    const a = assessment as AssessmentModel | null | undefined;
    setSettings(a
      ? {
          title: a.title,
          instructions: a.instructions ?? "",
          passMark: a.passMark,
          maxAttempts: a.maxAttempts ?? null,
          timeLimitMinutes: a.timeLimitMinutes ?? null,
          shuffleQuestions: a.shuffleQuestions,
          revealAnswers: a.revealAnswers,
        }
      // A new quiz is named after the module it is on, which is nearly always what the author wants.
      : { ...DEFAULTS, title: moduleTitle });
    setQuestions((a?.questions ?? []).map((q) => ({ ...q, options: q.options.map((o) => ({ ...o })) })));
    setSettingsDirty(false);
    setQuestionsDirty(false);
  }, [assessment, moduleTitle]);

  const bankOptions = useMemo(
    () => (banks?.data ?? []).filter((b) => b.isActive !== false && (b.questionCount ?? 0) > 0),
    [banks],
  );

  if (!enabled) {
    return (
      <p className="rounded-md border border-dashed border-border px-3 py-2 text-xs text-muted">
        {t("Save the content once — a quiz attaches to a saved module.")}
      </p>
    );
  }
  if (isLoading) return <Loading />;

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["assessment", moduleId] });

  const saveSettings = async () => {
    setBusy(true);
    const res = await saveAssessment({
      contentModuleId: moduleId as string,
      title: settings.title,
      instructions: settings.instructions || null,
      passMark: Number(settings.passMark),
      maxAttempts: settings.maxAttempts ? Number(settings.maxAttempts) : null,
      timeLimitMinutes: settings.timeLimitMinutes ? Number(settings.timeLimitMinutes) : null,
      shuffleQuestions: settings.shuffleQuestions,
      revealAnswers: settings.revealAnswers,
    });
    setBusy(false);
    if (res.status === "success") { toast.success(res.message); refresh(); }
    else toast.error(res.message);
  };

  const saveQuestions = async () => {
    if (!assessment?.id) {
      toast.error(t("Save the quiz settings first."));
      return;
    }
    setBusy(true);
    const res = await setAssessmentQuestions(assessment.id, questions);
    setBusy(false);
    if (res.status === "success") { toast.success(res.message); refresh(); }
    else toast.error(res.message);
  };

  const runImport = async () => {
    if (!assessment?.id || !bankId) return;
    if (questionsDirty) {
      // Import re-reads the stored quiz, so unsaved edits would be silently discarded.
      toast.error(t("Save your question changes before importing."));
      return;
    }
    setBusy(true);
    const res = await importQuestions(assessment.id, bankId);
    setBusy(false);
    if (res.status === "success") { toast.success(res.message); setBankId(""); refresh(); }
    else toast.error(res.message);
  };

  const totalMarks = questions.reduce((sum, q) => sum + (Number(q.points) || 0), 0);

  return (
    <div className="mt-2 rounded-md border border-primary/25 bg-primary/5 p-2.5">
      <div className="mb-2 flex items-center gap-2">
        <ClipboardCheck className="h-4 w-4 shrink-0 text-primary" />
        <p className="min-w-0 flex-1 text-xs font-semibold text-foreground">{t("Quiz")}</p>
        <button
          type="button" disabled={busy || !settingsDirty} onClick={saveSettings}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-2.5 py-1 text-[11px] font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
        >
          <Save className="h-3 w-3" />
          {busy ? t("Saving…") : settingsDirty ? t("Save Settings") : t("Saved")}
        </button>
      </div>

      <div className="mb-2 grid gap-2 sm:grid-cols-2">
        <input
          value={settings.title}
          onChange={(e) => { setSettings((s) => ({ ...s, title: e.target.value })); setSettingsDirty(true); }}
          placeholder={t("Quiz title") ?? ""}
          className="rounded border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none"
        />
        <div className="flex items-center gap-2 text-xs text-muted">
          <label className="inline-flex items-center gap-1.5">
            <input
              type="number" min={0} max={100}
              value={settings.passMark}
              onChange={(e) => { setSettings((s) => ({ ...s, passMark: Number(e.target.value) })); setSettingsDirty(true); }}
              className="w-16 rounded border border-border bg-card px-1.5 py-0.5 text-xs text-foreground focus:border-primary focus:outline-none"
            />
            {t("% to pass")}
          </label>
          <label className="inline-flex items-center gap-1.5">
            <input
              type="number" min={1}
              value={settings.maxAttempts ?? ""}
              onChange={(e) => {
                setSettings((s) => ({ ...s, maxAttempts: e.target.value === "" ? null : Number(e.target.value) }));
                setSettingsDirty(true);
              }}
              placeholder="∞"
              className="w-14 rounded border border-border bg-card px-1.5 py-0.5 text-xs text-foreground focus:border-primary focus:outline-none"
            />
            {t("attempts")}
          </label>
          <label className="inline-flex items-center gap-1.5">
            <input
              type="number" min={1}
              value={settings.timeLimitMinutes ?? ""}
              onChange={(e) => {
                setSettings((s) => ({ ...s, timeLimitMinutes: e.target.value === "" ? null : Number(e.target.value) }));
                setSettingsDirty(true);
              }}
              placeholder="—"
              className="w-14 rounded border border-border bg-card px-1.5 py-0.5 text-xs text-foreground focus:border-primary focus:outline-none"
            />
            {t("min limit")}
          </label>
        </div>
      </div>

      <input
        value={settings.instructions}
        onChange={(e) => { setSettings((s) => ({ ...s, instructions: e.target.value })); setSettingsDirty(true); }}
        placeholder={t("Instructions shown before the learner starts (optional)") ?? ""}
        className="mb-2 w-full rounded border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
      />

      <div className="mb-2 flex flex-wrap items-center gap-4 text-xs text-muted">
        <label className="inline-flex items-center gap-1.5">
          <input
            type="checkbox" checked={settings.shuffleQuestions}
            onChange={(e) => { setSettings((s) => ({ ...s, shuffleQuestions: e.target.checked })); setSettingsDirty(true); }}
            className="h-3.5 w-3.5 accent-primary"
          />
          {t("Shuffle questions")}
        </label>
        <label className="inline-flex items-center gap-1.5">
          <input
            type="checkbox" checked={settings.revealAnswers}
            onChange={(e) => { setSettings((s) => ({ ...s, revealAnswers: e.target.checked })); setSettingsDirty(true); }}
            className="h-3.5 w-3.5 accent-primary"
          />
          {/* The exact rule, because "show answers" alone reads as "show them after every attempt". */}
          {t("Show answers once they pass or run out of attempts")}
        </label>
      </div>

      {!assessment?.id ? (
        <p className="rounded border border-dashed border-border px-2.5 py-2 text-[11px] text-muted">
          {t("Save the settings above, then add the questions.")}
        </p>
      ) : (
        <>
          <div className="mb-2 flex flex-wrap items-center gap-2">
            <select
              value={bankId}
              onChange={(e) => setBankId(e.target.value)}
              className="min-w-0 flex-1 rounded border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
            >
              <option value="">{t("Import from a question bank…")}</option>
              {bankOptions.map((b) => (
                <option key={b.id} value={b.id}>{`${b.name} (${b.questionCount})`}</option>
              ))}
            </select>
            <button
              type="button" disabled={busy || !bankId} onClick={runImport}
              className="inline-flex shrink-0 items-center gap-1.5 rounded-md border border-border px-2.5 py-1 text-[11px] font-semibold text-foreground hover:bg-secondary disabled:opacity-40"
            >
              <Download className="h-3 w-3" /> {t("Import")}
            </button>
            <button
              type="button" disabled={busy || !questionsDirty} onClick={saveQuestions}
              className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-2.5 py-1 text-[11px] font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
            >
              <Save className="h-3 w-3" />
              {questionsDirty ? t("Save Questions") : t("Saved")}
            </button>
          </div>

          <QuestionEditor
            questions={questions}
            onChange={(next) => { setQuestions(next); setQuestionsDirty(true); }}
            emptyHint={t("No questions yet — the version cannot be published until this quiz has some.") ?? undefined}
          />

          {questions.length > 0 && (
            <p className="mt-1.5 text-[11px] text-muted">
              {`${t("Pass mark")} ${settings.passMark}% ${t("of")} ${totalMarks} ${t("marks")} = ${((Number(settings.passMark) / 100) * totalMarks).toFixed(1)} ${t("marks needed")}`}
            </p>
          )}
        </>
      )}
    </div>
  );
}

export default memo(QuizSection);
