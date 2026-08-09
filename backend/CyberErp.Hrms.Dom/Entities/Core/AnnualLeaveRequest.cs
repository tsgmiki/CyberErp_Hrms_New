using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnualLeaveUsage
{
    FullDay = 0,
    HalfDay = 1
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnualLeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    /// <summary>The employee returned on a different date than approved; the adjustment is awaiting approval.</summary>
    ReturnPending = 4,
    /// <summary>Return confirmed and settled — the ledger now reflects the days actually taken.</summary>
    Closed = 5
}

/// <summary>How the actual return compared with the approved end date.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnualLeaveReturnType
{
    /// <summary>Returned as approved — the original days stand, no approval needed.</summary>
    OnTime = 0,
    /// <summary>Came back early: fewer days taken, so the balance is due a credit.</summary>
    Early = 1,
    /// <summary>Came back late: extra days taken, which must be requested and approved.</summary>
    Late = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnualLeaveReturnStatus
{
    /// <summary>Early/late adjustment routed back through the approval workflow.</summary>
    PendingApproval = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>Which half of the day a <see cref="AnnualLeaveUsage.HalfDay"/> row covers.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HalfDayPart
{
    Morning = 0,
    Afternoon = 1
}

/// <summary>
/// HEADER of an Annual-Leave request (Master-Detail). Dedicated to <b>annual leave only</b>: the ledger
/// row it references (<see cref="LeaveBalance"/>) already fixes employee + fiscal year + the annual leave
/// type, so this structure intentionally carries <b>no LeaveType field</b>. The actual date ranges live in
/// <see cref="AnnualLeaveDetail"/> rows; approval debits <c>Taken</c> on the referenced ledger row.
/// </summary>
public class AnnualLeaveHeader : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }

    /// <summary>The annual-leave entitlement row this request is charged against (hrms_LeaveBalance).</summary>
    public Guid AnnualLeaveLedgerId { get; private set; }

    public DateTime RequestDate { get; private set; }
    public string? Remark { get; private set; }

    /// <summary>Denormalized Σ of the detail rows' <see cref="AnnualLeaveDetail.LeaveDays"/>, snapshotted at submission.</summary>
    public decimal TotalLeaveDays { get; private set; }

    public AnnualLeaveStatus Status { get; private set; } = AnnualLeaveStatus.Pending;

    /// <summary>
    /// Days actually taken, set when the return is settled. Null until then — <see cref="TotalLeaveDays"/>
    /// stays the APPROVED figure so the two can always be compared, which is what the history view and
    /// the ledger reconciliation need.
    /// </summary>
    public decimal? ActualLeaveDays { get; private set; }

    private readonly List<AnnualLeaveDetail> _details = [];
    public IReadOnlyCollection<AnnualLeaveDetail> Details => _details;

    private Employee? _employee;
    public Employee? Employee => _employee;

    private LeaveBalance? _ledger;
    public LeaveBalance? Ledger => _ledger;

    private AnnualLeaveHeader() : base() { }

    public static AnnualLeaveHeader Create(Guid employeeId, Guid annualLeaveLedgerId, DateTime requestDate, string? remark)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (annualLeaveLedgerId == Guid.Empty)
            throw new ArgumentException("Annual-leave ledger is required.", nameof(annualLeaveLedgerId));
        return new AnnualLeaveHeader
        {
            EmployeeId = employeeId,
            AnnualLeaveLedgerId = annualLeaveLedgerId,
            RequestDate = requestDate.Date,
            Remark = remark,
            Status = AnnualLeaveStatus.Pending
        };
    }

    /// <summary>Adds a detail row and re-totals the request.</summary>
    public void AddDetail(AnnualLeaveUsage usage, DateTime startDate, DateTime endDate, decimal leaveDays, HalfDayPart? halfDayPart = null)
    {
        _details.Add(AnnualLeaveDetail.Create(Id, usage, startDate, endDate, leaveDays, halfDayPart));
        TotalLeaveDays = _details.Sum(d => d.LeaveDays);
    }

    public void Approve()
    {
        RequireStatus(AnnualLeaveStatus.Pending);
        Status = AnnualLeaveStatus.Approved;
        base.Update();
    }

    public void Reject()
    {
        RequireStatus(AnnualLeaveStatus.Pending);
        Status = AnnualLeaveStatus.Rejected;
        base.Update();
    }

    public void Cancel()
    {
        if (Status is not (AnnualLeaveStatus.Pending or AnnualLeaveStatus.Approved))
            throw new InvalidOperationException($"Only a pending or approved request can be cancelled (current: {Status}).");
        Status = AnnualLeaveStatus.Cancelled;
        base.Update();
    }

    // ---- Return confirmation -------------------------------------------------

    /// <summary>
    /// The employee came back exactly as approved: nothing to approve and nothing to adjust, because
    /// the ledger was already debited for these days when the request was approved.
    /// </summary>
    public void CloseOnTimeReturn(decimal actualDays)
    {
        RequireStatus(AnnualLeaveStatus.Approved);
        ActualLeaveDays = actualDays;
        Status = AnnualLeaveStatus.Closed;
        base.Update();
    }

    /// <summary>An early or late return is parked here while its adjustment goes for approval.</summary>
    public void BeginReturnAdjustment()
    {
        RequireStatus(AnnualLeaveStatus.Approved);
        Status = AnnualLeaveStatus.ReturnPending;
        base.Update();
    }

    /// <summary>
    /// The adjustment was approved. <paramref name="actualDays"/> becomes the days actually taken; the
    /// caller has already moved the ledger by the difference.
    /// </summary>
    public void SettleReturn(decimal actualDays)
    {
        RequireStatus(AnnualLeaveStatus.ReturnPending);
        ActualLeaveDays = actualDays;
        Status = AnnualLeaveStatus.Closed;
        base.Update();
    }

    /// <summary>
    /// The adjustment was rejected, so the request goes back to Approved and the employee can confirm
    /// again. The ledger is untouched throughout — it only ever moves on an approved decision.
    /// </summary>
    public void RejectReturn()
    {
        RequireStatus(AnnualLeaveStatus.ReturnPending);
        Status = AnnualLeaveStatus.Approved;
        base.Update();
    }

    private void RequireStatus(AnnualLeaveStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Expected status {expected} but was {Status}.");
    }

    /// <summary>
    /// True while the request still holds a ledger debit a cancellation must reverse. A closed request
    /// still holds one — the days were taken — but cancelling is no longer offered for it.
    /// </summary>
    public bool HoldsBalance => Status is AnnualLeaveStatus.Approved or AnnualLeaveStatus.ReturnPending
        or AnnualLeaveStatus.Closed;

    /// <summary>The employee can confirm their return only once the request is approved and still open.</summary>
    public bool CanConfirmReturn => Status == AnnualLeaveStatus.Approved;
}

/// <summary>
/// The employee's confirmation that they are back, and the record of any difference from what was
/// approved. One row per confirmation attempt, so a rejected adjustment leaves its history behind
/// rather than being overwritten by the next attempt.
/// </summary>
public class AnnualLeaveReturn : BaseEntity, IAuditable
{
    public Guid AnnualLeaveHeaderId { get; private set; }

    /// <summary>Last day of leave as APPROVED — kept so the comparison survives later edits.</summary>
    public DateTime PlannedEndDate { get; private set; }

    /// <summary>Last day the employee was actually on leave (the day before they resumed work).</summary>
    public DateTime ActualEndDate { get; private set; }

    /// <summary>Working days originally approved.</summary>
    public decimal ApprovedDays { get; private set; }

    /// <summary>Working days actually taken, recomputed over the real range by the working calendar.</summary>
    public decimal ActualDays { get; private set; }

    /// <summary>
    /// <c>ActualDays - ApprovedDays</c>: negative for an early return (days to credit back), positive
    /// for a late one (extra days to debit), zero on time. Stored rather than derived because it is the
    /// figure an approver signed off on.
    /// </summary>
    public decimal AdjustmentDays { get; private set; }

    public AnnualLeaveReturnType ReturnType { get; private set; }
    public AnnualLeaveReturnStatus Status { get; private set; }

    /// <summary>Why the return differs from what was approved. Required for Early and Late.</summary>
    public string? Comment { get; private set; }

    public DateTime ConfirmedAt { get; private set; }

    private AnnualLeaveReturn() : base() { }

    public static AnnualLeaveReturn Create(
        Guid annualLeaveHeaderId, DateTime plannedEndDate, DateTime actualEndDate,
        decimal approvedDays, decimal actualDays, string? comment)
    {
        if (annualLeaveHeaderId == Guid.Empty)
            throw new ArgumentException("Annual leave request is required.", nameof(annualLeaveHeaderId));
        if (actualDays < 0)
            throw new ArgumentException("Actual days cannot be negative.", nameof(actualDays));

        var adjustment = actualDays - approvedDays;
        var type = adjustment switch
        {
            < 0 => AnnualLeaveReturnType.Early,
            > 0 => AnnualLeaveReturnType.Late,
            _ => AnnualLeaveReturnType.OnTime
        };

        // The comment is what an approver reads to judge the adjustment, so an unexplained one is not
        // acceptable. An on-time return needs no justification — nothing changed.
        if (type != AnnualLeaveReturnType.OnTime && string.IsNullOrWhiteSpace(comment))
            throw new ArgumentException(
                "Explain the difference between the approved and actual return.", nameof(comment));

        return new AnnualLeaveReturn
        {
            AnnualLeaveHeaderId = annualLeaveHeaderId,
            PlannedEndDate = plannedEndDate.Date,
            ActualEndDate = actualEndDate.Date,
            ApprovedDays = approvedDays,
            ActualDays = actualDays,
            AdjustmentDays = adjustment,
            ReturnType = type,
            // On time needs no approval: the ledger already holds exactly these days.
            Status = type == AnnualLeaveReturnType.OnTime
                ? AnnualLeaveReturnStatus.Approved
                : AnnualLeaveReturnStatus.PendingApproval,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            ConfirmedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        RequirePending();
        Status = AnnualLeaveReturnStatus.Approved;
        base.Update();
    }

    public void Reject()
    {
        RequirePending();
        Status = AnnualLeaveReturnStatus.Rejected;
        base.Update();
    }

    private void RequirePending()
    {
        if (Status != AnnualLeaveReturnStatus.PendingApproval)
            throw new InvalidOperationException($"The return is already {Status}.");
    }
}

/// <summary>DETAIL row of an <see cref="AnnualLeaveHeader"/> — one date range (or single half-day).</summary>
public class AnnualLeaveDetail : BaseEntity
{
    public Guid AnnualLeaveHeaderId { get; private set; }
    public AnnualLeaveUsage LeaveUsage { get; private set; } = AnnualLeaveUsage.FullDay;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    /// <summary>Chargeable days for this row (weekends/holidays excluded, half-day counted as 0.5).</summary>
    public decimal LeaveDays { get; private set; }

    /// <summary>Which half of the day — only set (and required) when <see cref="LeaveUsage"/> is HalfDay.</summary>
    public HalfDayPart? HalfDayPart { get; private set; }

    private AnnualLeaveDetail() : base() { }

    public static AnnualLeaveDetail Create(Guid annualLeaveHeaderId, AnnualLeaveUsage usage, DateTime startDate, DateTime endDate, decimal leaveDays, HalfDayPart? halfDayPart = null)
    {
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        if (usage == AnnualLeaveUsage.HalfDay && startDate.Date != endDate.Date)
            throw new ArgumentException("A half day must be a single date.", nameof(usage));
        if (usage == AnnualLeaveUsage.HalfDay && halfDayPart is null)
            throw new ArgumentException("Specify Morning or Afternoon for a half day.", nameof(halfDayPart));
        if (leaveDays <= 0)
            throw new ArgumentException("A detail row must span at least part of a working day.", nameof(leaveDays));
        return new AnnualLeaveDetail
        {
            AnnualLeaveHeaderId = annualLeaveHeaderId,
            LeaveUsage = usage,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            LeaveDays = leaveDays,
            // The morning/afternoon distinction only applies to half days; full days never carry it.
            HalfDayPart = usage == AnnualLeaveUsage.HalfDay ? halfDayPart : null
        };
    }
}
