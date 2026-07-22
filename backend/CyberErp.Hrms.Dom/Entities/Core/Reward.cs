using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Groups awards under a named category with shared eligibility criteria (HC178), e.g.
/// "Service Excellence", "Innovation". Badges optionally reference a category.
/// </summary>
public class AwardCategory : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Eligibility criteria applying to every award in the category.</summary>
    public string? Criteria { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private AwardCategory() : base() { }

    public static AwardCategory Create(string name, string? description, string? criteria,
        bool isActive = true, int sortOrder = 0)
    {
        Guard(name);
        return new AwardCategory
        {
            Name = name,
            Description = description,
            Criteria = criteria,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }

    public void Update(string name, string? description, string? criteria, bool isActive, int sortOrder)
    {
        Guard(name);
        Name = name;
        Description = description;
        Criteria = criteria;
        IsActive = isActive;
        SortOrder = sortOrder;
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
    }
}

/// <summary>Cadence a recognition program runs on (HC182).</summary>
public enum RecognitionProgramPeriod
{
    Monthly = 0,
    Quarterly = 1,
    Annual = 2,
    AdHoc = 3
}

/// <summary>
/// A recurring recognition program (HC182), e.g. "Employee of the Month", "Annual Innovation Award".
/// Nominations optionally reference the program they were raised under.
/// </summary>
public class RecognitionProgram : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public RecognitionProgramPeriod Period { get; private set; } = RecognitionProgramPeriod.AdHoc;
    /// <summary>Badge the program awards; null = chosen per nomination.</summary>
    public Guid? RecognitionBadgeId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private RecognitionProgram() : base() { }

    public static RecognitionProgram Create(string name, string? description, RecognitionProgramPeriod period,
        Guid? recognitionBadgeId, bool isActive = true)
    {
        Guard(name);
        return new RecognitionProgram
        {
            Name = name,
            Description = description,
            Period = period,
            RecognitionBadgeId = recognitionBadgeId,
            IsActive = isActive
        };
    }

    public void Update(string name, string? description, RecognitionProgramPeriod period,
        Guid? recognitionBadgeId, bool isActive)
    {
        Guard(name);
        Name = name;
        Description = description;
        Period = period;
        RecognitionBadgeId = recognitionBadgeId;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Program name cannot be empty.", nameof(name));
    }
}

/// <summary>Lifecycle of a reward nomination (HC179).</summary>
public enum NominationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// A manager/HR nomination of an employee for an award (HC179), routed through the generic workflow
/// engine (HC186). Approval grants the recognition (plus points and any monetary disbursement);
/// rejection closes the nomination without a grant.
/// </summary>
public class RewardNomination : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid NomineeEmployeeId { get; private set; }
    public Guid RecognitionBadgeId { get; private set; }
    public Guid? RecognitionProgramId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public NominationStatus Status { get; private set; } = NominationStatus.Pending;
    /// <summary>Who raised the nomination — audit snapshot, intentionally no FK.</summary>
    public Guid? NominatedByEmployeeId { get; private set; }
    public DateTime NominatedOn { get; private set; }
    /// <summary>The recognition created when the nomination was approved (idempotency anchor).</summary>
    public Guid? GrantedRecognitionId { get; private set; }
    public DateTime? DecidedOn { get; private set; }

    private RewardNomination() : base() { }

    public static RewardNomination Create(Guid nomineeEmployeeId, Guid recognitionBadgeId,
        Guid? recognitionProgramId, string reason, Guid? nominatedByEmployeeId, DateTime nominatedOn)
    {
        Guard(nomineeEmployeeId, recognitionBadgeId, reason);
        return new RewardNomination
        {
            NomineeEmployeeId = nomineeEmployeeId,
            RecognitionBadgeId = recognitionBadgeId,
            RecognitionProgramId = recognitionProgramId,
            Reason = reason,
            NominatedByEmployeeId = nominatedByEmployeeId,
            NominatedOn = nominatedOn
        };
    }

    /// <summary>Editable only while awaiting a decision.</summary>
    public void UpdateRequest(Guid nomineeEmployeeId, Guid recognitionBadgeId, Guid? recognitionProgramId, string reason)
    {
        Guard(nomineeEmployeeId, recognitionBadgeId, reason);
        EnsurePending();
        NomineeEmployeeId = nomineeEmployeeId;
        RecognitionBadgeId = recognitionBadgeId;
        RecognitionProgramId = recognitionProgramId;
        Reason = reason;
        base.Update();
    }

    public void MarkApproved(Guid grantedRecognitionId, DateTime decidedOn)
    {
        EnsurePending();
        Status = NominationStatus.Approved;
        GrantedRecognitionId = grantedRecognitionId;
        DecidedOn = decidedOn;
        base.Update();
    }

    public void MarkRejected(DateTime decidedOn)
    {
        EnsurePending();
        Status = NominationStatus.Rejected;
        DecidedOn = decidedOn;
        base.Update();
    }

    private void EnsurePending()
    {
        if (Status != NominationStatus.Pending)
            throw new InvalidOperationException($"Only a pending nomination can change state (current: {Status}).");
    }

    private static void Guard(Guid nomineeEmployeeId, Guid recognitionBadgeId, string reason)
    {
        if (nomineeEmployeeId == Guid.Empty)
            throw new ArgumentException("A nominee is required.", nameof(nomineeEmployeeId));
        if (recognitionBadgeId == Guid.Empty)
            throw new ArgumentException("An award is required.", nameof(recognitionBadgeId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A nomination reason is required.", nameof(reason));
    }
}

/// <summary>Why points moved on the ledger (HC180; Engagement = gamification, HC209).</summary>
public enum RewardPointsSource
{
    Recognition = 0,
    Redemption = 1,
    Adjustment = 2,
    Engagement = 3
}

/// <summary>
/// One immutable entry on an employee's reward-points ledger (HC180). Positive = earned (a granted
/// recognition), negative = redeemed or a correcting adjustment. Balance = SUM over the ledger.
/// </summary>
public class RewardPointsTransaction : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public int Points { get; private set; }
    public RewardPointsSource Source { get; private set; }
    /// <summary>The recognition / appraisal / redemption request that produced the entry.</summary>
    public Guid? ReferenceId { get; private set; }
    public string? Note { get; private set; }
    public DateTime TransactionDate { get; private set; }

    private RewardPointsTransaction() : base() { }

    public static RewardPointsTransaction Create(Guid employeeId, int points, RewardPointsSource source,
        DateTime transactionDate, Guid? referenceId = null, string? note = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (points == 0)
            throw new ArgumentException("A points entry cannot be zero.", nameof(points));
        return new RewardPointsTransaction
        {
            EmployeeId = employeeId,
            Points = points,
            Source = source,
            TransactionDate = transactionDate,
            ReferenceId = referenceId,
            Note = note
        };
    }
}

/// <summary>Payment state of a monetary reward hand-off row (HC185).</summary>
public enum DisbursementStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2
}

/// <summary>
/// The payroll/finance hand-off for a monetary reward (HC185): one row per granted gift-card or
/// monetary-bonus recognition. There is no payroll module — finance marks rows paid (or exports them)
/// from the admin list.
/// </summary>
public class RewardDisbursement : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid? RecognitionBadgeId { get; private set; }
    /// <summary>The recognition grant the payment fulfils.</summary>
    public Guid? EmployeeRecognitionId { get; private set; }
    public decimal Amount { get; private set; }
    public DisbursementStatus Status { get; private set; } = DisbursementStatus.Pending;
    public DateTime? PaidAt { get; private set; }
    /// <summary>Payment reference (cheque / transfer number) captured when marked paid.</summary>
    public string? Reference { get; private set; }

    private RewardDisbursement() : base() { }

    public static RewardDisbursement Create(Guid employeeId, decimal amount, Guid? recognitionBadgeId,
        Guid? employeeRecognitionId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (amount <= 0)
            throw new ArgumentException("Disbursement amount must be positive.", nameof(amount));
        return new RewardDisbursement
        {
            EmployeeId = employeeId,
            Amount = amount,
            RecognitionBadgeId = recognitionBadgeId,
            EmployeeRecognitionId = employeeRecognitionId
        };
    }

    public void MarkPaid(DateTime paidAt, string? reference)
    {
        if (Status != DisbursementStatus.Pending)
            throw new InvalidOperationException($"Only a pending disbursement can be paid (current: {Status}).");
        Status = DisbursementStatus.Paid;
        PaidAt = paidAt;
        Reference = reference;
        base.Update();
    }

    public void Cancel()
    {
        if (Status != DisbursementStatus.Pending)
            throw new InvalidOperationException($"Only a pending disbursement can be cancelled (current: {Status}).");
        Status = DisbursementStatus.Cancelled;
        base.Update();
    }
}
