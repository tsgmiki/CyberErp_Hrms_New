namespace CyberErp.Hrms.Dom.Entities.Core;

/*
 * Aligned with the SRMS platform schema (2026-08-13): `SortOrder` is now `DisplayOrder`, and
 * `SubSystemId` / `IsActive` were added.
 *
 * ⚠️ TWO DELIBERATE DEPARTURES FROM SRMS, both about the navigation shape:
 *
 * 1. `ModuleId` still points at Core.Module. In SRMS there IS no Module table — its operations nest
 *    into each other, and the column it calls `ModuleId` actually carries a FOREIGN KEY back to
 *    Operation.Id (a renamed ParentOperationId whose constraint name was never updated; the one
 *    called FK_Operation_Module_ModuleId in fact sits on SubSystemId). CERP groups 150 operations
 *    under 24 modules and the whole sidebar depends on it, so copying that topology would destroy
 *    the menu with nothing to migrate to.
 *
 * 2. `TenantId` is KEPT. See the note on User.
 *
 * `SubSystemId` is denormalised from the module's subsystem — the same value TenantOperation already
 * stores — so an operation can be resolved to its subsystem without a join.
 */
public class Operation : BaseEntity
{
    public Guid ModuleId { get; private set; }
    /// <summary>Denormalised from <see cref="Module"/>'s subsystem; kept in step on every write.</summary>
    public Guid SubSystemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Link { get; private set; } = string.Empty;
    public string Filter { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    /// <summary>Menu position. Named SortOrder before the SRMS alignment.</summary>
    public int DisplayOrder { get; private set; }
    /// <summary>False hides the screen everywhere — it is a template-level kill switch.</summary>
    public bool IsActive { get; private set; } = true;
    public Module Module { get; private set; } = null!;

    private Operation() : base() { }

    public static Operation Create(
        Guid moduleId,
        string name,
        string link,
        string filter,
        string icon,
        int displayOrder = 0,
        Guid? subSystemId = null,
        bool isActive = true)
    {
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module ID cannot be empty.", nameof(moduleId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Operation name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Link cannot be empty.", nameof(link));

        return new Operation
        {
            ModuleId = moduleId,
            SubSystemId = subSystemId ?? Guid.Empty,
            Name = name,
            Link = link,
            Filter = filter,
            Icon = icon,
            DisplayOrder = displayOrder,
            IsActive = isActive
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(
        Guid moduleId,
        string name,
        string link,
        string filter,
        string icon,
        int? displayOrder = null,
        Guid? subSystemId = null)
    {
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module ID cannot be empty.", nameof(moduleId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Operation name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Link cannot be empty.", nameof(link));

        ModuleId = moduleId;
        Name = name;
        Link = link;
        Filter = filter;
        Icon = icon;
        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;
        if (subSystemId.HasValue)
            SubSystemId = subSystemId.Value;
        base.Update();
    }

    /// <summary>Keeps the denormalised subsystem in step when the module moves.</summary>
    public void SetSubSystem(Guid subSystemId)
    {
        if (SubSystemId == subSystemId) return;
        SubSystemId = subSystemId;
        base.Update();
    }

    public void SetActive(bool isActive)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        base.Update();
    }
}
