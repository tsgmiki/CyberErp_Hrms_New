namespace CyberErp.Hrms.App.Features.Core.Reports
{
    /// <summary>
    /// Port over the ported legacy report-schedule stored procedures (reference _x_ReportClientSchedule*,
    /// _x_ReportFieldOutputRead, _x_ReportGenerateGetScheduleInfo, _x_ReportGenerateSendToHistory,
    /// _x_ReportActivate, _x_ReportDelete). Every method is a thin Dapper call into exactly one procedure
    /// so the schedule CRUD is driven by the SPs — not EF change tracking — exactly like the reference.
    /// The implementation lives in Inf (<c>ReportScheduleStore</c>).
    /// </summary>
    public interface IReportScheduleStore
    {
        /// <summary>_x_ReportClientSchedule — INSERT/UPDATE the header; returns the (existing or new) id.</summary>
        Task<Guid> UpsertHeaderAsync(ScheduleHeader header);

        /// <summary>_x_ReportClientScheduleDelete — <paramref name="modifyOnly"/>=true clears the three child
        /// tables only (for a re-save); false deletes children + header.</summary>
        Task DeleteAsync(Guid scheduleId, bool modifyOnly);

        /// <summary>_x_ReportClientScheduleEnable — flips the grid enable/disable toggle.</summary>
        Task EnableAsync(Guid scheduleId, bool enabled);

        /// <summary>_x_ReportClientScheduleFieldValue — append one saved criteria value.</summary>
        Task AddFieldValueAsync(Guid scheduleId, string reportKey, string field, string? value);

        /// <summary>_x_ReportClientScheduleFieldOutput — append one selected output column.</summary>
        Task AddFieldOutputAsync(Guid scheduleId, string reportKey, string field, string label, int fieldOrder, int sortOrder);

        /// <summary>_x_ReportClientScheduleRecipient @Type='Add' — append a user OR role OR literal e-mail.</summary>
        Task AddRecipientAsync(Guid scheduleId, Guid? userId, Guid? roleId, string? email, string tenantId);

        /// <summary>_x_ReportClientScheduleRead @Type='List' — every schedule for a report.</summary>
        Task<List<ScheduleRow>> ListAsync(Guid reportId, string tenantId);

        /// <summary>_x_ReportClientScheduleRead @Type='Read' — one schedule header.</summary>
        Task<ScheduleRow?> ReadAsync(Guid scheduleId, string tenantId);

        /// <summary>_x_ReportClientScheduleRecipient @Type='ListUsers' — the tenant's users, flagged assigned.</summary>
        Task<List<RecipientUserRow>> ListRecipientUsersAsync(Guid scheduleId, string tenantId);

        /// <summary>_x_ReportClientScheduleRecipient @Type='ListRoles' — the tenant's roles, flagged assigned.</summary>
        Task<List<RecipientRoleRow>> ListRecipientRolesAsync(Guid scheduleId, string tenantId);

        /// <summary>_x_ReportFieldOutputRead — a report's output columns; when a schedule id is supplied,
        /// each column is flagged isShow=1 if the schedule selected it (with its saved label/sort).</summary>
        Task<List<FieldOutputRow>> FieldOutputReadAsync(string reportKey, Guid? scheduleId, string tenantId);

        /// <summary>_x_ReportGenerateGetScheduleInfo — everything a background run needs: the header +
        /// stored proc, the saved criteria values, and the resolved recipient e-mail list.</summary>
        Task<ScheduleInfo?> GetScheduleInfoAsync(Guid scheduleId, string tenantId);

        /// <summary>_x_ReportGenerateSendToHistory — write one ReportRun row + its recipient rows.</summary>
        Task SendToHistoryAsync(string tenantId, string reportKey, bool isScheduled, string criteria,
            string? fieldOutput, int totalRecords, int runSeconds, string? ranBy, string? recipients);

        /// <summary>_x_ReportActivate — toggle a report's IsActive flag.</summary>
        Task ActivateReportAsync(Guid reportId, bool isActive, string tenantId);

        /// <summary>_x_ReportDelete — delete a report and its dependent rows.</summary>
        Task DeleteReportAsync(Guid reportId, string tenantId);
    }

    /// <summary>Upsert payload for _x_ReportClientSchedule.</summary>
    public record ScheduleHeader(
        Guid? Id, string TenantId, Guid? UserId, Guid ReportId, string Name, bool IsScheduled,
        string? MailSubject, string? MailBody, bool IsHideRecipients, string Frequency, int FrequencyWeekly,
        int TimeOfTheDay, DateOnly? ScheduleStartDate, int OutputFormat, string CronExpression);

    /// <summary>One row from _x_ReportClientScheduleRead (header + report joins).</summary>
    public record ScheduleRow(
        Guid ReportScheduleId, Guid ReportId, string ReportKey, string ReportName, string Name,
        bool IsScheduled, bool IsActive, string? MailSubject, string? MailBody, bool IsHideRecipients,
        string Frequency, int FrequencyWeekly, int TimeOfTheDay, DateTime? ScheduleStartDate,
        int OutputFormat, string CronExpression, string StoredProc);

    public record RecipientUserRow(Guid ReportScheduleId, Guid UserId, string UserName, bool IsAssigned, string? Email);
    public record RecipientRoleRow(Guid ReportScheduleId, Guid RoleId, string RoleName, bool IsAssigned);
    public record FieldOutputRow(int IsShow, string Field, string Label, int SortOrder, int FieldOrder);

    public record ScheduleInfo(ScheduleRow Header, Dictionary<string, string?> Criteria,
        List<FieldOutputRow> OutputFields, List<string> RecipientEmails);

    /// <summary>
    /// Cron generation ported verbatim from the reference (Part E): the schedule form captures a cadence,
    /// the server derives the 5-part cron. TimeOfTheDay = hour24*60 (minutes always :00). FrequencyWeekly
    /// is the day bitmask Sun=64..Sat=1; <see cref="DecodeWeeklyDays"/> maps it to cron DOW Sun=0..Sat=6.
    /// </summary>
    public static class ReportCron
    {
        private static readonly (int Bit, int Dow)[] WeekdayMap =
        [
            (64, 0), // Sunday
            (32, 1), // Monday
            (16, 2), // Tuesday
            (8, 3),  // Wednesday
            (4, 4),  // Thursday
            (2, 5),  // Friday
            (1, 6),  // Saturday
        ];

        public static string DecodeWeeklyDays(int mask)
        {
            var dows = WeekdayMap.Where(m => (mask & m.Bit) != 0).Select(m => m.Dow).OrderBy(d => d).ToList();
            return dows.Count == 0 ? "*" : string.Join(",", dows);
        }

        /// <summary>reference GenerateCronExpression.</summary>
        public static string Generate(string frequency, int frequencyWeekly, int timeOfTheDay, DateOnly? startDate)
        {
            var minute = timeOfTheDay % 60;
            var hour = timeOfTheDay / 60;
            var day = (startDate?.Day ?? 1).ToString();
            var month = (startDate?.Month ?? 1).ToString();
            return (frequency ?? "Daily").Trim().ToLowerInvariant() switch
            {
                "weekly" => $"{minute} {hour} * * {DecodeWeeklyDays(frequencyWeekly)}",
                "monthly" => $"{minute} {hour} {day} * *",
                "quarterly" => $"{minute} {hour} {day} 1,4,7,10 *",
                "yearly" => $"{minute} {hour} {day} {month} *",
                _ => $"{minute} {hour} * * *", // daily
            };
        }
    }
}
