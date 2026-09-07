using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- DTOs ---------------------------------------------------------------------------

    /// <summary>One criterion this evaluator personally owns on an applicant, with their score so far.</summary>
    public class MyEvaluationCriterionDto
    {
        public Guid CriterionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Weight { get; set; }
        /// <summary>Null until they have scored it — the portal shows an empty box rather than a zero.</summary>
        public decimal? Score { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>An applicant awaiting this evaluator, carrying only the criteria they own.</summary>
    public class MyEvaluationDto
    {
        public Guid ApplicationId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? CandidateNumber { get; set; }
        public Guid RequisitionId { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public string RequisitionTitle { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        /// <summary>False once the applicant is Selected or beyond — scores are locked.</summary>
        public bool IsEditable { get; set; }
        /// <summary>True when every criterion below already carries a score.</summary>
        public bool IsComplete { get; set; }
        public List<MyEvaluationCriterionDto> Criteria { get; set; } = [];
    }

    public class SubmitMyEvaluationDto
    {
        public Guid ApplicationId { get; set; }
        public List<ScoreEntryDto> Scores { get; set; } = [];
    }

    // ---- Interfaces ---------------------------------------------------------------------

    public interface IGetMyEvaluations { Task<List<MyEvaluationDto>> GetAsync(); }
    public interface ISubmitMyEvaluation { Task<int> SubmitAsync(SubmitMyEvaluationDto dto); }

    /// <summary>
    /// Shared resolution for the evaluator self-service portal.
    ///
    /// <para>⚠️ STRICTER THAN <c>EvaluationGuard</c> ON PURPOSE, and the difference is the whole
    /// safety property. That guard treats an employee who is not an evaluator as "acting as HR" and
    /// lets them through unconstrained — correct behind the recruitment screens, which are already
    /// gated on the recruitment operations. These endpoints are NOT so gated (an assigned evaluator
    /// is an ordinary employee who holds none of those operations — logic §12.78), so the same
    /// fallback here would hand every applicant in the tenant to every member of staff. Here,
    /// evaluator standing is REQUIRED, never assumed.</para>
    /// </summary>
    internal static class EvaluatorPortal
    {
        /// <summary>The caller's own criterion ids, or an empty set when they are not an evaluator.</summary>
        internal static async Task<(Guid? EmployeeId, HashSet<Guid> CriterionIds)> MineAsync(
            IRepository<User> users, IRepository<CriterionEvaluator> evaluators, Guid? userId)
        {
            if (!userId.HasValue) return (null, []);

            var employeeId = await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
            if (!employeeId.HasValue) return (null, []);

            var ids = await evaluators.GetAll().AsNoTracking()
                .Where(ev => ev.EmployeeId == employeeId.Value)
                .Select(ev => ev.CriterionId)
                .ToListAsync();
            return (employeeId, ids.ToHashSet());
        }
    }

    // ---- My evaluations (portal list) ----------------------------------------------------

    /// <summary>
    /// The applicants awaiting THIS evaluator, each carrying only the criteria they personally own.
    ///
    /// <para>Built for the Home portal so an examiner who is not HR can do their part without a
    /// recruitment licence. It never widens: a caller who is not an assigned evaluator gets an empty
    /// list, not everyone's applicants.</para>
    /// </summary>
    public class GetMyEvaluations(
        IRepository<JobApplication> applications,
        IRepository<JobRequisition> requisitions,
        IRepository<Candidate> candidates,
        IRepository<User> users,
        IRepository<CriterionEvaluator> evaluators,
        ICurrentUserService currentUser) : IGetMyEvaluations
    {
        public async Task<List<MyEvaluationDto>> GetAsync()
        {
            var (employeeId, mine) = await EvaluatorPortal.MineAsync(users, evaluators, currentUser.GetCurrentUserId());
            if (employeeId is null || mine.Count == 0) return [];

            // The criteria I own, and the requisitions they belong to — one pass over the definitions.
            var owned = await requisitions.GetAll().AsNoTracking()
                .SelectMany(q => q.ScreeningCriteria)
                .Where(c => mine.Contains(c.Id))
                .Select(c => new { c.Id, c.Name, c.Weight, c.RequisitionId })
                .ToListAsync();
            if (owned.Count == 0) return [];

            var requisitionIds = owned.Select(o => o.RequisitionId).Distinct().ToList();
            var heads = await requisitions.GetAll().AsNoTracking()
                .Where(q => requisitionIds.Contains(q.Id))
                .Select(q => new { q.Id, q.RequisitionNumber, q.Title })
                .ToListAsync();

            var rows = await applications.GetAll().AsNoTracking()
                .Where(a => requisitionIds.Contains(a.RequisitionId))
                .Select(a => new
                {
                    a.Id,
                    a.RequisitionId,
                    a.Stage,
                    a.CandidateId,
                    Scores = a.CriterionScores
                        .Where(s => mine.Contains(s.CriterionId))
                        .Select(s => new { s.CriterionId, s.Score, s.Remarks })
                        .ToList()
                })
                .ToListAsync();

            // JobApplication carries only CandidateId — there is no Candidate navigation on it — so
            // the names come from one batched lookup rather than a per-row query.
            var candidateIds = rows.Select(r => r.CandidateId).Distinct().ToList();
            var candidateById = await candidates.GetAll().AsNoTracking()
                .Where(c => candidateIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    c.CandidateNumber,
                    Name = (c.FirstName + " " + c.FatherName).Trim()
                })
                .ToDictionaryAsync(x => x.Id, x => x);

            var result = new List<MyEvaluationDto>();
            foreach (var a in rows)
            {
                var head = heads.FirstOrDefault(h => h.Id == a.RequisitionId);
                var criteria = owned.Where(o => o.RequisitionId == a.RequisitionId).ToList();
                if (criteria.Count == 0) continue;

                var who = candidateById.GetValueOrDefault(a.CandidateId);
                var dto = new MyEvaluationDto
                {
                    ApplicationId = a.Id,
                    CandidateName = string.IsNullOrWhiteSpace(who?.Name) ? "(unnamed candidate)" : who!.Name!,
                    CandidateNumber = who?.CandidateNumber,
                    RequisitionId = a.RequisitionId,
                    RequisitionNumber = head?.RequisitionNumber ?? string.Empty,
                    RequisitionTitle = head?.Title ?? string.Empty,
                    Stage = a.Stage.ToString(),
                    // Same lock the scoring handlers apply, surfaced so the portal can show it
                    // read-only instead of letting someone type into a box that will be refused.
                    IsEditable = EvaluationGuard.EvaluatableStages.Contains(a.Stage),
                    Criteria = [.. criteria
                        .Select(c =>
                        {
                            var existing = a.Scores.FirstOrDefault(s => s.CriterionId == c.Id);
                            return new MyEvaluationCriterionDto
                            {
                                CriterionId = c.Id,
                                Name = c.Name,
                                Weight = c.Weight,
                                Score = existing?.Score,
                                Remarks = existing?.Remarks
                            };
                        })
                        .OrderBy(c => c.Name)]
                };
                dto.IsComplete = dto.Criteria.All(c => c.Score.HasValue);
                result.Add(dto);
            }

            return [.. result.OrderBy(r => r.IsComplete).ThenBy(r => r.CandidateName)];
        }
    }

    // ---- Submit my scores ----------------------------------------------------------------

    /// <summary>
    /// Writes the caller's own criterion scores for one applicant.
    ///
    /// <para>⚠️ Every criterion in the payload must be one the caller is assigned to. There is no
    /// "not an evaluator, so treat as HR" fallback here — see <see cref="EvaluatorPortal"/>. HR keeps
    /// its own route through the recruitment screens.</para>
    /// </summary>
    public class SubmitMyEvaluation(
        IRepository<JobApplication> applications,
        IRepository<ApplicationCriterionScore> scores,
        IRepository<JobRequisition> requisitions,
        IRepository<User> users,
        IRepository<CriterionEvaluator> evaluators,
        ICurrentUserService currentUser,
        ILogger<SubmitMyEvaluation> logger) : ISubmitMyEvaluation
    {
        public async Task<int> SubmitAsync(SubmitMyEvaluationDto dto)
        {
            if (dto.ApplicationId == Guid.Empty)
                throw new ValidationException("applicationId", "An applicant is required.");
            if (dto.Scores is null || dto.Scores.Count == 0)
                throw new ValidationException("scores", "Provide at least one criterion score.");

            var (employeeId, mine) = await EvaluatorPortal.MineAsync(users, evaluators, currentUser.GetCurrentUserId());
            if (employeeId is null || mine.Count == 0)
                throw new ValidationException("access", "You are not assigned as an evaluator for any criteria.");

            var submitted = dto.Scores.Select(s => s.CriterionId).Distinct().ToList();
            if (submitted.Any(id => !mine.Contains(id)))
                throw new ValidationException("scores",
                    "You may only submit scores for the criteria you are assigned to as an evaluator.");

            var application = await applications.GetAll()
                    .Include(a => a.CriterionScores)
                    .FirstOrDefaultAsync(a => a.Id == dto.ApplicationId)
                ?? throw new NotFoundException(nameof(JobApplication), dto.ApplicationId.ToString());

            if (!EvaluationGuard.EvaluatableStages.Contains(application.Stage))
                throw new ValidationException("applicationId",
                    $"The evaluation is complete — a {application.Stage} applicant's scores are locked and can no longer be changed.");

            // The criteria must belong to THIS applicant's requisition. Without it, an evaluator on
            // requisition A could score an applicant of requisition B by posting their own criterion
            // id against someone else's application.
            var validHere = (await requisitions.GetAll().AsNoTracking()
                .SelectMany(q => q.ScreeningCriteria)
                .Where(c => c.RequisitionId == application.RequisitionId)
                .Select(c => new { c.Id, c.Weight })
                .ToListAsync()).ToDictionary(c => c.Id, c => c.Weight);

            if (submitted.Any(id => !validHere.ContainsKey(id)))
                throw new ValidationException("scores", "One of those criteria does not belong to this applicant's vacancy.");

            var before = application.CriterionScores.Select(s => s.Id).ToHashSet();
            foreach (var entry in dto.Scores)
            {
                application.ScoreCriterion(entry.CriterionId, entry.Score,
                    Math.Max(1, validHere[entry.CriterionId]), entry.Remarks, currentUser.GetCurrentUserName());
            }
            application.RecomputeScreeningScore();

            foreach (var score in application.CriterionScores.Where(s => !before.Contains(s.Id)))
            {
                if (string.IsNullOrEmpty(score.TenantId)) score.TenantId = application.TenantId;
                await scores.AddAsync(score);
            }
            applications.UpdateAsync(application);
            await applications.SaveChangesAsync();

            logger.LogInformation("Evaluator {EmployeeId} submitted {Count} score(s) for Application {Id} (total {Total})",
                employeeId, dto.Scores.Count, dto.ApplicationId, application.ScreeningScore);
            return dto.Scores.Count;
        }
    }
}
