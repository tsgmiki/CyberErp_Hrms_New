using System.Text;
using System.Text.Json;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Reports
{
    /// <summary>Hangfire port: registers/removes the recurring job for a schedule (implemented in Inf).</summary>
    public interface IReportJobScheduler
    {
        void Register(Guid scheduleId, string cronExpression);
        void Remove(Guid scheduleId);
    }

    /// <summary>One selected output column posted from the "Fields" popup on the schedule form.</summary>
    public class ScheduleOutputFieldDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int FieldOrder { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// The schedule form payload — the reference cadence model (reference _ScheduleForm.cshtml): a
    /// Frequency + weekly-day bitmask + hour-of-day, from which the server derives the cron; recipients
    /// as users/roles/e-mails; the current criteria values; and the chosen output columns.
    /// </summary>
    public class SaveReportScheduleDto
    {
        public Guid? Id { get; set; }
        public string ReportKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsScheduled { get; set; } = true;
        public string? MailSubject { get; set; }
        public string? MailBody { get; set; }
        public bool IsHideRecipients { get; set; }
        /// <summary>Daily / Weekly / Monthly / Quarterly / Yearly.</summary>
        public string Frequency { get; set; } = "Daily";
        /// <summary>Day bitmask for Weekly (Sun=64..Sat=1).</summary>
        public int FrequencyWeekly { get; set; }
        /// <summary>Hour of day 0–23 (server stores TimeOfTheDay = Hour24*60).</summary>
        public int Hour24 { get; set; }
        /// <summary>Start date (yyyy-MM-dd) — supplies day/month for Monthly/Quarterly/Yearly.</summary>
        public string? ScheduleStartDate { get; set; }
        /// <summary>1=CSV, 2=Excel, 3=PDF.</summary>
        public int OutputFormat { get; set; } = 1;
        public List<Guid> RecipientUserIds { get; set; } = [];
        public List<Guid> RecipientRoleIds { get; set; } = [];
        public List<string> RecipientEmails { get; set; } = [];
        public Dictionary<string, string?> Values { get; set; } = [];
        public List<ScheduleOutputFieldDto>? OutputFields { get; set; }
    }

    /// <summary>A schedule row for the "Schedule" tab grid.</summary>
    public class ReportScheduleListDto
    {
        public Guid Id { get; set; }
        public string ReportKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public int TimeOfTheDay { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsScheduled { get; set; }
    }

    /// <summary>Full detail for editing a schedule (hydrates the form).</summary>
    public class ReportScheduleDetailDto
    {
        public Guid Id { get; set; }
        public string ReportKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsScheduled { get; set; }
        public string? MailSubject { get; set; }
        public string? MailBody { get; set; }
        public bool IsHideRecipients { get; set; }
        public string Frequency { get; set; } = "Daily";
        public int FrequencyWeekly { get; set; }
        public int Hour24 { get; set; }
        public string? ScheduleStartDate { get; set; }
        public int OutputFormat { get; set; }
        public List<Guid> RecipientUserIds { get; set; } = [];
        public List<Guid> RecipientRoleIds { get; set; } = [];
        public List<string> RecipientEmails { get; set; } = [];
        public Dictionary<string, string?> Values { get; set; } = [];
        public List<ScheduleOutputFieldDto> OutputFields { get; set; } = [];
    }

    public class ScheduleRunResultDto
    {
        public int Rows { get; set; }
        public string Recipients { get; set; } = string.Empty;
        public bool Sent { get; set; }
    }

    /// <summary>
    /// Ad-hoc e-mail payload (reference _SendReportByEmail.cshtml + SendGeneratedReportByEmail): the report
    /// is dispatched to any mix of specific users, whole roles (expanded to their members), and literal
    /// e-mails; <see cref="IsCc"/> adds the sender (reference "CC Me"); output as CSV or Tab.
    /// </summary>
    public class EmailReportDto : GenerateReportDto
    {
        public List<Guid> RecipientUserIds { get; set; } = [];
        public List<Guid> RecipientRoleIds { get; set; } = [];
        public List<string> RecipientEmails { get; set; } = [];
        /// <summary>reference "CC Me" — also send to the current user (default on).</summary>
        public bool IsCc { get; set; } = true;
        /// <summary>reference "Hide all Recipient" — deliver one message per recipient.</summary>
        public bool IsHideRecipients { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        /// <summary>1=CSV (default), 0=Tab (reference emailoutputformat).</summary>
        public int OutputFormat { get; set; } = 1;
    }

    /// <summary>Ad-hoc e-mail of a generated report (reference SendGeneratedReportByEmail).</summary>
    public interface IEmailGeneratedReport { Task<ScheduleRunResultDto> SendAsync(EmailReportDto dto); }

    public interface ISaveReportSchedule { Task<Guid> SaveAsync(SaveReportScheduleDto dto); }
    public interface IGetReportSchedules { Task<List<ReportScheduleListDto>> GetAsync(string reportKey); }
    public interface IGetReportScheduleDetail { Task<ReportScheduleDetailDto> GetAsync(Guid id); }
    public interface IDeleteReportSchedule { Task DeleteAsync(Guid id); }
    public interface ISetReportScheduleEnabled { Task SetAsync(Guid id, bool enabled); }
    /// <summary>Executes a schedule NOW — called by the Hangfire recurring job and the run-now endpoint.</summary>
    public interface IRunReportSchedule { Task<ScheduleRunResultDto> RunAsync(Guid scheduleId); }

    /// <summary>
    /// Persists a schedule the reference way: derive the cron from the cadence, upsert the header via
    /// _x_ReportClientSchedule, then rewrite its three child tables (criteria values, output columns,
    /// recipients) through their dedicated SPs. Registers/removes the Hangfire job to match IsScheduled.
    /// </summary>
    public class SaveReportSchedule(
        IRepository<Report> reports,
        IReportScheduleStore store,
        ICurrentUserService currentUser,
        IReportJobScheduler jobScheduler,
        ILogger<SaveReportSchedule> logger) : ISaveReportSchedule
    {
        public async Task<Guid> SaveAsync(SaveReportScheduleDto dto)
        {
            var report = await reports.GetAll().FirstOrDefaultAsync(r => r.ReportKey == dto.ReportKey && r.IsActive)
                ?? throw new NotFoundException(nameof(Report), dto.ReportKey);
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("name", "Schedule name is required.");
            if (dto.RecipientUserIds.Count == 0 && dto.RecipientRoleIds.Count == 0 && dto.RecipientEmails.Count == 0)
                throw new ValidationException("recipients", "Select at least one recipient (user, role, or e-mail).");
            if (dto.Hour24 is < 0 or > 23)
                throw new ValidationException("hour", "Hour must be between 0 and 23.");

            var tenantId = string.Empty; // empty ⇒ the store resolves the ambient (request) tenant
            var startDate = ParseDate(dto.ScheduleStartDate);
            var timeOfDay = dto.Hour24 * 60;
            var cron = ReportCron.Generate(dto.Frequency, dto.FrequencyWeekly, timeOfDay, startDate);

            var id = await store.UpsertHeaderAsync(new ScheduleHeader(
                dto.Id, tenantId, currentUser.GetCurrentUserId(), report.Id, dto.Name.Trim(), dto.IsScheduled,
                dto.MailSubject, dto.MailBody, dto.IsHideRecipients, dto.Frequency, dto.FrequencyWeekly,
                timeOfDay, startDate, dto.OutputFormat, cron));

            // Re-save = clear then re-insert children (reference _x_ReportClientScheduleDelete pisModifyOnly=1).
            await store.DeleteAsync(id, modifyOnly: true);

            foreach (var (field, value) in dto.Values)
                await store.AddFieldValueAsync(id, report.ReportKey, field, value);

            foreach (var o in dto.OutputFields ?? [])
                await store.AddFieldOutputAsync(id, report.ReportKey, o.Field, o.Label, o.FieldOrder, o.SortOrder);

            foreach (var userId in dto.RecipientUserIds.Distinct())
                await store.AddRecipientAsync(id, userId, null, null, tenantId);
            foreach (var roleId in dto.RecipientRoleIds.Distinct())
                await store.AddRecipientAsync(id, null, roleId, null, tenantId);
            foreach (var email in dto.RecipientEmails.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct())
                await store.AddRecipientAsync(id, null, null, email.Trim(), tenantId);

            if (dto.IsScheduled) jobScheduler.Register(id, cron);
            else jobScheduler.Remove(id);

            logger.LogInformation("Report schedule {Id} ({Name}) saved; cron '{Cron}', scheduled={Scheduled}",
                id, dto.Name, cron, dto.IsScheduled);
            return id;
        }

        private static DateOnly? ParseDate(string? s) =>
            DateOnly.TryParse(s, out var d) ? d : null;
    }

    public class GetReportSchedules(IReportScheduleStore store, IRepository<Report> reports) : IGetReportSchedules
    {
        public async Task<List<ReportScheduleListDto>> GetAsync(string reportKey)
        {
            var reportId = await reports.GetAll().Where(r => r.ReportKey == reportKey)
                .Select(r => (Guid?)r.Id).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Report), reportKey);
            var rows = await store.ListAsync(reportId, string.Empty);
            return rows.Select(s => new ReportScheduleListDto
            {
                Id = s.ReportScheduleId, ReportKey = s.ReportKey, Name = s.Name, Frequency = s.Frequency,
                CronExpression = s.CronExpression, TimeOfTheDay = s.TimeOfTheDay,
                ScheduleStartDate = s.ScheduleStartDate, IsActive = s.IsActive, IsScheduled = s.IsScheduled
            }).ToList();
        }
    }

    public class GetReportScheduleDetail(IReportScheduleStore store) : IGetReportScheduleDetail
    {
        public async Task<ReportScheduleDetailDto> GetAsync(Guid id)
        {
            var tenantId = string.Empty; // ambient tenant
            var head = await store.ReadAsync(id, tenantId)
                ?? throw new NotFoundException(nameof(ReportSchedule), id.ToString());
            var users = await store.ListRecipientUsersAsync(id, tenantId);
            var roles = await store.ListRecipientRolesAsync(id, tenantId);
            var info = await store.GetScheduleInfoAsync(id, tenantId);
            var outputs = await store.FieldOutputReadAsync(head.ReportKey, id, tenantId);

            return new ReportScheduleDetailDto
            {
                Id = head.ReportScheduleId, ReportKey = head.ReportKey, Name = head.Name,
                IsScheduled = head.IsScheduled, MailSubject = head.MailSubject, MailBody = head.MailBody,
                IsHideRecipients = head.IsHideRecipients, Frequency = head.Frequency,
                FrequencyWeekly = head.FrequencyWeekly, Hour24 = head.TimeOfTheDay / 60,
                ScheduleStartDate = head.ScheduleStartDate?.ToString("yyyy-MM-dd"), OutputFormat = head.OutputFormat,
                RecipientUserIds = users.Where(u => u.IsAssigned).Select(u => u.UserId).ToList(),
                RecipientRoleIds = roles.Where(r => r.IsAssigned).Select(r => r.RoleId).ToList(),
                RecipientEmails = info?.RecipientEmails
                    .Where(e => !users.Any(u => u.IsAssigned && string.Equals(u.Email, e, StringComparison.OrdinalIgnoreCase)))
                    .ToList() ?? [],
                Values = info?.Criteria ?? [],
                OutputFields = outputs.Where(o => o.IsShow == 1).Select(o => new ScheduleOutputFieldDto
                {
                    Field = o.Field, Label = o.Label, FieldOrder = o.FieldOrder, SortOrder = o.SortOrder
                }).ToList()
            };
        }
    }

    public class DeleteReportSchedule(IReportScheduleStore store, IReportJobScheduler jobScheduler) : IDeleteReportSchedule
    {
        public async Task DeleteAsync(Guid id)
        {
            await store.DeleteAsync(id, modifyOnly: false);
            jobScheduler.Remove(id);
        }
    }

    public class SetReportScheduleEnabled(IReportScheduleStore store, IReportJobScheduler jobScheduler) : ISetReportScheduleEnabled
    {
        public async Task SetAsync(Guid id, bool enabled)
        {
            await store.EnableAsync(id, enabled);
            var head = await store.ReadAsync(id, string.Empty);
            if (enabled && head is not null) jobScheduler.Register(id, head.CronExpression);
            else jobScheduler.Remove(id);
        }
    }

    /// <summary>Builds the report attachment (reference CsvGeneration / GetOutputFormat) — shared by
    /// schedule + ad-hoc e-mail. <paramref name="outputFormat"/>: 1=CSV, 0=Tab-separated.</summary>
    internal static class ReportCsv
    {
        /// <summary>PIVOT export: order the rows by the group-by columns so a scheduled CSV is grouped-sorted.</summary>
        internal static ReportResult SortByGroups(ReportResult result, string[] groupBy)
        {
            if (groupBy.Length == 0) return result;
            var sorted = result.Rows
                .OrderBy(r => string.Join('', groupBy.Select(g =>
                    r.TryGetValue(g, out var v) && v is not null ? v.ToString() : string.Empty)), StringComparer.OrdinalIgnoreCase)
                .ToList();
            return result with { Rows = sorted };
        }

        internal static EmailAttachment Build(string reportName, ReportResult result, int outputFormat = 1)
        {
            var tab = outputFormat == 0;
            var sep = tab ? '\t' : ',';
            var sb = new StringBuilder();
            string Esc(string v) => tab
                ? v.Replace("\t", " ").Replace("\n", " ")
                : (v.Contains('"') || v.Contains(',') || v.Contains('\n') ? $"\"{v.Replace("\"", "\"\"")}\"" : v);
            sb.AppendLine(string.Join(sep, result.Columns.Select(c => Esc(c.Label))));
            foreach (var row in result.Rows)
                sb.AppendLine(string.Join(sep, result.Columns.Select(c =>
                    Esc(row.TryGetValue(c.Field, out var v) && v is not null ? v.ToString() ?? "" : ""))));
            var ext = tab ? "tsv" : "csv";
            var mime = tab ? "text/tab-separated-values" : "text/csv";
            return new EmailAttachment($"{reportName}.{ext}", Encoding.UTF8.GetBytes(sb.ToString()), mime);
        }
    }

    public class EmailGeneratedReport(
        IRepository<Report> reports,
        IReportExecutor executor,
        IReportScheduleStore store,
        IRepository<Dom.Entities.Core.User> users,
        IRepository<UserRole> userRoles,
        ICurrentUserService currentUser,
        IEmailService emailService,
        ILogger<EmailGeneratedReport> logger) : IEmailGeneratedReport
    {
        public async Task<ScheduleRunResultDto> SendAsync(EmailReportDto dto)
        {
            var report = await reports.GetAll().FirstOrDefaultAsync(r => r.ReportKey == dto.ReportKey && r.IsActive)
                ?? throw new NotFoundException(nameof(Report), dto.ReportKey);

            // Resolve recipients exactly like the reference: specific users + every member of the chosen
            // roles + literal e-mails (+ the sender when CC Me is on), de-duplicated.
            var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void AddAll(IEnumerable<string?> src) { foreach (var e in src) if (!string.IsNullOrWhiteSpace(e)) emails.Add(e.Trim()); }

            if (dto.RecipientUserIds.Count > 0)
                AddAll(await users.GetAll().Where(u => dto.RecipientUserIds.Contains(u.Id)).Select(u => u.Email).ToListAsync());
            if (dto.RecipientRoleIds.Count > 0)
            {
                var memberIds = await userRoles.GetAll().Where(ur => dto.RecipientRoleIds.Contains(ur.RoleId))
                    .Select(ur => ur.UserId).Distinct().ToListAsync();
                AddAll(await users.GetAll().Where(u => memberIds.Contains(u.Id)).Select(u => u.Email).ToListAsync());
            }
            AddAll(dto.RecipientEmails);
            if (dto.IsCc && currentUser.GetCurrentUserId() is Guid me)
                AddAll(await users.GetAll().Where(u => u.Id == me).Select(u => u.Email).ToListAsync());

            if (emails.Count == 0)
                throw new ValidationException("recipients", "Select at least one user, role, or e-mail address.");

            var criteria = JsonSerializer.Serialize(dto.Values ?? []);
            var outputFieldsJson = dto.OutputFields is { Count: > 0 } ? JsonSerializer.Serialize(dto.OutputFields) : null;
            var result = await executor.ExecuteAsync(report.StoredProc, report.ReportKey, criteria, outputFieldsJson);

            var attachment = ReportCsv.Build(report.ReportName, result, dto.OutputFormat);
            var subject = string.IsNullOrWhiteSpace(dto.Subject) ? $"Report: {report.ReportName}" : dto.Subject!;
            var body = string.IsNullOrWhiteSpace(dto.Body)
                ? $"Attached: {report.ReportName} ({result.Rows.Count} rows), generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC."
                : dto.Body!;
            var recipients = emails.ToList();
            // IsHideRecipients ⇒ one message per recipient (BCC-style); otherwise still per-recipient send
            // through the single-recipient IEmailService — same delivery, no cross-exposure.
            foreach (var to in recipients)
                await emailService.SendAsync(to, subject, body, [attachment]);

            await store.SendToHistoryAsync(string.Empty, report.ReportKey, isScheduled: false, criteria,
                outputFieldsJson, result.Rows.Count, 0, currentUser.GetCurrentUserName(), string.Join(";", recipients));

            logger.LogInformation("Report {Key} e-mailed to {N} recipient(s) ({U} user(s), {R} role(s))",
                report.ReportKey, recipients.Count, dto.RecipientUserIds.Count, dto.RecipientRoleIds.Count);
            return new ScheduleRunResultDto { Rows = result.Rows.Count, Recipients = string.Join("; ", recipients), Sent = true };
        }
    }

    /// <summary>
    /// The delivery pipeline (reference BaseReport.GenerateAndSendReportAsync): runs OUTSIDE an HTTP
    /// request (Hangfire), so the tenant comes from the SCHEDULE ROW, not ambient context. It reads the
    /// schedule's stored proc + criteria + resolved recipients via _x_ReportGenerateGetScheduleInfo,
    /// runs the report with the explicit-tenant executor, e-mails the CSV, and records the run.
    /// </summary>
    public class RunReportSchedule(
        IRepository<ReportSchedule> schedules,
        IReportScheduleStore store,
        IReportExecutor executor,
        IEmailService emailService,
        ILogger<RunReportSchedule> logger) : IRunReportSchedule
    {
        public async Task<ScheduleRunResultDto> RunAsync(Guid scheduleId)
        {
            // Discover the owning tenant from the row (no ambient context in a background job).
            var tenantId = await schedules.GetAllWithoutTenantFilter()
                .Where(s => s.Id == scheduleId).Select(s => s.TenantId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(ReportSchedule), scheduleId.ToString());

            var info = await store.GetScheduleInfoAsync(scheduleId, tenantId)
                ?? throw new NotFoundException(nameof(ReportSchedule), scheduleId.ToString());
            if (!info.Header.IsActive || !info.Header.IsScheduled)
            {
                logger.LogInformation("Schedule {Id} is disabled — skipping delivery.", scheduleId);
                return new ScheduleRunResultDto { Rows = 0, Recipients = string.Empty, Sent = false };
            }

            // Resolve any DYNAMIC date criteria (e.g. "StartOfMonth") to a concrete date AS OF NOW, so a
            // recurring schedule always covers the intended moving window (reference _x_ReportFieldValues).
            var resolvedCriteria = DynamicDate.ResolveCriteria(info.Criteria, DateOnly.FromDateTime(DateTime.UtcNow));
            var criteria = JsonSerializer.Serialize(resolvedCriteria);
            var outputFieldsJson = info.OutputFields is { Count: > 0 }
                ? JsonSerializer.Serialize(info.OutputFields.OrderBy(o => o.FieldOrder)
                    .Select((o, i) => new ReportOutputFieldDto { Field = o.Field, Label = o.Label, Order = i + 1, SortOrder = o.SortOrder }))
                : null;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await executor.ExecuteForTenantAsync(tenantId, info.Header.StoredProc,
                info.Header.ReportKey, criteria, outputFieldsJson);
            sw.Stop();

            // PIVOT: a schedule saves its grouping as a reserved "__groupBy" criteria value → order the
            // exported rows by those columns so the CSV is grouped-sorted.
            var groupBy = info.Criteria.TryGetValue("__groupBy", out var gbv) && !string.IsNullOrWhiteSpace(gbv)
                ? gbv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [];
            var attachment = ReportCsv.Build(info.Header.ReportName, ReportCsv.SortByGroups(result, groupBy), info.Header.OutputFormat);
            var recipients = info.RecipientEmails.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
            foreach (var to in recipients)
                await emailService.SendAsync(to,
                    string.IsNullOrWhiteSpace(info.Header.MailSubject) ? $"Scheduled report: {info.Header.ReportName}" : info.Header.MailSubject!,
                    string.IsNullOrWhiteSpace(info.Header.MailBody)
                        ? $"Attached: {info.Header.ReportName} ({result.Rows.Count} rows), generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC by schedule '{info.Header.Name}'."
                        : info.Header.MailBody!,
                    [attachment]);

            await store.SendToHistoryAsync(tenantId, info.Header.ReportKey, isScheduled: true, criteria,
                outputFieldsJson, result.Rows.Count, (int)(sw.ElapsedMilliseconds / 1000),
                $"schedule:{info.Header.Name}", string.Join(";", recipients));

            logger.LogInformation("Schedule {Id} delivered {Rows} rows to {N} recipient(s)", scheduleId, result.Rows.Count, recipients.Count);
            return new ScheduleRunResultDto { Rows = result.Rows.Count, Recipients = string.Join(";", recipients), Sent = recipients.Count > 0 };
        }
    }
}
