using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Type of structural mutation captured in the audit trail.</summary>
public enum AuditAction
{
    Created = 0,
    Modified = 1,
    Reassigned = 2,   // parent or branch changed (a unit moved within the structure)
    Deleted = 3,
    Rejected = 4      // reserved for the approval workflow (deferred)
}

/// <summary>
/// Immutable audit-trail entry for a structural mutation (HC007). Written automatically by the
/// SaveChanges interceptor for every change to an <see cref="IAuditable"/> entity. Branch-scoped
/// so branch admins only see their own branch's trail while Head Office sees the whole log.
/// </summary>
public class AuditLog : BaseEntity, IAggregateRoot, IBranchScoped
{
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? EntityName { get; private set; }
    public AuditAction Action { get; private set; }
    /// <summary>JSON of changed fields (old → new) for modifications, or a snapshot for create/delete.</summary>
    public string? Changes { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    public string? PerformedBy { get; private set; }
    public Guid? BranchId { get; private set; }

    private AuditLog() : base() { }

    public static AuditLog Record(
        string entityType,
        Guid entityId,
        AuditAction action,
        string tenantId,
        string? entityName = null,
        string? changes = null,
        Guid? branchId = null,
        Guid? performedByUserId = null,
        string? performedBy = null)
    {
        var log = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            Action = action,
            Changes = changes,
            BranchId = branchId,
            PerformedByUserId = performedByUserId,
            PerformedBy = performedBy
        };
        log.TenantId = tenantId;
        return log;
    }
}
