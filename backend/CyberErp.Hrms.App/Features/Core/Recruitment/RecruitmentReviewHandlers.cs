using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    public interface IGetHiringRequestForApproval { Task<HiringRequestDto> GetAsync(Guid id); }
    public interface IGetJobRequisitionForApproval { Task<JobRequisitionDto> GetAsync(Guid id); }

    /// <summary>
    /// The request as its assigned APPROVER may read it, so a decision is never taken blind.
    ///
    /// <para>⚠️ BEING THE APPROVER IS THE ENTITLEMENT. An approver is routed the request by the
    /// workflow and has no reason to hold the recruitment screen's menu permission — the chain runs
    /// Immediate Manager → HR → Finance, and the Finance Directorate's manager holds neither
    /// <c>hiringRequest</c> nor <c>jobRequisition</c>. Gating this read on those operations would
    /// force exactly the people who must decide to decide without seeing what they are deciding on,
    /// and the failure would surface as an empty panel rather than a refusal (logic §12.68).</para>
    ///
    /// <para>Same shape as <c>GetSalaryRevisionForApproval</c>, which established this: the
    /// controller action clears the class-level gate with a bare <c>[RequirePermission]</c> and the
    /// handler authorises instead — the module's own audience, or the approver it is currently
    /// routed to, and nobody else.</para>
    /// </summary>
    public class GetHiringRequestForApproval(
        IGetHiringRequestById inner,
        IEndpointPermissionService permissions,
        IWorkflowService workflowService,
        IWorkflowApproverAuth approverAuth) : IGetHiringRequestForApproval
    {
        // ⚠️ THE OWNING SCREEN ONLY, not the whole recruitment cluster. The controller gate lists
        // several links because those screens cross-reference each other; this shortcut exists purely
        // to spare recruitment staff the workflow lookup, so widening it past the screen that owns
        // the record just hands the record to a larger audience for no gain. The approver branch
        // below is what everyone else goes through.
        private static readonly string[] OwningScreen = ["hiringRequest"];

        public async Task<HiringRequestDto> GetAsync(Guid id)
        {
            if (!await permissions.HasAnyAsync(OwningScreen)
                && !await RecruitmentReviewAccess.IsCurrentApproverAsync(
                       workflowService, approverAuth, WorkflowEntityTypes.HiringRequest, id))
                throw new ValidationException("access",
                    "You do not have access to this hiring request. Only recruitment staff and the " +
                    "approver it is currently routed to can review it.");

            return await inner.GetAsync(id);
        }
    }

    /// <summary>The requisition as its assigned approver may read it — see <see cref="GetHiringRequestForApproval"/>.</summary>
    public class GetJobRequisitionForApproval(
        IGetJobRequisitionById inner,
        IEndpointPermissionService permissions,
        IWorkflowService workflowService,
        IWorkflowApproverAuth approverAuth) : IGetJobRequisitionForApproval
    {
        /// <summary>The owning screen only — see the hiring-request twin.</summary>
        private static readonly string[] OwningScreen = ["jobRequisition"];

        public async Task<JobRequisitionDto> GetAsync(Guid id)
        {
            if (!await permissions.HasAnyAsync(OwningScreen)
                && !await RecruitmentReviewAccess.IsCurrentApproverAsync(
                       workflowService, approverAuth, WorkflowEntityTypes.JobRequisition, id))
                throw new ValidationException("access",
                    "You do not have access to this job requisition. Only recruitment staff and the " +
                    "approver it is currently routed to can review it.");

            return await inner.GetAsync(id);
        }
    }

    internal static class RecruitmentReviewAccess
    {
        /// <summary>
        /// Whether the caller can decide the request's CURRENT step — the same evaluation the
        /// approve/reject endpoint runs, so "may I read it" and "may I decide it" cannot drift apart.
        ///
        /// <para>False when nothing is running: a draft or an already-decided request is not open for
        /// review by someone who only ever had approver standing.</para>
        /// </summary>
        internal static async Task<bool> IsCurrentApproverAsync(
            IWorkflowService workflowService, IWorkflowApproverAuth approverAuth,
            string entityType, Guid entityId)
        {
            var instance = await workflowService.GetRunningInstanceAsync(entityType, entityId);
            if (instance is null) return false;
            var (canDecide, _) = await approverAuth.EvaluateAsync(
                instance.DefinitionId, instance.CurrentStepOrder, instance.EmployeeId);
            return canDecide;
        }
    }
}
