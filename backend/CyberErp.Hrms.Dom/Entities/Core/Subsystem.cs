namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Master list of ERP subsystems (dbo.coreSubsystem). HRMS is one subsystem of the wider ERP;
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

    private Subsystem() : base() { }

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
