using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    /// <summary>Who an approver is organisationally allowed to hand their authority to.</summary>
    public interface IDelegationScopeService
    {
        /// <summary>
        /// Employee ids <paramref name="approverEmployeeId"/> may name as a stand-in.
        /// </summary>
        Task<HashSet<Guid>> DelegatableEmployeeIdsAsync(Guid approverEmployeeId);

        /// <summary>Is <paramref name="candidateEmployeeId"/> inside that scope?</summary>
        Task<bool> CanDelegateToAsync(Guid approverEmployeeId, Guid candidateEmployeeId);
    }

    /// <summary>
    /// The organisational half of "may this person stand in for me" — the seniority rules are
    /// <see cref="IDelegationEligibilityService"/>; this is the "and they must be one of mine" half.
    /// </summary>
    /// <remarks>
    /// <para>A department head may delegate <b>downward and sideways within their own branch of the
    /// tree</b>: anybody in a unit they manage, anybody in a unit beneath it, and their own
    /// colleagues. Nothing above them and nothing across the organisation.</para>
    ///
    /// <para>⚠️ THE OWN-UNIT FALLBACK IS NOT A LOOPHOLE, IT IS THE NON-MANAGER CASE. Approval
    /// authority does not only belong to department heads — an HR officer named directly on a
    /// workflow step has it too, and <c>EmployeesInMyManagedUnitsAsync</c> returns EMPTY for them
    /// because they manage no unit. Without the fallback the subtree rule would silently forbid
    /// every non-manager from ever arranging cover, which reads as the feature being broken rather
    /// than as a policy.</para>
    ///
    /// <para>⚠️ HR is exempt, and that exemption lives in the CALLER, not here. This service answers
    /// a question about the org chart; whether somebody is allowed to bypass the answer is an
    /// authorisation decision and belongs where the other authorisation decisions are.</para>
    /// </remarks>
    public class DelegationScopeService(
        IRepository<Employee> employees,
        IOrgManagerResolver managerResolver) : IDelegationScopeService
    {
        public async Task<HashSet<Guid>> DelegatableEmployeeIdsAsync(Guid approverEmployeeId)
        {
            // Everything in the units they manage, and everything beneath.
            var scope = await managerResolver.EmployeesInMyManagedUnitsAsync(approverEmployeeId);

            // Plus their own unit's members, which covers the approver who manages nothing.
            var myUnitId = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == approverEmployeeId && e.Position != null)
                .Select(e => (Guid?)e.Position!.OrganizationUnitId)
                .FirstOrDefaultAsync();

            if (myUnitId is Guid unit)
            {
                var peers = await employees.GetAll().AsNoTracking()
                    .Where(e => e.Position != null && e.Position.OrganizationUnitId == unit)
                    .Select(e => e.Id)
                    .ToListAsync();
                foreach (var id in peers) scope.Add(id);
            }

            // Never themselves: self-delegation is refused by the entity, and leaving it in the set
            // would make the picker offer an option that cannot be saved.
            scope.Remove(approverEmployeeId);
            return scope;
        }

        public async Task<bool> CanDelegateToAsync(Guid approverEmployeeId, Guid candidateEmployeeId)
        {
            if (approverEmployeeId == candidateEmployeeId) return false;
            var scope = await DelegatableEmployeeIdsAsync(approverEmployeeId);
            return scope.Contains(candidateEmployeeId);
        }
    }
}
