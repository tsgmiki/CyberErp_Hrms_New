"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ShieldCheck, AlertTriangle, Clock, RefreshCw } from "lucide-react";
import { getComplianceOverview, runComplianceSweep } from "@/services/admin/learningCompliance";
import type { ComplianceRowModel } from "@/models";
import { toast } from "@/components/common/toast";
import { confirm } from "@/components/common/dialog";
import Loading from "@/components/common/loader/loader";

/** A percentage bar that encodes state in colour as well as number, so it reads at a glance. */
function ComplianceBar({ percent }: { percent: number }) {
  const tone = percent >= 95 ? "bg-success" : percent >= 80 ? "bg-warning" : "bg-error";
  return (
    <div className="flex items-center gap-2">
      <div className="h-1.5 w-20 overflow-hidden rounded-full bg-secondary">
        <div className={`h-full rounded-full ${tone}`} style={{ width: `${Math.min(100, percent)}%` }} />
      </div>
      <span className="w-11 shrink-0 text-right text-xs tabular-nums text-foreground">
        {percent.toFixed(0)}%
      </span>
    </div>
  );
}

function Breakdown({ title, rows }: { title: string; rows: ComplianceRowModel[] }) {
  const { t } = useTranslation();
  if (rows.length === 0) return null;

  return (
    <div className="rounded-lg border border-border bg-card p-3">
      <p className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-muted">{t(title)}</p>
      <div className="overflow-x-auto">
        <table className="w-full min-w-[34rem] text-sm">
          <thead>
            <tr className="border-b border-border text-left text-[11px] uppercase tracking-wide text-muted">
              <th className="py-1.5 pr-3 font-semibold">{t("Name")}</th>
              <th className="py-1.5 pr-3 text-right font-semibold">{t("Done")}</th>
              <th className="py-1.5 pr-3 text-right font-semibold">{t("Outstanding")}</th>
              <th className="py-1.5 pr-3 text-right font-semibold">{t("Overdue")}</th>
              <th className="py-1.5 pr-3 text-right font-semibold">{t("Waived")}</th>
              <th className="py-1.5 font-semibold">{t("Compliance")}</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.id} className="border-b border-border last:border-0">
                <td className="py-1.5 pr-3 text-foreground">{r.name}</td>
                <td className="py-1.5 pr-3 text-right tabular-nums text-muted">{r.completed}</td>
                <td className="py-1.5 pr-3 text-right tabular-nums text-muted">{r.pending}</td>
                <td className={`py-1.5 pr-3 text-right tabular-nums ${r.overdue > 0 ? "font-semibold text-error" : "text-muted"}`}>
                  {r.overdue}
                </td>
                {/* Waivers are shown rather than folded away: they leave the percentage's
                    denominator, so the count is the only place they remain visible. */}
                <td className="py-1.5 pr-3 text-right tabular-nums text-muted">{r.waived || "—"}</td>
                <td className="py-1.5"><ComplianceBar percent={r.compliancePercent} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

/**
 * Where the organisation stands on mandatory training.
 *
 * <p>Every percentage is completions over everything that still counts: waived obligations leave the
 * denominator, because an excused row is neither a pass nor a failure. Their count is shown so the
 * figure can still be reconciled.</p>
 */
function ComplianceDashboard() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["complianceOverview"],
    queryFn: () => getComplianceOverview(),
  });

  if (isLoading) return <Loading />;

  const run = async () => {
    const ok = await confirm({
      title: t("Run the compliance sweep now?"),
      // This messages people. Saying so before the click is the difference between a useful button
      // and one nobody dares press.
      message: t("This reconciles every obligation and emails everyone with training due or overdue. It normally runs overnight."),
      confirmLabel: t("Run Now") ?? "Run Now",
    });
    if (!ok) return;
    setBusy(true);
    const res = await runComplianceSweep();
    setBusy(false);
    if (res.status === "success") {
      toast.success(res.message);
      queryClient.invalidateQueries({ queryKey: ["complianceOverview"] });
      queryClient.invalidateQueries({ queryKey: ["obligations"] });
      queryClient.invalidateQueries({ queryKey: ["learningAssignments"] });
    } else {
      toast.error(res.message);
    }
  };

  const overview = data;
  const nothing = !overview || overview.totalObligations === 0;

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs text-muted">
          {t("Obligations are reconciled nightly — new joiners picked up, completions matched, recertifications reopened.")}
        </p>
        <button
          type="button" disabled={busy} onClick={run}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md border border-border px-2.5 py-1.5 text-xs font-semibold text-foreground hover:bg-secondary disabled:opacity-40"
        >
          <RefreshCw className="h-3.5 w-3.5" /> {busy ? t("Running…") : t("Run Sweep Now")}
        </button>
      </div>

      {nothing ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("No obligations yet. Add an assignment, then run the sweep to materialise it.")}
        </p>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
            <div className="rounded-lg border border-border bg-card p-3">
              <p className="text-2xl font-semibold tabular-nums text-foreground">
                {overview.compliancePercent.toFixed(0)}%
              </p>
              <p className="text-[11px] text-muted">{t("compliant")}</p>
            </div>
            <div className="rounded-lg border border-border bg-card p-3">
              <p className="flex items-center gap-1.5 text-2xl font-semibold tabular-nums text-foreground">
                <ShieldCheck className="h-4 w-4 text-success" />{overview.completed}
              </p>
              <p className="text-[11px] text-muted">{t("completed")}</p>
            </div>
            <div className="rounded-lg border border-border bg-card p-3">
              <p className={`flex items-center gap-1.5 text-2xl font-semibold tabular-nums ${overview.overdue > 0 ? "text-error" : "text-foreground"}`}>
                <AlertTriangle className="h-4 w-4" />{overview.overdue}
              </p>
              <p className="text-[11px] text-muted">{t("overdue")}</p>
            </div>
            <div className="rounded-lg border border-border bg-card p-3">
              <p className="flex items-center gap-1.5 text-2xl font-semibold tabular-nums text-foreground">
                <Clock className="h-4 w-4 text-warning" />{overview.dueSoon}
              </p>
              <p className="text-[11px] text-muted">{t("due within 14 days")}</p>
            </div>
          </div>

          <Breakdown title="By course" rows={overview.byCourse} />
          <Breakdown title="By organizational unit" rows={overview.byUnit} />
        </>
      )}
    </div>
  );
}

export default memo(ComplianceDashboard);
