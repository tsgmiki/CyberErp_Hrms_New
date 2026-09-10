using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    /// <summary>What one nightly pass did, so the on-demand endpoint can report it back.</summary>
    public class ComplianceRunResult
    {
        public int ObligationsCreated { get; set; }
        public int ObligationsSatisfied { get; set; }
        public int NextCyclesOpened { get; set; }
        public int RemindersSent { get; set; }
        public int Escalations { get; set; }
    }

    public interface ILearningComplianceEngine
    {
        /// <summary>The nightly pass: materialise, satisfy, recur. No notifications.</summary>
        Task<ComplianceRunResult> ReconcileAsync();
    }

    /// <summary>
    /// Turns assignment RULES into the individual obligations that are chased and reported on.
    ///
    /// <para>Three passes, in this order, because each depends on the one before:</para>
    /// <list type="number">
    /// <item>MATERIALISE — everyone in an active assignment's audience who has no obligation yet gets
    /// cycle 1. This is what makes an assignment apply to people who join later.</item>
    /// <item>SATISFY — an outstanding obligation whose employee has a completed enrolment on that
    /// course, completed on or after the cycle started, is closed against that enrolment.</item>
    /// <item>RECUR — a completed obligation on a repeating assignment opens the next cycle, dated
    /// from the COMPLETION rather than from a calendar, which is what "valid for 12 months" means.</item>
    /// </list>
    ///
    /// <para>⚠️ Idempotent by construction, and it has to be: this runs every night and on demand,
    /// and a second run must change nothing. The unique index on (assignment, employee, cycle) is the
    /// backstop, but every pass also checks before it writes (logic §12.88).</para>
    ///
    /// <para>⚠️ NO TENANT CONTEXT. Hangfire invokes this with no signed-in user, exactly like the
    /// due-movements and trip-settlement jobs — so it must never call anything that authorises
    /// against the current user. That mistake cost a day on the settlement reminders (§12.73).</para>
    /// </summary>
    public class LearningComplianceEngine(
        IRepository<LearningAssignment> assignmentRepository,
        IRepository<AssignmentObligation> obligationRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<OrganizationUnit> unitRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        ILogger<LearningComplianceEngine> logger) : ILearningComplianceEngine
    {
        public async Task<ComplianceRunResult> ReconcileAsync()
        {
            var result = new ComplianceRunResult();
            var today = DateTime.UtcNow.Date;

            var assignments = await assignmentRepository.GetAll().AsNoTracking()
                .Where(a => a.IsActive)
                .ToListAsync();
            if (assignments.Count == 0) return result;

            foreach (var assignment in assignments)
            {
                var audience = await ResolveAudienceAsync(assignment);
                if (audience.Count == 0) continue;

                var existing = await obligationRepository.GetAll().AsNoTracking()
                    .Where(o => o.LearningAssignmentId == assignment.Id)
                    .Select(o => new { o.EmployeeId, o.CycleNumber })
                    .ToListAsync();
                var haveAny = existing.Select(e => e.EmployeeId).ToHashSet();

                // ---- 1. materialise ------------------------------------------------
                foreach (var employee in audience.Where(e => !haveAny.Contains(e.Id)))
                {
                    // A cycle cannot start before the person joined, or before the rule existed —
                    // otherwise a new assignment would land on long-serving staff already overdue.
                    var start = Max(employee.HireDate?.Date ?? today, assignment.CreatedAt.ToDateTimeUtc().Date);
                    if (start > today) start = today;

                    var obligation = AssignmentObligation.Create(assignment.Id, employee.Id, 1,
                        start, assignment.DueDateFor(start, 1));
                    if (string.IsNullOrEmpty(obligation.TenantId)) obligation.TenantId = assignment.TenantId;
                    await obligationRepository.AddAsync(obligation);
                    result.ObligationsCreated++;
                }
                if (result.ObligationsCreated > 0) await obligationRepository.SaveChangesAsync();
            }

            result.ObligationsSatisfied = await SatisfyAsync();
            result.NextCyclesOpened = await OpenNextCyclesAsync(assignments, today);

            if (result.ObligationsCreated + result.ObligationsSatisfied + result.NextCyclesOpened > 0)
                logger.LogInformation(
                    "Compliance reconcile: {Created} created, {Satisfied} satisfied, {Next} next cycles",
                    result.ObligationsCreated, result.ObligationsSatisfied, result.NextCyclesOpened);

            return result;
        }

        private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

        /// <summary>
        /// The employees an assignment currently applies to.
        ///
        /// <para>Terminated staff are excluded everywhere: chasing a leaver for training is noise,
        /// and it would make every compliance percentage wrong.</para>
        /// </summary>
        private async Task<List<AudienceMember>> ResolveAudienceAsync(LearningAssignment assignment)
        {
            var employees = employeeRepository.GetAll().AsNoTracking().Where(e => !e.IsTerminated);

            switch (assignment.Audience)
            {
                case AssignmentAudience.Everyone:
                    break;

                case AssignmentAudience.Branch:
                    employees = employees.Where(e => e.BranchId == assignment.AudienceId);
                    break;

                case AssignmentAudience.PositionClassAudience:
                {
                    var positionIds = await positionRepository.GetAll().AsNoTracking()
                        .Where(p => p.PositionClassId == assignment.AudienceId!.Value)
                        .Select(p => p.Id).ToListAsync();
                    employees = employees.Where(e => e.PositionId != null && positionIds.Contains(e.PositionId.Value));
                    break;
                }

                case AssignmentAudience.OrganizationUnit:
                {
                    var unitIds = assignment.IncludeSubUnits
                        ? await DescendantUnitsAsync(assignment.AudienceId!.Value)
                        : [assignment.AudienceId!.Value];
                    var positionIds = await positionRepository.GetAll().AsNoTracking()
                        .Where(p => unitIds.Contains(p.OrganizationUnitId))
                        .Select(p => p.Id).ToListAsync();
                    employees = employees.Where(e => e.PositionId != null && positionIds.Contains(e.PositionId.Value));
                    break;
                }
            }

            return await employees
                .Select(e => new AudienceMember { Id = e.Id, HireDate = e.HireDate })
                .ToListAsync();
        }

        /// <summary>
        /// A unit and everything beneath it.
        ///
        /// <para>Walked in memory over the whole (small) unit list rather than with a recursive CTE:
        /// the org tree is hundreds of rows, and one read beats a query per level. The visited set
        /// stops a cycle in the data from spinning for ever.</para>
        /// </summary>
        private async Task<List<Guid>> DescendantUnitsAsync(Guid rootId)
        {
            var all = await unitRepository.GetAll().AsNoTracking()
                .Select(u => new { u.Id, u.ParentId })
                .ToListAsync();

            var byParent = all.Where(u => u.ParentId.HasValue)
                .GroupBy(u => u.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var result = new List<Guid> { rootId };
            var seen = new HashSet<Guid> { rootId };
            var queue = new Queue<Guid>();
            queue.Enqueue(rootId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!byParent.TryGetValue(current, out var children)) continue;
                foreach (var child in children.Where(seen.Add))
                {
                    result.Add(child);
                    queue.Enqueue(child);
                }
            }
            return result;
        }

        /// <summary>
        /// Closes outstanding obligations against completions that happened inside their cycle.
        ///
        /// <para>⚠️ <c>CompletedOn &gt;= AssignedOn</c> is the rule that makes recertification real. A
        /// completion from the previous cycle must not satisfy this one, or nobody is ever re-trained
        /// and every compliance report is a lie.</para>
        /// </summary>
        private async Task<int> SatisfyAsync()
        {
            var outstanding = await obligationRepository.GetAll()
                .Where(o => o.Status == ObligationStatus.Pending)
                .ToListAsync();
            if (outstanding.Count == 0) return 0;

            var assignmentIds = outstanding.Select(o => o.LearningAssignmentId).Distinct().ToList();
            var courseOf = await assignmentRepository.GetAll().AsNoTracking()
                .Where(a => assignmentIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.TrainingCourseId);

            var employeeIds = outstanding.Select(o => o.EmployeeId).Distinct().ToList();
            var courseIds = courseOf.Values.Distinct().ToList();

            // One read of every relevant completion, joined to its course through the session.
            var completions = await enrollmentRepository.GetAll().AsNoTracking()
                .Where(e => e.Status == TrainingEnrollmentStatus.Completed
                    && e.CompletedOn != null
                    && employeeIds.Contains(e.EmployeeId))
                .Join(sessionRepository.GetAll().AsNoTracking(),
                    e => e.TrainingSessionId, s => s.Id,
                    (e, s) => new { e.Id, e.EmployeeId, e.CompletedOn, s.TrainingCourseId })
                .Where(x => courseIds.Contains(x.TrainingCourseId))
                .ToListAsync();

            var satisfied = 0;
            foreach (var obligation in outstanding)
            {
                if (!courseOf.TryGetValue(obligation.LearningAssignmentId, out var courseId)) continue;

                var hit = completions
                    .Where(c => c.EmployeeId == obligation.EmployeeId
                        && c.TrainingCourseId == courseId
                        && c.CompletedOn!.Value.Date >= obligation.AssignedOn.Date)
                    // The earliest qualifying completion, so the recertification clock starts from
                    // when they actually became compliant rather than from their latest refresher.
                    .OrderBy(c => c.CompletedOn)
                    .FirstOrDefault();
                if (hit is null) continue;

                obligation.Satisfy(hit.Id, hit.CompletedOn!.Value);
                obligationRepository.UpdateAsync(obligation);
                satisfied++;
            }

            if (satisfied > 0) await obligationRepository.SaveChangesAsync();
            return satisfied;
        }

        /// <summary>
        /// Opens the next cycle once a completed obligation's recertification window has elapsed.
        ///
        /// <para>Dated from the COMPLETION, not the calendar: "valid for 12 months" means twelve
        /// months from the day they passed, which is also what the certificate on the wall says.</para>
        /// </summary>
        private async Task<int> OpenNextCyclesAsync(List<LearningAssignment> assignments, DateTime today)
        {
            var repeating = assignments.Where(a => a.RecurrenceMonths.HasValue).ToList();
            if (repeating.Count == 0) return 0;

            var ids = repeating.Select(a => a.Id).ToList();
            var completed = await obligationRepository.GetAll().AsNoTracking()
                .Where(o => ids.Contains(o.LearningAssignmentId)
                    && o.Status == ObligationStatus.Completed
                    && o.CompletedOn != null)
                .Select(o => new { o.Id, o.LearningAssignmentId, o.EmployeeId, o.CycleNumber, o.CompletedOn, o.TenantId })
                .ToListAsync();
            if (completed.Count == 0) return 0;

            // Only the latest cycle per person can open a successor; older ones already have one.
            var latest = completed
                .GroupBy(o => new { o.LearningAssignmentId, o.EmployeeId })
                .Select(g => g.OrderByDescending(x => x.CycleNumber).First())
                .ToList();

            var existingCycles = await obligationRepository.GetAll().AsNoTracking()
                .Where(o => ids.Contains(o.LearningAssignmentId))
                .Select(o => new { o.LearningAssignmentId, o.EmployeeId, o.CycleNumber })
                .ToListAsync();

            // Resolved once per assignment, not once per person: this loop can be thousands of rows
            // wide and the audience query is the expensive part.
            var audiences = new Dictionary<Guid, HashSet<Guid>>();
            foreach (var assignment in repeating)
                audiences[assignment.Id] = (await ResolveAudienceAsync(assignment)).Select(a => a.Id).ToHashSet();

            var opened = 0;
            foreach (var prior in latest)
            {
                var assignment = repeating.First(a => a.Id == prior.LearningAssignmentId);
                var nextStart = prior.CompletedOn!.Value.Date.AddMonths(assignment.RecurrenceMonths!.Value);
                if (nextStart > today) continue;   // still certified

                var nextCycle = prior.CycleNumber + 1;
                if (existingCycles.Any(e => e.LearningAssignmentId == prior.LearningAssignmentId
                        && e.EmployeeId == prior.EmployeeId && e.CycleNumber == nextCycle))
                    continue;

                // Someone who has moved out of the unit — or left — is not chased again.
                if (!audiences[assignment.Id].Contains(prior.EmployeeId)) continue;

                var obligation = AssignmentObligation.Create(assignment.Id, prior.EmployeeId, nextCycle,
                    nextStart, assignment.DueDateFor(nextStart, nextCycle));
                if (string.IsNullOrEmpty(obligation.TenantId)) obligation.TenantId = prior.TenantId;
                await obligationRepository.AddAsync(obligation);
                opened++;
            }

            if (opened > 0) await obligationRepository.SaveChangesAsync();
            return opened;
        }

        private sealed class AudienceMember
        {
            public Guid Id { get; init; }
            public DateTime? HireDate { get; init; }
        }
    }
}
