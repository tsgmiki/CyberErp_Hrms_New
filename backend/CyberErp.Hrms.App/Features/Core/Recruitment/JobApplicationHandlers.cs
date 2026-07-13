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
    public interface IBulkMoveApplicationStage { Task<BulkMoveResultDto> MoveAsync(BulkMoveApplicationStageDto dto); }
    public interface IScoreJobApplication { Task ScoreAsync(ScoreApplicationDto dto); }
    public interface IGetApplicationRanking { Task<List<ApplicationRankingRowDto>> GetAsync(Guid requisitionId); }
    public interface IAdoptInterviewScores { Task<int> AdoptAsync(Guid applicationId); }
    public interface IGetEvaluatorContext { Task<EvaluatorContextDto> GetAsync(); }

    /// <summary>What the current login may evaluate — drives the UI's evaluator-scoped view.</summary>
    public class EvaluatorContextDto
    {
        /// <summary>True when the current user is an employee assigned as a criterion evaluator.</summary>
        public bool IsConstrainedEvaluator { get; set; }
        /// <summary>The criteria this evaluator may score (empty when unconstrained).</summary>
        public List<Guid> AssignedCriterionIds { get; set; } = [];
        /// <summary>The requisitions whose applicants this evaluator may see/score.</summary>
        public List<Guid> AssignedRequisitionIds { get; set; } = [];
    }

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
            if (stage is ApplicationStage.OfferPending or ApplicationStage.OfferAccepted or ApplicationStage.Hired)
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

    /// <summary>
    /// Shared rules that guard criterion scoring: WHEN scores may change (evaluation is locked once
    /// the applicant is Selected or beyond) and WHO may change them (an employee who is assigned as
    /// a criterion evaluator may only score the criteria assigned to them).
    /// </summary>
    internal static class EvaluationGuard
    {
        /// <summary>The pipeline stages during which an application is still being evaluated.</summary>
        internal static readonly ApplicationStage[] EvaluatableStages =
        [
            ApplicationStage.Received, ApplicationStage.Screening,
            ApplicationStage.Shortlisted, ApplicationStage.Interview
        ];

        /// <summary>Scores lock once the evaluation is concluded (Selected → decision made).</summary>
        internal static void EnsureEvaluatable(ApplicationStage stage)
        {
            if (!EvaluatableStages.Contains(stage))
                throw new ValidationException("id",
                    $"The evaluation is complete — a {stage} applicant's scores are locked and can no longer be changed.");
        }

        /// <summary>The employee behind the current login (null when the account has no employee link).</summary>
        internal static async Task<Guid?> CurrentEmployeeIdAsync(IRepository<User> users, Guid? userId)
        {
            if (!userId.HasValue) return null;
            return await users.GetAll()
                .Where(u => u.Id == userId.Value)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// What the current login is allowed to evaluate. A "constrained evaluator" is a logged-in
        /// employee who is assigned as a criterion evaluator ANYWHERE — they may only see/score the
        /// applicants of the requisitions they are assigned to, and only their own criteria. Anyone
        /// else (no employee link, or an employee never assigned as an evaluator) is unconstrained.
        /// </summary>
        internal sealed record EvaluatorContext(
            Guid? EmployeeId,
            bool IsConstrained,
            HashSet<Guid> AssignedCriterionIds,
            HashSet<Guid> AssignedRequisitionIds);

        internal static async Task<EvaluatorContext> GetContextAsync(
            IRepository<User> users,
            IRepository<CriterionEvaluator> evaluators,
            IRepository<JobRequisition> requisitions,
            Guid? userId)
        {
            var employeeId = await CurrentEmployeeIdAsync(users, userId);
            if (!employeeId.HasValue) return new(null, false, [], []);

            var criterionIds = (await evaluators.GetAll()
                .Where(ev => ev.EmployeeId == employeeId.Value)
                .Select(ev => ev.CriterionId)
                .ToListAsync()).ToHashSet();
            if (criterionIds.Count == 0) return new(employeeId, false, [], []);

            var requisitionIds = (await requisitions.GetAll()
                .SelectMany(q => q.ScreeningCriteria)
                .Where(c => criterionIds.Contains(c.Id))
                .Select(c => c.RequisitionId)
                .ToListAsync()).ToHashSet();

            return new(employeeId, true, criterionIds, requisitionIds);
        }

        /// <summary>
        /// Enforces evaluator ownership: if the current employee is an assigned criterion evaluator
        /// ANYWHERE, they may only score the criteria they are personally assigned to — scoring any
        /// other criterion is rejected. Callers who are not employees, or employees never assigned
        /// as an evaluator (HR / admins), are unconstrained.
        /// </summary>
        internal static async Task EnsureMayScoreAsync(
            IRepository<CriterionEvaluator> evaluators,
            Guid? currentEmployeeId,
            IReadOnlyCollection<Guid> criterionIdsBeingScored)
        {
            if (!currentEmployeeId.HasValue) return;

            var isEvaluator = await evaluators.GetAll().AnyAsync(ev => ev.EmployeeId == currentEmployeeId.Value);
            if (!isEvaluator) return;   // an employee, but not an evaluator → acts as HR (unconstrained)

            var mine = await evaluators.GetAll()
                .Where(ev => ev.EmployeeId == currentEmployeeId.Value && criterionIdsBeingScored.Contains(ev.CriterionId))
                .Select(ev => ev.CriterionId)
                .ToListAsync();
            var mineSet = mine.ToHashSet();

            if (criterionIdsBeingScored.Any(id => !mineSet.Contains(id)))
                throw new ValidationException("scores",
                    "You may only score the criteria you are assigned to as an evaluator for this applicant.");
        }
    }

    public class ScoreJobApplication(
        IRepository<JobApplication> repository,
        IRepository<ApplicationCriterionScore> scoreRepository,
        IRepository<JobRequisition> requisitionRepository,
        IRepository<User> userRepository,
        IRepository<CriterionEvaluator> evaluatorRepository,
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
            // Score locking: once the applicant is Selected or beyond, the evaluation is concluded.
            EvaluationGuard.EnsureEvaluatable(application.Stage);

            // Scores land on the requisition's own criteria (weights snapshotted at scoring time).
            var criteria = await requisitionRepository.GetAll()
                .Where(q => q.Id == application.RequisitionId)
                .SelectMany(q => q.ScreeningCriteria)
                .Select(c => new { c.Id, c.Weight })
                .ToDictionaryAsync(c => c.Id, c => c.Weight);

            // Evaluator ownership: an assigned employee-evaluator may only score their own criteria.
            var currentEmployeeId = await EvaluationGuard.CurrentEmployeeIdAsync(userRepository, currentUser.GetCurrentUserId());
            await EvaluationGuard.EnsureMayScoreAsync(
                evaluatorRepository, currentEmployeeId, dto.Scores.Select(s => s.CriterionId).ToList());

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

    // ---- Bulk stage move (mass processing: per-item outcomes, one transaction) ---------------

    /// <summary>
    /// Moves many applications in one action. Each application is checked against the SAME rules
    /// as a single move (final stages, offer-driven lock, already-there) — movable ones move in
    /// one transaction, the rest are reported back with the reason, never failing the batch.
    /// Bulk moves carry no screening scores (the score sheet owns those).
    /// </summary>
    public class BulkMoveApplicationStage(
        IRepository<JobApplication> repository,
        IRepository<JobApplicationStageLog> logRepository,
        IRepository<JobOffer> offerRepository,
        IRepository<Candidate> candidateRepository,
        ICurrentUserService currentUser,
        IValidator<BulkMoveApplicationStageDto> validator,
        ILogger<BulkMoveApplicationStage> logger) : IBulkMoveApplicationStage
    {
        public async Task<BulkMoveResultDto> MoveAsync(BulkMoveApplicationStageDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var stage = Enum.Parse<ApplicationStage>(dto.Stage, true);
            if (stage is ApplicationStage.OfferPending or ApplicationStage.OfferAccepted or ApplicationStage.Hired)
                throw new ValidationException("stage", "Offer and hire stages are driven by the offer process.");

            var ids = dto.Ids.Distinct().ToList();
            var applications = await repository.GetAll()
                .Include(a => a.StageLog)
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();
            var byId = applications.ToDictionary(a => a.Id);

            // Offer-driven lock, resolved in one batch: any live/accepted offer freezes its application.
            var lockedBy = (await offerRepository.GetAll()
                    .Where(o => ids.Contains(o.ApplicationId) &&
                        (o.Status == OfferStatus.Draft || o.Status == OfferStatus.PendingApproval ||
                         o.Status == OfferStatus.Approved || o.Status == OfferStatus.Sent ||
                         o.Status == OfferStatus.Accepted))
                    .Select(o => new { o.ApplicationId, o.OfferNumber, o.Status, o.CreatedAt })
                    .ToListAsync())
                .GroupBy(o => o.ApplicationId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAt).First());

            var actedBy = currentUser.GetCurrentUserName();
            var result = new BulkMoveResultDto();
            foreach (var id in ids)
            {
                if (!byId.TryGetValue(id, out var application))
                {
                    result.Skipped.Add(new BulkMoveSkippedDto { ApplicationId = id, Reason = "Application not found." });
                    continue;
                }
                string? reason =
                    application.Stage is ApplicationStage.Rejected or ApplicationStage.Withdrawn or ApplicationStage.Hired
                        ? $"A {application.Stage} application is final."
                    : application.Stage == stage
                        ? $"Already at the {stage} stage."
                    : lockedBy.TryGetValue(id, out var offer)
                        ? $"Offer {offer.OfferNumber} ({offer.Status}) drives this application."
                    : null;
                if (reason is not null)
                {
                    result.Skipped.Add(new BulkMoveSkippedDto { ApplicationId = id, Reason = reason });
                    continue;
                }

                var before = application.StageLog.Select(l => l.Id).ToHashSet();
                application.MoveToStage(stage, dto.Note, actedBy);
                foreach (var log in application.StageLog.Where(l => !before.Contains(l.Id)))
                {
                    if (string.IsNullOrEmpty(log.TenantId)) log.TenantId = application.TenantId;
                    await logRepository.AddAsync(log);
                }
                repository.UpdateAsync(application);
                result.Moved++;
            }

            if (result.Moved > 0)
                await repository.SaveChangesAsync();   // the movable subset commits as one unit

            // Names on the skip report so the user sees WHO was held back, not just ids.
            var skippedAppIds = result.Skipped.Select(s => s.ApplicationId).ToList();
            var names = await repository.GetAll()
                .Where(a => skippedAppIds.Contains(a.Id))
                .Select(a => new
                {
                    a.Id,
                    Name = candidateRepository.GetAll()
                        .Where(c => c.Id == a.CandidateId)
                        .Select(c => c.FirstName + " " + (c.FatherName ?? ""))
                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            foreach (var s in result.Skipped)
                s.CandidateName = names.GetValueOrDefault(s.ApplicationId)?.Trim();

            logger.LogInformation("Bulk stage move → {Stage}: {Moved} moved, {Skipped} skipped",
                stage, result.Moved, result.Skipped.Count);
            return result;
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
        IRepository<User> userRepository,
        IRepository<CriterionEvaluator> evaluatorRepository,
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
            // Same lock as direct scoring: adoption is blocked once evaluation is concluded.
            EvaluationGuard.EnsureEvaluatable(application.Stage);

            var consolidated = await consolidatedHandler.GetAsync(applicationId);
            var adoptable = consolidated.Criteria.Where(c => c.CriterionId.HasValue).ToList();
            if (adoptable.Count == 0)
                throw new ValidationException("applicationId",
                    "No criterion-linked interview feedback to adopt yet — panelists score named criteria first.");

            // Evaluator ownership: adoption writes criterion scores, so a constrained evaluator may
            // only adopt when every adopted criterion is one they are assigned to (else it's an HR
            // action). Closes the bypass around the direct-scoring gate.
            var currentEmployeeId = await EvaluationGuard.CurrentEmployeeIdAsync(userRepository, currentUser.GetCurrentUserId());
            await EvaluationGuard.EnsureMayScoreAsync(
                evaluatorRepository, currentEmployeeId, adoptable.Select(c => c.CriterionId!.Value).ToList());

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

    /// <summary>
    /// The current login's evaluator scope — the frontend uses it to show an assigned evaluator only
    /// their own criteria on the score sheet, and to reflect that they see only their applicants.
    /// </summary>
    public class GetEvaluatorContext(
        IRepository<User> userRepository,
        IRepository<CriterionEvaluator> evaluatorRepository,
        IRepository<JobRequisition> requisitionRepository,
        ICurrentUserService currentUser) : IGetEvaluatorContext
    {
        public async Task<EvaluatorContextDto> GetAsync()
        {
            var ctx = await EvaluationGuard.GetContextAsync(
                userRepository, evaluatorRepository, requisitionRepository, currentUser.GetCurrentUserId());
            return new EvaluatorContextDto
            {
                IsConstrainedEvaluator = ctx.IsConstrained,
                AssignedCriterionIds = [.. ctx.AssignedCriterionIds],
                AssignedRequisitionIds = [.. ctx.AssignedRequisitionIds]
            };
        }
    }

    // ---- Ranking + waitlist: weighted totals → 1st/2nd/3rd, top-N eligibility ------------------

    internal static class RankingShared
    {
        /// <summary>A declined or lapsed offer takes the candidate out of contention.</summary>
        internal static readonly OfferStatus[] RejectedOffers = [OfferStatus.Declined, OfferStatus.Expired];

        /// <summary>
        /// Assigns the ranking and hire-eligibility for a vacancy, WITHOUT any hidden tie-break:
        /// <list type="bullet">
        /// <item>Rank is standard-competition (ties SHARE a rank; the next distinct score skips) —
        /// so equal scores are shown as equal, never arbitrarily ordered.</item>
        /// <item>Eligibility is tie-safe: a candidate is Eligible when FEWER than the open-position
        /// count strictly outrank them on score. When a tie straddles the last open slot, EVERY
        /// tied candidate becomes co-eligible — the engine refuses to break a genuine tie on merit;
        /// HR makes the final call. The fill-auto-close + hire gate still cap actual hires at the
        /// open positions.</item>
        /// </list>
        /// A candidate whose latest offer was declined/expired drops out of contention, which lets
        /// the next scored tier (or tied group) move up automatically.
        /// </summary>
        internal static void AssignRanksAndEligibility(
            List<ApplicationRankingRowDto> rows, int numberOfPositions,
            IReadOnlyDictionary<Guid, OfferStatus> latestOfferByApplication)
        {
            foreach (var r in rows)
                if (latestOfferByApplication.TryGetValue(r.ApplicationId, out var status))
                    r.LatestOfferStatus = status.ToString();

            bool OfferRejected(ApplicationRankingRowDto r) =>
                latestOfferByApplication.TryGetValue(r.ApplicationId, out var s) && RejectedOffers.Contains(s);

            // Standard competition rank over scored rows: rank = 1 + (# rows scoring strictly
            // higher). Computed as ONE sort + a walk over the score groups — O(N log N), not a
            // per-row recount (the naive O(N²) version dominated request time on large vacancies).
            var scored = rows.Where(r => r.TotalScore.HasValue)
                .OrderByDescending(r => r.TotalScore).ToList();
            var processed = 0;
            foreach (var tier in scored.GroupBy(r => r.TotalScore))
            {
                var rank = processed + 1;              // ties share; the next tier skips
                foreach (var r in tier) r.Rank = rank;
                processed += tier.Count();
            }

            var openSlots = Math.Max(0, numberOfPositions - rows.Count(r => r.Stage == nameof(ApplicationStage.Hired)));

            var inPlay = rows.Where(r =>
                    r.TotalScore.HasValue && !r.FailsMandatory &&
                    r.Stage is not (nameof(ApplicationStage.Rejected) or nameof(ApplicationStage.Withdrawn) or nameof(ApplicationStage.Hired)) &&
                    !OfferRejected(r))
                .OrderByDescending(r => r.TotalScore)
                .ToList();
            // Same group-walk: everyone in a score tier shares `strictlyAhead`, so a tie at the
            // cut-off is co-eligible — semantics identical to the per-row count, at O(N log N).
            var strictlyAhead = 0;
            foreach (var tier in inPlay.GroupBy(r => r.TotalScore))
            {
                var tierRows = tier.ToList();
                foreach (var r in tierRows)
                {
                    r.HireEligibility = strictlyAhead < openSlots ? "Eligible" : "Waitlisted";
                    r.Tied = tierRows.Count > 1;
                }
                strictlyAhead += tierRows.Count;
            }

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
            var rows = await offers.GetAll().AsNoTracking()
                .Where(o => applicationIds.Contains(o.ApplicationId))
                .Select(o => new { o.ApplicationId, o.Status, o.CreatedAt })
                .ToListAsync();
            return rows
                .GroupBy(o => o.ApplicationId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAt).First().Status);
        }

        /// <summary>
        /// Page-scale eligibility: HireEligibility + Rank for EVERY application of the given
        /// requisitions from THREE set-based projection queries (no tracking, no per-criterion
        /// breakdown, no candidate hydration) + the shared assignment logic. The applications list
        /// runs this on every page load, so its cost must not scale with vacancy size the way the
        /// full ranking read (per-criterion breakdown per applicant) does.
        /// </summary>
        internal static async Task<Dictionary<Guid, (string? Eligibility, int? Rank)>> ComputeEligibilityAsync(
            IRepository<JobApplication> applications,
            IRepository<JobRequisition> requisitions,
            IRepository<ApplicationCriterionScore> scores,
            IRepository<JobOffer> offers,
            IReadOnlyCollection<Guid> requisitionIds)
        {
            if (requisitionIds.Count == 0) return [];

            var positions = await requisitions.GetAll().AsNoTracking()
                .Where(q => requisitionIds.Contains(q.Id))
                .Select(q => new { q.Id, q.NumberOfPositions })
                .ToDictionaryAsync(q => q.Id, q => q.NumberOfPositions);

            // The stored weighted total (ScreeningScore) is the ranking input — no need to reload
            // and re-aggregate the criterion-score rows here.
            var lite = await applications.GetAll().AsNoTracking()
                .Where(a => requisitionIds.Contains(a.RequisitionId))
                .Select(a => new { a.Id, a.RequisitionId, a.Stage, a.ScreeningScore })
                .ToListAsync();

            var mandatoryCriterionIds = await requisitions.GetAll().AsNoTracking()
                .Where(q => requisitionIds.Contains(q.Id))
                .SelectMany(q => q.ScreeningCriteria)
                .Where(c => c.IsMandatory)
                .Select(c => c.Id)
                .ToListAsync();
            var failsMandatory = mandatoryCriterionIds.Count == 0
                ? []
                : (await scores.GetAll().AsNoTracking()
                    .Where(s => mandatoryCriterionIds.Contains(s.CriterionId) && s.Score < 50)
                    .Select(s => s.ApplicationId)
                    .ToListAsync()).ToHashSet();

            var latestOffers = await LatestOffersAsync(offers, lite.Select(a => a.Id).ToList());

            var result = new Dictionary<Guid, (string?, int?)>();
            foreach (var vacancy in lite.GroupBy(a => a.RequisitionId))
            {
                var rows = vacancy.Select(a => new ApplicationRankingRowDto
                {
                    ApplicationId = a.Id,
                    Stage = a.Stage.ToString(),
                    TotalScore = a.ScreeningScore,
                    FailsMandatory = failsMandatory.Contains(a.Id)
                }).ToList();
                AssignRanksAndEligibility(rows, positions.GetValueOrDefault(vacancy.Key, 1), latestOffers);
                foreach (var r in rows) result[r.ApplicationId] = (r.HireEligibility, r.Rank);
            }
            return result;
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

            // Read-only hydration: no change tracking — on a large vacancy this is thousands of
            // application + score entities that would otherwise all enter the change tracker.
            var applications = await repository.GetAll()
                .AsNoTracking()
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
                        AppliedAt = a.AppliedAt,
                        TotalScore = a.ScreeningScore,
                        ScoredCriteria = a.CriterionScores.Count(s => criteriaById.ContainsKey(s.CriterionId)),
                        TotalCriteria = criteria.Count,
                        FailsMandatory = breakdown.Any(b => b.IsMandatory && b.Score is < 50),
                        Breakdown = breakdown
                    };
                })
                // Deterministic, reproducible order — NO hidden DB/insertion tie-break: highest
                // weighted total first, then (for equal scores) earliest application, then the
                // candidate number as a final stable business-key fallback. This decides only the
                // DISPLAY order; eligibility among ties is co-eligible (see AssignRanksAndEligibility).
                .OrderByDescending(r => r.TotalScore ?? -1)
                .ThenBy(r => r.AppliedAt)
                .ThenBy(r => r.CandidateNumber)
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
        IRepository<JobRequisition> requisitionRepository,
        IRepository<ApplicationCriterionScore> scoreRepository,
        IRepository<JobOffer> offerRepository,
        IRepository<User> userRepository,
        IRepository<CriterionEvaluator> evaluatorRepository,
        ICurrentUserService currentUser) : IGetAllJobApplications
    {
        public async Task<PaginatedResponse<JobApplicationDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            // Evaluator visibility: an assigned criterion evaluator only sees THEIR OWN applicants —
            // the applications of the requisitions they are assigned to. HR / unlinked users see all.
            var evaluatorContext = await EvaluationGuard.GetContextAsync(
                userRepository, evaluatorRepository, requisitionRepository, currentUser.GetCurrentUserId());
            if (evaluatorContext.IsConstrained)
            {
                var reqIds = evaluatorContext.AssignedRequisitionIds;
                query = query.Where(a => reqIds.Contains(a.RequisitionId));
            }

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

            // Per-row hire/offer eligibility from the vacancy ranking (criteria-scored vacancies
            // only): the Offer button activates ONLY for Eligible applicants — the UI mirrors the
            // server-side rank gate instead of discovering it as a 400 after filling the form.
            // Computed via the LIGHTWEIGHT set-based path — the previous full-ranking call per
            // requisition hydrated every applicant's criterion breakdown on every page load and
            // dominated list latency on large vacancies.
            var eligibilityByApplication = await RankingShared.ComputeEligibilityAsync(
                repository, requisitionRepository, scoreRepository, offerRepository,
                requisitionIds.Where(id => (criteriaStages.GetValueOrDefault(id)?.Count ?? 0) > 0).ToList());

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
                if (eligibilityByApplication.TryGetValue(x.Application.Id, out var e))
                {
                    dto.HireEligibility = e.Eligibility;
                    dto.Rank = e.Rank;
                }
                return dto;
            }).ToList();

            return new PaginatedResponse<JobApplicationDto> { Total = total, Data = data };
        }
    }
}
