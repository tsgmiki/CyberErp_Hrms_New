using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class CalculateLedgerRequest
    {
        public Guid SettingId { get; set; }
    }

    /// <summary>
    /// Annual Leave Ledger: previews and generates service-length-based leave entitlements for all
    /// eligible employees under a selected annual-leave setting (fiscal year + leave type).
    /// </summary>
    public class AnnualLeaveLedgerController(
        IGetAnnualLeaveLedger getHandler,
        ILeaveAccrualService accrualService) : BaseController
    {
        /// <summary>The ledger grid for one setting: every active employee's calculated entitlement + persisted balance.</summary>
        [HttpGet]
        public Task<AnnualLeaveLedgerDto> Get([FromQuery] Guid settingId)
            => getHandler.GetAsync(settingId);

        /// <summary>Generates (persists) the ledger entitlements for all eligible employees. Idempotent.</summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CalculateLedgerRequest request)
        {
            var count = await accrualService.GenerateEntitlementsAsync(request.SettingId);
            return Ok(new { count, message = $"{count} employee ledger entr{(count == 1 ? "y" : "ies")} generated." });
        }
    }
}
