using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Dom.Entities.Core;

public class Module : BaseEntity
{
    public string SubSystem { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    private readonly List<Operation> _operations = new();
    public IReadOnlyCollection<Operation> Operations => _operations.AsReadOnly();

    private Module() : base() { }

    public static Module Create(
        string subSystem,
        string name,
        string? icon = null)
    {
        if (string.IsNullOrWhiteSpace(subSystem))
            throw new ArgumentException("SubSystem cannot be empty.", nameof(subSystem));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Module name cannot be empty.", nameof(name));

        return new Module
        {
            SubSystem = subSystem,
            Name = name,
            Icon = icon
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(string? subSystem = null, string? name = null, string? icon = null)
    {
        if (subSystem != null)
        {
            if (string.IsNullOrWhiteSpace(subSystem))
                throw new ArgumentException("SubSystem cannot be empty.", nameof(subSystem));
            SubSystem = subSystem;
        }

        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Module name cannot be empty.", nameof(name));
            Name = name;
        }

        if (icon != null)
            Icon = icon;

        base.Update();
    }
    public void UpdateIcon(string? icon)
    {
        Icon = icon;
        base.Update();
    }
   
}

