using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.EmployeeFields;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Dynamic employee field definitions (HC021).</summary>
    public class EmployeeFieldController(
        ICreateEmployeeField createHandler,
        IUpdateEmployeeField updateHandler,
        IDeleteEmployeeField deleteHandler,
        IGetEmployeeFieldById getByIdHandler,
        IGetAllEmployeeFields getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<EmployeeFieldDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<EmployeeFieldDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateEmployeeFieldDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEmployeeFieldDto dto)
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
