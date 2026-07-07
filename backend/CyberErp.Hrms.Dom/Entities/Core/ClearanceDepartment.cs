using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Admin-configured clearance department for employee offboarding (mirrors the workflow engine's
/// definition/approver pattern). Active departments form the clearance checklist opened when a
/// termination is approved; each department carries its own authorized approvers — clearing the
/// item requires being (or holding a role of) one of them, and any single authorized user's
/// approval clears the department. A department with no approvers is open (anyone may clear it).
/// </summary>
public class ClearanceDepartment : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    /// <summary>Requirement text shown on the checklist item (what must be returned/settled).</summary>
    public string Description { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ClearanceDepartmentApprover> _approvers = [];
    /// <summary>Authorized approvers. Empty = any authenticated user may clear (open department).</summary>
    public IReadOnlyCollection<ClearanceDepartmentApprover> Approvers => _approvers;

    private ClearanceDepartment() : base() { }

    public static ClearanceDepartment Create(string name, string description, int sortOrder = 0, bool isActive = true)
    {
        Guard(name, description);
        return new ClearanceDepartment
        {
            Name = name,
            Description = description,
            SortOrder = sortOrder,
            IsActive = isActive
        };
    }

    public void Update(string name, string description, int sortOrder, bool isActive)
    {
        Guard(name, description);
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        base.Update();
    }

    /// <summary>Replaces the authorized approvers (same spec shape as workflow steps).</summary>
    public void SetApprovers(IEnumerable<WorkflowApproverSpec> approvers)
    {
        _approvers.Clear();
        foreach (var spec in approvers)
        {
            if (spec.ApproverId == Guid.Empty)
                throw new ArgumentException("Approver is required.", nameof(approvers));
            if (_approvers.Any(a => a.ApproverType == spec.Type && a.ApproverId == spec.ApproverId))
                continue;
            _approvers.Add(ClearanceDepartmentApprover.Create(Id, spec.Type, spec.ApproverId, spec.DisplayName));
        }
        base.Update();
    }

    private static void Guard(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Clearance requirement description cannot be empty.", nameof(description));
    }
}

/// <summary>One authorized approver (user or role) of a clearance department.</summary>
public class ClearanceDepartmentApprover : BaseEntity
{
    public Guid DepartmentId { get; private set; }
    public WorkflowApproverType ApproverType { get; private set; }
    /// <summary>User id or role id, depending on <see cref="ApproverType"/>.</summary>
    public Guid ApproverId { get; private set; }
    /// <summary>Display snapshot (user full name / role name) so lists render without joins.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    private ClearanceDepartmentApprover() : base() { }

    public static ClearanceDepartmentApprover Create(Guid departmentId, WorkflowApproverType type, Guid approverId, string displayName)
    {
        return new ClearanceDepartmentApprover
        {
            DepartmentId = departmentId,
            ApproverType = type,
            ApproverId = approverId,
            DisplayName = displayName
        };
    }
}
