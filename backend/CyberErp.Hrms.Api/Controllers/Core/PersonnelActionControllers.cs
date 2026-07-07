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
        IExecuteEmployeeMovement executeHandler,
        ICancelEmployeeMovement cancelHandler,
        IDeleteEmployeeMovement deleteHandler) : BaseController
    {
        /// <summary>Movement history (all types) for one employee, newest first.</summary>
        [HttpGet]
        public Task<List<EmployeeMovementDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveEmployeeMovementDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeMovementDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        /// <summary>Applies the movement to the employee master (position/grade/salary + vacancy sync).</summary>
        [HttpPost("{id:guid}/execute")]
        public async Task<IActionResult> Execute(Guid id)
        {
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

    /// <summary>Disciplinary case records per employee.</summary>
    public class DisciplinaryMeasureController(
        ISaveDisciplinaryMeasure saveHandler,
        IGetDisciplinaryMeasures getHandler,
        IDeleteDisciplinaryMeasure deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<List<DisciplinaryMeasureDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

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
