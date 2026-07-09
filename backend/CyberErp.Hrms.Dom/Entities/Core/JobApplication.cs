using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Recruitment pipeline stages (HC098). OfferPending / Hired are entered by the offer and
/// onboarding stages of the module; Rejected / Withdrawn / Hired are terminal.
/// </summary>
public enum ApplicationStage
{
    Received = 0,
    Screening = 1,
    Shortlisted = 2,
    Interview = 3,
    Selected = 4,
    OfferPending = 5,
    Hired = 6,
    Rejected = 7,
    Withdrawn = 8
}

/// <summary>
/// One candidate's application to one requisition (unique pair) — the pipeline record HR works
/// (HC098): stage machine Received → Screening → Shortlisted → Interview (bypassable, HC102) →
/// Selected → …, screening outcome against the requisition criteria (HC095/HC099), and an
/// append-only stage log for full traceability of every transition.
/// </summary>
public class JobApplication : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid CandidateId { get; private set; }
    public Guid RequisitionId { get; private set; }
    public ApplicationStage Stage { get; private set; } = ApplicationStage.Received;
    public DateTime AppliedAt { get; private set; }
    /// <summary>Aggregate screening score against the requisition criteria (HC095).</summary>
    public decimal? ScreeningScore { get; private set; }
    public string? ScreeningRemarks { get; private set; }

    private readonly List<JobApplicationStageLog> _stageLog = [];
    public IReadOnlyCollection<JobApplicationStageLog> StageLog => _stageLog;

    private readonly List<ApplicationCriterionScore> _criterionScores = [];
    /// <summary>Per-criterion evaluator scores; the weighted total rolls up into ScreeningScore.</summary>
    public IReadOnlyCollection<ApplicationCriterionScore> CriterionScores => _criterionScores;

    private JobApplication() : base() { }

    public static JobApplication Create(Guid candidateId, Guid requisitionId, DateTime? appliedAt, string? actedBy)
    {
        if (candidateId == Guid.Empty)
            throw new ArgumentException("A candidate is required.", nameof(candidateId));
        if (requisitionId == Guid.Empty)
            throw new ArgumentException("A requisition is required.", nameof(requisitionId));

        var application = new JobApplication
        {
            CandidateId = candidateId,
            RequisitionId = requisitionId,
            AppliedAt = appliedAt ?? DateTime.UtcNow
        };
        application._stageLog.Add(
            JobApplicationStageLog.Create(application.Id, ApplicationStage.Received, "Application received", actedBy));
        return application;
    }

    /// <summary>
    /// Moves the application through the pipeline (HC098/HC099). Terminal stages are immutable;
    /// every transition is appended to the stage log. The interview stage may be skipped entirely
    /// (HC102) — transitions are not forced through it.
    /// </summary>
    public void MoveToStage(ApplicationStage stage, string? note, string? actedBy)
    {
        if (Stage is ApplicationStage.Rejected or ApplicationStage.Withdrawn or ApplicationStage.Hired)
            throw new InvalidOperationException($"A {Stage} application is final and can no longer move.");
        if (stage == Stage)
            throw new InvalidOperationException($"The application is already at the {stage} stage.");

        Stage = stage;
        _stageLog.Add(JobApplicationStageLog.Create(Id, stage, note, actedBy));
        base.Update();
    }

    /// <summary>Records the screening outcome (HC095/HC099) — typically alongside a stage move.</summary>
    public void RecordScreening(decimal? score, string? remarks)
    {
        if (score is < 0 or > 100)
            throw new ArgumentException("Screening score must be between 0 and 100.", nameof(score));
        ScreeningScore = score;
        ScreeningRemarks = remarks;
        base.Update();
    }

    /// <summary>
    /// Upserts one evaluator's score for a criterion and recomputes the application's total as the
    /// weighted average of all scored criteria — the auto-calculated ranking score.
    /// </summary>
    public void ScoreCriterion(Guid criterionId, decimal score, int weight, string? remarks, string? scoredBy)
    {
        if (criterionId == Guid.Empty)
            throw new ArgumentException("A criterion is required.", nameof(criterionId));
        if (score is < 0 or > 100)
            throw new ArgumentException("A criterion score must be between 0 and 100.", nameof(score));

        var existing = _criterionScores.FirstOrDefault(s => s.CriterionId == criterionId);
        if (existing is null)
            _criterionScores.Add(ApplicationCriterionScore.Create(Id, criterionId, score, weight, remarks, scoredBy));
        else
            existing.Rescore(score, weight, remarks, scoredBy);
        base.Update();
    }

    /// <summary>Total = Σ(score × weight) / Σ(weight) over the scored criteria.</summary>
    public void RecomputeScreeningScore()
    {
        if (_criterionScores.Count == 0) return;
        var totalWeight = _criterionScores.Sum(s => s.Weight);
        ScreeningScore = totalWeight == 0
            ? 0
            : Math.Round(_criterionScores.Sum(s => s.Score * s.Weight) / totalWeight, 2);
        base.Update();
    }
}

/// <summary>One evaluator's score of one screening criterion for one application.</summary>
public class ApplicationCriterionScore : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    /// <summary>The requisition screening criterion being scored.</summary>
    public Guid CriterionId { get; private set; }
    /// <summary>0–100.</summary>
    public decimal Score { get; private set; }
    /// <summary>Criterion weight snapshot at scoring time (drives the weighted total).</summary>
    public int Weight { get; private set; } = 1;
    public string? Remarks { get; private set; }
    public string? ScoredBy { get; private set; }
    public DateTime ScoredAt { get; private set; }

    private ApplicationCriterionScore() : base() { }

    public static ApplicationCriterionScore Create(
        Guid applicationId, Guid criterionId, decimal score, int weight, string? remarks, string? scoredBy)
    {
        return new ApplicationCriterionScore
        {
            ApplicationId = applicationId,
            CriterionId = criterionId,
            Score = score,
            Weight = weight < 1 ? 1 : weight,
            Remarks = remarks,
            ScoredBy = scoredBy,
            ScoredAt = DateTime.UtcNow
        };
    }

    public void Rescore(decimal score, int weight, string? remarks, string? scoredBy)
    {
        Score = score;
        Weight = weight < 1 ? 1 : weight;
        Remarks = remarks;
        ScoredBy = scoredBy;
        ScoredAt = DateTime.UtcNow;
        base.Update();
    }
}

/// <summary>One pipeline transition of an application (append-only traceability, HC098).</summary>
public class JobApplicationStageLog : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public ApplicationStage Stage { get; private set; }
    public string? Note { get; private set; }
    public string? ActedBy { get; private set; }
    public DateTime ActedAt { get; private set; }

    private JobApplicationStageLog() : base() { }

    public static JobApplicationStageLog Create(Guid applicationId, ApplicationStage stage, string? note, string? actedBy)
    {
        return new JobApplicationStageLog
        {
            ApplicationId = applicationId,
            Stage = stage,
            Note = note,
            ActedBy = actedBy,
            ActedAt = DateTime.UtcNow
        };
    }
}
