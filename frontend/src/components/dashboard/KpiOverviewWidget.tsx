import { memo } from "react";
import { Building, Briefcase, CalendarClock, Hourglass, Network, UserPen, Users } from "lucide-react";
import { KpiTile } from "./shared";
import { useDashboardSummary } from "./useDashboardSummary";
import { useProfileChangeRequests } from "./useActionQueues";

/**
 * The KPI strip — one glanceable row, backed by the SINGLE aggregated /Dashboard/summary call (not
 * four separate paginated "GetAll?take=1" list requests). memo()'d: this widget only re-renders when
 * ITS OWN queries settle, never as a side effect of a decision modal's keystrokes elsewhere on the page.
 */
function KpiOverviewWidget() {
  const { data: summary, isLoading: ls } = useDashboardSummary();
  // Deduped against ActionQueueWidget's identical query (see useActionQueues) — one network call total.
  const { data: profileChanges, isLoading: lpc } = useProfileChangeRequests();
  const isChangeApprover = profileChanges?.isApprover === true;
  const changeCount = profileChanges?.items?.length ?? 0;

  return (
    <div
      className={`grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:gap-4 ${isChangeApprover ? "xl:grid-cols-7" : "xl:grid-cols-6"}`}
    >
      <KpiTile to="/employee" label="Employees" icon={<Users className="h-4.5 w-4.5" />} total={summary?.employeeCount} isLoading={ls} />
      <KpiTile to="/branch" label="Branches" icon={<Building className="h-4.5 w-4.5" />} total={summary?.branchCount} isLoading={ls} />
      <KpiTile to="/organizationUnit" label="Organization Units" icon={<Network className="h-4.5 w-4.5" />} total={summary?.organizationUnitCount} isLoading={ls} />
      <KpiTile to="/position" label="Positions" icon={<Briefcase className="h-4.5 w-4.5" />} total={summary?.positionCount} isLoading={ls} />
      <KpiTile to="/employee" label="On Probation" tone="warning" icon={<Hourglass className="h-4.5 w-4.5" />} total={summary?.probationCount} isLoading={ls} />
      <KpiTile to="/employee" label="Retiring Soon" tone="info" icon={<CalendarClock className="h-4.5 w-4.5" />} total={summary?.retirementCount} isLoading={ls} />
      {isChangeApprover && (
        <KpiTile to="/employee" label="Change Requests" tone="warning" icon={<UserPen className="h-4.5 w-4.5" />} total={changeCount} isLoading={lpc} />
      )}
    </div>
  );
}

export default memo(KpiOverviewWidget);
