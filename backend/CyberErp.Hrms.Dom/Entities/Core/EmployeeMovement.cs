using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Kind of personnel action recorded by an <see cref="EmployeeMovement"/>.</summary>
public enum MovementType
{
    Transfer = 0,
    Promotion = 1,
    Demotion = 2
}

/// <summary>Lifecycle of a personnel action: recorded → (optionally approved, awaiting its effective
/// date) → executed against the employee master (or cancelled).</summary>
public enum MovementStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2,
    /// <summary>Workflow-approved but future-dated — a scheduler executes it on the effective date (HC176).</summary>
    Approved = 3
}

/// <summary>What a Transfer changes (HC171): the role, the department/unit, or the work location.</summary>
public enum TransferKind
{
    Role = 0,
    Department = 1,
    Location = 2
}

/// <summary>
/// A dated personnel action (SAP-style): transfer, promotion or demotion. Captures the employee's
/// placement at creation time (From*) and the intended placement (To*). Executing the movement
/// applies the To* values to the employee master and marks it <see cref="MovementStatus.Completed"/>,
/// leaving an immutable movement history per employee.
/// </summary>
public class EmployeeMovement : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public MovementType MovementType { get; private set; }
    public MovementStatus Status { get; private set; } = MovementStatus.Pending;
    public DateTime EffectiveDate { get; private set; }

    // Placement snapshot when the action was recorded.
    public Guid? FromPositionId { get; private set; }
    /// <summary>The pay point (salary scale) the employee held at recording time — the grade derives from it.</summary>
    public Guid? FromSalaryScaleId { get; private set; }
    public decimal? FromSalary { get; private set; }
    public Guid? FromBranchId { get; private set; }

    // Intended placement. Null = leave unchanged (except Transfer, which requires a position).
    public Guid? ToPositionId { get; private set; }
    /// <summary>The target pay point (salary scale, FK to coreSalaryScale). Only set on Promotion/Demotion.</summary>
    public Guid? ToSalaryScaleId { get; private set; }
    public decimal? ToSalary { get; private set; }
    /// <summary>Resolved from the target position at execution (isolation follows the position).</summary>
    public Guid? ToBranchId { get; private set; }

    public string? Reason { get; private set; }
    public string? Remark { get; private set; }
    public DateTime? ExecutedAt { get; private set; }

    // Transfer-request details (HC170/HC171/HC173).
    /// <summary>What the transfer changes (Role / Department / Location) — Transfer rows only.</summary>
    public TransferKind? TransferKind { get; private set; }
    /// <summary>Who initiated the request (employee self-service, their manager, or HR) — audit snapshot, no FK.</summary>
    public Guid? RequestedByEmployeeId { get; private set; }
    /// <summary>Estimated relocation cost for budget-impact tracking (HC173) — Transfer rows only.</summary>
    public decimal? RelocationExpense { get; private set; }

    private EmployeeMovement() : base() { }

    public static EmployeeMovement Create(
        Guid employeeId,
        MovementType movementType,
        DateTime effectiveDate,
        Guid? fromPositionId, Guid? fromSalaryScaleId, decimal? fromSalary, Guid? fromBranchId,
        Guid? toPositionId, Guid? toSalaryScaleId, decimal? toSalary,
        string? reason = null, string? remark = null,
        TransferKind? transferKind = null, Guid? requestedByEmployeeId = null, decimal? relocationExpense = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        Guard(movementType, toPositionId, toSalaryScaleId, toSalary, transferKind, relocationExpense);

        return new EmployeeMovement
        {
            EmployeeId = employeeId,
            MovementType = movementType,
            EffectiveDate = effectiveDate,
            FromPositionId = fromPositionId,
            FromSalaryScaleId = fromSalaryScaleId,
            FromSalary = fromSalary,
            FromBranchId = fromBranchId,
            ToPositionId = toPositionId,
            ToSalaryScaleId = toSalaryScaleId,
            ToSalary = toSalary,
            Reason = reason,
            Remark = remark,
            TransferKind = transferKind,
            RequestedByEmployeeId = requestedByEmployeeId,
            RelocationExpense = relocationExpense
        };
    }

    /// <summary>Pending movements can be corrected before execution.</summary>
    public void Update(
        MovementType movementType,
        DateTime effectiveDate,
        Guid? toPositionId, Guid? toSalaryScaleId, decimal? toSalary,
        string? reason, string? remark,
        TransferKind? transferKind = null, decimal? relocationExpense = null)
    {
        EnsurePending();
        Guard(movementType, toPositionId, toSalaryScaleId, toSalary, transferKind, relocationExpense);
        MovementType = movementType;
        EffectiveDate = effectiveDate;
        ToPositionId = toPositionId;
        ToSalaryScaleId = toSalaryScaleId;
        ToSalary = toSalary;
        Reason = reason;
        Remark = remark;
        TransferKind = transferKind;
        RelocationExpense = relocationExpense;
        base.Update();
    }

    /// <summary>Workflow-approved but future-dated (HC176): parked until the scheduler executes it on
    /// the effective date. Immutable except for execution or cancellation.</summary>
    public void MarkApproved()
    {
        EnsurePending();
        Status = MovementStatus.Approved;
        base.Update();
    }

    public void MarkExecuted(Guid? resolvedToBranchId)
    {
        EnsureExecutable();
        ToBranchId = resolvedToBranchId;
        Status = MovementStatus.Completed;
        ExecutedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Cancel()
    {
        EnsureExecutable();   // Pending or Approved (a parked future-dated action can still be called off)
        Status = MovementStatus.Cancelled;
        base.Update();
    }

    private void EnsurePending()
    {
        if (Status != MovementStatus.Pending)
            throw new InvalidOperationException($"A {Status} movement can no longer be modified.");
    }

    /// <summary>Execution/cancellation applies to recorded (Pending) or approved-awaiting-date rows.</summary>
    private void EnsureExecutable()
    {
        if (Status is not (MovementStatus.Pending or MovementStatus.Approved))
            throw new InvalidOperationException($"A {Status} movement can no longer be modified.");
    }

    private static void Guard(MovementType type, Guid? toPositionId, Guid? toSalaryScaleId, decimal? toSalary,
        TransferKind? transferKind, decimal? relocationExpense)
    {
        if (type == MovementType.Transfer && !toPositionId.HasValue)
            throw new ArgumentException("A transfer requires a target position.", nameof(toPositionId));
        // Salary rule: pay (salary / pay-point scale) may only change on a Promotion or Demotion —
        // a Transfer moves the employee without altering their compensation.
        if (type == MovementType.Transfer && (toSalaryScaleId.HasValue || toSalary.HasValue))
            throw new ArgumentException("A transfer cannot change the salary or pay scale — use a Promotion or Demotion for pay changes.", nameof(toSalary));
        if (type != MovementType.Transfer && !toPositionId.HasValue && !toSalaryScaleId.HasValue && !toSalary.HasValue)
            throw new ArgumentException("A promotion/demotion must change at least the position, salary scale or salary.", nameof(toSalaryScaleId));
        if (toSalary is < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(toSalary));
        // Transfer-request details apply to transfers only (HC171/HC173).
        if (type != MovementType.Transfer && (transferKind.HasValue || relocationExpense.HasValue))
            throw new ArgumentException("Transfer kind and relocation expense apply to transfers only.", nameof(transferKind));
        if (relocationExpense is < 0)
            throw new ArgumentException("Relocation expense cannot be negative.", nameof(relocationExpense));
    }
}
