using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Delegations;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Approval delegation — lending one approver's authority to another for a bounded period.
    /// </summary>
    /// <remarks>
    /// ⚠️ Gated on the delegation screen, EXCEPT the two self-service reads below. An approver has
    /// to be able to see and arrange their own stand-in without holding the administrative
    /// privilege for everybody else's — that is the whole point of self-service delegation, and
    /// requiring the admin permission would mean only HR could ever go on leave cleanly.
    /// </remarks>
    [RequirePermission("approvalDelegation")]
    public class ApprovalDelegationController(
        ISaveApprovalDelegation saveHandler,
        IRevokeApprovalDelegation revokeHandler,
        IGetApprovalDelegations getAllHandler,
        IGetMyDelegations myHandler,
        ICheckDelegationEligibility eligibilityHandler,
        IGetDelegationPolicy getPolicyHandler,
        IGetMyDelegationScope myScopeHandler,
        ISaveDelegationPolicy savePolicyHandler) : BaseController
    {
        /// <summary>Every delegation in the tenant — the HR administration screen.</summary>
        [HttpGet]
        public Task<PaginatedResponse<ApprovalDelegationDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>
        /// The caller's own delegations, both directions — what they lent and what they hold.
        /// </summary>
        [HttpGet("mine")]
        [SelfScoped]
        public Task<List<ApprovalDelegationDto>> Mine() => myHandler.GetAsync();

        /// <summary>
        /// Would this person be allowed to stand in? Drives the live check on the form, so the
        /// answer arrives before somebody commits rather than as a save failure.
        /// </summary>
        [HttpGet("eligibility")]
        [SelfScoped]
        public Task<DelegationEligibilityDto> Eligibility(
            [FromQuery] Guid fromEmployeeId, [FromQuery] Guid toEmployeeId)
            => eligibilityHandler.CheckAsync(fromEmployeeId, toEmployeeId);

        /// <summary>
        /// Create or amend. The handler decides who may: your own authority, or anyone's if HR.
        /// </summary>
        [HttpPost]
        [SelfScoped]
        public async Task<IActionResult> Save([FromBody] SaveApprovalDelegationDto dto)
            => Ok(new { id = await saveHandler.SaveAsync(dto) });

        /// <summary>Withdraw a delegation early. Terminal.</summary>
        [HttpPut("revoke")]
        [SelfScoped]
        public async Task<IActionResult> Revoke([FromBody] RevokeApprovalDelegationDto dto)
        {
            await revokeHandler.RevokeAsync(dto);
            return Ok(new { message = "Delegation withdrawn" });
        }

        /// <summary>
        /// Whether the caller has anyone they could delegate to — the portal sidebar's probe.
        /// </summary>
        [HttpGet("my-scope")]
        [SelfScoped]
        public Task<MyDelegationScopeDto> MyScope() => myScopeHandler.GetAsync();

        /// <summary>The tenant's eligibility rules.</summary>
        [HttpGet("policy")]
        public Task<DelegationPolicyDto> GetPolicy() => getPolicyHandler.GetAsync();

        [HttpPut("policy")]
        public async Task<IActionResult> SavePolicy([FromBody] DelegationPolicyDto dto)
        {
            await savePolicyHandler.SaveAsync(dto);
            return Ok(new { message = "Delegation policy saved" });
        }
    }
}
