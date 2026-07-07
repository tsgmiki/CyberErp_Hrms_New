using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.PositionClasses;
using CyberErp.Hrms.App.Features.Core.PositionClasses.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class PositionClassController(
        ICreatePositionClass createHandler,
        IUpdatePositionClass updateHandler,
        IDeletePositionClass deleteHandler,
        IGetPositionClassById getByIdHandler,
        IGetAllPositionClasses getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<PositionClassDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<PositionClassDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreatePositionClassDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePositionClassDto dto)
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
