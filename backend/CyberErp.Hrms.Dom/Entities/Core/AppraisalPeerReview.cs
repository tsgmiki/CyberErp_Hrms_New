using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>State of a peer's invitation to review an appraisal.</summary>
public enum PeerReviewStatus
{
    Invited = 0,
    Submitted = 1,
    Declined = 2
}

/// <summary>
/// A peer's assessment of an appraisal (HC127). Kept appraisal-level (one score + comments per peer)
/// rather than per-line to avoid a line×peer fan-out; the submitted peer scores are averaged for the
/// manager/calibration to weigh, but do not alter the goals/competencies overall-score formula.
/// </summary>
public class AppraisalPeerReview : BaseEntity, IAuditable
{
    public Guid AppraisalId { get; private set; }
    public Guid PeerEmployeeId { get; private set; }
    public PeerReviewStatus Status { get; private set; } = PeerReviewStatus.Invited;
    public decimal? Score { get; private set; }
    public string? Comments { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    private AppraisalPeerReview() : base() { }

    public static AppraisalPeerReview Create(Guid appraisalId, Guid peerEmployeeId)
    {
        if (appraisalId == Guid.Empty)
            throw new ArgumentException("An appraisal is required.", nameof(appraisalId));
        if (peerEmployeeId == Guid.Empty)
            throw new ArgumentException("A peer employee is required.", nameof(peerEmployeeId));
        return new AppraisalPeerReview { AppraisalId = appraisalId, PeerEmployeeId = peerEmployeeId };
    }

    public void Submit(decimal? score, string? comments)
    {
        if (score is < 0)
            throw new ArgumentException("Score cannot be negative.", nameof(score));
        Score = score;
        Comments = comments;
        Status = PeerReviewStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Decline()
    {
        Status = PeerReviewStatus.Declined;
        base.Update();
    }
}
