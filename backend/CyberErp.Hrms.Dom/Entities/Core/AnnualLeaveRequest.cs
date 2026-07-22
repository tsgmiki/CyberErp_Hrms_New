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
    Cancelled = 3
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

    private void RequireStatus(AnnualLeaveStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Expected status {expected} but was {Status}.");
    }

    /// <summary>True while the request still holds a ledger debit a cancellation must reverse.</summary>
    public bool HoldsBalance => Status == AnnualLeaveStatus.Approved;
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
