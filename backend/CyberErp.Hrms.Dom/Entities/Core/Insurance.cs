using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Kind of group insurance an employer holds (HC247).</summary>
public enum InsuranceType
{
    Life = 0,
    Health = 1,
    Disability = 2,
    Accident = 3,
    WorkersCompensation = 4,
    Other = 5
}

/// <summary>Lifecycle of an insurance policy (HC247).</summary>
public enum InsurancePolicyStatus
{
    Active = 0,
    Expired = 1,
    Renewed = 2,
    Cancelled = 3
}

/// <summary>How the annual premium is broken into scheduled payments.</summary>
public enum PremiumFrequency
{
    Annual = 0,
    SemiAnnual = 1,
    Quarterly = 2,
    Monthly = 3
}

/// <summary>Payment state of one scheduled premium installment (HC250 finance hand-off).</summary>
public enum PremiumPaymentStatus
{
    Pending = 0,
    Paid = 1
}

/// <summary>
/// HC247 — an employer group insurance policy (life, health, disability…) for a policy year, with its
/// insurer, coverage, annual premium and the scheduled premium payments. New policies or renewals both
/// record an annual premium schedule; renewals link back to the prior policy.
/// </summary>
public class InsurancePolicy : BaseEntity, IAggregateRoot, IAuditable
{
    public string PolicyNumber { get; private set; } = string.Empty;
    public string InsurerName { get; private set; } = string.Empty;
    public InsuranceType InsuranceType { get; private set; }
    public string? Coverage { get; private set; }
    /// <summary>Sum insured / coverage ceiling.</summary>
    public decimal CoverageAmount { get; private set; }
    public int PolicyYear { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal AnnualPremium { get; private set; }
    public PremiumFrequency PremiumFrequency { get; private set; } = PremiumFrequency.Annual;
    public InsurancePolicyStatus Status { get; private set; } = InsurancePolicyStatus.Active;
    public bool IsRenewal { get; private set; }
    /// <summary>Prior policy this one renews (no FK — a soft renewal chain).</summary>
    public Guid? PreviousPolicyId { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<InsurancePremiumSchedule> _premiumSchedule = [];
    public IReadOnlyCollection<InsurancePremiumSchedule> PremiumSchedule => _premiumSchedule;

    private InsurancePolicy() : base() { }

    public static InsurancePolicy Create(string policyNumber, string insurerName, InsuranceType type,
        string? coverage, decimal coverageAmount, int policyYear, DateTime startDate, DateTime endDate,
        decimal annualPremium, PremiumFrequency frequency, bool isRenewal, Guid? previousPolicyId, string? notes)
    {
        Guard(policyNumber, insurerName, startDate, endDate, coverageAmount, annualPremium);
        return new InsurancePolicy
        {
            PolicyNumber = policyNumber.Trim(),
            InsurerName = insurerName.Trim(),
            InsuranceType = type,
            Coverage = coverage,
            CoverageAmount = coverageAmount,
            PolicyYear = policyYear,
            StartDate = startDate,
            EndDate = endDate,
            AnnualPremium = annualPremium,
            PremiumFrequency = frequency,
            Status = InsurancePolicyStatus.Active,
            IsRenewal = isRenewal,
            PreviousPolicyId = previousPolicyId,
            Notes = notes
        };
    }

    public void Update(string policyNumber, string insurerName, InsuranceType type, string? coverage,
        decimal coverageAmount, int policyYear, DateTime startDate, DateTime endDate, decimal annualPremium,
        PremiumFrequency frequency, string? notes)
    {
        Guard(policyNumber, insurerName, startDate, endDate, coverageAmount, annualPremium);
        PolicyNumber = policyNumber.Trim();
        InsurerName = insurerName.Trim();
        InsuranceType = type;
        Coverage = coverage;
        CoverageAmount = coverageAmount;
        PolicyYear = policyYear;
        StartDate = startDate;
        EndDate = endDate;
        AnnualPremium = annualPremium;
        PremiumFrequency = frequency;
        Notes = notes;
        base.Update();
    }

    public void SetStatus(InsurancePolicyStatus status)
    {
        Status = status;
        base.Update();
    }

    private static void Guard(string policyNumber, string insurerName, DateTime startDate, DateTime endDate,
        decimal coverageAmount, decimal annualPremium)
    {
        if (string.IsNullOrWhiteSpace(policyNumber)) throw new ArgumentException("Policy number is required.", nameof(policyNumber));
        if (string.IsNullOrWhiteSpace(insurerName)) throw new ArgumentException("Insurer name is required.", nameof(insurerName));
        if (endDate < startDate) throw new ArgumentException("End date cannot precede the start date.", nameof(endDate));
        if (coverageAmount < 0) throw new ArgumentException("Coverage amount cannot be negative.", nameof(coverageAmount));
        if (annualPremium < 0) throw new ArgumentException("Annual premium cannot be negative.", nameof(annualPremium));
    }
}

/// <summary>
/// HC247/HC250 — one scheduled premium payment for an <see cref="InsurancePolicy"/>. Marking it paid is
/// the finance/core-banking hand-off point (staged with a reference; live payment integration deferred).
/// </summary>
public class InsurancePremiumSchedule : BaseEntity
{
    public Guid InsurancePolicyId { get; private set; }
    public int Installment { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public PremiumPaymentStatus Status { get; private set; } = PremiumPaymentStatus.Pending;
    public DateTime? PaidAt { get; private set; }
    public string? PaymentReference { get; private set; }

    private InsurancePremiumSchedule() : base() { }

    public static InsurancePremiumSchedule Create(Guid policyId, int installment, DateTime dueDate, decimal amount)
    {
        if (policyId == Guid.Empty) throw new ArgumentException("Policy is required.", nameof(policyId));
        if (amount < 0) throw new ArgumentException("Premium amount cannot be negative.", nameof(amount));
        return new InsurancePremiumSchedule
        {
            InsurancePolicyId = policyId,
            Installment = installment,
            DueDate = dueDate,
            Amount = amount,
            Status = PremiumPaymentStatus.Pending
        };
    }

    public void MarkPaid(DateTime paidAt, string? reference)
    {
        if (Status != PremiumPaymentStatus.Pending)
            throw new InvalidOperationException("Only a pending premium can be marked paid.");
        Status = PremiumPaymentStatus.Paid;
        PaidAt = paidAt;
        PaymentReference = reference;
        base.Update();
    }
}

/// <summary>Lifecycle of an insurance coverage claim (HC249 tracking).</summary>
public enum InsuranceClaimStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Paid = 4
}

/// <summary>
/// HC248/HC249 — an employee's insurance coverage claim against a policy, with attached certificates and
/// documents. Tracked Pending → (UnderReview) → Approved/Rejected → Paid; the paid step is the finance/CBS
/// hand-off (reference captured, actual insurer payment external).
/// </summary>
public class InsuranceClaim : BaseEntity, IAggregateRoot, IAuditable
{
    public string ClaimNumber { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }
    public Guid InsurancePolicyId { get; private set; }
    public string ClaimType { get; private set; } = string.Empty;
    public DateTime IncidentDate { get; private set; }
    public DateTime SubmittedOn { get; private set; }
    public decimal ClaimedAmount { get; private set; }
    /// <summary>Amount the insurer approves (set at approval; null until approved).</summary>
    public decimal? ApprovedAmount { get; private set; }
    public InsuranceClaimStatus Status { get; private set; } = InsuranceClaimStatus.Pending;
    public string Description { get; private set; } = string.Empty;
    public string? Resolution { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? PaymentReference { get; private set; }

    private readonly List<InsuranceClaimAttachment> _attachments = [];
    public IReadOnlyCollection<InsuranceClaimAttachment> Attachments => _attachments;

    private InsuranceClaim() : base() { }

    public static InsuranceClaim Create(string claimNumber, Guid employeeId, Guid policyId, string claimType,
        DateTime incidentDate, decimal claimedAmount, string description, DateTime submittedOn)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (policyId == Guid.Empty) throw new ArgumentException("Policy is required.", nameof(policyId));
        if (string.IsNullOrWhiteSpace(claimType)) throw new ArgumentException("Claim type is required.", nameof(claimType));
        if (claimedAmount <= 0) throw new ArgumentException("Claimed amount must be positive.", nameof(claimedAmount));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        return new InsuranceClaim
        {
            ClaimNumber = claimNumber,
            EmployeeId = employeeId,
            InsurancePolicyId = policyId,
            ClaimType = claimType.Trim(),
            IncidentDate = incidentDate,
            SubmittedOn = submittedOn,
            ClaimedAmount = claimedAmount,
            Status = InsuranceClaimStatus.Pending,
            Description = description.Trim()
        };
    }

    public void Approve(decimal approvedAmount, string? note)
    {
        if (Status is InsuranceClaimStatus.Paid)
            throw new InvalidOperationException("A paid claim cannot be re-approved.");
        if (approvedAmount < 0 || approvedAmount > ClaimedAmount)
            throw new ArgumentException("Approved amount must be between 0 and the claimed amount.", nameof(approvedAmount));
        Status = InsuranceClaimStatus.Approved;
        ApprovedAmount = approvedAmount;
        Resolution = note;
        base.Update();
    }

    public void Reject(string reason)
    {
        if (Status is InsuranceClaimStatus.Paid)
            throw new InvalidOperationException("A paid claim cannot be rejected.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A rejection reason is required.", nameof(reason));
        Status = InsuranceClaimStatus.Rejected;
        Resolution = reason.Trim();
        base.Update();
    }

    /// <summary>Approved → Paid; the reference is the finance/CBS hand-off token (HC249/HC250).</summary>
    public void MarkPaid(DateTime paidAt, string? reference)
    {
        if (Status != InsuranceClaimStatus.Approved)
            throw new InvalidOperationException("Only an approved claim can be marked paid.");
        Status = InsuranceClaimStatus.Paid;
        PaidAt = paidAt;
        PaymentReference = reference;
        base.Update();
    }

    public void AttachViaAggregate(InsuranceClaimAttachment attachment) => _attachments.Add(attachment);
}

/// <summary>An inline document attached to an insurance claim (HC248) — stored self-contained like EmployeeDocument.</summary>
public class InsuranceClaimAttachment : BaseEntity
{
    public Guid InsuranceClaimId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long FileSize { get; private set; }
    public byte[] Content { get; private set; } = [];

    private InsuranceClaimAttachment() : base() { }

    public static InsuranceClaimAttachment Create(Guid claimId, string fileName, string? contentType, byte[] content)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (content is null || content.Length == 0) throw new ArgumentException("File content is required.", nameof(content));
        return new InsuranceClaimAttachment
        {
            InsuranceClaimId = claimId,
            FileName = fileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = content.Length,
            Content = content
        };
    }
}
