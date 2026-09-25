import { memo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import getEmployeesOnProbation from "@/services/admin/employee/onProbation";
import getUpcomingRetirements from "@/services/admin/employee/upcomingRetirements";
import getOverdueLeaveReturns from "@/services/admin/dashboard/overdueLeaveReturns";
import { UserCheck } from "lucide-react";
import {
  DaysBadge,
  EmptyRow,
  HAIRLINE,
  RECESSED,
  SURFACE,
  TABLE_HEAD,
  TABLE_ROW,
  TD,
  TD_STRONG,
  TH,
} from "./shared";

/** Shared column template so the header strip and every row align exactly. */
const COLS = "grid-cols-[minmax(0,1fr)_88px_84px]";
import { useDashboardSummary } from "./useDashboardSummary";

type WatchTab = "probation" | "retirements" | "notReturned";

/** The Not Returned tab needs a wider first column for the leave name, and no "date + remaining" pair. */
const RETURN_COLS = "grid-cols-[minmax(0,1fr)_96px_84px]";

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
  const { data: notReturned, isLoading: lnr } = useQuery({
    queryKey: ["overdueLeaveReturns"],
    queryFn: getOverdueLeaveReturns,
    staleTime: 60_000,
    enabled: activeTab === "notReturned",
  });

  const tabs: { key: WatchTab; label: string; count: number }[] = [
    { key: "probation", label: t("On Probation"), count: summary?.probationCount ?? 0 },
    { key: "retirements", label: t("Upcoming Retirements"), count: summary?.retirementCount ?? 0 },
    { key: "notReturned", label: t("Not Returned"), count: summary?.overdueLeaveReturnCount ?? 0 },
  ];

  return (
    <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
      <header className={`flex items-center gap-2 border-b px-4 py-2 ${HAIRLINE}`}>
        <UserCheck className="h-4 w-4 shrink-0 text-primary" />
        <h3 className="truncate text-[12px] font-semibold uppercase tracking-wide text-foreground">
          {t("Workforce Watchlist", "Workforce Watchlist")}
        </h3>
      </header>
      {/* Recessed rail + raised active pill: the standard enterprise segmented control. */}
      <div className={`flex flex-wrap items-center gap-1 border-b px-2 py-1.5 ${HAIRLINE} ${RECESSED}`}>
        {tabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() => setActiveTab(tab.key)}
            className={`flex items-center gap-2 rounded-md px-3 py-1.5 text-[13px] font-medium transition-colors ${
              activeTab === tab.key
                ? "bg-card text-primary shadow-[0_1px_2px_rgba(15,23,42,0.08)] ring-1 ring-[var(--border)]"
                : "text-muted hover:text-foreground"
            }`}
          >
            {tab.label}
            <span
              className={`rounded px-1.5 py-px text-[11px] font-semibold tabular-nums ${
                activeTab === tab.key ? "bg-primary/10 text-primary" : "bg-muted/30 text-muted"
              }`}
            >
              {tab.count}
            </span>
          </button>
        ))}
      </div>

      {activeTab === "probation" && (
        <>
          <div className={`${TABLE_HEAD} ${COLS}`}>
            <span className={TH}>{t("Employee", "Employee")}</span>
            <span className={`${TH} text-right`}>{t("Ends", "Ends")}</span>
            <span className={`${TH} text-right`}>{t("Remaining", "Remaining")}</span>
          </div>
          <div className={`divide-y ${HAIRLINE}`}>
            {lpr && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
            {!lpr && (probation?.length ?? 0) === 0 && (
              <EmptyRow text={t("No employees on probation.", "No employees on probation.")} />
            )}
            {probation?.map((e) => (
              <Link key={e.id} to="/employee" className={`${TABLE_ROW} ${COLS}`}>
                <div className="min-w-0">
                  <p className={TD_STRONG}>{e.fullName}</p>
                  <p className={TD}>
                    {e.employeeNumber}
                    {e.positionTitle ? ` · ${e.positionTitle}` : ""}
                  </p>
                </div>
                <span className={`text-right ${TD}`}>
                  {e.probationEndDate ? new Date(e.probationEndDate).toLocaleDateString() : "—"}
                </span>
                <span className="justify-self-end">
                  <DaysBadge days={e.daysRemaining} warnAt={7} />
                </span>
              </Link>
            ))}
          </div>
        </>
      )}

      {activeTab === "retirements" && (
        <>
          <div className={`${TABLE_HEAD} ${COLS}`}>
            <span className={TH}>{t("Employee", "Employee")}</span>
            <span className={`${TH} text-right`}>{t("Retires", "Retires")}</span>
            <span className={`${TH} text-right`}>{t("Remaining", "Remaining")}</span>
          </div>
          <div className={`divide-y ${HAIRLINE}`}>
            {lrt && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
            {!lrt && (retirements?.length ?? 0) === 0 && (
              <EmptyRow text={t("No retirements within a month.", "No retirements within a month.")} />
            )}
            {retirements?.map((e) => (
              <Link key={e.id} to="/employee" className={`${TABLE_ROW} ${COLS}`}>
                <div className="min-w-0">
                  <p className={TD_STRONG}>{e.fullName}</p>
                  <p className={TD}>{e.employeeNumber}</p>
                </div>
                <span className={`text-right ${TD}`}>
                  {new Date(e.retirementDate).toLocaleDateString()}
                </span>
                <span className="justify-self-end">
                  <DaysBadge days={e.daysRemaining} warnAt={14} />
                </span>
              </Link>
            ))}
          </div>
        </>
      )}

      {activeTab === "notReturned" && (
        <>
          <div className={`${TABLE_HEAD} ${RETURN_COLS}`}>
            <span className={TH}>{t("Employee", "Employee")}</span>
            {/* "Last Day", not "Due Back": the value is plannedEndDate — the last APPROVED DAY
                OF LEAVE, which is one day before the employee is due back. Labelling it "Due Back"
                showed every row a day early, the same off-by-one the history panel had. */}
            <span className={`${TH} text-right`}>{t("Last Day", "Last Day")}</span>
            <span className={`${TH} text-right`}>{t("Overdue", "Overdue")}</span>
          </div>
          <div className={`divide-y ${HAIRLINE}`}>
            {lnr && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
            {!lnr && (notReturned?.length ?? 0) === 0 && (
              <EmptyRow text={t("Everyone is back from leave.", "Everyone is back from leave.")} />
            )}
            {notReturned?.map((e) => (
              <Link
                key={`${e.source}-${e.requestId}`}
                // Annual rows are actionable — the return is confirmed on the Annual Leave screen.
                // Other Leave has no return step, so that row goes to its own register instead.
                to={e.source === "AnnualLeave" ? "/annualLeave" : "/otherLeave"}
                className={`${TABLE_ROW} ${RETURN_COLS}`}
              >
                <div className="min-w-0">
                  <p className={TD_STRONG}>{e.fullName}</p>
                  <p className={TD}>
                    {e.employeeNumber} · {e.leaveName}
                    {/* Said on the row, because "overdue" invites an action that does not exist for
                        Other Leave — there is nothing to confirm, only something to notice. */}
                    {!e.hasReturnConfirmation && ` · ${t("no return step", "no return step")}`}
                  </p>
                </div>
                <span className={`text-right ${TD}`}>
                  {new Date(e.plannedEndDate).toLocaleDateString()}
                </span>
                <span className="justify-self-end">
                  {/* Days since the leave ENDED, so a row is 1d the morning after the last leave day
                      — the day they are due back. DaysBadge renders negatives as "Nd overdue". */}
                  <DaysBadge days={-e.daysOverdue} warnAt={0} />
                </span>
              </Link>
            ))}
          </div>
        </>
      )}
    </section>
  );
}

export default memo(WorkforceWatchlistWidget);
