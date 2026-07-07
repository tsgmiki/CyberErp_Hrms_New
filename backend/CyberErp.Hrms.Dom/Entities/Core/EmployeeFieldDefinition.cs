using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum EmployeeFieldDataType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Boolean = 3,
    Select = 4
}

/// <summary>
/// Admin-defined dynamic employee field (HC021): lets the organization add or exclude employee
/// data fields without code changes. Values are stored per employee in
/// <see cref="EmployeeFieldValue"/>; the employee form renders these definitions dynamically.
/// Deactivating a definition hides the field everywhere without losing stored values.
/// </summary>
public class EmployeeFieldDefinition : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>Stable machine key (unique per tenant), e.g. "bloodType".</summary>
    public string Name { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public EmployeeFieldDataType DataType { get; private set; }
    /// <summary>Comma-separated options for <see cref="EmployeeFieldDataType.Select"/>.</summary>
    public string? Options { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private EmployeeFieldDefinition() : base() { }

    public static EmployeeFieldDefinition Create(
        string name,
        string label,
        EmployeeFieldDataType dataType,
        string? options = null,
        bool isRequired = false,
        bool isActive = true,
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Field label cannot be empty.", nameof(label));
        if (dataType == EmployeeFieldDataType.Select && string.IsNullOrWhiteSpace(options))
            throw new ArgumentException("Select fields require options.", nameof(options));

        return new EmployeeFieldDefinition
        {
            Name = name,
            Label = label,
            DataType = dataType,
            Options = options,
            IsRequired = isRequired,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }

    public void Update(
        string name,
        string label,
        EmployeeFieldDataType dataType,
        string? options,
        bool isRequired,
        bool isActive,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Field label cannot be empty.", nameof(label));
        if (dataType == EmployeeFieldDataType.Select && string.IsNullOrWhiteSpace(options))
            throw new ArgumentException("Select fields require options.", nameof(options));

        Name = name;
        Label = label;
        DataType = dataType;
        Options = options;
        IsRequired = isRequired;
        IsActive = isActive;
        SortOrder = sortOrder;
        base.Update();
    }
}
