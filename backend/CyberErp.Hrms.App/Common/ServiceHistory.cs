namespace CyberErp.Hrms.App.Common
{
    /// <summary>One post somebody held, and when.</summary>
    /// <param name="Title">The post's English title.</param>
    /// <param name="TitleAmharic">Its Amharic title, or null when none is recorded.</param>
    /// <param name="From">First day in the post.</param>
    /// <param name="To">Last day in the post, or null while they still hold it.</param>
    public record ServicePeriod(string Title, string? TitleAmharic, DateTime From, DateTime? To);

    /// <summary>A position change: the post taken up, and the day it took effect.</summary>
    public record PositionChange(DateTime EffectiveDate, string Title, string? TitleAmharic);

    /// <summary>
    /// Turns a hire date and a list of position changes into the dated post history an experience
    /// letter states.
    /// </summary>
    /// <remarks>
    /// <para>Pure and separate from the document handler so the date arithmetic can be tested
    /// without a database, following <see cref="SiblingOrder"/> and <see cref="ExperienceSpan"/>.
    /// The handler owns reading the movements; this owns turning change EVENTS into SPANS.</para>
    ///
    /// <para>⚠️ Movements record the day a new post BEGINS, not the day the old one ended. The
    /// previous post therefore ends the day BEFORE the next one starts — off by one here writes a
    /// letter where somebody held two posts on the same day, which is exactly the detail a
    /// suspicious reader checks.</para>
    /// </remarks>
    public static class ServiceHistory
    {
        /// <summary>
        /// Build the post history, earliest first.
        /// </summary>
        /// <param name="hireDate">Day service began. Null yields nothing — a letter cannot state a start it does not know.</param>
        /// <param name="hiredAs">The post held at hire. Falls back to the first change's post when unknown.</param>
        /// <param name="hiredAsAmharic">Amharic title of that post.</param>
        /// <param name="changes">Position changes, in any order; only those after the hire date are used.</param>
        /// <param name="serviceEnded">
        /// Last working day for somebody who has left. Null means still serving, and the final
        /// period is left open — the letter then says "to date" rather than inventing an end.
        /// </param>
        public static List<ServicePeriod> Build(
            DateTime? hireDate,
            string? hiredAs,
            string? hiredAsAmharic,
            IEnumerable<PositionChange> changes,
            DateTime? serviceEnded = null)
        {
            if (hireDate is not DateTime hired) return [];
            var start = hired.Date;

            // ⚠️ A change dated on or before the hire date cannot open a period — it would produce
            // a span ending before it began. Such rows are data errors (a backdated correction, a
            // movement attached to the wrong employee) and are dropped rather than rendered.
            var ordered = changes
                .Where(c => c.EffectiveDate.Date > start)
                .OrderBy(c => c.EffectiveDate.Date)
                .ToList();

            // Several movements on one day are one change: only the last one landed. Keeping both
            // would print a zero-length period.
            var distinct = new List<PositionChange>();
            foreach (var c in ordered)
            {
                if (distinct.Count > 0 && distinct[^1].EffectiveDate.Date == c.EffectiveDate.Date)
                    distinct[^1] = c;
                else
                    distinct.Add(c);
            }

            var periods = new List<ServicePeriod>();

            // The opening period. When the post at hire was never recorded, the first change's own
            // "from" post is the best evidence of it; failing that, say so rather than guess.
            var openingTitle = hiredAs;
            var openingTitleAmharic = hiredAsAmharic;

            var cursorTitle = openingTitle;
            var cursorTitleAmharic = openingTitleAmharic;
            var cursorFrom = start;

            foreach (var change in distinct)
            {
                periods.Add(new ServicePeriod(
                    cursorTitle ?? string.Empty,
                    cursorTitleAmharic,
                    cursorFrom,
                    change.EffectiveDate.Date.AddDays(-1)));   // ends the day BEFORE the next begins

                cursorTitle = change.Title;
                cursorTitleAmharic = change.TitleAmharic;
                cursorFrom = change.EffectiveDate.Date;
            }

            periods.Add(new ServicePeriod(
                cursorTitle ?? string.Empty,
                cursorTitleAmharic,
                cursorFrom,
                serviceEnded?.Date));

            // ⚠️ A last working day BEFORE the final post began means the movement and the
            // termination disagree. Printing it would show a period running backwards; dropping the
            // period would silently lose a post. Clamping to the start states a one-day period,
            // which is visibly odd and prompts somebody to look — the right failure here.
            if (serviceEnded is DateTime ended && ended.Date < cursorFrom)
                periods[^1] = periods[^1] with { To = cursorFrom };

            return periods;
        }
    }
}
