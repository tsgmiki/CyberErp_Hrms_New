using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Positions;
using CyberErp.Hrms.App.Features.Core.Positions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class PositionController(
        ICreatePosition createHandler,
        IUpdatePosition updateHandler,
        IDeletePosition deleteHandler,
        IGetPositionById getByIdHandler,
        IGetAllPositions getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<PositionDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<PositionDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreatePositionDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePositionDto dto)
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
