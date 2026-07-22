"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { Lightbulb, Plus, X, Send, Trash2, MessageSquareReply } from "lucide-react";
import { getAllSuggestions, submitSuggestion, respondSuggestion, deleteSuggestion } from "@/services/admin/engagement";
import type { SuggestionModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const TONE: Record<string, string> = {
  New: "bg-info/15 text-info",
  UnderReview: "bg-warning/15 text-warning",
  Actioned: "bg-success/15 text-success",
  Closed: "bg-muted/30 text-muted",
};

/** HC203/HC207 — the suggestion box: submit (named or anonymous), track the management response. */
function Suggestion() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [isAnonymous, setIsAnonymous] = useState(false);
  const [respondFor, setRespondFor] = useState<SuggestionModel | null>(null);
  const [respStatus, setRespStatus] = useState("Actioned");
  const [respText, setRespText] = useState("");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const [param] = useState({ ...parameterInitialData, take: 50 });
  const { data, isLoading } = useQuery({
    queryKey: ["suggestions", param],
    queryFn: () => getAllSuggestions(param),
    placeholderData: keepPreviousData,
  });
  const items = data?.data ?? [];

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["suggestions"] });
  };

  const submit = async () => {
    setBusy(true);
    const res = await submitSuggestion(title.trim(), body.trim(), isAnonymous);
    setBusy(false);
    refresh(res.ok
      ? (isAnonymous
        ? t("Submitted anonymously — nothing links it to you, so it will not appear in your list.")
        : t("Suggestion submitted — you will see the management response here."))
      : res.message);
    if (res.ok) {
      setTitle("");
      setBody("");
      setIsAnonymous(false);
      setShowForm(false);
    }
  };

  const confirmRespond = async () => {
    if (!respondFor?.id) return;
    setBusy(true);
    const res = await respondSuggestion(respondFor.id, respStatus, respText || undefined);
    setBusy(false);
    if (res.ok) {
      setRespondFor(null);
      setRespText("");
    }
    refresh(res.message);
  };

  // HR actions are server-gated (non-admins get a polite 400) — same pattern as other modules.

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Lightbulb className="h-5 w-5" /></span>
          <div>
            <h1 className="text-base font-semibold text-foreground">{t("Suggestions")}</h1>
            <p className="text-xs text-muted">{t("Ideas and feedback to management — submit openly or anonymously.")}</p>
          </div>
        </div>
        <button type="button" onClick={() => setShowForm((v) => !v)}
          className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90">
          {showForm ? <X size={14} /> : <Plus size={14} />} {showForm ? t("Cancel") : t("New Suggestion")}
        </button>
      </div>

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

      {showForm && (
        <div className="mb-3 rounded-lg border border-border bg-card p-4">
          <div className="grid grid-cols-1 gap-3">
            <div>
              <label className={LABEL}>{t("Title")} *</label>
              <input type="text" className={INPUT} value={title} onChange={(e) => setTitle(e.target.value)} />
            </div>
            <div>
              <label className={LABEL}>{t("Your suggestion")} *</label>
              <textarea className={INPUT} rows={3} value={body} onChange={(e) => setBody(e.target.value)} />
            </div>
            <label className="flex items-center gap-2 text-xs text-muted">
              <input type="checkbox" checked={isAnonymous} onChange={(e) => setIsAnonymous(e.target.checked)} />
              {t("Submit anonymously — no name, no account stamp, not even in the audit trail.")}
            </label>
          </div>
          <div className="mt-3 flex justify-end">
            <button type="button" disabled={busy || !title.trim() || !body.trim()} onClick={submit}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <Send size={14} /> {busy ? t("Submitting…") : t("Submit")}
            </button>
          </div>
        </div>
      )}

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("No suggestions to show — anonymous submissions never appear in the author's own list.")}
        </p>
      ) : (
        <div className="min-h-0 flex-1 space-y-3 overflow-auto">
          {items.map((s) => (
            <div key={s.id} className="rounded-lg border border-border bg-card p-4">
              <div className="mb-1 flex items-start justify-between gap-2">
                <div className="min-w-0">
                  <p className="truncate text-sm font-semibold text-foreground">{s.title}</p>
                  <p className="text-xs text-muted">
                    {s.employeeName}{s.submittedOn ? ` · ${String(s.submittedOn).slice(0, 10)}` : ""}
                  </p>
                </div>
                <span className="flex shrink-0 items-center gap-1.5">
                  <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${TONE[s.status ?? ""] ?? ""}`}>{s.status}</span>
                  <button type="button" title={t("Respond (HR)")} onClick={() => { setRespondFor(s); setRespStatus("Actioned"); setRespText(s.managementResponse ?? ""); }}
                    className="rounded p-1 text-muted hover:text-primary"><MessageSquareReply size={14} /></button>
                  <button type="button" title={t("Delete (HR)")} onClick={() => s.id && deleteSuggestion(s.id).then((r) => refresh(r.message))}
                    className="rounded p-1 text-muted hover:text-error"><Trash2 size={14} /></button>
                </span>
              </div>
              <p className="whitespace-pre-wrap text-sm text-foreground">{s.body}</p>
              {s.managementResponse && (
                <div className="mt-2 rounded-md border border-border/70 bg-secondary/10 px-3 py-2">
                  <p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Management response")}</p>
                  <p className="text-sm text-foreground">{s.managementResponse}</p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {respondFor && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md rounded-lg border border-border bg-background p-4 shadow-xl">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-foreground">{t("Respond to suggestion")}</h3>
              <button type="button" onClick={() => setRespondFor(null)} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
            </div>
            <p className="mb-3 text-xs text-muted">{respondFor.title}</p>
            <label className={LABEL}>{t("New status")}</label>
            <select className={INPUT} value={respStatus} onChange={(e) => setRespStatus(e.target.value)}>
              <option value="UnderReview">{t("Under review")}</option>
              <option value="Actioned">{t("Actioned")}</option>
              <option value="Closed">{t("Closed")}</option>
            </select>
            <label className={`${LABEL} mt-3`}>{t("Response")}</label>
            <textarea className={INPUT} rows={3} value={respText} onChange={(e) => setRespText(e.target.value)} />
            <div className="mt-4 flex justify-end gap-2">
              <button type="button" onClick={() => setRespondFor(null)} className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary/40">{t("Cancel")}</button>
              <button type="button" disabled={busy} onClick={confirmRespond}
                className="rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                {busy ? t("Saving…") : t("Save Response")}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(Suggestion);
