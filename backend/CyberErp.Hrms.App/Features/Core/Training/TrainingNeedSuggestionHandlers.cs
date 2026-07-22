using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class TrainingNeedSuggestionDto
    {
        /// <summary>CompetencyGap | Appraisal | Goal (maps to TrainingNeedSource).</summary>
        public string Source { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public Guid? CompetencyId { get; set; }
        public string? CompetencyName { get; set; }
        public Guid? GoalId { get; set; }
    }

    // ---- Interface ----------------------------------------------------------
    /// <summary>
    /// HC189 — performance-driven training needs: low-scored appraisal competencies (the gap), a weak
    /// overall result, and active employee goals each become a one-click suggestion the caller can turn
    /// into a <see cref="TrainingNeed"/>.
    /// </summary>
    public interface IGetTrainingNeedSuggestions { Task<List<TrainingNeedSuggestionDto>> GetAsync(Guid employeeId); }

    // ---- Handler ------------------------------------------------------------
    public class GetTrainingNeedSuggestions(
        IRepository<Appraisal> appraisalRepository,
        IRepository<AppraisalCompetency> appraisalCompetencyRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IRepository<EmployeeGoal> goalRepository,
        IRepository<TrainingNeed> needRepository,
        IPerformanceVisibilityService visibility) : IGetTrainingNeedSuggestions
    {
        /// <summary>Normalised score below which a competency / overall result suggests training.</summary>
        private const decimal LowScorePercent = 60m;

        public async Task<List<TrainingNeedSuggestionDto>> GetAsync(Guid employeeId)
        {
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(employeeId), "The employee is outside your scope.");

            var suggestions = new List<TrainingNeedSuggestionDto>();

            // Skip anything an open (pending/approved) need already covers — no duplicate requests.
            var openCompetencyIds = (await needRepository.GetAll().AsNoTracking()
                .Where(n => n.EmployeeId == employeeId && n.CompetencyId != null
                    && (n.Status == TrainingNeedStatus.Pending || n.Status == TrainingNeedStatus.Approved))
                .Select(n => n.CompetencyId!.Value).ToListAsync()).ToHashSet();

            // ---- Latest scored appraisal → overall + per-competency gaps -----------
            var latest = await appraisalRepository.GetAll().AsNoTracking()
                .Where(a => a.EmployeeId == employeeId && a.OverallScore != null)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new { a.Id, a.OverallScore, a.ReviewCycleId })
                .FirstOrDefaultAsync();

            if (latest?.OverallScore is decimal overall)
            {
                var cycle = await reviewCycleRepository.GetAll().AsNoTracking()
                    .Where(c => c.Id == latest.ReviewCycleId)
                    .Select(c => new { c.Name, c.RatingScaleId })
                    .FirstOrDefaultAsync();
                var max = cycle is null ? 0m : await ratingLevelRepository.GetAll().AsNoTracking()
                    .Where(l => l.RatingScaleId == cycle.RatingScaleId)
                    .Select(l => (decimal?)l.Value).MaxAsync() ?? 0m;

                if (max > 0)
                {
                    var overallPercent = Math.Round(overall / max * 100m, 1);
                    if (overallPercent < LowScorePercent)
                    {
                        suggestions.Add(new TrainingNeedSuggestionDto
                        {
                            Source = nameof(TrainingNeedSource.Appraisal),
                            Title = "Performance improvement training",
                            Rationale = $"Scored {overallPercent}% overall in {cycle!.Name} — below the {LowScorePercent}% development threshold."
                        });
                    }

                    var lowThreshold = max * LowScorePercent / 100m;
                    var lowCompetencies = await appraisalCompetencyRepository.GetAll().AsNoTracking()
                        .Where(c => c.AppraisalId == latest.Id && c.ManagerScore != null && c.ManagerScore < lowThreshold)
                        .OrderBy(c => c.ManagerScore)
                        .Select(c => new { c.CompetencyId, c.CompetencyName, c.ManagerScore })
                        .ToListAsync();

                    suggestions.AddRange(lowCompetencies
                        .Where(c => !openCompetencyIds.Contains(c.CompetencyId))
                        .Select(c => new TrainingNeedSuggestionDto
                        {
                            Source = nameof(TrainingNeedSource.CompetencyGap),
                            Title = $"Develop: {c.CompetencyName}",
                            Rationale = $"Manager-rated {c.ManagerScore:0.#} of {max:0.#} in {cycle!.Name}.",
                            CompetencyId = c.CompetencyId,
                            CompetencyName = c.CompetencyName
                        }));
                }
            }

            // ---- Active goals → goal-aligned learning -------------------------------
            var activeGoals = await goalRepository.GetAll().AsNoTracking()
                .Where(g => g.EmployeeId == employeeId && g.Status == GoalStatus.Active && g.ProgressPercent < 100)
                .OrderBy(g => g.DueDate)
                .Select(g => new { g.Id, g.Title, g.DueDate })
                .Take(10)
                .ToListAsync();

            suggestions.AddRange(activeGoals.Select(g => new TrainingNeedSuggestionDto
            {
                Source = nameof(TrainingNeedSource.Goal),
                Title = $"Training toward goal: {g.Title}",
                Rationale = $"Active goal due {g.DueDate:yyyy-MM-dd} — targeted learning accelerates delivery.",
                GoalId = g.Id
            }));

            return suggestions;
        }
    }
}
