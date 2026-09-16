using CyberErp.Hrms.App.Common.Authorization;
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
    [RequirePermission("annualLeaveLedger")]
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

        /// <summary>
        /// Re-applies the policy to entitlements that have ALREADY been generated, correcting the ones
        /// the current rules compute differently.
        /// </summary>
        /// <remarks>
        /// ⚠️ Separate from <c>calculate</c>, which skips anyone who already has a balance and so can
        /// never repair a figure produced by an earlier version of the rules. Carried-forward, adjusted
        /// and taken days are preserved; every change posts an Adjustment to the ledger (logic §12.94).
        /// </remarks>
        [RequirePermission("annualLeaveLedger", Access = PermissionAccess.Edit)]
        [HttpPost("recalculate")]
        public async Task<IActionResult> Recalculate([FromBody] CalculateLedgerRequest request)
        {
            var r = await accrualService.RecalculateEntitlementsAsync(request.SettingId);
            var message = r.Raised + r.Lowered + r.Created == 0
                ? "Every entitlement already matches the policy — nothing changed."
                : $"{r.Raised} raised, {r.Lowered} lowered, {r.Created} newly generated ({r.NetChange:+0.##;-0.##;0} day(s) net).";
            if (r.OverTaken.Count > 0)
                message += $" ⚠️ {r.OverTaken.Count} employee(s) have now taken more than they are entitled to: {string.Join(", ", r.OverTaken)}.";
            return Ok(new { r.Examined, r.Raised, r.Lowered, r.Created, r.NetChange, r.OverTaken, message });
        }
    }
}
