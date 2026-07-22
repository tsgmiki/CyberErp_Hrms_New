using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Planning horizon for a successor pool (HC157 — multi-level readiness).</summary>
public enum SuccessionHorizon
{
    ShortTerm = 0,
    MediumTerm = 1,
    LongTerm = 2
}

public enum SuccessionPlanStatus
{
    Active = 0,
    OnHold = 1,
    Closed = 2,
    /// <summary>Awaiting workflow approval (HC160 — set on submission when an approval chain is configured).</summary>
    PendingApproval = 3,
    /// <summary>Rejected by the approval workflow; editable and resubmittable.</summary>
    Rejected = 4
}

/// <summary>
/// A succession-planning instance for a critical role (HC152, HC157): manages and tracks the pool of
/// potential successors over a short/medium/long-term horizon. Successors and their development live in
/// <see cref="SuccessionCandidate"/> rows (managed on their own; cascades from the plan).
/// </summary>
public class SuccessionPlan : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid CriticalPositionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SuccessionHorizon Horizon { get; private set; } = SuccessionHorizon.MediumTerm;
    public SuccessionPlanStatus Status { get; private set; } = SuccessionPlanStatus.Active;
    public string? Notes { get; private set; }

    private CriticalPosition? _criticalPosition;
    public CriticalPosition? CriticalPosition => _criticalPosition;

    private SuccessionPlan() : base() { }

    public static SuccessionPlan Create(Guid criticalPositionId, string name, SuccessionHorizon horizon, SuccessionPlanStatus status, string? notes)
    {
        if (criticalPositionId == Guid.Empty) throw new ArgumentException("A critical position is required.", nameof(criticalPositionId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Succession plan name is required.", nameof(name));
        return new SuccessionPlan
        {
            CriticalPositionId = criticalPositionId,
            Name = name,
            Horizon = horizon,
            Status = status,
            Notes = notes
        };
    }

    public void Update(Guid criticalPositionId, string name, SuccessionHorizon horizon, SuccessionPlanStatus status, string? notes)
    {
        if (criticalPositionId == Guid.Empty) throw new ArgumentException("A critical position is required.", nameof(criticalPositionId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Succession plan name is required.", nameof(name));
        CriticalPositionId = criticalPositionId;
        Name = name;
        Horizon = horizon;
        Status = status;
        Notes = notes;
        base.Update();
    }

    /// <summary>Parks the plan awaiting approval (an active workflow definition governs this tenant).</summary>
    public void MarkPendingApproval()
    {
        Status = SuccessionPlanStatus.PendingApproval;
        base.Update();
    }

    /// <summary>Workflow callback — an approved plan goes live.</summary>
    public void ApproveViaWorkflow()
    {
        if (Status != SuccessionPlanStatus.PendingApproval) return; // idempotent
        Status = SuccessionPlanStatus.Active;
        base.Update();
    }

    /// <summary>Workflow callback — a rejected plan stays editable for resubmission.</summary>
    public void RejectViaWorkflow()
    {
        if (Status != SuccessionPlanStatus.PendingApproval) return;
        Status = SuccessionPlanStatus.Rejected;
        base.Update();
    }
}
