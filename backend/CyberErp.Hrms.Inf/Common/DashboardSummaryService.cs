using System.Data;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Dashboard;
using CyberErp.Hrms.Inf.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Dapper implementation of <see cref="IDashboardSummary"/> — nine COUNT-shaped numbers in ONE
    /// round trip to SQL Server via <c>QueryMultipleAsync</c> (same technique, same ambient EF
    /// connection reuse, as <see cref="ReportExecutor"/>). Deliberately raw SQL rather than nine
    /// separate <c>IRepository&lt;T&gt;.GetAll().CountAsync()</c> calls: each of those would be its
    /// own SQL Server round trip, and — worse — is exactly what the dashboard used to do by fetching
    /// full paginated lists (<c>take=1</c>) just to read the <c>total</c> on the response envelope.
    ///
    /// Tenant/branch isolation is applied HERE, in C#, replicating <c>Repository.ApplyTenantFilter</c> /
    /// <c>ApplyBranchFilter</c> exactly (not delegated to the repository, since this bypasses it for
    /// performance) — see the per-table comments below for which rule applies to which table.
    /// </summary>
    /// <summary>Dapper maps this by constructor-parameter name against the "Status"/"Cnt" columns —
    /// a record, not a ValueTuple, since Dapper's positional-tuple deserialization isn't guaranteed
    /// across versions and this is the well-supported mapping path.</summary>
    file sealed record WorkflowStatusCount(string Status, int Cnt);

    public class DashboardSummaryService(
        HrmsDbContext dbContext,
        ITenantService tenantService,
        ICurrentUserService currentUser) : IDashboardSummary
    {
        /// <summary>Statutory retirement age — mirrors <c>GetUpcomingRetirements</c> exactly.</summary>
        private const int RetirementAgeYears = 60;

        // One statement per line, semicolon-separated — Dapper reads them back as ordered result
        // sets via QueryMultipleAsync. @BranchScopeId is NULL for Head Office / branch-unassigned
        // users (no filter, matches Repository's "unrestricted" fallback); otherwise it narrows every
        // branch-scoped table to that one branch. hrmsBranch is filtered by Id (a branch admin sees
        // only THEIR OWN branch row — Repository's special case for the Branch entity itself);
        // hrmsWorkflowInstance carries no BranchId at all (WorkflowInstance is not IBranchScoped), so
        // it is tenant-only, matching the repository's no-op branch filter for that entity.
        private const string Sql = """
            SELECT COUNT(*) FROM dbo.hrmsBranch
              WHERE TenantId = @TenantId AND (@BranchScopeId IS NULL OR Id = @BranchScopeId);

            SELECT COUNT(*) FROM dbo.hrmsOrganizationUnit
              WHERE TenantId = @TenantId AND (@BranchScopeId IS NULL OR BranchId = @BranchScopeId);

            SELECT COUNT(*) FROM dbo.hrmsPosition
              WHERE TenantId = @TenantId AND (@BranchScopeId IS NULL OR BranchId = @BranchScopeId);

            SELECT COUNT(*) FROM dbo.hrmsEmployee
              WHERE TenantId = @TenantId AND (@BranchScopeId IS NULL OR BranchId = @BranchScopeId);

            SELECT Status, COUNT(*) AS Cnt FROM dbo.hrmsWorkflowInstance
              WHERE TenantId = @TenantId
              GROUP BY Status;

            SELECT COUNT(*) FROM dbo.hrmsEmployee
              WHERE TenantId = @TenantId AND (@BranchScopeId IS NULL OR BranchId = @BranchScopeId)
                AND EmploymentStatus = 'Active' AND IsProbation = 1;

            SELECT COUNT(*) FROM dbo.hrmsEmployee
              WHERE TenantId = @TenantId AND (@BranchScopeId IS NULL OR BranchId = @BranchScopeId)
                AND EmploymentStatus = 'Active' AND DateOfBirth IS NOT NULL AND DateOfBirth < @RetirementThreshold;
            """;

        public async Task<DashboardSummaryDto> GetAsync()
        {
            var dto = new DashboardSummaryDto();

            var tenantId = tenantService.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId)) return dto; // no tenant context — matches Repository's guard

            // Repository.ApplyBranchFilter: Head Office, or a user with no branch assignment, is unrestricted.
            Guid? branchScopeId = currentUser.IsHeadOffice() ? null : currentUser.GetCurrentBranchId();

            // Retirement threshold, precomputed once (SARGABLE constant, no per-row date function) —
            // identical formula to GetUpcomingRetirements: "retires within a month" <=> DateOfBirth
            // < today + 1 month - 60 years.
            var retirementThreshold = DateTime.Today.AddMonths(1).AddYears(-RetirementAgeYears);

            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await dbContext.Database.OpenConnectionAsync();

            using var multi = await conn.QueryMultipleAsync(
                Sql,
                new { TenantId = tenantId, BranchScopeId = branchScopeId, RetirementThreshold = retirementThreshold },
                commandType: CommandType.Text,
                commandTimeout: 10);

            dto.BranchCount = await multi.ReadSingleAsync<int>();
            dto.OrganizationUnitCount = await multi.ReadSingleAsync<int>();
            dto.PositionCount = await multi.ReadSingleAsync<int>();
            dto.EmployeeCount = await multi.ReadSingleAsync<int>();

            var wfCounts = (await multi.ReadAsync<WorkflowStatusCount>()).ToDictionary(x => x.Status, x => x.Cnt);
            dto.WorkflowRunning = wfCounts.GetValueOrDefault("Running");
            dto.WorkflowApproved = wfCounts.GetValueOrDefault("Approved");
            dto.WorkflowRejected = wfCounts.GetValueOrDefault("Rejected");

            dto.ProbationCount = await multi.ReadSingleAsync<int>();
            dto.RetirementCount = await multi.ReadSingleAsync<int>();

            return dto;
        }
    }
}
