using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.JobCategories;
using CyberErp.Hrms.App.Features.Core.JobCategories.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class JobCategoryController(
        ICreateJobCategory createHandler,
        IUpdateJobCategory updateHandler,
        IDeleteJobCategory deleteHandler,
        IGetJobCategoryById getByIdHandler,
        IGetAllJobCategories getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<JobCategoryDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<JobCategoryDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateJobCategoryDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateJobCategoryDto dto)
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
