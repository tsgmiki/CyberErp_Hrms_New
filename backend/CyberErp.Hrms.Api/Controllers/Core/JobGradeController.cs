using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.JobGrades;
using CyberErp.Hrms.App.Features.Core.JobGrades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class JobGradeController(
        ICreateJobGrade createHandler,
        IUpdateJobGrade updateHandler,
        IDeleteJobGrade deleteHandler,
        IGetJobGradeById getByIdHandler,
        IGetAllJobGrades getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<JobGradeDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<JobGradeDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateJobGradeDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateJobGradeDto dto)
        {
            await updateHandler.UpdateAsync(dto);
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
