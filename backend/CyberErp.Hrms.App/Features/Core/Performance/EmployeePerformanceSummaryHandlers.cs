using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- DTOs ---------------------------------------------------------------
    public class LatestAppraisalSummaryDto
    {
        public Guid Id { get; set; }
        public string? ReviewCycleName { get; set; }
        public string Stage { get; set; } = string.Empty;
        public decimal? OverallScore { get; set; }
        public string? FinalRatingLabel { get; set; }
        public string AcknowledgmentStatus { get; set; } = string.Empty;
    }

    public class ActivePipSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Unified per-employee performance view (HC147) — a single payload other modules (profile, rewards,
    /// promotions, training) can consume for consistent lifecycle data: the latest appraisal outcome, goal
    /// progress, any active improvement plan, and achievement / recognition counts.
    /// </summary>
    public class EmployeePerformanceSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public LatestAppraisalSummaryDto? LatestAppraisal { get; set; }
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public decimal AverageGoalProgress { get; set; }
        public ActivePipSummaryDto? ActivePip { get; set; }
        public int OpenAppeals { get; set; }
        public int AchievementsCount { get; set; }
        public int RecognitionsCount { get; set; }
    }

    public interface IGetEmployeePerformanceSummary { Task<EmployeePerformanceSummaryDto> GetAsync(Guid employeeId); }

    public class GetEmployeePerformanceSummary(
        IRepository<Employee> employeeRepository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IRepository<EmployeeGoal> goalRepository,
        IRepository<PerformanceImprovementPlan> pipRepository,
        IRepository<AppraisalAppeal> appealRepository,
        IRepository<Achievement> achievementRepository,
        IRepository<EmployeeRecognition> recognitionRepository,
        IPerformanceVisibilityService visibility) : IGetEmployeePerformanceSummary
    {
        public async Task<EmployeePerformanceSummaryDto> GetAsync(Guid employeeId)
        {
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException("access", "You do not have access to this employee's performance summary.");

            var employee = await employeeRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(e => e.Id == employeeId)
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == employeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();

            var dto = new EmployeePerformanceSummaryDto { EmployeeId = employeeId, EmployeeName = employeeName };

            var latest = await appraisalRepository.GetAll().AsNoTracking()
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.CreatedAt).FirstOrDefaultAsync();
            if (latest is not null)
            {
                var cycleName = await reviewCycleRepository.GetAll().Where(c => c.Id == latest.ReviewCycleId).Select(c => c.Name).FirstOrDefaultAsync();
                var label = latest.FinalRatingLevelId.HasValue
                    ? await ratingLevelRepository.GetAll().Where(l => l.Id == latest.FinalRatingLevelId.Value).Select(l => l.Label).FirstOrDefaultAsync()
                    : null;
                dto.LatestAppraisal = new LatestAppraisalSummaryDto
                {
                    Id = latest.Id,
                    ReviewCycleName = cycleName,
                    Stage = latest.Stage.ToString(),
                    OverallScore = latest.OverallScore,
                    FinalRatingLabel = label,
                    AcknowledgmentStatus = latest.AcknowledgmentStatus.ToString()
                };
            }

            var goals = goalRepository.GetAll().AsNoTracking().Where(g => g.EmployeeId == employeeId);
            dto.TotalGoals = await goals.CountAsync();
            dto.ActiveGoals = await goals.CountAsync(g => g.Status == GoalStatus.Active);
            dto.CompletedGoals = await goals.CountAsync(g => g.Status == GoalStatus.Completed);
            dto.AverageGoalProgress = dto.TotalGoals > 0
                ? Math.Round((decimal)await goals.AverageAsync(g => (double)g.ProgressPercent), 1)
                : 0;

            var pip = await pipRepository.GetAll().AsNoTracking()
                .Where(p => p.EmployeeId == employeeId && p.Status != PipStatus.Completed)
                .OrderByDescending(p => p.StartDate).FirstOrDefaultAsync();
            if (pip is not null)
                dto.ActivePip = new ActivePipSummaryDto { Id = pip.Id, Title = pip.Title, Status = pip.Status.ToString(), EndDate = pip.EndDate };

            dto.OpenAppeals = await appealRepository.GetAll().AsNoTracking()
                .CountAsync(a => a.EmployeeId == employeeId && (a.Status == AppealStatus.Open || a.Status == AppealStatus.UnderReview));
            dto.AchievementsCount = await achievementRepository.GetAll().AsNoTracking().CountAsync(a => a.EmployeeId == employeeId);
            dto.RecognitionsCount = await recognitionRepository.GetAll().AsNoTracking().CountAsync(r => r.EmployeeId == employeeId);

            return dto;
        }
    }
}
