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
        /// <summary>
        /// Set-based variant for LIST handlers: which of <paramref name="entityIds"/> have a running
        /// workflow — ONE query for the whole page instead of one per row.
        /// </summary>
        Task<HashSet<Guid>> RunningIdsAsync(string entityTypePrefix, IReadOnlyCollection<Guid> entityIds);
    }

    public interface IWorkflowService
    {
        /// <summary>
        /// Starts an approval when an active definition exists for the entity type; otherwise the
        /// module keeps operating directly (no workflow). Duplicate running workflows are skipped.
        /// </summary>
        /// <param name="startAtStepName">Optional — begin the instance at this named step instead of step 1
        /// (e.g. an appraisal whose self-assessment stage is disabled starts at ManagerReview).</param>
        /// <param name="preValidateApprovers">When false, skip the start-time "every dynamic approver resolves"
        /// pre-flight — for flows with OPTIONAL steps (e.g. an appraisal's second-level review) that may never be
        /// reached, so an unreachable step must not block starting.</param>
        Task StartIfDefinedAsync(string entityType, Guid entityId, Guid? employeeId, string summary, string? startAtStepName = null, bool preValidateApprovers = true);
        /// <summary>
        /// Validates that the active definition (if any) could run to completion for this request —
        /// i.e. every dynamic approver (Immediate/Unit Manager) resolves. Call BEFORE persisting the
        /// governed record so an unresolvable workflow rejects the submission instead of orphaning it.
        /// </summary>
        Task EnsureStartableAsync(string entityType, Guid? employeeId);
        Task ApproveAsync(Guid instanceId, string? comment);
        Task RejectAsync(Guid instanceId, string? comment);
        /// <summary>
        /// Module-driven advance: authorizes the current step, logs the decision, then moves the instance to
        /// the step whose Name == <paramref name="targetStepName"/> — or completes it (Approved + module handler)
        /// when <paramref name="targetStepName"/> is null. Unlike <see cref="ApproveAsync"/> (rigid immediate-next),
        /// this lets a module steer the instance to a NAMED stage, cleanly skipping steps it chooses not to run
        /// (e.g. per-cycle-disabled appraisal stages). The caller supplies the next stage from its own state machine.
        /// </summary>
        Task AdvanceToStepAsync(Guid instanceId, string? targetStepName, string? comment);
        /// <summary>The running instance governing a record, or null when none (for module lockstep + gating).</summary>
        Task<WorkflowInstance?> GetRunningInstanceAsync(string entityType, Guid entityId);
    }

    public class WorkflowGate(IRepository<WorkflowInstance> instances) : IWorkflowGate
    {
        public Task<bool> HasRunningAsync(string entityTypePrefix, Guid entityId) =>
            instances.GetAll().AnyAsync(i =>
                i.EntityId == entityId &&
                i.EntityType.StartsWith(entityTypePrefix) &&
                i.Status == WorkflowInstanceStatus.Running);

        public async Task<HashSet<Guid>> RunningIdsAsync(string entityTypePrefix, IReadOnlyCollection<Guid> entityIds)
        {
            if (entityIds.Count == 0) return [];
            var running = await instances.GetAll().AsNoTracking()
                .Where(i => entityIds.Contains(i.EntityId) &&
                            i.EntityType.StartsWith(entityTypePrefix) &&
                            i.Status == WorkflowInstanceStatus.Running)
                .Select(i => i.EntityId)
                .ToListAsync();
            return [.. running];
        }

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
        IPortalNotifier portalNotifier,
        ICurrentUserService currentUser,
        ILogger<WorkflowService> logger) : IWorkflowService
    {
        /// <summary>Source-entity key correlating portal alerts to the workflow instance that raised them.</summary>
        private const string PortalSource = "WorkflowInstance";

        public async Task StartIfDefinedAsync(string entityType, Guid entityId, Guid? employeeId, string summary, string? startAtStepName = null, bool preValidateApprovers = true)
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
            // ("(unresolved)"). Fail loudly with the exact data gap instead of starting it. Skipped for
            // flows with optional steps that may never be reached.
            if (preValidateApprovers)
                await EnsureDynamicApproversResolvableAsync(stepList, employeeId);

            var user = currentUser.GetCurrentUserName();

            // Entry step: honour an explicit startAtStepName (the module's state machine may skip leading
            // steps), else the first step; then skip past any leading manager steps that are bypassable
            // for this subject — e.g. the CEO has no manager above them, so an Immediate / Second-Level
            // Manager step cannot route anywhere and falls through to the next step (typically HR).
            var entryIndex = 0;
            if (!string.IsNullOrWhiteSpace(startAtStepName))
            {
                var idx = stepList.FindIndex(s => s.Name == startAtStepName);
                if (idx >= 0) entryIndex = idx;
            }
            var bypassed = new List<WorkflowStep>();
            while (entryIndex < stepList.Count - 1 &&
                   await IsBypassableManagerStepAsync(stepList[entryIndex].Approvers.Select(a => a.ApproverType).ToList(), employeeId))
                bypassed.Add(stepList[entryIndex++]);
            var entryStep = stepList[entryIndex];

            var instance = WorkflowInstance.Start(
                definition.Id, entityType, entityId, employeeId, summary,
                stepList[0].Name, stepList.Count, user);
            if (entryStep.StepOrder != stepList[0].StepOrder)
                instance.AdvanceTo(entryStep.StepOrder, entryStep.Name);

            await instances.AddAsync(instance);
            await actionLogs.AddAsync(WorkflowActionLog.Create(
                instance.Id, 0, "Submission", WorkflowActionType.Submitted, null, user));
            // Record each auto-bypassed manager step so the tracking screen shows why the flow started later.
            foreach (var sk in bypassed)
                await actionLogs.AddAsync(WorkflowActionLog.Create(
                    instance.Id, sk.StepOrder, sk.Name, WorkflowActionType.Skipped,
                    "Auto-skipped: the requester is at the top of the org chain (no manager to route to).", user));
            await instances.SaveChangesAsync();
            logger.LogInformation("Started workflow {InstanceId} ({EntityType}) for entity {EntityId}",
                instance.Id, entityType, entityId);

            // Alert the first pending step's approvers in the Home portal.
            await NotifyCurrentStepApproversAsync(instance);
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
                // A manager step the subject sits atop (top of the org chain — e.g. the CEO, who has
                // no superior to route to) is bypassed at runtime, so it is not a configuration gap.
                if (await IsBypassableManagerStepAsync(step.Approvers.Select(a => a.ApproverType).ToList(), employeeId))
                    continue;

                foreach (var approver in step.Approvers)
                {
                    if (approver.ApproverType == WorkflowApproverType.Subject)
                    {
                        if (employeeId is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' routes to the request's subject, but this request has no employee context.");
                    }
                    else if (approver.ApproverType is WorkflowApproverType.ImmediateManager or WorkflowApproverType.SecondLevelManager)
                    {
                        var levelLabel = approver.ApproverType == WorkflowApproverType.SecondLevelManager ? "Second-Level Manager" : "Immediate Manager";
                        if (employeeId is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' routes to the {levelLabel}, but this request has no employee context.");

                        var unit = await managerResolver.GetEmployeeUnitAsync(employeeId.Value);
                        if (unit is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' routes to the {levelLabel}, but the employee has no position / organizational unit. Assign the employee to a position first.");

                        var resolved = approver.ApproverType == WorkflowApproverType.SecondLevelManager
                            ? await managerResolver.ResolveSecondLevelManagerAsync(employeeId.Value)
                            : await managerResolver.ResolveImmediateManagerAsync(employeeId.Value);
                        if (resolved is null)
                            throw new ValidationException("workflow",
                                $"Step '{step.Name}' cannot resolve the {levelLabel}: no managerial employee is positioned in '{unit.Value.Name}' or any of its parent units. " +
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

        /// <summary>
        /// A manager-approval step is "bypassable" for this subject when EVERY approver on it is a
        /// managerial climb (Immediate / Second-Level Manager), NONE of them resolves to a real
        /// approver, and the subject is themselves a managerial employee — i.e. they sit at the top of
        /// their chain (the CEO) with no superior to route to. Such steps are skipped so the flow falls
        /// through to the next step (typically HR), which is exactly the intended CEO behaviour. A
        /// NON-managerial subject that fails to resolve is a genuine org-data gap, not a top-of-chain
        /// bypass, so it is NOT bypassable and the actionable configuration error is still surfaced.
        /// A step that also carries a static / unit approver stays actionable and is never bypassed.
        /// </summary>
        private async Task<bool> IsBypassableManagerStepAsync(IReadOnlyCollection<WorkflowApproverType> approverTypes, Guid? employeeId)
        {
            if (employeeId is null || approverTypes.Count == 0) return false;
            if (!approverTypes.All(t => t is WorkflowApproverType.ImmediateManager or WorkflowApproverType.SecondLevelManager))
                return false;
            if (!await managerResolver.IsManagerialAsync(employeeId.Value)) return false;

            foreach (var type in approverTypes)
            {
                var resolved = type == WorkflowApproverType.SecondLevelManager
                    ? await managerResolver.ResolveSecondLevelManagerAsync(employeeId.Value)
                    : await managerResolver.ResolveImmediateManagerAsync(employeeId.Value);
                if (resolved is not null) return false; // an approver resolves — the step is actionable
            }
            return true;
        }

        public async Task ApproveAsync(Guid instanceId, string? comment)
        {
            var instance = await GetRunningAsync(instanceId);
            EnsureNotModuleDriven(instance);
            await approverAuth.EnsureCanDecideAsync(instance);
            var user = currentUser.GetCurrentUserName();

            await actionLogs.AddAsync(WorkflowActionLog.Create(
                instance.Id, instance.CurrentStepOrder, instance.CurrentStepName,
                WorkflowActionType.Approved, comment, user));

            // Load the chain (steps + approvers) to pick the next step, skipping any manager step that is
            // bypassable for this subject (no superior to route to — e.g. the CEO), which falls through.
            var chain = await definitions.GetAllWithoutTenantFilter()
                .Where(d => d.Id == instance.DefinitionId)
                .Include(d => d.Steps).ThenInclude(s => s.Approvers)
                .FirstOrDefaultAsync();
            var subsequent = chain is null
                ? new List<WorkflowStep>()
                : chain.Steps.Where(s => s.StepOrder > instance.CurrentStepOrder).OrderBy(s => s.StepOrder).ToList();

            (int StepOrder, string Name)? next = null;
            foreach (var cand in subsequent)
            {
                if (await IsBypassableManagerStepAsync(cand.Approvers.Select(a => a.ApproverType).ToList(), instance.EmployeeId))
                {
                    await actionLogs.AddAsync(WorkflowActionLog.Create(
                        instance.Id, cand.StepOrder, cand.Name, WorkflowActionType.Skipped,
                        "Auto-skipped: the requester is at the top of the org chain (no manager to route to).", user));
                    continue;
                }
                next = (cand.StepOrder, cand.Name);
                break;
            }

            if (next is not null)
            {
                instance.AdvanceTo(next.Value.StepOrder, next.Value.Name);
                instances.UpdateAsync(instance);
                await instances.SaveChangesAsync();
                logger.LogInformation("Workflow {InstanceId} advanced to step {Step} ({Name})",
                    instance.Id, next.Value.StepOrder, next.Value.Name);
                // Clear this step's alerts, then alert the next step's approvers.
                await ResolvePortalAlertsAsync(instance);
                await NotifyCurrentStepApproversAsync(instance);
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

            // Fully decided — clear any outstanding portal approval alerts for this instance.
            await ResolvePortalAlertsAsync(instance);
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
            EnsureNotModuleDriven(instance);
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

            // Rejected — clear any outstanding portal approval alerts for this instance.
            await ResolvePortalAlertsAsync(instance);
        }

        public async Task AdvanceToStepAsync(Guid instanceId, string? targetStepName, string? comment)
        {
            var instance = await GetRunningAsync(instanceId);
            await approverAuth.EnsureCanDecideAsync(instance);
            var user = currentUser.GetCurrentUserName();

            await actionLogs.AddAsync(WorkflowActionLog.Create(
                instance.Id, instance.CurrentStepOrder, instance.CurrentStepName,
                WorkflowActionType.Approved, comment, user));

            if (string.IsNullOrWhiteSpace(targetStepName))
            {
                // Terminal — complete first so module gates open, then apply the outcome (compensate on failure).
                instance.Complete(WorkflowInstanceStatus.Approved);
                instances.UpdateAsync(instance);
                await instances.SaveChangesAsync();
                try
                {
                    var handler = handlers.FirstOrDefault(h => h.Supports(instance.EntityType));
                    if (handler is not null) await handler.OnApprovedAsync(instance.EntityType, instance.EntityId);
                }
                catch { await TryReopenAsync(instance); throw; }
                await ResolvePortalAlertsAsync(instance);
                return;
            }

            var target = await definitions.GetAllWithoutTenantFilter()
                .Where(d => d.Id == instance.DefinitionId)
                .SelectMany(d => d.Steps)
                .Where(s => s.Name == targetStepName)
                .Select(s => new { s.StepOrder, s.Name })
                .FirstOrDefaultAsync()
                ?? throw new ValidationException("workflow", $"No workflow step named '{targetStepName}' in this definition.");

            instance.AdvanceTo(target.StepOrder, target.Name);
            instances.UpdateAsync(instance);
            await instances.SaveChangesAsync();
            logger.LogInformation("Workflow {InstanceId} advanced to step {Step} ({Name})", instance.Id, target.StepOrder, target.Name);
            // Clear this step's alerts, then alert the next step's approvers.
            await ResolvePortalAlertsAsync(instance);
            await NotifyCurrentStepApproversAsync(instance);
        }

        /// <summary>Some entity types (e.g. Appraisal) advance through their own rich per-stage actions, not the
        /// generic approve/reject buttons — steer users to that screen instead of half-advancing the instance.</summary>
        private static void EnsureNotModuleDriven(WorkflowInstance instance)
        {
            if (instance.EntityType == WorkflowEntityTypes.Appraisal)
                throw new ValidationException("workflow",
                    "Act on this appraisal from the appraisal screen (score / sign / complete); the generic approve/reject does not apply here.");
        }

        /// <summary>
        /// Best-effort: alert the CURRENT step's approvers in the Home portal that this request awaits
        /// their decision. Resolves the same approver set the decision-time auth uses, then writes one
        /// coreNotification per approver (correlated to this instance). A failure is logged, never thrown —
        /// a portal-notification hiccup must not break the governing operation.
        /// </summary>
        private async Task NotifyCurrentStepApproversAsync(WorkflowInstance instance)
        {
            try
            {
                var userIds = await approverAuth.ResolveApproverUserIdsAsync(
                    instance.DefinitionId, instance.CurrentStepOrder, instance.EmployeeId);

                // An OPEN step (no configured approvers) resolves to nobody, yet EvaluateAsync lets
                // ANYONE act on it — so the request would sit waiting with not one person told. Fall
                // back to the users entitled to approve, keeping "who may act" and "who is told"
                // consistent. Hiring Requests hit this: their seeded chain ships with open steps.
                if (userIds.Count == 0)
                {
                    userIds = await approverAuth.ResolveOpenStepRecipientsAsync();
                    if (userIds.Count > 0)
                        logger.LogInformation(
                            "Workflow {InstanceId}: step '{Step}' has no configured approvers — alerting {Count} " +
                            "user(s) with workflow approval rights instead",
                            instance.Id, instance.CurrentStepName, userIds.Count);
                }

                if (userIds.Count == 0)
                {
                    // Never fail silently: without this the only symptom is a missing notification.
                    logger.LogWarning(
                        "Workflow {InstanceId} ({EntityType}): NOBODY could be alerted for step '{Step}'. " +
                        "The step has no approvers and no role grants CanApprove on '/workflow' — configure " +
                        "approvers on this step, or grant the approval permission.",
                        instance.Id, instance.EntityType, instance.CurrentStepName);
                    return;
                }

                await portalNotifier.NotifyUsersAsync(
                    userIds,
                    $"Approval required: {instance.Summary}",
                    $"Awaiting your approval at step '{instance.CurrentStepName}'.",
                    "/workflow",
                    "Action",
                    PortalSource,
                    instance.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Workflow {InstanceId}: failed to raise portal approval alerts", instance.Id);
            }
        }

        /// <summary>Best-effort: clear this instance's outstanding portal alerts once a step is decided.</summary>
        private async Task ResolvePortalAlertsAsync(WorkflowInstance instance)
        {
            try
            {
                await portalNotifier.ResolveAsync(PortalSource, instance.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Workflow {InstanceId}: failed to clear portal approval alerts", instance.Id);
            }
        }

        public Task<WorkflowInstance?> GetRunningInstanceAsync(string entityType, Guid entityId) =>
            instances.GetAll().FirstOrDefaultAsync(i =>
                i.EntityId == entityId && i.EntityType == entityType && i.Status == WorkflowInstanceStatus.Running);

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
