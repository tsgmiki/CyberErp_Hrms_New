using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Multi-stage flow of an appraisal (HC127): the employee self-assesses, the manager reviews, then it is
/// finalized. Peer assessment and calibration stages arrive in a later phase.
/// </summary>
public enum AppraisalStage
{
    SelfAssessment = 0,
    ManagerReview = 1,
    Completed = 2
}

/// <summary>Whether the employee has acknowledged (signed) or appealed a completed appraisal (HC142–HC144).</summary>
public enum AcknowledgmentStatus
{
    Pending = 0,
    Accepted = 1,
    Appealed = 2
}

/// <summary>Score set on one appraisal line (goal or competency).</summary>
public record AppraisalLineScore(Guid LineId, decimal? Score, string? Comments);

/// <summary>
/// An employee's scored appraisal for one review cycle (HC127/HC138). Generated from the employee's goals
/// (Phase B) and their position competencies, it carries the goals-vs-competencies weight split, runs
/// through the self → manager stages with stage-based locking (HC133), and yields a weighted overall
/// score plus a resolved rating level (HC138). Owns its goal + competency rating lines.
/// </summary>
public class Appraisal : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid ReviewCycleId { get; private set; }
    public Guid? AppraisalTemplateId { get; private set; }
    /// <summary>Start of the period this appraisal covers. Calendar cycles copy the cycle window; a
    /// probation cycle derives it from the employee's hire date (tenure-anchored, not calendar-fixed).</summary>
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal GoalsWeight { get; private set; }
    public decimal CompetenciesWeight { get; private set; }
    public AppraisalStage Stage { get; private set; } = AppraisalStage.SelfAssessment;
    public string? SelfComments { get; private set; }
    public string? ManagerComments { get; private set; }
    /// <summary>Weighted final score (HC138) — set when the manager review completes.</summary>
    public decimal? OverallScore { get; private set; }
    /// <summary>Denormalized rating-scale level matching <see cref="OverallScore"/> — for analytics.</summary>
    public Guid? FinalRatingLevelId { get; private set; }
    public DateTime? SelfSubmittedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    /// <summary>True once a calibration session has normalized this appraisal's score (HC128).</summary>
    public bool IsCalibrated { get; private set; }

    // Employee acknowledgment + signatures (HC142–HC144, HC146).
    public AcknowledgmentStatus AcknowledgmentStatus { get; private set; } = AcknowledgmentStatus.Pending;
    public string? EmployeeSignature { get; private set; }
    public DateTime? EmployeeSignedAt { get; private set; }
    public string? ManagerSignature { get; private set; }
    public DateTime? ManagerSignedAt { get; private set; }

    private readonly List<AppraisalGoal> _goals = [];
    public IReadOnlyCollection<AppraisalGoal> Goals => _goals;
    private readonly List<AppraisalCompetency> _competencies = [];
    public IReadOnlyCollection<AppraisalCompetency> Competencies => _competencies;

    private Appraisal() : base() { }

    public static Appraisal Create(Guid employeeId, Guid reviewCycleId, Guid? appraisalTemplateId,
        DateTime periodStart, DateTime periodEnd,
        decimal goalsWeight, decimal competenciesWeight, AppraisalStage startStage)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (reviewCycleId == Guid.Empty)
            throw new ArgumentException("A review cycle is required.", nameof(reviewCycleId));
        if (periodEnd < periodStart)
            throw new ArgumentException("Period end cannot be before the period start.", nameof(periodEnd));
        if (goalsWeight is < 0 or > 100 || competenciesWeight is < 0 or > 100)
            throw new ArgumentException("Weights must be between 0 and 100.", nameof(goalsWeight));
        return new Appraisal
        {
            EmployeeId = employeeId,
            ReviewCycleId = reviewCycleId,
            AppraisalTemplateId = appraisalTemplateId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GoalsWeight = goalsWeight,
            CompetenciesWeight = competenciesWeight,
            Stage = startStage
        };
    }

    public void AddGoalLine(Guid? employeeGoalId, string title, decimal weight) =>
        _goals.Add(AppraisalGoal.Create(Id, employeeGoalId, title, weight, _goals.Count));

    public void AddCompetencyLine(Guid competencyId, string competencyName, decimal weight) =>
        _competencies.Add(AppraisalCompetency.Create(Id, competencyId, competencyName, weight, _competencies.Count));

    /// <summary>Self-assessment scores — only writable while the appraisal is in the self stage (HC133).</summary>
    public void ApplySelfScores(string? comments,
        IEnumerable<AppraisalLineScore> goalScores, IEnumerable<AppraisalLineScore> competencyScores)
    {
        if (Stage != AppraisalStage.SelfAssessment)
            throw new ArgumentException("Self scores can only be edited during the self-assessment stage.");
        SelfComments = comments;
        foreach (var s in goalScores)
            _goals.FirstOrDefault(g => g.Id == s.LineId)?.SetSelfScore(s.Score, s.Comments);
        foreach (var s in competencyScores)
            _competencies.FirstOrDefault(c => c.Id == s.LineId)?.SetSelfScore(s.Score, s.Comments);
        base.Update();
    }

    /// <summary>Manager scores — only writable while the appraisal is in the manager-review stage (HC133).</summary>
    public void ApplyManagerScores(string? comments,
        IEnumerable<AppraisalLineScore> goalScores, IEnumerable<AppraisalLineScore> competencyScores)
    {
        if (Stage != AppraisalStage.ManagerReview)
            throw new ArgumentException("Manager scores can only be edited during the manager-review stage.");
        ManagerComments = comments;
        foreach (var s in goalScores)
            _goals.FirstOrDefault(g => g.Id == s.LineId)?.SetManagerScore(s.Score, s.Comments);
        foreach (var s in competencyScores)
            _competencies.FirstOrDefault(c => c.Id == s.LineId)?.SetManagerScore(s.Score, s.Comments);
        base.Update();
    }

    public void SubmitSelfAssessment()
    {
        if (Stage != AppraisalStage.SelfAssessment)
            throw new ArgumentException("Only a self-assessment can be submitted for manager review.");
        Stage = AppraisalStage.ManagerReview;
        SelfSubmittedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Finalize the manager review, recording the computed overall score + rating level (HC138).</summary>
    public void CompleteManagerReview(decimal overallScore, Guid? finalRatingLevelId)
    {
        if (Stage != AppraisalStage.ManagerReview)
            throw new ArgumentException("Only an appraisal in manager review can be completed.");
        Stage = AppraisalStage.Completed;
        OverallScore = overallScore;
        FinalRatingLevelId = finalRatingLevelId;
        CompletedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Overwrites the score with a calibrated value during moderation (HC128) — records the adjustment.</summary>
    public void ApplyCalibration(decimal overallScore, Guid? finalRatingLevelId)
    {
        OverallScore = overallScore;
        FinalRatingLevelId = finalRatingLevelId;
        IsCalibrated = true;
        base.Update();
    }

    /// <summary>Employee accepts the completed appraisal and digitally signs it (HC142/HC143).</summary>
    public void Acknowledge(string signature)
    {
        EnsureCompleted();
        if (AcknowledgmentStatus == AcknowledgmentStatus.Appealed)
            throw new ArgumentException("This appraisal is under appeal and cannot be acknowledged.");
        if (string.IsNullOrWhiteSpace(signature))
            throw new ArgumentException("A signature is required to acknowledge.", nameof(signature));
        AcknowledgmentStatus = AcknowledgmentStatus.Accepted;
        EmployeeSignature = signature;
        EmployeeSignedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Manager counter-signs so the report can carry both signatures (HC146).</summary>
    public void ManagerSign(string signature)
    {
        EnsureCompleted();
        if (string.IsNullOrWhiteSpace(signature))
            throw new ArgumentException("A signature is required.", nameof(signature));
        ManagerSignature = signature;
        ManagerSignedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Marks the appraisal as under appeal (HC143) — set when the employee submits an appeal.</summary>
    public void MarkAppealed()
    {
        EnsureCompleted();
        if (AcknowledgmentStatus == AcknowledgmentStatus.Accepted)
            throw new ArgumentException("An already-accepted appraisal cannot be appealed.");
        AcknowledgmentStatus = AcknowledgmentStatus.Appealed;
        base.Update();
    }

    private void EnsureCompleted()
    {
        if (Stage != AppraisalStage.Completed)
            throw new ArgumentException("Only a completed appraisal can be acknowledged, signed or appealed.");
    }
}

/// <summary>One goal being rated within an <see cref="Appraisal"/> (snapshot of an <see cref="EmployeeGoal"/>).</summary>
public class AppraisalGoal : BaseEntity
{
    public Guid AppraisalId { get; private set; }
    public Guid? EmployeeGoalId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Weight { get; private set; }
    public decimal? SelfScore { get; private set; }
    public string? SelfComments { get; private set; }
    public decimal? ManagerScore { get; private set; }
    public string? ManagerComments { get; private set; }
    public int SortOrder { get; private set; }

    private AppraisalGoal() : base() { }

    public static AppraisalGoal Create(Guid appraisalId, Guid? employeeGoalId, string title, decimal weight, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Appraisal goal title cannot be empty.", nameof(title));
        return new AppraisalGoal
        {
            AppraisalId = appraisalId,
            EmployeeGoalId = employeeGoalId,
            Title = title,
            Weight = weight,
            SortOrder = sortOrder
        };
    }

    public void SetSelfScore(decimal? score, string? comments) { SelfScore = score; SelfComments = comments; }
    public void SetManagerScore(decimal? score, string? comments) { ManagerScore = score; ManagerComments = comments; }
}

/// <summary>One competency being rated within an <see cref="Appraisal"/> (snapshot of a position competency).</summary>
public class AppraisalCompetency : BaseEntity
{
    public Guid AppraisalId { get; private set; }
    public Guid CompetencyId { get; private set; }
    public string CompetencyName { get; private set; } = string.Empty;
    public decimal Weight { get; private set; }
    public decimal? SelfScore { get; private set; }
    public string? SelfComments { get; private set; }
    public decimal? ManagerScore { get; private set; }
    public string? ManagerComments { get; private set; }
    public int SortOrder { get; private set; }

    private AppraisalCompetency() : base() { }

    public static AppraisalCompetency Create(Guid appraisalId, Guid competencyId, string competencyName, decimal weight, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(competencyName))
            throw new ArgumentException("Appraisal competency name cannot be empty.", nameof(competencyName));
        return new AppraisalCompetency
        {
            AppraisalId = appraisalId,
            CompetencyId = competencyId,
            CompetencyName = competencyName,
            Weight = weight,
            SortOrder = sortOrder
        };
    }

    public void SetSelfScore(decimal? score, string? comments) { SelfScore = score; SelfComments = comments; }
    public void SetManagerScore(decimal? score, string? comments) { ManagerScore = score; ManagerComments = comments; }
}
