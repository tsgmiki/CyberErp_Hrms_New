using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Features.Core.Dashboard;
using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>The home dashboard's aggregated KPI/workflow-stats/watchlist-count read.</summary>
    public class DashboardController(
        IDashboardSummary summary,
        IGetOverdueLeaveReturns overdueLeaveReturns) : BaseController
    {
        /// <summary>GET api/v1/Dashboard/summary — one round trip for the whole KPI row.</summary>
        [HttpGet("summary")]
        public Task<DashboardSummaryDto> Summary() => summary.GetAsync();

        /// <summary>
        /// Employees whose approved leave has ended without a recorded return.
        /// </summary>
        /// <remarks>
        /// ⚠️ GATED, unlike the summary beside it. The summary returns counts; this returns NAMES —
        /// who is absent, and for how long. That is employee-level data and is gated on the leave
        /// privileges, matching <c>LeaveBalanceController</c> (logic §12.106).
        /// </remarks>
        [RequirePermission("annualLeave", "otherLeave")]
        [HttpGet("overdue-leave-returns")]
        public Task<List<OverdueLeaveReturnDto>> OverdueLeaveReturns() => overdueLeaveReturns.GetAsync();
    }
}
