namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Master list of ERP subsystems (Core.Subsystem). HRMS is one subsystem of the wider ERP;
/// modules reference a subsystem by <see cref="Name"/> (Module.SubSystem is a string key,
/// preserved from the template's permission model — no FK).
/// </summary>
/// <remarks>
/// ⚠️ SRMS owns this catalogue and CERP mirrors its schema exactly (2026-08-16). Two CERP-only
/// columns were dropped to get there:
/// <list type="bullet">
///   <item><c>SortOrder</c> — a duplicate of <see cref="DisplayOrder"/>, which SRMS keeps and
///   which is now the only ordering column.</item>
///   <item><c>Url</c> — the subsystem application's address. A deployment address is not tenant
///   data: it differs per environment while these rows are shared, so it belongs in configuration.
///   Both SPAs now resolve it from an env-var registry keyed by <see cref="Code"/>
///   (<c>VITE_SUBSYSTEM_APPS</c>), NOT from this table. Anything needing a launch target must go
///   through that registry — do not reintroduce the column.</item>
/// </list>
/// </remarks>
public class Subsystem : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;

    // ---- SRMS platform alignment (2026-08-14, logic.md §12.13) -------------
    /// <summary>Short form for compact UI, e.g. "HR".</summary>
    public string? Abbreviation { get; private set; }
    public string? Icon { get; private set; }
    public string Description { get; private set; } = string.Empty;
    /// <summary>Launcher ordering — the only ordering column since SortOrder was dropped.</summary>
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    /// <summary>Path the launcher opens within the configured application URL, e.g. "/dashboard".</summary>
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

    public static Subsystem Create(string name, string code, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subsystem name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Subsystem code cannot be empty.", nameof(code));

        return new Subsystem
        {
            Name = name.Trim(),
            Code = code.Trim(),
            DisplayOrder = displayOrder
        };
    }

    public void Update(string name, string code, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subsystem name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Subsystem code cannot be empty.", nameof(code));

        Name = name.Trim();
        Code = code.Trim();
        DisplayOrder = displayOrder;
        base.Update();
    }
}
