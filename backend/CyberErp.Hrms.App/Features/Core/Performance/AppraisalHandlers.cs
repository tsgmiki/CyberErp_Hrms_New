using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- Read DTOs ----------------------------------------------------------
    public class AppraisalLineDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal? SelfScore { get; set; }
        public string? SelfComments { get; set; }
        public decimal? ManagerScore { get; set; }
        public string? ManagerComments { get; set; }
        public int SortOrder { get; set; }
        /// <summary>"Goal" or "Competency" — lets a single UI grid render both.</summary>
        public string LineType { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
    }

    public class AppraisalDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid ReviewCycleId { get; set; }
        public string? ReviewCycleName { get; set; }
        public Guid? AppraisalTemplateId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal GoalsWeight { get; set; }
        public decimal CompetenciesWeight { get; set; }
        public string Stage { get; set; } = string.Empty;
        public string? SelfComments { get; set; }
        public string? ManagerComments { get; set; }
        public decimal? OverallScore { get; set; }
        public Guid? FinalRatingLevelId { get; set; }
        public string? FinalRatingLabel { get; set; }
        public DateTime? SelfSubmittedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCalibrated { get; set; }
        public string AcknowledgmentStatus { get; set; } = string.Empty;
        public string? EmployeeSignature { get; set; }
        public DateTime? EmployeeSignedAt { get; set; }
        public string? ManagerSignature { get; set; }
        public DateTime? ManagerSignedAt { get; set; }
        public string? ReviewerComments { get; set; }
        public string? ReviewerSignature { get; set; }
        public DateTime? ReviewerSignedAt { get; set; }
        public string? HrSignature { get; set; }
        public DateTime? HrSignedAt { get; set; }
        public List<AppraisalLineDto> Goals { get; set; } = [];
        public List<AppraisalLineDto> Competencies { get; set; } = [];
        public List<AppraisalPeerReviewDto> PeerReviews { get; set; } = [];
        /// <summary>Average of submitted peer scores (HC127) — advisory, not folded into OverallScore.</summary>
        public decimal? PeerAverageScore { get; set; }

        // Identity-aware routing (filled on the single-record read): the current user's relationship to this
        // appraisal, whether they may act on the current stage, and who the stage is waiting on.
        public string? CurrentUserRole { get; set; }
        public bool CanActCurrentStage { get; set; }
        public string? CurrentStageActorName { get; set; }
        /// <summary>The caller is the resolved manager for this appraisal (drives counter-sign + peer administration).</summary>
        public bool CanManagerSign { get; set; }
        /// <summary>The caller is an HR administrator (head office / configured HR approver).</summary>
        public bool CanAdminister { get; set; }
    }

    // ---- Write DTOs ---------------------------------------------------------
    public class GenerateAppraisalDto
    {
        public Guid EmployeeId { get; set; }
        public Guid ReviewCycleId { get; set; }
        public Guid? AppraisalTemplateId { get; set; }
    }

    public class AppraisalLineScoreDto
    {
        public Guid LineId { get; set; }
        public decimal? Score { get; set; }
        public string? Comments { get; set; }
    }

    public class SaveAppraisalScoresDto
    {
        public Guid Id { get; set; }
        /// <summary>"Self" or "Manager".</summary>
        public string Scope { get; set; } = "Self";
        public string? Comments { get; set; }
        public List<AppraisalLineScoreDto> Goals { get; set; } = [];
        public List<AppraisalLineScoreDto> Competencies { get; set; } = [];
    }

    public class GenerateAppraisalDtoValidator : AbstractValidator<GenerateAppraisalDto>
    {
        public GenerateAppraisalDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.ReviewCycleId).NotEmpty();
        }
    }

    public class SaveAppraisalScoresDtoValidator : AbstractValidator<SaveAppraisalScoresDto>
    {
        public SaveAppraisalScoresDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Scope).NotEmpty()
                .Must(v => v is "Self" or "Manager").WithMessage("Scope must be 'Self' or 'Manager'.");
        }
    }

    /// <summary>Second-level manager (reviewer) sign-off — an approval signature plus high-level comments.</summary>
    public class ReviewerSignOffDto
    {
        public Guid Id { get; set; }
        public string Signature { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }

    public class ReviewerSignOffDtoValidator : AbstractValidator<ReviewerSignOffDto>
    {
        public ReviewerSignOffDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Signature).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Comments).MaximumLength(4000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGenerateAppraisal { Task<Guid> GenerateAsync(GenerateAppraisalDto dto); }
    public interface ISaveAppraisalScores { Task SaveAsync(SaveAppraisalScoresDto dto); }
    public interface ISubmitAppraisalSelfAssessment { Task SubmitAsync(Guid id); }
    public interface ICompleteAppraisal { Task CompleteAsync(Guid id); }
    public interface IReviewerSignOffAppraisal { Task SignOffAsync(ReviewerSignOffDto dto); }
    public interface IHrCloseAppraisal { Task CloseAsync(SignAppraisalDto dto); }
    public interface IDeleteAppraisal { Task DeleteAsync(Guid id); }
    public interface IGetAppraisalById { Task<AppraisalDto> GetAsync(Guid id); }
    public interface IGetAllAppraisals { Task<PaginatedResponse<AppraisalDto>> GetAsync(GetAllRequest request); }

    // ---- Generate -----------------------------------------------------------
    public class GenerateAppraisal(
        IRepository<Appraisal> repository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<AppraisalTemplate> templateRepository,
        IRepository<EmployeeGoal> goalRepository,
        IRepository<PositionCompetency> positionCompetencyRepository,
        IRepository<Competency> competencyRepository,
        IPerformanceHistoryWriter history,
        Workflows.IWorkflowService workflowService,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IPerformanceVisibilityService visibility,
        IValidator<GenerateAppraisalDto> validator,
        ILogger<GenerateAppraisal> logger) : IGenerateAppraisal
    {
        public async Task<Guid> GenerateAsync(GenerateAppraisalDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Self-service scope: HR (all), a manager (their subtree), or the employee themselves (own only).
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(dto.EmployeeId),
                    "Only an HR administrator, the employee's manager, or the employee themselves can initiate this appraisal.");

            var employee = await employeeRepository.GetAll().FirstOrDefaultAsync(e => e.Id == dto.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            var cycle = await reviewCycleRepository.GetAll().FirstOrDefaultAsync(c => c.Id == dto.ReviewCycleId)
                ?? throw new NotFoundException(nameof(ReviewCycle), dto.ReviewCycleId.ToString());

            if (await repository.GetAll().AnyAsync(a => a.EmployeeId == dto.EmployeeId && a.ReviewCycleId == dto.ReviewCycleId))
                throw new ValidationException(nameof(dto.EmployeeId), "An appraisal already exists for this employee in this review cycle.");

            // Weights: from the chosen template, else a 50/50 default.
            decimal goalsWeight = 50, competenciesWeight = 50;
            if (dto.AppraisalTemplateId.HasValue)
            {
                var template = await templateRepository.GetAll().FirstOrDefaultAsync(t => t.Id == dto.AppraisalTemplateId.Value)
                    ?? throw new NotFoundException(nameof(AppraisalTemplate), dto.AppraisalTemplateId.Value.ToString());
                goalsWeight = template.GoalsWeight;
                competenciesWeight = template.CompetenciesWeight;
            }

            // Period window: calendar cycles share the cycle window; a probation cycle is tenure-anchored,
            // so the window is derived per employee from their hire → probation-end dates.
            DateTime periodStart, periodEnd;
            if (cycle.PeriodType == ReviewPeriodType.Probation)
            {
                if (!employee.IsProbation)
                    throw new ValidationException(nameof(dto.EmployeeId), "A probation review can only be generated for an employee who is on probation.");
                if (!employee.HireDate.HasValue)
                    throw new ValidationException(nameof(dto.EmployeeId), "The employee must have a hire date for a probation review.");
                periodStart = employee.HireDate.Value;
                // The cycle's probation duration (if set) is the source of truth: end = hire date + n months.
                // Otherwise fall back to the employee's stored probation-end date.
                if (cycle.ProbationDurationMonths.HasValue)
                    periodEnd = periodStart.AddMonths(cycle.ProbationDurationMonths.Value);
                else if (employee.ProbationEndDate.HasValue)
                    periodEnd = employee.ProbationEndDate.Value;
                else
                    throw new ValidationException(nameof(dto.EmployeeId),
                        "Set a probation duration on the cycle, or a probation end date on the employee.");
                // The cycle window acts as the selection window — the probation must fall due within it.
                if (periodEnd < cycle.StartDate || periodEnd > cycle.EndDate)
                    throw new ValidationException(nameof(dto.EmployeeId),
                        "The employee's probation end date falls outside this review cycle's window.");
            }
            else
            {
                periodStart = cycle.StartDate;
                periodEnd = cycle.EndDate;
            }

            var startStage = cycle.EnableSelfAssessment ? AppraisalStage.SelfAssessment : AppraisalStage.ManagerReview;
            var appraisal = Appraisal.Create(dto.EmployeeId, dto.ReviewCycleId, dto.AppraisalTemplateId,
                periodStart, periodEnd, goalsWeight, competenciesWeight, startStage);

            // Goal lines: the employee's non-cancelled goals for this cycle.
            var goals = await goalRepository.GetAll()
                .Where(g => g.EmployeeId == dto.EmployeeId && g.ReviewCycleId == dto.ReviewCycleId && g.Status != GoalStatus.Cancelled)
                .OrderByDescending(g => g.StartDate).ToListAsync();
            foreach (var g in goals)
                appraisal.AddGoalLine(g.Id, g.Title, g.Weight);

            // Competency lines: the employee's position competencies (if placed on a position).
            if (employee.PositionId.HasValue)
            {
                var comps = await positionCompetencyRepository.GetAll()
                    .Where(pc => pc.PositionId == employee.PositionId.Value)
                    .Join(competencyRepository.GetAll(), pc => pc.CompetencyId, c => c.Id,
                        (pc, c) => new { c.Id, c.Name, pc.Weight })
                    .ToListAsync();
                foreach (var c in comps)
                    appraisal.AddCompetencyLine(c.Id, c.Name, c.Weight);
            }

            await repository.AddAsync(appraisal);   // stamps the root's TenantId
            AppraisalMapper.StampLineTenant(appraisal);
            await history.WriteAsync("Appraisal", appraisal.Id, "Generated",
                $"Appraisal generated ({appraisal.Goals.Count} goals, {appraisal.Competencies.Count} competencies).");
            await repository.SaveChangesAsync();

            // Guarantee the appraisal is workflow-governed: auto-create the routing definition if the tenant has
            // none, so every appraisal gets an instance (no "open" gating gap). Admins can edit it afterwards.
            if (!await workflowDefinitions.GetAll().AnyAsync(d => d.EntityType == WorkflowEntityTypes.Appraisal && d.IsActive))
            {
                var def = AppraisalWorkflowService.BuildDefaultDefinition();
                await workflowDefinitions.AddAsync(def);
                Workflows.SaveWorkflowDefinition.StampStepTenant(def);
                await workflowDefinitions.SaveChangesAsync();
                logger.LogInformation("Auto-created the Appraisal workflow definition for the tenant.");
            }

            // Start the routing instance in the shared workflow engine (EntityType "Appraisal", subject = the
            // appraisee), beginning at the appraisal's actual first stage. Approvers resolve lazily per stage —
            // don't pre-validate (optional second-level/HR steps may never be reached).
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.Appraisal, appraisal.Id, appraisal.EmployeeId,
                $"Appraisal — {employee.EmployeeNumber} ({cycle.Name})",
                startAtStepName: startStage.ToString(), preValidateApprovers: false);

            logger.LogInformation("Generated Appraisal {Id} for employee {EmployeeId} ({Goals} goals, {Comps} competencies)",
                appraisal.Id, dto.EmployeeId, appraisal.Goals.Count, appraisal.Competencies.Count);
            return appraisal.Id;
        }
    }

    // ---- Score (self / manager) ---------------------------------------------
    public class SaveAppraisalScores(
        IRepository<Appraisal> repository,
        IPerformanceHistoryWriter history,
        IAppraisalWorkflowService workflowService,
        IValidator<SaveAppraisalScoresDto> validator,
        ILogger<SaveAppraisalScores> logger) : ISaveAppraisalScores
    {
        public async Task SaveAsync(SaveAppraisalScoresDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appraisal = await repository.GetAll().Include(a => a.Goals).Include(a => a.Competencies)
                .FirstOrDefaultAsync(a => a.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Appraisal), dto.Id.ToString());

            var isSelf = dto.Scope == "Self";
            // Stage-based locking (HC133) — surface as a clean 400 rather than a domain 500.
            if (isSelf && appraisal.Stage != AppraisalStage.SelfAssessment)
                throw new ValidationException(nameof(dto.Scope), "Self scores can only be edited during the self-assessment stage.");
            if (!isSelf && appraisal.Stage != AppraisalStage.ManagerReview)
                throw new ValidationException(nameof(dto.Scope), "Manager scores can only be edited during the manager-review stage.");
            // Identity gating from the workflow engine: only the configured approver for the current stage
            // (SelfAssessment → the subject employee; ManagerReview → the manager) may edit scores.
            await workflowService.EnsureCanActAsync(appraisal);

            var goalScores = dto.Goals.Select(g => new AppraisalLineScore(g.LineId, g.Score, g.Comments));
            var compScores = dto.Competencies.Select(c => new AppraisalLineScore(c.LineId, c.Score, c.Comments));
            if (isSelf) appraisal.ApplySelfScores(dto.Comments, goalScores, compScores);
            else appraisal.ApplyManagerScores(dto.Comments, goalScores, compScores);

            repository.UpdateAsync(appraisal);
            await history.WriteAsync("Appraisal", dto.Id, "Scored", $"{dto.Scope} scores saved.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Saved {Scope} scores for Appraisal {Id}", dto.Scope, dto.Id);
        }
    }

    // ---- Submit self-assessment ---------------------------------------------
    public class SubmitAppraisalSelfAssessment(
        IRepository<Appraisal> repository,
        IPerformanceHistoryWriter history,
        IAppraisalWorkflowService workflowService,
        ILogger<SubmitAppraisalSelfAssessment> logger) : ISubmitAppraisalSelfAssessment
    {
        public async Task SubmitAsync(Guid id)
        {
            // Load tracked (GetByIdAsync is AsNoTracking → re-attach would break the RowVersion token).
            var appraisal = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Appraisal), id.ToString());
            if (appraisal.Stage != AppraisalStage.SelfAssessment)
                throw new ValidationException(nameof(id), "Only a self-assessment can be submitted for manager review.");
            await workflowService.EnsureCanActAsync(appraisal);
            appraisal.SubmitSelfAssessment();
            await history.WriteAsync("Appraisal", id, "SelfSubmitted", "Self-assessment submitted for manager review.");
            await repository.SaveChangesAsync();
            await workflowService.SyncInstanceAsync(appraisal, "Self-assessment submitted");   // route to the manager
            logger.LogInformation("Submitted self-assessment for Appraisal {Id}", id);
        }
    }

    // ---- Complete manager review (compute overall score, HC138) -------------
    public class CompleteAppraisal(
        IRepository<Appraisal> repository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IPerformanceHistoryWriter history,
        IEnumerable<IAppraisalCompletedHandler> completedHandlers,
        IAppraisalWorkflowService workflowService,
        ILogger<CompleteAppraisal> logger) : ICompleteAppraisal
    {
        public async Task CompleteAsync(Guid id)
        {
            var appraisal = await repository.GetAll().Include(a => a.Goals).Include(a => a.Competencies)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException(nameof(Appraisal), id.ToString());
            if (appraisal.Stage != AppraisalStage.ManagerReview)
                throw new ValidationException(nameof(id), "Only an appraisal in manager review can be completed.");
            await workflowService.EnsureCanActAsync(appraisal);

            // Distinguish "nothing to score" (a config gap) from "scored nothing yet" so the message is actionable.
            if (appraisal.Goals.Count == 0 && appraisal.Competencies.Count == 0)
                throw new ValidationException(nameof(id),
                    "This appraisal has no goals or competencies to score. It is generated from the employee's goals " +
                    "for this review cycle plus their position competencies — add those (or set the goals BEFORE generating), " +
                    "then regenerate the appraisal.");

            var goalScore = WeightedAverage(appraisal.Goals.Select(g => (g.ManagerScore, g.Weight)));
            var compScore = WeightedAverage(appraisal.Competencies.Select(c => (c.ManagerScore, c.Weight)));
            if (goalScore is null && compScore is null)
                throw new ValidationException(nameof(id), "Enter and save a manager score for at least one goal or competency before completing the appraisal.");

            decimal overall;
            if (goalScore is not null && compScore is not null)
            {
                var wSum = appraisal.GoalsWeight + appraisal.CompetenciesWeight;
                overall = wSum > 0
                    ? (goalScore.Value * appraisal.GoalsWeight + compScore.Value * appraisal.CompetenciesWeight) / wSum
                    : (goalScore.Value + compScore.Value) / 2m;
            }
            else overall = goalScore ?? compScore!.Value;

            overall = Math.Round(overall, 2);
            var finalLevelId = await ResolveRatingLevelAsync(appraisal.ReviewCycleId, overall);

            // Route onward: second-level reviewer if the cycle enables it, else straight to the employee's
            // acknowledgment. The overall score is fixed here regardless of the remaining sign-off steps.
            var enableSecondLevel = await reviewCycleRepository.GetAll()
                .Where(c => c.Id == appraisal.ReviewCycleId).Select(c => c.EnableSecondLevelReview).FirstOrDefaultAsync();
            var nextStage = enableSecondLevel ? AppraisalStage.SecondLevelReview : AppraisalStage.EmployeeAcknowledgment;

            appraisal.CompleteManagerReview(overall, finalLevelId, nextStage);
            repository.UpdateAsync(appraisal);
            await history.WriteAsync("Appraisal", id, "ManagerReviewCompleted",
                $"Manager review completed with overall score {overall}; routed to {nextStage}.");
            await repository.SaveChangesAsync();
            await workflowService.SyncInstanceAsync(appraisal, $"Manager review completed (score {overall})");   // route onward
            logger.LogInformation("Completed manager review for Appraisal {Id} (score {Score}, next {Stage})", id, overall, nextStage);

            // The "appraisal is final" event (HC147/HC153) fires only once the flow reaches the terminal,
            // locked state — normally at the employee acknowledgment or HR sign-off, not here.
            if (nextStage == AppraisalStage.Completed)
                await AppraisalCompletion.NotifyAsync(completedHandlers, logger, id, appraisal.EmployeeId);
        }

        /// <summary>Weighted average over the lines that carry a manager score; equal-weight fallback.</summary>
        private static decimal? WeightedAverage(IEnumerable<(decimal? Score, decimal Weight)> lines)
        {
            var scored = lines.Where(l => l.Score.HasValue).Select(l => (Score: l.Score!.Value, l.Weight)).ToList();
            if (scored.Count == 0) return null;
            var totalWeight = scored.Sum(l => l.Weight);
            return totalWeight > 0
                ? scored.Sum(l => l.Score * l.Weight) / totalWeight
                : scored.Average(l => l.Score);
        }

        /// <summary>Maps an overall score to a rating level: a band containing it, else the nearest ordinal.</summary>
        private async Task<Guid?> ResolveRatingLevelAsync(Guid reviewCycleId, decimal overall)
        {
            var scaleId = await reviewCycleRepository.GetAll().Where(c => c.Id == reviewCycleId)
                .Select(c => c.RatingScaleId).FirstOrDefaultAsync();
            if (scaleId == Guid.Empty) return null;
            var levels = await ratingLevelRepository.GetAll().Where(l => l.RatingScaleId == scaleId).ToListAsync();
            if (levels.Count == 0) return null;

            // Percentage bands take priority when configured.
            var band = levels.FirstOrDefault(l => l.MinScore.HasValue && l.MaxScore.HasValue
                && overall >= l.MinScore.Value && overall <= l.MaxScore.Value);
            if (band is not null) return band.Id;

            // Otherwise snap to the nearest numeric level value.
            return levels.OrderBy(l => Math.Abs(l.Value - overall)).First().Id;
        }
    }

    /// <summary>Fires the appraisal-completed extension handlers (HC147/HC153) when an appraisal reaches the
    /// terminal, locked state — best-effort; a listener failure must not undo the transition.</summary>
    internal static class AppraisalCompletion
    {
        internal static async Task NotifyAsync(IEnumerable<IAppraisalCompletedHandler> handlers, ILogger logger,
            Guid appraisalId, Guid employeeId)
        {
            foreach (var handler in handlers)
            {
                try { await handler.OnAppraisalCompletedAsync(appraisalId, employeeId); }
                catch (Exception ex) { logger.LogWarning(ex, "Appraisal-completed handler {Handler} failed for {Id}", handler.GetType().Name, appraisalId); }
            }
        }
    }

    // ---- Reviewer (second-level) sign-off -----------------------------------
    public class ReviewerSignOffAppraisal(
        IRepository<Appraisal> repository,
        IPerformanceHistoryWriter history,
        IAppraisalWorkflowService workflowService,
        IValidator<ReviewerSignOffDto> validator,
        ILogger<ReviewerSignOffAppraisal> logger) : IReviewerSignOffAppraisal
    {
        public async Task SignOffAsync(ReviewerSignOffDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appraisal = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Appraisal), dto.Id.ToString());
            if (appraisal.Stage != AppraisalStage.SecondLevelReview)
                throw new ValidationException(nameof(dto.Id), "This appraisal is not awaiting second-level review.");
            await workflowService.EnsureCanActAsync(appraisal);

            // Reviewer approval always routes back to the employee for the final acknowledgment signature.
            appraisal.ReviewerSignOff(dto.Signature, dto.Comments, AppraisalStage.EmployeeAcknowledgment);
            repository.UpdateAsync(appraisal);
            await history.WriteAsync("Appraisal", dto.Id, "ReviewerSignedOff",
                $"Second-level reviewer signed off ({dto.Signature}).");
            await repository.SaveChangesAsync();
            await workflowService.SyncInstanceAsync(appraisal, "Reviewer signed off");   // route to the employee
            logger.LogInformation("Reviewer signed off Appraisal {Id}", dto.Id);
        }
    }

    // ---- HR final sign-off (close & lock) -----------------------------------
    public class HrCloseAppraisal(
        IRepository<Appraisal> repository,
        IPerformanceHistoryWriter history,
        IEnumerable<IAppraisalCompletedHandler> completedHandlers,
        IAppraisalWorkflowService workflowService,
        IValidator<SignAppraisalDto> validator,
        ILogger<HrCloseAppraisal> logger) : IHrCloseAppraisal
    {
        public async Task CloseAsync(SignAppraisalDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var appraisal = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Appraisal), dto.Id.ToString());
            if (appraisal.Stage != AppraisalStage.HrSignOff)
                throw new ValidationException(nameof(dto.Id), "This appraisal is not awaiting HR sign-off.");
            await workflowService.EnsureCanActAsync(appraisal);

            appraisal.HrClose(dto.Signature);
            repository.UpdateAsync(appraisal);
            await history.WriteAsync("Appraisal", dto.Id, "HrClosed", $"HR closed and locked the appraisal ({dto.Signature}).");
            await repository.SaveChangesAsync();
            await workflowService.SyncInstanceAsync(appraisal, "HR closed & locked");   // completes the routing instance
            logger.LogInformation("HR closed Appraisal {Id}", dto.Id);

            // Terminal reached — notify downstream modules (HC147/HC153).
            await AppraisalCompletion.NotifyAsync(completedHandlers, logger, appraisal.Id, appraisal.EmployeeId);
        }
    }

    // ---- Delete -------------------------------------------------------------
    public class DeleteAppraisal(
        IRepository<Appraisal> repository,
        IAppraisalWorkflowService workflowService,
        ILogger<DeleteAppraisal> logger) : IDeleteAppraisal
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Appraisal), id.ToString());
            if (!await workflowService.CanAdministerAsync())
                throw new ValidationException(nameof(id), "Only an HR administrator can delete an appraisal.");
            repository.Delete(entity);   // lines cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Appraisal {Id}", id);
        }
    }

    // ---- Reads --------------------------------------------------------------
    public class GetAppraisalById(
        IRepository<Appraisal> repository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IRepository<AppraisalPeerReview> peerRepository,
        IAppraisalWorkflowService workflowService) : IGetAppraisalById
    {
        public async Task<AppraisalDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().Include(a => a.Goals).Include(a => a.Competencies).AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException(nameof(Appraisal), id.ToString());
            // Visibility: only the appraisee, their manager chain, or an HR administrator may open an appraisal.
            await workflowService.EnsureCanViewAsync(entity);
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == entity.EmployeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
            var cycleName = await reviewCycleRepository.GetAll().Where(c => c.Id == entity.ReviewCycleId).Select(c => c.Name).FirstOrDefaultAsync();
            var finalLabel = entity.FinalRatingLevelId.HasValue
                ? await ratingLevelRepository.GetAll().Where(l => l.Id == entity.FinalRatingLevelId.Value).Select(l => l.Label).FirstOrDefaultAsync()
                : null;
            var dto = AppraisalMapper.Map(entity, employeeName, cycleName, finalLabel);

            var peers = await peerRepository.GetAll().AsNoTracking().Where(p => p.AppraisalId == id).ToListAsync();
            var employees = employeeRepository.GetAll();
            foreach (var p in peers)
            {
                var name = await employees.Where(e => e.Id == p.PeerEmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
                dto.PeerReviews.Add(new AppraisalPeerReviewDto
                {
                    Id = p.Id,
                    AppraisalId = p.AppraisalId,
                    PeerEmployeeId = p.PeerEmployeeId,
                    PeerEmployeeName = name,
                    Status = p.Status.ToString(),
                    Score = p.Score,
                    Comments = p.Comments,
                    SubmittedAt = p.SubmittedAt
                });
            }
            var submitted = peers.Where(p => p.Status == PeerReviewStatus.Submitted && p.Score.HasValue).Select(p => p.Score!.Value).ToList();
            dto.PeerAverageScore = submitted.Count > 0 ? Math.Round(submitted.Average(), 2) : null;

            // Identity-aware routing: who owns the current stage, and whether the caller may act on it now.
            var actor = await workflowService.ResolveAsync(entity);
            dto.CurrentUserRole = actor.CurrentUserRole;
            dto.CanActCurrentStage = actor.CanActCurrentStage;
            dto.CurrentStageActorName = actor.CurrentStageActorName;
            dto.CanManagerSign = await workflowService.CanActOnStageAsync(entity, nameof(AppraisalStage.ManagerReview));
            dto.CanAdminister = await workflowService.CanAdministerAsync();

            // The appraisee sees only the peer AVERAGE — never who scored what (independent 360 feedback).
            if (actor.ViewerIsAppraisee && !dto.CanAdminister && !dto.CanManagerSign)
                dto.PeerReviews = [];
            return dto;
        }
    }

    public class GetAllAppraisals(
        IRepository<Appraisal> repository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IAppraisalWorkflowService workflowService,
        IPerformanceVisibilityService visibility) : IGetAllAppraisals
    {
        public async Task<PaginatedResponse<AppraisalDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (request.ReviewCycleId.HasValue)
                query = query.Where(x => x.ReviewCycleId == request.ReviewCycleId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AppraisalStage>(request.Status, out var stage))
                query = query.Where(x => x.Stage == stage);

            int total;
            List<Appraisal> rows;
            if (request.AssignedToMe == true)
            {
                // Worklist: the engine tells us which running appraisal instances route to the current user now
                // (same authorization as the shared "My Approvals" inbox). Intersect with any other filters.
                var mineIds = await workflowService.PendingMyActionAppraisalIdsAsync();
                var mine = mineIds.Count == 0
                    ? []
                    : await query.Where(x => mineIds.Contains(x.Id)).OrderByDescending(x => x.CreatedAt).ToListAsync();
                total = mine.Count;
                rows = mine.Skip(skip).Take(take).ToList();
            }
            else
            {
                // Role-based visibility as a single SQL predicate (no in-memory scans): admin → all;
                // manager → own + employees whose unit is in their subtree; employee → own only.
                var scope = await visibility.GetScopeAsync();
                if (!scope.IsAdmin)
                {
                    var myEmp = scope.EmployeeId ?? Guid.Empty;
                    if (scope.IsManager)
                    {
                        var unitIds = scope.UnitIds;
                        var emps = employeeRepository.GetAll();
                        query = query.Where(a => a.EmployeeId == myEmp ||
                            emps.Any(e => e.Id == a.EmployeeId && e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                    }
                    else
                    {
                        query = query.Where(a => a.EmployeeId == myEmp);
                    }
                }
                total = await query.CountAsync();
                rows = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take).ToListAsync();
            }

            var employees = employeeRepository.GetAll();
            var cycles = reviewCycleRepository.GetAll();
            var data = new List<AppraisalDto>(rows.Count);
            foreach (var a in rows)
            {
                var employeeName = await employees.Where(e => e.Id == a.EmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
                var cycleName = await cycles.Where(c => c.Id == a.ReviewCycleId).Select(c => c.Name).FirstOrDefaultAsync();
                // List rows omit the line detail — kept light for the grid.
                data.Add(AppraisalMapper.MapHeader(a, employeeName, cycleName));
            }

            return new PaginatedResponse<AppraisalDto> { Total = total, Data = data };
        }
    }

    internal static class AppraisalMapper
    {
        internal static AppraisalDto MapHeader(Appraisal a, string? employeeName, string? cycleName) => new()
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = employeeName,
            ReviewCycleId = a.ReviewCycleId,
            ReviewCycleName = cycleName,
            AppraisalTemplateId = a.AppraisalTemplateId,
            PeriodStart = a.PeriodStart,
            PeriodEnd = a.PeriodEnd,
            GoalsWeight = a.GoalsWeight,
            CompetenciesWeight = a.CompetenciesWeight,
            Stage = a.Stage.ToString(),
            SelfComments = a.SelfComments,
            ManagerComments = a.ManagerComments,
            OverallScore = a.OverallScore,
            FinalRatingLevelId = a.FinalRatingLevelId,
            SelfSubmittedAt = a.SelfSubmittedAt,
            CompletedAt = a.CompletedAt,
            IsCalibrated = a.IsCalibrated,
            AcknowledgmentStatus = a.AcknowledgmentStatus.ToString(),
            EmployeeSignature = a.EmployeeSignature,
            EmployeeSignedAt = a.EmployeeSignedAt,
            ManagerSignature = a.ManagerSignature,
            ManagerSignedAt = a.ManagerSignedAt,
            ReviewerComments = a.ReviewerComments,
            ReviewerSignature = a.ReviewerSignature,
            ReviewerSignedAt = a.ReviewerSignedAt,
            HrSignature = a.HrSignature,
            HrSignedAt = a.HrSignedAt
        };

        internal static AppraisalDto Map(Appraisal a, string? employeeName, string? cycleName, string? finalLabel)
        {
            var dto = MapHeader(a, employeeName, cycleName);
            dto.FinalRatingLabel = finalLabel;
            dto.Goals = a.Goals.OrderBy(g => g.SortOrder).Select(g => new AppraisalLineDto
            {
                Id = g.Id,
                Title = g.Title,
                Weight = g.Weight,
                SelfScore = g.SelfScore,
                SelfComments = g.SelfComments,
                ManagerScore = g.ManagerScore,
                ManagerComments = g.ManagerComments,
                SortOrder = g.SortOrder,
                LineType = "Goal",
                ReferenceId = g.EmployeeGoalId
            }).ToList();
            dto.Competencies = a.Competencies.OrderBy(c => c.SortOrder).Select(c => new AppraisalLineDto
            {
                Id = c.Id,
                Title = c.CompetencyName,
                Weight = c.Weight,
                SelfScore = c.SelfScore,
                SelfComments = c.SelfComments,
                ManagerScore = c.ManagerScore,
                ManagerComments = c.ManagerComments,
                SortOrder = c.SortOrder,
                LineType = "Competency",
                ReferenceId = c.CompetencyId
            }).ToList();
            return dto;
        }

        /// <summary>Copies the root's TenantId onto cascade-inserted lines (repository stamps roots only).</summary>
        internal static void StampLineTenant(Appraisal a)
        {
            foreach (var g in a.Goals)
                if (string.IsNullOrEmpty(g.TenantId)) g.TenantId = a.TenantId;
            foreach (var c in a.Competencies)
                if (string.IsNullOrEmpty(c.TenantId)) c.TenantId = a.TenantId;
        }
    }
}
