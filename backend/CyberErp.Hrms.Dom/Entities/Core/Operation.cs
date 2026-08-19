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
 * The 24 group rows were REMOVED on 2026-08-15 once Core.TenantModule existed to hold the tenant
 * copies. `SubSystemId` went too: SRMS normalised it onto the module, so a screen's subsystem is
 * its module's.
 *
 * `TenantId` is KEPT, unlike SRMS — see the note on User.
 */
public class Operation : BaseEntity
{
    /// <summary>
    /// The Core.Module this screen belongs to.
    ///
    /// <para>Nullable to match cybererp_srms exactly, which never tightened it (0 nulls there, 0
    /// here). Nothing in this codebase can create a screen without a module — <see cref="Create"/>
    /// rejects an empty Guid — so the application enforces what the column permits.</para>
    /// </summary>
    public Guid? ModuleId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    /// <summary>The route the screen grants, e.g. "/employee".</summary>
    public string Link { get; private set; } = string.Empty;
    public string Filter { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    /// <summary>Menu position among siblings. Named SortOrder before the SRMS alignment.</summary>
    public int DisplayOrder { get; private set; }
    /// <summary>False hides the screen everywhere — it is a template-level kill switch.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>The menu group this screen belongs to (Core.Module).</summary>
    public Module? Module { get; private set; }

    private Operation() : base() { }

    /// <summary>Creates a screen under the module <paramref name="moduleId"/>.</summary>
    public static Operation Create(
        Guid moduleId,
        string name,
        string link,
        string filter,
        string icon,
        int displayOrder = 0,
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
        int? displayOrder = null)
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
        base.Update();
    }

    public void SetActive(bool isActive)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        base.Update();
    }
}
