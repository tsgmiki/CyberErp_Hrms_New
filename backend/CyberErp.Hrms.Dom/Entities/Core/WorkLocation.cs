using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Hierarchical work location (HC009-HC010): Country -> Region -> City -> Office/Facility.
/// Self-referencing via <see cref="ParentId"/>.
/// </summary>
public enum WorkLocationType
{
    Country = 0,
    Region = 1,
    City = 2,
    Office = 3
}

public class WorkLocation : BaseEntity, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public WorkLocationType LocationType { get; private set; }
    public string? Address { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Self-referencing hierarchy
    public Guid? ParentId { get; private set; }

    private WorkLocation? _parent;
    public WorkLocation? Parent => _parent;

    private readonly List<WorkLocation> _children = new();
    public IReadOnlyCollection<WorkLocation> Children => _children.AsReadOnly();

    private WorkLocation() : base() { }

    public static WorkLocation Create(
        string code,
        string name,
        WorkLocationType locationType,
        Guid? parentId = null,
        string? address = null,
        string? description = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Work location code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Work location name cannot be empty.", nameof(name));

        return new WorkLocation
        {
            Code = code,
            Name = name,
            LocationType = locationType,
            ParentId = parentId,
            Address = address,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(
        string code,
        string name,
        WorkLocationType locationType,
        Guid? parentId,
        string? address,
        string? description,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Work location code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Work location name cannot be empty.", nameof(name));
        if (parentId.HasValue && parentId.Value == Id)
            throw new ArgumentException("A work location cannot be its own parent.", nameof(parentId));

        Code = code;
        Name = name;
        LocationType = locationType;
        ParentId = parentId;
        Address = address;
        Description = description;
        IsActive = isActive;
        base.Update();
    }
}
