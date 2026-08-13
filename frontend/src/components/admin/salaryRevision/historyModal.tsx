"use client";

import { memo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { FilePlus2, Send, CheckCircle2, XCircle, Play, Clock, History } from "lucide-react";
import Modal from "@/components/ui/modal";
import Loading from "@/components/common/loader/loader";
import { getPerformanceHistory } from "@/services/admin/appraisal";
import { SALARY_REVISION_HISTORY_TYPE } from "@/services/admin/compensation";

interface Props {
  revisionId: string;
  revisionName?: string;
  onClose: () => void;
}

const fmt = (v?: string | null) => (v ? String(v).replace("T", " ").slice(0, 16) : "");

/**
 * Icon + tone per action. The three facts this trail exists to answer — who CREATED it, who APPROVED
 * it, who SUBMITTED it — must be distinguishable at a glance rather than by reading every line.
 */
function entryStyle(action?: string) {
  switch (action) {
    case "Created": return { Icon: FilePlus2, tone: "text-muted", ring: "border-border" };
    case "SentForApproval": return { Icon: Send, tone: "text-warning", ring: "border-warning/40" };
    case "Approved": return { Icon: CheckCircle2, tone: "text-info", ring: "border-info/40" };
    case "Submitted": return { Icon: Send, tone: "text-primary", ring: "border-primary/40" };
    case "Applied": return { Icon: Play, tone: "text-success", ring: "border-success/40" };
    case "Rejected": return { Icon: XCircle, tone: "text-error", ring: "border-error/40" };
    default: return { Icon: Clock, tone: "text-muted", ring: "border-border" };
  }
}

/** Human label for the stored action verb. */
const ACTION_LABEL: Record<string, string> = {
  Created: "Created",
  SentForApproval: "Sent for approval",
  Approved: "Approved",
  Submitted: "Submitted",
  Applied: "Applied",
  Rejected: "Rejected",
};

/**
 * The full lifecycle of one salary revision: who created it, who approved it, who submitted it and
 * who applied it, each with the time it happened.
 *
 * <p>Rows come from the shared append-only history table, written at every transition, so this is a
 * record of what actually happened rather than a re-derivation from the current status — a revision
 * sitting in "Applied" still shows the approval that authorised it and the person who committed it.
 * The server returns newest-first; this renders that order so the latest decision is at the top.</p>
 */
function SalaryRevisionHistoryModal({ revisionId, revisionName, onClose }: Props) {
  const { t } = useTranslation();

  const { data, isLoading, isError } = useQuery({
    queryKey: ["salaryRevisionHistory", revisionId],
    queryFn: () => getPerformanceHistory(SALARY_REVISION_HISTORY_TYPE, revisionId),
    retry: false,
  });

  const entries = data ?? [];
  // The three attributions the audit is actually for, pulled out of the thread so they are readable
  // without scanning it. Earliest Created, and the LATEST approve/submit (a rejected-then-resubmitted
  // revision has more than one, and the operative one is the most recent).
  const actorFor = (action: string) =>
    entries.filter((e) => e.action === action).map((e) => e.createdBy).find(Boolean) ?? null;
  const summary = [
    { label: "Created by", who: actorFor("Created") },
    { label: "Approved by", who: actorFor("Approved") },
    { label: "Submitted by", who: actorFor("Submitted") },
  ];

  return (
    <Modal isOpen onClose={onClose} size="lg" title={t("Salary revision history") ?? undefined}>
      {isLoading ? (
        <Loading />
      ) : isError ? (
        <p className="py-6 text-center text-sm text-muted">
          {t("This revision's history is not available.")}
        </p>
      ) : (
        <div className="space-y-4">
          {revisionName && (
            <p className="text-sm font-semibold text-foreground">{revisionName}</p>
          )}

          {/* The audit answer up front. */}
          <div className="grid grid-cols-1 gap-2 rounded-lg border border-border bg-secondary/20 p-3 sm:grid-cols-3">
            {summary.map((s) => (
              <div key={s.label}>
                <p className="text-[11px] uppercase text-muted">{t(s.label)}</p>
                <p className="text-sm font-semibold text-foreground">{s.who ?? "—"}</p>
              </div>
            ))}
          </div>

          <ol className="relative space-y-3 border-l border-border pl-5">
            {entries.map((e, i) => {
              const { Icon, tone, ring } = entryStyle(e.action);
              return (
                <li key={e.id ?? i} className="relative">
                  <span
                    className={`absolute -left-[1.68rem] flex h-6 w-6 items-center justify-center rounded-full border bg-card ${ring}`}
                  >
                    <Icon size={13} className={tone} />
                  </span>
                  <div className="rounded-md border border-border bg-card px-3 py-2">
                    <div className="flex flex-wrap items-baseline justify-between gap-2">
                      <p className={`text-sm font-medium ${tone}`}>
                        {t(ACTION_LABEL[e.action ?? ""] ?? e.action ?? "")}
                      </p>
                      <span className="text-[11px] tabular-nums text-muted">{fmt(e.createdAt)}</span>
                    </div>
                    {e.summary && <p className="mt-0.5 text-xs text-muted">{e.summary}</p>}
                    {e.createdBy && (
                      <p className="mt-1 text-[11px] text-muted">
                        {t("by")} <span className="font-semibold text-foreground">{e.createdBy}</span>
                      </p>
                    )}
                  </div>
                </li>
              );
            })}
            {entries.length === 0 && (
              <li className="text-sm text-muted">
                {t("Nothing has happened on this revision yet.")}
              </li>
            )}
          </ol>

          <p className="flex items-center gap-1.5 text-[11px] text-muted">
            <History size={12} />
            {t("Every transition is recorded permanently and cannot be edited.")}
          </p>
        </div>
      )}
    </Modal>
  );
}

export default memo(SalaryRevisionHistoryModal);
