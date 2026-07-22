using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// The tenant's configurable exit-interview questionnaire (HC219) — one active definition whose
/// questions (survey-JSON shape) are SNAPSHOT onto each interview at launch.
/// </summary>
public class ExitQuestionnaire : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>JSON array of {key, text, type Rating|Choice|Text, options[], required}.</summary>
    public string QuestionsJson { get; private set; } = "[]";

    private ExitQuestionnaire() : base() { }

    public static ExitQuestionnaire Create(string questionsJson)
    {
        Guard(questionsJson);
        return new ExitQuestionnaire { QuestionsJson = questionsJson };
    }

    public void Update(string questionsJson)
    {
        Guard(questionsJson);
        QuestionsJson = questionsJson;
        base.Update();
    }

    private static void Guard(string questionsJson)
    {
        if (string.IsNullOrWhiteSpace(questionsJson))
            throw new ArgumentException("Questions are required.", nameof(questionsJson));
    }
}

public enum ExitInterviewStatus
{
    Pending = 0,
    Completed = 1
}

/// <summary>
/// One exit interview (HC219), launched against a termination case with the questionnaire snapshot
/// of that moment; completed by the leaver (self-service) or recorded by HR.
/// </summary>
public class ExitInterview : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TerminationId { get; private set; }
    public string QuestionsJson { get; private set; } = "[]";
    public string? AnswersJson { get; private set; }
    public ExitInterviewStatus Status { get; private set; } = ExitInterviewStatus.Pending;
    public DateTime? CompletedOn { get; private set; }
    /// <summary>Who recorded the answers — the leaver themselves or an HR interviewer (snapshot, no FK).</summary>
    public Guid? RecordedByEmployeeId { get; private set; }

    private ExitInterview() : base() { }

    public static ExitInterview Create(Guid terminationId, string questionsJson)
    {
        if (terminationId == Guid.Empty)
            throw new ArgumentException("A termination case is required.", nameof(terminationId));
        if (string.IsNullOrWhiteSpace(questionsJson))
            throw new ArgumentException("Questions are required.", nameof(questionsJson));
        return new ExitInterview
        {
            TerminationId = terminationId,
            QuestionsJson = questionsJson
        };
    }

    public void Complete(string answersJson, Guid? recordedByEmployeeId, DateTime completedOn)
    {
        if (Status == ExitInterviewStatus.Completed)
            throw new InvalidOperationException("The interview is already completed.");
        if (string.IsNullOrWhiteSpace(answersJson))
            throw new ArgumentException("Answers are required.", nameof(answersJson));
        AnswersJson = answersJson;
        RecordedByEmployeeId = recordedByEmployeeId;
        Status = ExitInterviewStatus.Completed;
        CompletedOn = completedOn;
        base.Update();
    }
}

/// <summary>Hand-off pipeline of a final settlement (HC216/HC217) — payroll is a future module.</summary>
public enum SettlementStatus
{
    Draft = 0,
    Approved = 1,
    Paid = 2
}

public enum SettlementLineKind
{
    Earning = 0,
    Deduction = 1
}

/// <summary>
/// The final-settlement worksheet of an exit case (HC216): earning lines (severance, retirement
/// benefit, accumulated-leave payout, reimbursements) and deduction lines (loans, advances),
/// auto-suggested then HR-edited while Draft; approval freezes it, payment records the hand-off.
/// </summary>
public class TerminationSettlement : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TerminationId { get; private set; }
    public SettlementStatus Status { get; private set; } = SettlementStatus.Draft;
    public DateTime? ApprovedOn { get; private set; }
    public DateTime? PaidOn { get; private set; }
    /// <summary>Payment reference captured at the payroll/finance hand-off (HC217).</summary>
    public string? PaidReference { get; private set; }
    public string? Notes { get; private set; }

    private TerminationSettlement() : base() { }

    public static TerminationSettlement Create(Guid terminationId, string? notes = null)
    {
        if (terminationId == Guid.Empty)
            throw new ArgumentException("A termination case is required.", nameof(terminationId));
        return new TerminationSettlement { TerminationId = terminationId, Notes = notes };
    }

    public void UpdateNotes(string? notes)
    {
        EnsureDraft();
        Notes = notes;
        base.Update();
    }

    public void Approve(DateTime approvedOn)
    {
        EnsureDraft();
        Status = SettlementStatus.Approved;
        ApprovedOn = approvedOn;
        base.Update();
    }

    public void MarkPaid(DateTime paidOn, string? reference)
    {
        if (Status != SettlementStatus.Approved)
            throw new InvalidOperationException($"Only an approved settlement can be paid (current: {Status}).");
        Status = SettlementStatus.Paid;
        PaidOn = paidOn;
        PaidReference = reference;
        base.Update();
    }

    public void EnsureDraft()
    {
        if (Status != SettlementStatus.Draft)
            throw new InvalidOperationException($"The settlement is {Status} and can no longer change.");
    }
}

/// <summary>One worksheet line (HC216) — auto-suggested or HR-entered.</summary>
public class SettlementLine : BaseEntity
{
    public Guid TerminationSettlementId { get; private set; }
    public SettlementLineKind Kind { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    /// <summary>True for system-suggested lines (leave payout, severance) — HR may still edit them.</summary>
    public bool IsAutoSuggested { get; private set; }
    public int SortOrder { get; private set; }

    private SettlementLine() : base() { }

    public static SettlementLine Create(Guid settlementId, SettlementLineKind kind, string label,
        decimal amount, bool isAutoSuggested, int sortOrder)
    {
        if (settlementId == Guid.Empty)
            throw new ArgumentException("A settlement is required.", nameof(settlementId));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("A line label is required.", nameof(label));
        if (amount < 0)
            throw new ArgumentException("Line amounts are positive; use the Deduction kind to subtract.", nameof(amount));
        return new SettlementLine
        {
            TerminationSettlementId = settlementId,
            Kind = kind,
            Label = label,
            Amount = amount,
            IsAutoSuggested = isAutoSuggested,
            SortOrder = sortOrder
        };
    }
}
