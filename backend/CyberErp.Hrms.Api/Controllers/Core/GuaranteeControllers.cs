using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Guarantees;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// §3.12 Employee Guarantee Commitment Management (HC305–HC307): commitments the employee holds
    /// toward EXTERNAL organizations per NBE guarantee procedures. Privacy is scope-based (like staff
    /// loans): non-admin callers only ever see and manage their OWN commitments; releases are HR-only.
    /// </summary>
    public class EmployeeGuaranteeController(
        ISaveEmployeeGuarantee saveHandler,
        IDeleteEmployeeGuarantee deleteHandler,
        IGetEmployeeGuaranteeById getByIdHandler,
        IGetAllEmployeeGuarantees getAllHandler,
        IReleaseEmployeeGuarantee releaseHandler,
        IGetGuaranteeDashboard dashboardHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<EmployeeGuaranteeDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        /// <summary>The caller's OWN commitments (self-service) — own slice even for admins.</summary>
        [HttpGet("mine")]
        public Task<PaginatedResponse<EmployeeGuaranteeDto>> Mine([FromQuery] GetAllRequest request) => getAllHandler.GetMineAsync(request);

        [HttpGet("{id:guid}")] public Task<EmployeeGuaranteeDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>HC307 — headline chips for the guarantee dashboard (own slice for non-admins).</summary>
        [HttpGet("dashboard")]
        public Task<GuaranteeDashboardDto> Dashboard() => dashboardHandler.GetAsync();

        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveEmployeeGuaranteeDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveEmployeeGuaranteeDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        /// <summary>HR discharges an active commitment once the external obligation ends.</summary>
        [HttpPost("{id:guid}/release")]
        public async Task<IActionResult> Release(Guid id, [FromBody] ReleaseEmployeeGuaranteeDto dto)
        {
            dto.Id = id;
            await releaseHandler.ReleaseAsync(dto);
            return Ok(new { message = "Released" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
