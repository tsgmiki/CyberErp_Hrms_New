using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Who initiated a medical claim (HC243).</summary>
public enum MedicalClaimSource
{
    Employee = 0,
    Provider = 1
}

/// <summary>Claim status (HC244).</summary>
public enum MedicalClaimStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Paid = 4
}

/// <summary>
/// HC239–244 — a medical reimbursement claim for a covered beneficiary against an enrollment. Moves
/// Pending → (UnderReview) → Approved/Rejected → Paid. The approved amount reflects the plan's coverage;
/// the paid step is the finance/CBS hand-off (reference captured, actual payment external).
/// </summary>
public class MedicalClaim : BaseEntity, IAggregateRoot, IAuditable
{
    public string ClaimNumber { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }
    public Guid MedicalEnrollmentId { get; private set; }
    public Guid MedicalBeneficiaryId { get; private set; }
    public BeneficiaryCategory BeneficiaryCategory { get; private set; }
    public Guid MedicalPlanId { get; private set; }
    public Guid? MedicalProviderId { get; private set; }
    public MedicalClaimSource Source { get; private set; }
    public DateTime ServiceDate { get; private set; }
    public DateTime SubmittedOn { get; private set; }
    public decimal ClaimedAmount { get; private set; }
    /// <summary>Reimbursable amount (set at approval from the plan coverage; null until approved).</summary>
    public decimal? ApprovedAmount { get; private set; }
    public MedicalClaimStatus Status { get; private set; } = MedicalClaimStatus.Pending;
    public string Description { get; private set; } = string.Empty;
    public string? Diagnosis { get; private set; }
    public string? Resolution { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? PaymentReference { get; private set; }

    private readonly List<MedicalClaimAttachment> _attachments = [];
    public IReadOnlyCollection<MedicalClaimAttachment> Attachments => _attachments;

    private MedicalClaim() : base() { }

    public static MedicalClaim Create(string claimNumber, Guid employeeId, Guid enrollmentId, Guid beneficiaryId,
        BeneficiaryCategory category, Guid medicalPlanId, Guid? providerId, MedicalClaimSource source,
        DateTime serviceDate, decimal claimedAmount, string description, string? diagnosis, DateTime submittedOn)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (enrollmentId == Guid.Empty) throw new ArgumentException("Enrollment is required.", nameof(enrollmentId));
        if (beneficiaryId == Guid.Empty) throw new ArgumentException("Beneficiary is required.", nameof(beneficiaryId));
        if (claimedAmount <= 0) throw new ArgumentException("Claimed amount must be positive.", nameof(claimedAmount));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        return new MedicalClaim
        {
            ClaimNumber = claimNumber,
            EmployeeId = employeeId,
            MedicalEnrollmentId = enrollmentId,
            MedicalBeneficiaryId = beneficiaryId,
            BeneficiaryCategory = category,
            MedicalPlanId = medicalPlanId,
            MedicalProviderId = providerId,
            Source = source,
            ServiceDate = serviceDate,
            SubmittedOn = submittedOn,
            ClaimedAmount = claimedAmount,
            Status = MedicalClaimStatus.Pending,
            Description = description.Trim(),
            Diagnosis = diagnosis
        };
    }

    public void StartReview()
    {
        if (Status is MedicalClaimStatus.Approved or MedicalClaimStatus.Rejected or MedicalClaimStatus.Paid)
            throw new InvalidOperationException("A closed claim cannot be moved to review.");
        Status = MedicalClaimStatus.UnderReview;
        base.Update();
    }

    public void Approve(decimal approvedAmount, string? note)
    {
        if (Status is MedicalClaimStatus.Paid)
            throw new InvalidOperationException("A paid claim cannot be re-approved.");
        if (approvedAmount < 0 || approvedAmount > ClaimedAmount)
            throw new ArgumentException("Approved amount must be between 0 and the claimed amount.", nameof(approvedAmount));
        Status = MedicalClaimStatus.Approved;
        ApprovedAmount = approvedAmount;
        Resolution = note;
        base.Update();
    }

    public void Reject(string reason)
    {
        if (Status is MedicalClaimStatus.Paid)
            throw new InvalidOperationException("A paid claim cannot be rejected.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A rejection reason is required.", nameof(reason));
        Status = MedicalClaimStatus.Rejected;
        Resolution = reason.Trim();
        base.Update();
    }

    /// <summary>Approved → Paid; the reference is the finance/CBS hand-off token (HC245).</summary>
    public void MarkPaid(DateTime paidAt, string? reference)
    {
        if (Status != MedicalClaimStatus.Approved)
            throw new InvalidOperationException("Only an approved claim can be marked paid.");
        Status = MedicalClaimStatus.Paid;
        PaidAt = paidAt;
        PaymentReference = reference;
        base.Update();
    }

    public void AttachViaAggregate(MedicalClaimAttachment attachment) => _attachments.Add(attachment);
}

/// <summary>An inline document attached to a medical claim (HC241) — stored self-contained like EmployeeDocument.</summary>
public class MedicalClaimAttachment : BaseEntity
{
    public Guid MedicalClaimId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long FileSize { get; private set; }
    public byte[] Content { get; private set; } = [];

    private MedicalClaimAttachment() : base() { }

    public static MedicalClaimAttachment Create(Guid claimId, string fileName, string? contentType, byte[] content)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (content is null || content.Length == 0) throw new ArgumentException("File content is required.", nameof(content));
        return new MedicalClaimAttachment
        {
            MedicalClaimId = claimId,
            FileName = fileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = content.Length,
            Content = content
        };
    }
}
