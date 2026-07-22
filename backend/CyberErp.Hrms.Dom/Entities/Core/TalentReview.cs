using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a talent-review / calibration session.</summary>
public enum TalentReviewStatus
{
    Draft = 0,
    InProgress = 1,
    Completed = 2,
    /// <summary>Awaiting workflow approval (set on submission when an approval chain is configured).</summary>
    PendingApproval = 3,
    /// <summary>Rejected by the approval workflow; editable and resubmittable.</summary>
    Rejected = 4
}

/// <summary>
/// A talent-review / calibration session (HC149): HR and managers assess a cohort of employees on the
/// 9-box (performance × potential) grid and flag high-potentials. The per-employee placements live in
/// <see cref="TalentAssessment"/> rows (managed on their own, so a huge cohort is never loaded whole).
/// </summary>
public class TalentReview : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Cycle { get; private set; }
    public Guid? OrganizationUnitId { get; private set; }
    public TalentReviewStatus Status { get; private set; } = TalentReviewStatus.Draft;
    public string? Notes { get; private set; }

    private TalentReview() : base() { }

    public static TalentReview Create(string name, string? cycle, Guid? organizationUnitId, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Talent review name is required.", nameof(name));
        return new TalentReview
        {
            Name = name,
            Cycle = cycle,
            OrganizationUnitId = organizationUnitId,
            Notes = notes
        };
    }

    public void Update(string name, string? cycle, Guid? organizationUnitId, TalentReviewStatus status, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Talent review name is required.", nameof(name));
        Name = name;
        Cycle = cycle;
        OrganizationUnitId = organizationUnitId;
        Status = status;
        Notes = notes;
        base.Update();
    }

    /// <summary>Parks the review awaiting approval (an active workflow definition governs this tenant).</summary>
    public void MarkPendingApproval()
    {
        Status = TalentReviewStatus.PendingApproval;
        base.Update();
    }

    /// <summary>Workflow callback — an approved review goes straight into calibration (InProgress).</summary>
    public void ApproveViaWorkflow()
    {
        if (Status != TalentReviewStatus.PendingApproval) return; // idempotent
        Status = TalentReviewStatus.InProgress;
        base.Update();
    }

    /// <summary>Workflow callback — a rejected review stays editable for resubmission.</summary>
    public void RejectViaWorkflow()
    {
        if (Status != TalentReviewStatus.PendingApproval) return;
        Status = TalentReviewStatus.Rejected;
        base.Update();
    }
}
