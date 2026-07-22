using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Educational background entry (HC017): level, field of study, institution, graduation.</summary>
public class EmployeeEducation : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid PersonId { get; private set; }
    public string EducationLevel { get; private set; } = string.Empty; // e.g. Diploma, BSc, MSc, PhD, Certification
    public string Institution { get; private set; } = string.Empty;
    public string? FieldOfStudy { get; private set; }
    public string? Qualification { get; private set; }
    public int? GraduationYear { get; private set; }
    public string? Remark { get; private set; }

    private EmployeeEducation() : base() { }

    public static EmployeeEducation Create(
        Guid personId,
        string educationLevel,
        string institution,
        string? fieldOfStudy = null,
        string? qualification = null,
        int? graduationYear = null,
        string? remark = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person is required.", nameof(personId));
        if (string.IsNullOrWhiteSpace(educationLevel))
            throw new ArgumentException("Education level cannot be empty.", nameof(educationLevel));
        if (string.IsNullOrWhiteSpace(institution))
            throw new ArgumentException("Institution cannot be empty.", nameof(institution));

        return new EmployeeEducation
        {
            PersonId = personId,
            EducationLevel = educationLevel,
            Institution = institution,
            FieldOfStudy = fieldOfStudy,
            Qualification = qualification,
            GraduationYear = graduationYear,
            Remark = remark
        };
    }

    public void Update(
        string educationLevel,
        string institution,
        string? fieldOfStudy,
        string? qualification,
        int? graduationYear,
        string? remark)
    {
        if (string.IsNullOrWhiteSpace(educationLevel))
            throw new ArgumentException("Education level cannot be empty.", nameof(educationLevel));
        if (string.IsNullOrWhiteSpace(institution))
            throw new ArgumentException("Institution cannot be empty.", nameof(institution));

        EducationLevel = educationLevel;
        Institution = institution;
        FieldOfStudy = fieldOfStudy;
        Qualification = qualification;
        GraduationYear = graduationYear;
        Remark = remark;
        base.Update();
    }
}
