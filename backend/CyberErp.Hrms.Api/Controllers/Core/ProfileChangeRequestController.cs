using CyberErp.Hrms.App.Features.Core.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Employee self-service profile change requests + HR review. Not permission-gated like the
    /// admin console: employees submit/track their OWN requests; the HR review/resolve endpoints
    /// gate on the caller's admin scope inside the handlers.
    /// </summary>
    public class ProfileChangeRequestController(
        IGetProfileChangeFields fieldsHandler,
        ISubmitProfileChangeRequest submitHandler,
        IGetMyProfileChangeRequests myHandler,
        IGetPendingProfileChangeRequests pendingHandler,
        IResolveProfileChangeRequest resolveHandler) : BaseController
    {
        /// <summary>The restricted fields the caller may raise a change request for (with current values).</summary>
        [HttpGet("fields")]
        public Task<List<ProfileChangeFieldDto>> Fields() => fieldsHandler.GetAsync();

        /// <summary>The caller's own change requests, newest first.</summary>
        [HttpGet("mine")]
        public Task<List<ProfileChangeRequestDto>> Mine() => myHandler.GetAsync();

        /// <summary>The HR review queue — all pending requests ({isApprover,items}); empty for non-HR.</summary>
        [HttpGet("pending")]
        public Task<ProfileChangeApprovalsDto> Pending() => pendingHandler.GetAsync();

        /// <summary>Employee raises a change request for a restricted field.</summary>
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitProfileChangeRequestDto dto)
            => Ok(new { id = await submitHandler.SubmitAsync(dto) });

        /// <summary>HR approves (auto-applies identity fields) or rejects a request.</summary>
        [HttpPost("{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveProfileChangeRequestDto dto)
        {
            await resolveHandler.ResolveAsync(id, dto);
            return Ok(new { message = "Request updated." });
        }
    }
}
