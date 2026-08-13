using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.OrganizationUnits;
using CyberErp.Hrms.App.Features.Core.OrganizationUnits.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class OrganizationUnitController(
        ICreateOrganizationUnit createHandler,
        IUpdateOrganizationUnit updateHandler,
        IDeleteOrganizationUnit deleteHandler,
        IGetOrganizationUnitById getByIdHandler,
        IGetAllOrganizationUnits getAllHandler,
        IGetMyOrganizationUnits myUnitsHandler,
        IGetOrganizationTree getTreeHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<OrganizationUnitDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>The units the caller may act for — admin=all, manager=own subtree, else none (self-service).</summary>
        [HttpGet("my-units")]
        public Task<PaginatedResponse<OrganizationUnitDto>> MyUnits([FromQuery] GetAllRequest request)
            => myUnitsHandler.GetAsync(request);

        [HttpGet("tree")]
        public Task<List<OrgUnitTreeNodeDto>> GetTree()
            => getTreeHandler.GetAsync();

        [HttpGet("{id:guid}")]
        public Task<OrganizationUnitDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]

        [RequirePermission("organizationUnit")]
        public Task<Guid> Create([FromBody] CreateOrganizationUnitDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]

        [RequirePermission("organizationUnit")]
        public async Task<IActionResult> Update([FromBody] UpdateOrganizationUnitDto dto)
        {
            await updateHandler.UpdateAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id:guid}")]

        [RequirePermission("organizationUnit")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
