using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// An appraisal form template — defines how the overall performance score is composed from the goals
/// and competencies sections (HC138). Competencies themselves are derived from the employee's position
/// (HC123), so a template only carries the section weight split.
/// </summary>
public class AppraisalTemplate : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Weight (%) of the goals section in the overall score.</summary>
    public decimal GoalsWeight { get; private set; }
    /// <summary>Weight (%) of the competencies section in the overall score.</summary>
    public decimal CompetenciesWeight { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AppraisalTemplate() : base() { }

    public static AppraisalTemplate Create(string name, decimal goalsWeight, decimal competenciesWeight,
        string? description = null, bool isActive = true)
    {
        Guard(name, goalsWeight, competenciesWeight);
        return new AppraisalTemplate
        {
            Name = name,
            GoalsWeight = goalsWeight,
            CompetenciesWeight = competenciesWeight,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(string name, decimal goalsWeight, decimal competenciesWeight, string? description, bool isActive)
    {
        Guard(name, goalsWeight, competenciesWeight);
        Name = name;
        GoalsWeight = goalsWeight;
        CompetenciesWeight = competenciesWeight;
        Description = description;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name, decimal goalsWeight, decimal competenciesWeight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name cannot be empty.", nameof(name));
        if (goalsWeight < 0 || competenciesWeight < 0)
            throw new ArgumentException("Weights cannot be negative.");
        if (goalsWeight + competenciesWeight != 100m)
            throw new ArgumentException("Goals and competencies weights must add up to 100%.");
    }
}
