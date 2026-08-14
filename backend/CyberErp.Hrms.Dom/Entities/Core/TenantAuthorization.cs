using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/*
 * The tenant-scoped authorization model, ported from the SRMS platform schema (phase 2).
 *
 * WHAT CHANGES CONCEPTUALLY
 * -------------------------
 * Today `Role`, `Operation` and `RolePermission` are single global tables. In this model the global
 * rows become TEMPLATES and each tenant gets its own instances:
 *
 *     Role      -> TenantRole        (SourceTemplateId points back at the template)
 *     Operation -> TenantOperation   (a per-tenant COPY: own Name/Link/Icon/DisplayOrder/IsActive)
 *     RolePermission -> TenantRolePermission (+ CanExport, which the old model lacks)
 *     UserRole  -> TenantUser + TenantUserRole (a user can belong to several tenants)
 *
 * That is what lets one tenant rename a screen, hide a module, or diverge from the template without
 * touching anyone else — impossible while every tenant shares one row.
 *
 * ⚠️ TenantId NAMING. BaseEntity already has a `TenantId` STRING: the Finbuckle discriminator that
 * scopes a row to its tenant. These entities also need a real FOREIGN KEY to Core.Tenant, which is a
 * Guid. The two are different things, so the FK is named `OwningTenantId` here rather than shadowing
 * the discriminator — the same choice made for TenantSubscriptionAddOn.
 */

/// <summary>Membership state of a user within one tenant.</summary>
public static class TenantUserStatuses
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Invited = "Invited";
}

/// <summary>Where a tenant's access to a subsystem came from.</summary>
public static class TenantSubSystemSources
{
    public const string Plan = "Plan";
    public const string AddOn = "AddOn";
    public const string Trial = "Trial";
}

/// <summary>
/// A role AS IT EXISTS FOR ONE TENANT — the per-tenant instance of a global <see cref="Role"/>.
///
/// <para><see cref="SourceTemplateId"/> records which template it was created from, and
/// <see cref="IsCustomized"/> whether the tenant has since diverged. Together they answer "can this
/// role be refreshed from the template, or would that discard local changes?".</para>
/// </summary>
public class TenantRole : BaseEntity, IAggregateRoot
{
    /// <summary>The global <see cref="Role"/> this was instantiated from; null for a bespoke role.</summary>
    public Guid? SourceTemplateId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>True once the tenant has changed it, so a template refresh must not overwrite it.</summary>
    public bool IsCustomized { get; private set; }

    private TenantRole() : base() { }

    public static TenantRole Create(Guid owningTenantId, string code, string name,
        Guid? sourceTemplateId = null, string? description = null)
    {
        if (owningTenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(owningTenantId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));
        return new TenantRole
        {
            TenantId = owningTenantId.ToString(),
            SourceTemplateId = sourceTemplateId,
            Code = string.IsNullOrWhiteSpace(code) ? name.Trim() : code.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
        };
    }

    public void Rename(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));
        Name = name.Trim();
        Description = description?.Trim();
        MarkCustomized();
    }

    /// <summary>Flags local divergence from the template. Called by any tenant-side edit.</summary>
    public void MarkCustomized()
    {
        IsCustomized = true;
        base.Update();
    }

    /// <summary>
    /// Re-applies the template's name/code. NO-OP once the tenant has customised the role — that is
    /// the whole point of <see cref="IsCustomized"/>: a refresh must never silently discard a local
    /// rename. Returns true when something actually changed.
    /// </summary>
    public bool SyncFromTemplate(string name, string? code, string? description)
    {
        if (IsCustomized) return false;

        var newName = (name ?? string.Empty).Trim();
        var newCode = string.IsNullOrWhiteSpace(code) ? newName : code.Trim();
        var newDescription = description?.Trim();
        if (Name == newName && Code == newCode && Description == newDescription) return false;

        Name = newName;
        Code = newCode;
        Description = newDescription;
        base.Update();
        return true;
    }
}

/// <summary>
/// A menu GROUP as it exists for one tenant — the tenant-scoped twin of <see cref="Module"/>, and
/// the parent of <see cref="TenantOperation"/>. Added 2026-08-15 to match cybererp_srms.
///
/// <para>Before this, a group was a <see cref="TenantOperation"/> row with a null ModuleId. Groups
/// now live here, which is why <c>TenantOperation.ModuleId</c> became NOT NULL and points at this
/// table.</para>
///
/// <para>⚠️ <see cref="ModuleId"/> is a CERP EXTRA — SRMS's tenant copies carry no link back to the
/// template at all (0 of its 220 rows share an Id with one). It is kept for exactly the reason
/// <see cref="TenantOperation.OperationId"/> is: the projector needs to know which template a copy
/// came from, and the UI reports template ids as the stable identifier. Deliberate superset,
/// recorded in the extras list.</para>
/// </summary>
public class TenantModule : BaseEntity
{
    public Guid SubSystemId { get; private set; }
    /// <summary>The global <see cref="Module"/> this mirrors. CERP extra — see the note above.</summary>
    public Guid ModuleId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    /// <summary>Optional per-tenant row filter expression; empty when unused.</summary>
    public string Filter { get; private set; } = string.Empty;

    private TenantModule() : base() { }

    public static TenantModule Create(Guid owningTenantId, Guid subSystemId, Guid moduleId,
        string name, string? icon, int displayOrder, bool isActive, string? filter = null)
    {
        if (owningTenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(owningTenantId));
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Source module is required.", nameof(moduleId));
        return new TenantModule
        {
            TenantId = owningTenantId.ToString(),
            SubSystemId = subSystemId,
            ModuleId = moduleId,
            Name = name?.Trim() ?? string.Empty,
            Icon = icon?.Trim() ?? string.Empty,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            Filter = filter?.Trim() ?? string.Empty,
        };
    }

    /// <summary>Re-applies the template's fields. True when something actually changed.</summary>
    public bool SyncFromTemplate(Guid subSystemId, string name, string? icon, int displayOrder,
        string? filter, bool isActive)
    {
        var newName = (name ?? string.Empty).Trim();
        var newIcon = (icon ?? string.Empty).Trim();
        var newFilter = (filter ?? string.Empty).Trim();

        if (SubSystemId == subSystemId && Name == newName && Icon == newIcon
            && DisplayOrder == displayOrder && Filter == newFilter && IsActive == isActive)
            return false;

        SubSystemId = subSystemId;
        Name = newName;
        Icon = newIcon;
        DisplayOrder = displayOrder;
        Filter = newFilter;
        IsActive = isActive;
        base.Update();
        return true;
    }

    /// <summary>Hides or restores the whole group for the tenant.</summary>
    public void SetActive(bool isActive)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        base.Update();
    }
}

/// <summary>
/// A menu operation AS IT EXISTS FOR ONE TENANT — a copy, not a reference.
///
/// <para>It carries its own <see cref="Name"/>, <see cref="Link"/>, <see cref="Icon"/>,
/// <see cref="DisplayOrder"/> and <see cref="IsActive"/> so a tenant can rename or hide a screen
/// without affecting others. <see cref="OperationId"/> keeps the line back to the global template.</para>
/// </summary>
public class TenantOperation : BaseEntity
{
    public Guid SubSystemId { get; private set; }
    /// <summary>The global <see cref="Operation"/> this mirrors. A CERP extra; SRMS has no such link.</summary>
    public Guid OperationId { get; private set; }
    /// <summary>
    /// The <see cref="TenantModule"/> this screen sits under — NOT the global module. Non-null since
    /// 2026-08-15: groups moved to TenantModule, so every remaining row here is a screen.
    /// </summary>
    public Guid ModuleId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    /// <summary>Route this operation grants, e.g. "/employee". The permission check matches on this.</summary>
    public string Link { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    /// <summary>Optional per-tenant row filter expression; empty when unused.</summary>
    public string Filter { get; private set; } = string.Empty;

    private TenantOperation() : base() { }

    public static TenantOperation Create(Guid owningTenantId, Guid subSystemId, Guid operationId,
        Guid moduleId, string name, string link, string? icon, int displayOrder, bool isActive)
    {
        if (owningTenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(owningTenantId));
        if (operationId == Guid.Empty)
            throw new ArgumentException("Source operation is required.", nameof(operationId));
        return new TenantOperation
        {
            TenantId = owningTenantId.ToString(),
            SubSystemId = subSystemId,
            OperationId = operationId,
            ModuleId = moduleId,
            Name = name?.Trim() ?? string.Empty,
            Link = link?.Trim() ?? string.Empty,
            Icon = icon?.Trim() ?? string.Empty,
            DisplayOrder = displayOrder,
            IsActive = isActive,
        };
    }

    /// <summary>
    /// Re-applies the template's presentation fields. Returns true when something actually changed,
    /// so the projector can skip a write.
    ///
    /// <para>Unlike <see cref="TenantRole"/> there is no customised guard yet: nothing in the UI can
    /// rename a tenant's copy of a screen, so there is no local edit to protect. Add one here the
    /// moment such a screen exists, or the first template edit will overwrite it.</para>
    /// </summary>
    public bool SyncFromTemplate(Guid subSystemId, Guid moduleId, string name, string link,
        string? icon, int displayOrder, string? filter)
    {
        var newName = (name ?? string.Empty).Trim();
        var newLink = (link ?? string.Empty).Trim();
        var newIcon = (icon ?? string.Empty).Trim();
        var newFilter = (filter ?? string.Empty).Trim();

        if (SubSystemId == subSystemId && ModuleId == moduleId && Name == newName && Link == newLink
            && Icon == newIcon && DisplayOrder == displayOrder && Filter == newFilter)
            return false;

        SubSystemId = subSystemId;
        ModuleId = moduleId;
        Name = newName;
        Link = newLink;
        Icon = newIcon;
        DisplayOrder = displayOrder;
        Filter = newFilter;
        base.Update();
        return true;
    }

    /// <summary>Hides or restores this screen for the tenant, without deleting the grant rows.</summary>
    public void SetActive(bool isActive)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        base.Update();
    }
}

/// <summary>
/// What one <see cref="TenantRole"/> may do with one <see cref="TenantOperation"/>.
///
/// <para>Adds <see cref="CanExport"/>, which the global <see cref="RolePermission"/> has no column
/// for — exporting a list is a distinct privilege from viewing it, and plenty of deployments want to
/// grant one without the other.</para>
/// </summary>
public class TenantRolePermission : BaseEntity
{
    public Guid TenantRoleId { get; private set; }
    public Guid TenantOperationId { get; private set; }
    public bool CanView { get; private set; }
    public bool CanAdd { get; private set; }
    public bool CanEdit { get; private set; }
    public bool CanDelete { get; private set; }
    public bool CanApprove { get; private set; }
    /// <summary>New in this model: the right to export a list, separate from viewing it.</summary>
    public bool CanExport { get; private set; }

    private TenantRolePermission() : base() { }

    public static TenantRolePermission Create(Guid tenantRoleId, Guid tenantOperationId,
        bool canView, bool canAdd, bool canEdit, bool canDelete, bool canApprove, bool canExport = false)
    {
        if (tenantRoleId == Guid.Empty)
            throw new ArgumentException("Tenant role is required.", nameof(tenantRoleId));
        if (tenantOperationId == Guid.Empty)
            throw new ArgumentException("Tenant operation is required.", nameof(tenantOperationId));
        return new TenantRolePermission
        {
            TenantRoleId = tenantRoleId,
            TenantOperationId = tenantOperationId,
            CanView = canView,
            CanAdd = canAdd,
            CanEdit = canEdit,
            CanDelete = canDelete,
            CanApprove = canApprove,
            CanExport = canExport,
        };
    }

    public void Set(bool canView, bool canAdd, bool canEdit, bool canDelete, bool canApprove, bool canExport)
    {
        CanView = canView; CanAdd = canAdd; CanEdit = canEdit;
        CanDelete = canDelete; CanApprove = canApprove; CanExport = canExport;
        base.Update();
    }
}

/// <summary>
/// One user's MEMBERSHIP of one tenant.
///
/// <para>This is the row that makes a user multi-tenant: the same person can belong to several
/// tenants with a different status and different roles in each, and <see cref="IsDefaultTenant"/>
/// says where they land at sign-in. The old model had no such concept — a `User` carried a single
/// tenant discriminator.</para>
/// </summary>
public class TenantUser : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Status { get; private set; } = TenantUserStatuses.Active;
    public bool IsDefaultTenant { get; private set; }

    private TenantUser() : base() { }

    public static TenantUser Create(Guid owningTenantId, Guid userId,
        string status = TenantUserStatuses.Active, bool isDefaultTenant = true)
    {
        if (owningTenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(owningTenantId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User is required.", nameof(userId));
        return new TenantUser
        {
            TenantId = owningTenantId.ToString(),
            UserId = userId,
            Status = string.IsNullOrWhiteSpace(status) ? TenantUserStatuses.Active : status.Trim(),
            IsDefaultTenant = isDefaultTenant,
        };
    }

    public void SetStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required.", nameof(status));
        Status = status.Trim();
        base.Update();
    }
}

/// <summary>A role held by a <see cref="TenantUser"/>, with who granted it and when.</summary>
public class TenantUserRole : BaseEntity
{
    public Guid TenantUserId { get; private set; }
    public Guid TenantRoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    /// <summary>
    /// The USER who granted the role. A Guid since the 2026-08-14 SRMS alignment — it was free text
    /// before and only ever held provenance markers ("seed-tenant-authorization", "projection"),
    /// never a real identity, so those were dropped to null rather than invented as ids.
    /// </summary>
    public Guid? AssignedBy { get; private set; }

    private TenantUserRole() : base() { }

    public static TenantUserRole Create(Guid tenantUserId, Guid tenantRoleId, Guid? assignedBy = null)
    {
        if (tenantUserId == Guid.Empty)
            throw new ArgumentException("Tenant user is required.", nameof(tenantUserId));
        if (tenantRoleId == Guid.Empty)
            throw new ArgumentException("Tenant role is required.", nameof(tenantRoleId));
        return new TenantUserRole
        {
            TenantUserId = tenantUserId,
            TenantRoleId = tenantRoleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy,
        };
    }
}

/// <summary>
/// A subsystem one tenant is licensed for, with the term it runs on.
///
/// <para>Replaces "every tenant sees every subsystem" with an explicit grant carrying a status and
/// dates, so a trial can expire and a lapsed subscription can close a subsystem without deleting
/// anything.</para>
/// </summary>
public class TenantSubSystem : BaseEntity
{
    public Guid SubSystemId { get; private set; }
    /// <summary>Plan | AddOn | Trial — see <see cref="TenantSubSystemSources"/>.</summary>
    public string SourceType { get; private set; } = TenantSubSystemSources.Plan;
    public string Status { get; private set; } = SubscriptionStatuses.Active;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }

    private TenantSubSystem() : base() { }

    public static TenantSubSystem Create(Guid owningTenantId, Guid subSystemId, string sourceType,
        string status, DateTime startDate, DateTime? endDate = null, DateTime? trialEndDate = null)
    {
        if (owningTenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(owningTenantId));
        if (subSystemId == Guid.Empty)
            throw new ArgumentException("Subsystem is required.", nameof(subSystemId));
        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));
        return new TenantSubSystem
        {
            TenantId = owningTenantId.ToString(),
            SubSystemId = subSystemId,
            SourceType = string.IsNullOrWhiteSpace(sourceType) ? TenantSubSystemSources.Plan : sourceType.Trim(),
            Status = string.IsNullOrWhiteSpace(status) ? SubscriptionStatuses.Active : status.Trim(),
            StartDate = startDate.Date,
            EndDate = endDate?.Date,
            TrialEndDate = trialEndDate?.Date,
        };
    }
}
