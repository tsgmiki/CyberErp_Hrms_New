using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Driver behind a salary revision (HC228).</summary>
public enum SalaryRevisionType
{
    Merit = 0,
    Market = 1,
    CostOfLiving = 2,
    /// <summary>
    /// The award is banded by each employee's appraisal score instead of one flat <c>Rate</c>.
    /// Unlike the other values — which are labels describing WHY pay is being revised — this one
    /// changes HOW the amount is derived: the bands on the revision supply a per-employee value,
    /// expressed in the units of the chosen <see cref="SalaryAdjustmentBasis"/> (steps, percent or
    /// amount). Requires at least one band.
    /// </summary>
    Performance = 3
}

/// <summary>How the revision adjusts each salary.</summary>
public enum SalaryAdjustmentBasis
{
    /// <summary>A percentage uplift of the current salary.</summary>
    Percentage = 0,
    /// <summary>A flat amount added to the current salary.</summary>
    FixedAmount = 1,
    /// <summary>
    /// Advance the employee along their job grade's pay ladder by a (possibly fractional) number of
    /// steps, reading the new salary from the salary scale. A fractional landing point is linearly
    /// interpolated between the two nearest defined steps of that SAME grade.
    /// Here <c>Rate</c> is a step increment (e.g. 1.5), not a percentage or an amount.
    /// </summary>
    Step = 2
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
    /// <summary>
    /// Performance revisions only: which review cycle's appraisal supplies the score. Null means
    /// "each employee's most recently completed appraisal", which can mix cycle types (a probation
    /// appraisal for one person, an annual for another) — pin the cycle when that matters.
    /// </summary>
    public Guid? TargetReviewCycleId { get; private set; }
    public SalaryRevisionStatus Status { get; private set; } = SalaryRevisionStatus.Draft;
    public DateTime? AppliedOn { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<SalaryRevisionLine> _lines = [];
    public IReadOnlyCollection<SalaryRevisionLine> Lines => _lines;

    private readonly List<SalaryRevisionBand> _bands = [];
    /// <summary>Score bands for a Performance revision; empty for the flat-rate types.</summary>
    public IReadOnlyCollection<SalaryRevisionBand> Bands => _bands;

    /// <summary>
    /// Replaces the whole band set (draft only). Bands are stored highest-threshold-first and matched
    /// on <c>score &gt;= MinScore</c>, so the lowest band should use a floor of 0 to act as a catch-all.
    /// </summary>
    public void ReplaceBands(IEnumerable<(decimal MinScore, decimal Value, string? Label)> bands)
    {
        if (Status != SalaryRevisionStatus.Draft)
            throw new InvalidOperationException("Only a draft revision's bands can be edited.");

        var incoming = bands.OrderByDescending(b => b.MinScore).ToList();
        if (incoming.Select(b => b.MinScore).Distinct().Count() != incoming.Count)
            throw new ArgumentException("Two bands cannot share the same minimum score.", nameof(bands));

        _bands.Clear();
        foreach (var b in incoming)
            _bands.Add(SalaryRevisionBand.Create(Id, b.MinScore, b.Value, b.Label));
        base.Update();
    }

    private SalaryRevision() : base() { }

    public static SalaryRevision Create(string name, SalaryRevisionType type, SalaryAdjustmentBasis basis,
        decimal rate, DateTime effectiveDate, Guid? targetJobGradeId, Guid? targetOrganizationUnitId,
        Guid? targetReviewCycleId, string? notes)
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
            TargetReviewCycleId = targetReviewCycleId,
            Notes = notes
        };
    }

    public void UpdateDraft(string name, SalaryRevisionType type, SalaryAdjustmentBasis basis,
        decimal rate, DateTime effectiveDate, Guid? targetJobGradeId, Guid? targetOrganizationUnitId,
        Guid? targetReviewCycleId, string? notes)
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
        TargetReviewCycleId = targetReviewCycleId;
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

/// <summary>
/// One score band of a Performance revision: "score at or above <see cref="MinScore"/> earns
/// <see cref="Value"/>". <see cref="Value"/> is expressed in the units of the parent revision's
/// <see cref="SalaryAdjustmentBasis"/> — steps, percent, or a flat amount — so the same band set
/// means "2.5 steps" or "15%" depending on the basis chosen.
///
/// <para>Bands are DATA, not constants, because the appraisal score scale is configured per tenant:
/// live rating scales here run 1-5, 1-3 and 0-130. A hard-coded "&gt; 90" tier would never fire on a
/// 1-5 scale and would silently drop every employee into the lowest band.</para>
/// </summary>
public class SalaryRevisionBand : BaseEntity
{
    public Guid SalaryRevisionId { get; private set; }
    /// <summary>Inclusive lower bound: this band applies when <c>score &gt;= MinScore</c>.</summary>
    public decimal MinScore { get; private set; }
    /// <summary>Award for this band, in the parent revision's basis units.</summary>
    public decimal Value { get; private set; }
    /// <summary>Optional display label, e.g. "Exceeds expectations".</summary>
    public string? Label { get; private set; }

    private SalaryRevisionBand() : base() { }

    public static SalaryRevisionBand Create(Guid salaryRevisionId, decimal minScore, decimal value, string? label)
    {
        if (salaryRevisionId == Guid.Empty)
            throw new ArgumentException("Revision is required.", nameof(salaryRevisionId));
        if (minScore < 0)
            throw new ArgumentException("Minimum score cannot be negative.", nameof(minScore));
        if (value < 0)
            throw new ArgumentException("Band value cannot be negative.", nameof(value));

        return new SalaryRevisionBand
        {
            SalaryRevisionId = salaryRevisionId,
            MinScore = minScore,
            Value = value,
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim()
        };
    }
}
