using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Constants;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>Answers "may the current caller access an endpoint gated to these menu operations?".</summary>
    public interface IEndpointPermissionService
    {
        /// <summary>
        /// True when one of the caller's roles has <c>CanView</c> on at least one operation whose
        /// <c>Link</c> matches (slash/case-insensitively) any of <paramref name="operationLinks"/>.
        /// An empty required set is treated as "no restriction" (true).
        /// </summary>
        Task<bool> HasAnyAsync(IReadOnlyList<string> operationLinks);

        /// <summary>
        /// Drops every cached granted-link set, so the next request re-reads from the database.
        /// Called whenever permissions change, so an admin's own save takes effect immediately
        /// instead of after the cache window.
        /// </summary>
        void InvalidateAll();
    }

    public class EndpointPermissionService(
        ICurrentUserService currentUser,
        IRepository<TenantUser> tenantUsers,
        IRepository<TenantUserRole> tenantUserRoles,
        IRepository<TenantRolePermission> tenantRolePermissions,
        IRepository<TenantOperation> tenantOperations,
        IMemoryCache cache) : IEndpointPermissionService
    {
        // PERFORMANCE: this service runs on EVERY [RequirePermission]-gated request, so the caller's
        // granted-link set is cached for a short window instead of hitting the database each time.
        // Permission changes made through the admin screens bust the cache outright (InvalidateAll);
        // the TTL only bounds changes made behind the application's back, e.g. directly in SQL.
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

        // Cache keys carry a generation number because IMemoryCache cannot enumerate or clear by
        // prefix. Bumping it orphans every existing entry at once — they then expire on their own.
        private static int _generation;

        public void InvalidateAll() => Interlocked.Increment(ref _generation);

        public async Task<bool> HasAnyAsync(IReadOnlyList<string> operationLinks)
        {
            if (operationLinks is null || operationLinks.Count == 0) return true;
            // Strictly role-based: no head-office/branch bypass. "Admin" access = a role granted the
            // operation's CanView, not a user who simply has no branch assignment.
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return false;

            var generation = Volatile.Read(ref _generation);
            var granted = await cache.GetOrCreateAsync($"perm-links:{generation}:{userId.Value}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                return await LoadGrantedLinksAsync(userId.Value);
            });

            if (granted is null || granted.Count == 0) return false;
            return operationLinks.Any(l => granted.Contains(Normalize(l)));
        }

        /// <summary>
        /// One round-trip through the TENANT-SCOPED model: the caller's membership of this tenant →
        /// the roles they hold in it → CanView grants → the tenant's own copy of each operation link.
        ///
        /// <para>Every repository call is already filtered to the current tenant by the Finbuckle
        /// discriminator, so a user who belongs to several tenants gets only the links for the one
        /// they are signed in to — which the previous global <c>UserRole</c> join could not express.</para>
        ///
        /// <para><c>IsActive</c> is honoured here: a tenant that hides a screen revokes access to it,
        /// not merely its sidebar entry.</para>
        /// </summary>
        private async Task<HashSet<string>> LoadGrantedLinksAsync(Guid userId)
        {
            var links = await tenantUsers.GetAll()
                .Where(tu => tu.UserId == userId && tu.Status == TenantUserStatuses.Active)
                .Join(tenantUserRoles.GetAll(),
                    tu => tu.Id, tur => tur.TenantUserId, (tu, tur) => tur.TenantRoleId)
                .Join(tenantRolePermissions.GetAll().Where(p => p.CanView),
                    roleId => roleId, p => p.TenantRoleId, (roleId, p) => p.TenantOperationId)
                .Join(tenantOperations.GetAll().Where(o => o.IsActive && o.Link != ""),
                    opId => opId, o => o.Id, (opId, o) => o.Link)
                .Distinct()
                .ToListAsync();

            return links.Select(Normalize).ToHashSet();
        }

        /// <summary>
        /// Reduces a link to its comparison key: no leading slash, lower case, and WITHOUT this
        /// subsystem's catalogue namespace.
        ///
        /// <para>⚠️ The namespace matters. Operation links are stored namespaced by their owning
        /// subsystem (<c>/hrms/setting</c>), because one catalogue serves them all; the links declared
        /// on <see cref="RequirePermissionAttribute"/> are bare (<c>"setting"</c>). Comparing the two
        /// verbatim never matches, so EVERY gated endpoint answered 403 to callers who did hold the
        /// grant. Both sides come through here.</para>
        /// </summary>
        private static string Normalize(string s)
        {
            var key = (s ?? string.Empty).TrimStart('/').ToLowerInvariant();
            return key.StartsWith(Subsystems.LinkNamespace, StringComparison.Ordinal)
                ? key[Subsystems.LinkNamespace.Length..]
                : key;
        }
    }
}
