"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { DoorOpen, Send, Hourglass, Ban } from "lucide-react";
import { getCpdSummary } from "@/services/admin/trainingCpd";
import { getTerminations, cancelTermination } from "@/services/admin/employee/termination";
import { getAssetRecoveries } from "@/services/admin/companyAsset";
import { getExitInterview, getSettlement } from "@/services/admin/employee/exitManagement";
import ExitInterviewForm from "../employee/exitInterviewForm";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { EmployeeTerminationModel } from "@/models";
import Loading from "../../common/loader/loader";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const CASE_TONE: Record<string, string> = {
  Initiated: "bg-info/15 text-info",
  ClearanceInProgress: "bg-warning/15 text-warning",
  Settled: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};

const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "—");

/** The employee's own VOLUNTARY exit request (HC209) — the server enforces the matrix. */
async function submitResignation(model: EmployeeTerminationModel): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/EmployeeTermination`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(model),
  });
  const text = await res.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
  if (!res.ok) return { ok: false, message: errorMessageParser(parsed.errors || parsed) };
  return { ok: true, message: "Your resignation has been submitted and routed for approval." };
}

/** Read-only tracker of the employee's own case incl. clearance + asset checklist. */
function CaseTracker({ item, onChanged }: { item: EmployeeTerminationModel; onChanged: (msg: string) => void }) {
  const { t } = useTranslation();
  const [busy, setBusy] = useState(false);
  const { data: recoveries } = useQuery({
    queryKey: ["assetRecoveries", item.id],
    queryFn: () => getAssetRecoveries(item.id!),
    enabled: !!item.id && item.status === "ClearanceInProgress",
  });
  const queryClient = useQueryClient();
  const { data: interview } = useQuery({
    queryKey: ["exitInterview", item.id],
    queryFn: () => getExitInterview(item.id!),
    enabled: !!item.id && item.status === "ClearanceInProgress",
    retry: false,
  });
  const { data: settlement } = useQuery({
    queryKey: ["settlement", item.id],
    queryFn: () => getSettlement(item.id!),
    enabled: !!item.id && item.status === "ClearanceInProgress",
    retry: false,
  });

  const clearances = item.clearances ?? [];
  const cleared = clearances.filter((c) => c.status === "Cleared").length;

  const cancel = async () => {
    if (!item.id) return;
    setBusy(true);
    const res = await cancelTermination(item.id);
    setBusy(false);
    onChanged(res.ok ? t("Your request has been withdrawn.") : res.message);
  };

  return (
    <div className="rounded-lg border border-border bg-card">
      <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border px-4 py-2.5">
        <h3 className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <DoorOpen size={16} className="text-primary" /> {t("My Exit Request")}
          <span className={`rounded px-2 py-0.5 text-xs font-semibold ${CASE_TONE[item.status ?? ""] ?? ""}`}>{t(item.status ?? "")}</span>
          <span className="rounded bg-secondary px-2 py-0.5 text-xs text-foreground">{t(item.terminationType ?? "")}</span>
        </h3>
        {item.status === "Initiated" && !item.awaitingWorkflow && (
          <button type="button" disabled={busy} onClick={cancel}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-error hover:text-error disabled:opacity-50">
            <Ban size={14} /> {t("Withdraw")}
          </button>
        )}
      </div>

      <div className="grid grid-cols-2 gap-x-6 gap-y-2 px-4 py-3 text-sm md:grid-cols-4">
        <div><p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Notice Date")}</p><p>{fmtDate(item.noticeDate)}</p></div>
        <div><p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Last Working Date")}</p><p>{fmtDate(item.lastWorkingDate)}</p></div>
        <div className="col-span-2"><p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Reason")}</p><p>{item.reason}</p></div>
      </div>

      {item.awaitingWorkflow && (
        <div className="mx-4 mb-3 flex items-center gap-2 rounded-md border border-info/30 bg-info/10 px-3 py-2 text-xs text-info">
          <Hourglass size={14} /> {t("Awaiting approval — HR and your manager have been notified.")}
        </div>
      )}

      {item.status === "ClearanceInProgress" && clearances.length > 0 && (
        <div className="border-t border-border px-4 py-3">
          <p className="mb-1 text-xs font-bold uppercase tracking-wide text-muted">{t("Departmental Clearance")} ({cleared}/{clearances.length})</p>
          <div className="flex flex-wrap gap-1.5">
            {clearances.map((c) => (
              <span key={c.id} className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${c.status === "Cleared" ? "bg-success/15 text-success" : "bg-warning/15 text-warning"}`}>
                {c.department}
              </span>
            ))}
          </div>
        </div>
      )}

      {(recoveries?.length ?? 0) > 0 && (
        <div className="border-t border-border px-4 py-3">
          <p className="mb-1 text-xs font-bold uppercase tracking-wide text-muted">{t("Company Property to Return")}</p>
          <div className="flex flex-wrap gap-1.5">
            {recoveries!.map((r) => (
              <span key={r.id} className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${r.status === "Outstanding" ? "bg-warning/15 text-warning" : "bg-success/15 text-success"}`}>
                {r.assetName}{r.status !== "Outstanding" ? ` ✓` : ""}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Exit interview — the leaver answers here (HC219) */}
      {interview && interview.status === "Pending" && (
        <div className="border-t border-border px-4 py-3">
          <p className="mb-1 text-xs font-bold uppercase tracking-wide text-muted">{t("Exit Interview")}</p>
          <p className="mb-2 text-xs text-muted">{t("Your feedback helps us improve — answers go to HR only.")}</p>
          <ExitInterviewForm
            interview={interview}
            onSubmitted={(m) => {
              onChanged(m);
              queryClient.invalidateQueries({ queryKey: ["exitInterview", item.id] });
            }}
          />
        </div>
      )}
      {interview && interview.status === "Completed" && (
        <div className="border-t border-border px-4 py-3">
          <p className="text-xs text-muted">✓ {t("Exit interview completed — thank you for your feedback.")}</p>
        </div>
      )}

      {/* Final settlement — read-only for the leaver (HC216/HC218) */}
      {settlement && (
        <div className="border-t border-border px-4 py-3">
          <p className="mb-1 text-xs font-bold uppercase tracking-wide text-muted">{t("Final Settlement")} ({settlement.status})</p>
          <div className="space-y-0.5 text-sm">
            {(settlement.lines ?? []).map((l, i) => (
              <p key={l.id ?? i} className="flex justify-between text-xs">
                <span className="text-muted">{l.label}</span>
                <span className={l.kind === "Deduction" ? "text-error" : "text-foreground"}>
                  {l.kind === "Deduction" ? "−" : ""}{Number(l.amount ?? 0).toLocaleString()}
                </span>
              </p>
            ))}
            <p className="mt-1 flex justify-between border-t border-border/60 pt-1 text-xs font-semibold">
              <span>{t("Net settlement")}</span>
              <span className="text-primary">{Number(settlement.netAmount ?? 0).toLocaleString()}</span>
            </p>
          </div>
        </div>
      )}
    </div>
  );
}

/** My Exit — resignation self-service (HC209) + case tracking (HC220 keeps stakeholders informed). */
function MyExit() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<{ noticeDate?: string; lastWorkingDate?: string; reason?: string; remarks?: string }>({});
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  // Resolves the caller's own employee id (same pattern as My Training).
  const { data: me, isLoading } = useQuery({ queryKey: ["myCpd"], queryFn: () => getCpdSummary(), retry: false });
  const myEmployeeId = me?.employeeId;

  const { data: cases } = useQuery({
    queryKey: ["myTerminations", myEmployeeId],
    queryFn: () => getTerminations(myEmployeeId!),
    enabled: !!myEmployeeId,
  });

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["myTerminations"] });
  };

  const list = cases ?? [];
  const active = list.find((x) => x.status === "Initiated" || x.status === "ClearanceInProgress");

  const submit = async () => {
    if (!myEmployeeId) return;
    setBusy(true);
    const res = await submitResignation({
      employeeId: myEmployeeId,
      terminationType: "Voluntary",
      noticeDate: meta.noticeDate,
      lastWorkingDate: meta.lastWorkingDate,
      reason: meta.reason,
      remarks: meta.remarks || undefined,
    });
    setBusy(false);
    refresh(res.message);
    if (res.ok) setMeta({});
  };

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><DoorOpen className="h-5 w-5" /></span>
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("My Exit")}</h1>
          <p className="text-xs text-muted">{t("Submit a resignation or early-retirement request and track your exit process.")}</p>
        </div>
      </div>

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

      {isLoading ? (
        <Loading />
      ) : !myEmployeeId ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("Your account is not linked to an employee record.")}
        </p>
      ) : (
        <div className="min-h-0 flex-1 space-y-4 overflow-auto">
          {active ? (
            <CaseTracker item={active} onChanged={refresh} />
          ) : (
            <div className="rounded-lg border border-border bg-card p-4">
              <h3 className="mb-1 text-sm font-semibold text-foreground">{t("Request a Voluntary Exit")}</h3>
              <p className="mb-3 text-xs text-muted">
                {t("Your request goes to HR and your manager for approval (resignation or early retirement). Involuntary cases are handled by HR.")}
              </p>
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div>
                  <label className={LABEL}>{t("Notice Date")} *</label>
                  <input type="date" className={INPUT} value={meta.noticeDate ?? ""} onChange={(e) => setMeta((p) => ({ ...p, noticeDate: e.target.value }))} />
                </div>
                <div>
                  <label className={LABEL}>{t("Intended Last Working Date")} *</label>
                  <input type="date" className={INPUT} value={meta.lastWorkingDate ?? ""} onChange={(e) => setMeta((p) => ({ ...p, lastWorkingDate: e.target.value }))} />
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Reason")} *</label>
                  <textarea className={INPUT} rows={3} placeholder={t("Why are you leaving?")} value={meta.reason ?? ""} onChange={(e) => setMeta((p) => ({ ...p, reason: e.target.value }))} />
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Remarks")}</label>
                  <input type="text" className={INPUT} value={meta.remarks ?? ""} onChange={(e) => setMeta((p) => ({ ...p, remarks: e.target.value }))} />
                </div>
              </div>
              <div className="mt-4 flex justify-end">
                <button type="button" disabled={busy || !meta.noticeDate || !meta.lastWorkingDate || !meta.reason?.trim()} onClick={submit}
                  className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                  <Send size={14} /> {busy ? t("Submitting…") : t("Submit Request")}
                </button>
              </div>
            </div>
          )}

          {list.filter((x) => x !== active).length > 0 && (
            <div className="rounded-lg border border-border bg-card">
              <p className="border-b border-border px-4 py-2 text-xs font-semibold uppercase tracking-wide text-muted">{t("Past Requests")}</p>
              <div className="divide-y divide-border/60">
                {list.filter((x) => x !== active).map((x) => (
                  <div key={x.id} className="flex items-center justify-between px-4 py-2 text-sm">
                    <span>{t(x.terminationType ?? "")} · {fmtDate(x.noticeDate)} → {fmtDate(x.lastWorkingDate)}</span>
                    <span className={`rounded px-2 py-0.5 text-xs font-semibold ${CASE_TONE[x.status ?? ""] ?? ""}`}>{t(x.status ?? "")}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default memo(MyExit);
