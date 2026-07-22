using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Family / relative entry (HC019). When the relative also works in the organization,
/// <see cref="RelatedEmployeeId"/> links to their employee record (internal relationships, HC020).
/// </summary>
public class EmployeeDependent : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid PersonId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Relationship { get; private set; } = string.Empty; // Spouse, Child, Parent, Sibling, ...
    public DateTime? DateOfBirth { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    /// <summary>True for dependents in the benefits sense (e.g. children).</summary>
    public bool IsDependent { get; private set; }
    /// <summary>Set when the relative is also an employee of the organization (HC020).</summary>
    public Guid? RelatedEmployeeId { get; private set; }
    public string? Remark { get; private set; }

    private EmployeeDependent() : base() { }

    public static EmployeeDependent Create(
        Guid personId,
        string fullName,
        string relationship,
        DateTime? dateOfBirth = null,
        string? phoneNumber = null,
        string? address = null,
        bool isDependent = false,
        Guid? relatedEmployeeId = null,
        string? remark = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person is required.", nameof(personId));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(relationship))
            throw new ArgumentException("Relationship cannot be empty.", nameof(relationship));
        // Self-reference (relative == the employee's own record) is validated in the save handler,
        // which knows the owning employee id — this entity only carries the person link.

        return new EmployeeDependent
        {
            PersonId = personId,
            FullName = fullName,
            Relationship = relationship,
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber,
            Address = address,
            IsDependent = isDependent,
            RelatedEmployeeId = relatedEmployeeId,
            Remark = remark
        };
    }

    public void Update(
        string fullName,
        string relationship,
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? address,
        bool isDependent,
        Guid? relatedEmployeeId,
        string? remark)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(relationship))
            throw new ArgumentException("Relationship cannot be empty.", nameof(relationship));

        FullName = fullName;
        Relationship = relationship;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        Address = address;
        IsDependent = isDependent;
        RelatedEmployeeId = relatedEmployeeId;
        Remark = remark;
        base.Update();
    }
}
