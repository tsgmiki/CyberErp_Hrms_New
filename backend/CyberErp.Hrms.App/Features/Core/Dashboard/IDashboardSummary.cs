namespace CyberErp.Hrms.App.Features.Core.Dashboard
{
    /// <summary>
    /// The header KPI + workflow-stats + watchlist-count payload for the HRMS home dashboard —
    /// everything a first paint needs EXCEPT row-level list data (recent activity, recent workflows,
    /// and the personal action queues), which stay as their own lightweight, independently-cached
    /// endpoints so a single slow list can never block the numbers.
    /// </summary>
    public class DashboardSummaryDto
    {
        public int BranchCount { get; set; }
        public int OrganizationUnitCount { get; set; }
        public int PositionCount { get; set; }
        public int EmployeeCount { get; set; }

        public int WorkflowRunning { get; set; }
        public int WorkflowApproved { get; set; }
        public int WorkflowRejected { get; set; }

        public int ProbationCount { get; set; }
        public int RetirementCount { get; set; }
    }

    /// <summary>
    /// One aggregated read for the dashboard's KPI row: nine COUNT-shaped numbers in a SINGLE
    /// database round trip (see <c>DashboardSummaryService</c> in Inf, which implements this with
    /// Dapper's <c>QueryMultipleAsync</c> over the same connection EF already holds open) instead of
    /// the four separate paginated "GetAll?take=1" list calls + a workflow-stats call + two watchlist
    /// list calls the dashboard used to issue just to read totals.
    /// </summary>
    public interface IDashboardSummary
    {
        Task<DashboardSummaryDto> GetAsync();
    }
}
