using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Company-asset registry + exit recovery checklists (HC214/HC215). HR manages the registry;
    /// employees see their own assignments; recovery lines resolve as items come back (or are waived).
    /// </summary>
    public class CompanyAssetController(
        ISaveCompanyAsset saveHandler,
        IDeleteCompanyAsset deleteHandler,
        IAssignCompanyAsset assignHandler,
        IReturnCompanyAsset returnHandler,
        IGetAllCompanyAssets getAllHandler,
        IGetAssetRecoveries recoveriesHandler,
        IResolveAssetRecovery resolveHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<CompanyAssetDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveCompanyAssetDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveCompanyAssetDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPost("{id:guid}/assign/{employeeId:guid}")]
        public async Task<IActionResult> Assign(Guid id, Guid employeeId)
        { await assignHandler.AssignAsync(id, employeeId); return Ok(new { message = "Assigned" }); }

        [HttpPost("{id:guid}/return")]
        public async Task<IActionResult> Return(Guid id) { await returnHandler.ReturnAsync(id); return Ok(new { message = "Returned" }); }

        /// <summary>The exit case's asset-recovery checklist (HC215).</summary>
        [HttpGet("recoveries/{terminationId:guid}")]
        public Task<List<AssetRecoveryDto>> Recoveries(Guid terminationId) => recoveriesHandler.GetAsync(terminationId);

        /// <summary>Ticks one checklist item: Recover (asset returns to the pool) or Waive (written off).</summary>
        [HttpPost("recoveries/{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveAssetRecoveryDto dto)
        { await resolveHandler.ResolveAsync(id, dto); return Ok(new { message = "Updated" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }
}
