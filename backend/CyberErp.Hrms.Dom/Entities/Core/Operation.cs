namespace CyberErp.Hrms.Dom.Entities.Core;

/*
 * The menu tree, in ONE self-referencing table — the SRMS topology, adopted 2026-08-13.
 *
 * `ModuleId` is the PARENT LINK, not a foreign key to Core.Module:
 *
 *     ModuleId IS NULL      -> a PARENT: a menu group, the row a module used to be
 *     ModuleId IS NOT NULL  -> a CHILD:  a screen, hanging off the parent with that Id
 *
 * The column keeps its name because that is what SRMS calls it (there it is a renamed
 * ParentOperationId whose constraint name was never updated). A parent carries no Link, so it grants
 * nothing: the permission gate skips rows with an empty Link, which is what makes a group a group.
 *
 * ⚠️ INVARIANT — a parent's Id EQUALS the Core.Module row it came from. The migration copied the 24
 * modules across using their own Ids, which is why the 150 existing children needed no repointing at
 * all, and SeedDefaultMenu maintains it. Core.Module still exists and must: SubscriptionPlanModule
 * and TenantSubscriptionAddOn have foreign keys into it. It is no longer what navigation reads.
 *
 * `TenantId` is KEPT, unlike SRMS — see the note on User.
 */
public class Operation : BaseEntity
{
    /// <summary>
    /// The PARENT operation's Id, or null when this row IS a parent. Not a link to Core.Module,
    /// despite the name — see the note above.
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

    public Operation? Parent { get; private set; }
    private readonly List<Operation> _children = new();
    public IReadOnlyCollection<Operation> Children => _children.AsReadOnly();

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
