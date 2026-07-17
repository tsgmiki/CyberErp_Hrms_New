using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of an objective: drafted → active (goals cascade off it) → closed.</summary>
public enum ObjectiveStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2
}

/// <summary>
/// An organizational objective (HC118) that goal-setting cascades from: organization → directorate →
/// team. The cascade is modelled by an optional <see cref="ParentObjectiveId"/> self-reference plus the
/// owning <see cref="OrganizationUnitId"/> (null at the top organizational level, a directorate/team unit
/// further down). Individual <see cref="EmployeeGoal"/>s link up to an objective (HC120); weights (HC122)
/// express an objective's relative importance among its siblings.
/// </summary>
public class OrganizationalObjective : BaseEntity, IAggregateRoot, IAuditable
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid ReviewCycleId { get; private set; }
    /// <summary>Owning unit — null for a top-level organizational objective, a directorate/team below.</summary>
    public Guid? OrganizationUnitId { get; private set; }
    /// <summary>Cascade parent — null at the organizational root, the parent objective below.</summary>
    public Guid? ParentObjectiveId { get; private set; }
    public decimal Weight { get; private set; }
    public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.Draft;

    private OrganizationalObjective() : base() { }

    public static OrganizationalObjective Create(string title, Guid reviewCycleId, string? description = null,
        Guid? organizationUnitId = null, Guid? parentObjectiveId = null, decimal weight = 0,
        ObjectiveStatus status = ObjectiveStatus.Draft)
    {
        Guard(title, reviewCycleId, weight);
        return new OrganizationalObjective
        {
            Title = title,
            ReviewCycleId = reviewCycleId,
            Description = description,
            OrganizationUnitId = organizationUnitId,
            ParentObjectiveId = parentObjectiveId,
            Weight = weight,
            Status = status
        };
    }

    public void Update(string title, Guid reviewCycleId, string? description,
        Guid? organizationUnitId, Guid? parentObjectiveId, decimal weight, ObjectiveStatus status)
    {
        Guard(title, reviewCycleId, weight);
        if (parentObjectiveId == Id)
            throw new ArgumentException("An objective cannot be its own parent.", nameof(parentObjectiveId));
        Title = title;
        ReviewCycleId = reviewCycleId;
        Description = description;
        OrganizationUnitId = organizationUnitId;
        ParentObjectiveId = parentObjectiveId;
        Weight = weight;
        Status = status;
        base.Update();
    }

    private static void Guard(string title, Guid reviewCycleId, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Objective title cannot be empty.", nameof(title));
        if (reviewCycleId == Guid.Empty)
            throw new ArgumentException("A review cycle is required.", nameof(reviewCycleId));
        if (weight is < 0 or > 100)
            throw new ArgumentException("Weight must be between 0 and 100.", nameof(weight));
    }
}
