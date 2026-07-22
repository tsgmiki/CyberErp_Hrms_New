using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- DTOs ---------------------------------------------------------------
    public class RatingDistributionRowDto
    {
        public Guid LevelId { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>Manager / HR performance dashboard aggregates (HC134).</summary>
    public class PerformanceDashboardDto
    {
        public int TotalAppraisals { get; set; }
        public int SelfAssessmentCount { get; set; }
        public int ManagerReviewCount { get; set; }
        public int CompletedCount { get; set; }
        /// <summary>Non-completed appraisals whose review cycle end date has passed.</summary>
        public int OverdueReviews { get; set; }
        /// <summary>Completed appraisals still awaiting employee acknowledgment.</summary>
        public int PendingAcknowledgment { get; set; }
        public List<RatingDistributionRowDto> RatingDistribution { get; set; } = [];
        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }
        public decimal AverageGoalProgress { get; set; }
        // Risk indicators
        public int ActivePips { get; set; }
        public int OpenAppeals { get; set; }
    }

    public interface IGetPerformanceDashboard { Task<PerformanceDashboardDto> GetAsync(GetAllRequest request); }

    public class GetPerformanceDashboard(
        IRepository<Appraisal> appraisalRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IRepository<EmployeeGoal> goalRepository,
        IRepository<PerformanceImprovementPlan> pipRepository,
        IRepository<AppraisalAppeal> appealRepository) : IGetPerformanceDashboard
    {
        public async Task<PerformanceDashboardDto> GetAsync(GetAllRequest request)
        {
            var appraisals = appraisalRepository.GetAll().AsNoTracking();
            var goals = goalRepository.GetAll().AsNoTracking();
            if (request.ReviewCycleId.HasValue)
            {
                appraisals = appraisals.Where(a => a.ReviewCycleId == request.ReviewCycleId.Value);
                goals = goals.Where(g => g.ReviewCycleId == request.ReviewCycleId.Value);
            }

            var dto = new PerformanceDashboardDto
            {
                TotalAppraisals = await appraisals.CountAsync(),
                SelfAssessmentCount = await appraisals.CountAsync(a => a.Stage == AppraisalStage.SelfAssessment),
                ManagerReviewCount = await appraisals.CountAsync(a => a.Stage == AppraisalStage.ManagerReview),
                CompletedCount = await appraisals.CountAsync(a => a.Stage == AppraisalStage.Completed),
                // Awaiting the employee's final acknowledgment signature (the collaborative flow signs off
                // before the terminal Completed/locked state).
                PendingAcknowledgment = await appraisals.CountAsync(a =>
                    a.Stage == AppraisalStage.EmployeeAcknowledgment),
            };

            // Overdue: non-completed appraisals whose cycle end date has passed.
            var today = DateTime.UtcNow.Date;
            dto.OverdueReviews = await appraisals.Where(a => a.Stage != AppraisalStage.Completed)
                .Join(reviewCycleRepository.GetAll(), a => a.ReviewCycleId, c => c.Id, (a, c) => c.EndDate)
                .CountAsync(end => end < today);

            // Rating distribution across completed appraisals.
            var dist = await appraisals.Where(a => a.Stage == AppraisalStage.Completed && a.FinalRatingLevelId != null)
                .GroupBy(a => a.FinalRatingLevelId!.Value)
                .Select(g => new { LevelId = g.Key, Count = g.Count() })
                .ToListAsync();
            if (dist.Count > 0)
            {
                var levelIds = dist.Select(d => d.LevelId).ToList();
                var levels = await ratingLevelRepository.GetAll().Where(l => levelIds.Contains(l.Id))
                    .ToDictionaryAsync(l => l.Id, l => new { l.Label, l.SortOrder });
                dto.RatingDistribution = dist
                    .Select(d => new RatingDistributionRowDto
                    {
                        LevelId = d.LevelId,
                        Label = levels.TryGetValue(d.LevelId, out var l) ? l.Label : "—",
                        Count = d.Count
                    })
                    .OrderBy(r => levels.TryGetValue(r.LevelId, out var l) ? l.SortOrder : 0)
                    .ToList();
            }

            // Goal progress.
            dto.TotalGoals = await goals.CountAsync();
            dto.CompletedGoals = await goals.CountAsync(g => g.Status == GoalStatus.Completed);
            dto.AverageGoalProgress = dto.TotalGoals > 0
                ? Math.Round((decimal)await goals.AverageAsync(g => (double)g.ProgressPercent), 1)
                : 0;

            // Risk indicators.
            dto.ActivePips = await pipRepository.GetAll().AsNoTracking().CountAsync(p => p.Status != PipStatus.Completed);
            dto.OpenAppeals = await appealRepository.GetAll().AsNoTracking()
                .CountAsync(a => a.Status == AppealStatus.Open || a.Status == AppealStatus.UnderReview);

            return dto;
        }
    }
}
