namespace CyberErp.Hrms.Dom.Entities.Core;

/*
 * A menu screen. `ModuleId` is a FOREIGN KEY to Core.Module — the menu GROUP it hangs off.
 *
 * ⚠️ HISTORY, because the column has meant two different things (2026-08-15).
 *
 * On 2026-08-13 this was made SELF-REFERENCING: a group was an Operation with ModuleId NULL, and
 * screens pointed at that parent row. That mirrored what cybererp_srms looked like at the time.
 * SRMS has since been corrected — its ModuleId genuinely constrains to Core.Module — so CERP
 * follows it back.
 *
 * The repoint needed NO data change, because the 2026-08-13 migration deliberately copied the 24
 * modules across USING THEIR OWN Ids: every parent operation's Id already equalled its module's, so
 * all 144 children were already pointing at a valid Core.Module row.
 *
 * The 24 parent rows still exist here with ModuleId NULL. They are what Core.Module now expresses,
 * and removing them is a separate step — the sidebar still reads groups from the tenant copies.
 *
 * `TenantId` is KEPT, unlike SRMS — see the note on User.
 */
public class Operation : BaseEntity
{
    /// <summary>
    /// The Core.Module this screen belongs to. Nullable only until the 24 legacy group rows are
    /// removed; SRMS has it NOT NULL and no group rows at all.
    /// </summary>
    public Guid? ModuleId { get; private set; }

    /// <summary>The subsystem this branch of the menu belongs to. Parents and children agree.</summary>
    public Guid SubSystemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>The route. EMPTY on a parent — a group is not navigable and grants nothing.</summary>
    public string Link { get; private set; } = string.Empty;
    public string Filter { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    /// <summary>Menu position among siblings. Named SortOrder before the SRMS alignment.</summary>
    public int DisplayOrder { get; private set; }
    /// <summary>False hides the screen everywhere — it is a template-level kill switch.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>The menu group this screen belongs to (Core.Module). Null on a legacy group row.</summary>
    public Module? Module { get; private set; }

    /// <summary>True when this row is a menu group rather than a screen.</summary>
    public bool IsParent => ModuleId is null;

    private Operation() : base() { }

    /// <summary>Creates a PARENT — a menu group, carrying no route.</summary>
    public static Operation CreateParent(
        Guid subSystemId,
        string name,
        string? icon,
        int displayOrder = 0,
        bool isActive = true)
    {
        if (subSystemId == Guid.Empty)
            throw new ArgumentException("Subsystem ID cannot be empty.", nameof(subSystemId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Operation name cannot be empty.", nameof(name));

        return new Operation
        {
            ModuleId = null,
            SubSystemId = subSystemId,
            Name = name,
            Link = string.Empty,
            Filter = string.Empty,
            Icon = icon ?? string.Empty,
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
    }

    /// <summary>Creates a CHILD — a screen hanging off <paramref name="moduleId"/>'s parent row.</summary>
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
            throw new ArgumentException("Parent ID cannot be empty.", nameof(moduleId));

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
            throw new ArgumentException("Parent ID cannot be empty.", nameof(moduleId));

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

    /// <summary>Renames or re-orders a PARENT. A group has no route, so there is nothing else to set.</summary>
    public void UpdateParent(string name, string? icon, int? displayOrder = null, Guid? subSystemId = null)
    {
        if (!IsParent)
            throw new InvalidOperationException("This operation is a child; use Update instead.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Operation name cannot be empty.", nameof(name));

        Name = name;
        Icon = icon ?? string.Empty;
        if (displayOrder.HasValue) DisplayOrder = displayOrder.Value;
        if (subSystemId.HasValue) SubSystemId = subSystemId.Value;
        base.Update();
    }

    /// <summary>Keeps the denormalised subsystem in step when the row moves to another parent.</summary>
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
