using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum OfferStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Sent = 3,
    Accepted = 4,
    Declined = 5,
    Withdrawn = 6,
    Expired = 7
}

/// <summary>
/// A formal employment offer for one application (HC111–HC114). Lifecycle:
/// Draft → (Submit → PendingApproval → Approved | back to Draft on rejection) → Sent →
/// Accepted | Declined | Expired; Withdrawn is reachable from any non-final state. At most one
/// ACTIVE offer (Draft/PendingApproval/Approved/Sent) exists per application (filtered unique
/// index). Salary is validated against the requisition's salary scale (HC113) — deviation needs a
/// justification. An accepted offer is the gate the hire conversion honors; hiring stamps
/// <see cref="HiredEmployeeId"/> (no FK — SQL Server cascade-path limit, same as Candidate).
/// </summary>
public class JobOffer : BaseEntity, IAggregateRoot, IAuditable
{
    public string OfferNumber { get; private set; } = string.Empty;
    public Guid ApplicationId { get; private set; }
    /// <summary>SET NULL on employee deletion — the name snapshot survives.</summary>
    public Guid? HiringManagerEmployeeId { get; private set; }
    public string? HiringManagerName { get; private set; }
    /// <summary>Offered monthly salary (ETB).</summary>
    public decimal Salary { get; private set; }
    /// <summary>The salary-scale pay point backing the offer (HC113).</summary>
    public Guid? SalaryScaleId { get; private set; }
    /// <summary>Required when the offered salary deviates from the scale amount (HC113).</summary>
    public string? SalaryJustification { get; private set; }
    public DateTime ProposedStartDate { get; private set; }
    /// <summary>The date the offer lapses when unanswered (HC114).</summary>
    public DateTime ExpiryDate { get; private set; }
    public OfferStatus Status { get; private set; } = OfferStatus.Draft;
    public DateTime? SentAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public string? ResponseNote { get; private set; }
    /// <summary>Generated/edited offer letter body (HC111) — frozen once sent.</summary>
    public string? LetterText { get; private set; }
    /// <summary>The employee the acceptance became at hire (no FK — cascade-path limit).</summary>
    public Guid? HiredEmployeeId { get; private set; }

    private JobOffer() : base() { }

    public static JobOffer Create(
        string offerNumber, Guid applicationId, Guid? hiringManagerEmployeeId, string? hiringManagerName,
        decimal salary, Guid? salaryScaleId, string? salaryJustification,
        DateTime proposedStartDate, DateTime expiryDate, string? letterText)
    {
        if (string.IsNullOrWhiteSpace(offerNumber))
            throw new ArgumentException("An offer number is required.", nameof(offerNumber));
        if (applicationId == Guid.Empty)
            throw new ArgumentException("An application is required.", nameof(applicationId));
        EnsureTerms(salary);

        return new JobOffer
        {
            OfferNumber = offerNumber.Trim(),
            ApplicationId = applicationId,
            HiringManagerEmployeeId = hiringManagerEmployeeId,
            HiringManagerName = hiringManagerName,
            Salary = salary,
            SalaryScaleId = salaryScaleId,
            SalaryJustification = salaryJustification,
            ProposedStartDate = proposedStartDate.Date,
            ExpiryDate = expiryDate.Date,
            LetterText = letterText
        };
    }

    /// <summary>Terms are editable while the offer is still a draft.</summary>
    public void UpdateTerms(
        Guid? hiringManagerEmployeeId, string? hiringManagerName,
        decimal salary, Guid? salaryScaleId, string? salaryJustification,
        DateTime proposedStartDate, DateTime expiryDate, string? letterText)
    {
        if (Status != OfferStatus.Draft)
            throw new InvalidOperationException($"A {Status} offer can no longer be edited.");
        EnsureTerms(salary);

        HiringManagerEmployeeId = hiringManagerEmployeeId;
        HiringManagerName = hiringManagerName;
        Salary = salary;
        SalaryScaleId = salaryScaleId;
        SalaryJustification = salaryJustification;
        ProposedStartDate = proposedStartDate.Date;
        ExpiryDate = expiryDate.Date;
        LetterText = letterText;
        base.Update();
    }

    /// <summary>Draft → PendingApproval (offer approval workflow, HC112).</summary>
    public void Submit()
    {
        if (Status != OfferStatus.Draft)
            throw new InvalidOperationException($"A {Status} offer cannot be submitted.");
        Status = OfferStatus.PendingApproval;
        base.Update();
    }

    /// <summary>Workflow outcome: approved and ready to send.</summary>
    public void Approve()
    {
        if (Status != OfferStatus.PendingApproval)
            throw new InvalidOperationException($"A {Status} offer cannot be approved.");
        Status = OfferStatus.Approved;
        base.Update();
    }

    /// <summary>Workflow outcome: rejected — returns to Draft for correction and resubmission.</summary>
    public void RejectToDraft()
    {
        if (Status != OfferStatus.PendingApproval)
            throw new InvalidOperationException($"A {Status} offer cannot be rejected.");
        Status = OfferStatus.Draft;
        base.Update();
    }

    /// <summary>
    /// Attaches the generated standard letter at delivery time when none was drafted.
    /// Allowed until the offer is sent — the letter freezes with the terms afterwards.
    /// </summary>
    public void AttachLetter(string letterText)
    {
        if (Status is OfferStatus.Sent or OfferStatus.Accepted or OfferStatus.Declined
            or OfferStatus.Withdrawn or OfferStatus.Expired)
            throw new InvalidOperationException($"The letter of a {Status} offer is frozen.");
        LetterText = letterText;
        base.Update();
    }

    /// <summary>Approved → Sent: the letter goes to the candidate; terms freeze (HC114 clock starts).</summary>
    public void MarkSent()
    {
        if (Status != OfferStatus.Approved)
            throw new InvalidOperationException($"A {Status} offer cannot be sent.");
        Status = OfferStatus.Sent;
        SentAt = DateTime.UtcNow;
        base.Update();
    }

    public void Accept(string? note)
    {
        if (Status != OfferStatus.Sent)
            throw new InvalidOperationException($"A {Status} offer cannot be accepted.");
        Status = OfferStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
        ResponseNote = note;
        base.Update();
    }

    public void Decline(string? note)
    {
        if (Status != OfferStatus.Sent)
            throw new InvalidOperationException($"A {Status} offer cannot be declined.");
        Status = OfferStatus.Declined;
        RespondedAt = DateTime.UtcNow;
        ResponseNote = note;
        base.Update();
    }

    /// <summary>HR withdraws a live offer (any state before a final response).</summary>
    public void Withdraw(string? note)
    {
        if (Status is OfferStatus.Accepted or OfferStatus.Declined or OfferStatus.Withdrawn or OfferStatus.Expired)
            throw new InvalidOperationException($"A {Status} offer can no longer be withdrawn.");
        Status = OfferStatus.Withdrawn;
        RespondedAt = DateTime.UtcNow;
        ResponseNote = note;
        base.Update();
    }

    /// <summary>A sent offer lapses when its expiry date passes without a response (HC114).</summary>
    public void MarkExpired()
    {
        if (Status != OfferStatus.Sent)
            throw new InvalidOperationException($"A {Status} offer cannot expire.");
        Status = OfferStatus.Expired;
        base.Update();
    }

    /// <summary>Hire conversion stamps the employee the acceptance became.</summary>
    public void AssignHiredEmployee(Guid employeeId)
    {
        if (Status != OfferStatus.Accepted)
            throw new InvalidOperationException("Only an accepted offer can be linked to a hire.");
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        HiredEmployeeId = employeeId;
        base.Update();
    }

    private static void EnsureTerms(decimal salary)
    {
        if (salary <= 0)
            throw new ArgumentException("The offered salary must be greater than zero.", nameof(salary));
    }
}

/// <summary>
/// Per-tenant atomic business-number counter (logic.md §7.1 adoption #5) — replaces race-prone
/// count+1 numbering. NOT a BaseEntity: managed by raw atomic SQL
/// (UPDATE … SET Value += 1 OUTPUT inserted.Value), no audit/rowversion machinery.
/// </summary>
public class NumberSequence
{
    public string TenantId { get; set; } = string.Empty;
    /// <summary>Counter key, e.g. "JobOffer".</summary>
    public string Key { get; set; } = string.Empty;
    public long Value { get; set; }
}
