import { memo } from "react";
import { useTranslation } from "react-i18next";
import { PieChart, Users2 } from "lucide-react";
import { ChartCard } from "./shared";
import { BarBreakdown, ChartLegend, DonutChart, type Slice } from "./charts";
import { useDashboardSummary } from "./useDashboardSummary";

/**
 * The analytics band: workflow status as a ring, workforce make-up as magnitude bars.
 *
 * Data comes from the SAME `useDashboardSummary()` query the KPI strip already runs — identical
 * queryKey, so React Query serves it from cache and this widget costs ZERO extra network calls.
 * Every number plotted is a field the aggregate endpoint already returned; nothing is derived from
 * a sample, extrapolated, or invented.
 */
function WorkforceAnalyticsWidget() {
  const { t } = useTranslation();
  const { data: summary, isLoading } = useDashboardSummary();

  const running = summary?.workflowRunning ?? 0;
  const approved = summary?.workflowApproved ?? 0;
  const rejected = summary?.workflowRejected ?? 0;
  const workflowTotal = running + approved + rejected;

  const workflowSlices: Slice[] = [
    { label: t("Running"), value: running, color: "var(--warning)" },
    { label: t("Approved"), value: approved, color: "var(--success)" },
    { label: t("Rejected"), value: rejected, color: "var(--error)" },
  ];

  // Headcount split into the two watch categories plus everyone else. Clamped at 0 so a stale count
  // can never render a negative bar.
  const employees = summary?.employeeCount ?? 0;
  const probation = summary?.probationCount ?? 0;
  const retiring = summary?.retirementCount ?? 0;
  const settled = Math.max(0, employees - probation - retiring);

  const workforceItems: Slice[] = [
    { label: t("Established"), value: settled, color: "var(--primary)" },
    { label: t("On Probation"), value: probation, color: "var(--warning)" },
    { label: t("Retiring Soon"), value: retiring, color: "var(--info)" },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
      <ChartCard title={t("Workflow Status", "Workflow Status")} icon={<PieChart className="h-4 w-4" />}>
        {isLoading ? (
          <div className="flex w-full items-center gap-5">
            <div className="h-[132px] w-[132px] shrink-0 animate-pulse rounded-full bg-muted/30" />
            <div className="flex-1 space-y-3">
              {[0, 1, 2].map((i) => (
                <div key={i} className="h-3.5 w-full animate-pulse rounded bg-muted/30" />
              ))}
            </div>
          </div>
        ) : (
          <div className="flex w-full flex-col items-center gap-5 sm:flex-row">
            <DonutChart
              slices={workflowSlices}
              centerValue={workflowTotal}
              centerLabel={t("Total", "Total")}
            />
            <ChartLegend slices={workflowSlices} total={workflowTotal} />
          </div>
        )}
      </ChartCard>

      <ChartCard title={t("Workforce Composition", "Workforce Composition")} icon={<Users2 className="h-4 w-4" />}>
        {isLoading ? (
          <div className="w-full space-y-5">
            {[0, 1, 2].map((i) => (
              <div key={i} className="space-y-2">
                <div className="h-3.5 w-1/3 animate-pulse rounded bg-muted/30" />
                <div className="h-2 w-full animate-pulse rounded-full bg-muted/30" />
              </div>
            ))}
          </div>
        ) : (
          <div className="w-full">
            <div className="mb-3 flex items-baseline gap-2">
              <span className="text-[24px] font-bold leading-7 tracking-tight text-foreground tabular-nums">
                {employees}
              </span>
              <span className="text-[12px] text-muted">{t("active employees", "active employees")}</span>
            </div>
            <BarBreakdown items={workforceItems} />
          </div>
        )}
      </ChartCard>
    </div>
  );
}

export default memo(WorkforceAnalyticsWidget);
