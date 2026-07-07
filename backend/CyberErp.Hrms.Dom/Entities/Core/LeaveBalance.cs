using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Attendance &amp; Leave (HC033): an employee's leave entitlement summary for a leave year and type.
/// This is the fast-read aggregate; every change is also written to <see cref="LeaveBalanceTransaction"/>
/// (an append-only ledger) so the balance is fully auditable and reversible.
/// Available = Entitled + CarriedForward + Adjusted − Taken.
/// </summary>
public class LeaveBalance : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    /// <summary>Balances are kept per fiscal year, not per calendar year.</summary>
    public Guid FiscalYearId { get; private set; }

    public decimal Entitled { get; private set; }
    public decimal CarriedForward { get; private set; }
    public decimal Adjusted { get; private set; }
    public decimal Taken { get; private set; }

    private LeaveType? _leaveType;
    public LeaveType? LeaveType => _leaveType;
    private FiscalYear? _fiscalYear;
    public FiscalYear? FiscalYear => _fiscalYear;

    public decimal Available => Entitled + CarriedForward + Adjusted - Taken;

    private LeaveBalance() : base() { }

    public static LeaveBalance Create(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, decimal entitled = 0)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (leaveTypeId == Guid.Empty) throw new ArgumentException("Leave type is required.", nameof(leaveTypeId));
        if (fiscalYearId == Guid.Empty) throw new ArgumentException("Fiscal year is required.", nameof(fiscalYearId));
        return new LeaveBalance
        {
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            FiscalYearId = fiscalYearId,
            Entitled = entitled < 0 ? 0 : entitled
        };
    }

    /// <summary>Credit carried-forward days (used by the fiscal-year rollover).</summary>
    public void AddCarryForward(decimal days)
    {
        if (days <= 0) throw new ArgumentException("Days must be positive.", nameof(days));
        CarriedForward += days;
        base.Update();
    }

    /// <summary>Debit the balance when leave is approved.</summary>
    public void RecordTaken(decimal days)
    {
        if (days <= 0) throw new ArgumentException("Days must be positive.", nameof(days));
        Taken += days;
        base.Update();
    }

    /// <summary>Credit the balance back when an approved leave is cancelled.</summary>
    public void ReverseTaken(decimal days)
    {
        if (days <= 0) throw new ArgumentException("Days must be positive.", nameof(days));
        Taken -= days;
        if (Taken < 0) Taken = 0;
        base.Update();
    }

    /// <summary>Set the opening figures (HC033) — entitlement, prior-year carry-forward and manual adjustment.</summary>
    public void SetOpening(decimal entitled, decimal carriedForward, decimal adjusted)
    {
        if (entitled < 0) throw new ArgumentException("Entitled cannot be negative.", nameof(entitled));
        if (carriedForward < 0) throw new ArgumentException("Carried-forward cannot be negative.", nameof(carriedForward));
        Entitled = entitled;
        CarriedForward = carriedForward;
        Adjusted = adjusted;
        base.Update();
    }
}
