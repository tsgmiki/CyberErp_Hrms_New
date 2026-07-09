using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Where a candidate entered the pipeline (HC088/HC092).</summary>
public enum CandidateSource
{
    External = 0,
    Internal = 1,      // current employee (internal job market, HC088/HC090)
    JobBoard = 2,
    SocialMedia = 3,
    Referral = 4,
    WalkIn = 5
}

/// <summary>
/// Centralized applicant record (HC096): identity, contacts, structured education/experience/skills
/// summaries (automated resume parsing is a later integration on top of these fields, HC094), the
/// uploaded resume, and the mandatory data-processing consent (HC097). Internal candidates link to
/// their employee record (HC090); promising profiles are flagged into the talent pool (HC089).
/// Retention: <see cref="Anonymize"/> scrubs PII while keeping the anonymous pipeline history.
/// </summary>
public class Candidate : BaseEntity, IAggregateRoot, IAuditable
{
    public string CandidateNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string? FatherName { get; private set; }
    public string? GrandFatherName { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Gender? Gender { get; private set; }
    public CandidateSource Source { get; private set; }
    /// <summary>The employee record of an internal candidate (HC090).</summary>
    public Guid? InternalEmployeeId { get; private set; }
    /// <summary>
    /// The shared person record (Core.CorePerson) backing this candidate — established at capture so
    /// hiring converts the candidate into an employee on the SAME person, with no data re-entry.
    /// </summary>
    public Guid? PersonId { get; private set; }
    /// <summary>The employee created when this candidate was hired (null until then).</summary>
    public Guid? HiredEmployeeId { get; private set; }

    // Structured profile (HC094 capture surface; automated parsing populates these later)
    public string? EducationSummary { get; private set; }
    public string? ExperienceSummary { get; private set; }
    public string? SkillsSummary { get; private set; }
    public int? YearsOfExperience { get; private set; }
    /// <summary>Stored resume file name (served via GET Candidate/{id}/resume, HC093).</summary>
    public string? ResumeFileName { get; private set; }

    // Consent & retention (HC097)
    public bool ConsentGiven { get; private set; }
    public DateTime? ConsentAt { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime? AnonymizedAt { get; private set; }

    // Talent pool (HC089)
    public bool IsInTalentPool { get; private set; }
    public string? TalentPoolNotes { get; private set; }

    public string FullName =>
        string.Join(" ", new[] { FirstName, FatherName, GrandFatherName }.Where(p => !string.IsNullOrWhiteSpace(p)));

    private Candidate() : base() { }

    public static Candidate Create(
        string candidateNumber,
        string firstName,
        CandidateSource source,
        bool consentGiven,
        string? fatherName = null,
        string? grandFatherName = null,
        string? email = null,
        string? phoneNumber = null,
        Gender? gender = null,
        Guid? internalEmployeeId = null,
        string? educationSummary = null,
        string? experienceSummary = null,
        string? skillsSummary = null,
        int? yearsOfExperience = null)
    {
        Guard(candidateNumber, firstName, consentGiven, yearsOfExperience);
        return new Candidate
        {
            CandidateNumber = candidateNumber,
            FirstName = firstName,
            FatherName = fatherName,
            GrandFatherName = grandFatherName,
            Email = email,
            PhoneNumber = phoneNumber,
            Gender = gender,
            Source = source,
            InternalEmployeeId = internalEmployeeId,
            EducationSummary = educationSummary,
            ExperienceSummary = experienceSummary,
            SkillsSummary = skillsSummary,
            YearsOfExperience = yearsOfExperience,
            ConsentGiven = consentGiven,
            ConsentAt = DateTime.UtcNow
        };
    }

    public void Update(
        string firstName,
        CandidateSource source,
        string? fatherName,
        string? grandFatherName,
        string? email,
        string? phoneNumber,
        Gender? gender,
        Guid? internalEmployeeId,
        string? educationSummary,
        string? experienceSummary,
        string? skillsSummary,
        int? yearsOfExperience)
    {
        EnsureNotAnonymized();
        Guard(CandidateNumber, firstName, ConsentGiven, yearsOfExperience);
        FirstName = firstName;
        Source = source;
        FatherName = fatherName;
        GrandFatherName = grandFatherName;
        Email = email;
        PhoneNumber = phoneNumber;
        Gender = gender;
        InternalEmployeeId = internalEmployeeId;
        EducationSummary = educationSummary;
        ExperienceSummary = experienceSummary;
        SkillsSummary = skillsSummary;
        YearsOfExperience = yearsOfExperience;
        base.Update();
    }

    /// <summary>Links the shared person record backing this candidate (hire-conversion anchor).</summary>
    public void SetPerson(Guid personId)
    {
        EnsureNotAnonymized();
        if (personId == Guid.Empty)
            throw new ArgumentException("A person record is required.", nameof(personId));
        PersonId = personId;
        base.Update();
    }

    /// <summary>
    /// Marks the candidate as hired: linked to the created employee and archived out of the active
    /// applicant pool (the person record lives on as the employee's identity).
    /// </summary>
    public void MarkHired(Guid employeeId)
    {
        EnsureNotAnonymized();
        if (employeeId == Guid.Empty)
            throw new ArgumentException("The hired employee reference is required.", nameof(employeeId));
        if (HiredEmployeeId.HasValue)
            throw new InvalidOperationException("The candidate has already been hired.");
        HiredEmployeeId = employeeId;
        IsArchived = true;
        base.Update();
    }

    /// <summary>Records the stored resume file after a successful upload (HC093).</summary>
    public void SetResume(string fileName)
    {
        EnsureNotAnonymized();
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Resume file name cannot be empty.", nameof(fileName));
        ResumeFileName = fileName;
        base.Update();
    }

    /// <summary>Flags / unflags the candidate for the internal talent pool (HC089).</summary>
    public void SetTalentPool(bool inPool, string? notes)
    {
        EnsureNotAnonymized();
        IsInTalentPool = inPool;
        TalentPoolNotes = inPool ? notes : null;
        base.Update();
    }

    /// <summary>Archives the record for retention (still identifiable; use Anonymize to scrub PII).</summary>
    public void Archive()
    {
        IsArchived = true;
        base.Update();
    }

    /// <summary>
    /// Retention-policy anonymization (HC097): scrubs all personal data while keeping the
    /// anonymous record (and its application history) for statistics. Irreversible.
    /// </summary>
    public void Anonymize()
    {
        if (HiredEmployeeId.HasValue)
            throw new InvalidOperationException(
                "A hired candidate's identity lives on as the employee record — it cannot be anonymized here.");
        FirstName = "Anonymized";
        FatherName = null;
        GrandFatherName = null;
        Email = null;
        PhoneNumber = null;
        Gender = null;
        InternalEmployeeId = null;
        EducationSummary = null;
        ExperienceSummary = null;
        SkillsSummary = null;
        YearsOfExperience = null;
        ResumeFileName = null;
        TalentPoolNotes = null;
        IsInTalentPool = false;
        IsArchived = true;
        AnonymizedAt = DateTime.UtcNow;
        base.Update();
    }

    private void EnsureNotAnonymized()
    {
        if (AnonymizedAt.HasValue)
            throw new InvalidOperationException("An anonymized candidate record can no longer change.");
    }

    private static void Guard(string number, string firstName, bool consentGiven, int? yearsOfExperience)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Candidate number cannot be empty.", nameof(number));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Candidate name cannot be empty.", nameof(firstName));
        if (!consentGiven)
            throw new ArgumentException("Data-processing consent is required to record a candidate (HC097).", nameof(consentGiven));
        if (yearsOfExperience is < 0)
            throw new ArgumentException("Years of experience cannot be negative.", nameof(yearsOfExperience));
    }
}
