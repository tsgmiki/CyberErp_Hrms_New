using System.Data;
using CyberErp.Hrms.App.Features.Core.Reports;
using CyberErp.Hrms.Inf.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Dapper implementation of <see cref="IReportScheduleStore"/> — one method per ported legacy
    /// procedure (reference _x_ReportClientSchedule*, _x_ReportFieldOutputRead,
    /// _x_ReportGenerateGetScheduleInfo, _x_ReportGenerateSendToHistory, _x_ReportActivate, _x_ReportDelete).
    /// Runs on the EF-owned connection (opened, never disposed here) exactly like <see cref="ReportExecutor"/>.
    /// </summary>
    public class ReportScheduleStore(HrmsDbContext dbContext, ITenantService tenantService) : IReportScheduleStore
    {
        private const string SchemaPrefix = "Core.";

        /// <summary>Empty ⇒ the ambient (request) tenant; a non-empty value ⇒ an explicit tenant, used by
        /// background Hangfire runs that have no ambient context.</summary>
        private string Resolve(string tenantId) =>
            string.IsNullOrEmpty(tenantId) ? tenantService.GetCurrentTenantId() ?? string.Empty : tenantId;

        private async Task<System.Data.Common.DbConnection> ConnAsync()
        {
            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await dbContext.Database.OpenConnectionAsync();
            return conn;
        }

        public async Task<Guid> UpsertHeaderAsync(ScheduleHeader h)
        {
            var conn = await ConnAsync();
            var p = new DynamicParameters();
            p.Add("ReportScheduleId", h.Id == Guid.Empty ? (Guid?)null : h.Id, DbType.Guid, ParameterDirection.InputOutput);
            p.Add("TenantId", Resolve(h.TenantId));
            p.Add("UserId", h.UserId);
            p.Add("ReportId", h.ReportId);
            p.Add("Name", h.Name);
            p.Add("IsScheduled", h.IsScheduled);
            p.Add("MailSubject", h.MailSubject);
            p.Add("MailBody", h.MailBody);
            p.Add("IsHideRecipients", h.IsHideRecipients);
            p.Add("Frequency", h.Frequency);
            p.Add("FrequencyWeekly", h.FrequencyWeekly);
            p.Add("TimeOfTheDay", h.TimeOfTheDay);
            p.Add("ScheduleStartDate", h.ScheduleStartDate is { } d ? d.ToDateTime(TimeOnly.MinValue) : (DateTime?)null);
            p.Add("OutputFormat", h.OutputFormat);
            p.Add("CronExpression", h.CronExpression);
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportClientSchedule", p, commandType: CommandType.StoredProcedure);
            return p.Get<Guid>("ReportScheduleId");
        }

        public async Task DeleteAsync(Guid scheduleId, bool modifyOnly)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportClientScheduleDelete",
                new { ReportScheduleId = scheduleId, IsModifyOnly = modifyOnly ? 1 : 0 },
                commandType: CommandType.StoredProcedure);
        }

        public async Task EnableAsync(Guid scheduleId, bool enabled)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportClientScheduleEnable",
                new { ReportScheduleId = scheduleId, Enabled = enabled ? 1 : 0 },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddFieldValueAsync(Guid scheduleId, string reportKey, string field, string? value)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportClientScheduleFieldValue",
                new { ReportScheduleId = scheduleId, ReportKey = reportKey, Field = field, Value = value },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddFieldOutputAsync(Guid scheduleId, string reportKey, string field, string label, int fieldOrder, int sortOrder)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportClientScheduleFieldOutput",
                new { ReportScheduleId = scheduleId, ReportKey = reportKey, Field = field, Label = label, FieldOrder = fieldOrder, SortOrder = sortOrder },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddRecipientAsync(Guid scheduleId, Guid? userId, Guid? roleId, string? email, string tenantId)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportClientScheduleRecipient",
                new { Type = "Add", ReportScheduleId = scheduleId, UserId = userId, RoleId = roleId, Email = email, TenantId = Resolve(tenantId) },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<ScheduleRow>> ListAsync(Guid reportId, string tenantId)
        {
            var conn = await ConnAsync();
            var rows = await conn.QueryAsync<ScheduleRow>(SchemaPrefix + "hrms_ReportClientScheduleRead",
                new { Type = "List", Id = reportId, TenantId = Resolve(tenantId) }, commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        public async Task<ScheduleRow?> ReadAsync(Guid scheduleId, string tenantId)
        {
            var conn = await ConnAsync();
            var rows = await conn.QueryAsync<ScheduleRow>(SchemaPrefix + "hrms_ReportClientScheduleRead",
                new { Type = "Read", Id = scheduleId, TenantId = Resolve(tenantId) }, commandType: CommandType.StoredProcedure);
            return rows.FirstOrDefault();
        }

        public async Task<List<RecipientUserRow>> ListRecipientUsersAsync(Guid scheduleId, string tenantId)
        {
            var conn = await ConnAsync();
            var rows = await conn.QueryAsync<RecipientUserRow>(SchemaPrefix + "hrms_ReportClientScheduleRecipient",
                new { Type = "ListUsers", ReportScheduleId = scheduleId, UserId = (Guid?)null, RoleId = (Guid?)null, Email = (string?)null, TenantId = Resolve(tenantId) },
                commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        public async Task<List<RecipientRoleRow>> ListRecipientRolesAsync(Guid scheduleId, string tenantId)
        {
            var conn = await ConnAsync();
            var rows = await conn.QueryAsync<RecipientRoleRow>(SchemaPrefix + "hrms_ReportClientScheduleRecipient",
                new { Type = "ListRoles", ReportScheduleId = scheduleId, UserId = (Guid?)null, RoleId = (Guid?)null, Email = (string?)null, TenantId = Resolve(tenantId) },
                commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        public async Task<List<FieldOutputRow>> FieldOutputReadAsync(string reportKey, Guid? scheduleId, string tenantId)
        {
            var conn = await ConnAsync();
            var rows = await conn.QueryAsync<FieldOutputRow>(SchemaPrefix + "hrms_ReportFieldOutputRead",
                new { ReportKey = reportKey, ReportScheduleId = scheduleId, TenantId = Resolve(tenantId) },
                commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }

        public async Task<ScheduleInfo?> GetScheduleInfoAsync(Guid scheduleId, string tenantId)
        {
            var conn = await ConnAsync();
            using var multi = await conn.QueryMultipleAsync(SchemaPrefix + "hrms_ReportGenerateGetScheduleInfo",
                new { TenantId = Resolve(tenantId), ReportScheduleId = scheduleId }, commandType: CommandType.StoredProcedure);

            var header = (await multi.ReadAsync<ScheduleRow>()).FirstOrDefault();
            if (header is null) return null;
            var criteria = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (IDictionary<string, object?> r in await multi.ReadAsync())
            {
                var field = r.TryGetValue("Field", out var f) ? f?.ToString() : null;
                if (string.IsNullOrEmpty(field)) continue;
                criteria[field] = r.TryGetValue("Value", out var v) ? v?.ToString() : null;
            }
            var emails = (await multi.ReadAsync<string>()).Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
            var outputs = multi.IsConsumed ? [] : (await multi.ReadAsync<FieldOutputRow>()).ToList();
            return new ScheduleInfo(header, criteria, outputs, emails);
        }

        public async Task SendToHistoryAsync(string tenantId, string reportKey, bool isScheduled, string criteria,
            string? fieldOutput, int totalRecords, int runSeconds, string? ranBy, string? recipients)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportGenerateSendToHistory", new
            {
                TenantId = Resolve(tenantId), ReportKey = reportKey, IsScheduled = isScheduled, Criteria = criteria,
                FieldOutput = fieldOutput, TotalRecords = totalRecords, RunSeconds = runSeconds,
                RanBy = ranBy, Recipients = recipients
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task ActivateReportAsync(Guid reportId, bool isActive, string tenantId)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportActivate",
                new { ReportId = reportId, IsActive = isActive, TenantId = Resolve(tenantId) },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteReportAsync(Guid reportId, string tenantId)
        {
            var conn = await ConnAsync();
            await conn.ExecuteAsync(SchemaPrefix + "hrms_ReportDelete",
                new { ReportId = reportId, TenantId = Resolve(tenantId) },
                commandType: CommandType.StoredProcedure);
        }
    }
}
