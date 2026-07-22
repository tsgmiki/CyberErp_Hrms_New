namespace CyberErp.Hrms.App.Features.Core.Reports
{
    /// <summary>One result column as declared BY THE STORED PROCEDURE (its first result set).</summary>
    public record ReportColumn(string Field, string Label, string Type, int? Width, string? LinkPage, string? LinkPageValue);

    /// <summary>One dropdown/radio option produced by the master lookup procedure.</summary>
    public record ReportLookupOption(string Value, string Label);

    /// <summary>A generated report: SP-declared columns + dynamic rows. A pivot SP may return an
    /// optional 3rd result set of per-group SUBTOTALS (one dict per group: group column values +
    /// GroupCount + numeric totals); flat reports leave it null.</summary>
    public record ReportResult(
        IReadOnlyList<ReportColumn> Columns,
        IReadOnlyList<Dictionary<string, object?>> Rows,
        IReadOnlyList<Dictionary<string, object?>>? Summaries = null);

    /// <summary>
    /// Infrastructure port for the generic report engine (ported from the reference APSmart module).
    /// Implementations execute the report's OWN stored procedure via parameterized ADO (Dapper) —
    /// result set 1 = column metadata, result set 2 = data rows — and the master lookup procedure
    /// (Core.hrms_ReportFieldValues) for dropdown options. Ambient scope (@TenantId, @BranchId,
    /// @UserId) is injected server-side on every call; user filter values travel as ONE bound
    /// @Criteria JSON parameter parsed inside the procedure, so no SQL is built from user input.
    /// </summary>
    public interface IReportExecutor
    {
        /// <param name="outputFieldsJson">JSON array of the user's chosen output columns (the port of
        /// the reference's pReportFieldOutput) — null/empty = the SP returns its full default set.</param>
        Task<ReportResult> ExecuteAsync(string storedProc, string reportKey, string criteriaJson,
            string? outputFieldsJson = null, int timeoutSeconds = 45);
        Task<List<ReportLookupOption>> GetFieldValuesAsync(string reportKey, string field, string? dependency, string? search);
        /// <summary>Whether the (validated) procedure name exists in the database — checked at registry save.</summary>
        Task<bool> StoredProcedureExistsAsync(string storedProc);
        /// <summary>Background (Hangfire) execution: no HTTP context, so the tenant is EXPLICIT.</summary>
        Task<ReportResult> ExecuteForTenantAsync(string tenantId, string storedProc, string reportKey,
            string criteriaJson, string? outputFieldsJson = null, int timeoutSeconds = 120);
    }
}
