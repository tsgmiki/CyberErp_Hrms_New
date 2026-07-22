using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>How business-critical a role is (drives succession prioritisation).</summary>
public enum CriticalityLevel
{
    Low = 0,
    Medium = 1,
    High = 2
}

/// <summary>Approval lifecycle of a critical-position flag (HC151 — workflow-owned states).</summary>
public enum CriticalPositionStatus
{
    Active = 0,
    /// <summary>Awaiting workflow approval (set on submission when an approval chain is configured).</summary>
    PendingApproval = 1,
    /// <summary>Rejected by the approval workflow; editable and resubmittable.</summary>
    Rejected = 2
}

/// <summary>
/// A role (position class) flagged as business-critical for succession planning (HC151). HR marks a
/// role critical with a risk level and the criteria/justification used; each critical position anchors
/// one or more <see cref="SuccessionPlan"/>s.
/// </summary>
public class CriticalPosition : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid PositionId { get; private set; }
    public CriticalityLevel RiskLevel { get; private set; } = CriticalityLevel.Medium;
    public string? Reason { get; private set; }
    public string? Criteria { get; private set; }
    public bool IsActive { get; private set; } = true;
    public CriticalPositionStatus Status { get; private set; } = CriticalPositionStatus.Active;

    private Position? _position;
    public Position? Position => _position;

    private CriticalPosition() : base() { }

    public static CriticalPosition Create(Guid positionId, CriticalityLevel riskLevel, string? reason, string? criteria, bool isActive)
    {
        if (positionId == Guid.Empty)
            throw new ArgumentException("A position is required.", nameof(positionId));
        return new CriticalPosition
        {
            PositionId = positionId,
            RiskLevel = riskLevel,
            Reason = reason,
            Criteria = criteria,
            IsActive = isActive
        };
    }

    public void Update(Guid positionId, CriticalityLevel riskLevel, string? reason, string? criteria, bool isActive)
    {
        if (positionId == Guid.Empty)
            throw new ArgumentException("A position is required.", nameof(positionId));
        PositionId = positionId;
        RiskLevel = riskLevel;
        Reason = reason;
        Criteria = criteria;
        IsActive = isActive;
        base.Update();
    }

    /// <summary>Parks the flag awaiting approval — also inactive so active-only feeds exclude it.</summary>
    public void MarkPendingApproval()
    {
        Status = CriticalPositionStatus.PendingApproval;
        IsActive = false;
        base.Update();
    }

    /// <summary>Workflow callback — an approved flag goes live.</summary>
    public void ApproveViaWorkflow()
    {
        if (Status != CriticalPositionStatus.PendingApproval) return; // idempotent
        Status = CriticalPositionStatus.Active;
        IsActive = true;
        base.Update();
    }

    /// <summary>Workflow callback — a rejected flag stays editable (and inactive) for resubmission.</summary>
    public void RejectViaWorkflow()
    {
        if (Status != CriticalPositionStatus.PendingApproval) return;
        Status = CriticalPositionStatus.Rejected;
        IsActive = false;
        base.Update();
    }
}
