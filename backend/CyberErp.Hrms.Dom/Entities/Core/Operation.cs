using System.Reflection;

namespace CyberErp.Hrms.Dom.Entities.Core;

public class Operation : BaseEntity
{
    public Guid ModuleId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Link { get; private set; } = string.Empty;
    public string Filter { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    public Module Module { get; private set; } = null!;

    private Operation() : base() { }

    public static Operation Create(
        Guid moduleId,
        string name,
        string link,
        string filter,
        string icon)
    {
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module ID cannot be empty.", nameof(moduleId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Operation name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Link cannot be empty.", nameof(link));

        return new Operation
        {
            ModuleId = moduleId,
            Name = name,
            Link = link,
            Filter = filter,
            Icon = icon
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(
        Guid moduleId,
        string name,
        string link,
        string filter,
        string icon)
    {
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module ID cannot be empty.", nameof(moduleId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Operation name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Link cannot be empty.", nameof(link));

        ModuleId = moduleId;
        Name = name;
        Link = link;
        Filter = filter;
        Icon = icon;
        base.Update();
    }
}

