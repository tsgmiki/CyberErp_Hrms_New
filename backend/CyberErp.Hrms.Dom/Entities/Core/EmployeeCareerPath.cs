using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum EmployeeCareerPathStatus
{
    Active = 0,
    Completed = 1,
    OnHold = 2
}

public enum CareerStepProgressStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}

public record CareerStepProgressSpec(Guid CareerPathStepId, CareerStepProgressStatus Status, DateTime? CompletedDate, string? Notes);

/// <summary>
/// An employee's assignment to a <see cref="CareerPath"/> (HC163) with progress tracking (HC165): the
/// current step, a denormalised progress %, and per-step milestone status
/// (<see cref="EmployeeCareerPathStepProgress"/>). Progress % is recomputed from the step statuses so
/// lists never recompute it.
/// </summary>
public class EmployeeCareerPath : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid CareerPathId { get; private set; }
    public Guid? CurrentStepId { get; private set; }
    public string? AssignedBy { get; private set; }
    public DateTime? AssignedDate { get; private set; }
    public decimal ProgressPercent { get; private set; }
    public EmployeeCareerPathStatus Status { get; private set; } = EmployeeCareerPathStatus.Active;
    public string? Notes { get; private set; }

    private Employee? _employee;
    public Employee? Employee => _employee;
    private CareerPath? _careerPath;
    public CareerPath? CareerPath => _careerPath;

    private readonly List<EmployeeCareerPathStepProgress> _stepProgress = [];
    public IReadOnlyCollection<EmployeeCareerPathStepProgress> StepProgress => _stepProgress;

    private EmployeeCareerPath() : base() { }

    public static EmployeeCareerPath Create(Guid employeeId, Guid careerPathId, Guid? currentStepId, string? assignedBy, DateTime? assignedDate, EmployeeCareerPathStatus status, string? notes)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (careerPathId == Guid.Empty) throw new ArgumentException("A career path is required.", nameof(careerPathId));
        return new EmployeeCareerPath
        {
            EmployeeId = employeeId,
            CareerPathId = careerPathId,
            CurrentStepId = currentStepId,
            AssignedBy = assignedBy,
            AssignedDate = assignedDate ?? DateTime.UtcNow,
            Status = status,
            Notes = notes,
        };
    }

    public void Update(Guid employeeId, Guid careerPathId, Guid? currentStepId, string? assignedBy, EmployeeCareerPathStatus status, string? notes)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (careerPathId == Guid.Empty) throw new ArgumentException("A career path is required.", nameof(careerPathId));
        EmployeeId = employeeId;
        CareerPathId = careerPathId;
        CurrentStepId = currentStepId;
        AssignedBy = assignedBy;
        Status = status;
        Notes = notes;
        base.Update();
    }

    /// <summary>Replace per-step progress and recompute the denormalised progress % (completed ÷ total).</summary>
    public void SetStepProgress(IEnumerable<CareerStepProgressSpec> progress)
    {
        _stepProgress.Clear();
        foreach (var p in progress)
            _stepProgress.Add(EmployeeCareerPathStepProgress.Create(Id, p.CareerPathStepId, p.Status, p.CompletedDate, p.Notes));
        RecomputeProgress();
        base.Update();
    }

    private void RecomputeProgress()
    {
        var total = _stepProgress.Count;
        var completed = _stepProgress.Count(s => s.Status == CareerStepProgressStatus.Completed);
        ProgressPercent = total > 0 ? Math.Round((decimal)completed / total * 100m, 1) : 0m;
    }
}

/// <summary>Per-step milestone status for an employee on a career path (HC165).</summary>
public class EmployeeCareerPathStepProgress : BaseEntity
{
    public Guid EmployeeCareerPathId { get; private set; }
    public Guid CareerPathStepId { get; private set; }
    public CareerStepProgressStatus Status { get; private set; } = CareerStepProgressStatus.NotStarted;
    public DateTime? CompletedDate { get; private set; }
    public string? Notes { get; private set; }

    private EmployeeCareerPathStepProgress() : base() { }

    public static EmployeeCareerPathStepProgress Create(Guid employeeCareerPathId, Guid careerPathStepId, CareerStepProgressStatus status, DateTime? completedDate, string? notes)
    {
        if (careerPathStepId == Guid.Empty) throw new ArgumentException("A career path step is required.", nameof(careerPathStepId));
        return new EmployeeCareerPathStepProgress
        {
            EmployeeCareerPathId = employeeCareerPathId,
            CareerPathStepId = careerPathStepId,
            Status = status,
            CompletedDate = completedDate,
            Notes = notes,
        };
    }
}
