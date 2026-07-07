using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Leave type configuration (HC030).</summary>
    public class LeaveTypeController(
        ISaveLeaveType saveHandler,
        IGetLeaveTypeById getByIdHandler,
        IGetAllLeaveTypes getAllHandler,
        IDeleteLeaveType deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<LeaveTypeDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<LeaveTypeDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveLeaveTypeDto dto) => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveLeaveTypeDto dto)
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

    /// <summary>Public/holiday calendar (HC040), including the working-day calculator.</summary>
    public class HolidayController(
        ISaveHoliday saveHandler,
        IGetHolidayById getByIdHandler,
        IGetAllHolidays getAllHandler,
        IDeleteHoliday deleteHandler,
        IGetWorkingDays workingDaysHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<HolidayDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<HolidayDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveHolidayDto dto) => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveHolidayDto dto)
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

        /// <summary>Working days between two dates, excluding weekends and holidays (HC040).</summary>
        [HttpGet("working-days")]
        public Task<WorkingDaysDto> WorkingDays([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] bool halfDay = false)
            => workingDaysHandler.GetAsync(start, end, halfDay);
    }
}
