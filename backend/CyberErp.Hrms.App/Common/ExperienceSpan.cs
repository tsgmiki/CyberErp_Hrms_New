namespace CyberErp.Hrms.App.Common
{
    /// <summary>
    /// How many years of experience a set of employment periods actually represents.
    /// </summary>
    /// <remarks>
    /// <para>Pure and separate from the handlers so the arithmetic can be tested without a
    /// database. The caller owns which periods count — internal service, prior employment, a
    /// recorded acting assignment — and this owns turning them into a number of years.</para>
    ///
    /// <para>⚠️ THE WHOLE POINT IS THAT PERIODS OVERLAP. Experience is a span of somebody's life,
    /// not a quantity to be added up: two concurrent part-time posts across the same three years
    /// are three years of experience, not six. Anything that sums period lengths independently is
    /// wrong the moment two of them touch, and the sources here genuinely do touch — several roles
    /// at one employer, and, routinely, a row describing the job the person still holds.</para>
    /// </remarks>
    public static class ExperienceSpan
    {
        /// <summary>Days in a year, averaged over the leap cycle.</summary>
        private const double DaysPerYear = 365.25;

        /// <summary>
        /// Total years covered by <paramref name="periods"/>, counting overlapping time once.
        /// </summary>
        /// <param name="periods">
        /// Start/end pairs. A period ending on or before it starts contributes nothing and is
        /// dropped rather than counted negative; the caller is expected to have already resolved
        /// "still ongoing" to a concrete end date.
        /// </param>
        public static decimal Years(IEnumerable<(DateTime Start, DateTime End)> periods)
        {
            var days = Merge(periods).Sum(p => (p.End - p.Start).TotalDays);
            return (decimal)(days / DaysPerYear);
        }

        /// <summary>
        /// Collapse <paramref name="periods"/> into non-overlapping spans, earliest first.
        /// </summary>
        /// <remarks>
        /// Exposed so a caller can show somebody WHICH periods were counted, not just the total —
        /// "we merged these three rows into one span" is the only useful answer when an employee
        /// disputes their years.
        /// </remarks>
        public static List<(DateTime Start, DateTime End)> Merge(
            IEnumerable<(DateTime Start, DateTime End)> periods)
        {
            var ordered = periods
                .Select(p => (Start: p.Start.Date, End: p.End.Date))
                .Where(p => p.End > p.Start)
                .OrderBy(p => p.Start)
                .ToList();
            if (ordered.Count == 0) return [];

            var merged = new List<(DateTime Start, DateTime End)>();
            var current = ordered[0];
            foreach (var next in ordered.Skip(1))
            {
                // `<=` not `<`: periods that merely touch (one ends the day the next begins) are
                // one continuous span, not two abutting ones.
                if (next.Start <= current.End)
                    current = (current.Start, next.End > current.End ? next.End : current.End);
                else
                {
                    merged.Add(current);
                    current = next;
                }
            }
            merged.Add(current);
            return merged;
        }
    }
}
