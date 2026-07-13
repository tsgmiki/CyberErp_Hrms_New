using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Stored value of a dynamic custom field (HC021); one row per owner record per definition.
/// Polymorphic (<see cref="OwnerType"/> + <see cref="OwnerId"/>, no FK) so the same engine serves the
/// Employee form and every child form — the owner cascade is handled by each owner's delete handler.</summary>
public class EmployeeFieldValue : BaseEntity, IAggregateRoot, IAuditable
{
    public EmployeeFieldOwnerType OwnerType { get; private set; }
    /// <summary>Id of the owning record — an employee id for <see cref="EmployeeFieldOwnerType.Employee"/>,
    /// or the child record's id (education/experience/…) for the other owner types.</summary>
    public Guid OwnerId { get; private set; }
    public Guid FieldDefinitionId { get; private set; }
    public string? Value { get; private set; }

    private EmployeeFieldValue() : base() { }

    public static EmployeeFieldValue Create(EmployeeFieldOwnerType ownerType, Guid ownerId, Guid fieldDefinitionId, string? value)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner is required.", nameof(ownerId));
        if (fieldDefinitionId == Guid.Empty)
            throw new ArgumentException("Field definition is required.", nameof(fieldDefinitionId));

        return new EmployeeFieldValue
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
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
