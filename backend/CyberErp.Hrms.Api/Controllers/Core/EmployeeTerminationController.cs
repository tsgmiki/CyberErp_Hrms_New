using CyberErp.Hrms.App.Features.Core.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Termination &amp; clearance module: initiation (routed through the workflow engine),
    /// departmental clearance checklist, and final settlement automations.
    /// </summary>
    public class EmployeeTerminationController(
        ISaveEmployeeTermination saveHandler,
        IGetEmployeeTerminations getHandler,
        IUpdateTerminationClearance clearanceHandler,
        IFinalizeEmployeeTermination finalizeHandler,
        ICancelEmployeeTermination cancelHandler,
        IDeleteEmployeeTermination deleteHandler) : BaseController
    {
        /// <summary>Termination cases (with clearance checklist) for one employee, newest first.</summary>
        [HttpGet]
        public Task<List<EmployeeTerminationDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveEmployeeTerminationDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeTerminationDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        /// <summary>Sets a departmental clearance item to Pending / Cleared / Blocked.</summary>
        [HttpPut("clearance")]
        public async Task<IActionResult> UpdateClearance([FromBody] UpdateTerminationClearanceDto dto)
        {
            await clearanceHandler.UpdateAsync(dto);
            return Ok(new { message = "Clearance updated" });
        }

        /// <summary>
        /// Final settlement — requires every clearance to be Cleared. Sets the employee to
        /// Terminated, decouples the position and reopens it (IsVacant = true).
        /// </summary>
        [HttpPost("{id:guid}/finalize")]
        public async Task<IActionResult> Finalize(Guid id)
        {
            await finalizeHandler.FinalizeAsync(id);
            return Ok(new { message = "Termination settled" });
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await cancelHandler.CancelAsync(id);
            return Ok(new { message = "Termination cancelled" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
