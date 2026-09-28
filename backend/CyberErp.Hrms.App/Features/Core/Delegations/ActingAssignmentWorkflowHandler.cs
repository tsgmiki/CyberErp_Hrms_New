using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    /// <summary>
    /// Applies the outcome of an acting-compensation approval.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ THIS IS THE ONLY PLACE A DEPUTY'S PAY CHANGES. The delegation that raised the
    /// assignment can be created self-service by a department head; the money it implies cannot.
    /// Everything up to this point is a proposal, and an assignment that is never approved simply
    /// sits there having cost nobody anything.</para>
    ///
    /// <para>The mirror of every other <c>IWorkflowEntityHandler</c>: the engine owns the chain and
    /// the decision, the module owns what the decision MEANS.</para>
    /// </remarks>
    public class ActingAssignmentWorkflowHandler(
        IRepository<ActingAssignment> assignments,
        IRepository<Employee> employees,
        IActingEntitlementService entitlements,
        ILogger<ActingAssignmentWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) => entityType == WorkflowEntityTypes.ActingAssignment;

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var assignment = await assignments.GetAll().FirstOrDefaultAsync(a => a.Id == entityId);
            if (assignment is null || assignment.Status != ActingAssignmentStatus.PendingApproval)
                return;

            var deputy = await employees.GetAll().FirstOrDefaultAsync(e => e.Id == assignment.EmployeeId);
            if (deputy is null)
            {
                logger.LogWarning("Acting assignment {Id}: the deputy no longer exists — not activated.", entityId);
                return;
            }

            // ⚠️ Re-snapshot what they are on RIGHT NOW, not what they were on when the assignment
            // was raised. Approval can be days or weeks later, and an ordinary increment in between
            // would otherwise be silently erased when the assignment eventually reverts.
            if (deputy.Salary != assignment.ActingSalary)
                assignment.CaptureOriginalSalary(deputy.Salary);

            deputy.ApplyActingSalary(assignment.ActingSalary);
            assignment.Activate();

            employees.UpdateAsync(deputy);
            assignments.UpdateAsync(assignment);
            await assignments.SaveChangesAsync();

            // The post's allowances and benefits follow its salary — that is what "the salary and
            // benefits associated with the position" means. Dated to the assignment, so they
            // expire with it even if nothing ever runs again.
            try
            {
                await entitlements.GrantAsync(assignment);
            }
            catch (Exception ex)
            {
                // The salary is already applied and committed. An entitlement failure is a gap to
                // fix, not a reason to leave somebody half-promoted with no pay change at all.
                logger.LogError(ex,
                    "Acting assignment {Id}: salary applied but post entitlements FAILED to grant.", entityId);
            }

            logger.LogInformation(
                "Acting assignment {Id} activated: {Employee} paid {Salary} until {End:yyyy-MM-dd}",
                entityId, assignment.EmployeeId, assignment.ActingSalary, assignment.EndDate);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var assignment = await assignments.GetAll().FirstOrDefaultAsync(a => a.Id == entityId);
            if (assignment is null || assignment.Status != ActingAssignmentStatus.PendingApproval)
                return;

            // Nothing to unwind — pay never moved. The delegation itself stands: the deputy keeps
            // the authority, the organisation declined to pay extra for it, and those are separate
            // decisions that the refusal of one must not silently reverse in the other.
            assignment.Reject("Acting compensation was not approved.");
            assignments.UpdateAsync(assignment);
            await assignments.SaveChangesAsync();
            logger.LogInformation("Acting assignment {Id} rejected; the delegation is unaffected.", entityId);
        }
    }
}
