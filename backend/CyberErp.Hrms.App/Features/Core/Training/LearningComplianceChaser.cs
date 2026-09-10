using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    public interface ILearningComplianceChaser
    {
        /// <summary>On-demand from the compliance screen. HR only — it messages the whole workforce.</summary>
        Task<ComplianceRunResult> RunAsync();

        /// <summary>The nightly Hangfire pass. No signed-in user, so no authorisation here.</summary>
        Task<ComplianceRunResult> RunUnattendedAsync();
    }

    /// <summary>
    /// Reconciles the obligations, then chases the ones that are outstanding.
    ///
    /// <para>⚠️ RunUnattendedAsync, NOT RunAsync, is what Hangfire calls. The HR guard belongs on the
    /// on-demand path only: a background job has no HTTP context, so the signed-in user is null and
    /// an admin check there rejects the job on every single run — the exact bug that broke the trip
    /// settlement reminders for weeks (§12.73).</para>
    ///
    /// <para>Two levels, and the thresholds are the whole design:</para>
    /// <list type="bullet">
    /// <item>A REMINDER goes to the learner from <see cref="RemindWithinDays"/> before the deadline
    /// onwards, at most once every <see cref="ReminderCooldownDays"/> days. Without the cooldown a
    /// nightly job mails the same person every night until they comply, which trains people to
    /// filter it.</item>
    /// <item>An ESCALATION goes to their manager once, and only once, after
    /// <see cref="EscalateAfterDays"/> days overdue. Escalating on day one turns the manager's inbox
    /// into the same noise; never escalating leaves compliance with nobody accountable.</item>
    /// </list>
    /// </summary>
    public class LearningComplianceChaser(
        ILearningComplianceEngine engine,
        IRepository<AssignmentObligation> obligationRepository,
        IRepository<LearningAssignment> assignmentRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Employee> employeeRepository,
        IRepository<User> userRepository,
        IOrgManagerResolver managerResolver,
        IPortalNotifier portalNotifier,
        IPerformanceVisibilityService visibility,
        ILogger<LearningComplianceChaser> logger) : ILearningComplianceChaser
    {
        /// <summary>
        /// The only severities CoreNotification accepts. Named here rather than inlined because the
        /// entity throws on anything else, and a typo in a string literal is a sweep that fails after
        /// it has already written half its work.
        /// </summary>
        private static class Severity
        {
            internal const string Info = "Info";
            internal const string Warning = "Warning";
            internal const string Action = "Action";
        }

        /// <summary>Start nudging a fortnight out — enough time to actually book and sit the course.</summary>
        private const int RemindWithinDays = 14;
        private const int ReminderCooldownDays = 7;
        private const int EscalateAfterDays = 7;

        public async Task<ComplianceRunResult> RunAsync()
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin)
                throw new ValidationException("access", "Only HR can run the compliance sweep.");

            return await RunUnattendedAsync();
        }

        public async Task<ComplianceRunResult> RunUnattendedAsync()
        {
            var result = await engine.ReconcileAsync();
            var today = DateTime.UtcNow.Date;

            var due = await obligationRepository.GetAll()
                .Where(o => o.Status == ObligationStatus.Pending
                    && o.DueOn <= today.AddDays(RemindWithinDays))
                .ToListAsync();
            if (due.Count == 0) return result;

            var assignmentIds = due.Select(o => o.LearningAssignmentId).Distinct().ToList();
            var assignments = await assignmentRepository.GetAll().AsNoTracking()
                .Where(a => assignmentIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Name, a.TrainingCourseId })
                .ToListAsync();
            var courseIds = assignments.Select(a => a.TrainingCourseId).Distinct().ToList();
            var courseNames = await courseRepository.GetAll().AsNoTracking()
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var employeeIds = due.Select(o => o.EmployeeId).Distinct().ToList();
            // One query for the learner's own login, rather than one per obligation.
            var usersByEmployee = await userRepository.GetAll().AsNoTracking()
                .Where(u => u.EmployeeId != null && employeeIds.Contains(u.EmployeeId.Value))
                .Select(u => new { u.Id, EmployeeId = u.EmployeeId!.Value })
                .ToListAsync();

            // Warm the employee→unit lookup before resolving any manager: without it each escalation
            // costs a round-trip of its own.
            await managerResolver.PreloadEmployeeUnitsAsync(employeeIds);

            foreach (var obligation in due)
            {
                var assignment = assignments.FirstOrDefault(a => a.Id == obligation.LearningAssignmentId);
                if (assignment is null) continue;
                var courseName = courseNames.TryGetValue(assignment.TrainingCourseId, out var n)
                    ? n : assignment.Name;

                var overdueDays = (today - obligation.DueOn.Date).Days;
                var learnerUserIds = usersByEmployee
                    .Where(u => u.EmployeeId == obligation.EmployeeId)
                    .Select(u => u.Id).ToList();

                var cooledDown = obligation.LastReminderOn is null
                    || (today - obligation.LastReminderOn.Value.Date).Days >= ReminderCooldownDays;

                if (learnerUserIds.Count > 0 && cooledDown)
                {
                    var overdue = overdueDays > 0;
                    await portalNotifier.NotifyUsersAsync(
                        learnerUserIds,
                        overdue ? $"Overdue training: {courseName}" : $"Training due: {courseName}",
                        overdue
                            ? $"'{courseName}' was due on {obligation.DueOn:dd MMM yyyy} — {overdueDays} day(s) ago."
                            : $"'{courseName}' is due on {obligation.DueOn:dd MMM yyyy}.",
                        "/myLearning",
                        // ⚠️ Capitalised: CoreNotification.Create accepts only "Info", "Warning" or
                        // "Action" and THROWS on anything else. An overdue item is an Action — it is
                        // something the learner must now do, not merely be told about.
                        overdue ? Severity.Action : Severity.Info,
                        nameof(AssignmentObligation), obligation.Id);

                    obligation.MarkReminded(today);
                    obligationRepository.UpdateAsync(obligation);
                    result.RemindersSent++;
                }

                // Escalate ONCE, and only when it is genuinely late.
                if (overdueDays >= EscalateAfterDays && obligation.EscalatedOn is null)
                {
                    var manager = await managerResolver.ResolveImmediateManagerAsync(obligation.EmployeeId);
                    if (manager is not null && manager.UserIds.Count > 0)
                    {
                        var who = await employeeRepository.GetAll().AsNoTracking()
                            .Where(e => e.Id == obligation.EmployeeId)
                            .Select(e => e.Person != null
                                ? e.Person.FirstName + " " + e.Person.GrandFatherName
                                : e.EmployeeNumber)
                            .FirstOrDefaultAsync() ?? "An employee";

                        await portalNotifier.NotifyUsersAsync(
                            manager.UserIds,
                            $"Overdue training in your team: {courseName}",
                            $"{who} has not completed '{courseName}', due {obligation.DueOn:dd MMM yyyy} ({overdueDays} day(s) overdue).",
                            "/myLearning",
                            Severity.Warning,
                            nameof(AssignmentObligation), obligation.Id);

                        obligation.MarkEscalated(today);
                        obligationRepository.UpdateAsync(obligation);
                        result.Escalations++;
                    }
                    else
                    {
                        // Not an error worth failing the sweep for, but worth seeing: an unplaced
                        // employee or a unit with no designated manager has nobody to escalate to.
                        logger.LogWarning(
                            "No manager to escalate obligation {Id} for employee {EmployeeId}",
                            obligation.Id, obligation.EmployeeId);
                    }
                }
            }

            if (result.RemindersSent + result.Escalations > 0)
                await obligationRepository.SaveChangesAsync();

            logger.LogInformation(
                "Compliance chase: {Reminders} reminder(s), {Escalations} escalation(s) over {Due} outstanding",
                result.RemindersSent, result.Escalations, due.Count);
            return result;
        }
    }
}
