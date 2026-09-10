using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    /// <summary>
    /// The compliance dashboard: where the organisation stands, by course and by unit.
    ///
    /// <para>⚠️ Waived obligations leave the DENOMINATOR. An excused row is neither a pass nor a
    /// failure; counting it as compliant flatters the figure and counting it as outstanding punishes
    /// a decision HR deliberately took. Every percentage here is completions over
    /// (completions + outstanding), and waivers are reported separately so the count is still
    /// visible (logic §12.88).</para>
    /// </summary>
    public class GetComplianceOverview(
        IRepository<AssignmentObligation> obligationRepository,
        IRepository<LearningAssignment> assignmentRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<OrganizationUnit> unitRepository) : IGetComplianceOverview
    {
        /// <summary>A deadline inside this window is "due soon" — the same horizon the chaser uses.</summary>
        private const int DueSoonDays = 14;

        public async Task<ComplianceOverviewDto> GetAsync(Guid? trainingCourseId)
        {
            var today = DateTime.UtcNow.Date;

            var assignments = await assignmentRepository.GetAll().AsNoTracking()
                .Select(a => new { a.Id, a.TrainingCourseId })
                .ToListAsync();
            if (trainingCourseId.HasValue)
                assignments = [.. assignments.Where(a => a.TrainingCourseId == trainingCourseId.Value)];

            var assignmentIds = assignments.Select(a => a.Id).ToList();
            if (assignmentIds.Count == 0) return new ComplianceOverviewDto();

            var rows = await obligationRepository.GetAll().AsNoTracking()
                .Where(o => assignmentIds.Contains(o.LearningAssignmentId))
                .Select(o => new Fact(o.LearningAssignmentId, o.EmployeeId, o.Status, o.DueOn))
                .ToListAsync();

            var dto = new ComplianceOverviewDto
            {
                TotalObligations = rows.Count,
                Completed = rows.Count(r => r.Status == ObligationStatus.Completed),
                Overdue = rows.Count(r => r.Status == ObligationStatus.Pending && r.DueOn.Date < today),
                DueSoon = rows.Count(r => r.Status == ObligationStatus.Pending
                    && r.DueOn.Date >= today && r.DueOn.Date <= today.AddDays(DueSoonDays))
            };
            var counting = rows.Count(r => r.Status != ObligationStatus.Waived);
            dto.CompliancePercent = ComplianceShared.Percent(dto.Completed, counting);

            // ---- by course ------------------------------------------------------
            var courseIds = assignments.Select(a => a.TrainingCourseId).Distinct().ToList();
            var courseNames = await courseRepository.GetAll().AsNoTracking()
                .Where(c => courseIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);

            var courseOfAssignment = assignments.ToDictionary(a => a.Id, a => a.TrainingCourseId);
            dto.ByCourse = [.. rows
                .GroupBy(r => courseOfAssignment[r.LearningAssignmentId])
                .Select(g => Row(g.Key, courseNames.TryGetValue(g.Key, out var n) ? n : "(course)", g, today))
                .OrderByDescending(r => r.Overdue).ThenBy(r => r.Name)];

            // ---- by unit --------------------------------------------------------
            var employeeIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
            var people = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => employeeIds.Contains(e.Id))
                .Select(e => new { e.Id, e.PositionId })
                .ToListAsync();
            var positionIds = people.Where(p => p.PositionId.HasValue)
                .Select(p => p.PositionId!.Value).Distinct().ToList();
            var unitOfPosition = await positionRepository.GetAll().AsNoTracking()
                .Where(p => positionIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.OrganizationUnitId);
            var unitNames = await unitRepository.GetAll().AsNoTracking()
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            var unitOfEmployee = people.ToDictionary(
                p => p.Id,
                p => p.PositionId.HasValue && unitOfPosition.TryGetValue(p.PositionId.Value, out var u)
                    ? u : Guid.Empty);

            dto.ByUnit = [.. rows
                .GroupBy(r => unitOfEmployee.TryGetValue(r.EmployeeId, out var u) ? u : Guid.Empty)
                .Select(g => Row(
                    g.Key,
                    // An unplaced employee is a real state in this data, and hiding those rows would
                    // make the totals disagree with the breakdown.
                    g.Key == Guid.Empty ? "(no unit)"
                        : unitNames.TryGetValue(g.Key, out var n) ? n : "(unit)",
                    g, today))
                .OrderByDescending(r => r.Overdue).ThenBy(r => r.Name)];

            return dto;
        }

        /// <summary>One obligation reduced to what any of these tallies needs.</summary>
        private sealed record Fact(Guid LearningAssignmentId, Guid EmployeeId, ObligationStatus Status, DateTime DueOn);

        private static ComplianceRowDto Row(Guid id, string name, IEnumerable<Fact> group, DateTime today)
        {
            var items = group.ToList();
            var completed = items.Count(x => x.Status == ObligationStatus.Completed);
            var waived = items.Count(x => x.Status == ObligationStatus.Waived);
            var overdue = items.Count(x => x.Status == ObligationStatus.Pending && x.DueOn.Date < today);
            var pending = items.Count(x => x.Status == ObligationStatus.Pending) - overdue;

            return new ComplianceRowDto
            {
                Id = id,
                Name = name,
                Total = items.Count,
                Completed = completed,
                Pending = pending,
                Overdue = overdue,
                Waived = waived,
                CompliancePercent = ComplianceShared.Percent(completed, items.Count - waived)
            };
        }
    }

    /// <summary>
    /// Training effectiveness — Kirkpatrick levels 1 and 2 measured, level 3 as a signal.
    ///
    /// <para>⚠️ EVERY FIGURE CARRIES ITS SAMPLE SIZE, and a figure with no data is null rather than
    /// zero. "Average score 0% (0 results)" and "average score 0% (40 results)" mean opposite things,
    /// and a dashboard that renders both as 0 is worse than one that renders neither.</para>
    ///
    /// <para>Level 3 is deliberately labelled a signal, not a measurement: it compares appraisal
    /// scores on the competencies a course develops, before and after completion, and an appraisal is
    /// a judgement made for other reasons. It is evidence worth looking at, not proof the training
    /// worked — anything stronger needs a control group this product will never have.</para>
    /// </summary>
    public class GetTrainingEffectiveness(
        IRepository<TrainingCourse> courseRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<CourseCompetency> courseCompetencyRepository,
        IRepository<AppraisalCompetency> appraisalCompetencyRepository,
        IRepository<Appraisal> appraisalRepository) : IGetTrainingEffectiveness
    {
        public async Task<List<EffectivenessRowDto>> GetAsync(Guid? trainingCourseId)
        {
            var courses = await courseRepository.GetAll().AsNoTracking()
                .Where(c => !trainingCourseId.HasValue || c.Id == trainingCourseId.Value)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            if (courses.Count == 0) return [];

            var courseIds = courses.Select(c => c.Id).ToList();

            // Completions, joined to their course through the session, in one read.
            var completions = await enrollmentRepository.GetAll().AsNoTracking()
                .Where(e => e.Status == TrainingEnrollmentStatus.Completed && e.CompletedOn != null)
                .Join(sessionRepository.GetAll().AsNoTracking(),
                    e => e.TrainingSessionId, s => s.Id,
                    (e, s) => new
                    {
                        e.EmployeeId, e.CompletedOn, e.FeedbackRating, e.AssessmentScore,
                        s.TrainingCourseId
                    })
                .Where(x => courseIds.Contains(x.TrainingCourseId))
                .ToListAsync();

            var competencyMap = await courseCompetencyRepository.GetAll().AsNoTracking()
                .Where(cc => courseIds.Contains(cc.TrainingCourseId))
                .Select(cc => new { cc.TrainingCourseId, cc.CompetencyId })
                .ToListAsync();

            var movement = competencyMap.Count == 0
                ? []
                : await CompetencyMovementAsync(completions
                        .Select(c => new Completion(c.TrainingCourseId, c.EmployeeId, c.CompletedOn!.Value))
                        .ToList(),
                    competencyMap.ToLookup(m => m.TrainingCourseId, m => m.CompetencyId));

            var result = new List<EffectivenessRowDto>();
            foreach (var course in courses)
            {
                var mine = completions.Where(c => c.TrainingCourseId == course.Id).ToList();
                var feedback = mine.Where(c => c.FeedbackRating.HasValue).ToList();
                var scored = mine.Where(c => c.AssessmentScore.HasValue).ToList();
                var hasMovement = movement.TryGetValue(course.Id, out var move);

                var row = new EffectivenessRowDto
                {
                    TrainingCourseId = course.Id,
                    CourseName = course.Name,
                    Completions = mine.Count,
                    FeedbackResponses = feedback.Count,
                    AverageFeedback = feedback.Count == 0
                        ? null : Math.Round((decimal)feedback.Average(c => c.FeedbackRating!.Value), 2),
                    AssessmentResults = scored.Count,
                    AverageAssessmentScore = scored.Count == 0
                        ? null : Math.Round(scored.Average(c => c.AssessmentScore!.Value), 1),
                    CompetencyPairs = hasMovement ? move.Pairs : 0,
                    CompetencyMovement = hasMovement && move.Pairs > 0 ? Math.Round(move.Delta, 2) : null
                };

                // The pass rate is over people who HAVE a score; a course with no quiz has none, and
                // saying so beats printing 0%.
                row.FirstAttemptPassRate = scored.Count == 0
                    ? null
                    : ComplianceShared.Percent(scored.Count(c => c.AssessmentScore >= 60m), scored.Count);

                row.Caveat = mine.Count == 0
                    ? "No completions yet."
                    : scored.Count == 0
                        ? "No assessment on this course, so learning is not measured."
                        : row.CompetencyPairs == 0
                            ? "No before/after appraisal pairs yet, so the behaviour signal is unavailable."
                            : null;

                result.Add(row);
            }

            return [.. result.OrderByDescending(r => r.Completions).ThenBy(r => r.CourseName)];
        }

        private sealed record Completion(Guid CourseId, Guid EmployeeId, DateTime CompletedOn);

        /// <summary>
        /// The average appraisal-score change on a course's competencies, before versus after.
        ///
        /// <para>Pairs only: an employee counts once per competency, and only when they have an
        /// appraisal score on BOTH sides of the completion date. Everyone else is excluded rather
        /// than defaulted, which is what keeps the number honest and the sample size meaningful.</para>
        /// </summary>
        private async Task<Dictionary<Guid, (decimal Delta, int Pairs)>> CompetencyMovementAsync(
            List<Completion> completions, ILookup<Guid, Guid> competenciesOfCourse)
        {
            if (completions.Count == 0) return [];

            var employeeIds = completions.Select(c => c.EmployeeId).Distinct().ToList();
            var competencyIds = competenciesOfCourse.SelectMany(g => g).Distinct().ToList();

            // Every relevant appraisal score, with the date of the appraisal it belongs to.
            var scores = await appraisalCompetencyRepository.GetAll().AsNoTracking()
                .Where(ac => ac.ManagerScore != null && competencyIds.Contains(ac.CompetencyId))
                .Join(appraisalRepository.GetAll().AsNoTracking(),
                    ac => ac.AppraisalId, a => a.Id,
                    (ac, a) => new { ac.CompetencyId, ac.ManagerScore, a.EmployeeId, a.PeriodEnd })
                .Where(x => employeeIds.Contains(x.EmployeeId))
                .ToListAsync();
            if (scores.Count == 0) return [];

            var totals = new Dictionary<Guid, (decimal Sum, int Pairs)>();
            foreach (var completion in completions)
            {
                foreach (var competencyId in competenciesOfCourse[completion.CourseId])
                {
                    var mine = scores
                        .Where(s => s.EmployeeId == completion.EmployeeId && s.CompetencyId == competencyId)
                        .ToList();

                    var before = mine.Where(s => s.PeriodEnd.Date <= completion.CompletedOn.Date)
                        .OrderByDescending(s => s.PeriodEnd).FirstOrDefault();
                    var after = mine.Where(s => s.PeriodEnd.Date > completion.CompletedOn.Date)
                        .OrderBy(s => s.PeriodEnd).FirstOrDefault();
                    if (before is null || after is null) continue;

                    var delta = after.ManagerScore!.Value - before.ManagerScore!.Value;
                    var current = totals.TryGetValue(completion.CourseId, out var v) ? v : (0m, 0);
                    totals[completion.CourseId] = (current.Item1 + delta, current.Item2 + 1);
                }
            }

            return totals.ToDictionary(kv => kv.Key, kv => (kv.Value.Sum / kv.Value.Pairs, kv.Value.Pairs));
        }
    }
}
