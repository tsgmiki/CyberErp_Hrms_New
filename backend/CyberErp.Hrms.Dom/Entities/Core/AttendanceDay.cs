using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttendanceStatus
{
    /// <summary>Nothing has been worked out for this day yet.</summary>
    NotProcessed = 0,
    Present = 1,
    Late = 2,
    HalfDay = 3,
    Absent = 4,
    /// <summary>Covered by approved leave.</summary>
    OnLeave = 5,
    Holiday = 6,
    /// <summary>A non-working weekday per the work-week configuration.</summary>
    RestDay = 7
}

/// <summary>
/// Attendance (HC043): what one employee's one day came to, derived from their punches.
/// </summary>
/// <remarks>
/// <para>⚠️ DERIVED, AND SEPARATE FROM THE EVIDENCE. Punches are never edited; this is the layer an
/// administrator corrects, which is why the override fields exist and why
/// <see cref="DerivedStatus"/> is kept alongside <see cref="Status"/>. "Marked present by Almaz on
/// the 4th, the machine said absent" is a defensible record; silently showing Present is not.</para>
///
/// <para>⚠️ Re-processing must NOT discard a correction. <see cref="ApplyDerivation"/> refuses to
/// touch an overridden row, so a nightly job cannot quietly undo what HR decided that morning.</para>
///
/// <para>⚠️ One row per employee per date, enforced by a unique index — the processor upserts, so a
/// day run twice settles rather than duplicating.</para>
/// </remarks>
public class AttendanceDay : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public DateTime WorkDate { get; private set; }

    /// <summary>The shift in force that day, or null if none was assigned.</summary>
    public Guid? WorkShiftId { get; private set; }

    public DateTime? FirstIn { get; private set; }
    public DateTime? LastOut { get; private set; }

    public int WorkedMinutes { get; private set; }
    public int LateMinutes { get; private set; }
    public int EarlyLeaveMinutes { get; private set; }

    /// <summary>What is reported. Equals <see cref="DerivedStatus"/> unless somebody overrode it.</summary>
    public AttendanceStatus Status { get; private set; } = AttendanceStatus.NotProcessed;

    /// <summary>What the punches said, preserved even after an override.</summary>
    public AttendanceStatus DerivedStatus { get; private set; } = AttendanceStatus.NotProcessed;

    /// <summary>How much of a working day this counts as — 1, 0.5 or 0. Feeds payroll later.</summary>
    public decimal DayValue { get; private set; }

    public bool IsOverridden { get; private set; }
    public string? OverrideReason { get; private set; }
    public string? OverriddenBy { get; private set; }
    public DateTime? OverriddenAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }
    public string? Remark { get; private set; }

    public Employee? Employee { get; private set; }
    public WorkShift? WorkShift { get; private set; }

    private AttendanceDay() : base() { }

    public static AttendanceDay Create(Guid employeeId, DateTime workDate)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        return new AttendanceDay { EmployeeId = employeeId, WorkDate = workDate.Date };
    }

    /// <summary>
    /// Writes the processor's verdict.
    /// </summary>
    /// <returns>
    /// False when the row is overridden and was therefore left alone — the caller reports it as
    /// skipped rather than counting a correction it did not make.
    /// </returns>
    public bool ApplyDerivation(
        AttendanceStatus derived, decimal dayValue, Guid? shiftId,
        DateTime? firstIn, DateTime? lastOut,
        int workedMinutes, int lateMinutes, int earlyLeaveMinutes)
    {
        // ⚠️ The derived verdict is recorded even on an overridden row, so the two can be compared
        // later; only the REPORTED status is left alone.
        DerivedStatus = derived;
        WorkShiftId = shiftId;
        FirstIn = firstIn;
        LastOut = lastOut;
        WorkedMinutes = workedMinutes;
        LateMinutes = lateMinutes;
        EarlyLeaveMinutes = earlyLeaveMinutes;
        ProcessedAt = DateTime.UtcNow;

        if (IsOverridden)
        {
            base.Update();
            return false;
        }

        Status = derived;
        DayValue = dayValue;
        base.Update();
        return true;
    }

    /// <summary>An administrator's correction, with its reason and author.</summary>
    public void Override(AttendanceStatus status, decimal dayValue, string reason, string? by, string? remark = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("An override must say why.", nameof(reason));
        if (dayValue is < 0 or > 1)
            throw new ArgumentException("A day is worth between 0 and 1.", nameof(dayValue));

        Status = status;
        DayValue = dayValue;
        IsOverridden = true;
        OverrideReason = reason.Trim();
        OverriddenBy = by;
        OverriddenAt = DateTime.UtcNow;
        Remark = remark;
        base.Update();
    }

    /// <summary>Drops the correction and restores what the punches said.</summary>
    public void ClearOverride()
    {
        IsOverridden = false;
        OverrideReason = null;
        OverriddenBy = null;
        OverriddenAt = null;
        Status = DerivedStatus;
        base.Update();
    }
}
