using CyberErp.Hrms.Dom.Entities.Core;
using CyberErp.Hrms.Inf.Models;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
namespace CyberErp.Hrms.Inf.MultiTenant;

/// <summary>
/// A tenant store that fetches tenant information from the database,
/// including subscription details for validation.
/// </summary>
public class DatabaseTenantStore(DbContext dbContext) : IMultiTenantStore<AppTenantInfo>
{
    private readonly DbContext _dbContext = dbContext;

    public async Task<AppTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;

        var tenant = await _dbContext.Set<Tenant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Identifier == identifier);

        return tenant == null ? null : MapTenant(tenant);
    }

    public async Task<AppTenantInfo?> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        if (!Guid.TryParse(id, out var tenantId))
            return await GetByIdentifierAsync(id);

        var tenant = await _dbContext.Set<Tenant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        return tenant == null ? null : MapTenant(tenant);
    }

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
