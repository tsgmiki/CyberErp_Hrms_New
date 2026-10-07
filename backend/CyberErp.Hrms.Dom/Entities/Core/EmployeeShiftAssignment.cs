using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Attendance (HC041): which shift an employee works, and from when.
/// </summary>
/// <remarks>
/// <para>⚠️ DATED, not a column on the employee. People move between shifts, and attendance for
/// LAST month must still be read against the shift they were on THEN. Storing the current shift on
/// the employee would silently rewrite history every time somebody is moved to nights.</para>
///
/// <para>⚠️ <see cref="EffectiveTo"/> null means "still on it". Overlap is rejected by the handler,
/// which is the only place that can see an employee's other assignments.</para>
/// </remarks>
public class EmployeeShiftAssignment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid WorkShiftId { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public string? Remark { get; private set; }

    public Employee? Employee { get; private set; }
    public WorkShift? WorkShift { get; private set; }

    private EmployeeShiftAssignment() : base() { }

    public static EmployeeShiftAssignment Create(
        Guid employeeId, Guid workShiftId, DateTime effectiveFrom,
        DateTime? effectiveTo = null, string? remark = null)
    {
        Guard(employeeId, workShiftId, effectiveFrom, effectiveTo);
        return new EmployeeShiftAssignment
        {
            EmployeeId = employeeId,
            WorkShiftId = workShiftId,
            EffectiveFrom = effectiveFrom.Date,
            EffectiveTo = effectiveTo?.Date,
            Remark = remark
        };
    }

    public void Update(Guid workShiftId, DateTime effectiveFrom, DateTime? effectiveTo, string? remark)
    {
        Guard(EmployeeId, workShiftId, effectiveFrom, effectiveTo);
        WorkShiftId = workShiftId;
        EffectiveFrom = effectiveFrom.Date;
        EffectiveTo = effectiveTo?.Date;
        Remark = remark;
        base.Update();
    }

    /// <summary>Closes an open assignment the day before a new one begins.</summary>
    public void EndOn(DateTime lastDay)
    {
        if (lastDay.Date < EffectiveFrom)
            throw new ArgumentException("An assignment cannot end before it starts.", nameof(lastDay));
        EffectiveTo = lastDay.Date;
        base.Update();
    }

    /// <summary>Whether this assignment governs <paramref name="date"/>.</summary>
    public bool Covers(DateTime date)
    {
        var d = date.Date;
        return d >= EffectiveFrom && (EffectiveTo is null || d <= EffectiveTo);
    }

    private static void Guard(Guid employeeId, Guid workShiftId, DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (workShiftId == Guid.Empty)
            throw new ArgumentException("Shift is required.", nameof(workShiftId));
        if (effectiveTo is DateTime to && to.Date < effectiveFrom.Date)
            throw new ArgumentException("The end date cannot be before the start date.", nameof(effectiveTo));
    }
}
