using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of an appraisal appeal reviewed by HR / management (HC144).</summary>
public enum AppealStatus
{
    Open = 0,
    UnderReview = 1,
    Resolved = 2,
    Rejected = 3
}

/// <summary>
/// An employee's appeal against their completed appraisal (HC143). Carries the employee's comments and
/// an optional request for a follow-up discussion, and moves through an HR/management review flow (HC144):
/// Open → UnderReview → Resolved / Rejected, with a documented resolution.
/// </summary>
public class AppraisalAppeal : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid AppraisalId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Comments { get; private set; } = string.Empty;
    public bool RequestFollowUp { get; private set; }
    public AppealStatus Status { get; private set; } = AppealStatus.Open;
    public string? Resolution { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private AppraisalAppeal() : base() { }

    public static AppraisalAppeal Create(Guid appraisalId, Guid employeeId, string comments, bool requestFollowUp)
    {
        if (appraisalId == Guid.Empty)
            throw new ArgumentException("An appraisal is required.", nameof(appraisalId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(comments))
            throw new ArgumentException("Appeal comments are required.", nameof(comments));
        return new AppraisalAppeal
        {
            AppraisalId = appraisalId,
            EmployeeId = employeeId,
            Comments = comments,
            RequestFollowUp = requestFollowUp
        };
    }

    public void StartReview()
    {
        if (Status != AppealStatus.Open)
            throw new ArgumentException("Only an open appeal can be moved to review.");
        Status = AppealStatus.UnderReview;
        base.Update();
    }

    /// <summary>Close the appeal with a decision (HC144): resolved (upheld) or rejected, with a documented reason.</summary>
    public void Resolve(bool upheld, string resolution)
    {
        if (Status is AppealStatus.Resolved or AppealStatus.Rejected)
            throw new ArgumentException("This appeal has already been closed.");
        if (string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("A resolution is required.", nameof(resolution));
        Status = upheld ? AppealStatus.Resolved : AppealStatus.Rejected;
        Resolution = resolution;
        ResolvedAt = DateTime.UtcNow;
        base.Update();
    }
}
