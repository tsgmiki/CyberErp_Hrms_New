using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Other (non-annual) leave: STATIC position-based entitlements per fiscal year — no accrual,
    /// gender-aware (maternity/paternity), lump-sum types taken in one block, and never charged
    /// against the annual-leave ledger. Approval rides the SAME workflow mechanism as Annual Leave.
    /// </summary>
    public class OtherLeaveController(
        ISubmitOtherLeave submitHandler,
        ICancelOtherLeave cancelHandler,
        IGetOtherLeaveById getByIdHandler,
        IGetAllOtherLeaves getAllHandler,
        IGetOtherLeaveBalances balancesHandler,
        IGetLumpSumEndDate lumpSumHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<OtherLeaveHeaderDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>The employee's selectable entitlements for the ACTIVE fiscal year (gender-filtered).</summary>
        [HttpGet("balances/{employeeId:guid}")]
        public Task<List<OtherLeaveBalanceDto>> Balances(Guid employeeId) => balancesHandler.GetAsync(employeeId);

        /// <summary>Computed end date of a lump-sum block (allocation working days from start).</summary>
        [HttpGet("lump-sum-end")]
        public Task<LumpSumEndDto> LumpSumEnd([FromQuery] Guid employeeId, [FromQuery] Guid otherLeaveSettingId, [FromQuery] DateTime startDate)
            => lumpSumHandler.GetAsync(employeeId, otherLeaveSettingId, startDate);

        [HttpGet("{id:guid}")]
        public Task<OtherLeaveHeaderDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveOtherLeaveDto dto) => submitHandler.SubmitAsync(dto);

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelOtherLeaveDto dto)
        {
            await cancelHandler.CancelAsync(dto);
            return Ok(new { message = "Other leave request cancelled" });
        }
    }

    /// <summary>Per-fiscal-year policy rows for the non-annual leave types (hrmsOtherLeaveSetting).</summary>
    public class OtherLeaveSettingController(
        ISaveOtherLeaveSetting saveHandler,
        IDeleteOtherLeaveSetting deleteHandler,
        IGetOtherLeaveSettingById getByIdHandler,
        IGetAllOtherLeaveSettings getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<OtherLeaveSettingDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<OtherLeaveSettingDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveOtherLeaveSettingDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveOtherLeaveSettingDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
