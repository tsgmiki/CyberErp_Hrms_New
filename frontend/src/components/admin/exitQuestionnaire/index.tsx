"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ClipboardList, Plus, Trash2, ArrowUp, ArrowDown, Save } from "lucide-react";
import { getExitQuestionnaire, saveExitQuestionnaire } from "@/services/admin/employee/exitManagement";
import type { ExitQuestionModel } from "@/models";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

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
          <button type="button" onClick={() => setQuestions((p) => [...p, { text: "", type: "Rating", options: [], required: true }])}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-sm font-semibold text-foreground hover:bg-secondary/40">
            <Plus size={14} /> {t("Add Question")}
          </button>
          <button type="button" disabled={busy || questions.length === 0 || questions.some((q) => !q.text?.trim())} onClick={save}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
            <Save size={14} /> {busy ? t("Saving…") : t("Save Questionnaire")}
          </button>
        </div>
      </div>

      {msg && <p className={`mb-2 rounded-lg border border-border px-3 py-2 text-xs ${msg.ok ? "bg-secondary/20 text-muted" : "bg-error/10 text-error"}`}>{msg.text}</p>}

      {isLoading ? (
        <Loading />
      ) : questions.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("No questions yet — add the first one.")}
        </p>
      ) : (
        <div className="min-h-0 flex-1 space-y-2 overflow-auto">
          {questions.map((q, i) => (
            <div key={i} className="rounded-lg border border-border bg-card p-3">
              <div className="flex flex-wrap items-end gap-2">
                <span className="pb-2 text-xs font-bold text-muted">{i + 1}.</span>
                <div className="min-w-[220px] flex-1">
                  <input type="text" className={INPUT} placeholder={t("Question text")} value={q.text ?? ""} onChange={(e) => set(i, { text: e.target.value })} />
                </div>
                <select className="rounded-md border border-border bg-card px-2 py-1.5 text-sm text-foreground" value={q.type ?? "Rating"}
                  onChange={(e) => set(i, { type: e.target.value })}>
                  <option value="Rating">{t("Rating (1–5)")}</option>
                  <option value="Choice">{t("Choice")}</option>
                  <option value="Text">{t("Free text")}</option>
                </select>
                <label className="flex items-center gap-1 pb-2 text-xs text-muted">
                  <input type="checkbox" checked={q.required !== false} onChange={(e) => set(i, { required: e.target.checked })} />
                  {t("Required")}
                </label>
                <span className="flex items-center gap-1 pb-1">
                  <button type="button" onClick={() => move(i, -1)} className="rounded p-1 text-muted hover:text-primary"><ArrowUp size={13} /></button>
                  <button type="button" onClick={() => move(i, 1)} className="rounded p-1 text-muted hover:text-primary"><ArrowDown size={13} /></button>
                  <button type="button" onClick={() => setQuestions((p) => p.filter((_q, j) => j !== i))} className="rounded p-1 text-muted hover:text-error"><Trash2 size={13} /></button>
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
