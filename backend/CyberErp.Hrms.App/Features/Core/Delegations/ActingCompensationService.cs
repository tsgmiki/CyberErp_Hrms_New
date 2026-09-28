using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    public interface IActingCompensationService
    {
        /// <summary>
        /// Raise an acting assignment when this delegation is long enough to qualify. No-op when
        /// the feature is off, the delegation is short, or one already exists.
        /// </summary>
        Task<ActingAssignment?> RaiseIfQualifyingAsync(ApprovalDelegation delegation);

        /// <summary>End any assignment behind a withdrawn delegation, reverting pay if it started.</summary>
        Task CancelForDelegationAsync(Guid delegationId, string? reason);

        /// <summary>
        /// Conclude assignments whose end date has passed: revert pay, and record the period as
        /// experience. Returns how many were concluded.
        /// </summary>
        Task<int> ConcludeDueAsync();
    }

    /// <summary>
    /// Turns a long delegation into a paid acting appointment, and unwinds it afterwards.
    /// </summary>
    /// <remarks>
    /// <para>The client's rule: past a configured threshold, standing in stops being a favour and
    /// becomes a job. <c>DelegationPolicy</c> owns every number — whether this happens at all, how
    /// long is long enough, and whether the period counts as experience — so the rule is the
    /// client's rather than the code's.</para>
    ///
    /// <para>⚠️ QUALIFICATION IS ON PLANNED LENGTH, not elapsed. The pay change has to be approved
    /// before it starts, and approval cannot be applied retroactively three months into an
    /// assignment somebody has already been underpaid for. A delegation shortened by an early
    /// withdrawal takes its assignment down with it.</para>
    ///
    /// <para>⚠️ The acting rate comes from the POST, through its PositionClass's salary scale —
    /// never from what the covered approver personally earns. A delegator paid above scale, or
    /// still on an old figure, would otherwise pass their own pay history to their deputy.</para>
    /// </remarks>
    public class ActingCompensationService(
        IRepository<ActingAssignment> assignments,
        IRepository<ApprovalDelegation> delegations,
        IRepository<Employee> employees,
        IRepository<Position> positions,
        IRepository<EmployeeExperience> experiences,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IDelegationEligibilityService eligibility,
        IActingEntitlementService entitlements,
        IWorkflowService workflowService,
        ILogger<ActingCompensationService> logger) : IActingCompensationService
    {
        public async Task<ActingAssignment?> RaiseIfQualifyingAsync(ApprovalDelegation delegation)
        {
            var policy = await eligibility.GetPolicyAsync();
            if (!policy.ActingCompensationEnabled) return null;

            var plannedDays = (int)(delegation.EndDate.Date - delegation.StartDate.Date).TotalDays + 1;
            if (plannedDays <= policy.ActingCompensationMinDays) return null;

            // Amending a delegation must not raise a second assignment for the same cover.
            var existing = await assignments.GetAll()
                .FirstOrDefaultAsync(a => a.DelegationId == delegation.Id
                    && (a.Status == ActingAssignmentStatus.PendingApproval
                        || a.Status == ActingAssignmentStatus.Active));
            if (existing is not null) return existing;

            var covered = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == delegation.FromEmployeeId)
                .Select(e => new { e.PositionId })
                .FirstOrDefaultAsync();

            // ⚠️ The post's rate, from its class's scale. An approver with no position has no post
            // to be paid for, so there is nothing to compensate and the delegation stands alone.
            var post = covered?.PositionId is null ? null : await positions.GetAll().AsNoTracking()
                .Where(p => p.Id == covered.PositionId!.Value)
                .Select(p => new
                {
                    p.Id,
                    Title = p.PositionClass != null ? p.PositionClass.Title : p.Code,
                    Salary = p.PositionClass != null && p.PositionClass.SalaryScale != null
                        ? (decimal?)p.PositionClass.SalaryScale.Salary
                        : null
                })
                .FirstOrDefaultAsync();

            if (post?.Salary is not decimal actingSalary)
            {
                logger.LogInformation(
                    "Delegation {Id} runs {Days} days but the covered post has no salary scale — "
                    + "no acting assignment raised.", delegation.Id, plannedDays);
                return null;
            }

            var deputySalary = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == delegation.ToEmployeeId)
                .Select(e => e.Salary)
                .FirstOrDefaultAsync();

            // ⚠️ An acting rate at or below what the deputy already earns is not compensation, it
            // is a pay cut wearing the word "acting". Cover the post, keep your own salary.
            if (deputySalary is decimal own && actingSalary <= own)
            {
                logger.LogInformation(
                    "Delegation {Id}: the post's rate ({Acting}) does not exceed the deputy's own "
                    + "({Own}) — no acting assignment raised.", delegation.Id, actingSalary, own);
                return null;
            }

            var assignment = ActingAssignment.Create(
                delegation.Id, delegation.ToEmployeeId, delegation.FromEmployeeId,
                post.Id, post.Title ?? string.Empty,
                delegation.StartDate, delegation.EndDate,
                actingSalary, deputySalary,
                $"Raised automatically: the delegation runs {plannedDays} days, beyond the "
                + $"{policy.ActingCompensationMinDays}-day acting threshold.");

            await assignments.AddAsync(assignment);
            await assignments.SaveChangesAsync();

            await StartApprovalAsync(assignment);
            logger.LogInformation(
                "Acting assignment {Id} raised for delegation {DelegationId}: {Days} days at {Salary}",
                assignment.Id, delegation.Id, plannedDays, actingSalary);
            return assignment;
        }

        /// <summary>
        /// Put the assignment in front of an approver.
        /// </summary>
        /// <remarks>
        /// ⚠️ With NO workflow configured the assignment stays pending rather than activating
        /// itself. A pay change that approves itself because nobody set up a chain is the failure
        /// this whole approval step exists to prevent — better a visibly stuck assignment somebody
        /// chases than a silent rise nobody signed.
        /// </remarks>
        private async Task StartApprovalAsync(ActingAssignment assignment)
        {
            var hasWorkflow = await workflowDefinitions.GetAll()
                .AnyAsync(d => d.IsActive && d.EntityType == WorkflowEntityTypes.ActingAssignment);
            if (!hasWorkflow)
            {
                logger.LogWarning(
                    "Acting assignment {Id} has no active '{Type}' workflow — it stays pending, and "
                    + "no pay changes, until one is configured.",
                    assignment.Id, WorkflowEntityTypes.ActingAssignment);
                return;
            }

            var deputy = await EmployeeNameAsync(assignment.EmployeeId);
            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.ActingAssignment, assignment.Id, assignment.EmployeeId,
                $"Acting pay — {deputy} covering {assignment.PositionTitle} "
                + $"({assignment.StartDate:yyyy-MM-dd} to {assignment.EndDate:yyyy-MM-dd})");
        }

        public async Task CancelForDelegationAsync(Guid delegationId, string? reason)
        {
            var open = await assignments.GetAll()
                .Where(a => a.DelegationId == delegationId
                    && (a.Status == ActingAssignmentStatus.PendingApproval
                        || a.Status == ActingAssignmentStatus.Active))
                .ToListAsync();
            if (open.Count == 0) return;

            foreach (var assignment in open)
            {
                var wasPaying = assignment.Cancel(reason);
                if (wasPaying)
                {
                    await RevertPayAsync(assignment);
                    // Cut short: the entitlements end TODAY, not on the date the cover was
                    // originally meant to run to.
                    await entitlements.WithdrawAsync(assignment, DateTime.UtcNow.Date);
                }
                assignments.UpdateAsync(assignment);
            }
            await assignments.SaveChangesAsync();
            logger.LogInformation("Cancelled {Count} acting assignment(s) for delegation {Id}",
                open.Count, delegationId);
        }

        public async Task<int> ConcludeDueAsync()
        {
            var today = DateTime.UtcNow.Date;
            var due = await assignments.GetAll()
                .Where(a => a.Status == ActingAssignmentStatus.Active && a.EndDate < today)
                .ToListAsync();
            if (due.Count == 0) return 0;

            var policy = await eligibility.GetPolicyAsync();

            foreach (var assignment in due)
            {
                await RevertPayAsync(assignment);
                // Ran its course, so the entitlements end on the day the cover did. Most will
                // already have expired on their own window; this closes any that did not.
                await entitlements.WithdrawAsync(assignment, assignment.EndDate);

                var recorded = false;
                if (policy.RecordActingExperience)
                    recorded = await RecordExperienceAsync(assignment);

                assignment.Conclude(recorded);
                assignments.UpdateAsync(assignment);
            }

            await assignments.SaveChangesAsync();
            logger.LogInformation("Concluded {Count} acting assignment(s)", due.Count);
            return due.Count;
        }

        private async Task RevertPayAsync(ActingAssignment assignment)
        {
            var deputy = await employees.GetAll().FirstOrDefaultAsync(e => e.Id == assignment.EmployeeId);
            if (deputy is null) return;
            deputy.RevertActingSalary(assignment.OriginalSalary);
            employees.UpdateAsync(deputy);
        }

        /// <summary>
        /// Write the acting period into the deputy's experience record.
        /// </summary>
        /// <remarks>
        /// ⚠️ <c>IsExternal = false</c>: this is service with THIS organisation, not prior
        /// employment elsewhere. The distinction matters beyond tidiness — the annual-leave accrual
        /// policy can be configured to count external experience separately, and mislabelling an
        /// internal acting stint as external would feed a different number into leave entitlement.
        ///
        /// <para>It closes a loop worth noticing: <c>DelegationEligibilityService</c> counts exactly
        /// these rows when judging whether somebody is senior enough to stand in, so covering a post
        /// once helps qualify you to cover one again.</para>
        /// </remarks>
        private async Task<bool> RecordExperienceAsync(ActingAssignment assignment)
        {
            var personId = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == assignment.EmployeeId)
                .Select(e => (Guid?)e.PersonId)
                .FirstOrDefaultAsync();
            if (personId is null) return false;

            var organisation = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == assignment.EmployeeId && e.Branch != null)
                .Select(e => e.Branch!.Name)
                .FirstOrDefaultAsync();

            var row = EmployeeExperience.Create(
                personId.Value,
                organization: string.IsNullOrWhiteSpace(organisation) ? "Internal" : organisation,
                jobTitle: $"Acting {assignment.PositionTitle}".Trim(),
                startDate: assignment.StartDate,
                endDate: assignment.EndDate,
                responsibilities: $"Acting appointment covering {assignment.PositionTitle} for "
                    + $"{assignment.DurationDays} day(s) under an approved delegation.",
                isExternal: false,
                isGovernmental: false,
                salary: assignment.ActingSalary);

            await experiences.AddAsync(row);
            return true;
        }

        private async Task<string> EmployeeNameAsync(Guid employeeId) =>
            await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => e.Person != null
                    ? e.Person.FirstName + " " + e.Person.GrandFatherName
                    : e.EmployeeNumber)
                .FirstOrDefaultAsync() ?? "the deputy";
    }
}
