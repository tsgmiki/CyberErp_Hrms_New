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
using Microsoft.Extensions.Logging;

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
    /// The master lookup SP (Hrms.ReportFieldValues) feeds dropdown/radio options.
    /// </summary>
    public partial class ReportExecutor(
        HrmsDbContext dbContext,
        ITenantService tenantService,
        ICurrentUserService currentUser) : IReportExecutor
    {
        private const string LookupProc = "Hrms.ReportFieldValues";

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
    /// <remarks>
    /// <para>⚠️ The cron is registered against a real zone, NOT UTC. The schedule form asks for an
    /// hour of day and the user means their own clock; without a zone every schedule fired at that
    /// hour UTC (logic §12.92).</para>
    ///
    /// <para>⚠️ The zone comes from THE SCHEDULE, falling back to the organisation default only when
    /// the schedule does not name one. Deriving it from the caller instead would re-time the schedule
    /// every time somebody else re-saved or re-enabled it (logic §12.93).</para>
    /// </remarks>
    public class ReportJobScheduler(
        IRecurringJobManager jobs,
        IJobTimeZone timeZone,
        ILogger<ReportJobScheduler> logger) : IReportJobScheduler
    {
        private static string JobId(Guid id) => $"report-schedule:{id}";

        public void Register(Guid scheduleId, string cronExpression, string? timeZoneId) =>
            jobs.AddOrUpdate<IRunReportSchedule>(JobId(scheduleId),
                r => r.RunAsync(scheduleId), cronExpression,
                new RecurringJobOptions { TimeZone = Resolve(scheduleId, timeZoneId) });

        public void Remove(Guid scheduleId) => jobs.RemoveIfExists(JobId(scheduleId));

        /// <summary>
        /// The schedule's own zone, or the organisation default when it does not name one.
        /// </summary>
        /// <remarks>
        /// ⚠️ Falls back to the ORGANISATION DEFAULT and not to UTC. A stored id can stop resolving —
        /// the tzdb drops and renames zones, and a server rebuilt without ICU resolves no IANA id at
        /// all — and silently dropping such a schedule to UTC would move it by hours without anyone
        /// touching it. The save path validates the id, so reaching the warning means the world
        /// changed underneath a schedule that was already saved.
        /// </remarks>
        private TimeZoneInfo Resolve(Guid scheduleId, string? timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId)) return timeZone.Zone;
            if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var zone)) return zone;

            logger.LogWarning(
                "Report schedule {ScheduleId} names time zone '{TimeZoneId}', which this server no longer " +
                "recognises — falling back to {Fallback}. Re-save the schedule to pick a valid zone.",
                scheduleId, timeZoneId, timeZone.Zone.Id);
            return timeZone.Zone;
        }
    }

    /// <summary>
    /// The zones offered by the schedule form's picker.
    /// </summary>
    /// <remarks>
    /// <para>Built from <see cref="TimeZoneInfo.GetSystemTimeZones"/> so the list is exactly what this
    /// server can actually resolve — offering a zone the save path would then reject is worse than
    /// offering a short list.</para>
    ///
    /// <para>⚠️ NORMALISED TO IANA IDS. <c>GetSystemTimeZones</c> returns WINDOWS ids on Windows
    /// ("E. Africa Standard Time") and IANA ids on Linux ("Africa/Nairobi"), so an un-normalised list
    /// would offer a different vocabulary depending on the host, store whichever the user happened to
    /// click, and fail to recognise its own configured default — which is exactly what it did on the
    /// first run of this code: not one of the 141 zones matched "Africa/Nairobi" (logic §12.93).</para>
    /// </remarks>
    public class GetSchedulingTimeZones(IJobTimeZone timeZone) : IGetSchedulingTimeZones
    {
        /// <summary>
        /// The IANA id for a zone when this platform can give one, else the id unchanged.
        /// </summary>
        /// <remarks>
        /// IANA is the portable form — it resolves on both Windows and Linux under ICU, while a
        /// Windows id is meaningless off Windows. Storing the portable one keeps a schedule valid if
        /// the application is ever rehosted.
        /// </remarks>
        private static string PreferIana(string id) =>
            TimeZoneInfo.TryConvertWindowsIdToIanaId(id, out var iana) ? iana : id;

        public List<TimeZoneOptionDto> Get()
        {
            var defaultId = PreferIana(timeZone.Zone.Id);

            return [.. TimeZoneInfo.GetSystemTimeZones()
                .Select(z => new { Id = PreferIana(z.Id), z.DisplayName, z.BaseUtcOffset })
                // Several Windows zones can normalise onto one IANA id; the picker shows each once.
                .GroupBy(z => z.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .Select(z => new TimeZoneOptionDto
                {
                    Id = z.Id,
                    Label = z.DisplayName,
                    Offset = (z.BaseUtcOffset < TimeSpan.Zero ? "-" : "+") + z.BaseUtcOffset.Duration().ToString(@"hh\:mm"),
                    IsDefault = string.Equals(z.Id, defaultId, StringComparison.OrdinalIgnoreCase)
                })
                .OrderBy(z => z.Offset, StringComparer.Ordinal)
                .ThenBy(z => z.Id, StringComparer.OrdinalIgnoreCase)];
        }
    }
}
