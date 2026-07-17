using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>How soon an employee is ready for the target role (multi-level readiness, HC153/HC157).
/// Ordinal ascends with readiness so it sorts naturally.</summary>
public enum ReadinessLevel
{
    NotReady = 0,
    Ready3PlusYears = 1,
    Ready1To2Years = 2,
    ReadyNow = 3
}

/// <summary>The multi-rater input backing one 9-box placement (HC149) — a line manager / 2nd-level
/// manager / HR score for the employee's performance and potential.</summary>
public record TalentRatingSpec(Guid RaterEmployeeId, string? RaterRole, decimal? PerformanceScore, decimal? PotentialScore, string? Comment);

/// <summary>
/// One employee's placement on a <see cref="TalentReview"/>'s 9-box grid (HC148, HC150): a performance
/// band × potential band (1–3 each), a high-potential flag, and a readiness level, backed by the
/// multi-rater <see cref="TalentRating"/> inputs. The (review, perf-band, pot-band) index lets the
/// 9-box/heat-map counts be computed with a single server-side GROUP BY.
/// </summary>
public class TalentAssessment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TalentReviewId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public int PerformanceBand { get; private set; } = 2;
    public int PotentialBand { get; private set; } = 2;
    public bool IsHiPo { get; private set; }
    public ReadinessLevel Readiness { get; private set; } = ReadinessLevel.NotReady;
    public string? Notes { get; private set; }

    private Employee? _employee;
    public Employee? Employee => _employee;

    private readonly List<TalentRating> _ratings = [];
    public IReadOnlyCollection<TalentRating> Ratings => _ratings;

    private TalentAssessment() : base() { }

    private static int Band(int v) => v < 1 ? 1 : v > 3 ? 3 : v;

    public static TalentAssessment Create(Guid talentReviewId, Guid employeeId, int performanceBand, int potentialBand, bool isHiPo, ReadinessLevel readiness, string? notes)
    {
        if (talentReviewId == Guid.Empty) throw new ArgumentException("A talent review is required.", nameof(talentReviewId));
        if (employeeId == Guid.Empty) throw new ArgumentException("An employee is required.", nameof(employeeId));
        return new TalentAssessment
        {
            TalentReviewId = talentReviewId,
            EmployeeId = employeeId,
            PerformanceBand = Band(performanceBand),
            PotentialBand = Band(potentialBand),
            IsHiPo = isHiPo,
            Readiness = readiness,
            Notes = notes
        };
    }

    public void Update(Guid employeeId, int performanceBand, int potentialBand, bool isHiPo, ReadinessLevel readiness, string? notes)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("An employee is required.", nameof(employeeId));
        EmployeeId = employeeId;
        PerformanceBand = Band(performanceBand);
        PotentialBand = Band(potentialBand);
        IsHiPo = isHiPo;
        Readiness = readiness;
        Notes = notes;
        base.Update();
    }

    /// <summary>Flag/unflag as a high-potential (HC148 — talent-review calibration outcome).</summary>
    public void SetHiPo(bool isHiPo)
    {
        IsHiPo = isHiPo;
        base.Update();
    }

    /// <summary>Replace the multi-rater inputs (clear + re-add, so a save is idempotent).</summary>
    public void SetRatings(IEnumerable<TalentRatingSpec> ratings)
    {
        _ratings.Clear();
        foreach (var r in ratings)
            _ratings.Add(TalentRating.Create(Id, r.RaterEmployeeId, r.RaterRole, r.PerformanceScore, r.PotentialScore, r.Comment));
        base.Update();
    }
}

/// <summary>One rater's scores for a <see cref="TalentAssessment"/> (HC149 multi-rater).</summary>
public class TalentRating : BaseEntity
{
    public Guid TalentAssessmentId { get; private set; }
    public Guid RaterEmployeeId { get; private set; }
    public string? RaterRole { get; private set; }
    public decimal? PerformanceScore { get; private set; }
    public decimal? PotentialScore { get; private set; }
    public string? Comment { get; private set; }

    private TalentRating() : base() { }

    public static TalentRating Create(Guid talentAssessmentId, Guid raterEmployeeId, string? raterRole, decimal? performanceScore, decimal? potentialScore, string? comment)
    {
        if (raterEmployeeId == Guid.Empty) throw new ArgumentException("A rater is required.", nameof(raterEmployeeId));
        return new TalentRating
        {
            TalentAssessmentId = talentAssessmentId,
            RaterEmployeeId = raterEmployeeId,
            RaterRole = raterRole,
            PerformanceScore = performanceScore,
            PotentialScore = potentialScore,
            Comment = comment
        };
    }
}
