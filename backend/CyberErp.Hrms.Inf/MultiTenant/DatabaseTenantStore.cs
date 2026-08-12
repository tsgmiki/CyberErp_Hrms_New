using CyberErp.Hrms.Dom.Entities.Core;
using CyberErp.Hrms.Inf.Models;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
namespace CyberErp.Hrms.Inf.MultiTenant;

/// <summary>
/// A tenant store that fetches tenant information from the database,
/// including subscription details for validation.
/// </summary>
public class DatabaseTenantStore(DbContext dbContext, IMemoryCache cache) : IMultiTenantStore<AppTenantInfo>
{
    private readonly DbContext _dbContext = dbContext;

    /// <summary>
    /// Tenant rows are effectively immutable at request cadence (name, theme, subscription window),
    /// but Finbuckle resolves the tenant on EVERY request — so without this the app paid a database
    /// round-trip per request just to learn who the caller's tenant is, on top of whatever the request
    /// actually does. A dashboard firing ~17 calls spent ~34 queries on it. Short TTL so a
    /// subscription change still takes effect promptly.
    /// </summary>
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public Task<AppTenantInfo?> GetByIdentifierAsync(string identifier) => ResolveAsync(identifier);

    public Task<AppTenantInfo?> GetAsync(string id) => ResolveAsync(id);

    /// <summary>
    /// One lookup for both entry points. The cookie/claim flow carries the tenant GUID while the
    /// host/header flow carries the identifier (see HybridTenantStrategy), and the old code tried
    /// Identifier first and then fell back to Id — so the common cookie path always cost TWO queries,
    /// the first of which could never match. A single predicate covers both.
    /// </summary>
    private async Task<AppTenantInfo?> ResolveAsync(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (cache.TryGetValue(CacheKey(key), out AppTenantInfo? cached)) return cached;

        Guid.TryParse(key, out var tenantId);
        var tenant = await _dbContext.Set<Tenant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Identifier == key || (tenantId != Guid.Empty && t.Id == tenantId));

        var info = tenant == null ? null : MapTenant(tenant);

        // A miss is cached too, briefly: an unknown identifier otherwise re-queries on every request
        // of a request storm. Both of the tenant's keys are primed so the id and identifier flows share.
        cache.Set(CacheKey(key), info, CacheTtl);
        if (info is not null)
        {
            cache.Set(CacheKey(info.Id!), info, CacheTtl);
            if (!string.IsNullOrEmpty(info.Identifier)) cache.Set(CacheKey(info.Identifier), info, CacheTtl);
        }
        return info;
    }

    private static string CacheKey(string key) => $"tenant-store:{key}";

    public async Task<IEnumerable<AppTenantInfo>> GetAllAsync()
        => await GetAllAsync(int.MaxValue, 0);

    public async Task<IEnumerable<AppTenantInfo>> GetAllAsync(int take, int skip)
    {
        var tenants = await _dbContext.Set<Tenant>()
            .AsNoTracking()
            .OrderBy(t => t.Identifier)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return tenants.Select(MapTenant);
    }

    public Task<bool> AddAsync(AppTenantInfo tenantInfo) => Task.FromResult(false);

    public Task<bool> UpdateAsync(AppTenantInfo tenantInfo) => Task.FromResult(false);

    public Task<bool> RemoveAsync(string id) => Task.FromResult(false);

    private static AppTenantInfo MapTenant(Tenant tenant) => new()
    {
        Id = tenant.Id.ToString(),
        Identifier = tenant.Identifier,
        Name = tenant.Name,
        ConnectionString = tenant.ConnectionString,
        Theme = tenant.Theme,
        SubscriptionStartDate = tenant.SubscriptionStartDate,
        SubscriptionEndDate = tenant.SubscriptionEndDate,
        IsActive = tenant.IsActive
    };
}
