using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Kinds of files a candidate attaches. NationalId, GuarantorForm, MedicalCertificate and a signed
/// offer letter / employment contract form the MANDATORY compliance set that gates hiring.
/// </summary>
public enum CandidateDocumentType
{
    EducationCertificate = 0,
    ExperienceLetter = 1,
    NationalId = 2,
    GuarantorForm = 3,
    MedicalCertificate = 4,
    SignedOfferLetter = 5,
    EmploymentContract = 6,
    Resume = 7,
    Other = 8
}

/// <summary>
/// A file attached to a candidate (education certificates, experience letters, compliance
/// documents…). Stored inline like <see cref="EmployeeDocument"/>, so hiring migrates the rows
/// verbatim onto the employee's permanent history.
/// </summary>
public class CandidateDocument : BaseEntity, IAggregateRoot
{
    public Guid CandidateId { get; private set; }
    public CandidateDocumentType DocumentType { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long FileSize { get; private set; }
    public byte[] Content { get; private set; } = [];

    private CandidateDocument() : base() { }

    public static CandidateDocument Create(
        Guid candidateId,
        CandidateDocumentType documentType,
        string fileName,
        string contentType,
        byte[] content)
    {
        if (candidateId == Guid.Empty)
            throw new ArgumentException("Candidate is required.", nameof(candidateId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        if (content is null || content.Length == 0)
            throw new ArgumentException("File content cannot be empty.", nameof(content));

        return new CandidateDocument
        {
            CandidateId = candidateId,
            DocumentType = documentType,
            FileName = fileName,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = content.LongLength,
            Content = content
        };
    }
}
