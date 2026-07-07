using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A distinct branch / business unit (multi-branch organizational structure). Self-referencing
/// to support a multi-tiered branch hierarchy (e.g. Region → Branch → Sub-branch). Every
/// branch-scoped record (organization units, positions) references a branch via BranchId.
/// </summary>
public class Branch : BaseEntity, IAggregateRoot, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    /// <summary>Designates the Head Office branch (retains global oversight).</summary>
    public bool IsHeadOffice { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Self-referencing branch hierarchy
    public Guid? ParentId { get; private set; }

    private Branch? _parent;
    public Branch? Parent => _parent;

    private readonly List<Branch> _children = new();
    public IReadOnlyCollection<Branch> Children => _children.AsReadOnly();

    private Branch() : base() { }

    public static Branch Create(
        string code,
        string name,
        Guid? parentId = null,
        string? description = null,
        string? address = null,
        bool isHeadOffice = false,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Branch code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name cannot be empty.", nameof(name));

        return new Branch
        {
            Code = code,
            Name = name,
            ParentId = parentId,
            Description = description,
            Address = address,
            IsHeadOffice = isHeadOffice,
            IsActive = isActive
        };
    }

    public void Update(
        string code,
        string name,
        Guid? parentId,
        string? description,
        string? address,
        bool isHeadOffice,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Branch code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name cannot be empty.", nameof(name));
        if (parentId.HasValue && parentId.Value == Id)
            throw new ArgumentException("A branch cannot be its own parent.", nameof(parentId));

        Code = code;
        Name = name;
        ParentId = parentId;
        Description = description;
        Address = address;
        IsHeadOffice = isHeadOffice;
        IsActive = isActive;
        base.Update();
    }
}
