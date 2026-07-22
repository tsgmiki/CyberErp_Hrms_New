using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A user-defined dynamic form (a custom tab / repeatable collection) built without a developer —
/// SAP/Dynamics-style custom infotype. Scoped to a <see cref="Module"/> (e.g. "Employee") so the same
/// engine serves other modules later. Carries its own <see cref="DynamicFormField"/> schema; its data
/// rows live in <see cref="DynamicFormRecord"/> (one JSON document per record, keyed by owner).
/// </summary>
public class DynamicForm : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>Owning module (e.g. "Employee") — the tab renders inside that module's entity profile.</summary>
    public string Module { get; private set; } = string.Empty;
    /// <summary>Stable machine key (unique per tenant + module).</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>Tab title shown to end users.</summary>
    public string Label { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Optional lucide icon name for the tab (frontend maps it).</summary>
    public string? Icon { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private readonly List<DynamicFormField> _fields = [];
    public IReadOnlyCollection<DynamicFormField> Fields => _fields;

    private DynamicForm() : base() { }

    public static DynamicForm Create(string module, string name, string label,
        string? description = null, string? icon = null, bool isActive = true, int sortOrder = 0)
    {
        Guard(module, name, label);
        return new DynamicForm
        {
            Module = module,
            Name = name,
            Label = label,
            Description = description,
            Icon = icon,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }

    public void Update(string label, string? description, string? icon, bool isActive, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Form label cannot be empty.", nameof(label));
        Label = label;
        Description = description;
        Icon = icon;
        IsActive = isActive;
        SortOrder = sortOrder;
        base.Update();
    }

    /// <summary>
    /// Replaces the form's field schema from the submitted specs. Existing fields keep their Id when a
    /// spec carries one (so stored record data still resolves by field name); new specs create fresh
    /// fields. Field names must be unique within the form.
    /// </summary>
    public void SetFields(IEnumerable<DynamicFormFieldSpec> specs)
    {
        var list = specs.ToList();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in list)
        {
            if (string.IsNullOrWhiteSpace(s.Name))
                throw new ArgumentException("Field name cannot be empty.", nameof(specs));
            if (!names.Add(s.Name))
                throw new ArgumentException($"Duplicate field name '{s.Name}'.", nameof(specs));
        }

        _fields.Clear();
        var order = 0;
        foreach (var s in list)
        {
            _fields.Add(DynamicFormField.Create(Id, s.Name, s.Label, s.DataType, s.Options,
                s.IsRequired, s.IsActive, s.SortOrder != 0 ? s.SortOrder : order, s.ShowInList));
            order++;
        }
        base.Update();
    }

    private static void Guard(string module, string name, string label)
    {
        if (string.IsNullOrWhiteSpace(module))
            throw new ArgumentException("Module cannot be empty.", nameof(module));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Form name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Form label cannot be empty.", nameof(label));
    }
}

/// <summary>Input spec for one field when saving a form's schema (see <see cref="DynamicForm.SetFields"/>).</summary>
public record DynamicFormFieldSpec(
    string Name,
    string Label,
    EmployeeFieldDataType DataType,
    string? Options,
    bool IsRequired,
    bool IsActive,
    int SortOrder,
    bool ShowInList);

/// <summary>A field (column) of a <see cref="DynamicForm"/>. Reuses the HC021 <see cref="EmployeeFieldDataType"/>.</summary>
public class DynamicFormField : BaseEntity
{
    public Guid DynamicFormId { get; private set; }
    /// <summary>Stable machine key (unique per form) — also the JSON key in the record's data.</summary>
    public string Name { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public EmployeeFieldDataType DataType { get; private set; }
    /// <summary>Comma-separated options for <see cref="EmployeeFieldDataType.Select"/>.</summary>
    public string? Options { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }
    /// <summary>Whether the field appears as a column in the tab's list grid.</summary>
    public bool ShowInList { get; private set; } = true;

    private DynamicFormField() : base() { }

    public static DynamicFormField Create(Guid dynamicFormId, string name, string label,
        EmployeeFieldDataType dataType, string? options, bool isRequired, bool isActive, int sortOrder, bool showInList)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Field label cannot be empty.", nameof(label));
        if (dataType == EmployeeFieldDataType.Select && string.IsNullOrWhiteSpace(options))
            throw new ArgumentException("Select fields require options.", nameof(options));

        return new DynamicFormField
        {
            DynamicFormId = dynamicFormId,
            Name = name,
            Label = label,
            DataType = dataType,
            Options = options,
            IsRequired = isRequired,
            IsActive = isActive,
            SortOrder = sortOrder,
            ShowInList = showInList
        };
    }
}
