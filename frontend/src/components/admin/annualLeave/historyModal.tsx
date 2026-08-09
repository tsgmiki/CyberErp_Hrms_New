"use client";

import { memo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  FileText, CheckCircle2, XCircle, Undo2, Flag, History, Clock,
} from "lucide-react";
import Modal from "@/components/ui/modal";
import Loading from "@/components/common/loader/loader";
import { getAnnualLeaveHistory } from "@/services/admin/annualLeave/returnFromLeave";
import type { AnnualLeaveHistoryEntryModel } from "@/models";

interface Props {
  annualLeaveId: string;
  onClose: () => void;
}

const fmt = (v?: string | null) => (v ? String(v).replace("T", " ").slice(0, 16) : "");
const day = (v?: string | null) => (v ? String(v).slice(0, 10) : "—");

/**
 * Icon + tone per entry kind. A rejection has to read differently from an approval at a glance —
 * an approver scanning the thread should not have to read every line to find where it turned.
 */
function entryStyle(e: AnnualLeaveHistoryEntryModel) {
  if (e.kind === "Submitted") return { Icon: FileText, tone: "text-info", ring: "border-info/40" };
  if (e.kind === "Return") return { Icon: Undo2, tone: "text-warning", ring: "border-warning/40" };
  if (e.kind === "Settled") return { Icon: Flag, tone: "text-success", ring: "border-success/40" };
  if (/reject/i.test(e.action ?? "")) return { Icon: XCircle, tone: "text-error", ring: "border-error/40" };
  if (/approve/i.test(e.action ?? "")) return { Icon: CheckCircle2, tone: "text-success", ring: "border-success/40" };
  return { Icon: Clock, tone: "text-muted", ring: "border-border" };
}

/**
 * The complete lifecycle of one annual leave request, as a popup.
 *
 * <p>Built for the APPROVER deciding on a return adjustment: they need what was originally approved,
 * who approved it, what the employee said when they came back, and why any earlier attempt was
 * rejected — all before they can judge the current one. The server returns it as a single ordered
 * timeline, so this component only has to render.</p>
 */
function AnnualLeaveHistoryModal({ annualLeaveId, onClose }: Props) {
  const { t } = useTranslation();

  const { data, isLoading, isError } = useQuery({
    queryKey: ["annualLeaveHistory", annualLeaveId],
    queryFn: () => getAnnualLeaveHistory(annualLeaveId),
    retry: false,
  });

  const adj = data?.adjustmentDays ?? 0;

  return (
    <Modal isOpen onClose={onClose} size="lg" title={t("Leave request history") ?? undefined}>
      {isLoading ? (
        <Loading />
      ) : isError || !data ? (
        <p className="py-6 text-center text-sm text-muted">
          {t("This request's history is no longer available.")}
        </p>
      ) : (
        <div className="space-y-4">
          {/* The summary an approver checks first: what was asked, what was taken, what it changes. */}
          <div className="rounded-lg border border-border bg-secondary/20 p-3">
            <p className="text-sm font-semibold text-foreground">
              {data.employeeName || "—"}
              {data.employeeNumber ? <span className="ml-2 text-xs text-muted">{data.employeeNumber}</span> : null}
            </p>
            <div className="mt-2 grid grid-cols-2 gap-2 text-xs sm:grid-cols-4">
              <div>
                <p className="uppercase text-muted">{t("Approved")}</p>
                <p className="font-semibold tabular-nums text-foreground">{data.approvedDays ?? "—"} {t("day(s)")}</p>
              </div>
              <div>
                <p className="uppercase text-muted">{t("Actually taken")}</p>
                <p className="font-semibold tabular-nums text-foreground">
                  {data.actualDays == null ? "—" : `${data.actualDays} ${t("day(s)")}`}
                </p>
              </div>
              <div>
                <p className="uppercase text-muted">{t("Planned return")}</p>
                <p className="font-semibold tabular-nums text-foreground">{day(data.plannedEndDate)}</p>
              </div>
              <div>
                <p className="uppercase text-muted">{t("Actual return")}</p>
                <p className="font-semibold tabular-nums text-foreground">{day(data.actualEndDate)}</p>
              </div>
            </div>
            {adj !== 0 && (
              <p className={`mt-2 text-xs font-semibold ${adj < 0 ? "text-success" : "text-warning"}`}>
                {adj < 0
                  ? `${t("Returned early")} — ${Math.abs(adj)} ${t("day(s) credited back")}`
                  : `${t("Returned late")} — ${adj} ${t("extra day(s) deducted")}`}
              </p>
            )}
          </div>

          {/* The thread itself. */}
          <ol className="relative space-y-3 border-l border-border pl-5">
            {(data.entries ?? []).map((e, i) => {
              const { Icon, tone, ring } = entryStyle(e);
              return (
                <li key={i} className="relative">
                  <span
                    className={`absolute -left-[1.68rem] flex h-6 w-6 items-center justify-center rounded-full border bg-card ${ring}`}
                  >
                    <Icon size={13} className={tone} />
                  </span>
                  <div className="rounded-md border border-border bg-card px-3 py-2">
                    <div className="flex flex-wrap items-baseline justify-between gap-2">
                      <p className="text-sm font-medium text-foreground">{t(e.title ?? "")}</p>
                      <span className="text-[11px] tabular-nums text-muted">{fmt(e.at)}</span>
                    </div>
                    {e.detail && <p className="mt-0.5 text-xs text-muted">{e.detail}</p>}
                    {/* The comment is the whole point for an adjustment — give it visual weight. */}
                    {e.comment && (
                      <p className="mt-1 border-l-2 border-border pl-2 text-xs italic text-foreground">
                        “{e.comment}”
                      </p>
                    )}
                    {(e.actor || e.action) && (
                      <p className="mt-1 text-[11px] text-muted">
                        {e.action ? <span className={`font-semibold ${tone}`}>{t(e.action)}</span> : null}
                        {e.action && e.actor ? " · " : ""}
                        {e.actor ? `${t("by")} ${e.actor}` : ""}
                      </p>
                    )}
                  </div>
                </li>
              );
            })}
            {(data.entries ?? []).length === 0 && (
              <li className="text-sm text-muted">{t("Nothing has happened on this request yet.")}</li>
            )}
          </ol>

          <p className="flex items-center gap-1.5 text-[11px] text-muted">
            <History size={12} /> {t("Current status")}:{" "}
            <span className="font-semibold text-foreground">{t(data.status ?? "")}</span>
          </p>
        </div>
      )}
    </Modal>
  );
}

export default memo(AnnualLeaveHistoryModal);
