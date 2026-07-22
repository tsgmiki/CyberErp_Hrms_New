using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a PIP.</summary>
public enum PipStatus
{
    Draft = 0,
    Active = 1,
    UnderReview = 2,
    Completed = 3
}

/// <summary>Recorded outcome of a PIP (HC135).</summary>
public enum PipOutcome
{
    Pending = 0,
    Successful = 1,
    Unsuccessful = 2,
    Extended = 3
}

/// <summary>Progress state of a single PIP objective.</summary>
public enum PipObjectiveStatus
{
    NotStarted = 0,
    InProgress = 1,
    Met = 2,
    NotMet = 3
}

/// <summary>
/// A Performance Improvement Plan (HC135): defined objectives with a review timeline
/// (<see cref="StartDate"/>–<see cref="EndDate"/>), progress tracking on each <see cref="PipObjective"/>,
/// and a recorded <see cref="Outcome"/> at the end. Optionally anchored to the triggering appraisal.
/// </summary>
public class PerformanceImprovementPlan : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid? AppraisalId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public PipStatus Status { get; private set; } = PipStatus.Draft;
    public PipOutcome Outcome { get; private set; } = PipOutcome.Pending;
    public string? OutcomeNotes { get; private set; }
    public DateTime? OutcomeRecordedAt { get; private set; }

    private readonly List<PipObjective> _objectives = [];
    public IReadOnlyCollection<PipObjective> Objectives => _objectives;

    private PerformanceImprovementPlan() : base() { }

    public static PerformanceImprovementPlan Create(Guid employeeId, string title, string reason,
        DateTime startDate, DateTime endDate, Guid? appraisalId = null, PipStatus status = PipStatus.Draft)
    {
        Guard(employeeId, title, reason, startDate, endDate);
        return new PerformanceImprovementPlan
        {
            EmployeeId = employeeId,
            Title = title,
            Reason = reason,
            StartDate = startDate,
            EndDate = endDate,
            AppraisalId = appraisalId,
            Status = status
        };
    }

    public void Update(Guid employeeId, string title, string reason, DateTime startDate, DateTime endDate,
        Guid? appraisalId, PipStatus status)
    {
        Guard(employeeId, title, reason, startDate, endDate);
        if (Status == PipStatus.Completed)
            throw new ArgumentException("A completed PIP can no longer be edited.");
        EmployeeId = employeeId;
        Title = title;
        Reason = reason;
        StartDate = startDate;
        EndDate = endDate;
        AppraisalId = appraisalId;
        Status = status;
        base.Update();
    }

    public void SetObjectives(IEnumerable<PipObjectiveSpec> specs)
    {
        var list = specs.ToList();
        foreach (var s in list)
            if (string.IsNullOrWhiteSpace(s.Description))
                throw new ArgumentException("PIP objective description cannot be empty.", nameof(specs));

        _objectives.Clear();
        var order = 0;
        foreach (var s in list)
        {
            _objectives.Add(PipObjective.Create(Id, s.Description, s.TargetDate, s.Status, s.ProgressPercent,
                s.SortOrder != 0 ? s.SortOrder : order));
            order++;
        }
        base.Update();
    }

    /// <summary>Record the final outcome (HC135) and close the plan.</summary>
    public void RecordOutcome(PipOutcome outcome, string? notes)
    {
        if (outcome == PipOutcome.Pending)
            throw new ArgumentException("Select a concrete outcome to record.", nameof(outcome));
        Outcome = outcome;
        OutcomeNotes = notes;
        OutcomeRecordedAt = DateTime.UtcNow;
        Status = PipStatus.Completed;
        base.Update();
    }

    private static void Guard(Guid employeeId, string title, string reason, DateTime startDate, DateTime endDate)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("PIP title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required.", nameof(reason));
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));
    }
}

/// <summary>Input spec for one PIP objective when saving a plan.</summary>
public record PipObjectiveSpec(
    Guid? Id,
    string Description,
    DateTime? TargetDate,
    PipObjectiveStatus Status,
    int ProgressPercent,
    int SortOrder);

/// <summary>One objective of a <see cref="PerformanceImprovementPlan"/> (HC135).</summary>
public class PipObjective : BaseEntity
{
    public Guid PipId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime? TargetDate { get; private set; }
    public PipObjectiveStatus Status { get; private set; } = PipObjectiveStatus.NotStarted;
    public int ProgressPercent { get; private set; }
    public int SortOrder { get; private set; }

    private PipObjective() : base() { }

    public static PipObjective Create(Guid pipId, string description, DateTime? targetDate,
        PipObjectiveStatus status, int progressPercent, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("PIP objective description cannot be empty.", nameof(description));
        if (progressPercent is < 0 or > 100)
            throw new ArgumentException("Progress must be between 0 and 100.", nameof(progressPercent));
        return new PipObjective
        {
            PipId = pipId,
            Description = description,
            TargetDate = targetDate,
            Status = status,
            ProgressPercent = progressPercent,
            SortOrder = sortOrder
        };
    }
}
