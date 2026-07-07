using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Sanction applied by a disciplinary case.</summary>
public enum DisciplinaryMeasureType
{
    VerbalWarning = 0,
    WrittenWarning = 1,
    FinalWarning = 2,
    Suspension = 3,
    SalaryDeduction = 4,
    Demotion = 5,
    Termination = 6
}

/// <summary>Lifecycle of a disciplinary case.</summary>
public enum DisciplinaryStatus
{
    Open = 0,
    UnderReview = 1,
    Resolved = 2,
    Cancelled = 3
}

/// <summary>
/// Disciplinary case record: a violation, the measure taken and its lifecycle. Kept as an
/// append-style case history per employee (enterprise HR keeps discipline separate from the
/// movement log; a resulting demotion is recorded as its own <see cref="EmployeeMovement"/>).
/// </summary>
public class DisciplinaryMeasure : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public DateTime ViolationDate { get; private set; }
    /// <summary>Short classification, e.g. Absenteeism, Misconduct, Policy Violation.</summary>
    public string ViolationType { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DisciplinaryMeasureType MeasureType { get; private set; }
    public DisciplinaryStatus Status { get; private set; } = DisciplinaryStatus.Open;
    /// <summary>When the measure takes effect (e.g. suspension start).</summary>
    public DateTime? EffectiveDate { get; private set; }
    public string? Resolution { get; private set; }

    private DisciplinaryMeasure() : base() { }

    public static DisciplinaryMeasure Create(
        Guid employeeId,
        DateTime violationDate,
        string violationType,
        DisciplinaryMeasureType measureType,
        DisciplinaryStatus status = DisciplinaryStatus.Open,
        string? description = null,
        DateTime? effectiveDate = null,
        string? resolution = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(violationType))
            throw new ArgumentException("Violation type cannot be empty.", nameof(violationType));

        return new DisciplinaryMeasure
        {
            EmployeeId = employeeId,
            ViolationDate = violationDate,
            ViolationType = violationType,
            MeasureType = measureType,
            Status = status,
            Description = description,
            EffectiveDate = effectiveDate,
            Resolution = resolution
        };
    }

    /// <summary>Workflow outcome hook: approval confirms the measure, rejection voids the case.</summary>
    public void SetStatus(DisciplinaryStatus status)
    {
        Status = status;
        base.Update();
    }

    public void Update(
        DateTime violationDate,
        string violationType,
        DisciplinaryMeasureType measureType,
        DisciplinaryStatus status,
        string? description,
        DateTime? effectiveDate,
        string? resolution)
    {
        if (string.IsNullOrWhiteSpace(violationType))
            throw new ArgumentException("Violation type cannot be empty.", nameof(violationType));

        ViolationDate = violationDate;
        ViolationType = violationType;
        MeasureType = measureType;
        Status = status;
        Description = description;
        EffectiveDate = effectiveDate;
        Resolution = resolution;
        base.Update();
    }
}
