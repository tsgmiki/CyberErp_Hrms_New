import { memo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import getEmployeesOnProbation from "@/services/admin/employee/onProbation";
import getUpcomingRetirements from "@/services/admin/employee/upcomingRetirements";
import { DaysBadge, EmptyRow } from "./shared";
import { useDashboardSummary } from "./useDashboardSummary";

type WatchTab = "probation" | "retirements";

/**
 * Probation / Retirement watchlist. Tab BADGE counts come from the aggregated summary (always
 * available immediately, no extra call); the ROW-LEVEL list is fetched ONLY for whichever tab is
 * active (`enabled: activeTab === key`) — previously both full lists fetched unconditionally on
 * mount even though only one is ever visible. Switching tabs re-uses the cache after the first visit.
 * `activeTab` is LOCAL state: switching tabs re-renders only this widget, never its siblings.
 */
function WorkforceWatchlistWidget() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<WatchTab>("probation");
  const { data: summary } = useDashboardSummary();

  const { data: probation, isLoading: lpr } = useQuery({
    queryKey: ["employeesOnProbation"],
    queryFn: getEmployeesOnProbation,
    staleTime: 60_000,
    enabled: activeTab === "probation",
  });
  const { data: retirements, isLoading: lrt } = useQuery({
    queryKey: ["upcomingRetirements"],
    queryFn: getUpcomingRetirements,
    staleTime: 60_000,
    enabled: activeTab === "retirements",
  });

  const tabs: { key: WatchTab; label: string; count: number }[] = [
    { key: "probation", label: t("On Probation"), count: summary?.probationCount ?? 0 },
    { key: "retirements", label: t("Upcoming Retirements"), count: summary?.retirementCount ?? 0 },
  ];

  return (
    <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
      <div className="flex items-center gap-1 border-b border-border px-2 pt-1.5">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() => setActiveTab(tab.key)}
            className={`relative flex items-center gap-2 rounded-t-lg px-3 py-2 text-[13px] font-medium transition-colors ${
              activeTab === tab.key ? "text-primary" : "text-muted hover:text-foreground"
            }`}
          >
            {tab.label}
            <span
              className={`rounded-full px-1.5 py-0.5 text-[11px] font-semibold tabular-nums ${
                activeTab === tab.key ? "bg-primary/10 text-primary" : "bg-muted/25 text-muted"
              }`}
            >
              {tab.count}
            </span>
            {activeTab === tab.key && <span className="absolute inset-x-2 -bottom-px h-0.5 rounded-full bg-primary" />}
          </button>
        ))}
      </div>

      {activeTab === "probation" && (
        <div className="divide-y divide-border/60">
          {lpr && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
          {!lpr && (probation?.length ?? 0) === 0 && (
            <EmptyRow text={t("No employees on probation.", "No employees on probation.")} />
          )}
          {probation?.map((e) => (
            <Link
              key={e.id}
              to="/employee"
              className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40"
            >
              <div className="min-w-0 flex-1">
                <p className="truncate text-[13px] font-medium text-foreground">{e.fullName}</p>
                <p className="truncate text-xs text-muted">
                  {e.employeeNumber}
                  {e.positionTitle ? ` · ${e.positionTitle}` : ""}
                </p>
              </div>
              <div className="shrink-0 text-right">
                <p className="text-xs text-muted">
                  {e.probationEndDate ? new Date(e.probationEndDate).toLocaleDateString() : "—"}
                </p>
                <DaysBadge days={e.daysRemaining} warnAt={7} />
              </div>
            </Link>
          ))}
        </div>
      )}

      {activeTab === "retirements" && (
        <div className="divide-y divide-border/60">
          {lrt && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
          {!lrt && (retirements?.length ?? 0) === 0 && (
            <EmptyRow text={t("No retirements within a month.", "No retirements within a month.")} />
          )}
          {retirements?.map((e) => (
            <Link
              key={e.id}
              to="/employee"
              className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40"
            >
              <div className="min-w-0 flex-1">
                <p className="truncate text-[13px] font-medium text-foreground">{e.fullName}</p>
                <p className="truncate text-xs text-muted">{e.employeeNumber}</p>
              </div>
              <div className="shrink-0 text-right">
                <p className="text-xs text-muted">{new Date(e.retirementDate).toLocaleDateString()}</p>
                <DaysBadge days={e.daysRemaining} warnAt={14} />
              </div>
            </Link>
          ))}
        </div>
      )}
    </section>
  );
}

export default memo(WorkforceWatchlistWidget);
