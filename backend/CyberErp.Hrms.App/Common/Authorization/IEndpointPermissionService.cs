using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
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
    }

    public class EndpointPermissionService(
        ICurrentUserService currentUser,
        IRepository<UserRole> userRoles,
        IRepository<RolePermission> rolePermissions,
        IRepository<Operation> operations,
        IMemoryCache cache) : IEndpointPermissionService
    {
        // PERFORMANCE: this service runs on EVERY [RequirePermission]-gated request, so the caller's
        // granted-link set is cached for a short window instead of hitting the database each time.
        // Permission changes therefore take up to this long to affect API access (menu/UI refetches
        // separately); the window is deliberately short.
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

        public async Task<bool> HasAnyAsync(IReadOnlyList<string> operationLinks)
        {
            if (operationLinks is null || operationLinks.Count == 0) return true;
            // Strictly role-based: no head-office/branch bypass. "Admin" access = a role granted the
            // operation's CanView, not a user who simply has no branch assignment.
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return false;

            var granted = await cache.GetOrCreateAsync($"perm-links:{userId.Value}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                return await LoadGrantedLinksAsync(userId.Value);
            });

            if (granted is null || granted.Count == 0) return false;
            return operationLinks.Any(l => granted.Contains(Normalize(l)));
        }

        /// <summary>One round-trip: the caller's roles → CanView permissions → operation links.</summary>
        private async Task<HashSet<string>> LoadGrantedLinksAsync(Guid userId)
        {
            var links = await userRoles.GetAll()
                .Where(ur => ur.UserId == userId)
                .Join(rolePermissions.GetAll().Where(rp => rp.CanView),
                    ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.OperationId)
                .Join(operations.GetAll().Where(o => o.Link != null),
                    opId => opId, o => o.Id, (opId, o) => o.Link!)
                .Distinct()
                .ToListAsync();

            return links.Select(Normalize).ToHashSet();
        }

        private static string Normalize(string s) => (s ?? string.Empty).TrimStart('/').ToLowerInvariant();
    }
}
