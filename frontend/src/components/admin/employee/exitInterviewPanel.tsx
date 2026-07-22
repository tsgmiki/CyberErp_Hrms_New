"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { MessageSquareQuote, Rocket } from "lucide-react";
import { getExitInterview, launchExitInterview } from "@/services/admin/employee/exitManagement";
import ExitInterviewForm from "./exitInterviewForm";

/** HC219 — the case's exit interview: launch (HR), record, or read the completed answers. */
function ExitInterviewPanel({ terminationId }: { terminationId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const { data: interview } = useQuery({
    queryKey: ["exitInterview", terminationId],
    queryFn: () => getExitInterview(terminationId),
    retry: false,
  });

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["exitInterview", terminationId] });
  };

  const launch = async () => {
    setBusy(true);
    const res = await launchExitInterview(terminationId);
    setBusy(false);
    refresh(res.ok ? t("Interview launched — the employee can now answer from My Exit.") : res.message);
  };

  return (
    <div className="border-t border-border">
      <div className="flex items-center justify-between px-4 py-2">
        <h4 className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wide text-muted">
          <MessageSquareQuote size={13} /> {t("Exit Interview")}
        </h4>
        {interview && (
          <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${interview.status === "Completed" ? "bg-success/15 text-success" : "bg-warning/15 text-warning"}`}>
            {interview.status}
          </span>
        )}
      </div>
      {msg && <p className="mx-4 mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-1.5 text-xs text-muted">{msg}</p>}

      <div className="px-4 pb-3">
        {!interview ? (
          <button type="button" disabled={busy} onClick={launch}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-secondary/40 disabled:opacity-50">
            <Rocket size={13} /> {busy ? t("Launching…") : t("Launch Interview")}
          </button>
        ) : interview.status === "Pending" ? (
          <div className="rounded-md border border-border/70 bg-secondary/10 p-3">
            <p className="mb-2 text-xs text-muted">
              {t("Awaiting the employee's answers (My Exit) — or record the conversation here.")}
            </p>
            <ExitInterviewForm interview={interview} onSubmitted={refresh} />
          </div>
        ) : (
          <div className="space-y-2">
            {(interview.questions ?? []).map((q) => (
              <div key={q.key} className="rounded-md border border-border/70 px-3 py-2">
                <p className="text-xs font-medium text-muted">{q.text}</p>
                <p className="text-sm text-foreground">{interview.answers?.[q.key ?? ""] || "—"}</p>
              </div>
            ))}
            <p className="text-[11px] text-muted">{t("Completed")} {String(interview.completedOn ?? "").slice(0, 10)}</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default memo(ExitInterviewPanel);
