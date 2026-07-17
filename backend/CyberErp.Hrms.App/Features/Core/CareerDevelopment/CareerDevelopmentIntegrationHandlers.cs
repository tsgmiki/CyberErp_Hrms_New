using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    /// <summary>Readiness computation breakdown (HC153) — the score + the two components it blends.</summary>
    public class ReadinessComputationDto
    {
        public Guid SuccessionCandidateId { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public decimal? ReadinessScore { get; set; }
        /// <summary>Latest appraisal, normalised to 0–100 (null = the employee has no appraisal yet).</summary>
        public decimal? PerformanceScore { get; set; }
        /// <summary>Competency coverage 0–100 (met ÷ required; null = the target role has no competencies).</summary>
        public decimal? CompetencyScore { get; set; }
        public int CompetencyMet { get; set; }
        public int CompetencyRequired { get; set; }
        public bool HasAppraisal { get; set; }
    }

    /// <summary>Holistic candidate view (HC158) — career-development readiness + the performance summary + gap.</summary>
    public class SuccessionCandidateProfileDto
    {
        public Guid SuccessionCandidateId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string Readiness { get; set; } = nameof(ReadinessLevel.NotReady);
        public decimal? ReadinessScore { get; set; }
        public EmployeePerformanceSummaryDto? Performance { get; set; }
        public CompetencyGapDto? Gap { get; set; }
    }

    public class IdentifyHiPosResultDto
    {
        public int Flagged { get; set; }
        public int TotalHiPo { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IComputeSuccessionCandidateReadiness { Task<ReadinessComputationDto> ComputeAsync(Guid candidateId); }
    public interface IGetSuccessionCandidateProfile { Task<SuccessionCandidateProfileDto> GetAsync(Guid candidateId); }
    public interface IIdentifyHiPos { Task<IdentifyHiPosResultDto> IdentifyAsync(Guid talentReviewId); }

    // ---- Handlers -----------------------------------------------------------
    /// <summary>
    /// Auto-computes a successor's readiness (HC153) by blending two signals — the employee's latest
    /// appraisal (normalised to 0–100 against its rating scale) and their competency coverage for the
    /// target role — and persists the denormalised score/level. Reuses the gap handler; a handful of
    /// indexed queries, invoked ON DEMAND per candidate (never in a list render).
    /// </summary>
    public class ComputeSuccessionCandidateReadiness(
        IRepository<SuccessionCandidate> repository,
        IGetSuccessionCandidateGap gapHandler,
        IRepository<Appraisal> appraisals,
        IRepository<ReviewCycle> reviewCycles,
        IRepository<RatingScaleLevel> ratingLevels,
        ILogger<ComputeSuccessionCandidateReadiness> logger) : IComputeSuccessionCandidateReadiness
    {
        public async Task<ReadinessComputationDto> ComputeAsync(Guid candidateId)
        {
            var candidate = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == candidateId)
                ?? throw new NotFoundException(nameof(SuccessionCandidate), candidateId.ToString());

            // Competency coverage 0–100 (reuse the gap analysis).
            var gap = await gapHandler.GetAsync(candidateId);
            decimal? competencyScore = gap.RequiredCount > 0
                ? Math.Round((decimal)gap.MetCount / gap.RequiredCount * 100m, 1) : null;

            // Performance 0–100 = latest appraisal's overall score ÷ the rating scale's max.
            var latest = await appraisals.GetAll().AsNoTracking()
                .Where(a => a.EmployeeId == candidate.EmployeeId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new { a.OverallScore, a.ReviewCycleId })
                .FirstOrDefaultAsync();
            decimal? performanceScore = null;
            if (latest?.OverallScore is decimal score)
            {
                var scaleId = await reviewCycles.GetAll().Where(c => c.Id == latest.ReviewCycleId)
                    .Select(c => c.RatingScaleId).FirstOrDefaultAsync();
                var scaleMax = scaleId != Guid.Empty
                    ? await ratingLevels.GetAll().Where(l => l.RatingScaleId == scaleId && l.MaxScore != null)
                        .MaxAsync(l => (decimal?)l.MaxScore) : null;
                if (scaleMax is decimal max && max > 0)
                    performanceScore = Math.Round(Math.Clamp(score / max * 100m, 0m, 100m), 1);
            }

            var components = new List<decimal>();
            if (performanceScore.HasValue) components.Add(performanceScore.Value);
            if (competencyScore.HasValue) components.Add(competencyScore.Value);
            decimal? readinessScore = components.Count > 0 ? Math.Round(components.Average(), 1) : candidate.ReadinessScore;
            var level = ToLevel(readinessScore);

            candidate.SetReadiness(level, readinessScore);
            repository.UpdateAsync(candidate);
            await repository.SaveChangesAsync();
            logger.LogInformation("Computed readiness for SuccessionCandidate {Id}: {Score} ({Level})", candidateId, readinessScore, level);

            return new ReadinessComputationDto
            {
                SuccessionCandidateId = candidateId,
                Readiness = level.ToString(),
                ReadinessScore = readinessScore,
                PerformanceScore = performanceScore,
                CompetencyScore = competencyScore,
                CompetencyMet = gap.MetCount,
                CompetencyRequired = gap.RequiredCount,
                HasAppraisal = latest is not null,
            };
        }

        private static ReadinessLevel ToLevel(decimal? score) => score switch
        {
            >= 80 => ReadinessLevel.ReadyNow,
            >= 55 => ReadinessLevel.Ready1To2Years,
            >= 30 => ReadinessLevel.Ready3PlusYears,
            _ => ReadinessLevel.NotReady,
        };
    }

    /// <summary>Holistic candidate view (HC158) — composes career-development readiness with the existing
    /// (already-optimised) performance summary and the competency gap.</summary>
    public class GetSuccessionCandidateProfile(
        IRepository<SuccessionCandidate> repository,
        IGetEmployeePerformanceSummary summaryHandler,
        IGetSuccessionCandidateGap gapHandler) : IGetSuccessionCandidateProfile
    {
        public async Task<SuccessionCandidateProfileDto> GetAsync(Guid candidateId)
        {
            var c = await repository.GetAll().Where(x => x.Id == candidateId)
                .Select(x => new
                {
                    x.Id, x.EmployeeId, x.Readiness, x.ReadinessScore,
                    EmployeeName = x.Employee != null && x.Employee.Person != null
                        ? x.Employee.Person.FirstName + " " + x.Employee.Person.GrandFatherName : null,
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(SuccessionCandidate), candidateId.ToString());

            return new SuccessionCandidateProfileDto
            {
                SuccessionCandidateId = candidateId,
                EmployeeId = c.EmployeeId,
                EmployeeName = c.EmployeeName,
                Readiness = c.Readiness.ToString(),
                ReadinessScore = c.ReadinessScore,
                Performance = await summaryHandler.GetAsync(c.EmployeeId),
                Gap = await gapHandler.GetAsync(candidateId),
            };
        }
    }

    /// <summary>Identify high-potentials (HC148): flag every top-box (High performance × High potential)
    /// assessment in a talent review. One small set-based load + flag.</summary>
    public class IdentifyHiPos(
        IRepository<TalentAssessment> repository,
        ILogger<IdentifyHiPos> logger) : IIdentifyHiPos
    {
        public async Task<IdentifyHiPosResultDto> IdentifyAsync(Guid talentReviewId)
        {
            var topBox = await repository.GetAll()
                .Where(a => a.TalentReviewId == talentReviewId && a.PerformanceBand == 3 && a.PotentialBand == 3 && !a.IsHiPo)
                .ToListAsync();
            foreach (var a in topBox) { a.SetHiPo(true); repository.UpdateAsync(a); }
            if (topBox.Count > 0) await repository.SaveChangesAsync();

            var total = await repository.GetAll().CountAsync(a => a.TalentReviewId == talentReviewId && a.IsHiPo);
            logger.LogInformation("Identified {N} HiPo(s) in TalentReview {Id}", topBox.Count, talentReviewId);
            return new IdentifyHiPosResultDto { Flagged = topBox.Count, TotalHiPo = total };
        }
    }
}
