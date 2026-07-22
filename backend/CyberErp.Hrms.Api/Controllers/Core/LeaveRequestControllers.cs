using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Leave requests (HC034–HC039). Approvals are driven by the generic workflow engine.</summary>
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
    public class AnnualLeaveController(
        ISubmitAnnualLeave submitHandler,
        ICancelAnnualLeave cancelHandler,
        IGetAnnualLeaveById getByIdHandler,
        IGetAllAnnualLeaves getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AnnualLeaveHeaderDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AnnualLeaveHeaderDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

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
