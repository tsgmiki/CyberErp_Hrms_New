using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Rewards
{
    /// <summary>
    /// HC181 — automatic recognition from performance results. When an appraisal completes, any active
    /// badge whose <c>AutoGrantMinScore</c> the employee's normalised score meets is granted (with its
    /// points/disbursement side effects). The recognition's SourceRef ("Appraisal:{id}") makes the grant
    /// idempotent per appraisal, so re-firing the completion hook never double-awards.
    /// </summary>
    public class AppraisalAutoGrantHandler(
        IRepository<Appraisal> appraisalRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IRepository<RecognitionBadge> badgeRepository,
        IRepository<EmployeeRecognition> recognitionRepository,
        IRepository<RewardPointsTransaction> pointsRepository,
        IRepository<RewardDisbursement> disbursementRepository,
        ILogger<AppraisalAutoGrantHandler> logger) : IAppraisalCompletedHandler
    {
        public async Task OnAppraisalCompletedAsync(Guid appraisalId, Guid employeeId)
        {
            var appraisal = await appraisalRepository.GetAll().AsNoTracking()
                .Where(a => a.Id == appraisalId)
                .Select(a => new { a.OverallScore, a.ReviewCycleId, a.TenantId })
                .FirstOrDefaultAsync();
            if (appraisal?.OverallScore is not decimal score) return;

            var cycle = await reviewCycleRepository.GetAll().AsNoTracking()
                .Where(c => c.Id == appraisal.ReviewCycleId)
                .Select(c => new { c.Name, c.RatingScaleId })
                .FirstOrDefaultAsync();
            if (cycle is null) return;

            var max = await ratingLevelRepository.GetAll().AsNoTracking()
                .Where(l => l.RatingScaleId == cycle.RatingScaleId)
                .Select(l => (decimal?)l.Value).MaxAsync() ?? 0m;
            if (max <= 0) return;
            var percent = Math.Round(score / max * 100m, 1);

            var eligible = await badgeRepository.GetAll().AsNoTracking()
                .Where(b => b.IsActive && b.AutoGrantMinScore != null && b.AutoGrantMinScore <= percent)
                .ToListAsync();
            if (eligible.Count == 0) return;

            var sourceRef = $"Appraisal:{appraisalId}";
            var alreadyGranted = await recognitionRepository.GetAll()
                .Where(r => r.SourceRef == sourceRef)
                .Select(r => r.RecognitionBadgeId).ToListAsync();

            var granted = 0;
            foreach (var badge in eligible.Where(b => !alreadyGranted.Contains(b.Id)))
            {
                var recognition = EmployeeRecognition.Create(employeeId, badge.Id,
                    $"Auto-granted — scored {percent}% in {cycle.Name}.",
                    DateTime.UtcNow.Date, isPublic: true, sourceRef: sourceRef);
                if (string.IsNullOrEmpty(recognition.TenantId)) recognition.TenantId = appraisal.TenantId;
                await recognitionRepository.AddAsync(recognition);
                await RewardGrantShared.ApplyGrantSideEffectsAsync(recognition, badge, pointsRepository, disbursementRepository);
                granted++;
            }

            if (granted > 0)
            {
                await recognitionRepository.SaveChangesAsync();
                logger.LogInformation("Auto-granted {Count} badge(s) for appraisal {Appraisal} ({Percent}%)",
                    granted, appraisalId, percent);
            }
        }
    }
}
