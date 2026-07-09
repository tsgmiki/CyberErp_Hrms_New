using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Well-known workflow entity-type keys. A key identifies the HR process a definition governs;
/// new modules add a key here (or use any unique string) plus an <c>IWorkflowEntityHandler</c>.
/// </summary>
public static class WorkflowEntityTypes
{
    public const string Transfer = "EmployeeMovement.Transfer";
    public const string Promotion = "EmployeeMovement.Promotion";
    public const string Demotion = "EmployeeMovement.Demotion";
    public const string DisciplinaryMeasure = "DisciplinaryMeasure";
    public const string Termination = "EmployeeTermination";
    public const string LeaveRequest = "LeaveRequest";
    public const string WorkforcePlan = "WorkforcePlan";
    public const string HiringRequest = "HiringRequest";
    public const string JobRequisition = "JobRequisition";
}

/// <summary>
/// Admin-configured approval chain for one HR process (identified by <see cref="EntityType"/>).
/// The engine is generic: any module can be workflow-enabled by (1) creating a definition for its
/// entity-type key and (2) registering an <c>IWorkflowEntityHandler</c> that applies the outcome.
/// At most one definition per entity type may be active; deactivating it returns the module to
/// direct (non-workflow) operation.
/// </summary>
public class WorkflowDefinition : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    /// <summary>Process key, e.g. "EmployeeMovement.Transfer" (see <see cref="WorkflowEntityTypes"/>).</summary>
    public string EntityType { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<WorkflowStep> _steps = [];
    public IReadOnlyCollection<WorkflowStep> Steps => _steps;

    private WorkflowDefinition() : base() { }

    public static WorkflowDefinition Create(string name, string entityType, string? description = null, bool isActive = true)
    {
        Guard(name, entityType);
        return new WorkflowDefinition
        {
            Name = name,
            EntityType = entityType,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(string name, string entityType, string? description, bool isActive)
    {
        Guard(name, entityType);
        Name = name;
        EntityType = entityType;
        Description = description;
        IsActive = isActive;
        base.Update();
    }

    /// <summary>Replaces the approval chain with the given ordered step names.</summary>
    public void SetSteps(IEnumerable<(string Name, string? ApproverRole)> steps) =>
        SetSteps(steps.Select(s => new WorkflowStepSpec(s.Name, s.ApproverRole)));

    /// <summary>Replaces the approval chain with ordered steps, each with its authorized approvers.</summary>
    public void SetSteps(IEnumerable<WorkflowStepSpec> steps)
    {
        var list = steps.ToList();
        if (list.Count == 0)
            throw new ArgumentException("A workflow needs at least one step.", nameof(steps));

        _steps.Clear();
        var order = 1;
        foreach (var spec in list)
        {
            var step = WorkflowStep.Create(Id, order++, spec.Name, spec.ApproverRole);
            foreach (var approver in spec.Approvers ?? [])
                step.AddApprover(approver.Type, approver.ApproverId, approver.DisplayName);
            _steps.Add(step);
        }
        base.Update();
    }

    private static void Guard(string name, string entityType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workflow name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty.", nameof(entityType));
    }
}

/// <summary>Who may act on a workflow step: a specific user, or any user holding a role.</summary>
public enum WorkflowApproverType
{
    User = 0,
    Role = 1
}

/// <summary>Input spec for one approver of a step (see <see cref="WorkflowDefinition.SetSteps(IEnumerable{WorkflowStepSpec})"/>).</summary>
public record WorkflowApproverSpec(WorkflowApproverType Type, Guid ApproverId, string DisplayName);

/// <summary>Input spec for one ordered step of an approval chain.</summary>
public record WorkflowStepSpec(string Name, string? ApproverRole = null, IReadOnlyList<WorkflowApproverSpec>? Approvers = null);

/// <summary>One ordered approval step of a <see cref="WorkflowDefinition"/>.</summary>
public class WorkflowStep : BaseEntity
{
    public Guid DefinitionId { get; private set; }
    public int StepOrder { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>Legacy free-text approver hint; superseded by <see cref="Approvers"/>.</summary>
    public string? ApproverRole { get; private set; }

    private readonly List<WorkflowStepApprover> _approvers = [];
    /// <summary>Authorized approvers. Empty = any authenticated user may act (open step).</summary>
    public IReadOnlyCollection<WorkflowStepApprover> Approvers => _approvers;

    private WorkflowStep() : base() { }

    public static WorkflowStep Create(Guid definitionId, int stepOrder, string name, string? approverRole = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name cannot be empty.", nameof(name));
        return new WorkflowStep
        {
            DefinitionId = definitionId,
            StepOrder = stepOrder,
            Name = name,
            ApproverRole = approverRole
        };
    }

    public void AddApprover(WorkflowApproverType type, Guid approverId, string displayName)
    {
        if (approverId == Guid.Empty)
            throw new ArgumentException("Approver is required.", nameof(approverId));
        if (_approvers.Any(a => a.ApproverType == type && a.ApproverId == approverId))
            return;
        _approvers.Add(WorkflowStepApprover.Create(Id, type, approverId, displayName));
    }
}

/// <summary>One authorized approver (user or role) of a workflow step.</summary>
public class WorkflowStepApprover : BaseEntity
{
    public Guid StepId { get; private set; }
    public WorkflowApproverType ApproverType { get; private set; }
    /// <summary>User id or role id, depending on <see cref="ApproverType"/>.</summary>
    public Guid ApproverId { get; private set; }
    /// <summary>Display snapshot (user full name / role name) so lists render without joins.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    private WorkflowStepApprover() : base() { }

    public static WorkflowStepApprover Create(Guid stepId, WorkflowApproverType type, Guid approverId, string displayName)
    {
        return new WorkflowStepApprover
        {
            StepId = stepId,
            ApproverType = type,
            ApproverId = approverId,
            DisplayName = displayName
        };
    }
}
