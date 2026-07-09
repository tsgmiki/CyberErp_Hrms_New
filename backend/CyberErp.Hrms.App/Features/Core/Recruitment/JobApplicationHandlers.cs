using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- Interfaces -----------------------------------------------------------

    public interface ICreateJobApplication { Task<Guid> CreateAsync(CreateJobApplicationDto dto); }
    public interface IGetJobApplicationById { Task<JobApplicationDto> GetAsync(Guid id); }
    public interface IGetAllJobApplications { Task<PaginatedResponse<JobApplicationDto>> GetAsync(GetAllRequest request); }
    public interface IMoveJobApplicationStage { Task MoveAsync(MoveApplicationStageDto dto); }
    public interface IScoreJobApplication { Task ScoreAsync(ScoreApplicationDto dto); }
    public interface IGetApplicationRanking { Task<List<ApplicationRankingRowDto>> GetAsync(Guid requisitionId); }

    internal static class ApplicationShared
    {
        internal static JobApplicationDto ToDto(
            JobApplication a,
            string? candidateNumber, string? candidateName,
            string? requisitionNumber, string? requisitionTitle,
            bool includeLog)
        {
            return new JobApplicationDto
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                CandidateNumber = candidateNumber,
                CandidateName = candidateName,
                RequisitionId = a.RequisitionId,
                RequisitionNumber = requisitionNumber,
                RequisitionTitle = requisitionTitle,
                Stage = a.Stage.ToString(),
                AppliedAt = a.AppliedAt,
                ScreeningScore = a.ScreeningScore,
                ScreeningRemarks = a.ScreeningRemarks,
                StageLog = includeLog
                    ? a.StageLog.OrderBy(l => l.ActedAt)
                        .Select(l => new ApplicationStageLogDto
                        {
                            Stage = l.Stage.ToString(),
                            Note = l.Note,
                            ActedBy = l.ActedBy,
                            ActedAt = l.ActedAt
                        })
                        .ToList()
                    : []
            };
        }
    }

    // ---- Create (receive an application, HC098) ------------------------------------

    public class CreateJobApplication(
        IRepository<JobApplication> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobRequisition> requisitionRepository,
        ICurrentUserService currentUser,
        IValidator<CreateJobApplicationDto> validator,
        ILogger<CreateJobApplication> logger) : ICreateJobApplication
    {
        public async Task<Guid> CreateAsync(CreateJobApplicationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var candidate = await candidateRepository.GetAll().FirstOrDefaultAsync(c => c.Id == dto.CandidateId)
                ?? throw new NotFoundException(nameof(Candidate), dto.CandidateId.ToString());
            if (candidate.IsArchived)
                throw new ValidationException("candidateId", "An archived/anonymized candidate cannot apply.");

            var requisition = await requisitionRepository.GetAll().FirstOrDefaultAsync(q => q.Id == dto.RequisitionId)
                ?? throw new NotFoundException(nameof(JobRequisition), dto.RequisitionId.ToString());
            if (requisition.Status is not (RequisitionStatus.Approved or RequisitionStatus.Posted))
                throw new ValidationException("requisitionId",
                    $"Applications are only received on approved or posted requisitions (current: {requisition.Status}).");

            if (await repository.GetAll().AnyAsync(a =>
                    a.CandidateId == dto.CandidateId && a.RequisitionId == dto.RequisitionId))
                throw new DuplicateException(nameof(JobApplication), "candidate", candidate.CandidateNumber);

            var created = JobApplication.Create(dto.CandidateId, dto.RequisitionId, dto.AppliedAt,
                currentUser.GetCurrentUserName());
            await repository.AddAsync(created);   // stamps the root; the initial log row rides the insert graph
            foreach (var log in created.StageLog)
                if (string.IsNullOrEmpty(log.TenantId))
                    log.TenantId = created.TenantId;
            await repository.SaveChangesAsync();
            logger.LogInformation("Application {Id}: candidate {Candidate} → requisition {Requisition}",
                created.Id, candidate.CandidateNumber, requisition.RequisitionNumber);
            return created.Id;
        }
    }

    // ---- Stage moves (pipeline machine + screening outcome, HC098/HC099) -----------------

    public class MoveJobApplicationStage(
        IRepository<JobApplication> repository,
        IRepository<JobApplicationStageLog> logRepository,
        ICurrentUserService currentUser,
        IValidator<MoveApplicationStageDto> validator,
        ILogger<MoveJobApplicationStage> logger) : IMoveJobApplicationStage
    {
        public async Task MoveAsync(MoveApplicationStageDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var stage = Enum.Parse<ApplicationStage>(dto.Stage, true);
            // Offer/Hired transitions belong to the offer & onboarding stage of the module.
            if (stage is ApplicationStage.OfferPending or ApplicationStage.Hired)
                throw new ValidationException("stage", "Offer and hire stages are driven by the offer process.");

            var application = await repository.GetAll()
                    .Include(a => a.StageLog)
                    .FirstOrDefaultAsync(a => a.Id == dto.Id)
                ?? throw new NotFoundException(nameof(JobApplication), dto.Id.ToString());

            if (application.Stage is ApplicationStage.Rejected or ApplicationStage.Withdrawn or ApplicationStage.Hired)
                throw new ValidationException("stage", $"A {application.Stage} application is final and can no longer move.");
            if (application.Stage == stage)
                throw new ValidationException("stage", $"The application is already at the {stage} stage.");

            if (dto.ScreeningScore.HasValue || !string.IsNullOrWhiteSpace(dto.ScreeningRemarks))
                application.RecordScreening(dto.ScreeningScore, dto.ScreeningRemarks);

            var before = application.StageLog.Select(l => l.Id).ToHashSet();
            application.MoveToStage(stage, dto.Note, currentUser.GetCurrentUserName());
            // New log rows need the tenant stamp + an explicit Add (the aggregate-child gotcha).
            foreach (var log in application.StageLog.Where(l => !before.Contains(l.Id)))
            {
                if (string.IsNullOrEmpty(log.TenantId))
                    log.TenantId = application.TenantId;
                await logRepository.AddAsync(log);
            }
            repository.UpdateAsync(application);
            await repository.SaveChangesAsync();
            logger.LogInformation("Application {Id} moved to {Stage}", dto.Id, stage);
        }
    }

    // ---- Evaluator scoring: per-criterion scores → auto total (requirement #1) --------------

    public class ScoreJobApplication(
        IRepository<JobApplication> repository,
        IRepository<ApplicationCriterionScore> scoreRepository,
        IRepository<JobRequisition> requisitionRepository,
        ICurrentUserService currentUser,
        IValidator<ScoreApplicationDto> validator,
        ILogger<ScoreJobApplication> logger) : IScoreJobApplication
    {
        public async Task ScoreAsync(ScoreApplicationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var application = await repository.GetAll()
                    .Include(a => a.CriterionScores)
                    .FirstOrDefaultAsync(a => a.Id == dto.Id)
                ?? throw new NotFoundException(nameof(JobApplication), dto.Id.ToString());
            if (application.Stage is ApplicationStage.Rejected or ApplicationStage.Withdrawn or ApplicationStage.Hired)
                throw new ValidationException("id", $"A {application.Stage} application can no longer be scored.");

            // Scores land on the requisition's own criteria (weights snapshotted at scoring time).
            var criteria = await requisitionRepository.GetAll()
                .Where(q => q.Id == application.RequisitionId)
                .SelectMany(q => q.ScreeningCriteria)
                .Select(c => new { c.Id, c.Weight })
                .ToDictionaryAsync(c => c.Id, c => c.Weight);

            var actedBy = currentUser.GetCurrentUserName();
            var before = application.CriterionScores.Select(s => s.Id).ToHashSet();
            foreach (var entry in dto.Scores)
            {
                if (!criteria.TryGetValue(entry.CriterionId, out var weight))
                    throw new ValidationException("scores", "A score references a criterion that is not on this requisition.");
                application.ScoreCriterion(entry.CriterionId, entry.Score, weight, entry.Remarks, actedBy);
            }
            application.RecomputeScreeningScore();

            // New score rows need the tenant stamp + an explicit Add (the aggregate-child gotcha).
            foreach (var score in application.CriterionScores.Where(s => !before.Contains(s.Id)))
            {
                if (string.IsNullOrEmpty(score.TenantId))
                    score.TenantId = application.TenantId;
                await scoreRepository.AddAsync(score);
            }
            repository.UpdateAsync(application);
            await repository.SaveChangesAsync();
            logger.LogInformation("Scored application {Id}: total {Total}", dto.Id, application.ScreeningScore);
        }
    }

    // ---- Ranking: auto-calculated weighted totals per vacancy (requirement #1) ----------------

    public class GetApplicationRanking(
        IRepository<JobApplication> repository,
        IRepository<JobRequisition> requisitionRepository,
        IRepository<Candidate> candidateRepository) : IGetApplicationRanking
    {
        public async Task<List<ApplicationRankingRowDto>> GetAsync(Guid requisitionId)
        {
            var criteria = await requisitionRepository.GetAll()
                .Where(q => q.Id == requisitionId)
                .SelectMany(q => q.ScreeningCriteria)
                .Select(c => new { c.Id, c.Name, c.IsMandatory, c.Weight, c.EvaluatorType, c.EvaluatorName })
                .ToListAsync();
            var criteriaById = criteria.ToDictionary(c => c.Id);

            var applications = await repository.GetAll()
                .Include(a => a.CriterionScores)
                .Where(a => a.RequisitionId == requisitionId)
                .ToListAsync();

            var candidateIds = applications.Select(a => a.CandidateId).Distinct().ToList();
            var candidates = await candidateRepository.GetAll()
                .Where(c => candidateIds.Contains(c.Id))
                .Select(c => new { c.Id, c.CandidateNumber, c.FirstName, c.FatherName, c.GrandFatherName })
                .ToDictionaryAsync(c => c.Id, c => c);

            return applications
                .Select(a =>
                {
                    var scoresByCriterion = a.CriterionScores.ToDictionary(s => s.CriterionId);
                    var breakdown = criteria.Select(c =>
                    {
                        scoresByCriterion.TryGetValue(c.Id, out var s);
                        return new CriterionScoreDto
                        {
                            CriterionId = c.Id,
                            CriterionName = c.Name,
                            IsMandatory = c.IsMandatory,
                            Weight = c.Weight,
                            EvaluatorType = c.EvaluatorType.ToString(),
                            EvaluatorName = c.EvaluatorName,
                            Score = s?.Score,
                            Remarks = s?.Remarks,
                            ScoredBy = s?.ScoredBy,
                            ScoredAt = s?.ScoredAt
                        };
                    }).ToList();
                    var candidate = candidates.GetValueOrDefault(a.CandidateId);
                    return new ApplicationRankingRowDto
                    {
                        ApplicationId = a.Id,
                        CandidateId = a.CandidateId,
                        CandidateNumber = candidate?.CandidateNumber,
                        CandidateName = candidate is null
                            ? null
                            : string.Join(" ", new[] { candidate.FirstName, candidate.FatherName, candidate.GrandFatherName }
                                .Where(p => !string.IsNullOrWhiteSpace(p))),
                        Stage = a.Stage.ToString(),
                        TotalScore = a.ScreeningScore,
                        ScoredCriteria = a.CriterionScores.Count(s => criteriaById.ContainsKey(s.CriterionId)),
                        TotalCriteria = criteria.Count,
                        FailsMandatory = breakdown.Any(b => b.IsMandatory && b.Score is < 50),
                        Breakdown = breakdown
                    };
                })
                .OrderByDescending(r => r.TotalScore ?? -1)
                .ToList();
        }
    }

    // ---- Get by id / Get all -------------------------------------------------------------

    public class GetJobApplicationById(
        IRepository<JobApplication> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobRequisition> requisitionRepository) : IGetJobApplicationById
    {
        public async Task<JobApplicationDto> GetAsync(Guid id)
        {
            var a = await repository.GetAll()
                    .Include(x => x.StageLog)
                    .Include(x => x.CriterionScores)
                    .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(JobApplication), id.ToString());

            var candidate = await candidateRepository.GetAll()
                .Where(c => c.Id == a.CandidateId)
                .Select(c => new { c.CandidateNumber, c.FirstName, c.FatherName, c.GrandFatherName })
                .FirstOrDefaultAsync();
            var requisition = await requisitionRepository.GetAll()
                .Where(q => q.Id == a.RequisitionId)
                .Select(q => new { q.RequisitionNumber, q.Title })
                .FirstOrDefaultAsync();

            var name = candidate is null
                ? null
                : string.Join(" ", new[] { candidate.FirstName, candidate.FatherName, candidate.GrandFatherName }
                    .Where(p => !string.IsNullOrWhiteSpace(p)));
            var dto = ApplicationShared.ToDto(a, candidate?.CandidateNumber, name,
                requisition?.RequisitionNumber, requisition?.Title, includeLog: true);

            // Score sheet: the requisition's criteria merged with this application's scores.
            var criteria = await requisitionRepository.GetAll()
                .Where(q => q.Id == a.RequisitionId)
                .SelectMany(q => q.ScreeningCriteria)
                .Select(c => new { c.Id, c.Name, c.IsMandatory, c.Weight, c.EvaluatorType, c.EvaluatorName })
                .ToListAsync();
            var scoresByCriterion = a.CriterionScores.ToDictionary(s => s.CriterionId);
            dto.CriterionScores = criteria.Select(c =>
            {
                scoresByCriterion.TryGetValue(c.Id, out var s);
                return new CriterionScoreDto
                {
                    CriterionId = c.Id,
                    CriterionName = c.Name,
                    IsMandatory = c.IsMandatory,
                    Weight = c.Weight,
                    EvaluatorType = c.EvaluatorType.ToString(),
                    EvaluatorName = c.EvaluatorName,
                    Score = s?.Score,
                    Remarks = s?.Remarks,
                    ScoredBy = s?.ScoredBy,
                    ScoredAt = s?.ScoredAt
                };
            }).ToList();
            return dto;
        }
    }

    public class GetAllJobApplications(
        IRepository<JobApplication> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobRequisition> requisitionRepository) : IGetAllJobApplications
    {
        public async Task<PaginatedResponse<JobApplicationDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<ApplicationStage>(request.Status, true, out var stage))
                query = query.Where(a => a.Stage == stage);
            // ParentId scopes applications to one requisition (the vacancy's pipeline view);
            // CategoryId scopes to one candidate (the talent-pool application history).
            if (request.ParentId.HasValue)
                query = query.Where(a => a.RequisitionId == request.ParentId.Value);
            if (request.CategoryId.HasValue)
                query = query.Where(a => a.CandidateId == request.CategoryId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(a =>
                    candidateRepository.GetAll().Any(c => c.Id == a.CandidateId &&
                        (c.CandidateNumber.Contains(term) || c.FirstName.Contains(term) ||
                         (c.Email != null && c.Email.Contains(term)))) ||
                    requisitionRepository.GetAll().Any(q => q.Id == a.RequisitionId &&
                        (q.RequisitionNumber.Contains(term) || q.Title.Contains(term))));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(a => a.AppliedAt)
                .Skip(skip).Take(take)
                .Select(a => new
                {
                    Application = a,
                    Candidate = candidateRepository.GetAll()
                        .Where(c => c.Id == a.CandidateId)
                        .Select(c => new { c.CandidateNumber, c.FirstName, c.FatherName, c.GrandFatherName })
                        .FirstOrDefault(),
                    Requisition = requisitionRepository.GetAll()
                        .Where(q => q.Id == a.RequisitionId)
                        .Select(q => new { q.RequisitionNumber, q.Title })
                        .FirstOrDefault()
                })
                .ToListAsync();

            var data = rows.Select(x =>
            {
                var name = x.Candidate is null
                    ? null
                    : string.Join(" ", new[] { x.Candidate.FirstName, x.Candidate.FatherName, x.Candidate.GrandFatherName }
                        .Where(p => !string.IsNullOrWhiteSpace(p)));
                return ApplicationShared.ToDto(x.Application, x.Candidate?.CandidateNumber, name,
                    x.Requisition?.RequisitionNumber, x.Requisition?.Title, includeLog: false);
            }).ToList();

            return new PaginatedResponse<JobApplicationDto> { Total = total, Data = data };
        }
    }
}
