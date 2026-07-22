using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Fiscal years (Core.FiscalYear) — anchor for leave balances, accrual and rollover.</summary>
    public class FiscalYearController(
        ISaveFiscalYear saveHandler,
        IGetFiscalYearById getByIdHandler,
        IGetAllFiscalYears getAllHandler,
        IDeleteFiscalYear deleteHandler,
        ILeaveAccrualService accrualService) : BaseController
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

        /// <summary>
        /// Year-end rollover: carries remaining balances into the following fiscal year (respecting
        /// carry-forward caps), expires over-aged carry, and closes this year.
        /// </summary>
        [HttpPost("{id:guid}/rollover-leave")]
        public async Task<IActionResult> RolloverLeave(Guid id)
        {
            var result = await accrualService.RolloverAsync(id);
            return Ok(new
            {
                message = $"Rolled {result.BalancesRolled} balance(s): {result.TotalCarried} day(s) carried, {result.TotalExpired} expired.",
                result.BalancesRolled,
                result.TotalCarried,
                result.TotalExpired
            });
        }
    }

    /// <summary>Annual-leave accrual policy per fiscal year (successor of legacy hrmsAnnualLeaveSetting).</summary>
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
    }
}
