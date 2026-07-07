using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.ClearanceDepartments;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Dynamic clearance configuration (offboarding): the departments that make up the termination
    /// clearance checklist, each with its authorized approvers (users and/or roles). Any single
    /// authorized user's approval clears the department.
    /// </summary>
    public class ClearanceDepartmentController(
        ISaveClearanceDepartment saveHandler,
        IGetAllClearanceDepartments getAllHandler,
        IGetClearanceDepartmentById getByIdHandler,
        IDeleteClearanceDepartment deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<ClearanceDepartmentDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<ClearanceDepartmentDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveClearanceDepartmentDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveClearanceDepartmentDto dto)
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
