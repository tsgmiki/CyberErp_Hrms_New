using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    /// <summary>
    /// Step-level authorization: a user may act on a step when the step has no configured
    /// approvers (open step), when they are listed as a user approver, or when they hold any
    /// of the listed approver roles.
    /// </summary>
    public interface IWorkflowApproverAuth
    {
        /// <summary>Returns whether the current user can decide + the step's approver display names.</summary>
        Task<(bool CanDecide, List<string> ApproverNames)> EvaluateAsync(Guid definitionId, int stepOrder);
        /// <summary>Throws a 400 when the current user is not authorized for the instance's current step.</summary>
        Task EnsureCanDecideAsync(WorkflowInstance instance);
        /// <summary>Role ids held by the current user (for batch evaluation in list queries).</summary>
        Task<HashSet<Guid>> GetCurrentUserRoleIdsAsync();
    }

    public class WorkflowApproverAuth(
        IRepository<WorkflowDefinition> definitions,
        IRepository<UserRole> userRoles,
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

        public async Task<(bool CanDecide, List<string> ApproverNames)> EvaluateAsync(Guid definitionId, int stepOrder)
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

            var names = approvers.Select(a => a.DisplayName).ToList();
            if (approvers.Count == 0) return (true, names); // open step — anyone may act

            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return (false, names);

            if (approvers.Any(a => a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId.Value))
                return (true, names);

            var roleIds = await GetCurrentUserRoleIdsAsync();
            var allowed = approvers.Any(a => a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId));
            return (allowed, names);
        }

        public async Task EnsureCanDecideAsync(WorkflowInstance instance)
        {
            var (canDecide, names) = await EvaluateAsync(instance.DefinitionId, instance.CurrentStepOrder);
            if (!canDecide)
                throw new ValidationException("approver",
                    $"You are not an authorized approver for step '{instance.CurrentStepName}'." +
                    (names.Count > 0 ? $" Authorized: {string.Join(", ", names)}." : string.Empty));
        }
    }
}
