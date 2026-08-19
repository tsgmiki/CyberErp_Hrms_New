using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// The TEMPLATE role ids the signed-in user holds IN THE CURRENT TENANT.
    ///
    /// <para><b>Why this exists.</b> Four places asked that question by reading <c>Core.UserRole</c>
    /// directly — the workflow approver check, and three clearance/termination approver checks. That
    /// worked only while <c>UserRole</c> carried a TenantId and the repository filtered on it. The
    /// column was dropped on 2026-08-15 for SRMS parity, which makes the table GLOBAL: the same query
    /// would return the roles a user holds in <i>every</i> tenant, and a multi-tenant user would pass
    /// an approver check using a role granted somewhere else entirely.</para>
    ///
    /// <para>The tenant-scoped model answers it correctly:
    /// <c>TenantUser</c> (this tenant, active) → <c>TenantUserRole</c> → <c>TenantRole</c>. Template
    /// ids are returned rather than tenant-role ids because that is what the callers compare against:
    /// clearance and workflow approvers are configured with GLOBAL role ids.</para>
    /// </summary>
    public interface ICurrentUserRoles
    {
        /// <summary>Template role ids held in this tenant. Empty when unauthenticated.</summary>
        Task<HashSet<Guid>> GetTemplateRoleIdsAsync();

        /// <summary>
        /// The reverse lookup: <c>Core.User</c> ids holding any of <paramref name="templateRoleIds"/>
        /// IN THIS TENANT. Used to notify approvers, and scoped for the same reason — reading
        /// <c>Core.UserRole</c> by RoleId would return holders in every tenant.
        /// </summary>
        Task<HashSet<Guid>> GetUserIdsInRolesAsync(IReadOnlyCollection<Guid> templateRoleIds);
    }

    public class CurrentUserRoles(
        ICurrentUserService currentUser,
        IRepository<TenantUser> tenantUsers,
        IRepository<TenantUserRole> tenantUserRoles,
        IRepository<TenantRole> tenantRoles) : ICurrentUserRoles
    {
        private HashSet<Guid>? _cached;

        public async Task<HashSet<Guid>> GetTemplateRoleIdsAsync()
        {
            if (_cached is not null) return _cached;

            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return _cached = [];

            // TenantUser and TenantRole are still tenant-filtered, so this chain is scoped even
            // though TenantUserRole itself no longer carries a TenantId.
            var ids = await tenantUsers.GetAll()
                .Where(tu => tu.UserId == userId.Value && tu.Status == TenantUserStatuses.Active)
                .Join(tenantUserRoles.GetAll(),
                    tu => tu.Id, tur => tur.TenantUserId, (tu, tur) => tur.TenantRoleId)
                .Join(tenantRoles.GetAll().Where(r => r.SourceTemplateId != null),
                    roleId => roleId, r => r.Id, (roleId, r) => r.SourceTemplateId!.Value)
                .Distinct()
                .ToListAsync();

            return _cached = ids.ToHashSet();
        }

        public async Task<HashSet<Guid>> GetUserIdsInRolesAsync(IReadOnlyCollection<Guid> templateRoleIds)
        {
            if (templateRoleIds.Count == 0) return [];

            var ids = await tenantRoles.GetAll()
                .Where(r => r.SourceTemplateId != null && templateRoleIds.Contains(r.SourceTemplateId!.Value))
                .Join(tenantUserRoles.GetAll(),
                    r => r.Id, tur => tur.TenantRoleId, (r, tur) => tur.TenantUserId)
                .Join(tenantUsers.GetAll().Where(tu => tu.Status == TenantUserStatuses.Active),
                    tenantUserId => tenantUserId, tu => tu.Id, (tenantUserId, tu) => tu.UserId)
                .Distinct()
                .ToListAsync();

            return ids.ToHashSet();
        }
    }
}
