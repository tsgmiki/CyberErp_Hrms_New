using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Person master record (table <c>Core.CorePerson</c>) — personal identity separated from the
/// employment record. Ethiopian naming (first / father / grandfather) with Amharic variants.
/// An <see cref="Employee"/> references a person via PersonId; person-owned collections
/// (education, experience, family) hang off the person so identity data survives and can be
/// shared across employment records. Soft-deletable via <see cref="IsDeleted"/>.
/// </summary>
public class Person : BaseEntity, IAggregateRoot, IAuditable
{
    public string FirstName { get; private set; } = string.Empty;
    /// <summary>First name in Amharic.</summary>
    public string? FirstNameA { get; private set; }
    public string? FatherName { get; private set; }
    public string? FatherNameA { get; private set; }
    public string GrandFatherName { get; private set; } = string.Empty;
    public string? GrandFatherNameA { get; private set; }
    public Gender Gender { get; private set; }
    /// <summary>Future nationality lookup reference (no lookup table yet).</summary>
    public Guid? NationalityId { get; private set; }
    /// <summary>Marital status stored as the numeric enum value (per the CorePerson column spec).</summary>
    public MaritalStatus MaritalStatusId { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? LocationName { get; private set; }
    public bool IsDeleted { get; private set; }

    /// <summary>Latin full name: first + father + grandfather.</summary>
    public string FullName => string.Join(" ",
        new[] { FirstName, FatherName, GrandFatherName }.Where(p => !string.IsNullOrWhiteSpace(p)));

    private Person() : base() { }

    public static Person Create(
        string firstName,
        string grandFatherName,
        Gender gender,
        MaritalStatus maritalStatus,
        string? fatherName = null,
        string? firstNameA = null,
        string? fatherNameA = null,
        string? grandFatherNameA = null,
        Guid? nationalityId = null,
        string? phoneNumber = null,
        string? locationName = null)
    {
        Guard(firstName, grandFatherName);
        return new Person
        {
            FirstName = firstName,
            FirstNameA = firstNameA,
            FatherName = fatherName,
            FatherNameA = fatherNameA,
            GrandFatherName = grandFatherName,
            GrandFatherNameA = grandFatherNameA,
            Gender = gender,
            MaritalStatusId = maritalStatus,
            NationalityId = nationalityId,
            PhoneNumber = phoneNumber,
            LocationName = locationName
        };
    }

    public void Update(
        string firstName,
        string? fatherName,
        string grandFatherName,
        Gender gender,
        MaritalStatus maritalStatus,
        string? firstNameA,
        string? fatherNameA,
        string? grandFatherNameA,
        Guid? nationalityId,
        string? phoneNumber,
        string? locationName)
    {
        Guard(firstName, grandFatherName);
        FirstName = firstName;
        FirstNameA = firstNameA;
        FatherName = fatherName;
        FatherNameA = fatherNameA;
        GrandFatherName = grandFatherName;
        GrandFatherNameA = grandFatherNameA;
        Gender = gender;
        MaritalStatusId = maritalStatus;
        NationalityId = nationalityId;
        PhoneNumber = phoneNumber;
        LocationName = locationName;
        base.Update();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        base.Update();
    }

    private static void Guard(string firstName, string grandFatherName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(grandFatherName))
            throw new ArgumentException("Grandfather name cannot be empty.", nameof(grandFatherName));
    }
}
