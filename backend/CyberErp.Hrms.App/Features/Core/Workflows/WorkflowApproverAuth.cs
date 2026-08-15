using CyberErp.Hrms.App.Common.Authorization;
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
        /// The caller's linked employee id (null for system/unlinked accounts). Exposed so the approval
        /// inbox can pre-filter subject-routed steps in SQL instead of loading every instance to test
        /// them in memory. Memoised per request like the rest of this service.
        /// </summary>
        Task<Guid?> CurrentEmployeeIdForInboxAsync();
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
        ICurrentUserRoles currentUserRoles,
        IRepository<User> users,
        IRepository<TenantRolePermission> tenantRolePermissions,
        IRepository<TenantOperation> tenantOperations,
        IRepository<TenantRole> tenantRoles,
        IOrgManagerResolver managerResolver,
        ICurrentUserService currentUser) : IWorkflowApproverAuth
    {
        /// <summary>Link of the operation that grants the right to act on workflow approvals.</summary>
        private const string WorkflowOperationLink = "/workflow";

        // ---- Per-request memoisation --------------------------------------------------------
        // This service is SCOPED, so these live exactly one request. Every field below answers a
        // question whose answer cannot change mid-request (who am I, which roles do I hold, who
        // approves step N of definition D), yet each was re-queried on EVERY call — and callers call
        // in loops: the approval inbox evaluates one instance at a time, and opening an appraisal asks
        // the workflow service four separate questions. That turned into ~5 round-trips per instance
        // and 20 queries to open one record. Caching here fixes both without touching the callers.
        private HashSet<Guid>? _roleIds;
        private Guid? _myEmployeeId;
        private bool _myEmployeeLoaded;
        private readonly Dictionary<(Guid DefinitionId, int StepOrder), List<StepApprover>> _stepApprovers = [];

        /// <summary>Approver row as this service needs it (flattened out of the definition aggregate).</summary>
        private sealed record StepApprover(WorkflowApproverType ApproverType, Guid ApproverId, string DisplayName);

        public async Task<HashSet<Guid>> GetCurrentUserRoleIdsAsync()
        {
            // Resolved from the TENANT model, not Core.UserRole. That table lost its TenantId on
            // 2026-08-15 and is global now, so reading it by UserId would return the roles this user
            // holds in EVERY tenant — and a multi-tenant approver would pass a check using a role
            // granted somewhere else. See ICurrentUserRoles.
            return _roleIds ??= await currentUserRoles.GetTemplateRoleIdsAsync();
        }

        public Task<Guid?> CurrentEmployeeIdForInboxAsync() => CurrentEmployeeIdAsync();

        /// <summary>The current user's linked employee id (null for system/unlinked accounts).</summary>
        private async Task<Guid?> CurrentEmployeeIdAsync()
        {
            if (_myEmployeeLoaded) return _myEmployeeId;
            _myEmployeeLoaded = true;
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return _myEmployeeId = null;
            return _myEmployeeId = await users.GetAll()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// The approver rows of one step. Cached per (definition, step) because a single inbox render
        /// asks for the same step repeatedly — once per instance sitting on it.
        /// </summary>
        private async Task<List<StepApprover>> StepApproversAsync(Guid definitionId, int stepOrder)
        {
            if (_stepApprovers.TryGetValue((definitionId, stepOrder), out var cached)) return cached;

            // Approver rows are read through the definition aggregate (children carry no reliable
            // tenant stamp); by-id access keeps this tenant-safe.
            var rows = await definitions.GetAllWithoutTenantFilter()
                .Where(d => d.Id == definitionId)
                .SelectMany(d => d.Steps)
                .Where(s => s.StepOrder == stepOrder)
                .SelectMany(s => s.Approvers)
                .Select(a => new StepApprover(a.ApproverType, a.ApproverId, a.DisplayName))
                .ToListAsync();

            _stepApprovers[(definitionId, stepOrder)] = rows;
            return rows;
        }

        public async Task<(bool CanDecide, List<string> ApproverNames)> EvaluateAsync(
            Guid definitionId, int stepOrder, Guid? requesterEmployeeId)
        {
            var approvers = await StepApproversAsync(definitionId, stepOrder);

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
                        // Holders IN THIS TENANT. Core.UserRole is global since 2026-08-15, so a
                        // direct read would notify role-holders in other tenants.
                        var roleUserIds = await currentUserRoles.GetUserIdsInRolesAsync([a.ApproverId]);
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

            // Holders IN THIS TENANT — see the note in ResolveApproverUserIdsAsync.
            return await currentUserRoles.GetUserIdsInRolesAsync(roleIds.ToList());
        }

        public async Task<bool> CanActOnOpenStepsAsync()
        {
            if (currentUser.GetCurrentUserId() is null) return false;
            var approveRoles = await WorkflowApproveRoleIdsAsync();
            if (approveRoles.Count == 0) return false;
            var mine = await GetCurrentUserRoleIdsAsync();
            return mine.Overlaps(approveRoles);
        }

        /// <summary>
        /// Roles granted approval rights on the Workflow Tracking operation.
        ///
        /// <para>Reads the tenant-scoped grants — Core.RolePermission was retired on 2026-08-13 — but
        /// still returns TEMPLATE role ids, because that is what <see cref="GetCurrentUserRoleIdsAsync"/>
        /// yields (it reads Core.UserRole) and the two sets are compared directly.</para>
        /// </summary>
        private async Task<List<Guid>> WorkflowApproveRoleIdsAsync() =>
            await tenantRolePermissions.GetAll()
                .Where(p => p.CanApprove)
                .Join(tenantOperations.GetAll().Where(o => o.Link == WorkflowOperationLink && o.IsActive),
                    p => p.TenantOperationId, o => o.Id, (p, o) => p.TenantRoleId)
                .Join(tenantRoles.GetAll().Where(r => r.SourceTemplateId != null),
                    roleId => roleId, r => r.Id, (roleId, r) => r.SourceTemplateId!.Value)
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
