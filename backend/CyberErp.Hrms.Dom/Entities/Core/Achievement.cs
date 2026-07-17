using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Kind of achievement recorded in the log (HC140).</summary>
public enum AchievementCategory
{
    Milestone = 0,
    Award = 1,
    Project = 2,
    Certification = 3,
    Other = 4
}

/// <summary>
/// An employee achievement or milestone (HC139) recorded by a manager. The collection of these for an
/// employee forms the "Achievements Log" surfaced on the profile (HC140). Optionally linked to the
/// appraisal it arose from.
/// </summary>
public class Achievement : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime AchievementDate { get; private set; }
    public AchievementCategory Category { get; private set; } = AchievementCategory.Milestone;
    public Guid? AppraisalId { get; private set; }

    private Achievement() : base() { }

    public static Achievement Create(Guid employeeId, string title, DateTime achievementDate,
        AchievementCategory category, string? description = null, Guid? appraisalId = null)
    {
        Guard(employeeId, title);
        return new Achievement
        {
            EmployeeId = employeeId,
            Title = title,
            AchievementDate = achievementDate,
            Category = category,
            Description = description,
            AppraisalId = appraisalId
        };
    }

    public void Update(Guid employeeId, string title, DateTime achievementDate,
        AchievementCategory category, string? description, Guid? appraisalId)
    {
        Guard(employeeId, title);
        EmployeeId = employeeId;
        Title = title;
        AchievementDate = achievementDate;
        Category = category;
        Description = description;
        AppraisalId = appraisalId;
        base.Update();
    }

    private static void Guard(Guid employeeId, string title)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Achievement title cannot be empty.", nameof(title));
    }
}
