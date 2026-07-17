using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A configurable award / badge (HC141) that can be granted to recognize high performers, e.g.
/// "Employee of the Month", "Innovator". Carries display metadata (colour, icon) for public boards.
/// </summary>
public class RecognitionBadge : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Display colour (hex) for the badge chip.</summary>
    public string? Color { get; private set; }
    /// <summary>Lucide icon name for the badge.</summary>
    public string? Icon { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private RecognitionBadge() : base() { }

    public static RecognitionBadge Create(string name, string? description, string? color, string? icon,
        bool isActive = true, int sortOrder = 0)
    {
        Guard(name);
        return new RecognitionBadge
        {
            Name = name,
            Description = description,
            Color = color,
            Icon = icon,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }

    public void Update(string name, string? description, string? color, string? icon, bool isActive, int sortOrder)
    {
        Guard(name);
        Name = name;
        Description = description;
        Color = color;
        Icon = icon;
        IsActive = isActive;
        SortOrder = sortOrder;
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Badge name cannot be empty.", nameof(name));
    }
}

/// <summary>
/// A recognition granted to an employee — a <see cref="RecognitionBadge"/> awarded with a citation
/// (HC141). Public grants surface on the recognition board; who/when granted come from the audit
/// interceptor.
/// </summary>
public class EmployeeRecognition : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid RecognitionBadgeId { get; private set; }
    public string Citation { get; private set; } = string.Empty;
    public DateTime RecognizedOn { get; private set; }
    public bool IsPublic { get; private set; } = true;

    private EmployeeRecognition() : base() { }

    public static EmployeeRecognition Create(Guid employeeId, Guid recognitionBadgeId, string citation,
        DateTime recognizedOn, bool isPublic = true)
    {
        Guard(employeeId, recognitionBadgeId, citation);
        return new EmployeeRecognition
        {
            EmployeeId = employeeId,
            RecognitionBadgeId = recognitionBadgeId,
            Citation = citation,
            RecognizedOn = recognizedOn,
            IsPublic = isPublic
        };
    }

    public void Update(Guid employeeId, Guid recognitionBadgeId, string citation, DateTime recognizedOn, bool isPublic)
    {
        Guard(employeeId, recognitionBadgeId, citation);
        EmployeeId = employeeId;
        RecognitionBadgeId = recognitionBadgeId;
        Citation = citation;
        RecognizedOn = recognizedOn;
        IsPublic = isPublic;
        base.Update();
    }

    private static void Guard(Guid employeeId, Guid recognitionBadgeId, string citation)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (recognitionBadgeId == Guid.Empty)
            throw new ArgumentException("A badge is required.", nameof(recognitionBadgeId));
        if (string.IsNullOrWhiteSpace(citation))
            throw new ArgumentException("A citation is required.", nameof(citation));
    }
}
