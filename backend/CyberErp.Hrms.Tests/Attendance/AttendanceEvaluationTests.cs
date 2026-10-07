using CyberErp.Hrms.App.Common;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Attendance;

/// <summary>
/// What a day's punches come to — the rules that decide whether somebody was paid for a day.
/// </summary>
public class AttendanceEvaluationTests
{
    private static readonly DateTime Day = new(2026, 10, 7);

    /// <summary>08:00–17:00, one hour unpaid, ten minutes' grace.</summary>
    private static ShiftRules Day9to5(int minimum = 0, int? half = null) =>
        new(TimeSpan.FromHours(8), TimeSpan.FromHours(17),
            BreakMinutes: 60, GraceMinutes: 10, MinimumMinutesForPresent: minimum, HalfDayMinutes: half);

    private static DateTime At(int h, int m = 0) => Day.AddHours(h).AddMinutes(m);

    private static AttendanceVerdict Eval(
        ShiftRules? shift, DateTime[] punches, decimal calendarValue = 1m,
        bool holiday = false, bool rest = false, decimal? leave = null) =>
        AttendanceEvaluation.Evaluate(shift, punches, calendarValue, holiday, rest, leave);

    // ---- the ordinary day ------------------------------------------------------------------

    [Fact]
    public void AFullDayOnTimeIsPresent()
    {
        var v = Eval(Day9to5(), [At(8), At(17)]);

        Assert.Equal(AttendanceStatus.Present, v.Status);
        Assert.Equal(1m, v.DayValue);
        Assert.Equal(480, v.WorkedMinutes);   // nine hours less the unpaid hour
        Assert.Equal(0, v.LateMinutes);
        Assert.Equal(0, v.EarlyLeaveMinutes);
    }

    [Fact]
    public void NoPunchesIsAbsent()
    {
        var v = Eval(Day9to5(), []);

        Assert.Equal(AttendanceStatus.Absent, v.Status);
        Assert.Equal(0m, v.DayValue);
        Assert.Null(v.FirstIn);
    }

    // ---- lateness ---------------------------------------------------------------------------

    /// <summary>⚠️ Inside the grace window is NOT late — that is what grace is for.</summary>
    [Fact]
    public void ArrivingWithinGraceIsNotLate()
    {
        var v = Eval(Day9to5(), [At(8, 9), At(17)]);

        Assert.Equal(AttendanceStatus.Present, v.Status);
        Assert.Equal(0, v.LateMinutes);
    }

    /// <summary>
    /// ⚠️ Lateness is counted from the SCHEDULED START, not from the end of grace. Twenty minutes
    /// late with ten minutes' grace is twenty minutes late; reporting ten would understate every
    /// late arrival in the building.
    /// </summary>
    [Fact]
    public void LatenessIsMeasuredFromTheScheduledStartNotTheEndOfGrace()
    {
        var v = Eval(Day9to5(), [At(8, 20), At(17)]);

        Assert.Equal(AttendanceStatus.Late, v.Status);
        Assert.Equal(20, v.LateMinutes);
        Assert.Equal(1m, v.DayValue);   // still a full day's work
    }

    [Fact]
    public void LeavingEarlyIsRecorded()
    {
        var v = Eval(Day9to5(), [At(8), At(16, 30)]);

        Assert.Equal(30, v.EarlyLeaveMinutes);
    }

    // ---- precedence ---------------------------------------------------------------------------

    /// <summary>⚠️ A holiday outranks everything: nobody is absent on a day they were not owed.</summary>
    [Fact]
    public void AHolidayBeatsAnAbsence()
    {
        var v = Eval(Day9to5(), [], calendarValue: 0m, holiday: true);

        Assert.Equal(AttendanceStatus.Holiday, v.Status);
        Assert.Equal(0m, v.DayValue);
    }

    [Fact]
    public void ARestDayBeatsAnAbsence()
    {
        var v = Eval(Day9to5(), [], calendarValue: 0m, rest: true);

        Assert.Equal(AttendanceStatus.RestDay, v.Status);
    }

    /// <summary>
    /// ⚠️ THE ONE THAT COSTS SOMEBODY A DAY. Approved leave on a public holiday must report as the
    /// holiday, not as leave — otherwise the entitlement is spent on a day nobody was due to work.
    /// </summary>
    [Fact]
    public void AHolidayBeatsApprovedLeave()
    {
        var v = Eval(Day9to5(), [], calendarValue: 0m, holiday: true, leave: 1m);

        Assert.Equal(AttendanceStatus.Holiday, v.Status);
    }

    [Fact]
    public void ApprovedLeaveBeatsAnAbsence()
    {
        var v = Eval(Day9to5(), [], leave: 1m);

        Assert.Equal(AttendanceStatus.OnLeave, v.Status);
        Assert.Equal(1m, v.DayValue);
    }

    [Fact]
    public void HalfDayLeaveIsWorthHalfADay()
    {
        var v = Eval(Day9to5(), [], leave: 0.5m);

        Assert.Equal(AttendanceStatus.OnLeave, v.Status);
        Assert.Equal(0.5m, v.DayValue);
    }

    /// <summary>
    /// ⚠️ Work on a holiday is RECORDED but does not become "Present". The minutes are kept so
    /// overtime can be paid from them later; calling it a present day would consume an entitlement
    /// for a day nobody owed.
    /// </summary>
    [Fact]
    public void WorkingOnAHolidayKeepsTheMinutesButNotTheStatus()
    {
        var v = Eval(Day9to5(), [At(8), At(12)], calendarValue: 0m, holiday: true);

        Assert.Equal(AttendanceStatus.Holiday, v.Status);
        Assert.Equal(0m, v.DayValue);
        Assert.Equal(180, v.WorkedMinutes);   // four hours less the unpaid hour
    }

    // ---- short days -----------------------------------------------------------------------------

    /// <summary>⚠️ A thirty-second appearance must not read as a full day.</summary>
    [Fact]
    public void BelowTheMinimumIsAbsentEvenWithPunches()
    {
        var v = Eval(Day9to5(minimum: 120), [At(8), At(8, 20)]);

        Assert.Equal(AttendanceStatus.Absent, v.Status);
        Assert.Equal(0m, v.DayValue);
        Assert.NotNull(v.FirstIn);   // the punch is still on the record
    }

    [Fact]
    public void BetweenTheMinimumAndTheHalfDayThresholdIsAHalfDay()
    {
        // 08:00–13:00 = 300 minutes less the 60-minute break = 240.
        var v = Eval(Day9to5(minimum: 60, half: 300), [At(8), At(13)]);

        Assert.Equal(AttendanceStatus.HalfDay, v.Status);
        Assert.Equal(0.5m, v.DayValue);
    }

    [Fact]
    public void AtOrAboveTheHalfDayThresholdIsAFullDay()
    {
        var v = Eval(Day9to5(minimum: 60, half: 240), [At(8), At(17)]);

        Assert.Equal(AttendanceStatus.Present, v.Status);
        Assert.Equal(1m, v.DayValue);
    }

    // ---- the working calendar scales the day ------------------------------------------------------

    /// <summary>
    /// ⚠️ A full attendance on a HALF-WORK Saturday is worth half a day, not a whole one. The
    /// calendar decides what the date is worth; attendance decides how much of it was earned.
    /// </summary>
    [Fact]
    public void AFullDayOnAHalfWorkSaturdayIsWorthHalf()
    {
        var v = Eval(Day9to5(), [At(8), At(17)], calendarValue: 0.5m);

        Assert.Equal(AttendanceStatus.Present, v.Status);
        Assert.Equal(0.5m, v.DayValue);
    }

    // ---- pairing ---------------------------------------------------------------------------------

    /// <summary>
    /// ⚠️ Punches pair up; they are not measured first-to-last. A terminal on every door records
    /// the lunch trip home, and first-to-last would pay for it.
    /// </summary>
    [Fact]
    public void PunchesArePairedSoABreakOutIsNotPaid()
    {
        // in 08:00, out 12:00, in 13:00, out 17:00 = 8h worked, less the 1h unpaid break = 420.
        var v = Eval(Day9to5(), [At(8), At(12), At(13), At(17)]);

        Assert.Equal(420, v.WorkedMinutes);
        Assert.Equal(At(8), v.FirstIn);
        Assert.Equal(At(17), v.LastOut);
    }

    /// <summary>
    /// ⚠️ Clocked in and never out: the dangling punch earns NOTHING rather than being stretched to
    /// the end of the shift. Inventing an exit time is how a missed punch becomes unearned hours.
    /// </summary>
    [Fact]
    public void ADanglingPunchEarnsNothing()
    {
        var v = Eval(Day9to5(minimum: 1), [At(8)]);

        Assert.Equal(0, v.WorkedMinutes);
        Assert.Equal(AttendanceStatus.Absent, v.Status);
        Assert.Equal(At(8), v.FirstIn);   // but it is still visible to whoever reviews the day
    }

    [Fact]
    public void PunchesOutOfOrderAreSortedFirst()
    {
        var a = Eval(Day9to5(), [At(17), At(8)]);
        var b = Eval(Day9to5(), [At(8), At(17)]);

        Assert.Equal(b.WorkedMinutes, a.WorkedMinutes);
        Assert.Equal(b.FirstIn, a.FirstIn);
    }

    // ---- night shift ------------------------------------------------------------------------------

    /// <summary>⚠️ 22:00–06:00 is eight hours, not minus sixteen.</summary>
    [Fact]
    public void ANightShiftSpansMidnight()
    {
        var night = new ShiftRules(TimeSpan.FromHours(22), TimeSpan.FromHours(6));

        Assert.True(night.CrossesMidnight);
        Assert.Equal(480, night.ScheduledMinutes);
    }

    [Fact]
    public void ANightShiftWorkedInFullIsPresentAndNotLate()
    {
        var night = new ShiftRules(TimeSpan.FromHours(22), TimeSpan.FromHours(6), GraceMinutes: 10);
        var v = Eval(night, [Day.AddHours(22), Day.AddDays(1).AddHours(6)]);

        Assert.Equal(AttendanceStatus.Present, v.Status);
        Assert.Equal(480, v.WorkedMinutes);
        Assert.Equal(0, v.LateMinutes);
        Assert.Equal(0, v.EarlyLeaveMinutes);
    }

    /// <summary>A night-shift arrival after midnight is measured against the PREVIOUS day's start.</summary>
    [Fact]
    public void ANightShiftArrivalAfterMidnightIsNotSixteenHoursLate()
    {
        var night = new ShiftRules(TimeSpan.FromHours(22), TimeSpan.FromHours(6), GraceMinutes: 10);
        var v = Eval(night, [Day.AddDays(1).AddHours(0).AddMinutes(30), Day.AddDays(1).AddHours(6)]);

        Assert.Equal(150, v.LateMinutes);   // 22:00 → 00:30, not 22:00 → 00:30 + a day
    }

    // ---- no shift assigned -------------------------------------------------------------------------

    /// <summary>
    /// ⚠️ Without a shift there is no schedule to judge against, so presence is all that can
    /// honestly be reported — not lateness invented from a default start time.
    /// </summary>
    [Fact]
    public void WithoutAShiftAPunchedDayIsPresentWithNoLateness()
    {
        var v = Eval(null, [At(9, 45), At(17)]);

        Assert.Equal(AttendanceStatus.Present, v.Status);
        Assert.Equal(0, v.LateMinutes);
        Assert.Equal(1m, v.DayValue);
    }

    [Fact]
    public void WithoutAShiftAndWithoutPunchesTheDayIsAbsent()
    {
        Assert.Equal(AttendanceStatus.Absent, Eval(null, []).Status);
    }
}
