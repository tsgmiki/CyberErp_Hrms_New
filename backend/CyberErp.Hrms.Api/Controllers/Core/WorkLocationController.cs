using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.WorkLocations;
using CyberErp.Hrms.App.Features.Core.WorkLocations.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class WorkLocationController(
        ICreateWorkLocation createHandler,
        IUpdateWorkLocation updateHandler,
        IDeleteWorkLocation deleteHandler,
        IGetWorkLocationById getByIdHandler,
        IGetAllWorkLocations getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<WorkLocationDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<WorkLocationDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateWorkLocationDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateWorkLocationDto dto)
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
