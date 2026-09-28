using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Features.Core.Delegations;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// What a POST carries beyond its salary — the allowances and benefits attached to the job
    /// rather than to whoever currently holds it.
    /// </summary>
    /// <remarks>
    /// ⚠️ Gated on <c>positionClass</c>, the screen that defines the job, and NOT on
    /// <c>approvalDelegation</c>. This is establishment data: it says what a post is worth. Acting
    /// compensation happens to read it, but anyone who may edit a job definition may set its
    /// entitlements, and arranging a stand-in must not confer the right to change what a post pays.
    /// </remarks>
    [RequirePermission("positionClass")]
    public class PositionEntitlementController(
        ISavePositionEntitlement saveHandler,
        IDeletePositionEntitlement deleteHandler,
        IGetPositionEntitlements getHandler) : BaseController
    {
        /// <summary>Everything one post class carries.</summary>
        [HttpGet]
        public Task<List<PositionEntitlementDto>> GetAll([FromQuery] Guid positionClassId)
            => getHandler.GetAsync(positionClassId);

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SavePositionEntitlementDto dto)
            => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Entitlement removed" });
        }
    }
}
