"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { Vote, X, Send, CheckCircle2, Star } from "lucide-react";
import { getSurveyFeed, respondSurvey } from "@/services/admin/engagement";
import type { SurveyModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

/** HC204 — employee side: open surveys & polls addressed to me; answer once, anonymously when so configured. */
function SurveyTake() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [taking, setTaking] = useState<SurveyModel | null>(null);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const [param] = useState({ ...parameterInitialData, take: 50 });
  const { data, isLoading } = useQuery({
    queryKey: ["surveyFeed", param],
    queryFn: () => getSurveyFeed(param),
    placeholderData: keepPreviousData,
  });
  const items = data?.data ?? [];

  const start = (s: SurveyModel) => {
    setTaking(s);
    setAnswers({});
  };

  const missingRequired = (taking?.questions ?? []).some((q) => q.required && !(answers[q.key ?? ""] ?? "").trim());

  const submit = async () => {
    if (!taking?.id) return;
    setBusy(true);
    const filled = Object.fromEntries(Object.entries(answers).filter(([, v]) => v.trim() !== ""));
    const res = await respondSurvey(taking.id, filled);
    setBusy(false);
    setMsg(res.ok
      ? (taking.isAnonymous
        ? t("Response recorded anonymously — your answers are not linked to your name.")
        : t("Response recorded — thank you."))
      : res.message);
    if (res.ok) setTaking(null);
    queryClient.invalidateQueries({ queryKey: ["surveyFeed"] });
  };

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Vote className="h-5 w-5" /></span>
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("Surveys & Polls")}</h1>
          <p className="text-xs text-muted">{t("Open questionnaires waiting for your voice — each can be answered once.")}</p>
        </div>
      </div>

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">{t("No open surveys or polls right now.")}</p>
      ) : (
        <div className="grid min-h-0 flex-1 auto-rows-min grid-cols-1 gap-3 overflow-auto md:grid-cols-2 lg:grid-cols-3">
          {items.map((s) => (
            <div key={s.id} className="flex flex-col rounded-lg border border-border bg-card p-4">
              <div className="mb-1 flex items-start justify-between gap-2">
                <p className="text-sm font-semibold text-foreground">{s.title}</p>
                <span className="shrink-0 rounded-full bg-secondary/40 px-2 py-0.5 text-[11px] font-semibold text-muted">
                  {s.isPoll ? t("Poll") : t("Survey")}
                </span>
              </div>
              {s.description && <p className="mb-2 text-xs text-muted">{s.description}</p>}
              <p className="mb-3 text-xs text-muted">
                {t("{{n}} question(s)", { n: s.questionCount })}{s.isAnonymous ? ` · ${t("anonymous")}` : ""}
                {s.closesOn ? ` · ${t("closes")} ${String(s.closesOn).slice(0, 10)}` : ""}
              </p>
              <div className="mt-auto">
                {s.hasResponded ? (
                  <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-success">
                    <CheckCircle2 size={14} /> {t("Responded")}
                  </span>
                ) : (
                  <button type="button" onClick={() => start(s)}
                    className="rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90">
                    {s.isPoll ? t("Vote") : t("Take Survey")}
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {taking && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="flex max-h-[90vh] w-full max-w-lg flex-col rounded-lg border border-border bg-background shadow-xl">
            <div className="flex items-center justify-between border-b border-border px-4 py-3">
              <div>
                <h3 className="text-sm font-semibold text-foreground">{taking.title}</h3>
                <p className="text-xs text-muted">{taking.isAnonymous ? t("Anonymous — answers are stored without your identity.") : t("Your name is attached to this response.")}</p>
              </div>
              <button type="button" onClick={() => setTaking(null)} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
            </div>
            <div className="min-h-0 flex-1 space-y-4 overflow-auto p-4">
              {(taking.questions ?? []).map((q, i) => {
                const key = q.key ?? "";
                const val = answers[key] ?? "";
                return (
                  <div key={key}>
                    <p className="mb-1.5 text-sm font-medium text-foreground">
                      {i + 1}. {q.text}{q.required && <span className="text-error"> *</span>}
                    </p>
                    {q.type === "Rating" && (
                      <div className="flex gap-1">
                        {[1, 2, 3, 4, 5].map((n) => (
                          <button key={n} type="button" onClick={() => setAnswers((a) => ({ ...a, [key]: String(n) }))}
                            className={`rounded p-1 ${Number(val) >= n ? "text-warning" : "text-muted/50 hover:text-muted"}`}>
                            <Star size={22} fill={Number(val) >= n ? "currentColor" : "none"} />
                          </button>
                        ))}
                      </div>
                    )}
                    {q.type === "Choice" && (
                      <div className="space-y-1.5">
                        {(q.options ?? []).map((o) => (
                          <label key={o} className="flex items-center gap-2 text-sm text-foreground">
                            <input type="radio" name={key} checked={val === o} onChange={() => setAnswers((a) => ({ ...a, [key]: o }))} />
                            {o}
                          </label>
                        ))}
                      </div>
                    )}
                    {q.type === "Text" && (
                      <textarea className={INPUT} rows={2} value={val}
                        onChange={(e) => setAnswers((a) => ({ ...a, [key]: e.target.value }))} />
                    )}
                  </div>
                );
              })}
            </div>
            <div className="flex justify-end gap-2 border-t border-border px-4 py-3">
              <button type="button" onClick={() => setTaking(null)} className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary/40">{t("Cancel")}</button>
              <button type="button" disabled={busy || missingRequired} onClick={submit}
                className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <Send size={14} /> {busy ? t("Submitting…") : t("Submit")}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(SurveyTake);
