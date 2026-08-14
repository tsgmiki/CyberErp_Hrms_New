using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Dom.Entities.Core;

public class Module : BaseEntity
{
    /// <summary>FK to the subsystem master list (Core.Subsystem).</summary>
    public Guid SubsystemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>NOT NULL since the 2026-08-15 SRMS alignment — blank, never null.</summary>
    public string Icon { get; private set; } = string.Empty;
    /// <summary>Platform column (row-level filter expression) — SRMS has it; CERP seeds it empty.</summary>
    public string Filter { get; private set; } = string.Empty;
    /// <summary>Menu position. Named SortOrder before the 2026-08-15 SRMS alignment.</summary>
    public int DisplayOrder { get; private set; }
    /// <summary>False hides the whole group — a template-level kill switch, as on Operation.</summary>
    public bool IsActive { get; private set; } = true;
    public Subsystem Subsystem { get; private set; } = null!;

    /*
     * ⚠️ The Operations collection is GONE (2026-08-13). Core.Operation now holds the menu tree in
     * one self-referencing table: a group is an Operation with a null ModuleId, and Operation.ModuleId
     * points at THAT row, not at a Module. Navigation reads the hierarchy, not this table.
     *
     * Core.Module survives because SubscriptionPlanModule and TenantSubscriptionAddOn have foreign
     * keys into it, and because a parent Operation's Id equals the Module it was copied from — an
     * invariant the migration established and SeedDefaultMenu maintains.
     */

    private Module() : base() { }

    /// <summary>
    /// Creates a module under an Id chosen by the caller, so it can mirror the parent
    /// <c>Operation</c> it belongs to. See the invariant note above — the two share an Id, which is
    /// what let the 150 existing operations survive the hierarchy migration without repointing.
    /// </summary>
    public static Module CreateWithId(
        Guid id,
        Guid subsystemId,
        string name,
        string? icon = null,
        int sortOrder = 0)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Module ID cannot be empty.", nameof(id));

        var module = Create(subsystemId, name, icon, sortOrder);
        module.AssignId(id);
        return module;
    }

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
            Icon = icon ?? string.Empty,
            DisplayOrder = sortOrder
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
            DisplayOrder = sortOrder.Value;

        base.Update();
    }
    public void UpdateIcon(string? icon)
    {
        Icon = icon ?? string.Empty;
        base.Update();
    }

}
