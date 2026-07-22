using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    /// <summary>The current user's relationship to an appraisal + whether they may act on the current stage.</summary>
    public class AppraisalActorContext
    {
        /// <summary>"Actor" (it is the caller's turn) | "Observer" | "Open" (no workflow configured).</summary>
        public string CurrentUserRole { get; set; } = "Observer";
        public bool CanActCurrentStage { get; set; }
        /// <summary>Display name(s) of whoever the current stage is routed to (for "Awaiting …" hints).</summary>
        public string? CurrentStageActorName { get; set; }
        /// <summary>The viewer is the employee being appraised — drives what they may see (no peer detail).</summary>
        public bool ViewerIsAppraisee { get; set; }
    }

    /// <summary>
    /// Routes the appraisal through the tenant's EXISTING dynamic workflow engine. Each appraisal owns a
    /// <see cref="WorkflowInstance"/> (EntityType "Appraisal", subject = the appraisee) whose current step mirrors
    /// the appraisal stage; per-stage authorization comes from the configured approvers (Subject / ImmediateManager
    /// / SecondLevelManager / Role / …) via <see cref="IWorkflowApproverAuth"/>. The appraisal's rich per-stage
    /// actions (score / sign / complete) run in the appraisal handlers, which drive the instance in lockstep.
    /// Step names in the Appraisal definition MUST equal the <see cref="AppraisalStage"/> values.
    /// </summary>
    public interface IAppraisalWorkflowService
    {
        /// <summary>DTO fields for the scoring screen: whose turn it is + whether the caller may act now.</summary>
        Task<AppraisalActorContext> ResolveAsync(Appraisal appraisal);
        /// <summary>Throws a 400 unless the caller is the configured approver for the CURRENT stage. Call BEFORE the mutation.</summary>
        Task EnsureCanActAsync(Appraisal appraisal);
        /// <summary>Whether the caller is the configured approver for a NAMED stage (for side-actions like manager counter-sign).</summary>
        Task<bool> CanActOnStageAsync(Appraisal appraisal, string stageName);
        /// <summary>After the appraisal's stage has advanced, move the routing instance to match (or complete it). Call AFTER the mutation.</summary>
        Task SyncInstanceAsync(Appraisal appraisal, string? comment);
        /// <summary>Appraisal ids the current user must act on now, from the engine (the shared "pending my action" worklist).</summary>
        Task<HashSet<Guid>> PendingMyActionAppraisalIdsAsync();

        // ---- Role-based visibility (delegates to IPerformanceVisibilityService's per-request scope) ----
        /// <summary>The current user's linked employee id (null for unlinked/system accounts).</summary>
        Task<Guid?> CurrentEmployeeIdAsync();
        /// <summary>HR administrator: a head-office user, or an explicit User/Role approver configured on the
        /// Appraisal definition's HrSignOff step. An OPEN HrSignOff step does NOT make everyone an admin.</summary>
        Task<bool> CanAdministerAsync();
        /// <summary>The caller is a manager whose unit subtree contains the given employee (never true for self).</summary>
        Task<bool> CanManageEmployeeAsync(Guid employeeId);
        /// <summary>Throws a 400 unless the caller may VIEW the appraisal (appraisee / their manager / HR admin).</summary>
        Task EnsureCanViewAsync(Appraisal appraisal);
    }

    public class AppraisalWorkflowService(
        IWorkflowService workflowService,
        IWorkflowApproverAuth approverAuth,
        IRepository<WorkflowInstance> instances,
        IRepository<WorkflowDefinition> definitions,
        IPerformanceVisibilityService visibility) : IAppraisalWorkflowService
    {
        /// <summary>
        /// The default Appraisal routing definition — steps named EXACTLY after the <see cref="AppraisalStage"/>
        /// values (the lockstep key), routed to the subject / manager / second-level / subject, with HR sign-off
        /// left open for an admin to attach the HR role. Shared by the workflow seed and the generate-time
        /// auto-ensure so every appraisal is always workflow-governed (no "open" gating gaps).
        /// </summary>
        public static WorkflowDefinition BuildDefaultDefinition()
        {
            var def = WorkflowDefinition.Create("Appraisal Routing", WorkflowEntityTypes.Appraisal,
                "Collaborative appraisal routing (self → manager → 2nd-level → employee sign-off → HR)");
            def.SetSteps(new[]
            {
                new WorkflowStepSpec("SelfAssessment", null, [new WorkflowApproverSpec(WorkflowApproverType.Subject, Guid.Empty, "Employee (self)")]),
                new WorkflowStepSpec("ManagerReview", null, [new WorkflowApproverSpec(WorkflowApproverType.ImmediateManager, Guid.Empty, "Immediate Manager")]),
                new WorkflowStepSpec("SecondLevelReview", null, [new WorkflowApproverSpec(WorkflowApproverType.SecondLevelManager, Guid.Empty, "Second-Level Manager")]),
                new WorkflowStepSpec("EmployeeAcknowledgment", null, [new WorkflowApproverSpec(WorkflowApproverType.Subject, Guid.Empty, "Employee (self)")]),
                new WorkflowStepSpec("HrSignOff", "HR", null),
            });
            return def;
        }

        public async Task<AppraisalActorContext> ResolveAsync(Appraisal appraisal)
        {
            var myEmp = (await visibility.GetScopeAsync()).EmployeeId;
            var viewerIsAppraisee = myEmp.HasValue && myEmp.Value == appraisal.EmployeeId;

            var instance = await workflowService.GetRunningInstanceAsync(WorkflowEntityTypes.Appraisal, appraisal.Id);
            if (instance is null)
                // No routing instance (an appraisal created before routing existed, or a deactivated definition):
                // fail SAFE — nobody may act until it is re-attached (regenerate). Never fall open.
                return new AppraisalActorContext { CurrentUserRole = "Observer", CanActCurrentStage = false, CurrentStageActorName = "—", ViewerIsAppraisee = viewerIsAppraisee };

            var (canDecide, names) = await approverAuth.EvaluateAsync(instance.DefinitionId, instance.CurrentStepOrder, instance.EmployeeId);
            return new AppraisalActorContext
            {
                CurrentUserRole = canDecide ? "Actor" : "Observer",
                CanActCurrentStage = canDecide,
                CurrentStageActorName = names.Count > 0 ? string.Join(", ", names) : instance.CurrentStepName,
                ViewerIsAppraisee = viewerIsAppraisee,
            };
        }

        public async Task EnsureCanActAsync(Appraisal appraisal)
        {
            var instance = await workflowService.GetRunningInstanceAsync(WorkflowEntityTypes.Appraisal, appraisal.Id);
            if (instance is null)
                // Fail safe — an appraisal with no routing instance is not actionable (regenerate to attach one).
                throw new ValidationException("workflow",
                    "This appraisal is not attached to a workflow. Ask an administrator to configure the Appraisal workflow, then regenerate it.");
            await approverAuth.EnsureCanDecideAsync(instance);
        }

        public async Task<bool> CanActOnStageAsync(Appraisal appraisal, string stageName)
        {
            var instance = await workflowService.GetRunningInstanceAsync(WorkflowEntityTypes.Appraisal, appraisal.Id);
            if (instance is null) return false; // fail safe — no routing instance, no side actions
            var stepOrder = await definitions.GetAllWithoutTenantFilter()
                .Where(d => d.Id == instance.DefinitionId)
                .SelectMany(d => d.Steps)
                .Where(s => s.Name == stageName)
                .Select(s => (int?)s.StepOrder)
                .FirstOrDefaultAsync();
            if (stepOrder is null) return false;
            var (canDecide, _) = await approverAuth.EvaluateAsync(instance.DefinitionId, stepOrder.Value, instance.EmployeeId);
            return canDecide;
        }

        public async Task SyncInstanceAsync(Appraisal appraisal, string? comment)
        {
            var instance = await workflowService.GetRunningInstanceAsync(WorkflowEntityTypes.Appraisal, appraisal.Id);
            if (instance is null) return; // no workflow configured
            // Completed → close the instance (fires the module handler); otherwise advance to the step named for the new stage.
            var target = appraisal.Stage == AppraisalStage.Completed ? null : appraisal.Stage.ToString();
            await workflowService.AdvanceToStepAsync(instance.Id, target, comment);
        }

        public async Task<HashSet<Guid>> PendingMyActionAppraisalIdsAsync()
        {
            var running = await instances.GetAll()
                .Where(i => i.EntityType == WorkflowEntityTypes.Appraisal && i.Status == WorkflowInstanceStatus.Running)
                .Select(i => new { i.EntityId, i.DefinitionId, i.CurrentStepOrder, i.EmployeeId })
                .ToListAsync();
            var mine = new HashSet<Guid>();
            foreach (var i in running)
            {
                var (canDecide, _) = await approverAuth.EvaluateAsync(i.DefinitionId, i.CurrentStepOrder, i.EmployeeId);
                if (canDecide) mine.Add(i.EntityId);
            }
            return mine;
        }

        // ---- Role-based visibility — thin delegates over the per-request scope ----

        public async Task<Guid?> CurrentEmployeeIdAsync() => (await visibility.GetScopeAsync()).EmployeeId;

        public async Task<bool> CanAdministerAsync() => (await visibility.GetScopeAsync()).IsAdmin;

        public async Task<bool> CanManageEmployeeAsync(Guid employeeId)
        {
            var scope = await visibility.GetScopeAsync();
            if (scope.EmployeeId == employeeId) return false;   // nobody "manages" themselves
            if (!scope.IsAdmin && !scope.IsManager) return false;
            return await visibility.CanAccessEmployeeAsync(employeeId);
        }

        public async Task EnsureCanViewAsync(Appraisal appraisal)
        {
            if (!await visibility.CanAccessEmployeeAsync(appraisal.EmployeeId))
                throw new ValidationException("access", "You do not have access to this appraisal.");
        }
    }
}
