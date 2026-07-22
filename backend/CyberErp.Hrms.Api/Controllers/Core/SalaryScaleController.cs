using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.SalaryScales;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Salary scale (coreSalaryScale). The list is always filtered by <c>jobGradeId</c>.</summary>
    [RequirePermission("salaryScale", "employee", "position", "positionClass", "jobApplication", "jobRequisition")]
    public class SalaryScaleController(
        ISaveSalaryScale saveHandler,
        IGetSalaryScaleById getByIdHandler,
        IGetAllSalaryScales getAllHandler,
        IDeleteSalaryScale deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<SalaryScaleDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<SalaryScaleDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveSalaryScaleDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveSalaryScaleDto dto)
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
