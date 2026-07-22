using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum TerminationType
{
    Voluntary = 0,   // resignation, retirement on request
    Involuntary = 1  // dismissal, redundancy, contract end
}

/// <summary>
/// Lifecycle of a termination case: recorded → (workflow approval) → departmental clearance →
/// final settlement. Settlement is blocked until every clearance item is cleared.
/// </summary>
public enum TerminationStatus
{
    Initiated = 0,
    ClearanceInProgress = 1,
    Settled = 2,
    Cancelled = 3
}

public enum ClearanceStatus
{
    Pending = 0,
    Cleared = 1,
    Blocked = 2
}

/// <summary>
/// Termination &amp; clearance case (offboarding). Routed through the generic workflow engine
/// (entity type "EmployeeTermination"); approval opens the departmental clearance checklist, and
/// <c>Finalize</c> applies the system automations: employee → Terminated, position decoupled and
/// reopened (IsVacant = true).
/// </summary>
public class EmployeeTermination : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public TerminationType TerminationType { get; private set; }
    public TerminationStatus Status { get; private set; } = TerminationStatus.Initiated;
    public DateTime NoticeDate { get; private set; }
    public DateTime LastWorkingDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Remarks { get; private set; }
    public DateTime? SettledAt { get; private set; }
    /// <summary>
    /// The position the employee held, snapshotted at settlement (their live PositionId is nulled by
    /// <see cref="Employee.Terminate"/>). Lets reinstatement restore the previous placement. Carries
    /// no FK — a historical snapshot, like <see cref="EmployeeMovement"/>'s From/To position columns.
    /// </summary>
    public Guid? VacatedPositionId { get; private set; }
    /// <summary>Set when a settled termination is reversed by reinstatement (null otherwise).</summary>
    public DateTime? ReinstatedAt { get; private set; }

    private readonly List<TerminationClearance> _clearances = [];
    public IReadOnlyCollection<TerminationClearance> Clearances => _clearances;

    private EmployeeTermination() : base() { }

    public static EmployeeTermination Create(
        Guid employeeId,
        TerminationType terminationType,
        DateTime noticeDate,
        DateTime lastWorkingDate,
        string reason,
        string? remarks = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        Guard(noticeDate, lastWorkingDate, reason);

        return new EmployeeTermination
        {
            EmployeeId = employeeId,
            TerminationType = terminationType,
            NoticeDate = noticeDate,
            LastWorkingDate = lastWorkingDate,
            Reason = reason,
            Remarks = remarks
        };
    }

    /// <summary>Corrections are allowed while the case is still at initiation.</summary>
    public void Update(
        TerminationType terminationType,
        DateTime noticeDate,
        DateTime lastWorkingDate,
        string reason,
        string? remarks)
    {
        if (Status != TerminationStatus.Initiated)
            throw new InvalidOperationException($"A {Status} termination can no longer be edited.");
        Guard(noticeDate, lastWorkingDate, reason);
        TerminationType = terminationType;
        NoticeDate = noticeDate;
        LastWorkingDate = lastWorkingDate;
        Reason = reason;
        Remarks = remarks;
        base.Update();
    }

    /// <summary>
    /// Approval outcome: opens the departmental clearance checklist. Items built from a configured
    /// <see cref="ClearanceDepartment"/> carry its id so approver authorization can be enforced.
    /// </summary>
    public void BeginClearance(IEnumerable<(string Department, string Description, Guid? DepartmentId)> checklist)
    {
        if (Status != TerminationStatus.Initiated)
            throw new InvalidOperationException($"Clearance can only start from Initiated (current: {Status}).");
        Status = TerminationStatus.ClearanceInProgress;
        foreach (var (department, description, departmentId) in checklist)
            if (_clearances.All(c => c.Department != department))
                _clearances.Add(TerminationClearance.Create(Id, department, description, departmentId));
        base.Update();
    }

    /// <summary>
    /// Final settlement — only when every clearance item is cleared. Snapshots the vacated position
    /// (before the employee's placement is decoupled) so a later reinstatement can restore it.
    /// </summary>
    public void MarkSettled(Guid? vacatedPositionId)
    {
        if (Status != TerminationStatus.ClearanceInProgress)
            throw new InvalidOperationException($"Only a clearance-in-progress termination can be settled (current: {Status}).");
        if (_clearances.Count == 0 || _clearances.Any(c => c.Status != ClearanceStatus.Cleared))
            throw new InvalidOperationException("All clearance items must be cleared before settlement.");
        Status = TerminationStatus.Settled;
        SettledAt = DateTime.UtcNow;
        VacatedPositionId = vacatedPositionId;
        base.Update();
    }

    /// <summary>Records that this settled termination was reversed by an employee reinstatement.</summary>
    public void MarkReinstated()
    {
        if (Status != TerminationStatus.Settled)
            throw new InvalidOperationException($"Only a settled termination can be reinstated (current: {Status}).");
        if (ReinstatedAt.HasValue)
            throw new InvalidOperationException("This termination has already been reinstated.");
        ReinstatedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Cancel()
    {
        if (Status is TerminationStatus.Settled or TerminationStatus.Cancelled)
            throw new InvalidOperationException($"A {Status} termination can no longer be cancelled.");
        Status = TerminationStatus.Cancelled;
        base.Update();
    }

    private static void Guard(DateTime noticeDate, DateTime lastWorkingDate, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Termination reason cannot be empty.", nameof(reason));
        if (lastWorkingDate < noticeDate)
            throw new ArgumentException("Last working date cannot be before the notice date.", nameof(lastWorkingDate));
    }
}

/// <summary>One departmental clearance item of a termination case (IT assets, Store property, Finance loans…).</summary>
public class TerminationClearance : BaseEntity
{
    public Guid TerminationId { get; private set; }
    public string Department { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    /// <summary>Configured <see cref="ClearanceDepartment"/> this item was built from (null for
    /// the built-in fallback checklist and legacy rows) — resolves the authorized approvers.</summary>
    public Guid? DepartmentId { get; private set; }
    public ClearanceStatus Status { get; private set; } = ClearanceStatus.Pending;
    public string? Note { get; private set; }
    public string? ClearedBy { get; private set; }
    public DateTime? ClearedAt { get; private set; }

    private TerminationClearance() : base() { }

    public static TerminationClearance Create(Guid terminationId, string department, string description, Guid? departmentId = null)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department cannot be empty.", nameof(department));
        return new TerminationClearance
        {
            TerminationId = terminationId,
            Department = department,
            Description = description,
            DepartmentId = departmentId
        };
    }

    public void SetStatus(ClearanceStatus status, string? note, string? actedBy)
    {
        Status = status;
        Note = note;
        if (status == ClearanceStatus.Cleared)
        {
            ClearedBy = actedBy;
            ClearedAt = DateTime.UtcNow;
        }
        else
        {
            ClearedBy = null;
            ClearedAt = null;
        }
        base.Update();
    }
}
