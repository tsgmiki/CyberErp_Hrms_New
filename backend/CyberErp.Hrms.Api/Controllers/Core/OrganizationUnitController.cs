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
        IGetOrganizationTree getTreeHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<OrganizationUnitDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("tree")]
        public Task<List<OrgUnitTreeNodeDto>> GetTree()
            => getTreeHandler.GetAsync();

        [HttpGet("{id:guid}")]
        public Task<OrganizationUnitDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateOrganizationUnitDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateOrganizationUnitDto dto)
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
