"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ClipboardList, Plus, Trash2, ArrowUp, ArrowDown, Save } from "lucide-react";
import { getExitQuestionnaire, saveExitQuestionnaire } from "@/services/admin/employee/exitManagement";
import type { ExitQuestionModel } from "@/models";
import Loading from "../../common/loader/loader";
import EmptyState from "../../common/emptyState";
import { FORM_INPUT_CLASS } from "@/components/ui/fieldStyles";

/**
 * ⚠️ The shared control class, not a bespoke one.
 *
 * This screen defined its own `INPUT` ending in `focus:border-primary` — which is an UNREGISTERED
 * palette utility that emits nothing, so every field here had no focus indicator at all. The
 * shared class gets its focus border from `.input-focus:focus`, which IS hand-written in
 * theme.css, and it also matches the height, radius and placeholder colour of every other input in
 * the app.
 */
const INPUT = FORM_INPUT_CLASS;

/**
 * The same control, sized to its content.
 *
 * ⚠️ Built by REPLACING `w-full`, not by appending `w-auto`. Tailwind resolves conflicting
 * utilities by their order in the generated stylesheet, not by their order in the class attribute,
 * so appending loses and the select stretches onto a line of its own.
 */
const SELECT = `${FORM_INPUT_CLASS.replace("w-full", "w-auto")} min-w-[9rem]`;

/** HC219 — the tenant's exit-interview questionnaire; interviews snapshot it at launch. */
function ExitQuestionnaire() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [questions, setQuestions] = useState<ExitQuestionModel[]>([]);
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState<{ ok: boolean; text: string } | null>(null);

  const { data, isLoading } = useQuery({ queryKey: ["exitQuestionnaire"], queryFn: getExitQuestionnaire, retry: false });

  useEffect(() => {
    if (data?.questions) setQuestions(data.questions.map((q) => ({ ...q, options: q.options ?? [] })));
  }, [data]);

  const set = (i: number, patch: Partial<ExitQuestionModel>) =>
    setQuestions((p) => p.map((q, j) => (j === i ? { ...q, ...patch } : q)));
  const move = (i: number, dir: -1 | 1) =>
    setQuestions((p) => {
      const next = [...p];
      const j = i + dir;
      if (j < 0 || j >= next.length) return p;
      [next[i], next[j]] = [next[j], next[i]];
      return next;
    });

  const save = async () => {
    setBusy(true);
    setMsg(null);
    const res = await saveExitQuestionnaire(questions.map((q, i) => ({
      key: `q${i + 1}`,
      text: q.text,
      type: q.type ?? "Rating",
      options: q.type === "Choice" ? (q.options ?? []).filter((o) => o.trim()) : [],
      required: q.required !== false,
    })));
    setBusy(false);
    setMsg({ ok: res.ok, text: res.ok ? t("Questionnaire saved — new interviews will use it.") : res.message });
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["exitQuestionnaire"] });
  };

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><ClipboardList className="h-5 w-5" /></span>
          <div>
            <h1 className="text-base font-semibold text-foreground">{t("Exit Interview Questionnaire")}</h1>
            <p className="text-xs text-muted">{t("Launched interviews snapshot these questions — later edits never rewrite past interviews.")}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {/* ⚠️ hover:bg-secondary/40 is unregistered — this button had no hover at all. */}
          <button type="button" onClick={() => setQuestions((p) => [...p, { text: "", type: "Rating", options: [], required: true }])}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-sm font-semibold text-foreground transition-opacity hover:opacity-70">
            <Plus size={14} /> {t("Add Question")}
          </button>
          <button type="button" disabled={busy || questions.length === 0 || questions.some((q) => !q.text?.trim())} onClick={save}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
            <Save size={14} /> {busy ? t("Saving…") : t("Save Questionnaire")}
          </button>
        </div>
      </div>

      {/* ⚠️ A SUCCESS used to render in neutral grey (bg-secondary/20 text-muted) while only a
          failure was coloured, so "saved" looked like a passing remark rather than a confirmation.
          Both outcomes are now stated in their own colour, using the registered /20 border widths. */}
      {msg && (
        <p
          className={`mb-2 rounded-lg border px-3 py-2 text-xs ${
            msg.ok
              ? "border-success/20 bg-success/15 text-success"
              : "border-error/20 bg-error/15 text-error"
          }`}
        >
          {msg.text}
        </p>
      )}

      {isLoading ? (
        <Loading />
      ) : questions.length === 0 ? (
        <EmptyState
          icon={<ClipboardList className="h-6 w-6" aria-hidden />}
          title={t("No questions yet")}
          description={t(
            "Add the questions a leaver is asked at their exit interview. They are snapshotted when an interview is launched, so later edits never rewrite past interviews.",
          )}
        />
      ) : (
        <div className="min-h-0 flex-1 space-y-2 overflow-auto">
          {questions.map((q, i) => (
            <div key={i} className="rounded-lg border border-border bg-card p-3">
              <div className="flex flex-wrap items-end gap-2">
                <span className="pb-2 text-xs font-bold text-muted">{i + 1}.</span>
                <div className="min-w-[220px] flex-1">
                  <input type="text" className={INPUT} placeholder={t("Question text")} value={q.text ?? ""} onChange={(e) => set(i, { text: e.target.value })} />
                </div>
                {/* ⚠️ A native select is kept deliberately: three fixed options in a dense repeating
                    row is exactly what it is for, and the app's DropDownField is a searchable popup
                    that would be heavier and worse here. It carries the SHARED control class so it
                    matches every other field — minus `appearance-none`, which would strip the
                    native arrow and leave no indication it opens. */}
                <select className={SELECT} value={q.type ?? "Rating"}
                  onChange={(e) => set(i, { type: e.target.value })}>
                  <option value="Rating">{t("Rating (1–5)")}</option>
                  <option value="Choice">{t("Choice")}</option>
                  <option value="Text">{t("Free text")}</option>
                </select>
                <label className="flex items-center gap-1 pb-2 text-xs text-muted">
                  <input type="checkbox" checked={q.required !== false} onChange={(e) => set(i, { required: e.target.checked })} />
                  {t("Required")}
                </label>
                {/* ⚠️ All three used `hover:text-*` palette variants that are unregistered, so
                    they were permanently grey and never responded to the pointer. They carry their
                    colour outright now and dim on hover. `disabled` on the ends also makes it
                    visible that the first row cannot move up and the last cannot move down. */}
                <span className="flex items-center gap-1 pb-1">
                  <button type="button" title={t("Move up") ?? ""} disabled={i === 0} onClick={() => move(i, -1)}
                    className="rounded p-1 text-primary transition-opacity hover:opacity-70 disabled:cursor-not-allowed disabled:opacity-30"><ArrowUp size={13} /></button>
                  <button type="button" title={t("Move down") ?? ""} disabled={i === questions.length - 1} onClick={() => move(i, 1)}
                    className="rounded p-1 text-primary transition-opacity hover:opacity-70 disabled:cursor-not-allowed disabled:opacity-30"><ArrowDown size={13} /></button>
                  <button type="button" title={t("Remove question") ?? ""} onClick={() => setQuestions((p) => p.filter((_q, j) => j !== i))}
                    className="rounded p-1 text-error transition-opacity hover:opacity-70"><Trash2 size={13} /></button>
                </span>
              </div>
              {q.type === "Choice" && (
                <div className="mt-2">
                  <input type="text" className={INPUT} placeholder={t("Options, comma-separated (e.g. Compensation, Growth, Management)")}
                    value={(q.options ?? []).join(", ")}
                    onChange={(e) => set(i, { options: e.target.value.split(",").map((s) => s.trim()) })} />
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default memo(ExitQuestionnaire);
