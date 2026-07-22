using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Configurable competency category (HC125) — e.g. Technical, Leadership, Teamwork, Behavioral,
/// Communication. Groups the competency library; the categories themselves are user-maintained.
/// </summary>
public class CompetencyCategory : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private CompetencyCategory() : base() { }

    public static CompetencyCategory Create(string name, string? description = null, int sortOrder = 0, bool isActive = true)
    {
        Guard(name);
        return new CompetencyCategory { Name = name, Description = description, SortOrder = sortOrder, IsActive = isActive };
    }

    public void Update(string name, string? description, int sortOrder, bool isActive)
    {
        Guard(name);
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Competency category name cannot be empty.", nameof(name));
    }
}
