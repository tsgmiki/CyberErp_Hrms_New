using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.App.Common
{
    /// <summary>The shift rules a day is judged against. A plain value, so the maths needs no database.</summary>
    /// <param name="Start">Scheduled start, time of day.</param>
    /// <param name="End">Scheduled end. Less than or equal to <paramref name="Start"/> crosses midnight.</param>
    /// <param name="BreakMinutes">Unpaid break deducted from worked time.</param>
    /// <param name="GraceMinutes">Lateness tolerated before an arrival counts as late.</param>
    /// <param name="MinimumMinutesForPresent">Below this the day is absent however many punches there are.</param>
    /// <param name="HalfDayMinutes">At or above the minimum but below this, the day is a half day.</param>
    public record ShiftRules(
        TimeSpan Start,
        TimeSpan End,
        int BreakMinutes = 0,
        int GraceMinutes = 0,
        int MinimumMinutesForPresent = 0,
        int? HalfDayMinutes = null)
    {
        public bool CrossesMidnight => End <= Start;

        public int ScheduledMinutes
        {
            get
            {
                var span = CrossesMidnight ? TimeSpan.FromDays(1) - Start + End : End - Start;
                return Math.Max(0, (int)span.TotalMinutes - BreakMinutes);
            }
        }
    }

    /// <summary>What the day came to.</summary>
    public record AttendanceVerdict(
        AttendanceStatus Status,
        decimal DayValue,
        DateTime? FirstIn,
        DateTime? LastOut,
        int WorkedMinutes,
        int LateMinutes,
        int EarlyLeaveMinutes);

    /// <summary>
    /// Turns a day's punches into a verdict.
    /// </summary>
    /// <remarks>
    /// <para>Pure and separate from the handler so the rules can be tested without a database,
    /// following <see cref="SiblingOrder"/>, <see cref="ExperienceSpan"/> and
    /// <see cref="ServiceHistory"/>. The handler owns what only the data can answer — which shift
    /// applied, whether leave was approved, whether the date is a holiday — and this owns the
    /// arithmetic and the precedence.</para>
    ///
    /// <para>⚠️ PRECEDENCE IS THE WHOLE POINT, and it is deliberately this order: holiday, then
    /// rest day, then approved leave, then punches. A person on approved leave on a public holiday
    /// must not be charged a leave day, and somebody absent on a rest day is not absent — they were
    /// not expected.</para>
    ///
    /// <para>⚠️ Work on a holiday or rest day is RECORDED but does not change the status. The
    /// minutes are kept so overtime (Phase 4) has something to pay from; calling it "Present" would
    /// quietly consume a day's entitlement for a day nobody owed.</para>
    /// </remarks>
    public static class AttendanceEvaluation
    {
        /// <param name="punches">
        /// The day's punch times, in any order. ⚠️ The CALLER decides which punches belong to this
        /// day — that matters for a night shift, where 02:00 settles against the previous day.
        /// </param>
        /// <param name="calendarDayValue">
        /// What a full attendance on this date is worth per the working calendar: 1 for a normal
        /// day, 0.5 for a half-work Saturday, 0 for a rest day or holiday.
        /// </param>
        public static AttendanceVerdict Evaluate(
            ShiftRules? shift,
            IEnumerable<DateTime> punches,
            decimal calendarDayValue,
            bool isHoliday = false,
            bool isRestDay = false,
            decimal? approvedLeaveValue = null)
        {
            var ordered = punches.OrderBy(p => p).ToList();
            var firstIn = ordered.Count > 0 ? ordered[0] : (DateTime?)null;
            var lastOut = ordered.Count > 1 ? ordered[^1] : (DateTime?)null;
            var worked = WorkedMinutes(ordered, shift?.BreakMinutes ?? 0);

            // ---- precedence ------------------------------------------------------------------
            if (isHoliday)
                return new AttendanceVerdict(AttendanceStatus.Holiday, 0m, firstIn, lastOut, worked, 0, 0);
            if (isRestDay)
                return new AttendanceVerdict(AttendanceStatus.RestDay, 0m, firstIn, lastOut, worked, 0, 0);
            if (approvedLeaveValue is decimal leave && leave > 0m)
                return new AttendanceVerdict(AttendanceStatus.OnLeave, leave, firstIn, lastOut, worked, 0, 0);

            // ⚠️ No shift assigned: the punches are still recorded, but there is no schedule to
            // judge lateness or a short day against, so presence is all that can honestly be said.
            if (shift is null)
                return ordered.Count == 0
                    ? new AttendanceVerdict(AttendanceStatus.Absent, 0m, null, null, 0, 0, 0)
                    : new AttendanceVerdict(AttendanceStatus.Present, calendarDayValue, firstIn, lastOut, worked, 0, 0);

            if (ordered.Count == 0)
                return new AttendanceVerdict(AttendanceStatus.Absent, 0m, null, null, 0, 0, 0);

            var (late, early) = Deviations(shift, firstIn, lastOut);

            // ---- how much of a day was earned -------------------------------------------------
            decimal fraction;
            AttendanceStatus status;
            if (worked < shift.MinimumMinutesForPresent)
            {
                // ⚠️ Somebody stood at the door and left. The punch is kept, the day is not.
                fraction = 0m;
                status = AttendanceStatus.Absent;
            }
            else if (shift.HalfDayMinutes is int half && worked < half)
            {
                fraction = 0.5m;
                status = AttendanceStatus.HalfDay;
            }
            else
            {
                fraction = 1m;
                status = late > 0 ? AttendanceStatus.Late : AttendanceStatus.Present;
            }

            return new AttendanceVerdict(
                status, fraction * calendarDayValue, firstIn, lastOut, worked, late, early);
        }

        /// <summary>
        /// Minutes between successive punches, taken in PAIRS.
        /// </summary>
        /// <remarks>
        /// ⚠️ Pairs, not first-to-last. First-to-last would pay a two-hour lunch trip home as time
        /// worked on any terminal that records every passage. A dangling final punch — clocked in,
        /// never out — contributes nothing rather than being stretched to the end of the shift,
        /// because inventing an exit time is how a missed punch turns into unearned hours.
        /// </remarks>
        private static int WorkedMinutes(IReadOnlyList<DateTime> ordered, int breakMinutes)
        {
            var total = 0d;
            for (var i = 0; i + 1 < ordered.Count; i += 2)
                total += (ordered[i + 1] - ordered[i]).TotalMinutes;
            return Math.Max(0, (int)Math.Round(total) - breakMinutes);
        }

        /// <summary>Lateness and early leaving against the schedule, in minutes.</summary>
        /// <remarks>
        /// ⚠️ Lateness is measured from the SCHEDULED START, not from the end of grace: arriving
        /// twenty minutes late with a ten-minute grace is twenty minutes late, and grace only
        /// decides whether it counts at all. Reporting ten would understate every late arrival in
        /// the building.
        /// </remarks>
        private static (int Late, int Early) Deviations(ShiftRules shift, DateTime? firstIn, DateTime? lastOut)
        {
            var late = 0;
            var early = 0;

            if (firstIn is DateTime inAt)
            {
                var scheduledStart = inAt.Date + shift.Start;
                // A night shift's start belongs to the previous day when the punch is after midnight.
                if (shift.CrossesMidnight && inAt.TimeOfDay < shift.End)
                    scheduledStart = scheduledStart.AddDays(-1);

                var minutesLate = (int)Math.Round((inAt - scheduledStart).TotalMinutes);
                if (minutesLate > shift.GraceMinutes) late = minutesLate;
            }

            if (lastOut is DateTime outAt)
            {
                var scheduledEnd = outAt.Date + shift.End;
                if (shift.CrossesMidnight && outAt.TimeOfDay >= shift.Start)
                    scheduledEnd = scheduledEnd.AddDays(1);

                var minutesEarly = (int)Math.Round((scheduledEnd - outAt).TotalMinutes);
                if (minutesEarly > 0) early = minutesEarly;
            }

            return (late, early);
        }
    }
}
