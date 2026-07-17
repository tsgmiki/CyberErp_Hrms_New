using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of an individual goal.</summary>
public enum GoalStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>
/// An individual employee goal (HC119) set collaboratively by the employee and their manager, optionally
/// linked up to an <see cref="OrganizationalObjective"/> (HC120). Goals are SMART (HC121): specific
/// (title/description), measurable (<see cref="Measure"/> / <see cref="TargetValue"/>) and time-bound
/// (<see cref="StartDate"/>/<see cref="DueDate"/>), carry a weight (HC122), and own their action-plan
/// items (<see cref="GoalActionItem"/>, HC121).
/// </summary>
public class EmployeeGoal : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid ReviewCycleId { get; private set; }
    /// <summary>Links the goal up to an organizational objective (HC120) — null if unaligned.</summary>
    public Guid? OrganizationalObjectiveId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>SMART "measurable" — how success is measured (e.g. "% of tickets closed within SLA").</summary>
    public string? Measure { get; private set; }
    public decimal? TargetValue { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal Weight { get; private set; }
    /// <summary>Self-reported completion, 0–100.</summary>
    public int ProgressPercent { get; private set; }
    public GoalStatus Status { get; private set; } = GoalStatus.Draft;
    /// <summary>True when the goal originated from the manager rather than the employee (HC119).</summary>
    public bool SetByManager { get; private set; }

    private readonly List<GoalActionItem> _actionItems = [];
    public IReadOnlyCollection<GoalActionItem> ActionItems => _actionItems;

    private EmployeeGoal() : base() { }

    public static EmployeeGoal Create(Guid employeeId, Guid reviewCycleId, string title,
        DateTime startDate, DateTime dueDate, string? description = null, string? measure = null,
        decimal? targetValue = null, Guid? organizationalObjectiveId = null, decimal weight = 0,
        int progressPercent = 0, GoalStatus status = GoalStatus.Draft, bool setByManager = false)
    {
        Guard(employeeId, reviewCycleId, title, startDate, dueDate, weight, progressPercent);
        return new EmployeeGoal
        {
            EmployeeId = employeeId,
            ReviewCycleId = reviewCycleId,
            Title = title,
            StartDate = startDate,
            DueDate = dueDate,
            Description = description,
            Measure = measure,
            TargetValue = targetValue,
            OrganizationalObjectiveId = organizationalObjectiveId,
            Weight = weight,
            ProgressPercent = progressPercent,
            Status = status,
            SetByManager = setByManager
        };
    }

    public void Update(Guid employeeId, Guid reviewCycleId, string title, DateTime startDate, DateTime dueDate,
        string? description, string? measure, decimal? targetValue, Guid? organizationalObjectiveId,
        decimal weight, int progressPercent, GoalStatus status, bool setByManager)
    {
        Guard(employeeId, reviewCycleId, title, startDate, dueDate, weight, progressPercent);
        EmployeeId = employeeId;
        ReviewCycleId = reviewCycleId;
        Title = title;
        StartDate = startDate;
        DueDate = dueDate;
        Description = description;
        Measure = measure;
        TargetValue = targetValue;
        OrganizationalObjectiveId = organizationalObjectiveId;
        Weight = weight;
        ProgressPercent = progressPercent;
        Status = status;
        SetByManager = setByManager;
        base.Update();
    }

    /// <summary>Replaces the goal's action items from the submitted specs (HC121 action plan).</summary>
    public void SetActionItems(IEnumerable<GoalActionItemSpec> specs)
    {
        var list = specs.ToList();
        foreach (var s in list)
            if (string.IsNullOrWhiteSpace(s.Description))
                throw new ArgumentException("Action item description cannot be empty.", nameof(specs));

        _actionItems.Clear();
        var order = 0;
        foreach (var s in list)
        {
            _actionItems.Add(GoalActionItem.Create(Id, s.Description, s.DueDate, s.IsCompleted,
                s.SortOrder != 0 ? s.SortOrder : order));
            order++;
        }
        base.Update();
    }

    private static void Guard(Guid employeeId, Guid reviewCycleId, string title,
        DateTime startDate, DateTime dueDate, decimal weight, int progressPercent)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (reviewCycleId == Guid.Empty)
            throw new ArgumentException("A review cycle is required.", nameof(reviewCycleId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Goal title cannot be empty.", nameof(title));
        if (dueDate < startDate)
            throw new ArgumentException("Due date cannot be before the start date.", nameof(dueDate));
        if (weight is < 0 or > 100)
            throw new ArgumentException("Weight must be between 0 and 100.", nameof(weight));
        if (progressPercent is < 0 or > 100)
            throw new ArgumentException("Progress must be between 0 and 100.", nameof(progressPercent));
    }
}

/// <summary>Input spec for one action item when saving a goal (see <see cref="EmployeeGoal.SetActionItems"/>).</summary>
public record GoalActionItemSpec(
    Guid? Id,
    string Description,
    DateTime? DueDate,
    bool IsCompleted,
    int SortOrder);

/// <summary>One action-plan step of an <see cref="EmployeeGoal"/> (HC121).</summary>
public class GoalActionItem : BaseEntity
{
    public Guid EmployeeGoalId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime? DueDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public int SortOrder { get; private set; }

    private GoalActionItem() : base() { }

    public static GoalActionItem Create(Guid employeeGoalId, string description, DateTime? dueDate,
        bool isCompleted, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Action item description cannot be empty.", nameof(description));
        return new GoalActionItem
        {
            EmployeeGoalId = employeeGoalId,
            Description = description,
            DueDate = dueDate,
            IsCompleted = isCompleted,
            SortOrder = sortOrder
        };
    }
}
