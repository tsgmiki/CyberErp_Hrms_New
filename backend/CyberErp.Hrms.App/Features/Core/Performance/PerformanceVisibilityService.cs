using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.App.Features.Core.WorkforcePlans;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    /// <summary>
    /// The caller's role-based data scope for the performance module, computed ONCE per request:
    /// HR admin → unrestricted; managerial employee → their org unit + all child units (subtree);
    /// everyone else → self only.
    /// </summary>
    public sealed class VisibilityScope
    {
        public bool IsAdmin { get; init; }
        /// <summary>The caller's linked employee id (null for unlinked/system accounts).</summary>
        public Guid? EmployeeId { get; init; }
        public bool IsManager { get; init; }
        /// <summary>The manager's unit subtree (their unit + descendants); empty unless <see cref="IsManager"/>.</summary>
        public HashSet<Guid> UnitIds { get; init; } = [];
    }

    /// <summary>
    /// Role-based visibility for performance data (appraisals, goals, employee pickers). PERFORMANCE:
    /// the scope is resolved once per request (memoized — 2-3 small queries + an in-memory unit-tree BFS)
    /// and every consumer turns it into a single SQL predicate — no per-row org-tree climbs.
    /// <b>HR admin = holder of the HR employee-register permission, OR an explicit User/Role approver
    /// on the Appraisal workflow's HrSignOff step.</b>
    /// </summary>
    public interface IPerformanceVisibilityService
    {
        Task<VisibilityScope> GetScopeAsync();
        /// <summary>admin, OR the employee themselves, OR a manager whose subtree contains the employee.</summary>
        Task<bool> CanAccessEmployeeAsync(Guid employeeId);
    }

    public class PerformanceVisibilityService(
        ICurrentUserService currentUser,
        IRepository<User> users,
        IRepository<Employee> employees,
        IRepository<OrganizationUnit> units,
        IRepository<WorkflowDefinition> definitions,
        IRepository<TenantUser> tenantUsers,
        IRepository<TenantUserRole> tenantUserRoles,
        IRepository<TenantRole> tenantRoles,
        IWorkflowApproverAuth approverAuth) : IPerformanceVisibilityService
    {
        private VisibilityScope? _scope;                              // scoped service — one computation per request
        private readonly Dictionary<Guid, bool> _accessCache = [];    // per-employee access answers within the request

        public async Task<VisibilityScope> GetScopeAsync()
        {
            if (_scope is not null) return _scope;

            var userId = currentUser.GetCurrentUserId();
            Guid? myEmp = userId is null
                ? null
                : await users.GetAll().Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();

            var isAdmin = await IsAdminAsync(userId);
            var isManager = false;
            HashSet<Guid> unitIds = [];
            if (!isAdmin && myEmp.HasValue)
            {
                var me = await employees.GetAll().Where(e => e.Id == myEmp.Value)
                    .Select(e => new { e.IsManagerial, UnitId = e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null })
                    .FirstOrDefaultAsync();
                if (me is { IsManagerial: true, UnitId: not null })
                {
                    isManager = true;
                    // The unit tree is small — one (Id, ParentId) load + in-memory BFS (reused establishment helper).
                    unitIds = await EstablishmentShared.ResolveSubtreeAsync(units, me.UnitId) ?? [];
                }
            }

            return _scope = new VisibilityScope { IsAdmin = isAdmin, EmployeeId = myEmp, IsManager = isManager, UnitIds = unitIds };
        }

        /// <summary>
        /// True when the caller holds a role whose CODE is in <see cref="HrRoles.OrganizationWide"/>,
        /// through the tenant-scoped membership chain. Every repository call is already filtered to
        /// the signed-in tenant, so a user who belongs to several tenants is judged only on the roles
        /// they hold in this one.
        /// </summary>
        private async Task<bool> HasOrganizationWideRoleAsync(Guid userId) =>
            await tenantUsers.GetAll()
                .Where(tu => tu.UserId == userId && tu.Status)
                .Join(tenantUserRoles.GetAll(), tu => tu.Id, tur => tur.TenantUserId, (tu, tur) => tur.TenantRoleId)
                .Join(tenantRoles.GetAll(), roleId => roleId, r => r.Id, (roleId, r) => r.Code)
                .AnyAsync(code => HrRoles.OrganizationWide.Contains(code));

        public async Task<bool> CanAccessEmployeeAsync(Guid employeeId)
        {
            var scope = await GetScopeAsync();
            if (scope.IsAdmin) return true;
            if (scope.EmployeeId == employeeId) return true;
            if (!scope.IsManager) return false;
            if (_accessCache.TryGetValue(employeeId, out var cached)) return cached;

            var unitId = await employees.GetAll().Where(e => e.Id == employeeId)
                .Select(e => e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null)
                .FirstOrDefaultAsync();
            return _accessCache[employeeId] = unitId.HasValue && scope.UnitIds.Contains(unitId.Value);
        }

        /// <summary>HR admin: a role carrying organisation-wide HR authority, or an explicit
        /// User/Role approver configured on the Appraisal definition's HrSignOff step (an OPEN step
        /// does NOT make everyone an admin).</summary>
        private async Task<bool> IsAdminAsync(Guid? userId)
        {
            if (userId is null) return false;

            // HR admin is a ROLE, and it has taken two attempts to say so.
            //
            // It first read `if (currentUser.IsHeadOffice()) return true;`. That flag comes from the
            // employee's branch, and in a single-branch tenant — where the one branch is flagged
            // IsHeadOffice — it is true for EVERY employee-linked user, so the ~73 checks written as
            // `if (!scope.IsAdmin) narrow(...)` never narrowed anything (logic §11).
            //
            // It was then replaced by "holds the employee register", on the stated assumption that
            // only Administrator and HR Admin would ever hold that screen. ⚠️ That assumption is not
            // the client's to keep: in CERP the Department Manager role holds 141 of the 142 screens
            // HR Admin holds, employee register included — so every department head was an
            // organisation-wide administrator, and could raise a hiring request for any unit in the
            // company (logic §12.47). No screen grant discriminates here, so no screen can define
            // this.
            //
            // Head-office status still drives BRANCH scoping elsewhere (ICurrentUserService); it just
            // no longer doubles as "this person is HR".
            if (await HasOrganizationWideRoleAsync(userId.Value)) return true;

            var approvers = await definitions.GetAll()
                .Where(d => d.EntityType == WorkflowEntityTypes.Appraisal && d.IsActive)
                .SelectMany(d => d.Steps)
                .Where(s => s.Name == nameof(AppraisalStage.HrSignOff))
                .SelectMany(s => s.Approvers)
                .Select(a => new { a.ApproverType, a.ApproverId })
                .ToListAsync();
            if (approvers.Count == 0) return false;
            if (approvers.Any(a => a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId.Value)) return true;
            var roleIds = await approverAuth.GetCurrentUserRoleIdsAsync();
            return approvers.Any(a => a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId));
        }
    }
}
