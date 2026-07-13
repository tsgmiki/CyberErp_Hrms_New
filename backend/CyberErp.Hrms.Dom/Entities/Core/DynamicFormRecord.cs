using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// One data row of a <see cref="DynamicForm"/> for a specific owner (e.g. an employee). All field
/// values are stored together in a single JSON <see cref="Data"/> document ({fieldName: value}), so a
/// record is one row — reading a form's records for an owner is a single indexed range scan on
/// (DynamicFormId, OwnerType, OwnerId), with no EAV pivot. The polymorphic owner (OwnerType + OwnerId,
/// no FK) mirrors <see cref="EmployeeFieldValue"/> / EmployeeDocument, so any module can own records.
/// </summary>
public class DynamicFormRecord : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid DynamicFormId { get; private set; }
    /// <summary>Owning entity type (e.g. "Employee").</summary>
    public string OwnerType { get; private set; } = string.Empty;
    /// <summary>Id of the owning record (e.g. the employee id).</summary>
    public Guid OwnerId { get; private set; }
    /// <summary>Field values as a JSON object string: {"fieldName":"value", ...}. Values are strings.</summary>
    public string Data { get; private set; } = "{}";

    private DynamicFormRecord() : base() { }

    public static DynamicFormRecord Create(Guid dynamicFormId, string ownerType, Guid ownerId, string data)
    {
        if (dynamicFormId == Guid.Empty)
            throw new ArgumentException("Form is required.", nameof(dynamicFormId));
        if (string.IsNullOrWhiteSpace(ownerType))
            throw new ArgumentException("Owner type is required.", nameof(ownerType));
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner is required.", nameof(ownerId));

        return new DynamicFormRecord
        {
            DynamicFormId = dynamicFormId,
            OwnerType = ownerType,
            OwnerId = ownerId,
            Data = string.IsNullOrWhiteSpace(data) ? "{}" : data
        };
    }

    public void SetData(string data)
    {
        Data = string.IsNullOrWhiteSpace(data) ? "{}" : data;
        base.Update();
    }
}
