using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    /// <summary>
    /// Extensibility point of the workflow engine. A module becomes workflow-enabled by
    /// registering one implementation: the engine calls it when a workflow for one of its
    /// entity-type keys reaches a final outcome. No engine changes are needed per module.
    /// </summary>
    public interface IWorkflowEntityHandler
    {
        /// <summary>Whether this handler owns the given entity-type key (e.g. an "EmployeeMovement.*" prefix).</summary>
        bool Supports(string entityType);
        /// <summary>Apply the approved outcome (e.g. execute the movement).</summary>
        Task OnApprovedAsync(string entityType, Guid entityId);
        /// <summary>Apply the rejected outcome (e.g. cancel the movement).</summary>
        Task OnRejectedAsync(string entityType, Guid entityId);
    }

    /// <summary>Lets modules block direct mutations while an approval is in flight.</summary>
    public interface IWorkflowGate
    {
        Task<bool> HasRunningAsync(string entityTypePrefix, Guid entityId);
        /// <summary>Throws a 400 when a running workflow governs the record.</summary>
        Task EnsureNoRunningAsync(string entityTypePrefix, Guid entityId);
    }

    public interface IWorkflowService
    {
        /// <summary>
        /// Starts an approval when an active definition exists for the entity type; otherwise the
        /// module keeps operating directly (no workflow). Duplicate running workflows are skipped.
        /// </summary>
        Task StartIfDefinedAsync(string entityType, Guid entityId, Guid? employeeId, string summary);
        /// <summary>
        /// Validates that the active definition (if any) could run to completion for this request —
        /// i.e. every dynamic approver (Immediate/Unit Manager) resolves. Call BEFORE persisting the
        /// governed record so an unresolvable workflow rejects the submission instead of orphaning it.
        /// </summary>
        Task EnsureStartableAsync(string entityType, Guid? employeeId);
        Task ApproveAsync(Guid instanceId, string? comment);
        Task RejectAsync(Guid instanceId, string? comment);
    }

    public class WorkflowGate(IRepository<WorkflowInstance> instances) : IWorkflowGate
    {
        public Task<bool> HasRunningAsync(string entityTypePrefix, Guid entityId) =>
            instances.GetAll().AnyAsync(i =>
                i.EntityId == entityId &&
                i.EntityType.StartsWith(entityTypePrefix) &&
                i.Status == WorkflowInstanceStatus.Running);

        public async Task EnsureNoRunningAsync(string entityTypePrefix, Guid entityId)
        {
            if (await HasRunningAsync(entityTypePrefix, entityId))
                throw new ValidationException("workflow",
                    "This record is awaiting workflow approval. Approve or reject it from the workflow screen.");
        }
    }

    public class WorkflowService(
        IRepository<WorkflowInstance> instances,
        IRepository<WorkflowDefinition> definitions,
        IRepository<WorkflowActionLog> actionLogs,
        IEnumerable<IWorkflowEntityHandler> handlers,
        IWorkflowApproverAuth approverAuth,
        IOrgManagerResolver managerResolver,
        ICurrentUserService currentUser,
        ILogger<WorkflowService> logger) : IWorkflowService
    {
        public async Task StartIfDefinedAsync(string entityType, Guid entityId, Guid? employeeId, string summary)
        {
            // Steps are read through the definition aggregate (children carry no tenant stamp).
            var definition = await definitions.GetAll()
                .Include(d => d.Steps).ThenInclude(s => s.Approvers)
                .FirstOrDefaultAsync(d => d.EntityType == entityType && d.IsActive);
            if (definition is null) return; // no workflow configured — module operates directly

            if (await instances.GetAll().AnyAsync(i =>
                    i.EntityId == entityId && i.EntityType == entityType && i.Status == WorkflowInstanceStatus.Running))
                return;

            var stepList = definition.Steps.OrderBy(s => s.StepOrder).ToList();
            if (stepList.Count == 0) return; // misconfigured definition — treat as no workflow

            // Pre-flight: every DYNAMIC approver (Immediate/Unit Manager) across ALL steps must be
            // resolvable NOW, otherwise the instance would get stuck on a step nobody can action
            // ("(unresolved)"). Fail loudly with the exact data gap instead of starting it.
            await EnsureDynamicApproversResolvableAsync(stepList, employeeId);

            var user = currentUser.GetCurrentUserName();
            var instance = WorkflowInstance.Start(
                definition.Id, entityType, entityId, employeeId, summary,
                stepList[0].Name, stepList.Count, user);
            await instances.AddAsync(instance);
            await actionLogs.AddAsync(WorkflowActionLog.Create(
                instance.Id, 0, "Submission", WorkflowActionType.Submitted, null, user));
            await instances.SaveChangesAsync();
            logger.LogInformation("Started workflow {InstanceId} ({EntityType}) for entity {EntityId}",
                instance.Id, entityType, entityId);
        }

        public async Task EnsureStartableAsync(string entityType, Guid? employeeId)
        {
            var definition = await definitions.GetAll()
                .Include(d => d.Steps).ThenInclude(s => s.Approvers)
                .FirstOrDefaultAsync(d => d.EntityType == entityType && d.IsActive);
            if (definition is null) return; // nothing would start — nothing to validate

            await EnsureDynamicApproversResolvableAsync(
                definition.Steps.OrderBy(s => s.StepOrder).ToList(), employeeId);
        }

        /// <summary>
        /// Validates that each dynamic approver can resolve to a real manager for this request —
        /// the same climb the decision-time authorization performs — and throws an actionable 400
        /// naming the exact configuration gap (unplaced employee / unmanaged unit chain) otherwise.
        /// </summary>
        private async Task EnsureDynamicApproversResolvableAsync(IReadOnlyList<WorkflowStep> steps, Guid? employeeId)
        {
            foreach (var step in steps)
            {
                foreach (var approver in step.Approvers)
                {
                    if (approver.ApproverType == WorkflowApproverType.ImmediateManager)
                    {
                        if (employeeId is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' routes to the Immediate Manager, but this request has no employee context.");

                        var unit = await managerResolver.GetEmployeeUnitAsync(employeeId.Value);
                        if (unit is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' routes to the Immediate Manager, but the employee has no position / organizational unit. Assign the employee to a position first.");

                        if (await managerResolver.ResolveImmediateManagerAsync(employeeId.Value) is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' cannot resolve an approver: no managerial employee is positioned in '{unit.Value.Name}' or any of its parent units. " +
                                "Designate a manager by ticking 'Managerial' on an employee whose position belongs to that unit (or a parent unit).");
                    }
                    else if (approver.ApproverType == WorkflowApproverType.UnitManager)
                    {
                        if (await managerResolver.ResolveUnitManagerAsync(approver.ApproverId, employeeId) is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' cannot resolve '{approver.DisplayName}': no managerial employee is positioned in that unit or any of its parent units. " +
                                "Designate a manager by ticking 'Managerial' on an employee whose position belongs to that unit (or a parent unit).");
                    }
                }
            }
        }

        public async Task ApproveAsync(Guid instanceId, string? comment)
        {
            var instance = await GetRunningAsync(instanceId);
            await approverAuth.EnsureCanDecideAsync(instance);
            var user = currentUser.GetCurrentUserName();

            await actionLogs.AddAsync(WorkflowActionLog.Create(
                instance.Id, instance.CurrentStepOrder, instance.CurrentStepName,
                WorkflowActionType.Approved, comment, user));

            var next = await definitions.GetAllWithoutTenantFilter()
                .Where(d => d.Id == instance.DefinitionId)
                .SelectMany(d => d.Steps)
                .Where(s => s.StepOrder > instance.CurrentStepOrder)
                .OrderBy(s => s.StepOrder)
                .Select(s => new { s.StepOrder, s.Name })
                .FirstOrDefaultAsync();

            if (next is not null)
            {
                instance.AdvanceTo(next.StepOrder, next.Name);
                instances.UpdateAsync(instance);
                await instances.SaveChangesAsync();
                logger.LogInformation("Workflow {InstanceId} advanced to step {Step} ({Name})",
                    instance.Id, next.StepOrder, next.Name);
                return;
            }

            // Final approval: complete first so module gates open, then apply the outcome.
            // If the module action fails, compensate by reopening the instance.
            instance.Complete(WorkflowInstanceStatus.Approved);
            instances.UpdateAsync(instance);
            await instances.SaveChangesAsync();
            try
            {
                var handler = handlers.FirstOrDefault(h => h.Supports(instance.EntityType));
                if (handler is not null)
                    await handler.OnApprovedAsync(instance.EntityType, instance.EntityId);
                logger.LogInformation("Workflow {InstanceId} approved and applied", instance.Id);
            }
            catch
            {
                await TryReopenAsync(instance);
                throw;
            }
        }

        /// <summary>
        /// Compensation after a failed outcome handler. Best effort: the shared DbContext may
        /// still track the handler's failed changes, in which case the reopen cannot be saved —
        /// log it rather than mask the original exception.
        /// </summary>
        private async Task TryReopenAsync(WorkflowInstance instance)
        {
            try
            {
                instance.Reopen();
                instances.UpdateAsync(instance);
                await instances.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Workflow {InstanceId}: outcome handler failed AND compensation could not be saved — instance may need manual reopening",
                    instance.Id);
            }
        }

        public async Task RejectAsync(Guid instanceId, string? comment)
        {
            var instance = await GetRunningAsync(instanceId);
            await approverAuth.EnsureCanDecideAsync(instance);
            var user = currentUser.GetCurrentUserName();

            await actionLogs.AddAsync(WorkflowActionLog.Create(
                instance.Id, instance.CurrentStepOrder, instance.CurrentStepName,
                WorkflowActionType.Rejected, comment, user));

            instance.Complete(WorkflowInstanceStatus.Rejected);
            instances.UpdateAsync(instance);
            await instances.SaveChangesAsync();
            try
            {
                var handler = handlers.FirstOrDefault(h => h.Supports(instance.EntityType));
                if (handler is not null)
                    await handler.OnRejectedAsync(instance.EntityType, instance.EntityId);
                logger.LogInformation("Workflow {InstanceId} rejected", instance.Id);
            }
            catch
            {
                await TryReopenAsync(instance);
                throw;
            }
        }

        private async Task<WorkflowInstance> GetRunningAsync(Guid instanceId)
        {
            var instance = await instances.GetAll().FirstOrDefaultAsync(i => i.Id == instanceId)
                ?? throw new NotFoundException(nameof(WorkflowInstance), instanceId.ToString());
            if (instance.Status != WorkflowInstanceStatus.Running)
                throw new ValidationException("status", $"This workflow is already {instance.Status}.");
            return instance;
        }
    }
}
