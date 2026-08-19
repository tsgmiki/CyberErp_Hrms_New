using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Leave requests (HC034–HC039). Approvals are driven by the generic workflow engine.</summary>
    // Gated on the leave screens rather than on a link of its own: there is NO leaveRequest operation
    // in Core.TenantOperation, and gating on a link nobody can hold denies everyone. Its own page
    // (/leaveRequest) is consequently absent from every menu, so the grants that matter are the leave
    // screens an employee actually holds.
    [RequirePermission("annualLeave", "otherLeave")]
    public class LeaveRequestController(
        ISubmitLeaveRequest submitHandler,
        ICancelLeaveRequest cancelHandler,
        IGetLeaveRequestById getByIdHandler,
        IGetAllLeaveRequests getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<LeaveRequestDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<LeaveRequestDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveLeaveRequestDto dto) => submitHandler.SubmitAsync(dto);

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelLeaveRequestDto dto)
        {
            await cancelHandler.CancelAsync(dto);
            return Ok(new { message = "Leave request cancelled" });
        }
    }

    /// <summary>Annual-leave requests (Master-Detail, dedicated to annual leave). Charged against the
    /// employee's annual-leave ledger row (hrms_LeaveBalance); approvals run through the workflow engine.</summary>
    [RequirePermission("annualLeave")]
    public class AnnualLeaveController(
        ISubmitAnnualLeave submitHandler,
        ICancelAnnualLeave cancelHandler,
        IGetAnnualLeaveById getByIdHandler,
        IGetAllAnnualLeaves getAllHandler,
        IGetMyAnnualLeaveBalance myBalanceHandler,
        IConfirmAnnualLeaveReturn confirmReturnHandler,
        IPreviewAnnualLeaveReturn previewReturnHandler,
        IGetAnnualLeaveHistory historyHandler) : BaseController
    {
        /// <summary>The signed-in employee's annual-leave balances across ALL active fiscal years (dashboard widget).</summary>
        [HttpGet("my-balance")]
        public Task<MyAnnualLeaveBalancesDto> MyBalance() => myBalanceHandler.GetAsync();

        [HttpGet]
        public Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>Self-service list — strictly the signed-in employee's own requests (Home portal grid).</summary>
        [HttpGet("mine")]
        public Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetMine([FromQuery] GetAllRequest request)
            => getAllHandler.GetMineAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AnnualLeaveHeaderDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>The full lifecycle — request, approvals, return, adjustment — for the history popup.</summary>
        [HttpGet("{id:guid}/history")]
        public Task<AnnualLeaveHistoryDto> History(Guid id) => historyHandler.GetAsync(id);

        /// <summary>What confirming this return date would do, without committing to it.</summary>
        [HttpGet("{id:guid}/return-preview")]
        public Task<AnnualLeaveReturnPreviewDto> PreviewReturn(Guid id, [FromQuery] DateTime actualEndDate)
            => previewReturnHandler.PreviewAsync(id, actualEndDate);

        /// <summary>Employee confirms they are back. On time settles; early/late go for approval.</summary>
        [HttpPost("confirm-return")]
        public Task<AnnualLeaveReturnResultDto> ConfirmReturn([FromBody] ConfirmAnnualLeaveReturnDto dto)
            => confirmReturnHandler.ConfirmAsync(dto);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveAnnualLeaveDto dto) => submitHandler.SubmitAsync(dto);

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelAnnualLeaveDto dto)
        {
            await cancelHandler.CancelAsync(dto);
            return Ok(new { message = "Annual leave request cancelled" });
        }
    }

    /// <summary>Leave balances (HC033): view per employee, set opening figures / adjust.</summary>
    // Same reasoning as LeaveRequestController, and it is not only its own screen: the ANNUAL LEAVE
    // form in BOTH applications reads balances from here, so the annualLeave grant is the one that has
    // to open it.
    [RequirePermission("annualLeave", "otherLeave")]
    public class LeaveBalanceController(
        IGetLeaveBalances getHandler,
        ISetLeaveBalance setHandler) : BaseController
    {
        [HttpGet]
        public Task<List<LeaveBalanceDto>> GetByEmployee([FromQuery] Guid employeeId, [FromQuery] Guid? fiscalYearId)
            => getHandler.GetAsync(employeeId, fiscalYearId);

        [HttpPost]
        public async Task<IActionResult> Set([FromBody] SetLeaveBalanceDto dto)
        {
            await setHandler.SetAsync(dto);
            return Ok(new { message = "Leave balance saved" });
        }
    }
}
