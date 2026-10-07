using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Attendance (HC041): a named working pattern — when the day starts and ends, how long the unpaid
/// break is, and how much lateness is tolerated before it counts.
/// </summary>
/// <remarks>
/// <para>⚠️ A shift says NOTHING about which days are worked. That is the
/// <see cref="WorkWeekConfiguration"/>'s job (Full / Half / Rest per weekday) and the
/// <c>Holiday</c> table's, both already owned by the working calendar. Duplicating a "works
/// Saturday?" flag here would give two answers to one question, and leave and attendance would
/// eventually disagree about the same date.</para>
///
/// <para>⚠️ A shift may CROSS MIDNIGHT (22:00 → 06:00). That is not an edge case in a vaccine
/// plant with night production, and it is why <see cref="CrossesMidnight"/> exists: the end time
/// belongs to the NEXT calendar day, so a punch at 02:00 settles against the previous day's shift
/// rather than opening a new one.</para>
/// </remarks>
public class WorkShift : BaseEntity, IAggregateRoot, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? NameA { get; private set; }

    /// <summary>Scheduled start, as a time of day.</summary>
    public TimeSpan StartTime { get; private set; }
    /// <summary>Scheduled end. Less than or equal to <see cref="StartTime"/> means it crosses midnight.</summary>
    public TimeSpan EndTime { get; private set; }

    /// <summary>Unpaid break, deducted from worked minutes.</summary>
    public int BreakMinutes { get; private set; }

    /// <summary>Lateness tolerated before an arrival counts as late.</summary>
    public int GraceMinutes { get; private set; }

    /// <summary>
    /// Minutes that must be worked for the day to count as present at all. Below this the day is
    /// absent even though somebody punched — it stops a thirty-second appearance reading as a
    /// full day.
    /// </summary>
    public int MinimumMinutesForPresent { get; private set; }

    /// <summary>Worked minutes at or above this, but below a full day, count as a half day.</summary>
    public int? HalfDayMinutes { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>True when the shift ends on the following calendar day.</summary>
    public bool CrossesMidnight => EndTime <= StartTime;

    /// <summary>Scheduled length, net of the break; correct across midnight.</summary>
    public int ScheduledMinutes
    {
        get
        {
            var span = CrossesMidnight
                ? TimeSpan.FromDays(1) - StartTime + EndTime
                : EndTime - StartTime;
            return Math.Max(0, (int)span.TotalMinutes - BreakMinutes);
        }
    }

    private WorkShift() : base() { }

    public static WorkShift Create(
        string code, string name, TimeSpan startTime, TimeSpan endTime,
        string? nameA = null, int breakMinutes = 0, int graceMinutes = 0,
        int minimumMinutesForPresent = 0, int? halfDayMinutes = null, bool isActive = true)
    {
        Guard(code, name, startTime, endTime, breakMinutes, graceMinutes, minimumMinutesForPresent, halfDayMinutes);
        return new WorkShift
        {
            Code = code.Trim(),
            Name = name.Trim(),
            NameA = string.IsNullOrWhiteSpace(nameA) ? null : nameA.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            BreakMinutes = breakMinutes,
            GraceMinutes = graceMinutes,
            MinimumMinutesForPresent = minimumMinutesForPresent,
            HalfDayMinutes = halfDayMinutes,
            IsActive = isActive
        };
    }

    public void Update(
        string code, string name, TimeSpan startTime, TimeSpan endTime,
        string? nameA, int breakMinutes, int graceMinutes,
        int minimumMinutesForPresent, int? halfDayMinutes, bool isActive)
    {
        Guard(code, name, startTime, endTime, breakMinutes, graceMinutes, minimumMinutesForPresent, halfDayMinutes);
        Code = code.Trim();
        Name = name.Trim();
        NameA = string.IsNullOrWhiteSpace(nameA) ? null : nameA.Trim();
        StartTime = startTime;
        EndTime = endTime;
        BreakMinutes = breakMinutes;
        GraceMinutes = graceMinutes;
        MinimumMinutesForPresent = minimumMinutesForPresent;
        HalfDayMinutes = halfDayMinutes;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(
        string code, string name, TimeSpan startTime, TimeSpan endTime,
        int breakMinutes, int graceMinutes, int minimumMinutesForPresent, int? halfDayMinutes)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Shift code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shift name is required.", nameof(name));
        if (startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1))
            throw new ArgumentException("Start time must be a time of day.", nameof(startTime));
        if (endTime < TimeSpan.Zero || endTime >= TimeSpan.FromDays(1))
            throw new ArgumentException("End time must be a time of day.", nameof(endTime));
        if (breakMinutes < 0)
            throw new ArgumentException("Break minutes cannot be negative.", nameof(breakMinutes));
        if (graceMinutes < 0)
            throw new ArgumentException("Grace minutes cannot be negative.", nameof(graceMinutes));
        if (minimumMinutesForPresent < 0)
            throw new ArgumentException("Minimum minutes cannot be negative.", nameof(minimumMinutesForPresent));
        if (halfDayMinutes is < 0)
            throw new ArgumentException("Half-day minutes cannot be negative.", nameof(halfDayMinutes));

        // ⚠️ A break longer than the shift would make every full day read as negative work.
        var span = endTime <= startTime
            ? TimeSpan.FromDays(1) - startTime + endTime
            : endTime - startTime;
        if (breakMinutes >= span.TotalMinutes)
            throw new ArgumentException("The break cannot be as long as the shift itself.", nameof(breakMinutes));
    }
}
