using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Cadence of an appraisal cycle (HC126).</summary>
public enum ReviewPeriodType
{
    Annual = 0,
    BiAnnual = 1,
    Quarterly = 2,
    Probation = 3,
    Custom = 4
}

/// <summary>Lifecycle of a review cycle: configured → open for appraisals → closed.</summary>
public enum ReviewCycleStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2
}

/// <summary>
/// A configurable appraisal cycle (HC126) — the container an appraisal run happens in. Anchors to an
/// optional fiscal year, applies a <see cref="RatingScale"/>, and configures which stages are enabled
/// (self / peer / calibration, HC127–HC128). Appraisal generation + status transitions arrive in a
/// later phase; here it is configuration only.
/// </summary>
public class ReviewCycle : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public ReviewPeriodType PeriodType { get; private set; }
    public Guid? FiscalYearId { get; private set; }
    public Guid RatingScaleId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime? SelfReviewDue { get; private set; }
    public DateTime? ManagerReviewDue { get; private set; }
    public bool EnableSelfAssessment { get; private set; } = true;
    public bool EnablePeerAssessment { get; private set; }
    public bool EnableCalibration { get; private set; }
    public ReviewCycleStatus Status { get; private set; } = ReviewCycleStatus.Draft;
    /// <summary>
    /// For a <see cref="ReviewPeriodType.Probation"/> cycle, the probation length in months. When set, an
    /// appraisal's period end is computed as hire date + this many months (rather than reading the
    /// employee's stored probation-end date). Null = derive from the employee's ProbationEndDate.
    /// </summary>
    public int? ProbationDurationMonths { get; private set; }

    private ReviewCycle() : base() { }

    public static ReviewCycle Create(string name, ReviewPeriodType periodType, Guid ratingScaleId,
        DateTime startDate, DateTime endDate, Guid? fiscalYearId = null,
        DateTime? selfReviewDue = null, DateTime? managerReviewDue = null,
        bool enableSelfAssessment = true, bool enablePeerAssessment = false, bool enableCalibration = false,
        int? probationDurationMonths = null)
    {
        Guard(name, ratingScaleId, startDate, endDate, probationDurationMonths);
        return new ReviewCycle
        {
            Name = name,
            PeriodType = periodType,
            RatingScaleId = ratingScaleId,
            StartDate = startDate,
            EndDate = endDate,
            FiscalYearId = fiscalYearId,
            SelfReviewDue = selfReviewDue,
            ManagerReviewDue = managerReviewDue,
            EnableSelfAssessment = enableSelfAssessment,
            EnablePeerAssessment = enablePeerAssessment,
            EnableCalibration = enableCalibration,
            ProbationDurationMonths = probationDurationMonths
        };
    }

    public void Update(string name, ReviewPeriodType periodType, Guid ratingScaleId,
        DateTime startDate, DateTime endDate, Guid? fiscalYearId,
        DateTime? selfReviewDue, DateTime? managerReviewDue,
        bool enableSelfAssessment, bool enablePeerAssessment, bool enableCalibration,
        int? probationDurationMonths = null)
    {
        Guard(name, ratingScaleId, startDate, endDate, probationDurationMonths);
        Name = name;
        PeriodType = periodType;
        RatingScaleId = ratingScaleId;
        StartDate = startDate;
        EndDate = endDate;
        FiscalYearId = fiscalYearId;
        SelfReviewDue = selfReviewDue;
        ManagerReviewDue = managerReviewDue;
        EnableSelfAssessment = enableSelfAssessment;
        EnablePeerAssessment = enablePeerAssessment;
        EnableCalibration = enableCalibration;
        ProbationDurationMonths = probationDurationMonths;
        base.Update();
    }

    private static void Guard(string name, Guid ratingScaleId, DateTime startDate, DateTime endDate,
        int? probationDurationMonths)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Review cycle name cannot be empty.", nameof(name));
        if (ratingScaleId == Guid.Empty)
            throw new ArgumentException("A rating scale is required.", nameof(ratingScaleId));
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));
        if (probationDurationMonths is < 1 or > 24)
            throw new ArgumentException("Probation duration must be between 1 and 24 months.", nameof(probationDurationMonths));
    }
}
