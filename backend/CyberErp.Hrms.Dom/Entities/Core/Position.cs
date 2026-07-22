using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Places a <see cref="PositionClass"/> (job definition) at a specific <see cref="OrganizationUnit"/>
/// with a unique code — a concrete headcount slot. Deliberately thin: all job attributes live on the
/// class. Branch-isolated via <see cref="BranchId"/> (inherited from the organization unit).
/// </summary>
public class Position : BaseEntity, IAggregateRoot, IBranchScoped, IAuditable
{
    public string Code { get; private set; } = string.Empty;

    public Guid PositionClassId { get; private set; }
    private PositionClass? _positionClass;
    public PositionClass? PositionClass => _positionClass;

    public Guid OrganizationUnitId { get; private set; }
    private OrganizationUnit? _organizationUnit;
    public OrganizationUnit? OrganizationUnit => _organizationUnit;

    // Branch this position belongs to (inherited from its organization unit). Drives isolation.
    public Guid? BranchId { get; private set; }
    private Branch? _branch;
    public Branch? Branch => _branch;

    /// <summary>True when no employee occupies this position (open seat). Kept in sync by the
    /// employee handlers on assignment / unassignment. New positions start vacant.</summary>
    public bool IsVacant { get; private set; } = true;

    private Position() : base() { }

    public static Position Create(
        string code,
        Guid positionClassId,
        Guid organizationUnitId,
        Guid? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Position code cannot be empty.", nameof(code));
        if (positionClassId == Guid.Empty)
            throw new ArgumentException("A position must have a position class.", nameof(positionClassId));
        if (organizationUnitId == Guid.Empty)
            throw new ArgumentException("A position must belong to an organization unit.", nameof(organizationUnitId));

        return new Position
        {
            Code = code,
            PositionClassId = positionClassId,
            OrganizationUnitId = organizationUnitId,
            BranchId = branchId
        };
    }

    public void Update(
        string code,
        Guid positionClassId,
        Guid organizationUnitId,
        Guid? branchId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Position code cannot be empty.", nameof(code));
        if (positionClassId == Guid.Empty)
            throw new ArgumentException("A position must have a position class.", nameof(positionClassId));
        if (organizationUnitId == Guid.Empty)
            throw new ArgumentException("A position must belong to an organization unit.", nameof(organizationUnitId));

        Code = code;
        PositionClassId = positionClassId;
        OrganizationUnitId = organizationUnitId;
        BranchId = branchId;
        base.Update();
    }

    /// <summary>Marks the position vacant (open) or occupied. Called when an employee is
    /// assigned to or removed from this position.</summary>
    public void SetVacant(bool isVacant)
    {
        if (IsVacant == isVacant) return;
        IsVacant = isVacant;
        base.Update();
    }
}
