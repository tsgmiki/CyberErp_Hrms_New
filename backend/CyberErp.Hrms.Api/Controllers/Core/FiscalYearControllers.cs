using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Fiscal years (Core.FiscalYear) — the anchor for leave balances and accrual. Plain CRUD: the
    /// year-end leave rollover is driven from the leave setting instead (logic §12.97).
    /// </summary>
    [RequirePermission("fiscalYear")]
    public class FiscalYearController(
        ISaveFiscalYear saveHandler,
        IGetFiscalYearById getByIdHandler,
        IGetAllFiscalYears getAllHandler,
        IDeleteFiscalYear deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<FiscalYearDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<FiscalYearDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveFiscalYearDto dto) => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveFiscalYearDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        // ⚠️ THE LEAVE ROLLOVER USED TO LIVE HERE, at POST {id}/rollover-leave. It moved to
        // AnnualLeaveSettingController (logic §12.97) and the endpoint is REMOVED rather than merely
        // unlinked from the UI: it closes a fiscal year and expires people's carried leave, so leaving
        // a second live route to it — reachable by anyone with the URL or a stale SPA build — is worse
        // than having no route at all. Rollover is now reached only through the leave policy that
        // defines the carry cap it applies.
    }

    /// <summary>Annual-leave accrual policy per fiscal year (successor of legacy hrmsAnnualLeaveSetting).</summary>
    [RequirePermission("annualLeaveSetting")]
    public class AnnualLeaveSettingController(
        ISaveAnnualLeaveSetting saveHandler,
        IGetAnnualLeaveSettingById getByIdHandler,
        IGetAllAnnualLeaveSettings getAllHandler,
        IDeleteAnnualLeaveSetting deleteHandler,
        ILeaveAccrualService accrualService) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AnnualLeaveSettingDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AnnualLeaveSettingDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveAnnualLeaveSettingDto dto) => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveAnnualLeaveSettingDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        /// <summary>Generates service-length-based entitlements for all active employees (idempotent).</summary>
        [HttpPost("{id:guid}/generate-entitlements")]
        public async Task<IActionResult> GenerateEntitlements(Guid id)
        {
            var count = await accrualService.GenerateEntitlementsAsync(id);
            return Ok(new { message = $"{count} entitlement(s) generated.", count });
        }

        /// <summary>
        /// Year-end rollover for the fiscal year this policy governs: carries remaining balances into
        /// the following year up to this policy's carry-forward cap, expires over-aged carry, and
        /// closes the year.
        /// </summary>
        /// <remarks>
        /// ⚠️ MOVED HERE FROM THE FISCAL YEAR SCREEN (logic §12.97). The cap it honours
        /// (<c>CarryForwardMaxDays</c>) and the expiry rule are both fields of this policy, so this is
        /// where an operator can see what a rollover is about to do. <paramref name="id"/> is the
        /// SETTING id, not the fiscal year's.
        ///
        /// <para>⚠️ It still CLOSES the fiscal year and it cannot be undone — the destructive confirm
        /// on the grid is load-bearing.</para>
        /// </remarks>
        [RequirePermission("annualLeaveSetting", Access = PermissionAccess.Edit)]
        [HttpPost("{id:guid}/rollover-leave")]
        public async Task<IActionResult> RolloverLeave(Guid id)
        {
            var result = await accrualService.RolloverForSettingAsync(id);
            return Ok(new
            {
                message = $"Rolled {result.BalancesRolled} balance(s): {result.TotalCarried} day(s) carried, {result.TotalExpired} expired.",
                result.BalancesRolled,
                result.TotalCarried,
                result.TotalExpired
            });
        }
    }
}
