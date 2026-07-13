using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum EmployeeFieldDataType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Boolean = 3,
    Select = 4,
    /// <summary>File attachments (dynamic forms only). Values live in EmployeeDocument, not the field value.</summary>
    Attachment = 5
}

/// <summary>
/// Which form/record a custom field definition (and its values) belongs to. Lets the same dynamic-field
/// engine (HC021) serve the main Employee form and every employee child form. The value name matches the
/// owning entity; the UI labels <see cref="Dependent"/> as "Family".
/// </summary>
public enum EmployeeFieldOwnerType
{
    Employee = 0,
    Education = 1,
    Experience = 2,
    Dependent = 3,
    Movement = 4,
    Discipline = 5,
    Termination = 6
}

/// <summary>
/// Admin-defined dynamic employee field (HC021): lets the organization add or exclude employee
/// data fields without code changes. Values are stored per employee in
/// <see cref="EmployeeFieldValue"/>; the employee form renders these definitions dynamically.
/// Deactivating a definition hides the field everywhere without losing stored values.
/// </summary>
public class EmployeeFieldDefinition : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>Which form/record this field applies to (Employee or a child form). Names are unique per
    /// (tenant, owner type), so each form has its own field namespace.</summary>
    public EmployeeFieldOwnerType OwnerType { get; private set; } = EmployeeFieldOwnerType.Employee;
    /// <summary>Stable machine key (unique per tenant + owner type), e.g. "bloodType".</summary>
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
        EmployeeFieldOwnerType ownerType = EmployeeFieldOwnerType.Employee,
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
            OwnerType = ownerType,
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
        EmployeeFieldOwnerType ownerType,
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

        OwnerType = ownerType;
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
