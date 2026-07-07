using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Job category lookup (HC004). Referenced by <see cref="Position"/>.
/// </summary>
public class JobCategory : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private JobCategory() : base() { }

    public static JobCategory Create(
        string name,
        string code,
        string? description = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Job category name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Job category code cannot be empty.", nameof(code));

        return new JobCategory
        {
            Name = name,
            Code = code,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        string code,
        string? description,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Job category name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Job category code cannot be empty.", nameof(code));

        Name = name;
        Code = code;
        Description = description;
        IsActive = isActive;
        base.Update();
    }
}
