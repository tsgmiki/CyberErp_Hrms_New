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
/// Attendance &amp; Leave (HC034–HC039): an employee's request for leave over a date range. Routed
/// through the generic workflow engine for approval; on approval the balance ledger is debited.
/// </summary>
public class LeaveRequest : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    /// <summary>The fiscal year the request is charged against (resolved from the start date).</summary>
    public Guid FiscalYearId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public LeaveDayPart DayPart { get; private set; } = LeaveDayPart.Full;
    /// <summary>Chargeable working days (weekends/holidays excluded), snapshotted at submission.</summary>
    public decimal WorkingDays { get; private set; }
    public string? Reason { get; private set; }
    public LeaveRequestStatus Status { get; private set; } = LeaveRequestStatus.Pending;
    public string? DecisionComment { get; private set; }
    public string? CancelReason { get; private set; }

    private Employee? _employee;
    public Employee? Employee => _employee;
    private LeaveType? _leaveType;
    public LeaveType? LeaveType => _leaveType;

    private LeaveRequest() : base() { }

    public static LeaveRequest Create(
        Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, DateTime startDate, DateTime endDate,
        LeaveDayPart dayPart, decimal workingDays, string? reason)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (leaveTypeId == Guid.Empty)
            throw new ArgumentException("Leave type is required.", nameof(leaveTypeId));
        if (fiscalYearId == Guid.Empty)
            throw new ArgumentException("Fiscal year is required.", nameof(fiscalYearId));
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        if (dayPart != LeaveDayPart.Full && startDate.Date != endDate.Date)
            throw new ArgumentException("Half-day leave must be a single day.", nameof(dayPart));
        if (workingDays <= 0)
            throw new ArgumentException("A leave request must span at least one working day.", nameof(workingDays));

        return new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            FiscalYearId = fiscalYearId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            DayPart = dayPart,
            WorkingDays = workingDays,
            Reason = reason,
            Status = LeaveRequestStatus.Pending
        };
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
