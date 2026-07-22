using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Kinds of company property tracked for recovery (HC214).</summary>
public enum AssetCategory
{
    ITEquipment = 0,
    AccessCard = 1,
    Key = 2,
    Vehicle = 3,
    Tool = 4,
    Other = 5
}

public enum AssetStatus
{
    Available = 0,
    Assigned = 1,
    Retired = 2
}

/// <summary>
/// A trackable company asset (HC214): IT equipment, access cards, office keys, vehicles, tools.
/// Assigned to at most one employee at a time; exits generate a recovery checklist from the
/// employee's assignments.
/// </summary>
public class CompanyAsset : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public AssetCategory Category { get; private set; } = AssetCategory.ITEquipment;
    public string? SerialNo { get; private set; }
    public string? Description { get; private set; }
    public AssetStatus Status { get; private set; } = AssetStatus.Available;
    public Guid? AssignedToEmployeeId { get; private set; }
    public DateTime? AssignedOn { get; private set; }

    private CompanyAsset() : base() { }

    public static CompanyAsset Create(string name, AssetCategory category, string? serialNo, string? description)
    {
        Guard(name);
        return new CompanyAsset
        {
            Name = name,
            Category = category,
            SerialNo = serialNo,
            Description = description
        };
    }

    public void Update(string name, AssetCategory category, string? serialNo, string? description)
    {
        Guard(name);
        Name = name;
        Category = category;
        SerialNo = serialNo;
        Description = description;
        base.Update();
    }

    public void AssignTo(Guid employeeId, DateTime assignedOn)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (Status != AssetStatus.Available)
            throw new InvalidOperationException($"Only an available asset can be assigned (current: {Status}).");
        Status = AssetStatus.Assigned;
        AssignedToEmployeeId = employeeId;
        AssignedOn = assignedOn;
        base.Update();
    }

    /// <summary>Back into the pool — also the effect of a successful exit recovery (HC215).</summary>
    public void Return()
    {
        if (Status != AssetStatus.Assigned)
            throw new InvalidOperationException($"Only an assigned asset can be returned (current: {Status}).");
        Status = AssetStatus.Available;
        AssignedToEmployeeId = null;
        AssignedOn = null;
        base.Update();
    }

    /// <summary>Written off — the effect of waiving an unrecoverable item on an exit checklist.</summary>
    public void Retire()
    {
        Status = AssetStatus.Retired;
        AssignedToEmployeeId = null;
        AssignedOn = null;
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Asset name cannot be empty.", nameof(name));
    }
}

/// <summary>State of one item on an exit's asset-recovery checklist (HC215).</summary>
public enum AssetRecoveryStatus
{
    Outstanding = 0,
    Recovered = 1,
    Waived = 2
}

/// <summary>
/// One line of the asset-recovery checklist generated when an exit case enters clearance (HC215):
/// a snapshot of the assigned asset, ticked off as items come back (or waived as written off).
/// Settlement is blocked while lines are outstanding.
/// </summary>
public class TerminationAssetRecovery : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TerminationId { get; private set; }
    public Guid CompanyAssetId { get; private set; }
    /// <summary>Display snapshot — the checklist stays readable even as the registry changes.</summary>
    public string AssetName { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string? SerialNo { get; private set; }
    public AssetRecoveryStatus Status { get; private set; } = AssetRecoveryStatus.Outstanding;
    public DateTime? ResolvedOn { get; private set; }
    public string? Note { get; private set; }

    private TerminationAssetRecovery() : base() { }

    public static TerminationAssetRecovery Create(Guid terminationId, CompanyAsset asset)
    {
        if (terminationId == Guid.Empty)
            throw new ArgumentException("A termination case is required.", nameof(terminationId));
        return new TerminationAssetRecovery
        {
            TerminationId = terminationId,
            CompanyAssetId = asset.Id,
            AssetName = asset.Name,
            Category = asset.Category.ToString(),
            SerialNo = asset.SerialNo
        };
    }

    public void MarkRecovered(DateTime resolvedOn, string? note)
    {
        EnsureOutstanding();
        Status = AssetRecoveryStatus.Recovered;
        ResolvedOn = resolvedOn;
        Note = note;
        base.Update();
    }

    public void Waive(DateTime resolvedOn, string? note)
    {
        EnsureOutstanding();
        Status = AssetRecoveryStatus.Waived;
        ResolvedOn = resolvedOn;
        Note = note;
        base.Update();
    }

    private void EnsureOutstanding()
    {
        if (Status != AssetRecoveryStatus.Outstanding)
            throw new InvalidOperationException($"The item is already {Status}.");
    }
}
