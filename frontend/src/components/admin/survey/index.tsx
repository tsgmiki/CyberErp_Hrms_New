"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { ClipboardList, Plus, X, Pencil, Trash2, Play, Square, BarChart3, ArrowUp, ArrowDown } from "lucide-react";
import {
  getAllSurveys, getSurveyById, getSurveyResults, saveSurvey, openSurvey, closeSurvey, deleteSurvey,
} from "@/services/admin/engagement";
import type { SurveyModel, SurveyQuestionModel, SurveyResultsModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-secondary/40 text-muted",
  Open: "bg-success/15 text-success",
  Closed: "bg-muted/30 text-muted",
};

const emptyQuestion = (): SurveyQuestionModel => ({ text: "", type: "Rating", required: true, options: [] });
const emptyForm = (): SurveyModel & { questions: SurveyQuestionModel[] } =>
  ({ isPoll: false, isAnonymous: true, questions: [emptyQuestion()] });

/** HC204 — survey/poll builder for HR: questions (rating/choice/text), open/close lifecycle, aggregated results. */
function Survey() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm());
  const [resultsFor, setResultsFor] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const [param] = useState({ ...parameterInitialData, take: 50 });
  const { data, isLoading } = useQuery({
    queryKey: ["surveys", param],
    queryFn: () => getAllSurveys(param),
    placeholderData: keepPreviousData,
  });
  const items = data?.data ?? [];

  const { data: results } = useQuery<SurveyResultsModel>({
    queryKey: ["surveyResults", resultsFor],
    queryFn: () => getSurveyResults(resultsFor!),
    enabled: !!resultsFor,
  });

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["surveys"] });
    queryClient.invalidateQueries({ queryKey: ["surveyFeed"] });
    if (resultsFor) queryClient.invalidateQueries({ queryKey: ["surveyResults", resultsFor] });
  };

  const set = (patch: Partial<SurveyModel>) => setForm((f) => ({ ...f, ...patch }));
  const setQ = (i: number, patch: Partial<SurveyQuestionModel>) =>
    setForm((f) => ({ ...f, questions: f.questions.map((q, j) => (j === i ? { ...q, ...patch } : q)) }));
  const moveQ = (i: number, dir: -1 | 1) =>
    setForm((f) => {
      const qs = [...f.questions];
      const j = i + dir;
      if (j < 0 || j >= qs.length) return f;
      [qs[i], qs[j]] = [qs[j], qs[i]];
      return { ...f, questions: qs };
    });

  // The paged list carries only questionCount — pull the full draft (questions included) for editing.
  const edit = async (row: SurveyModel) => {
    const s = row.id ? await getSurveyById(row.id) : row;
    setForm({
      ...s,
      opensOn: s.opensOn ? String(s.opensOn).slice(0, 10) : "",
      closesOn: s.closesOn ? String(s.closesOn).slice(0, 10) : "",
      questions: (s.questions?.length ? s.questions : [emptyQuestion()]).map((q) => ({ ...q, options: q.options ?? [] })),
    });
    setShowForm(true);
  };

  const submit = async () => {
    setBusy(true);
    const res = await saveSurvey({
      ...form,
      opensOn: form.opensOn || undefined,
      closesOn: form.closesOn || undefined,
      questions: form.questions.map((q) => ({
        ...q,
        options: q.type === "Choice" ? (q.options ?? []).filter((o) => o.trim()) : undefined,
      })),
    });
    setBusy(false);
    refresh(res.message);
    if (res.ok) {
      setForm(emptyForm());
      setShowForm(false);
    }
  };

  const act = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const res = await fn();
    setBusy(false);
    refresh(res.message);
  };

  const formInvalid =
    !form.title?.trim() ||
    form.questions.length === 0 ||
    form.questions.some((q) => !q.text?.trim() || (q.type === "Choice" && (q.options ?? []).filter((o) => o.trim()).length < 2));

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><ClipboardList className="h-5 w-5" /></span>
          <div>
            <h1 className="text-base font-semibold text-foreground">{t("Survey Builder")}</h1>
            <p className="text-xs text-muted">{t("Design surveys and quick polls, open them to employees, and read aggregated results.")}</p>
          </div>
        </div>
        <button type="button" onClick={() => { setForm(emptyForm()); setShowForm((v) => !v); }}
          className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90">
          {showForm ? <X size={14} /> : <Plus size={14} />} {showForm ? t("Cancel") : t("New Survey")}
        </button>
      </div>

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

      {showForm && (
        <div className="mb-3 overflow-auto rounded-lg border border-border bg-card p-4">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-4">
            <div className="md:col-span-2">
              <label className={LABEL}>{t("Title")} *</label>
              <input type="text" className={INPUT} value={form.title ?? ""} onChange={(e) => set({ title: e.target.value })} />
            </div>
            <div>
              <label className={LABEL}>{t("Opens on")}</label>
              <input type="date" className={INPUT} value={form.opensOn ?? ""} onChange={(e) => set({ opensOn: e.target.value })} />
            </div>
            <div>
              <label className={LABEL}>{t("Closes on")}</label>
              <input type="date" className={INPUT} value={form.closesOn ?? ""} onChange={(e) => set({ closesOn: e.target.value })} />
            </div>
            <div className="md:col-span-2">
              <label className={LABEL}>{t("Description")}</label>
              <textarea className={INPUT} rows={2} value={form.description ?? ""} onChange={(e) => set({ description: e.target.value })} />
            </div>
            <div className="flex items-end gap-4 pb-2 text-xs text-muted md:col-span-2">
              <label className="flex items-center gap-2">
                <input type="checkbox" checked={!!form.isPoll} onChange={(e) => set({ isPoll: e.target.checked })} />
                {t("Quick poll")}
              </label>
              <label className="flex items-center gap-2">
                <input type="checkbox" checked={!!form.isAnonymous} onChange={(e) => set({ isAnonymous: e.target.checked })} />
                {t("Anonymous responses")}
              </label>
            </div>
          </div>

          <h3 className="mb-2 mt-4 text-xs font-semibold uppercase tracking-wide text-muted">{t("Questions")}</h3>
          <div className="space-y-3">
            {form.questions.map((q, i) => (
              <div key={i} className="rounded-md border border-border/70 bg-background p-3">
                <div className="grid grid-cols-1 gap-2 md:grid-cols-6">
                  <div className="md:col-span-4">
                    <label className={LABEL}>{t("Question")} {i + 1} *</label>
                    <input type="text" className={INPUT} value={q.text ?? ""} onChange={(e) => setQ(i, { text: e.target.value })} />
                  </div>
                  <div>
                    <label className={LABEL}>{t("Type")}</label>
                    <select className={INPUT} value={q.type}
                      onChange={(e) => setQ(i, { type: e.target.value, options: e.target.value === "Choice" ? (q.options?.length ? q.options : ["", ""]) : [] })}>
                      <option value="Rating">{t("Rating (1–5)")}</option>
                      <option value="Choice">{t("Multiple choice")}</option>
                      <option value="Text">{t("Free text")}</option>
                    </select>
                  </div>
                  <div className="flex items-end justify-end gap-1 pb-1">
                    <label className="mr-2 flex items-center gap-1.5 text-xs text-muted">
                      <input type="checkbox" checked={!!q.required} onChange={(e) => setQ(i, { required: e.target.checked })} />
                      {t("Required")}
                    </label>
                    <button type="button" title={t("Move up")} onClick={() => moveQ(i, -1)} className="rounded p-1 text-muted hover:text-primary"><ArrowUp size={14} /></button>
                    <button type="button" title={t("Move down")} onClick={() => moveQ(i, 1)} className="rounded p-1 text-muted hover:text-primary"><ArrowDown size={14} /></button>
                    <button type="button" title={t("Remove")} disabled={form.questions.length === 1}
                      onClick={() => setForm((f) => ({ ...f, questions: f.questions.filter((_, j) => j !== i) }))}
                      className="rounded p-1 text-muted hover:text-error disabled:opacity-40"><Trash2 size={14} /></button>
                  </div>
                </div>
                {q.type === "Choice" && (
                  <div className="mt-2 space-y-1.5">
                    {(q.options ?? []).map((o, oi) => (
                      <div key={oi} className="flex items-center gap-2">
                        <input type="text" className={INPUT} placeholder={`${t("Option")} ${oi + 1}`} value={o}
                          onChange={(e) => setQ(i, { options: (q.options ?? []).map((x, xi) => (xi === oi ? e.target.value : x)) })} />
                        <button type="button" disabled={(q.options?.length ?? 0) <= 2}
                          onClick={() => setQ(i, { options: (q.options ?? []).filter((_, xi) => xi !== oi) })}
                          className="rounded p-1 text-muted hover:text-error disabled:opacity-40"><X size={14} /></button>
                      </div>
                    ))}
                    <button type="button" onClick={() => setQ(i, { options: [...(q.options ?? []), ""] })}
                      className="text-xs font-medium text-primary hover:underline">+ {t("Add option")}</button>
                  </div>
                )}
              </div>
            ))}
          </div>
          <div className="mt-3 flex justify-between">
            <button type="button" onClick={() => setForm((f) => ({ ...f, questions: [...f.questions, emptyQuestion()] }))}
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary/40">
              <Plus size={14} /> {t("Add Question")}
            </button>
            <button type="button" disabled={busy || formInvalid} onClick={submit}
              className="rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              {busy ? t("Saving…") : form.id ? t("Update Draft") : t("Save Draft")}
            </button>
          </div>
        </div>
      )}

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">{t("No surveys yet.")}</p>
      ) : (
        <div className="min-h-0 flex-1 overflow-auto rounded-lg border border-border bg-card">
          <table className="w-full text-sm">
            <thead className="sticky top-0 bg-secondary/30 text-left text-xs uppercase tracking-wide text-muted">
              <tr>
                <th className="px-3 py-2">{t("Title")}</th>
                <th className="px-3 py-2">{t("Kind")}</th>
                <th className="px-3 py-2">{t("Questions")}</th>
                <th className="px-3 py-2">{t("Responses")}</th>
                <th className="px-3 py-2">{t("Status")}</th>
                <th className="px-3 py-2 text-right">{t("Actions")}</th>
              </tr>
            </thead>
            <tbody>
              {items.map((s) => (
                <tr key={s.id} className="border-t border-border/60 hover:bg-secondary/20">
                  <td className="px-3 py-2 font-medium text-foreground">{s.title}</td>
                  <td className="px-3 py-2 text-muted">{s.isPoll ? t("Poll") : t("Survey")}{s.isAnonymous ? ` · ${t("anonymous")}` : ""}</td>
                  <td className="px-3 py-2 text-muted">{s.questionCount}</td>
                  <td className="px-3 py-2 text-muted">{s.responseCount}</td>
                  <td className="px-3 py-2"><span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[s.status ?? ""] ?? ""}`}>{s.status}</span></td>
                  <td className="px-3 py-2 text-right">
                    {s.status === "Draft" && (
                      <>
                        <button type="button" title={t("Edit")} onClick={() => edit(s)} className="rounded p-1 text-muted hover:text-primary"><Pencil size={14} /></button>
                        <button type="button" title={t("Open to employees")} onClick={() => s.id && act(() => openSurvey(s.id!))}
                          className="rounded p-1 text-muted hover:text-success"><Play size={14} /></button>
                      </>
                    )}
                    {s.status === "Open" && (
                      <button type="button" title={t("Close")} onClick={() => s.id && act(() => closeSurvey(s.id!))}
                        className="rounded p-1 text-muted hover:text-warning"><Square size={14} /></button>
                    )}
                    <button type="button" title={t("Results")} onClick={() => setResultsFor(s.id ?? null)}
                      className="rounded p-1 text-muted hover:text-primary"><BarChart3 size={14} /></button>
                    {s.status === "Draft" && (
                      <button type="button" title={t("Delete")} onClick={() => s.id && act(() => deleteSurvey(s.id!))}
                        className="rounded p-1 text-muted hover:text-error"><Trash2 size={14} /></button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {resultsFor && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="flex max-h-[90vh] w-full max-w-2xl flex-col rounded-lg border border-border bg-background shadow-xl">
            <div className="flex items-center justify-between border-b border-border px-4 py-3">
              <div>
                <h3 className="text-sm font-semibold text-foreground">{results?.title ?? t("Results")}</h3>
                {results && (
                  <p className="text-xs text-muted">
                    {t("{{n}} responses · {{p}}% completion", { n: results.responseCount, p: results.completionRatePercent })}
                    {results.isAnonymous ? ` · ${t("anonymous")}` : ""}
                  </p>
                )}
              </div>
              <button type="button" onClick={() => setResultsFor(null)} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
            </div>
            <div className="min-h-0 flex-1 space-y-4 overflow-auto p-4">
              {!results ? (
                <Loading />
              ) : (
                results.questions.map((q) => {
                  const total = Object.values(q.counts ?? {}).reduce((a, b) => a + b, 0);
                  return (
                    <div key={q.key} className="rounded-md border border-border/70 bg-card p-3">
                      <p className="mb-1 text-sm font-medium text-foreground">{q.text}</p>
                      <p className="mb-2 text-xs text-muted">
                        {q.answered} {t("answered")}{q.type === "Rating" && q.average != null ? ` · ${t("average")} ${q.average.toFixed(2)} / 5` : ""}
                      </p>
                      {q.type !== "Text" && (
                        <div className="space-y-1.5">
                          {Object.entries(q.counts ?? {}).map(([opt, n]) => (
                            <div key={opt} className="flex items-center gap-2">
                              <span className="w-24 shrink-0 truncate text-xs text-muted">{opt}</span>
                              <div className="h-2 flex-1 overflow-hidden rounded bg-secondary/40">
                                <div className="h-full rounded bg-primary" style={{ width: total ? `${(n / total) * 100}%` : 0 }} />
                              </div>
                              <span className="w-8 shrink-0 text-right text-xs tabular-nums text-foreground">{n}</span>
                            </div>
                          ))}
                        </div>
                      )}
                      {q.type === "Text" && (
                        <ul className="max-h-40 space-y-1 overflow-auto">
                          {(q.textAnswers ?? []).map((a, i) => (
                            <li key={i} className="rounded bg-secondary/20 px-2 py-1 text-xs text-foreground">{a}</li>
                          ))}
                          {(q.textAnswers?.length ?? 0) === 0 && <li className="text-xs text-muted">{t("No text answers.")}</li>}
                        </ul>
                      )}
                    </div>
                  );
                })
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(Survey);
