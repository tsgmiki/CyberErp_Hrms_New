using CyberErp.Hrms.App.Features.Core.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>The home dashboard's aggregated KPI/workflow-stats/watchlist-count read.</summary>
    public class DashboardController(IDashboardSummary summary) : BaseController
    {
        /// <summary>GET api/v1/Dashboard/summary — one round trip for the whole KPI row.</summary>
        [HttpGet("summary")]
        public Task<DashboardSummaryDto> Summary() => summary.GetAsync();
    }
}
