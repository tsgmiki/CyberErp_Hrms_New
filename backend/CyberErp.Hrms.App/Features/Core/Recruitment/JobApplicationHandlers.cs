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
    public interface IAdoptInterviewScores { Task<int> AdoptAsync(Guid applicationId); }

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
        IRepository<JobRequisition> requisitionRepository,
        IRepository<JobOffer> offerRepository,
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

            // SEQUENCE RULE: once an offer is in play (live or accepted), the OFFER drives the
            // pipeline — a manual move would strand it (e.g. a Sent offer on a Rejected
            // application). Respond to / withdraw the offer first; declined, expired and
            // withdrawn offers release the application automatically.
            var drivingOffer = await offerRepository.GetAll()
                .Where(o => o.ApplicationId == dto.Id &&
                    (o.Status == OfferStatus.Draft || o.Status == OfferStatus.PendingApproval ||
                     o.Status == OfferStatus.Approved || o.Status == OfferStatus.Sent ||
                     o.Status == OfferStatus.Accepted))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new { o.OfferNumber, o.Status })
                .FirstOrDefaultAsync();
            if (drivingOffer is not null)
                throw new ValidationException("stage",
                    $"Offer {drivingOffer.OfferNumber} ({drivingOffer.Status}) drives this application — " +
                    "respond to it or withdraw it before moving the stage manually.");

            if (dto.ScreeningScore.HasValue || !string.IsNullOrWhiteSpace(dto.ScreeningRemarks))
            {
                // One source of truth: on a vacancy with weighted criteria, the criterion engine
                // OWNS the screening total — a manually typed score would be silently overwritten
                // by the next recompute (and could corrupt the hire-eligibility ranking).
                if (dto.ScreeningScore.HasValue && await requisitionRepository.GetAll()
                        .AnyAsync(q => q.Id == application.RequisitionId && q.ScreeningCriteria.Any()))
                    throw new ValidationException("screeningScore",
                        "This vacancy is scored against weighted criteria — record scores on the score sheet; the total is calculated automatically.");
                application.RecordScreening(dto.ScreeningScore, dto.ScreeningRemarks);
            }

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

    // ---- Adopt interview results into the ranking (no double entry) -----------------------

    /// <summary>
    /// Copies the consolidated per-criterion interview averages into the application's criterion
    /// scores in one click — panelist feedback flows into the weighted ranking without anyone
    /// retyping numbers. Criterion-linked feedback only; overall impressions stay commentary.
    /// </summary>
    public class AdoptInterviewScores(
        IRepository<JobApplication> repository,
        IRepository<ApplicationCriterionScore> scoreRepository,
        IGetInterviewConsolidated consolidatedHandler,
        ICurrentUserService currentUser,
        ILogger<AdoptInterviewScores> logger) : IAdoptInterviewScores
    {
        public async Task<int> AdoptAsync(Guid applicationId)
        {
            var application = await repository.GetAll()
                    .Include(a => a.CriterionScores)
                    .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new NotFoundException(nameof(JobApplication), applicationId.ToString());
            if (application.Stage is ApplicationStage.Rejected or ApplicationStage.Withdrawn or ApplicationStage.Hired)
                throw new ValidationException("applicationId", $"A {application.Stage} application can no longer be scored.");

            var consolidated = await consolidatedHandler.GetAsync(applicationId);
            var adoptable = consolidated.Criteria.Where(c => c.CriterionId.HasValue).ToList();
            if (adoptable.Count == 0)
                throw new ValidationException("applicationId",
                    "No criterion-linked interview feedback to adopt yet — panelists score named criteria first.");

            var before = application.CriterionScores.Select(s => s.Id).ToHashSet();
            foreach (var c in adoptable)
                application.ScoreCriterion(c.CriterionId!.Value, c.Average, Math.Max(1, c.Weight),
                    $"Adopted from the interview consolidated report ({c.Scores} score(s))",
                    currentUser.GetCurrentUserName());
            application.RecomputeScreeningScore();

            foreach (var score in application.CriterionScores.Where(s => !before.Contains(s.Id)))
            {
                if (string.IsNullOrEmpty(score.TenantId))
                    score.TenantId = application.TenantId;
                await scoreRepository.AddAsync(score);
            }
            repository.UpdateAsync(application);
            await repository.SaveChangesAsync();
            logger.LogInformation("Adopted {Count} interview criterion average(s) into Application {Id} (total {Total})",
                adoptable.Count, applicationId, application.ScreeningScore);
            return adoptable.Count;
        }
    }

    // ---- Ranking + waitlist: weighted totals → 1st/2nd/3rd, top-N eligibility ------------------

    internal static class RankingShared
    {
        /// <summary>A declined or lapsed offer takes the candidate out of contention.</summary>
        internal static readonly OfferStatus[] RejectedOffers = [OfferStatus.Declined, OfferStatus.Expired];

        /// <summary>
        /// Assigns 1-based ranks (scored rows, weighted-total order) and the hire-eligibility
        /// window: only the top N in-play candidates are Eligible (N = positions minus hires);
        /// the rest are Waitlisted. A candidate whose latest offer was declined/expired drops out
        /// of contention, sliding the next waitlisted candidate up automatically.
        /// </summary>
        internal static void AssignRanksAndEligibility(
            List<ApplicationRankingRowDto> rows, int numberOfPositions,
            IReadOnlyDictionary<Guid, OfferStatus> latestOfferByApplication)
        {
            var rank = 0;
            foreach (var r in rows.Where(r => r.TotalScore.HasValue))
                r.Rank = ++rank;

            foreach (var r in rows)
                if (latestOfferByApplication.TryGetValue(r.ApplicationId, out var status))
                    r.LatestOfferStatus = status.ToString();

            var openSlots = Math.Max(0, numberOfPositions - rows.Count(r => r.Stage == nameof(ApplicationStage.Hired)));

            bool OfferRejected(ApplicationRankingRowDto r) =>
                latestOfferByApplication.TryGetValue(r.ApplicationId, out var s) && RejectedOffers.Contains(s);

            var inPlay = rows.Where(r =>
                    r.TotalScore.HasValue && !r.FailsMandatory &&
                    r.Stage is not (nameof(ApplicationStage.Rejected) or nameof(ApplicationStage.Withdrawn) or nameof(ApplicationStage.Hired)) &&
                    !OfferRejected(r))
                .ToList();   // preserves weighted-total order
            for (var i = 0; i < inPlay.Count; i++)
                inPlay[i].HireEligibility = i < openSlots ? "Eligible" : "Waitlisted";

            foreach (var r in rows.Where(r => r.HireEligibility is null))
                r.HireEligibility =
                    r.Stage == nameof(ApplicationStage.Hired) ? "Hired" :
                    r.Stage is nameof(ApplicationStage.Rejected) or nameof(ApplicationStage.Withdrawn) ? "OutOfContention" :
                    OfferRejected(r) ? "OfferRejected" :
                    r.FailsMandatory ? "FailsMandatory" :
                    "NotScored";
        }

        /// <summary>The newest offer per application (drives contention + display).</summary>
        internal static async Task<Dictionary<Guid, OfferStatus>> LatestOffersAsync(
            IRepository<JobOffer> offers, IReadOnlyCollection<Guid> applicationIds)
        {
            var rows = await offers.GetAll()
                .Where(o => applicationIds.Contains(o.ApplicationId))
                .Select(o => new { o.ApplicationId, o.Status, o.CreatedAt })
                .ToListAsync();
            return rows
                .GroupBy(o => o.ApplicationId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAt).First().Status);
        }
    }

    public class GetApplicationRanking(
        IRepository<JobApplication> repository,
        IRepository<JobRequisition> requisitionRepository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobOffer> offerRepository) : IGetApplicationRanking
    {
        public async Task<List<ApplicationRankingRowDto>> GetAsync(Guid requisitionId)
        {
            var requisition = await requisitionRepository.GetAll()
                    .Where(q => q.Id == requisitionId)
                    .Select(q => new { q.NumberOfPositions })
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(JobRequisition), requisitionId.ToString());
            var criteria = await requisitionRepository.GetAll()
                .Where(q => q.Id == requisitionId)
                .SelectMany(q => q.ScreeningCriteria)
                .Select(c => new
                {
                    c.Id, c.Name, c.IsMandatory, c.Weight, c.AppliesAtStage,
                    EvaluatorNames = c.Evaluators.OrderBy(e => e.Name).Select(e => e.Name).ToList()
                })
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

            var rows = applications
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
                            EvaluatorName = c.EvaluatorNames.Count > 0 ? string.Join(", ", c.EvaluatorNames) : null,
                            AppliesAtStage = c.AppliesAtStage.HasValue ? c.AppliesAtStage.Value.ToString() : null,
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

            var latestOffers = await RankingShared.LatestOffersAsync(
                offerRepository, applications.Select(a => a.Id).ToList());
            RankingShared.AssignRanksAndEligibility(rows, requisition.NumberOfPositions, latestOffers);
            return rows;
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
                .Select(c => new
                {
                    c.Id, c.Name, c.IsMandatory, c.Weight, c.AppliesAtStage,
                    EvaluatorNames = c.Evaluators.OrderBy(e => e.Name).Select(e => e.Name).ToList()
                })
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
                    EvaluatorName = c.EvaluatorNames.Count > 0 ? string.Join(", ", c.EvaluatorNames) : null,
                    AppliesAtStage = c.AppliesAtStage.HasValue ? c.AppliesAtStage.Value.ToString() : null,
                    Score = s?.Score,
                    Remarks = s?.Remarks,
                    ScoredBy = s?.ScoredBy,
                    ScoredAt = s?.ScoredAt
                };
            }).ToList();
            dto.TotalCriteriaCount = criteria.Count;
            dto.ScoreableCriteriaCount = criteria.Count(c => c.AppliesAtStage == null || c.AppliesAtStage == a.Stage);
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

            // Criterion level-scopes per requisition: the UI shows the score button only when the
            // application's CURRENT stage has scoreable criteria (global ones always count).
            var requisitionIds = rows.Select(x => x.Application.RequisitionId).Distinct().ToList();
            var criteriaStages = (await requisitionRepository.GetAll()
                    .Where(q => requisitionIds.Contains(q.Id))
                    .SelectMany(q => q.ScreeningCriteria)
                    .Select(c => new { c.RequisitionId, c.AppliesAtStage })
                    .ToListAsync())
                .GroupBy(c => c.RequisitionId)
                .ToDictionary(g => g.Key, g => g.Select(c => c.AppliesAtStage).ToList());

            var data = rows.Select(x =>
            {
                var name = x.Candidate is null
                    ? null
                    : string.Join(" ", new[] { x.Candidate.FirstName, x.Candidate.FatherName, x.Candidate.GrandFatherName }
                        .Where(p => !string.IsNullOrWhiteSpace(p)));
                var dto = ApplicationShared.ToDto(x.Application, x.Candidate?.CandidateNumber, name,
                    x.Requisition?.RequisitionNumber, x.Requisition?.Title, includeLog: false);
                var stages = criteriaStages.GetValueOrDefault(x.Application.RequisitionId) ?? [];
                dto.TotalCriteriaCount = stages.Count;
                dto.ScoreableCriteriaCount = stages.Count(st => st is null || st == x.Application.Stage);
                return dto;
            }).ToList();

            return new PaginatedResponse<JobApplicationDto> { Total = total, Data = data };
        }
    }
}
