using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Stored value of a dynamic employee field (HC021); one row per employee per definition.</summary>
public class EmployeeFieldValue : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid FieldDefinitionId { get; private set; }
    public string? Value { get; private set; }

    private EmployeeFieldValue() : base() { }

    public static EmployeeFieldValue Create(Guid employeeId, Guid fieldDefinitionId, string? value)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (fieldDefinitionId == Guid.Empty)
            throw new ArgumentException("Field definition is required.", nameof(fieldDefinitionId));

        return new EmployeeFieldValue
        {
            EmployeeId = employeeId,
            FieldDefinitionId = fieldDefinitionId,
            Value = value
        };
    }

    public void SetValue(string? value)
    {
        Value = value;
        base.Update();
    }
}
