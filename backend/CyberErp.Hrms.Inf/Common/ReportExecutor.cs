using System.Data;
using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Reports;
using CyberErp.Hrms.Dom.Entities.Core;
using CyberErp.Hrms.Inf.Models;
using Dapper;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Dapper-based implementation of the generic report engine's execution port (ported from the
    /// reference APSmart module, which ran MySQL SPs the same way). Contract with every report SP:
    ///   EXEC dbo_or_Core.&lt;proc&gt; @TenantId, @BranchId, @UserId, @ReportKey, @Criteria
    ///   → result set 1 = column metadata (Field, Label, Type, Width, LinkPage, LinkPageValue)
    ///   → result set 2 = the data rows (arbitrary columns)
    /// Ambient scope is injected here from the authenticated context — never from the client — and
    /// the user's filters travel as ONE bound @Criteria JSON string parsed inside the procedure.
    /// The master lookup SP (Core.hrms_ReportFieldValues) feeds dropdown/radio options.
    /// </summary>
    public partial class ReportExecutor(
        HrmsDbContext dbContext,
        ITenantService tenantService,
        ICurrentUserService currentUser) : IReportExecutor
    {
        private const string LookupProc = "Core.hrms_ReportFieldValues";

        // Plain (optionally schema-qualified / bracketed) identifier — defense in depth on top of
        // the fact that the proc name only ever comes from the tenant-scoped registry row.
        [GeneratedRegex(@"^\[?[A-Za-z0-9_]+\]?(\.\[?[A-Za-z0-9_]+\]?)?$")]
        private static partial Regex ProcNameRegex();

        public Task<ReportResult> ExecuteForTenantAsync(string tenantId, string storedProc, string reportKey,
            string criteriaJson, string? outputFieldsJson = null, int timeoutSeconds = 120)
        {
            var param = new DynamicParameters();
            param.Add("TenantId", tenantId);
            param.Add("BranchId", (Guid?)null);   // scheduled runs are tenant-wide
            param.Add("UserId", (Guid?)null);
            param.Add("ReportKey", reportKey);
            param.Add("Source", "Scheduled");     // reference pSource
            param.Add("Roles", (string?)null);
            return ExecuteCoreAsync(param, storedProc, reportKey, criteriaJson, outputFieldsJson, timeoutSeconds);
        }

        public async Task<ReportResult> ExecuteAsync(string storedProc, string reportKey, string criteriaJson,
            string? outputFieldsJson = null, int timeoutSeconds = 45)
        {
            GuardProcName(storedProc);
            if (!await StoredProcedureExistsAsync(storedProc))
                throw new InvalidOperationException($"Stored procedure '{storedProc}' does not exist.");

            var param = AmbientParameters(reportKey);
            param.Add("Source", "Generated");     // reference pSource
            // Reference pRoles: the caller's role ids, comma-separated, for role-aware SPs.
            var userId = currentUser.GetCurrentUserId();
            var roleIds = userId is null ? [] : await dbContext.Set<UserRole>()
                .Where(u => u.UserId == userId.Value).Select(u => u.RoleId).ToListAsync();
            param.Add("Roles", roleIds.Count > 0 ? string.Join(",", roleIds) : null);
            return await ExecuteCoreAsync(param, storedProc, reportKey, criteriaJson, outputFieldsJson, timeoutSeconds);
        }

        private async Task<ReportResult> ExecuteCoreAsync(DynamicParameters param, string storedProc,
            string reportKey, string criteriaJson, string? outputFieldsJson, int timeoutSeconds)
        {
            GuardProcName(storedProc);
            if (!await StoredProcedureExistsAsync(storedProc))
                throw new InvalidOperationException($"Stored procedure '{storedProc}' does not exist.");
            param.Add("Criteria", criteriaJson);
            param.Add("OutputFields", outputFieldsJson); // pReportFieldOutput port — part of the SP contract

            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await dbContext.Database.OpenConnectionAsync();

            try
            {
                using var multi = await conn.QueryMultipleAsync(
                    storedProc, param, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);

                // Result set 1 — the SP declares its own columns (dynamic schema).
                var columnRows = (await multi.ReadAsync()).Cast<IDictionary<string, object?>>();
                var columns = columnRows.Select(r => new ReportColumn(
                        Field: Str(r, "Field") ?? string.Empty,
                        Label: Str(r, "Label") ?? Str(r, "Field") ?? string.Empty,
                        Type: (Str(r, "Type") ?? "string").ToLowerInvariant(),
                        Width: Int(r, "Width"),
                        LinkPage: Str(r, "LinkPage"),
                        LinkPageValue: Str(r, "LinkPageValue")))
                    .Where(c => c.Field.Length > 0)
                    .ToList();

                // Result set 2 — the data, columns discovered at runtime.
                var rows = new List<Dictionary<string, object?>>();
                if (!multi.IsConsumed)
                {
                    foreach (IDictionary<string, object?> row in await multi.ReadAsync())
                        rows.Add(new Dictionary<string, object?>(row, StringComparer.OrdinalIgnoreCase));
                }

                // Result set 3 (optional) — a pivot SP's per-group subtotals (group values + aggregates).
                List<Dictionary<string, object?>>? summaries = null;
                if (!multi.IsConsumed)
                {
                    summaries = [];
                    foreach (IDictionary<string, object?> row in await multi.ReadAsync())
                        summaries.Add(new Dictionary<string, object?>(row, StringComparer.OrdinalIgnoreCase));
                }

                return new ReportResult(columns, rows, summaries);
            }
            catch (SqlException ex) when (ex.Number == -2) // command timeout
            {
                throw new TimeoutException($"Report '{reportKey}' timed out after {timeoutSeconds}s.", ex);
            }
        }

        public async Task<List<ReportLookupOption>> GetFieldValuesAsync(string reportKey, string field, string? dependency, string? search)
        {
            var param = AmbientParameters(reportKey);
            param.Add("Field", field);
            param.Add("Dependency", dependency);
            param.Add("Search", search);

            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await dbContext.Database.OpenConnectionAsync();

            var rows = await conn.QueryAsync(LookupProc, param, commandType: CommandType.StoredProcedure, commandTimeout: 30);
            return rows.Cast<IDictionary<string, object?>>()
                .Select(r => new ReportLookupOption(Str(r, "Value") ?? string.Empty, Str(r, "Label") ?? string.Empty))
                .Where(o => o.Value.Length > 0)
                .ToList();
        }

        public async Task<bool> StoredProcedureExistsAsync(string storedProc)
        {
            if (!ProcNameRegex().IsMatch(storedProc ?? string.Empty)) return false;
            var bare = storedProc!.Replace("[", "").Replace("]", "");

            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await dbContext.Database.OpenConnectionAsync();

            // OBJECT_ID with a bound name — no dynamic SQL.
            var id = await conn.ExecuteScalarAsync<int?>(
                "SELECT OBJECT_ID(@name, 'P')", new { name = bare });
            return id.HasValue;
        }

        /// <summary>@TenantId / @BranchId / @UserId — sourced from the authenticated context only.</summary>
        private DynamicParameters AmbientParameters(string reportKey)
        {
            var param = new DynamicParameters();
            param.Add("TenantId", tenantService.GetCurrentTenantId() ?? string.Empty);
            // Head Office (or unassigned) sees all branches → NULL lets the SP skip the branch filter.
            param.Add("BranchId", currentUser.IsHeadOffice() ? null : currentUser.GetCurrentBranchId());
            param.Add("UserId", currentUser.GetCurrentUserId());
            param.Add("ReportKey", reportKey);
            return param;
        }

        private static void GuardProcName(string storedProc)
        {
            if (!ProcNameRegex().IsMatch(storedProc ?? string.Empty))
                throw new InvalidOperationException("Invalid stored procedure name.");
        }

        private static string? Str(IDictionary<string, object?> row, string key) =>
            row.TryGetValue(key, out var v) && v is not null ? v.ToString() : null;

        private static int? Int(IDictionary<string, object?> row, string key) =>
            row.TryGetValue(key, out var v) && v is not null && int.TryParse(v.ToString(), out var i) ? i : null;
    }

    /// <summary>Hangfire recurring-job registration for report schedules (reference HangfireHelperMethod).</summary>
    public class ReportJobScheduler(IRecurringJobManager jobs) : IReportJobScheduler
    {
        private static string JobId(Guid id) => $"report-schedule:{id}";

        public void Register(Guid scheduleId, string cronExpression) =>
            jobs.AddOrUpdate<IRunReportSchedule>(JobId(scheduleId),
                r => r.RunAsync(scheduleId), cronExpression);

        public void Remove(Guid scheduleId) => jobs.RemoveIfExists(JobId(scheduleId));
    }
}
