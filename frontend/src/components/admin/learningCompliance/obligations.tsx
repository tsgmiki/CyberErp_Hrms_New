"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ShieldOff, ChevronLeft, ChevronRight } from "lucide-react";
import { getObligations, waiveObligation } from "@/services/admin/learningCompliance";
import type { ObligationModel } from "@/models";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";

const PAGE = 25;

const day = (v?: string | null) =>
  v ? new Date(v).toLocaleDateString(undefined, { day: "2-digit", month: "short", year: "numeric" }) : "—";

const FILTERS = [
  { id: "Overdue", name: "Overdue" },
  { id: "Pending", name: "Outstanding" },
  { id: "Completed", name: "Completed" },
  { id: "Waived", name: "Waived" },
  { id: "", name: "All" },
];

/**
 * Who owes what — the row-level view behind the dashboard's percentages.
 *
 * <p>Defaults to Overdue, because that is the only tab anyone opens this screen to act on.</p>
 */
function ObligationsPanel() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [status, setStatus] = useState("Overdue");
  const [page, setPage] = useState(0);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [waivingId, setWaivingId] = useState<string | null>(null);
  const [reason, setReason] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["obligations", status, page],
    queryFn: () => getObligations({ skip: page * PAGE, take: PAGE, status: status || undefined }),
  });

  const rows = data?.data ?? [];
  const total = data?.total ?? 0;

  // A reason is required by the server, so it is asked for here rather than discovered as an
  // error — and it is what an audit reads to understand the exemption.
  const waive = async (row: ObligationModel) => {
    if (!reason.trim()) {
      toast.error(t("A waiver needs a reason."));
      return;
    }
    setBusyId(row.id);
    const res = await waiveObligation(row.id, reason.trim());
    setBusyId(null);
    setWaivingId(null);
    setReason("");
    if (res.status === "success") {
      toast.success(res.message);
      queryClient.invalidateQueries({ queryKey: ["obligations"] });
      queryClient.invalidateQueries({ queryKey: ["complianceOverview"] });
      queryClient.invalidateQueries({ queryKey: ["learningAssignments"] });
    } else {
      toast.error(res.message);
    }
  };

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center gap-1.5">
        {FILTERS.map((f) => (
          <button
            key={f.id || "all"} type="button"
            onClick={() => { setStatus(f.id); setPage(0); }}
            className={`rounded-md px-2.5 py-1 text-xs font-semibold ${
              status === f.id ? "bg-primary text-on-accent" : "border border-border text-muted hover:bg-secondary"
            }`}
          >
            {t(f.name)}
          </button>
        ))}
        <span className="ml-auto text-xs text-muted">{total} {t("record(s)")}</span>
      </div>

      {isLoading ? (
        <Loading />
      ) : rows.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {status === "Overdue"
            ? t("Nothing is overdue. ")
            : t("Nothing to show here.")}
        </p>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-border bg-card">
          <table className="w-full min-w-[52rem] text-sm">
            <thead>
              <tr className="border-b border-border text-left text-[11px] uppercase tracking-wide text-muted">
                <th className="px-3 py-2 font-semibold">{t("Employee")}</th>
                <th className="px-3 py-2 font-semibold">{t("Unit")}</th>
                <th className="px-3 py-2 font-semibold">{t("Course")}</th>
                <th className="px-3 py-2 font-semibold">{t("Cycle")}</th>
                <th className="px-3 py-2 font-semibold">{t("Due")}</th>
                <th className="px-3 py-2 font-semibold">{t("Status")}</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.id} className="border-b border-border last:border-0">
                  <td className="px-3 py-2">
                    <span className="block text-foreground">{r.employeeName ?? "—"}</span>
                    <span className="block text-[11px] text-muted">{r.employeeNumber}</span>
                  </td>
                  <td className="px-3 py-2 text-muted">{r.unitName ?? "—"}</td>
                  <td className="px-3 py-2">
                    <span className="block text-foreground">{r.courseName}</span>
                    <span className="block text-[11px] text-muted">{r.assignmentName}</span>
                  </td>
                  <td className="px-3 py-2 tabular-nums text-muted">{r.cycleNumber}</td>
                  <td className="px-3 py-2">
                    <span className={`tabular-nums ${r.isOverdue ? "font-semibold text-error" : "text-muted"}`}>
                      {day(r.dueOn)}
                    </span>
                    {/* The number of days is what makes a date actionable at a glance. */}
                    {r.status === "Pending" && (
                      <span className="block text-[11px] text-muted">
                        {r.daysRemaining < 0
                          ? `${Math.abs(r.daysRemaining)} ${t("day(s) overdue")}`
                          : `${r.daysRemaining} ${t("day(s) left")}`}
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    {r.status === "Completed" ? (
                      <span className="rounded-full bg-success/15 px-2 py-0.5 text-[11px] font-semibold text-success">
                        {t("Completed")} {day(r.completedOn)}
                      </span>
                    ) : r.status === "Waived" ? (
                      <span
                        title={r.waivedReason ?? undefined}
                        className="rounded-full bg-secondary px-2 py-0.5 text-[11px] font-semibold text-muted"
                      >
                        {t("Waived")}
                      </span>
                    ) : r.isOverdue ? (
                      <span className="rounded-full bg-error/15 px-2 py-0.5 text-[11px] font-semibold text-error">
                        {t("Overdue")}
                      </span>
                    ) : (
                      <span className="rounded-full bg-warning/15 px-2 py-0.5 text-[11px] font-semibold text-warning">
                        {t("Outstanding")}
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-2 text-right">
                    {r.status !== "Pending" ? null : waivingId === r.id ? (
                      <div className="flex items-center justify-end gap-1.5">
                        <input
                          autoFocus
                          value={reason}
                          onChange={(e) => setReason(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") void waive(r);
                            if (e.key === "Escape") { setWaivingId(null); setReason(""); }
                          }}
                          placeholder={t("Reason for the waiver") ?? ""}
                          className="w-52 rounded border border-border bg-card px-2 py-1 text-[11px] text-foreground focus:border-primary focus:outline-none"
                        />
                        <button
                          type="button" disabled={busyId === r.id} onClick={() => waive(r)}
                          className="rounded-md bg-primary px-2 py-1 text-[11px] font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
                        >
                          {t("Save")}
                        </button>
                        <button
                          type="button" onClick={() => { setWaivingId(null); setReason(""); }}
                          className="rounded-md border border-border px-2 py-1 text-[11px] font-semibold text-muted hover:bg-secondary"
                        >
                          {t("Cancel")}
                        </button>
                      </div>
                    ) : (
                      <button
                        type="button"
                        onClick={() => { setWaivingId(r.id); setReason(""); }}
                        className="inline-flex items-center gap-1.5 rounded-md border border-border px-2 py-1 text-[11px] font-semibold text-muted hover:bg-secondary"
                      >
                        <ShieldOff className="h-3 w-3" /> {t("Waive")}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {total > PAGE && (
        <div className="flex items-center justify-end gap-2 text-xs text-muted">
          <button
            type="button" disabled={page === 0} onClick={() => setPage((p) => p - 1)}
            className="rounded border border-border p-1 hover:bg-secondary disabled:opacity-30"
          >
            <ChevronLeft className="h-3.5 w-3.5" />
          </button>
          <span className="tabular-nums">
            {page * PAGE + 1}–{Math.min((page + 1) * PAGE, total)} {t("of")} {total}
          </span>
          <button
            type="button" disabled={(page + 1) * PAGE >= total} onClick={() => setPage((p) => p + 1)}
            className="rounded border border-border p-1 hover:bg-secondary disabled:opacity-30"
          >
            <ChevronRight className="h-3.5 w-3.5" />
          </button>
        </div>
      )}
    </div>
  );
}

export default memo(ObligationsPanel);
