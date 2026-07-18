using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

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
    }

    public class AppraisalWorkflowService(
        IWorkflowService workflowService,
        IWorkflowApproverAuth approverAuth,
        IRepository<WorkflowInstance> instances,
        IRepository<WorkflowDefinition> definitions) : IAppraisalWorkflowService
    {
        public async Task<AppraisalActorContext> ResolveAsync(Appraisal appraisal)
        {
            var instance = await workflowService.GetRunningInstanceAsync(WorkflowEntityTypes.Appraisal, appraisal.Id);
            if (instance is null)
                // No routing configured (definition deactivated) — engine idiom is "operate directly".
                return new AppraisalActorContext { CurrentUserRole = "Open", CanActCurrentStage = true };

            var (canDecide, names) = await approverAuth.EvaluateAsync(instance.DefinitionId, instance.CurrentStepOrder, instance.EmployeeId);
            return new AppraisalActorContext
            {
                CurrentUserRole = canDecide ? "Actor" : "Observer",
                CanActCurrentStage = canDecide,
                CurrentStageActorName = names.Count > 0 ? string.Join(", ", names) : instance.CurrentStepName,
            };
        }

        public async Task EnsureCanActAsync(Appraisal appraisal)
        {
            var instance = await workflowService.GetRunningInstanceAsync(WorkflowEntityTypes.Appraisal, appraisal.Id);
            if (instance is null) return; // no workflow configured — operate directly
            await approverAuth.EnsureCanDecideAsync(instance);
        }

        public async Task<bool> CanActOnStageAsync(Appraisal appraisal, string stageName)
        {
            var instance = await workflowService.GetRunningInstanceAsync(WorkflowEntityTypes.Appraisal, appraisal.Id);
            if (instance is null) return true; // open
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
    }
}
