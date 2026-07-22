using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A competency in the assessment library (HC123–HC125): a measurable skill/behavior belonging to a
/// configurable <see cref="CompetencyCategory"/> and associated to positions via
/// <see cref="PositionCompetency"/>. Employees are assessed on the competencies of their position.
/// </summary>
public class Competency : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public Guid CompetencyCategoryId { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Competency() : base() { }

    public static Competency Create(string name, Guid competencyCategoryId, string? description = null, bool isActive = true)
    {
        Guard(name, competencyCategoryId);
        return new Competency
        {
            Name = name,
            CompetencyCategoryId = competencyCategoryId,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(string name, Guid competencyCategoryId, string? description, bool isActive)
    {
        Guard(name, competencyCategoryId);
        Name = name;
        CompetencyCategoryId = competencyCategoryId;
        Description = description;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name, Guid competencyCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Competency name cannot be empty.", nameof(name));
        if (competencyCategoryId == Guid.Empty)
            throw new ArgumentException("Competency category is required.", nameof(competencyCategoryId));
    }
}
