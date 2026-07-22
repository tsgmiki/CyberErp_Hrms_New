using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>The kind of record a document is attached to.</summary>
public enum EmployeeDocumentOwner
{
    Education = 0,
    Experience = 1,
    /// <summary>Documents migrated from the candidate file at hire (OwnerId = the employee id).</summary>
    Recruitment = 2,
    /// <summary>Files attached to a dynamic-form record via an Attachment field (OwnerId = the record id).</summary>
    DynamicFormRecord = 3
}

/// <summary>
/// A file attached to an employee sub-record (education / experience, HC017/HC018). The binary is
/// stored inline so downloads are self-contained. <see cref="OwnerType"/> + <see cref="OwnerId"/>
/// identify the record it belongs to; <see cref="EmployeeId"/> scopes access through the employee.
/// </summary>
public class EmployeeDocument : BaseEntity, IAggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public EmployeeDocumentOwner OwnerType { get; private set; }
    public Guid OwnerId { get; private set; }
    /// <summary>Optional sub-scope within the owner — e.g. a dynamic form's Attachment field name, so
    /// each attachment field on the same record keeps a separate file pool. Null for education/experience.</summary>
    public string? OwnerField { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long FileSize { get; private set; }
    public byte[] Content { get; private set; } = [];

    private EmployeeDocument() : base() { }

    /// <summary>
    /// Re-anchors the access scope to a (new) employee. Used at hire: documents attached to a
    /// candidate's education/experience rows are anchored to the candidate id until the person
    /// becomes an employee, then move under the employee's scope.
    /// </summary>
    public void AssignEmployee(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        EmployeeId = employeeId;
        Update();
    }

    public static EmployeeDocument Create(
        Guid employeeId,
        EmployeeDocumentOwner ownerType,
        Guid ownerId,
        string fileName,
        string contentType,
        byte[] content,
        string? ownerField = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner record is required.", nameof(ownerId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        if (content is null || content.Length == 0)
            throw new ArgumentException("File content cannot be empty.", nameof(content));

        return new EmployeeDocument
        {
            EmployeeId = employeeId,
            OwnerType = ownerType,
            OwnerId = ownerId,
            OwnerField = string.IsNullOrWhiteSpace(ownerField) ? null : ownerField,
            FileName = fileName,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = content.LongLength,
            Content = content
        };
    }
}
