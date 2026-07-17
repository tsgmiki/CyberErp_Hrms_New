using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Inf.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Per-tenant atomic counter (logic.md §7.1 adoption #5). The increment is a single
    /// UPDATE … OUTPUT statement, so concurrent callers can never receive the same value —
    /// unlike count+1 numbering. The seed row is created lazily; the duplicate-key race on
    /// first use is absorbed by one retry.
    /// </summary>
    public class NumberSequenceService(HrmsDbContext context, ITenantService tenantService) : INumberSequenceService
    {
        public async Task<long> NextAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A sequence key is required.", nameof(key));
            var tenantId = tenantService.GetCurrentTenantId()
                ?? throw new InvalidOperationException("Number sequences require a tenant context.");

            var next = await TryIncrementAsync(tenantId, key);
            if (next.HasValue) return next.Value;

            // First use for this tenant+key: seed the row, absorbing a concurrent-seed race.
            try
            {
                await context.Database.ExecuteSqlAsync(
                    $"INSERT INTO [dbo].[hrmsNumberSequence] ([TenantId], [Key], [Value]) VALUES ({tenantId}, {key}, 0)");
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                // Another caller seeded it between our UPDATE and INSERT — fall through to increment.
            }

            return await TryIncrementAsync(tenantId, key)
                ?? throw new InvalidOperationException($"Failed to allocate a number for sequence '{key}'.");
        }

        private async Task<long?> TryIncrementAsync(string tenantId, string key)
        {
            var values = await context.Database.SqlQuery<long>(
                    $@"UPDATE [dbo].[hrmsNumberSequence]
                       SET [Value] = [Value] + 1
                       OUTPUT inserted.[Value]
                       WHERE [TenantId] = {tenantId} AND [Key] = {key}")
                .ToListAsync();
            return values.Count > 0 ? values[0] : null;
        }
    }
}
