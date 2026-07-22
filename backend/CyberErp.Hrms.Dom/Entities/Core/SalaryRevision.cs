using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Driver behind a salary revision (HC228).</summary>
public enum SalaryRevisionType
{
    Merit = 0,
    Market = 1,
    CostOfLiving = 2
}

/// <summary>How the revision adjusts each salary.</summary>
public enum SalaryAdjustmentBasis
{
    /// <summary>A percentage uplift of the current salary.</summary>
    Percentage = 0,
    /// <summary>A flat amount added to the current salary.</summary>
    FixedAmount = 1
}

/// <summary>Lifecycle of a salary revision plan.</summary>
public enum SalaryRevisionStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Applied = 3,
    Cancelled = 4
}

/// <summary>
/// HC228 — a salary revision plan: a merit/market/COLA adjustment applied to a targeted set of
/// employees (optionally filtered by grade/unit). Its <see cref="SalaryRevisionLine"/>s are the
/// materialized scenario (current → proposed per employee); on Apply each employee's base salary is
/// updated. Workflow-enabled (<c>WorkflowEntityTypes.SalaryRevision</c>).
/// </summary>
public class SalaryRevision : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public SalaryRevisionType RevisionType { get; private set; }
    public SalaryAdjustmentBasis Basis { get; private set; }
    /// <summary>Percent uplift (Percentage) or flat amount (FixedAmount).</summary>
    public decimal Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    /// <summary>Optional target filters (null = all employees with a base salary).</summary>
    public Guid? TargetJobGradeId { get; private set; }
    public Guid? TargetOrganizationUnitId { get; private set; }
    public SalaryRevisionStatus Status { get; private set; } = SalaryRevisionStatus.Draft;
    public DateTime? AppliedOn { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<SalaryRevisionLine> _lines = [];
    public IReadOnlyCollection<SalaryRevisionLine> Lines => _lines;

    private SalaryRevision() : base() { }

    public static SalaryRevision Create(string name, SalaryRevisionType type, SalaryAdjustmentBasis basis,
        decimal rate, DateTime effectiveDate, Guid? targetJobGradeId, Guid? targetOrganizationUnitId, string? notes)
    {
        Guard(name, basis, rate);
        return new SalaryRevision
        {
            Name = name.Trim(),
            RevisionType = type,
            Basis = basis,
            Rate = rate,
            EffectiveDate = effectiveDate,
            TargetJobGradeId = targetJobGradeId,
            TargetOrganizationUnitId = targetOrganizationUnitId,
            Notes = notes
        };
    }

    public void UpdateDraft(string name, SalaryRevisionType type, SalaryAdjustmentBasis basis,
        decimal rate, DateTime effectiveDate, Guid? targetJobGradeId, Guid? targetOrganizationUnitId, string? notes)
    {
        if (Status != SalaryRevisionStatus.Draft)
            throw new InvalidOperationException("Only a draft revision can be edited.");
        Guard(name, basis, rate);
        Name = name.Trim();
        RevisionType = type;
        Basis = basis;
        Rate = rate;
        EffectiveDate = effectiveDate;
        TargetJobGradeId = targetJobGradeId;
        TargetOrganizationUnitId = targetOrganizationUnitId;
        Notes = notes;
        base.Update();
    }

    /// <summary>Draft → PendingApproval when routed to the approval chain.</summary>
    public void Submit()
    {
        if (Status != SalaryRevisionStatus.Draft)
            throw new InvalidOperationException("Only a draft revision can be submitted.");
        Status = SalaryRevisionStatus.PendingApproval;
        base.Update();
    }

    public void Approve()
    {
        if (Status != SalaryRevisionStatus.PendingApproval)
            throw new InvalidOperationException("Only a pending revision can be approved.");
        Status = SalaryRevisionStatus.Approved;
        base.Update();
    }

    public void Reject()
    {
        if (Status is SalaryRevisionStatus.Applied)
            throw new InvalidOperationException("An applied revision cannot be cancelled.");
        Status = SalaryRevisionStatus.Cancelled;
        base.Update();
    }

    public void MarkApplied(DateTime appliedOn)
    {
        if (Status != SalaryRevisionStatus.Approved)
            throw new InvalidOperationException("Only an approved revision can be applied.");
        Status = SalaryRevisionStatus.Applied;
        AppliedOn = appliedOn;
        base.Update();
    }

    private static void Guard(string name, SalaryAdjustmentBasis basis, decimal rate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Revision name cannot be empty.", nameof(name));
        if (rate < 0)
            throw new ArgumentException("Rate cannot be negative.", nameof(rate));
        if (basis == SalaryAdjustmentBasis.Percentage && rate > 100)
            throw new ArgumentException("A percentage revision cannot exceed 100.", nameof(rate));
    }
}

/// <summary>One employee's line in a salary revision: current → proposed (HC228).</summary>
public class SalaryRevisionLine : BaseEntity
{
    public Guid SalaryRevisionId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal CurrentSalary { get; private set; }
    public decimal ProposedSalary { get; private set; }

    private SalaryRevisionLine() : base() { }

    public static SalaryRevisionLine Create(Guid salaryRevisionId, Guid employeeId, decimal currentSalary, decimal proposedSalary)
    {
        if (salaryRevisionId == Guid.Empty)
            throw new ArgumentException("Revision is required.", nameof(salaryRevisionId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        return new SalaryRevisionLine
        {
            SalaryRevisionId = salaryRevisionId,
            EmployeeId = employeeId,
            CurrentSalary = currentSalary,
            ProposedSalary = proposedSalary
        };
    }

    /// <summary>HR override of the computed proposal (Draft only — enforced by the handler).</summary>
    public void SetProposed(decimal proposedSalary)
    {
        if (proposedSalary < 0)
            throw new ArgumentException("Proposed salary cannot be negative.", nameof(proposedSalary));
        ProposedSalary = proposedSalary;
        base.Update();
    }
}
