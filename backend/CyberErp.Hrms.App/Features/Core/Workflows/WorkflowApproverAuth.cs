using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    /// <summary>
    /// Step-level authorization: a user may act on a step when the step has no configured
    /// approvers (open step), when they are listed as a user approver, when they hold any of
    /// the listed approver roles, or when a DYNAMIC approver (Immediate Manager / Unit Manager)
    /// resolves to them through the org-structure traversal for the instance's requester.
    /// </summary>
    public interface IWorkflowApproverAuth
    {
        /// <summary>
        /// Returns whether the current user can decide + the step's approver display names.
        /// <paramref name="requesterEmployeeId"/> anchors dynamic (manager) resolution — pass the
        /// instance's EmployeeId; static-only steps ignore it.
        /// </summary>
        Task<(bool CanDecide, List<string> ApproverNames)> EvaluateAsync(Guid definitionId, int stepOrder, Guid? requesterEmployeeId);
        /// <summary>Throws a 400 when the current user is not authorized for the instance's current step.</summary>
        Task EnsureCanDecideAsync(WorkflowInstance instance);
        /// <summary>Role ids held by the current user (for batch evaluation in list queries).</summary>
        Task<HashSet<Guid>> GetCurrentUserRoleIdsAsync();
        /// <summary>
        /// The DISTINCT Core.User ids who may act on a step — the same resolution
        /// <see cref="EvaluateAsync"/> performs, but projected to recipients (for notifying approvers).
        /// Empty for an open step (no configured approvers). <paramref name="requesterEmployeeId"/> anchors
        /// dynamic (manager) resolution — pass the instance's EmployeeId.
        /// </summary>
        Task<HashSet<Guid>> ResolveApproverUserIdsAsync(Guid definitionId, int stepOrder, Guid? requesterEmployeeId);
        /// <summary>
        /// Recipients for an OPEN step (one with no configured approvers, which <see cref="EvaluateAsync"/>
        /// lets ANYONE act on). Without this an open step notifies nobody: the request sits waiting while
        /// every would-be approver is unaware of it. Bounded to users whose roles carry CanApprove on the
        /// Workflow Tracking operation — the people entitled to act — rather than the whole tenant.
        /// </summary>
        Task<HashSet<Guid>> ResolveOpenStepRecipientsAsync();
        /// <summary>
        /// Whether the CURRENT user belongs to the open-step audience — i.e. whether an open step
        /// should surface in their approval inbox. Deliberately the same rule as
        /// <see cref="ResolveOpenStepRecipientsAsync"/> so the people alerted about an open step are
        /// exactly the people who then find it waiting in their inbox.
        /// </summary>
        Task<bool> CanActOnOpenStepsAsync();
    }

    public class WorkflowApproverAuth(
        IRepository<WorkflowDefinition> definitions,
        IRepository<UserRole> userRoles,
        IRepository<User> users,
        IRepository<RolePermission> rolePermissions,
        IOrgManagerResolver managerResolver,
        ICurrentUserService currentUser) : IWorkflowApproverAuth
    {
        /// <summary>Link of the operation that grants the right to act on workflow approvals.</summary>
        private const string WorkflowOperationLink = "/workflow";

        public async Task<HashSet<Guid>> GetCurrentUserRoleIdsAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return [];
            return (await userRoles.GetAll()
                    .Where(u => u.UserId == userId.Value)
                    .Select(u => u.RoleId)
                    .ToListAsync())
                .ToHashSet();
        }

        /// <summary>The current user's linked employee id (null for system/unlinked accounts).</summary>
        private async Task<Guid?> CurrentEmployeeIdAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return null;
            return await users.GetAll().Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        }

        public async Task<(bool CanDecide, List<string> ApproverNames)> EvaluateAsync(
            Guid definitionId, int stepOrder, Guid? requesterEmployeeId)
        {
            // Approver rows are read through the definition aggregate (children carry no reliable
            // tenant stamp); by-id access keeps this tenant-safe.
            var approvers = await definitions.GetAllWithoutTenantFilter()
                .Where(d => d.Id == definitionId)
                .SelectMany(d => d.Steps)
                .Where(s => s.StepOrder == stepOrder)
                .SelectMany(s => s.Approvers)
                .Select(a => new { a.ApproverType, a.ApproverId, a.DisplayName })
                .ToListAsync();

            if (approvers.Count == 0)
                return (true, []); // open step — anyone may act

            var userId = currentUser.GetCurrentUserId();
            var canDecide = false;
            var names = new List<string>();
            HashSet<Guid>? roleIds = null;
            Guid? myEmployeeId = null;
            var myEmployeeLoaded = false;

            foreach (var a in approvers)
            {
                switch (a.ApproverType)
                {
                    case WorkflowApproverType.User:
                        names.Add(a.DisplayName);
                        if (userId.HasValue && a.ApproverId == userId.Value) canDecide = true;
                        break;

                    case WorkflowApproverType.Role:
                        names.Add(a.DisplayName);
                        if (userId.HasValue)
                        {
                            roleIds ??= await GetCurrentUserRoleIdsAsync();
                            if (roleIds.Contains(a.ApproverId)) canDecide = true;
                        }
                        break;

                    case WorkflowApproverType.Subject:
                        // The subject employee acts on their own step (self-service). No override — only them.
                        names.Add(a.DisplayName);
                        if (!myEmployeeLoaded) { myEmployeeId = await CurrentEmployeeIdAsync(); myEmployeeLoaded = true; }
                        if (requesterEmployeeId.HasValue && myEmployeeId.HasValue && myEmployeeId.Value == requesterEmployeeId.Value)
                            canDecide = true;
                        break;

                    case WorkflowApproverType.ImmediateManager:
                    case WorkflowApproverType.UnitManager:
                    case WorkflowApproverType.SecondLevelManager:
                        var resolved = a.ApproverType switch
                        {
                            WorkflowApproverType.ImmediateManager => requesterEmployeeId.HasValue
                                ? await managerResolver.ResolveImmediateManagerAsync(requesterEmployeeId.Value) : null,
                            WorkflowApproverType.SecondLevelManager => requesterEmployeeId.HasValue
                                ? await managerResolver.ResolveSecondLevelManagerAsync(requesterEmployeeId.Value) : null,
                            _ => await managerResolver.ResolveUnitManagerAsync(a.ApproverId, requesterEmployeeId),
                        };

                        names.Add(resolved is null ? $"{a.DisplayName} (unresolved)" : $"{a.DisplayName}: {resolved.Name}");
                        if (resolved is not null && userId.HasValue && resolved.UserIds.Contains(userId.Value))
                            canDecide = true;
                        break;
                }
            }

            return (canDecide, names);
        }

        public async Task<HashSet<Guid>> ResolveApproverUserIdsAsync(
            Guid definitionId, int stepOrder, Guid? requesterEmployeeId)
        {
            var approvers = await definitions.GetAllWithoutTenantFilter()
                .Where(d => d.Id == definitionId)
                .SelectMany(d => d.Steps)
                .Where(s => s.StepOrder == stepOrder)
                .SelectMany(s => s.Approvers)
                .Select(a => new { a.ApproverType, a.ApproverId })
                .ToListAsync();

            var userIds = new HashSet<Guid>();

            foreach (var a in approvers)
            {
                switch (a.ApproverType)
                {
                    case WorkflowApproverType.User:
                        userIds.Add(a.ApproverId);
                        break;

                    case WorkflowApproverType.Role:
                        var roleUserIds = await userRoles.GetAll()
                            .Where(u => u.RoleId == a.ApproverId)
                            .Select(u => u.UserId)
                            .ToListAsync();
                        foreach (var id in roleUserIds) userIds.Add(id);
                        break;

                    case WorkflowApproverType.Subject:
                        if (requesterEmployeeId.HasValue)
                        {
                            var subjectUserIds = await users.GetAll()
                                .Where(u => u.EmployeeId == requesterEmployeeId.Value)
                                .Select(u => u.Id)
                                .ToListAsync();
                            foreach (var id in subjectUserIds) userIds.Add(id);
                        }
                        break;

                    case WorkflowApproverType.ImmediateManager:
                    case WorkflowApproverType.SecondLevelManager:
                    case WorkflowApproverType.UnitManager:
                        var resolved = a.ApproverType switch
                        {
                            WorkflowApproverType.ImmediateManager => requesterEmployeeId.HasValue
                                ? await managerResolver.ResolveImmediateManagerAsync(requesterEmployeeId.Value) : null,
                            WorkflowApproverType.SecondLevelManager => requesterEmployeeId.HasValue
                                ? await managerResolver.ResolveSecondLevelManagerAsync(requesterEmployeeId.Value) : null,
                            _ => await managerResolver.ResolveUnitManagerAsync(a.ApproverId, requesterEmployeeId),
                        };
                        if (resolved is not null)
                            foreach (var id in resolved.UserIds) userIds.Add(id);
                        break;
                }
            }

            return userIds;
        }

        public async Task<HashSet<Guid>> ResolveOpenStepRecipientsAsync()
        {
            var roleIds = await WorkflowApproveRoleIdsAsync();
            if (roleIds.Count == 0) return [];

            return (await userRoles.GetAll()
                    .Where(u => roleIds.Contains(u.RoleId))
                    .Select(u => u.UserId)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet();
        }

        public async Task<bool> CanActOnOpenStepsAsync()
        {
            if (currentUser.GetCurrentUserId() is null) return false;
            var approveRoles = await WorkflowApproveRoleIdsAsync();
            if (approveRoles.Count == 0) return false;
            var mine = await GetCurrentUserRoleIdsAsync();
            return mine.Overlaps(approveRoles);
        }

        /// <summary>Roles granted approval rights on the Workflow Tracking operation.</summary>
        private async Task<List<Guid>> WorkflowApproveRoleIdsAsync() =>
            await rolePermissions.GetAll()
                .Where(p => p.CanApprove && p.Operation.Link == WorkflowOperationLink)
                .Select(p => p.RoleId)
                .Distinct()
                .ToListAsync();

        public async Task EnsureCanDecideAsync(WorkflowInstance instance)
        {
            var (canDecide, names) = await EvaluateAsync(instance.DefinitionId, instance.CurrentStepOrder, instance.EmployeeId);
            if (!canDecide)
                throw new ValidationException("approver",
                    $"You are not an authorized approver for step '{instance.CurrentStepName}'." +
                    (names.Count > 0 ? $" Authorized: {string.Join(", ", names)}." : string.Empty));
        }
    }
}
