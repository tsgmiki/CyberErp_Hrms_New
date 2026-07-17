using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Append-only audit trail / version history for performance data (HC132). One immutable row is written
/// at every significant transition — a goal saved, an appraisal stage change or scoring, a peer
/// submission, a calibration adjustment — capturing what happened plus an optional JSON snapshot of the
/// state at that moment. Never updated or deleted; who + when come from the audited
/// <see cref="BaseEntity.CreatedBy"/> / <see cref="BaseEntity.CreatedAt"/>.
/// </summary>
public class PerformanceHistory : BaseEntity
{
    /// <summary>The kind of record this entry is about: "Appraisal", "Goal", "Calibration", "PeerReview".</summary>
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    /// <summary>What happened: "Generated", "SelfSubmitted", "Scored", "Completed", "Calibrated", …</summary>
    public string Action { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    /// <summary>Optional JSON snapshot of the entity state at this point.</summary>
    public string? SnapshotJson { get; private set; }

    private PerformanceHistory() : base() { }

    public static PerformanceHistory Record(string entityType, Guid entityId, string action, string summary, string? snapshotJson = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required.", nameof(action));
        return new PerformanceHistory
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Summary = summary ?? string.Empty,
            SnapshotJson = snapshotJson
        };
    }
}
