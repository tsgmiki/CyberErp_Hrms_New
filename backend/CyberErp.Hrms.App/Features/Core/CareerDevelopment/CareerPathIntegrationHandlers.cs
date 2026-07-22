using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    /// <summary>A suggested career path for an employee (HC163), scored by competency match + performance.</summary>
    public class CareerPathSuggestionDto
    {
        public Guid CareerPathId { get; set; }
        public string CareerPathName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int RequiredCount { get; set; }
        public int MetCount { get; set; }
        /// <summary>Competency coverage 0–100 of the path's entry step.</summary>
        public decimal MatchPercent { get; set; }
        /// <summary>The employee's latest appraisal, normalised 0–100 (same across paths; null if none).</summary>
        public decimal? PerformanceScore { get; set; }
        /// <summary>Overall fit used for ranking (HC163) — blends competency match with performance.</summary>
        public decimal FitScore { get; set; }
        public bool AlreadyAssigned { get; set; }
    }

    public class DevelopmentRecommendationItemDto
    {
        public Guid CompetencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        /// <summary>Suggested development activity to close the gap (HC164).</summary>
        public string SuggestedAction { get; set; } = "Training";
    }

    /// <summary>Development recommendations to progress an employee to their next career step (HC164).</summary>
    public class DevelopmentRecommendationDto
    {
        public Guid EmployeeCareerPathId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? TargetStepId { get; set; }
        public string? TargetStepName { get; set; }
        public int GapCount { get; set; }
        public List<DevelopmentRecommendationItemDto> Recommendations { get; set; } = [];
    }

    public class CreateDevelopmentGoalsResultDto
    {
        public Guid ReviewCycleId { get; set; }
        public int Created { get; set; }
        public int Skipped { get; set; }
        /// <summary>The organizational objective the goals were aligned to (HC167) — null if none applied.</summary>
        public Guid? OrganizationalObjectiveId { get; set; }
        public string? OrganizationalObjectiveTitle { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISuggestCareerPaths { Task<List<CareerPathSuggestionDto>> SuggestAsync(Guid employeeId); }
    public interface IGetCareerPathRecommendations { Task<DevelopmentRecommendationDto> GetAsync(Guid employeeCareerPathId); }
    public interface ICreateDevelopmentGoals { Task<CreateDevelopmentGoalsResultDto> CreateAsync(Guid employeeCareerPathId, Guid? reviewCycleId, Guid? organizationalObjectiveId); }

    // ---- HC163: career-path suggestions -------------------------------------
    /// <summary>
    /// Suggests career paths for an employee (HC163) by scoring how well their CURRENT position's
    /// competencies cover each active path's entry-step requirements, blended with the employee's latest
    /// appraisal (performance reviews). A handful of set-based queries — invoked on demand, never in a
    /// list render.
    /// </summary>
    public class SuggestCareerPaths(
        IRepository<Employee> employeeRepository,
        IRepository<PositionCompetency> positionCompetencyRepository,
        IRepository<CareerPath> pathRepository,
        IRepository<CareerPathStep> stepRepository,
        IRepository<CareerPathStepCompetency> stepCompetencyRepository,
        IRepository<EmployeeCareerPath> assignmentRepository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository) : ISuggestCareerPaths
    {
        public async Task<List<CareerPathSuggestionDto>> SuggestAsync(Guid employeeId)
        {
            var positionId = await employeeRepository.GetAll().Where(e => e.Id == employeeId)
                .Select(e => e.PositionId).FirstOrDefaultAsync();
            var metSet = positionId.HasValue
                ? (await positionCompetencyRepository.GetAll().Where(pc => pc.PositionId == positionId.Value)
                    .Select(pc => pc.CompetencyId).ToListAsync()).ToHashSet()
                : [];

            // Performance component (HC163) — latest appraisal normalised to 0–100 against its rating scale.
            // Employee-level, so computed once and blended equally into every path's fit.
            var performanceScore = await LatestPerformanceScoreAsync(employeeId);

            var paths = await pathRepository.GetAll().Where(p => p.IsActive)
                .Select(p => new { p.Id, p.Name, p.Code }).ToListAsync();
            var pathIds = paths.Select(p => p.Id).ToList();

            // Entry step = the lowest-order step of each path.
            var steps = await stepRepository.GetAll().Where(s => pathIds.Contains(s.CareerPathId))
                .Select(s => new { s.Id, s.CareerPathId, s.StepOrder }).ToListAsync();
            var entryStepByPath = steps.GroupBy(s => s.CareerPathId)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StepOrder).First().Id);
            var entryStepIds = entryStepByPath.Values.ToList();

            var entryComps = await stepCompetencyRepository.GetAll()
                .Where(sc => entryStepIds.Contains(sc.CareerPathStepId))
                .Select(sc => new { sc.CareerPathStepId, sc.CompetencyId }).ToListAsync();
            var compsByStep = entryComps.GroupBy(c => c.CareerPathStepId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.CompetencyId).ToList());

            var assignedPaths = (await assignmentRepository.GetAll()
                .Where(a => a.EmployeeId == employeeId).Select(a => a.CareerPathId).ToListAsync()).ToHashSet();

            var result = paths.Select(p =>
            {
                var required = entryStepByPath.TryGetValue(p.Id, out var stepId) && compsByStep.TryGetValue(stepId, out var cs) ? cs : [];
                var met = required.Count(metSet.Contains);
                var matchPercent = required.Count > 0 ? Math.Round((decimal)met / required.Count * 100m, 1) : 0m;
                // Fit = average of competency match + performance when an appraisal exists, else match only.
                var fit = performanceScore.HasValue ? Math.Round((matchPercent + performanceScore.Value) / 2m, 1) : matchPercent;
                return new CareerPathSuggestionDto
                {
                    CareerPathId = p.Id, CareerPathName = p.Name, Code = p.Code,
                    RequiredCount = required.Count, MetCount = met,
                    MatchPercent = matchPercent, PerformanceScore = performanceScore, FitScore = fit,
                    AlreadyAssigned = assignedPaths.Contains(p.Id),
                };
            }).OrderByDescending(x => x.FitScore).ThenByDescending(x => x.MatchPercent).ThenBy(x => x.CareerPathName).ToList();

            return result;
        }

        /// <summary>Employee's latest appraisal overall score, normalised 0–100 against its rating scale max (null if none).</summary>
        private async Task<decimal?> LatestPerformanceScoreAsync(Guid employeeId)
        {
            var latest = await appraisalRepository.GetAll().AsNoTracking()
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new { a.OverallScore, a.ReviewCycleId })
                .FirstOrDefaultAsync();
            if (latest?.OverallScore is not decimal score) return null;

            var scaleId = await reviewCycleRepository.GetAll().Where(c => c.Id == latest.ReviewCycleId)
                .Select(c => c.RatingScaleId).FirstOrDefaultAsync();
            var scaleMax = scaleId != Guid.Empty
                ? await ratingLevelRepository.GetAll().Where(l => l.RatingScaleId == scaleId && l.MaxScore != null)
                    .MaxAsync(l => (decimal?)l.MaxScore)
                : null;
            return scaleMax is decimal max && max > 0 ? Math.Round(Math.Clamp(score / max * 100m, 0m, 100m), 1) : null;
        }
    }

    // ---- HC164: development recommendations ---------------------------------
    /// <summary>
    /// Recommends the competencies an employee should develop to reach the NEXT step on their assigned
    /// career path (HC164) — the next step's required competencies their current position lacks. Small
    /// set-based queries, on demand.
    /// </summary>
    public class GetCareerPathRecommendations(
        IRepository<EmployeeCareerPath> assignmentRepository,
        IRepository<CareerPathStep> stepRepository,
        IRepository<CareerPathStepCompetency> stepCompetencyRepository,
        IRepository<Competency> competencyRepository,
        IRepository<Employee> employeeRepository,
        IRepository<PositionCompetency> positionCompetencyRepository) : IGetCareerPathRecommendations
    {
        public async Task<DevelopmentRecommendationDto> GetAsync(Guid employeeCareerPathId)
        {
            var assignment = await assignmentRepository.GetAll().Where(a => a.Id == employeeCareerPathId)
                    .Select(a => new { a.Id, a.EmployeeId, a.CareerPathId, a.CurrentStepId }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(EmployeeCareerPath), employeeCareerPathId.ToString());

            var steps = await stepRepository.GetAll().Where(s => s.CareerPathId == assignment.CareerPathId)
                .OrderBy(s => s.StepOrder).Select(s => new { s.Id, s.StepOrder, s.Name }).ToListAsync();

            // Target = the step AFTER the current one (the growth target); fall back to the first step,
            // and to the last step if the employee is already at the top.
            var currentIndex = assignment.CurrentStepId.HasValue
                ? steps.FindIndex(s => s.Id == assignment.CurrentStepId.Value) : -1;
            var target = steps.Count == 0 ? null
                : currentIndex < 0 ? steps[0]
                : currentIndex + 1 < steps.Count ? steps[currentIndex + 1]
                : steps[currentIndex];

            var recos = new List<DevelopmentRecommendationItemDto>();
            if (target != null)
            {
                var positionId = await employeeRepository.GetAll().Where(e => e.Id == assignment.EmployeeId)
                    .Select(e => e.PositionId).FirstOrDefaultAsync();
                var metSet = positionId.HasValue
                    ? (await positionCompetencyRepository.GetAll().Where(pc => pc.PositionId == positionId.Value)
                        .Select(pc => pc.CompetencyId).ToListAsync()).ToHashSet()
                    : [];

                var required = await stepCompetencyRepository.GetAll().Where(sc => sc.CareerPathStepId == target.Id)
                    .Join(competencyRepository.GetAll(), sc => sc.CompetencyId, c => c.Id,
                        (sc, c) => new { sc.CompetencyId, c.Name, sc.Weight }).ToListAsync();

                recos = required.Where(r => !metSet.Contains(r.CompetencyId))
                    .Select(r => new DevelopmentRecommendationItemDto
                    {
                        CompetencyId = r.CompetencyId, Name = r.Name, Weight = r.Weight, SuggestedAction = "Training",
                    }).ToList();
            }

            return new DevelopmentRecommendationDto
            {
                EmployeeCareerPathId = employeeCareerPathId,
                EmployeeId = assignment.EmployeeId,
                TargetStepId = target?.Id,
                TargetStepName = target?.Name,
                GapCount = recos.Count,
                Recommendations = recos,
            };
        }
    }

    // ---- HC167: turn gaps into development goals (reuse the EmployeeGoal engine) ----
    /// <summary>
    /// Materialises the career-development gap (HC164) as individual development GOALS on the performance
    /// engine (HC167) — one <see cref="EmployeeGoal"/> per missing competency, tied to a review cycle.
    /// Idempotent: an employee's existing goal with the same title in that cycle is skipped.
    /// </summary>
    public class CreateDevelopmentGoals(
        IGetCareerPathRecommendations recommendationsHandler,
        IRepository<EmployeeGoal> goalRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationalObjective> objectiveRepository,
        IRepository<Employee> employeeRepository,
        ILogger<CreateDevelopmentGoals> logger) : ICreateDevelopmentGoals
    {
        public async Task<CreateDevelopmentGoalsResultDto> CreateAsync(Guid employeeCareerPathId, Guid? reviewCycleId, Guid? organizationalObjectiveId)
        {
            var recos = await recommendationsHandler.GetAsync(employeeCareerPathId);
            if (recos.Recommendations.Count == 0)
                throw new ValidationException("recommendations", "No competency gaps to turn into goals — the employee already meets the next step's requirements.");

            // Resolve the review cycle: the one supplied, else the latest Active, else the most recent.
            var cycleId = reviewCycleId ?? Guid.Empty;
            if (cycleId == Guid.Empty)
            {
                cycleId = await reviewCycleRepository.GetAll().Where(c => c.Status == ReviewCycleStatus.Active)
                    .OrderByDescending(c => c.StartDate).Select(c => c.Id).FirstOrDefaultAsync();
                if (cycleId == Guid.Empty)
                    cycleId = await reviewCycleRepository.GetAll()
                        .OrderByDescending(c => c.StartDate).Select(c => c.Id).FirstOrDefaultAsync();
            }
            if (cycleId == Guid.Empty)
                throw new ValidationException("reviewCycleId", "No review cycle exists to attach the development goals to. Create a review cycle first.");

            // Align the goals to an organizational objective (HC167): the one supplied, else auto-resolve the
            // objective for the employee's org unit in this cycle, else a top-level (org-wide) objective.
            var objective = await ResolveObjectiveAsync(organizationalObjectiveId, recos.EmployeeId, cycleId);

            var now = DateTime.UtcNow;
            var existingTitles = (await goalRepository.GetAll()
                .Where(g => g.EmployeeId == recos.EmployeeId && g.ReviewCycleId == cycleId)
                .Select(g => g.Title).ToListAsync()).ToHashSet();

            var created = 0; var skipped = 0;
            foreach (var r in recos.Recommendations)
            {
                var title = $"Develop: {r.Name}";
                if (existingTitles.Contains(title)) { skipped++; continue; }
                var goal = EmployeeGoal.Create(recos.EmployeeId, cycleId, title, now, now.AddDays(90),
                    description: $"Career development goal — build the '{r.Name}' competency required for the next career-path step.",
                    organizationalObjectiveId: objective?.Id,
                    status: GoalStatus.Active, setByManager: true);
                await goalRepository.AddAsync(goal);
                created++;
            }
            if (created > 0) await goalRepository.SaveChangesAsync();
            logger.LogInformation("Created {Created} development goal(s) (skipped {Skipped}, objective {Obj}) for assignment {Id}",
                created, skipped, objective?.Id, employeeCareerPathId);

            return new CreateDevelopmentGoalsResultDto
            {
                ReviewCycleId = cycleId, Created = created, Skipped = skipped,
                OrganizationalObjectiveId = objective?.Id, OrganizationalObjectiveTitle = objective?.Title,
            };
        }

        /// <summary>
        /// Picks the organizational objective to align the goals to (HC167): an explicit one (validated to
        /// this cycle), else the objective owned by the employee's org unit in this cycle, else a top-level
        /// (org-wide, no unit) objective — favouring Active objectives. Null when none exists.
        /// </summary>
        private async Task<OrganizationalObjective?> ResolveObjectiveAsync(Guid? explicitId, Guid employeeId, Guid cycleId)
        {
            if (explicitId is Guid id && id != Guid.Empty)
                return await objectiveRepository.GetAll().FirstOrDefaultAsync(o => o.Id == id && o.ReviewCycleId == cycleId)
                    ?? throw new ValidationException("organizationalObjectiveId", "The selected organizational objective was not found in the resolved review cycle.");

            var unitId = await employeeRepository.GetAll().Where(e => e.Id == employeeId)
                .Select(e => e.Position != null ? e.Position.OrganizationUnitId : (Guid?)null).FirstOrDefaultAsync();

            var cycleObjectives = objectiveRepository.GetAll().Where(o => o.ReviewCycleId == cycleId);
            OrganizationalObjective? match = null;
            if (unitId.HasValue)
                match = await cycleObjectives.Where(o => o.OrganizationUnitId == unitId.Value)
                    .OrderByDescending(o => o.Status == ObjectiveStatus.Active).ThenByDescending(o => o.Weight).FirstOrDefaultAsync();
            // Fall back to a top-level (org-wide) objective so goals still align to company needs.
            match ??= await cycleObjectives.Where(o => o.OrganizationUnitId == null)
                .OrderByDescending(o => o.Status == ObjectiveStatus.Active).ThenByDescending(o => o.Weight).FirstOrDefaultAsync();
            return match;
        }
    }

    // ---- HC169: workflow-engine outcomes for path change requests -----------
    /// <summary>
    /// Workflow outcomes for career-path change requests (HC169): final approval marks the request
    /// approved and assigns the employee to the requested path; rejection marks it rejected. Registered
    /// like every other module handler — the generic engine drives the routing.
    /// </summary>
    public class CareerPathChangeRequestWorkflowHandler(
        IRepository<CareerPathChangeRequest> repository,
        IRepository<EmployeeCareerPath> assignmentRepository) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.CareerPathChangeRequest, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var request = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(CareerPathChangeRequest), entityId.ToString());
            request.Approve("Approved via workflow");
            repository.UpdateAsync(request);

            var alreadyAssigned = await assignmentRepository.GetAll()
                .AnyAsync(a => a.EmployeeId == request.EmployeeId && a.CareerPathId == request.RequestedCareerPathId);
            if (!alreadyAssigned)
            {
                var assignment = EmployeeCareerPath.Create(request.EmployeeId, request.RequestedCareerPathId,
                    null, "Change request approval", null, EmployeeCareerPathStatus.Active, request.Reason);
                await assignmentRepository.AddAsync(assignment);
            }
            await repository.SaveChangesAsync();
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var request = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId)
                ?? throw new NotFoundException(nameof(CareerPathChangeRequest), entityId.ToString());
            request.Reject("Rejected via workflow");
            repository.UpdateAsync(request);
            await repository.SaveChangesAsync();
        }
    }
}
