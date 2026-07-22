using CyberErp.Hrms.App.Features.Core.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Personnel actions (SAP-style movement log): transfers, promotions and demotions.
    /// Actions are recorded as Pending, then executed against the employee master (or cancelled).
    /// </summary>
    public class EmployeeMovementController(
        ISaveEmployeeMovement saveHandler,
        IGetEmployeeMovements getHandler,
        IGetAllEmployeeMovements getAllHandler,
        IAssessEmployeeTransfer assessHandler,
        IGetEmployeeMovementById getByIdHandler,
        IExecuteEmployeeMovement executeHandler,
        IExecuteDueMovements dueHandler,
        ICancelEmployeeMovement cancelHandler,
        IDeleteEmployeeMovement deleteHandler,
        CyberErp.Hrms.App.Features.Core.Performance.IPerformanceVisibilityService visibility) : BaseController
    {
        /// <summary>Movement history (all types) for one employee, newest first.</summary>
        [HttpGet]
        public Task<List<EmployeeMovementDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

        [HttpGet("{id:guid}")]
        public Task<EmployeeMovementDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>Transfer eligibility + impact assessment (HC173) — advisory, read-only.</summary>
        [HttpGet("assess")]
        public Task<TransferAssessmentDto> Assess([FromQuery] Guid employeeId, [FromQuery] Guid toPositionId)
            => assessHandler.AssessAsync(employeeId, toPositionId);

        /// <summary>Standalone paged list, role-scoped (admin all / manager subtree / employee own) — the
        /// Transfer Requests screen (HC170).</summary>
        [HttpGet("paged")]
        public Task<CyberErp.Hrms.App.Common.DTOs.PaginatedResponse<EmployeeMovementDto>> GetPaged(
            [FromQuery] CyberErp.Hrms.App.Common.DTOs.GetAllRequest request) => getAllHandler.GetAsync(request);

        /// <summary>Manual trigger of the due-movement sweep (the daily scheduler runs the same handler) — HR admin only.</summary>
        [HttpPost("execute-due")]
        public async Task<IActionResult> ExecuteDue()
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin)
                return BadRequest(new { message = "Only an HR administrator can run the due-movement sweep." });
            var executed = await dueHandler.ExecuteAsync();
            return Ok(new { message = $"{executed} due movement(s) executed" });
        }

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveEmployeeMovementDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeMovementDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        /// <summary>Applies the movement to the employee master (position/grade/salary + vacancy sync).
        /// Manual execution is a manager/HR action — employees cannot fast-forward their own approved,
        /// future-dated transfer (the workflow and the daily scheduler call the handler directly).</summary>
        [HttpPost("{id:guid}/execute")]
        public async Task<IActionResult> Execute(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && !scope.IsManager)
                return BadRequest(new { message = "Only a manager or HR administrator can execute a movement manually." });
            await executeHandler.ExecuteAsync(id);
            return Ok(new { message = "Movement executed" });
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await cancelHandler.CancelAsync(id);
            return Ok(new { message = "Movement cancelled" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    /// <summary>Disciplinary case records per employee, plus the standalone role-scoped case list.</summary>
    public class DisciplinaryMeasureController(
        ISaveDisciplinaryMeasure saveHandler,
        IGetDisciplinaryMeasures getHandler,
        IGetDisciplinaryMeasureById getByIdHandler,
        IGetDisciplinaryCases casesHandler,
        IGetDisciplinaryEligibility eligibilityHandler,
        IDeleteDisciplinaryMeasure deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<List<DisciplinaryMeasureDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

        /// <summary>Single case (for the standalone case form), visibility-scoped.</summary>
        [HttpGet("{id:guid}")]
        public Task<DisciplinaryMeasureDto?> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>HC222/HC225 — standalone paged, role-scoped disciplinary case list (work-unit intake).</summary>
        [HttpGet("paged")]
        public Task<CyberErp.Hrms.App.Common.DTOs.PaginatedResponse<DisciplinaryCaseDto>> GetCases(
            [FromQuery] CyberErp.Hrms.App.Common.DTOs.GetAllRequest request)
            => casesHandler.GetAsync(request);

        /// <summary>HC224/HC225 — promotion/reward eligibility snapshot from active disciplinary measures.</summary>
        [HttpGet("eligibility")]
        public Task<DisciplinaryEligibilityDto> GetEligibility([FromQuery] Guid employeeId)
            => eligibilityHandler.GetAsync(employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveDisciplinaryMeasureDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveDisciplinaryMeasureDto dto)
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
    }
}
