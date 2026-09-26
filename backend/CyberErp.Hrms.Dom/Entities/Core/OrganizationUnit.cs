using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A node in the organizational hierarchy (HC001-HC003). A single self-referencing,
/// typed tree models every level (business unit, directorate, division, department, team),
/// so the whole structure renders as one org chart via <see cref="ParentId"/>.
/// </summary>
public enum OrganizationUnitType
{
    BusinessUnit = 0,
    Directorate = 1,
    Division = 2,
    Department = 3,
    Team = 4,
    Branch = 5
}

public class OrganizationUnit : BaseEntity, IAggregateRoot, IBranchScoped, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public OrganizationUnitType UnitType { get; private set; }
    public int? AllocatedHeadcount { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Position among SIBLINGS, ascending. Sparse by design — the resequencer numbers 10, 20, 30…
    /// so a later insertion between two units does not have to renumber the whole level.
    /// </summary>
    /// <remarks>
    /// ⚠️ Deliberately NOT a parameter of <see cref="Update"/>. Ordering is set by dragging, and the
    /// edit form knows nothing about it — if it travelled through Update, every ordinary save of a
    /// unit's name would silently reset its position to whatever the form last happened to hold.
    /// It moves only through <see cref="MoveTo"/> and <see cref="SetSortOrder"/>.
    /// </remarks>
    public int SortOrder { get; private set; }

    // Branch this unit belongs to (null = global / Head-Office level). Drives branch isolation.
    public Guid? BranchId { get; private set; }

    private Branch? _branch;
    public Branch? Branch => _branch;

    // Self-referencing hierarchy
    public Guid? ParentId { get; private set; }

    private OrganizationUnit? _parent;
    public OrganizationUnit? Parent => _parent;

    private readonly List<OrganizationUnit> _children = new();
    public IReadOnlyCollection<OrganizationUnit> Children => _children.AsReadOnly();

    // Optional link to a work location (HC010)
    public Guid? WorkLocationId { get; private set; }

    private WorkLocation? _workLocation;
    public WorkLocation? WorkLocation => _workLocation;

    // NOTE: a unit carries no direct manager link. The manager of a unit is RELATIONAL:
    // an employee with IsManagerial = true whose Position belongs to this unit. Workflow
    // "Immediate Manager" routing resolves that join, climbing ParentId when no managerial
    // employee sits in the unit (or the requester is the only one).

    private OrganizationUnit() : base() { }

    public static OrganizationUnit Create(
        string code,
        string name,
        OrganizationUnitType unitType,
        Guid? branchId = null,
        Guid? parentId = null,
        Guid? workLocationId = null,
        int? allocatedHeadcount = null,
        string? description = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Organization unit code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization unit name cannot be empty.", nameof(name));
        if (allocatedHeadcount.HasValue && allocatedHeadcount < 0)
            throw new ArgumentException("Allocated headcount cannot be negative.", nameof(allocatedHeadcount));

        return new OrganizationUnit
        {
            Code = code,
            Name = name,
            UnitType = unitType,
            BranchId = branchId,
            ParentId = parentId,
            WorkLocationId = workLocationId,
            AllocatedHeadcount = allocatedHeadcount,
            Description = description,
            IsActive = isActive
        };
    }

    /// <summary>
    /// Reparent this unit and/or place it among its new siblings — the drag-and-drop operation.
    /// </summary>
    /// <remarks>
    /// Only the SELF-parent case is caught here. "The new parent must not be one of my own
    /// descendants" needs every other node to answer, which an entity cannot see; the handler
    /// checks it with <c>HierarchyGuard.WouldCreateCycle</c> before calling this.
    /// </remarks>
    public void MoveTo(Guid? parentId, int sortOrder)
    {
        if (parentId.HasValue && parentId.Value == Id)
            throw new ArgumentException("An organization unit cannot be its own parent.", nameof(parentId));

        ParentId = parentId;
        SortOrder = sortOrder;
        base.Update();
    }

    /// <summary>Renumber this unit within its level, without moving it. Used by the resequencer.</summary>
    public void SetSortOrder(int sortOrder)
    {
        if (SortOrder == sortOrder) return;
        SortOrder = sortOrder;
        base.Update();
    }

    public void Update(
        string code,
        string name,
        OrganizationUnitType unitType,
        Guid? branchId,
        Guid? parentId,
        Guid? workLocationId,
        int? allocatedHeadcount,
        string? description,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Organization unit code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization unit name cannot be empty.", nameof(name));
        if (parentId.HasValue && parentId.Value == Id)
            throw new ArgumentException("An organization unit cannot be its own parent.", nameof(parentId));
        if (allocatedHeadcount.HasValue && allocatedHeadcount < 0)
            throw new ArgumentException("Allocated headcount cannot be negative.", nameof(allocatedHeadcount));

        Code = code;
        Name = name;
        UnitType = unitType;
        BranchId = branchId;
        ParentId = parentId;
        WorkLocationId = workLocationId;
        AllocatedHeadcount = allocatedHeadcount;
        Description = description;
        IsActive = isActive;
        base.Update();
    }
}
