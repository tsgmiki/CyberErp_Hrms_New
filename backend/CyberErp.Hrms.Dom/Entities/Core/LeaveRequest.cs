using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LeaveRequestStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LeaveDayPart
{
    Full = 0,
    FirstHalf = 1,
    SecondHalf = 2
}

/// <summary>
/// Attendance &amp; Leave (HC034–HC039): the HEADER of an employee's leave request. A single request may
/// combine multiple date ranges / leave types as <see cref="LeaveRequestLine"/> detail lines (e.g. a
/// full-day range + a half-day + another full day). The engine costs each line via the working-day
/// calendar and aggregates the total; approval debits the balance ledger per leave type. Routed through
/// the generic workflow engine. All lines are charged to one fiscal year.
/// </summary>
public class LeaveRequest : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    /// <summary>The fiscal year the request is charged against (resolved from the earliest line).</summary>
    public Guid FiscalYearId { get; private set; }
    public DateTime SubmittedDate { get; private set; }
    /// <summary>Chargeable working days summed across all lines, snapshotted at submission.</summary>
    public decimal TotalWorkingDays { get; private set; }
    public string? Reason { get; private set; }
    public LeaveRequestStatus Status { get; private set; } = LeaveRequestStatus.Pending;
    public string? DecisionComment { get; private set; }
    public string? CancelReason { get; private set; }

    private readonly List<LeaveRequestLine> _lines = [];
    public IReadOnlyCollection<LeaveRequestLine> Lines => _lines;

    private Employee? _employee;
    public Employee? Employee => _employee;

    private LeaveRequest() : base() { }

    public static LeaveRequest Create(Guid employeeId, Guid fiscalYearId, DateTime submittedDate, string? reason)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (fiscalYearId == Guid.Empty)
            throw new ArgumentException("Fiscal year is required.", nameof(fiscalYearId));
        return new LeaveRequest
        {
            EmployeeId = employeeId,
            FiscalYearId = fiscalYearId,
            SubmittedDate = submittedDate.Date,
            Reason = reason,
            Status = LeaveRequestStatus.Pending
        };
    }

    /// <summary>Adds a detail line and re-totals the request.</summary>
    public void AddLine(Guid leaveTypeId, DateTime startDate, DateTime endDate, LeaveDayPart dayPart, decimal workingDays)
    {
        _lines.Add(LeaveRequestLine.Create(Id, leaveTypeId, startDate, endDate, dayPart, workingDays));
        TotalWorkingDays = _lines.Sum(l => l.WorkingDays);
    }

    public void Approve(string? comment = null)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException($"Only a pending request can be approved (current: {Status}).");
        Status = LeaveRequestStatus.Approved;
        DecisionComment = comment;
        base.Update();
    }

    public void Reject(string? comment = null)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException($"Only a pending request can be rejected (current: {Status}).");
        Status = LeaveRequestStatus.Rejected;
        DecisionComment = comment;
        base.Update();
    }

    public void Cancel(string? reason = null)
    {
        if (Status is not (LeaveRequestStatus.Pending or LeaveRequestStatus.Approved))
            throw new InvalidOperationException($"Only a pending or approved request can be cancelled (current: {Status}).");
        Status = LeaveRequestStatus.Cancelled;
        CancelReason = reason;
        base.Update();
    }

    /// <summary>True while the request still holds a balance deduction that a cancellation must reverse.</summary>
    public bool HoldsBalance => Status == LeaveRequestStatus.Approved;
}

/// <summary>One detail line of a <see cref="LeaveRequest"/> — a single date range + leave type + day part.</summary>
public class LeaveRequestLine : BaseEntity
{
    public Guid LeaveRequestId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public LeaveDayPart DayPart { get; private set; } = LeaveDayPart.Full;
    /// <summary>Chargeable working days for this line (weekends/holidays excluded, half-days at 0.5).</summary>
    public decimal WorkingDays { get; private set; }

    private LeaveType? _leaveType;
    public LeaveType? LeaveType => _leaveType;

    private LeaveRequestLine() : base() { }

    public static LeaveRequestLine Create(Guid leaveRequestId, Guid leaveTypeId, DateTime startDate,
        DateTime endDate, LeaveDayPart dayPart, decimal workingDays)
    {
        if (leaveTypeId == Guid.Empty)
            throw new ArgumentException("Leave type is required.", nameof(leaveTypeId));
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        if (dayPart != LeaveDayPart.Full && startDate.Date != endDate.Date)
            throw new ArgumentException("Half-day leave must be a single day.", nameof(dayPart));
        if (workingDays <= 0)
            throw new ArgumentException("A leave line must span at least one working day.", nameof(workingDays));
        return new LeaveRequestLine
        {
            LeaveRequestId = leaveRequestId,
            LeaveTypeId = leaveTypeId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            DayPart = dayPart,
            WorkingDays = workingDays
        };
    }
}
