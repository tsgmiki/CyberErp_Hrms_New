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
    }

    public class WorkflowApproverAuth(
        IRepository<WorkflowDefinition> definitions,
        IRepository<UserRole> userRoles,
        IOrgManagerResolver managerResolver,
        ICurrentUserService currentUser) : IWorkflowApproverAuth
    {
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

                    case WorkflowApproverType.ImmediateManager:
                    case WorkflowApproverType.UnitManager:
                        var resolved = a.ApproverType == WorkflowApproverType.ImmediateManager
                            ? (requesterEmployeeId.HasValue
                                ? await managerResolver.ResolveImmediateManagerAsync(requesterEmployeeId.Value)
                                : null)
                            : await managerResolver.ResolveUnitManagerAsync(a.ApproverId, requesterEmployeeId);

                        names.Add(resolved is null ? $"{a.DisplayName} (unresolved)" : $"{a.DisplayName}: {resolved.Name}");
                        if (resolved is not null && userId.HasValue && resolved.UserIds.Contains(userId.Value))
                            canDecide = true;
                        break;
                }
            }

            return (canDecide, names);
        }

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
