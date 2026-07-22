using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Dom.Entities.Core;

public class Module : BaseEntity
{
    /// <summary>FK to the subsystem master list (dbo.coreSubsystem).</summary>
    public Guid SubsystemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public int SortOrder { get; private set; }
    public Subsystem Subsystem { get; private set; } = null!;
    private readonly List<Operation> _operations = new();
    public IReadOnlyCollection<Operation> Operations => _operations.AsReadOnly();

    private Module() : base() { }

    public static Module Create(
        Guid subsystemId,
        string name,
        string? icon = null,
        int sortOrder = 0)
    {
        if (subsystemId == Guid.Empty)
            throw new ArgumentException("Subsystem ID cannot be empty.", nameof(subsystemId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Module name cannot be empty.", nameof(name));

        return new Module
        {
            SubsystemId = subsystemId,
            Name = name,
            Icon = icon,
            SortOrder = sortOrder
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(Guid? subsystemId = null, string? name = null, string? icon = null, int? sortOrder = null)
    {
        if (subsystemId.HasValue)
        {
            if (subsystemId.Value == Guid.Empty)
                throw new ArgumentException("Subsystem ID cannot be empty.", nameof(subsystemId));
            SubsystemId = subsystemId.Value;
        }

        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Module name cannot be empty.", nameof(name));
            Name = name;
        }

        if (icon != null)
            Icon = icon;

        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;

        base.Update();
    }
    public void UpdateIcon(string? icon)
    {
        Icon = icon;
        base.Update();
    }

}
