using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>What an award materially confers when granted (HC177).</summary>
public enum RewardKind
{
    Badge = 0,
    Certificate = 1,
    GiftCard = 2,
    MonetaryBonus = 3,
    PointsBased = 4,
    RisingStar = 5,
}

/// <summary>
/// A configurable award / badge (HC141, HC177) that can be granted to recognize high performers, e.g.
/// "Employee of the Month", "Innovator". Carries display metadata (colour, icon) for public boards plus
/// its reward value: kind, optional monetary amount (disbursed via <see cref="RewardDisbursement"/>),
/// redeemable points, eligibility criteria and an optional auto-grant performance threshold (HC181).
/// </summary>
public class RecognitionBadge : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Display colour (hex) for the badge chip.</summary>
    public string? Color { get; private set; }
    /// <summary>Lucide icon name for the badge.</summary>
    public string? Icon { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }
    public RewardKind RewardKind { get; private set; } = RewardKind.Badge;
    /// <summary>Monetary value handed to payroll/finance on grant — GiftCard / MonetaryBonus kinds only.</summary>
    public decimal? MonetaryValue { get; private set; }
    /// <summary>Redeemable points credited to the employee's ledger on grant (0 = none).</summary>
    public int PointsValue { get; private set; }
    /// <summary>Eligibility criteria shown to nominators (HC178).</summary>
    public string? Criteria { get; private set; }
    /// <summary>Normalised appraisal score (%) at or above which the badge is auto-granted (HC181); null = manual only.</summary>
    public decimal? AutoGrantMinScore { get; private set; }
    public Guid? AwardCategoryId { get; private set; }

    private RecognitionBadge() : base() { }

    public static RecognitionBadge Create(string name, string? description, string? color, string? icon,
        bool isActive = true, int sortOrder = 0, RewardKind rewardKind = RewardKind.Badge,
        decimal? monetaryValue = null, int pointsValue = 0, string? criteria = null,
        decimal? autoGrantMinScore = null, Guid? awardCategoryId = null)
    {
        Guard(name, rewardKind, monetaryValue, pointsValue, autoGrantMinScore);
        return new RecognitionBadge
        {
            Name = name,
            Description = description,
            Color = color,
            Icon = icon,
            IsActive = isActive,
            SortOrder = sortOrder,
            RewardKind = rewardKind,
            MonetaryValue = monetaryValue,
            PointsValue = pointsValue,
            Criteria = criteria,
            AutoGrantMinScore = autoGrantMinScore,
            AwardCategoryId = awardCategoryId
        };
    }

    public void Update(string name, string? description, string? color, string? icon, bool isActive, int sortOrder,
        RewardKind rewardKind = RewardKind.Badge, decimal? monetaryValue = null, int pointsValue = 0,
        string? criteria = null, decimal? autoGrantMinScore = null, Guid? awardCategoryId = null)
    {
        Guard(name, rewardKind, monetaryValue, pointsValue, autoGrantMinScore);
        Name = name;
        Description = description;
        Color = color;
        Icon = icon;
        IsActive = isActive;
        SortOrder = sortOrder;
        RewardKind = rewardKind;
        MonetaryValue = monetaryValue;
        PointsValue = pointsValue;
        Criteria = criteria;
        AutoGrantMinScore = autoGrantMinScore;
        AwardCategoryId = awardCategoryId;
        base.Update();
    }

    private static void Guard(string name, RewardKind rewardKind, decimal? monetaryValue, int pointsValue,
        decimal? autoGrantMinScore)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Badge name cannot be empty.", nameof(name));
        if (monetaryValue is < 0)
            throw new ArgumentException("Monetary value cannot be negative.", nameof(monetaryValue));
        if (monetaryValue is > 0 && rewardKind is not (RewardKind.GiftCard or RewardKind.MonetaryBonus))
            throw new ArgumentException("Only gift-card or monetary-bonus awards carry a monetary value.", nameof(monetaryValue));
        if (pointsValue < 0)
            throw new ArgumentException("Points value cannot be negative.", nameof(pointsValue));
        if (autoGrantMinScore is < 0 or > 100)
            throw new ArgumentException("Auto-grant threshold must be a percentage between 0 and 100.", nameof(autoGrantMinScore));
    }
}

/// <summary>
/// A recognition granted to an employee — a <see cref="RecognitionBadge"/> awarded with a citation
/// (HC141). Public grants surface on the recognition board; who/when granted come from the audit
/// interceptor.
/// </summary>
public class EmployeeRecognition : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid RecognitionBadgeId { get; private set; }
    public string Citation { get; private set; } = string.Empty;
    public DateTime RecognizedOn { get; private set; }
    public bool IsPublic { get; private set; } = true;
    /// <summary>
    /// What produced the grant — "Nomination:{id}" (HC179) or "Appraisal:{id}" (HC181); null = direct
    /// manual grant. Doubles as the idempotency key so a source never grants the same badge twice.
    /// </summary>
    public string? SourceRef { get; private set; }

    private EmployeeRecognition() : base() { }

    public static EmployeeRecognition Create(Guid employeeId, Guid recognitionBadgeId, string citation,
        DateTime recognizedOn, bool isPublic = true, string? sourceRef = null)
    {
        Guard(employeeId, recognitionBadgeId, citation);
        return new EmployeeRecognition
        {
            EmployeeId = employeeId,
            RecognitionBadgeId = recognitionBadgeId,
            Citation = citation,
            RecognizedOn = recognizedOn,
            IsPublic = isPublic,
            SourceRef = sourceRef
        };
    }

    public void Update(Guid employeeId, Guid recognitionBadgeId, string citation, DateTime recognizedOn, bool isPublic)
    {
        Guard(employeeId, recognitionBadgeId, citation);
        EmployeeId = employeeId;
        RecognitionBadgeId = recognitionBadgeId;
        Citation = citation;
        RecognizedOn = recognizedOn;
        IsPublic = isPublic;
        base.Update();
    }

    private static void Guard(Guid employeeId, Guid recognitionBadgeId, string citation)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (recognitionBadgeId == Guid.Empty)
            throw new ArgumentException("A badge is required.", nameof(recognitionBadgeId));
        if (string.IsNullOrWhiteSpace(citation))
            throw new ArgumentException("A citation is required.", nameof(citation));
    }
}
