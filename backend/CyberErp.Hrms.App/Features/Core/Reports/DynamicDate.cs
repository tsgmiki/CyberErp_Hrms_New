namespace CyberErp.Hrms.App.Features.Core.Reports
{
    /// <summary>
    /// Relative ("dynamic") date criteria for SCHEDULED reports (reference _x_ReportFieldValues date
    /// options). A schedule saves a TOKEN (e.g. "StartOfMonth") for a date criterion instead of a static
    /// calendar date; the execution engine resolves it to a concrete date at RUN TIME so a recurring
    /// schedule always covers the intended moving window (this month, last 7 days, …).
    ///
    /// The <see cref="Catalog"/> here is the C# side of the pair — it MUST stay in sync with the
    /// '@DynamicDate' branch of <c>Core.hrms_ReportFieldValues</c>, which structures the same tokens as
    /// selectable options for the schedule form.
    /// </summary>
    public static class DynamicDate
    {
        /// <summary>Reserved lookup field key the master SP recognises to return the relative-date catalog.</summary>
        public const string FieldKey = "@DynamicDate";

        /// <summary>(Token, friendly label) — the selectable relative-date options.</summary>
        public static readonly IReadOnlyList<(string Token, string Label)> Catalog =
        [
            ("Today", "Today"),
            ("Yesterday", "Yesterday"),
            ("Tomorrow", "Tomorrow"),
            ("StartOfWeek", "Start of this week"),
            ("EndOfWeek", "End of this week"),
            ("StartOfMonth", "Start of this month"),
            ("EndOfMonth", "End of this month"),
            ("StartOfLastMonth", "Start of last month"),
            ("EndOfLastMonth", "End of last month"),
            ("StartOfQuarter", "Start of this quarter"),
            ("EndOfQuarter", "End of this quarter"),
            ("StartOfYear", "Start of this year"),
            ("EndOfYear", "End of this year"),
            ("Last7Days", "7 days ago"),
            ("Last30Days", "30 days ago"),
            ("Last90Days", "90 days ago"),
        ];

        private static readonly HashSet<string> TokenSet =
            new(Catalog.Select(c => c.Token), StringComparer.OrdinalIgnoreCase);

        public static bool IsToken(string? value) => value is not null && TokenSet.Contains(value.Trim());

        /// <summary>Resolve a relative token to a concrete date as of <paramref name="asOf"/>.
        /// Returns false (and leaves <paramref name="date"/> unchanged) for a non-token value.</summary>
        public static bool TryResolve(string? value, DateOnly asOf, out DateOnly date)
        {
            date = asOf;
            if (string.IsNullOrWhiteSpace(value)) return false;
            var startOfMonth = new DateOnly(asOf.Year, asOf.Month, 1);
            switch (value.Trim().ToLowerInvariant())
            {
                case "today": date = asOf; return true;
                case "yesterday": date = asOf.AddDays(-1); return true;
                case "tomorrow": date = asOf.AddDays(1); return true;
                case "startofweek": date = StartOfWeek(asOf); return true;
                case "endofweek": date = StartOfWeek(asOf).AddDays(6); return true;
                case "startofmonth": date = startOfMonth; return true;
                case "endofmonth": date = startOfMonth.AddMonths(1).AddDays(-1); return true;
                case "startoflastmonth": date = startOfMonth.AddMonths(-1); return true;
                case "endoflastmonth": date = startOfMonth.AddDays(-1); return true;
                case "startofquarter": date = StartOfQuarter(asOf); return true;
                case "endofquarter": date = StartOfQuarter(asOf).AddMonths(3).AddDays(-1); return true;
                case "startofyear": date = new DateOnly(asOf.Year, 1, 1); return true;
                case "endofyear": date = new DateOnly(asOf.Year, 12, 31); return true;
                case "last7days": date = asOf.AddDays(-7); return true;
                case "last30days": date = asOf.AddDays(-30); return true;
                case "last90days": date = asOf.AddDays(-90); return true;
                default: return false;
            }
        }

        /// <summary>Resolve every dynamic-date token in a saved criteria set to concrete dates (yyyy-MM-dd);
        /// non-token values pass through untouched.</summary>
        public static Dictionary<string, string?> ResolveCriteria(IReadOnlyDictionary<string, string?> criteria, DateOnly asOf)
        {
            var result = new Dictionary<string, string?>(criteria.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in criteria)
                result[key] = TryResolve(value, asOf, out var d) ? d.ToString("yyyy-MM-dd") : value;
            return result;
        }

        // Monday-based start of week.
        private static DateOnly StartOfWeek(DateOnly d) => d.AddDays(-(((int)d.DayOfWeek + 6) % 7));
        private static DateOnly StartOfQuarter(DateOnly d) => new(d.Year, ((d.Month - 1) / 3) * 3 + 1, 1);
    }
}
