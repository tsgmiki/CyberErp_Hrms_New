namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Master list of ERP subsystems (Core.Subsystem). HRMS is one subsystem of the wider ERP;
/// modules reference a subsystem by <see cref="Name"/> (Module.SubSystem is a string key,
/// preserved from the template's permission model — no FK).
/// </summary>
public class Subsystem : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    /// <summary>
    /// Where the subsystem's application lives (absolute URL, or a path on a shared origin).
    /// The Home portal's launcher tiles deep-link here; null = not yet routable (tile disabled).
    /// </summary>
    public string? Url { get; private set; }

    // ---- SRMS platform alignment (2026-08-14, logic.md §12.13) -------------
    /// <summary>Short form for compact UI, e.g. "HR".</summary>
    public string? Abbreviation { get; private set; }
    public string? Icon { get; private set; }
    public string Description { get; private set; } = string.Empty;
    /// <summary>Launcher ordering. Distinct from <see cref="SortOrder"/>, which CERP already had.</summary>
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    /// <summary>Path the launcher opens within <see cref="Url"/>, e.g. "/dashboard".</summary>
    public string LandingPath { get; private set; } = string.Empty;

    private Subsystem() : base() { }

    /// <summary>Sets the presentation fields the SRMS schema carries.</summary>
    public void SetPresentation(string? abbreviation, string? icon, string? description,
        int displayOrder, string? landingPath, bool isActive)
    {
        Abbreviation = string.IsNullOrWhiteSpace(abbreviation) ? null : abbreviation.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        Description = description?.Trim() ?? string.Empty;
        DisplayOrder = displayOrder;
        LandingPath = landingPath?.Trim() ?? string.Empty;
        IsActive = isActive;
        base.Update();
    }

    public static Subsystem Create(string name, string code, int sortOrder = 0, string? url = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subsystem name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Subsystem code cannot be empty.", nameof(code));

        return new Subsystem
        {
            Name = name.Trim(),
            Code = code.Trim(),
            SortOrder = sortOrder,
            Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim()
        };
    }

    public void Update(string name, string code, int sortOrder, string? url = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subsystem name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Subsystem code cannot be empty.", nameof(code));

        Name = name.Trim();
        Code = code.Trim();
        SortOrder = sortOrder;
        Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        base.Update();
    }
}
