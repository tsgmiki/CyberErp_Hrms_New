using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of an <see cref="ApprovalDelegation"/>.</summary>
/// <remarks>
/// ⚠️ Scheduled/Active/Expired are DERIVED from the dates, never stored as a flag — see
/// <see cref="ApprovalDelegation.StatusOn"/>. Only <see cref="Revoked"/> is a real transition
/// somebody performs. A status column that needs a nightly sweep to stay true is a status column
/// that is wrong every morning until the sweep runs.
/// </remarks>
public enum DelegationStatus
{
    /// <summary>Dated, but the window has not started yet.</summary>
    Scheduled = 0,
    /// <summary>Within its window and not revoked — the delegate can act.</summary>
    Active = 1,
    /// <summary>Withdrawn before its end date. Terminal.</summary>
    Revoked = 2,
    /// <summary>The window has passed. Terminal.</summary>
    Expired = 3
}

/// <summary>
/// One person's authority to act on another's workflow approval steps, for a bounded period.
/// </summary>
/// <remarks>
/// <para>The enterprise "substitution" record: while effective, the delegate may decide any approval
/// step the delegator could have decided, within the configured scope and money ceiling.</para>
///
/// <para>⚠️ IT GRANTS NOTHING ELSE. No menu permissions, no data visibility, no ability to open
/// records or salaries the delegate could not already see. Approval authority and read access are
/// separate concerns, and a delegation that quietly widened the second would be a
/// privilege-escalation path wearing a business feature's clothes.</para>
///
/// <para>⚠️ AUTHORITY IS NOT RE-DELEGABLE. A delegation covers the delegator's OWN authority only,
/// never authority they themselves hold through another delegation. The resolver does not recurse,
/// so A to B and B to C never yields A to C. Without that rule a chain of individually reasonable
/// delegations moves an executive's approval rights to somebody nobody chose, and no single record
/// in the chain looks wrong.</para>
/// </remarks>
public class ApprovalDelegation : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>The real approver, whose authority is being lent.</summary>
    public Guid FromEmployeeId { get; private set; }
    /// <summary>The stand-in, who may act during the window.</summary>
    public Guid ToEmployeeId { get; private set; }

    /// <summary>First day the delegation is effective, inclusive.</summary>
    public DateTime StartDate { get; private set; }
    /// <summary>Last day the delegation is effective, inclusive.</summary>
    public DateTime EndDate { get; private set; }

    /// <summary>Why — shown to approvers and kept for audit.</summary>
    public string? Reason { get; private set; }

    /// <summary>Cover every workflow process, rather than the named <see cref="Scopes"/>.</summary>
    public bool AllProcesses { get; private set; } = true;

    /// <summary>
    /// Ceiling on what the DELEGATE may approve, in the request's own amount. Null = the delegator's
    /// own authority, uncapped.
    /// </summary>
    /// <remarks>
    /// ⚠️ A request above the ceiling is NOT rejected — it simply stays with the real approver. A
    /// stand-in who cannot sign for the amount is a reason to wait, never a reason to refuse
    /// somebody's loan.
    /// </remarks>
    public decimal? ApprovalLimit { get; private set; }

    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedBy { get; private set; }
    public string? RevocationReason { get; private set; }

    private readonly List<ApprovalDelegationScope> _scopes = [];
    /// <summary>The workflow entity-type keys this covers. Empty when <see cref="AllProcesses"/>.</summary>
    public IReadOnlyCollection<ApprovalDelegationScope> Scopes => _scopes;

    private ApprovalDelegation() : base() { }

    public static ApprovalDelegation Create(
        Guid fromEmployeeId, Guid toEmployeeId, DateTime startDate, DateTime endDate,
        string? reason, bool allProcesses, IEnumerable<string>? entityTypes, decimal? approvalLimit)
    {
        Guard(fromEmployeeId, toEmployeeId, startDate, endDate, approvalLimit);

        var delegation = new ApprovalDelegation
        {
            FromEmployeeId = fromEmployeeId,
            ToEmployeeId = toEmployeeId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Reason = reason,
            AllProcesses = allProcesses,
            ApprovalLimit = approvalLimit
        };
        delegation.SetScopes(entityTypes ?? []);
        return delegation;
    }

    private static void Guard(Guid fromEmployeeId, Guid toEmployeeId,
        DateTime startDate, DateTime endDate, decimal? approvalLimit)
    {
        if (fromEmployeeId == Guid.Empty)
            throw new ArgumentException("The delegating approver is required.", nameof(fromEmployeeId));
        if (toEmployeeId == Guid.Empty)
            throw new ArgumentException("The delegate is required.", nameof(toEmployeeId));
        if (fromEmployeeId == toEmployeeId)
            throw new ArgumentException("An approver cannot delegate to themselves.", nameof(toEmployeeId));
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("The delegation cannot end before it starts.", nameof(endDate));
        if (approvalLimit is < 0)
            throw new ArgumentException("An approval limit cannot be negative.", nameof(approvalLimit));
    }

    /// <summary>Replaces the covered process list. A no-op while <see cref="AllProcesses"/>.</summary>
    public void SetScopes(IEnumerable<string> entityTypes)
    {
        _scopes.Clear();
        if (AllProcesses) return;

        var distinct = entityTypes
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinct.Count == 0)
            throw new ArgumentException(
                "Select at least one process to delegate, or delegate all processes.", nameof(entityTypes));

        foreach (var t in distinct)
            _scopes.Add(ApprovalDelegationScope.Create(Id, t));
    }

    public void Update(DateTime startDate, DateTime endDate, string? reason,
        bool allProcesses, IEnumerable<string>? entityTypes, decimal? approvalLimit)
    {
        if (IsRevoked)
            throw new InvalidOperationException("A revoked delegation can no longer be changed.");
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("The delegation cannot end before it starts.", nameof(endDate));
        if (approvalLimit is < 0)
            throw new ArgumentException("An approval limit cannot be negative.", nameof(approvalLimit));

        StartDate = startDate.Date;
        EndDate = endDate.Date;
        Reason = reason;
        AllProcesses = allProcesses;
        ApprovalLimit = approvalLimit;
        SetScopes(entityTypes ?? []);
        base.Update();
    }

    /// <summary>Withdraw the delegation. Terminal, and deliberately not undoable.</summary>
    public void Revoke(string? revokedBy, string? reason)
    {
        if (IsRevoked) return;
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        RevocationReason = reason;
        base.Update();
    }

    /// <summary>Is this delegation in force on <paramref name="onDate"/>?</summary>
    public bool IsEffectiveOn(DateTime onDate)
    {
        if (IsRevoked) return false;
        var d = onDate.Date;
        return d >= StartDate && d <= EndDate;
    }

    /// <summary>Does this delegation cover <paramref name="entityType"/>?</summary>
    public bool Covers(string entityType) =>
        AllProcesses || _scopes.Any(s => string.Equals(s.EntityType, entityType, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Can the delegate sign for <paramref name="amount"/>? A request whose amount is unknown
    /// (null) is treated as within the ceiling — only a KNOWN amount above the limit holds it back.
    /// </summary>
    public bool CoversAmount(decimal? amount) =>
        ApprovalLimit is not decimal limit || amount is not decimal value || value <= limit;

    /// <summary>Derived status, from the dates and the revocation flag.</summary>
    public DelegationStatus StatusOn(DateTime onDate)
    {
        if (IsRevoked) return DelegationStatus.Revoked;
        var d = onDate.Date;
        if (d < StartDate) return DelegationStatus.Scheduled;
        return d > EndDate ? DelegationStatus.Expired : DelegationStatus.Active;
    }
}

/// <summary>One workflow process covered by an <see cref="ApprovalDelegation"/>.</summary>
public class ApprovalDelegationScope : BaseEntity
{
    public Guid DelegationId { get; private set; }
    /// <summary>A <see cref="WorkflowEntityTypes"/> key, e.g. "EmployeeLoan".</summary>
    public string EntityType { get; private set; } = string.Empty;

    private ApprovalDelegationScope() : base() { }

    public static ApprovalDelegationScope Create(Guid delegationId, string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Process key is required.", nameof(entityType));
        return new ApprovalDelegationScope { DelegationId = delegationId, EntityType = entityType.Trim() };
    }
}
