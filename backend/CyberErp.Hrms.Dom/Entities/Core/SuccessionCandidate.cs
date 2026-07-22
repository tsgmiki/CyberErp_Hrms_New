using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Kind of development activity for a successor (HC156).</summary>
public enum SuccessionActionType
{
    Mentorship = 0,
    Training = 1,
    JobRotation = 2,
    Coaching = 3,
    Other = 4
}

public enum SuccessionActionStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum KnowledgeTransferStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}

public record SuccessionActionSpec(Guid? Id, SuccessionActionType Type, string Description, DateTime? DueDate, SuccessionActionStatus Status, Guid? MentorEmployeeId);
public record KnowledgeTransferSpec(Guid? Id, string Topic, Guid? FromEmployeeId, KnowledgeTransferStatus Status, DateTime? TargetDate, DateTime? CompletedDate);

/// <summary>
/// A ranked potential successor for a <see cref="SuccessionPlan"/> (HC153, HC154): readiness level +
/// a denormalised readiness score (0–100, computed from performance/competency inputs so lists never
/// recompute), a competency-gap summary, and the successor's development actions (HC156) and knowledge
/// transfer activities (HC160) as owned child collections.
/// </summary>
public class SuccessionCandidate : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid SuccessionPlanId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public int Rank { get; private set; }
    public ReadinessLevel Readiness { get; private set; } = ReadinessLevel.NotReady;
    /// <summary>Denormalised 0–100 readiness score (HC153) — recomputed only when its inputs change.</summary>
    public decimal? ReadinessScore { get; private set; }
    public string? GapSummary { get; private set; }
    public string? Notes { get; private set; }

    private Employee? _employee;
    public Employee? Employee => _employee;

    private readonly List<SuccessionDevelopmentAction> _developmentActions = [];
    public IReadOnlyCollection<SuccessionDevelopmentAction> DevelopmentActions => _developmentActions;

    private readonly List<KnowledgeTransfer> _knowledgeTransfers = [];
    public IReadOnlyCollection<KnowledgeTransfer> KnowledgeTransfers => _knowledgeTransfers;

    private SuccessionCandidate() : base() { }

    public static SuccessionCandidate Create(Guid successionPlanId, Guid employeeId, int rank, ReadinessLevel readiness, decimal? readinessScore, string? gapSummary, string? notes)
    {
        if (successionPlanId == Guid.Empty) throw new ArgumentException("A succession plan is required.", nameof(successionPlanId));
        if (employeeId == Guid.Empty) throw new ArgumentException("An employee is required.", nameof(employeeId));
        return new SuccessionCandidate
        {
            SuccessionPlanId = successionPlanId,
            EmployeeId = employeeId,
            Rank = rank,
            Readiness = readiness,
            ReadinessScore = readinessScore,
            GapSummary = gapSummary,
            Notes = notes
        };
    }

    public void Update(Guid employeeId, int rank, ReadinessLevel readiness, decimal? readinessScore, string? gapSummary, string? notes)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("An employee is required.", nameof(employeeId));
        EmployeeId = employeeId;
        Rank = rank;
        Readiness = readiness;
        ReadinessScore = readinessScore;
        GapSummary = gapSummary;
        Notes = notes;
        base.Update();
    }

    /// <summary>Set the (denormalized) readiness from a server-side computation (HC153) — leaves rank/notes intact.</summary>
    public void SetReadiness(ReadinessLevel readiness, decimal? readinessScore)
    {
        Readiness = readiness;
        ReadinessScore = readinessScore;
        base.Update();
    }

    public void SetDevelopmentActions(IEnumerable<SuccessionActionSpec> actions)
    {
        _developmentActions.Clear();
        foreach (var a in actions)
            _developmentActions.Add(SuccessionDevelopmentAction.Create(Id, a.Type, a.Description, a.DueDate, a.Status, a.MentorEmployeeId));
        base.Update();
    }

    public void SetKnowledgeTransfers(IEnumerable<KnowledgeTransferSpec> items)
    {
        _knowledgeTransfers.Clear();
        foreach (var k in items)
            _knowledgeTransfers.Add(KnowledgeTransfer.Create(Id, k.Topic, k.FromEmployeeId, k.Status, k.TargetDate, k.CompletedDate));
        base.Update();
    }
}

/// <summary>A development activity for a successor — mentorship / training / rotation (HC156).</summary>
public class SuccessionDevelopmentAction : BaseEntity
{
    public Guid SuccessionCandidateId { get; private set; }
    public SuccessionActionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime? DueDate { get; private set; }
    public SuccessionActionStatus Status { get; private set; } = SuccessionActionStatus.Planned;
    public Guid? MentorEmployeeId { get; private set; }

    private SuccessionDevelopmentAction() : base() { }

    public static SuccessionDevelopmentAction Create(Guid successionCandidateId, SuccessionActionType type, string description, DateTime? dueDate, SuccessionActionStatus status, Guid? mentorEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("A description is required.", nameof(description));
        return new SuccessionDevelopmentAction
        {
            SuccessionCandidateId = successionCandidateId,
            Type = type,
            Description = description,
            DueDate = dueDate,
            Status = status,
            MentorEmployeeId = mentorEmployeeId
        };
    }
}

/// <summary>A knowledge-transfer activity between the current role holder and a successor (HC160).</summary>
public class KnowledgeTransfer : BaseEntity
{
    public Guid SuccessionCandidateId { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public Guid? FromEmployeeId { get; private set; }
    public KnowledgeTransferStatus Status { get; private set; } = KnowledgeTransferStatus.NotStarted;
    public DateTime? TargetDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }

    private KnowledgeTransfer() : base() { }

    public static KnowledgeTransfer Create(Guid successionCandidateId, string topic, Guid? fromEmployeeId, KnowledgeTransferStatus status, DateTime? targetDate, DateTime? completedDate)
    {
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("A topic is required.", nameof(topic));
        return new KnowledgeTransfer
        {
            SuccessionCandidateId = successionCandidateId,
            Topic = topic,
            FromEmployeeId = fromEmployeeId,
            Status = status,
            TargetDate = targetDate,
            CompletedDate = completedDate
        };
    }
}
