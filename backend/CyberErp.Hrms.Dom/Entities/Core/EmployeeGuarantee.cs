using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a guarantee commitment. Approval states are workflow-owned (HC307).</summary>
public enum GuaranteeCommitmentStatus
{
    /// <summary>In force — the commitment binds the employee.</summary>
    Active = 0,
    /// <summary>Discharged by HR — the external obligation ended.</summary>
    Released = 1,
    /// <summary>Awaiting workflow approval (set on submission when an approval chain is configured).</summary>
    PendingApproval = 2,
    /// <summary>Rejected by the approval workflow; editable and resubmittable.</summary>
    Rejected = 3
}

/// <summary>
/// An employee's guarantee commitment toward an EXTERNAL organization per NBE procedures (§3.12,
/// HC305–HC307): the staff member stands as guarantor for a third party (loan/employment). Recorded
/// by the employee or HR, optionally routed through the generic approval workflow, and released by
/// HR when the underlying obligation is discharged.
/// </summary>
public class EmployeeGuarantee : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    /// <summary>Kind of guarantee — a value of the global "GuaranteeType" lookup category (stored by name).</summary>
    public string Type { get; private set; } = string.Empty;
    /// <summary>The external organization holding the guaranteed obligation (bank, MFI, employer…).</summary>
    public string ExternalOrganization { get; private set; } = string.Empty;
    /// <summary>The third party whose obligation the employee guarantees.</summary>
    public string BeneficiaryName { get; private set; } = string.Empty;
    public string? BeneficiaryRelationship { get; private set; }
    /// <summary>External reference (guarantee letter / contract number).</summary>
    public string? ReferenceNumber { get; private set; }
    /// <summary>Committed amount (the guaranteed exposure).</summary>
    public decimal Amount { get; private set; }
    public DateTime StartDate { get; private set; }
    /// <summary>Expected end of the commitment (null = open-ended until released).</summary>
    public DateTime? EndDate { get; private set; }
    public GuaranteeCommitmentStatus Status { get; private set; } = GuaranteeCommitmentStatus.Active;
    public string? Remarks { get; private set; }
    public DateTime? ReleasedDate { get; private set; }
    public string? ReleaseNote { get; private set; }

    private EmployeeGuarantee() : base() { }

    public static EmployeeGuarantee Create(Guid employeeId, string type,
        string externalOrganization, string beneficiaryName, string? beneficiaryRelationship,
        string? referenceNumber, decimal amount, DateTime startDate, DateTime? endDate, string? remarks)
    {
        Guard(employeeId, type, externalOrganization, beneficiaryName, amount, startDate, endDate);
        return new EmployeeGuarantee
        {
            EmployeeId = employeeId,
            Type = type.Trim(),
            ExternalOrganization = externalOrganization.Trim(),
            BeneficiaryName = beneficiaryName.Trim(),
            BeneficiaryRelationship = beneficiaryRelationship,
            ReferenceNumber = referenceNumber,
            Amount = amount,
            StartDate = startDate.Date,
            EndDate = endDate?.Date,
            Remarks = remarks
        };
    }

    public void Update(string type, string externalOrganization, string beneficiaryName,
        string? beneficiaryRelationship, string? referenceNumber, decimal amount,
        DateTime startDate, DateTime? endDate, string? remarks)
    {
        Guard(EmployeeId, type, externalOrganization, beneficiaryName, amount, startDate, endDate);
        if (Status == GuaranteeCommitmentStatus.Released)
            throw new ArgumentException("A released commitment can no longer be amended.", nameof(type));
        Type = type.Trim();
        ExternalOrganization = externalOrganization.Trim();
        BeneficiaryName = beneficiaryName.Trim();
        BeneficiaryRelationship = beneficiaryRelationship;
        ReferenceNumber = referenceNumber;
        Amount = amount;
        StartDate = startDate.Date;
        EndDate = endDate?.Date;
        Remarks = remarks;
        base.Update();
    }

    /// <summary>Parks the commitment awaiting approval (an active workflow definition governs this tenant).</summary>
    public void MarkPendingApproval()
    {
        Status = GuaranteeCommitmentStatus.PendingApproval;
        base.Update();
    }

    /// <summary>Workflow callback — an approved commitment is in force.</summary>
    public void ApproveViaWorkflow()
    {
        if (Status != GuaranteeCommitmentStatus.PendingApproval) return; // idempotent
        Status = GuaranteeCommitmentStatus.Active;
        base.Update();
    }

    /// <summary>Workflow callback — a rejected commitment stays editable for resubmission.</summary>
    public void RejectViaWorkflow()
    {
        if (Status != GuaranteeCommitmentStatus.PendingApproval) return;
        Status = GuaranteeCommitmentStatus.Rejected;
        base.Update();
    }

    /// <summary>HR discharges the commitment once the external obligation ends.</summary>
    public void Release(string? note, DateTime releasedDate)
    {
        if (Status != GuaranteeCommitmentStatus.Active)
            throw new ArgumentException("Only an active commitment can be released.", nameof(note));
        Status = GuaranteeCommitmentStatus.Released;
        ReleasedDate = releasedDate.Date;
        ReleaseNote = note;
        base.Update();
    }

    private static void Guard(Guid employeeId, string type, string externalOrganization, string beneficiaryName,
        decimal amount, DateTime startDate, DateTime? endDate)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("The guarantee type is required.", nameof(type));
        if (string.IsNullOrWhiteSpace(externalOrganization))
            throw new ArgumentException("The external organization is required.", nameof(externalOrganization));
        if (string.IsNullOrWhiteSpace(beneficiaryName))
            throw new ArgumentException("The beneficiary is required.", nameof(beneficiaryName));
        if (amount <= 0)
            throw new ArgumentException("The committed amount must be positive.", nameof(amount));
        if (endDate.HasValue && endDate.Value.Date <= startDate.Date)
            throw new ArgumentException("The end date must be after the start date.", nameof(endDate));
    }
}
