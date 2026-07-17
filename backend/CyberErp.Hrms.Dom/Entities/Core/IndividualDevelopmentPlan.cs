using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a development / improvement plan.</summary>
public enum DevelopmentPlanStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>Progress state of a single development action.</summary>
public enum DevelopmentActionStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2
}

/// <summary>
/// An Individual Development Plan (HC130–HC131): growth actions for an employee, optionally anchored to a
/// performance outcome (<see cref="AppraisalId"/>). Each <see cref="DevelopmentAction"/> targets a
/// competency gap (<see cref="DevelopmentAction.CompetencyId"/>) via a learning intervention, tracked to
/// completion by employee and manager.
/// </summary>
public class IndividualDevelopmentPlan : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid? AppraisalId { get; private set; }
    public Guid? ReviewCycleId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DevelopmentPlanStatus Status { get; private set; } = DevelopmentPlanStatus.Draft;

    private readonly List<DevelopmentAction> _actions = [];
    public IReadOnlyCollection<DevelopmentAction> Actions => _actions;

    private IndividualDevelopmentPlan() : base() { }

    public static IndividualDevelopmentPlan Create(Guid employeeId, string title, DateTime startDate, DateTime endDate,
        string? description = null, Guid? appraisalId = null, Guid? reviewCycleId = null,
        DevelopmentPlanStatus status = DevelopmentPlanStatus.Draft)
    {
        Guard(employeeId, title, startDate, endDate);
        return new IndividualDevelopmentPlan
        {
            EmployeeId = employeeId,
            Title = title,
            StartDate = startDate,
            EndDate = endDate,
            Description = description,
            AppraisalId = appraisalId,
            ReviewCycleId = reviewCycleId,
            Status = status
        };
    }

    public void Update(Guid employeeId, string title, DateTime startDate, DateTime endDate,
        string? description, Guid? appraisalId, Guid? reviewCycleId, DevelopmentPlanStatus status)
    {
        Guard(employeeId, title, startDate, endDate);
        EmployeeId = employeeId;
        Title = title;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
        AppraisalId = appraisalId;
        ReviewCycleId = reviewCycleId;
        Status = status;
        base.Update();
    }

    public void SetActions(IEnumerable<DevelopmentActionSpec> specs)
    {
        var list = specs.ToList();
        foreach (var s in list)
            if (string.IsNullOrWhiteSpace(s.Description))
                throw new ArgumentException("Development action description cannot be empty.", nameof(specs));

        _actions.Clear();
        var order = 0;
        foreach (var s in list)
        {
            _actions.Add(DevelopmentAction.Create(Id, s.Description, s.CompetencyId, s.LearningIntervention,
                s.TargetDate, s.Status, s.ProgressPercent, s.SortOrder != 0 ? s.SortOrder : order));
            order++;
        }
        base.Update();
    }

    private static void Guard(Guid employeeId, string title, DateTime startDate, DateTime endDate)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Plan title cannot be empty.", nameof(title));
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));
    }
}

/// <summary>Input spec for one development action when saving an IDP.</summary>
public record DevelopmentActionSpec(
    Guid? Id,
    string Description,
    Guid? CompetencyId,
    string? LearningIntervention,
    DateTime? TargetDate,
    DevelopmentActionStatus Status,
    int ProgressPercent,
    int SortOrder);

/// <summary>One development action of an <see cref="IndividualDevelopmentPlan"/> (HC131).</summary>
public class DevelopmentAction : BaseEntity
{
    public Guid DevelopmentPlanId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    /// <summary>The competency gap this action addresses (HC130) — optional.</summary>
    public Guid? CompetencyId { get; private set; }
    /// <summary>Learning intervention, e.g. "Course", "Mentoring", "On-the-job".</summary>
    public string? LearningIntervention { get; private set; }
    public DateTime? TargetDate { get; private set; }
    public DevelopmentActionStatus Status { get; private set; } = DevelopmentActionStatus.Planned;
    public int ProgressPercent { get; private set; }
    public int SortOrder { get; private set; }

    private DevelopmentAction() : base() { }

    public static DevelopmentAction Create(Guid developmentPlanId, string description, Guid? competencyId,
        string? learningIntervention, DateTime? targetDate, DevelopmentActionStatus status, int progressPercent, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Development action description cannot be empty.", nameof(description));
        if (progressPercent is < 0 or > 100)
            throw new ArgumentException("Progress must be between 0 and 100.", nameof(progressPercent));
        return new DevelopmentAction
        {
            DevelopmentPlanId = developmentPlanId,
            Description = description,
            CompetencyId = competencyId,
            LearningIntervention = learningIntervention,
            TargetDate = targetDate,
            Status = status,
            ProgressPercent = progressPercent,
            SortOrder = sortOrder
        };
    }
}
