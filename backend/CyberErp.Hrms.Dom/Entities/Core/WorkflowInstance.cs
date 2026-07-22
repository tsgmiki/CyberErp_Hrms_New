using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum WorkflowInstanceStatus
{
    Running = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum WorkflowActionType
{
    Submitted = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

/// <summary>
/// A running (or finished) approval for one business record. The engine is module-agnostic:
/// <see cref="EntityType"/> + <see cref="EntityId"/> point at the governed record, and
/// <see cref="Summary"/> is a precomputed display line so tracking screens never need to join
/// module tables. Step decisions are appended to <see cref="WorkflowActionLog"/> rows.
/// </summary>
public class WorkflowInstance : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid DefinitionId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    /// <summary>Subject employee (drives per-branch visibility on tracking screens).</summary>
    public Guid? EmployeeId { get; private set; }
    /// <summary>Human-readable subject, e.g. "Transfer — Abebe Kebede (EMP-001)".</summary>
    public string Summary { get; private set; } = string.Empty;

    public WorkflowInstanceStatus Status { get; private set; } = WorkflowInstanceStatus.Running;
    public int CurrentStepOrder { get; private set; } = 1;
    public string CurrentStepName { get; private set; } = string.Empty;
    public int TotalSteps { get; private set; }
    public string? RequestedBy { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private WorkflowInstance() : base() { }

    public static WorkflowInstance Start(
        Guid definitionId, string entityType, Guid entityId, Guid? employeeId,
        string summary, string firstStepName, int totalSteps, string? requestedBy)
    {
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity is required.", nameof(entityId));
        if (totalSteps < 1)
            throw new ArgumentException("A workflow needs at least one step.", nameof(totalSteps));

        return new WorkflowInstance
        {
            DefinitionId = definitionId,
            EntityType = entityType,
            EntityId = entityId,
            EmployeeId = employeeId,
            Summary = summary,
            CurrentStepOrder = 1,
            CurrentStepName = firstStepName,
            TotalSteps = totalSteps,
            RequestedBy = requestedBy
        };
    }

    public void AdvanceTo(int stepOrder, string stepName)
    {
        EnsureRunning();
        CurrentStepOrder = stepOrder;
        CurrentStepName = stepName;
        base.Update();
    }

    public void Complete(WorkflowInstanceStatus outcome)
    {
        EnsureRunning();
        if (outcome == WorkflowInstanceStatus.Running)
            throw new ArgumentException("Completion outcome cannot be Running.", nameof(outcome));
        Status = outcome;
        CompletedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Compensation: reopen after a failed completion side-effect (module action threw).</summary>
    public void Reopen()
    {
        Status = WorkflowInstanceStatus.Running;
        CompletedAt = null;
        base.Update();
    }

    private void EnsureRunning()
    {
        if (Status != WorkflowInstanceStatus.Running)
            throw new InvalidOperationException($"A {Status} workflow can no longer be modified.");
    }
}

/// <summary>Immutable log of one decision taken on a workflow instance.</summary>
public class WorkflowActionLog : BaseEntity
{
    public Guid InstanceId { get; private set; }
    public int StepOrder { get; private set; }
    public string StepName { get; private set; } = string.Empty;
    public WorkflowActionType Action { get; private set; }
    public string? Comment { get; private set; }
    public string? ActedBy { get; private set; }
    public DateTime ActedAt { get; private set; }

    private WorkflowActionLog() : base() { }

    public static WorkflowActionLog Create(
        Guid instanceId, int stepOrder, string stepName,
        WorkflowActionType action, string? comment, string? actedBy)
    {
        return new WorkflowActionLog
        {
            InstanceId = instanceId,
            StepOrder = stepOrder,
            StepName = stepName,
            Action = action,
            Comment = comment,
            ActedBy = actedBy,
            ActedAt = DateTime.UtcNow
        };
    }
}
