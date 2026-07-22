"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { ShieldAlert, Plus, X, Send, UserCheck, CheckCircle2, Lock, MessageSquarePlus } from "lucide-react";
import {
  getAllGrievances, getGrievance, submitGrievance, assignGrievance,
  resolveGrievance, closeGrievance, addGrievanceNote,
} from "@/services/admin/engagement";
import type { GrievanceModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import EmployeePicker from "@/components/common/employeePicker";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const STATUS_TONE: Record<string, string> = {
  Submitted: "bg-info/15 text-info",
  UnderReview: "bg-warning/15 text-warning",
  Resolved: "bg-success/15 text-success",
  Closed: "bg-muted/30 text-muted",
};
const SEVERITY_TONE: Record<string, string> = {
  Low: "bg-secondary/40 text-muted",
  Medium: "bg-info/15 text-info",
  High: "bg-warning/15 text-warning",
  Critical: "bg-error/15 text-error",
};

const CATEGORIES = ["Workplace", "Compensation", "Harassment", "Discrimination", "Management", "Safety", "Other"];

const emptyForm: GrievanceModel = { category: "Workplace", severity: "Medium", isConfidential: false };

/** HC205 — grievances: submit, assign a handler, keep a progress-note trail, resolve, close. */
function Grievance() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<GrievanceModel>({ ...emptyForm });
  const [openId, setOpenId] = useState<string | null>(null);
  const [assignee, setAssignee] = useState<{ id: string; name: string } | null>(null);
  const [noteText, setNoteText] = useState("");
  const [resolutionText, setResolutionText] = useState("");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const [param] = useState({ ...parameterInitialData, take: 50 });
  const { data, isLoading } = useQuery({
    queryKey: ["grievances", param],
    queryFn: () => getAllGrievances(param),
    placeholderData: keepPreviousData,
  });
  const items = data?.data ?? [];

  const { data: detail } = useQuery({
    queryKey: ["grievance", openId],
    queryFn: () => getGrievance(openId!),
    enabled: !!openId,
  });

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["grievances"] });
    if (openId) queryClient.invalidateQueries({ queryKey: ["grievance", openId] });
  };

  const set = (patch: Partial<GrievanceModel>) => setForm((f) => ({ ...f, ...patch }));

  const submit = async () => {
    setBusy(true);
    const res = await submitGrievance(form);
    setBusy(false);
    refresh(res.ok ? t("Grievance submitted — HR has been notified and it now appears in your list.") : res.message);
    if (res.ok) {
      setForm({ ...emptyForm });
      setShowForm(false);
    }
  };

  const act = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const res = await fn();
    setBusy(false);
    refresh(res.message);
    return res.ok;
  };

  const g = detail;

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><ShieldAlert className="h-5 w-5" /></span>
          <div>
            <h1 className="text-base font-semibold text-foreground">{t("Grievances")}</h1>
            <p className="text-xs text-muted">{t("Raise a workplace issue and follow its handling to resolution.")}</p>
          </div>
        </div>
        <button type="button" onClick={() => setShowForm((v) => !v)}
          className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90">
          {showForm ? <X size={14} /> : <Plus size={14} />} {showForm ? t("Cancel") : t("New Grievance")}
        </button>
      </div>

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

      {showForm && (
        <div className="mb-3 rounded-lg border border-border bg-card p-4">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-3">
            <div>
              <label className={LABEL}>{t("Category")} *</label>
              <select className={INPUT} value={form.category} onChange={(e) => set({ category: e.target.value })}>
                {CATEGORIES.map((c) => <option key={c} value={c}>{t(c)}</option>)}
              </select>
            </div>
            <div>
              <label className={LABEL}>{t("Severity")} *</label>
              <select className={INPUT} value={form.severity} onChange={(e) => set({ severity: e.target.value })}>
                {["Low", "Medium", "High", "Critical"].map((s) => <option key={s} value={s}>{t(s)}</option>)}
              </select>
            </div>
            <label className="flex items-end gap-2 pb-2 text-xs text-muted">
              <input type="checkbox" checked={!!form.isConfidential} onChange={(e) => set({ isConfidential: e.target.checked })} />
              {t("Confidential — visible only to HR and the assigned handler")}
            </label>
            <div className="md:col-span-3">
              <label className={LABEL}>{t("Subject")} *</label>
              <input type="text" className={INPUT} value={form.subject ?? ""} onChange={(e) => set({ subject: e.target.value })} />
            </div>
            <div className="md:col-span-3">
              <label className={LABEL}>{t("Details")} *</label>
              <textarea className={INPUT} rows={4} value={form.details ?? ""} onChange={(e) => set({ details: e.target.value })} />
            </div>
          </div>
          <div className="mt-3 flex justify-end">
            <button type="button" disabled={busy || !form.subject?.trim() || !form.details?.trim()} onClick={submit}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <Send size={14} /> {busy ? t("Submitting…") : t("Submit Grievance")}
            </button>
          </div>
        </div>
      )}

      {isLoading ? (
        <Loading />
      ) : items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">{t("No grievances to show.")}</p>
      ) : (
        <div className="min-h-0 flex-1 overflow-auto rounded-lg border border-border bg-card">
          <table className="w-full text-sm">
            <thead className="sticky top-0 bg-secondary/30 text-left text-xs uppercase tracking-wide text-muted">
              <tr>
                <th className="px-3 py-2">{t("Subject")}</th>
                <th className="px-3 py-2">{t("Employee")}</th>
                <th className="px-3 py-2">{t("Category")}</th>
                <th className="px-3 py-2">{t("Severity")}</th>
                <th className="px-3 py-2">{t("Status")}</th>
                <th className="px-3 py-2">{t("Assigned To")}</th>
                <th className="px-3 py-2">{t("Submitted")}</th>
              </tr>
            </thead>
            <tbody>
              {items.map((x) => (
                <tr key={x.id} className="cursor-pointer border-t border-border/60 hover:bg-secondary/20"
                  onClick={() => { setOpenId(x.id ?? null); setAssignee(null); setNoteText(""); setResolutionText(""); }}>
                  <td className="px-3 py-2 font-medium text-foreground">
                    <span className="inline-flex items-center gap-1.5">
                      {x.isConfidential && <Lock size={12} className="text-warning" />}{x.subject}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-muted">{x.employeeName}</td>
                  <td className="px-3 py-2 text-muted">{x.category}</td>
                  <td className="px-3 py-2"><span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${SEVERITY_TONE[x.severity ?? ""] ?? ""}`}>{x.severity}</span></td>
                  <td className="px-3 py-2"><span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[x.status ?? ""] ?? ""}`}>{x.status}</span></td>
                  <td className="px-3 py-2 text-muted">{x.assignedToName ?? "—"}</td>
                  <td className="px-3 py-2 text-muted">{x.submittedOn ? String(x.submittedOn).slice(0, 10) : ""}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {openId && g && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="flex max-h-[90vh] w-full max-w-2xl flex-col rounded-lg border border-border bg-background shadow-xl">
            <div className="flex items-center justify-between border-b border-border px-4 py-3">
              <div className="min-w-0">
                <h3 className="truncate text-sm font-semibold text-foreground">
                  {g.isConfidential && <Lock size={12} className="mr-1 inline text-warning" />}{g.subject}
                </h3>
                <p className="text-xs text-muted">
                  {g.employeeName} · {g.category} · {g.submittedOn ? String(g.submittedOn).slice(0, 10) : ""}
                </p>
              </div>
              <span className="flex items-center gap-2">
                <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${SEVERITY_TONE[g.severity ?? ""] ?? ""}`}>{g.severity}</span>
                <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[g.status ?? ""] ?? ""}`}>{g.status}</span>
                <button type="button" onClick={() => setOpenId(null)} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
              </span>
            </div>

            <div className="min-h-0 flex-1 overflow-auto p-4">
              <p className="whitespace-pre-wrap text-sm text-foreground">{g.details}</p>
              {g.assignedToName && <p className="mt-2 text-xs text-muted">{t("Handler")}: <span className="font-medium text-foreground">{g.assignedToName}</span></p>}
              {g.resolution && (
                <div className="mt-3 rounded-md border border-success/40 bg-success/10 px-3 py-2">
                  <p className="text-[11px] font-semibold uppercase tracking-wide text-success">{t("Resolution")}</p>
                  <p className="text-sm text-foreground">{g.resolution}</p>
                </div>
              )}

              <h4 className="mb-2 mt-4 text-xs font-semibold uppercase tracking-wide text-muted">{t("Progress notes")}</h4>
              {(g.notes?.length ?? 0) === 0 ? (
                <p className="text-xs text-muted">{t("No notes yet.")}</p>
              ) : (
                <ol className="relative ml-2 space-y-3 border-l border-border pl-4">
                  {g.notes!.map((n) => (
                    <li key={n.id}>
                      <span className="absolute -left-[5px] mt-1.5 h-2.5 w-2.5 rounded-full border border-background bg-primary" />
                      <p className="text-xs text-muted">{n.authorName} · {n.notedAt ? String(n.notedAt).slice(0, 16).replace("T", " ") : ""}</p>
                      <p className="whitespace-pre-wrap text-sm text-foreground">{n.note}</p>
                    </li>
                  ))}
                </ol>
              )}

              {g.status !== "Closed" && (
                <div className="mt-3 flex items-start gap-2">
                  <textarea className={INPUT} rows={2} placeholder={t("Add a progress note…")} value={noteText} onChange={(e) => setNoteText(e.target.value)} />
                  <button type="button" disabled={busy || !noteText.trim()}
                    onClick={async () => { if (await act(() => addGrievanceNote(g.id!, noteText.trim()))) setNoteText(""); }}
                    className="inline-flex shrink-0 items-center gap-1.5 rounded-md border border-border px-3 py-2 text-sm text-foreground hover:bg-secondary/40 disabled:opacity-50">
                    <MessageSquarePlus size={14} /> {t("Note")}
                  </button>
                </div>
              )}
            </div>

            {g.status !== "Closed" && (
              <div className="space-y-3 border-t border-border px-4 py-3">
                {(g.status === "Submitted" || g.status === "UnderReview") && (
                  <div className="flex items-end gap-2">
                    <div className="flex-1">
                      <label className={LABEL}>{t("Assign handler (HR)")}</label>
                      <EmployeePicker value={assignee?.id} displayValue={assignee?.name}
                        onSelect={(id, name) => setAssignee({ id, name })} placeholder={t("Search employee…")} />
                    </div>
                    <button type="button" disabled={busy || !assignee}
                      onClick={async () => { if (await act(() => assignGrievance(g.id!, assignee!.id))) setAssignee(null); }}
                      className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-2 text-sm text-foreground hover:bg-secondary/40 disabled:opacity-50">
                      <UserCheck size={14} /> {t("Assign")}
                    </button>
                  </div>
                )}
                {g.status !== "Resolved" && (
                  <div className="flex items-end gap-2">
                    <div className="flex-1">
                      <label className={LABEL}>{t("Resolution")}</label>
                      <textarea className={INPUT} rows={2} value={resolutionText} onChange={(e) => setResolutionText(e.target.value)} />
                    </div>
                    <button type="button" disabled={busy || !resolutionText.trim()}
                      onClick={() => act(() => resolveGrievance(g.id!, resolutionText.trim()))}
                      className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                      <CheckCircle2 size={14} /> {t("Resolve")}
                    </button>
                  </div>
                )}
                {g.status === "Resolved" && (
                  <div className="flex justify-end">
                    <button type="button" disabled={busy} onClick={() => act(() => closeGrievance(g.id!))}
                      className="rounded-md border border-border px-3.5 py-2 text-sm font-semibold text-foreground hover:bg-secondary/40 disabled:opacity-50">
                      {t("Close Grievance")}
                    </button>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(Grievance);
