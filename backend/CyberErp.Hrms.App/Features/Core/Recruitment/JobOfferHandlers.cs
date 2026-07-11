using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- DTOs -----------------------------------------------------------------

    public class JobOfferDto
    {
        public Guid Id { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public Guid ApplicationId { get; set; }
        public Guid? HiringManagerEmployeeId { get; set; }
        public string? HiringManagerName { get; set; }
        public decimal Salary { get; set; }
        public Guid? SalaryScaleId { get; set; }
        public decimal? SalaryScaleAmount { get; set; }
        public string? SalaryJustification { get; set; }
        public DateTime ProposedStartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResponseNote { get; set; }
        public string? LetterText { get; set; }
        public Guid? HiredEmployeeId { get; set; }
        public bool AwaitingWorkflow { get; set; }
    }

    public class SaveJobOfferDto
    {
        public Guid? Id { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid? HiringManagerEmployeeId { get; set; }
        public decimal Salary { get; set; }
        public Guid? SalaryScaleId { get; set; }
        public string? SalaryJustification { get; set; }
        public DateTime ProposedStartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? LetterText { get; set; }
    }

    public class SaveJobOfferDtoValidator : AbstractValidator<SaveJobOfferDto>
    {
        public SaveJobOfferDtoValidator()
        {
            RuleFor(x => x.ApplicationId).NotEmpty();
            RuleFor(x => x.Salary).GreaterThan(0);
            RuleFor(x => x.ProposedStartDate).NotEmpty();
            RuleFor(x => x.ExpiryDate).NotEmpty();
            RuleFor(x => x.SalaryJustification).MaximumLength(1000);
            RuleFor(x => x.LetterText).MaximumLength(8000);
        }
    }

    public class RespondJobOfferDto
    {
        public Guid Id { get; set; }
        /// <summary>Accept | Decline</summary>
        public string Response { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    // ---- Interfaces -------------------------------------------------------------

    public interface ISaveJobOffer { Task<Guid> SaveAsync(SaveJobOfferDto dto); }
    public interface IGetJobOffers { Task<List<JobOfferDto>> GetAsync(Guid applicationId); }
    public interface ISubmitJobOffer { Task SubmitAsync(Guid id); }
    public interface ISendJobOffer { Task SendAsync(Guid id); }
    public interface IRespondJobOffer { Task RespondAsync(RespondJobOfferDto dto); }
    public interface IWithdrawJobOffer { Task WithdrawAsync(Guid id, string? note); }
    public interface IGenerateOfferLetter { Task<string> GenerateAsync(Guid id); }
    public interface IDeleteJobOffer { Task DeleteAsync(Guid id); }

    internal static class OfferShared
    {
        internal static readonly OfferStatus[] ActiveStatuses =
            [OfferStatus.Draft, OfferStatus.PendingApproval, OfferStatus.Approved, OfferStatus.Sent];

        /// <summary>
        /// HC113: when the vacancy carries a salary scale, the offered salary must match the scale
        /// amount unless a written justification accompanies the deviation.
        /// </summary>
        internal static void EnsureSalaryAgainstScale(decimal salary, decimal? scaleAmount, string? justification)
        {
            if (scaleAmount.HasValue && salary != scaleAmount.Value && string.IsNullOrWhiteSpace(justification))
                throw new ValidationException("salary",
                    $"The offered salary deviates from the scale amount ({scaleAmount:N2}) — a justification is required (HC113).");
        }

        /// <summary>A sent offer past its expiry lapses on read (HC114) — persisted lazily.</summary>
        internal static bool ExpireIfLapsed(JobOffer offer)
        {
            if (offer.Status == OfferStatus.Sent && offer.ExpiryDate.Date < DateTime.UtcNow.Date)
            {
                offer.MarkExpired();
                return true;
            }
            return false;
        }

        /// <summary>A declined/withdrawn/expired offer releases the application back to Selected.</summary>
        internal static async Task ReleaseApplicationAsync(
            IRepository<JobApplication> applications,
            IRepository<JobApplicationStageLog> stageLogs,
            Guid applicationId, string note, string? actedBy)
        {
            var application = await applications.GetAll().Include(a => a.StageLog)
                .FirstOrDefaultAsync(a => a.Id == applicationId);
            if (application is null || application.Stage != ApplicationStage.OfferPending) return;

            var before = application.StageLog.Select(l => l.Id).ToHashSet();
            application.MoveToStage(ApplicationStage.Selected, note, actedBy);
            foreach (var log in application.StageLog.Where(l => !before.Contains(l.Id)))
            {
                if (string.IsNullOrEmpty(log.TenantId)) log.TenantId = application.TenantId;
                await stageLogs.AddAsync(log);
            }
            applications.UpdateAsync(application);
        }

        internal static JobOfferDto ToDto(JobOffer o, decimal? scaleAmount, bool awaitingWorkflow) => new()
        {
            Id = o.Id,
            OfferNumber = o.OfferNumber,
            ApplicationId = o.ApplicationId,
            HiringManagerEmployeeId = o.HiringManagerEmployeeId,
            HiringManagerName = o.HiringManagerName,
            Salary = o.Salary,
            SalaryScaleId = o.SalaryScaleId,
            SalaryScaleAmount = scaleAmount,
            SalaryJustification = o.SalaryJustification,
            ProposedStartDate = o.ProposedStartDate,
            ExpiryDate = o.ExpiryDate,
            Status = o.Status.ToString(),
            SentAt = o.SentAt,
            RespondedAt = o.RespondedAt,
            ResponseNote = o.ResponseNote,
            LetterText = o.LetterText,
            HiredEmployeeId = o.HiredEmployeeId,
            AwaitingWorkflow = awaitingWorkflow
        };
    }

    // ---- Handlers -----------------------------------------------------------------

    public class SaveJobOffer(
        IRepository<JobOffer> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<Employee> employeeRepository,
        IRepository<SalaryScale> salaryScaleRepository,
        INumberSequenceService numberSequence,
        IGetApplicationRanking rankingHandler,
        IValidator<SaveJobOfferDto> validator,
        IWorkflowGate workflowGate,
        ILogger<SaveJobOffer> logger) : ISaveJobOffer
    {
        public async Task<Guid> SaveAsync(SaveJobOfferDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var application = await applicationRepository.GetAll()
                    .FirstOrDefaultAsync(a => a.Id == dto.ApplicationId)
                ?? throw new NotFoundException(nameof(JobApplication), dto.ApplicationId.ToString());
            if (application.Stage is not (ApplicationStage.Selected or ApplicationStage.OfferPending))
                throw new ValidationException("applicationId",
                    $"Offers are made to SELECTED applicants — this application is at {application.Stage}.");

            var scaleAmount = dto.SalaryScaleId.HasValue
                ? await salaryScaleRepository.GetAll()
                    .Where(s => s.Id == dto.SalaryScaleId.Value).Select(s => (decimal?)s.Salary).FirstOrDefaultAsync()
                : null;
            OfferShared.EnsureSalaryAgainstScale(dto.Salary, scaleAmount, dto.SalaryJustification);

            var managerName = dto.HiringManagerEmployeeId.HasValue
                ? await employeeRepository.GetAll()
                    .Where(e => e.Id == dto.HiringManagerEmployeeId.Value)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefaultAsync()
                : null;

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.JobOffer, dto.Id.Value);
                var entity = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(JobOffer), dto.Id.Value.ToString());
                entity.UpdateTerms(dto.HiringManagerEmployeeId, managerName, dto.Salary, dto.SalaryScaleId,
                    dto.SalaryJustification, dto.ProposedStartDate, dto.ExpiryDate, dto.LetterText);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated JobOffer {Id}", entity.Id);
                return entity.Id;
            }

            // One ACTIVE offer per application (also backed by a filtered unique index).
            if (await repository.GetAll().AnyAsync(o =>
                    o.ApplicationId == dto.ApplicationId && OfferShared.ActiveStatuses.Contains(o.Status)))
                throw new ValidationException("applicationId",
                    "An active offer already exists for this application — withdraw it before creating another.");

            // Rank gate (mirrors the hire gate): a scored vacancy only issues offers the system can
            // honor — never to a waitlisted, unscored or mandatory-failing candidate. Without this,
            // an accepted offer would dead-end against the hire eligibility window.
            var rankingRows = await rankingHandler.GetAsync(application.RequisitionId);
            var myRow = rankingRows.FirstOrDefault(r => r.ApplicationId == dto.ApplicationId);
            if (myRow is not null && myRow.TotalCriteria > 0 && myRow.HireEligibility != "Eligible")
                throw new ValidationException("applicationId", myRow.HireEligibility switch
                {
                    "Waitlisted" =>
                        $"The candidate is ranked #{myRow.Rank} and WAITLISTED — offers go to the eligible top-ranked candidate(s) first.",
                    "NotScored" =>
                        "The candidate has not been scored against the vacancy's weighted criteria — complete the evaluation before offering.",
                    "FailsMandatory" =>
                        "The candidate failed a mandatory criterion and cannot receive an offer for this vacancy.",
                    "OfferRejected" =>
                        "The candidate already rejected an offer for this vacancy and is out of contention.",
                    _ => $"The candidate is not offer-eligible for this vacancy ({myRow.HireEligibility})."
                });

            var number = $"OFR-{await numberSequence.NextAsync("JobOffer"):D4}";
            var created = JobOffer.Create(number, dto.ApplicationId, dto.HiringManagerEmployeeId, managerName,
                dto.Salary, dto.SalaryScaleId, dto.SalaryJustification,
                dto.ProposedStartDate, dto.ExpiryDate, dto.LetterText);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created JobOffer {Id} ({Number}) for Application {ApplicationId}",
                created.Id, number, dto.ApplicationId);
            return created.Id;
        }
    }

    public class GetJobOffers(
        IRepository<JobOffer> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobApplicationStageLog> stageLogRepository,
        IRepository<SalaryScale> salaryScaleRepository,
        IWorkflowGate workflowGate) : IGetJobOffers
    {
        public async Task<List<JobOfferDto>> GetAsync(Guid applicationId)
        {
            var offers = await repository.GetAll()
                .Where(o => o.ApplicationId == applicationId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Lazy expiry (HC114): a sent offer past its date lapses and releases the application.
            var lapsed = false;
            foreach (var offer in offers.Where(OfferShared.ExpireIfLapsed))
            {
                repository.UpdateAsync(offer);
                await OfferShared.ReleaseApplicationAsync(applicationRepository, stageLogRepository,
                    offer.ApplicationId, $"Offer {offer.OfferNumber} expired unanswered", null);
                lapsed = true;
            }
            if (lapsed) await repository.SaveChangesAsync();

            var scaleIds = offers.Where(o => o.SalaryScaleId.HasValue).Select(o => o.SalaryScaleId!.Value).Distinct().ToList();
            var scaleAmounts = await salaryScaleRepository.GetAll()
                .Where(s => scaleIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Salary);

            var result = new List<JobOfferDto>();
            foreach (var o in offers)
            {
                var awaiting = o.Status == OfferStatus.PendingApproval
                    && await workflowGate.HasRunningAsync(WorkflowEntityTypes.JobOffer, o.Id);
                result.Add(OfferShared.ToDto(
                    o, o.SalaryScaleId.HasValue ? scaleAmounts.GetValueOrDefault(o.SalaryScaleId.Value) : null, awaiting));
            }
            return result;
        }
    }

    public class SubmitJobOffer(
        IRepository<JobOffer> repository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        ILogger<SubmitJobOffer> logger) : ISubmitJobOffer
    {
        public async Task SubmitAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.JobOffer, id);
            var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException(nameof(JobOffer), id.ToString());

            offer.Submit();
            repository.UpdateAsync(offer);
            await repository.SaveChangesAsync();

            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.JobOffer, offer.Id, null,
                $"Offer {offer.OfferNumber} — {offer.Salary:N2} ETB, start {offer.ProposedStartDate:yyyy-MM-dd}");

            // No active approval chain configured → the offer approves directly.
            if (!await workflowGate.HasRunningAsync(WorkflowEntityTypes.JobOffer, offer.Id))
            {
                offer.Approve();
                repository.UpdateAsync(offer);
                await repository.SaveChangesAsync();
            }
            logger.LogInformation("Submitted JobOffer {Id}", id);
        }
    }

    public class SendJobOffer(
        IRepository<JobOffer> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobApplicationStageLog> stageLogRepository,
        ICurrentUserService currentUser,
        ILogger<SendJobOffer> logger) : ISendJobOffer
    {
        public async Task SendAsync(Guid id)
        {
            var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException(nameof(JobOffer), id.ToString());

            offer.MarkSent();
            repository.UpdateAsync(offer);

            // The offer drives the pipeline: the application moves to OfferPending (logged).
            var application = await applicationRepository.GetAll().Include(a => a.StageLog)
                    .FirstOrDefaultAsync(a => a.Id == offer.ApplicationId)
                ?? throw new NotFoundException(nameof(JobApplication), offer.ApplicationId.ToString());
            if (application.Stage != ApplicationStage.OfferPending)
            {
                var before = application.StageLog.Select(l => l.Id).ToHashSet();
                application.MoveToStage(ApplicationStage.OfferPending,
                    $"Offer {offer.OfferNumber} sent", currentUser.GetCurrentUserName());
                foreach (var log in application.StageLog.Where(l => !before.Contains(l.Id)))
                {
                    if (string.IsNullOrEmpty(log.TenantId)) log.TenantId = application.TenantId;
                    await stageLogRepository.AddAsync(log);
                }
                applicationRepository.UpdateAsync(application);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Sent JobOffer {Id}", id);
        }
    }

    public class RespondJobOffer(
        IRepository<JobOffer> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobApplicationStageLog> stageLogRepository,
        ICurrentUserService currentUser,
        ILogger<RespondJobOffer> logger) : IRespondJobOffer
    {
        public async Task RespondAsync(RespondJobOfferDto dto)
        {
            var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == dto.Id)
                ?? throw new NotFoundException(nameof(JobOffer), dto.Id.ToString());

            switch (dto.Response?.Trim().ToLowerInvariant())
            {
                case "accept":
                    offer.Accept(dto.Note);
                    break;
                case "decline":
                    offer.Decline(dto.Note);
                    await OfferShared.ReleaseApplicationAsync(applicationRepository, stageLogRepository,
                        offer.ApplicationId, $"Offer {offer.OfferNumber} declined", currentUser.GetCurrentUserName());
                    break;
                default:
                    throw new ValidationException("response", "Response must be Accept or Decline.");
            }

            repository.UpdateAsync(offer);
            await repository.SaveChangesAsync();
            logger.LogInformation("JobOffer {Id} → {Status}", dto.Id, offer.Status);
        }
    }

    public class WithdrawJobOffer(
        IRepository<JobOffer> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobApplicationStageLog> stageLogRepository,
        IWorkflowGate workflowGate,
        ICurrentUserService currentUser,
        ILogger<WithdrawJobOffer> logger) : IWithdrawJobOffer
    {
        public async Task WithdrawAsync(Guid id, string? note)
        {
            // While an approval runs, the workflow screen owns the record.
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.JobOffer, id);
            var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException(nameof(JobOffer), id.ToString());

            offer.Withdraw(note);
            repository.UpdateAsync(offer);
            await OfferShared.ReleaseApplicationAsync(applicationRepository, stageLogRepository,
                offer.ApplicationId, $"Offer {offer.OfferNumber} withdrawn", currentUser.GetCurrentUserName());
            await repository.SaveChangesAsync();
            logger.LogInformation("Withdrew JobOffer {Id}", id);
        }
    }

    /// <summary>Standard offer-letter text assembled server-side (HC111) — editable before sending.</summary>
    public class GenerateOfferLetter(
        IRepository<JobOffer> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobRequisition> requisitionRepository,
        IRepository<OrganizationUnit> organizationUnitRepository) : IGenerateOfferLetter
    {
        public async Task<string> GenerateAsync(Guid id)
        {
            var offer = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException(nameof(JobOffer), id.ToString());
            var application = await applicationRepository.GetAll()
                    .FirstOrDefaultAsync(a => a.Id == offer.ApplicationId)
                ?? throw new NotFoundException(nameof(JobApplication), offer.ApplicationId.ToString());
            var candidate = await candidateRepository.GetAll()
                .Where(c => c.Id == application.CandidateId)
                .Select(c => new { c.FirstName, c.FatherName, c.GrandFatherName })
                .FirstOrDefaultAsync();
            var requisition = await requisitionRepository.GetAll()
                .Where(q => q.Id == application.RequisitionId)
                .Select(q => new { q.Title, q.EmploymentType, q.OrganizationUnitId })
                .FirstOrDefaultAsync();
            var unitName = requisition is null
                ? null
                : await organizationUnitRepository.GetAll()
                    .Where(u => u.Id == requisition.OrganizationUnitId).Select(u => u.Name).FirstOrDefaultAsync();

            var candidateName = candidate is null
                ? "Candidate"
                : string.Join(" ", new[] { candidate.FirstName, candidate.FatherName, candidate.GrandFatherName }
                    .Where(n => !string.IsNullOrWhiteSpace(n)));

            return $"""
                Dear {candidateName},

                Following your application and the completion of our selection process, we are pleased
                to offer you the position of {requisition?.Title ?? "the advertised role"}{(unitName is null ? "" : $" in {unitName}")}.

                Terms of the offer:
                  • Employment type: {requisition?.EmploymentType.ToString() ?? "Permanent"}
                  • Gross monthly salary: {offer.Salary:N2} ETB
                  • Proposed start date: {offer.ProposedStartDate:dd MMMM yyyy}

                This offer remains valid until {offer.ExpiryDate:dd MMMM yyyy}. Please confirm your
                acceptance in writing before that date; the offer lapses automatically afterwards.

                We look forward to welcoming you to the team.

                Sincerely,
                Human Resources
                (Offer {offer.OfferNumber})
                """;
        }
    }

    public class DeleteJobOffer(
        IRepository<JobOffer> repository,
        ILogger<DeleteJobOffer> logger) : IDeleteJobOffer
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException(nameof(JobOffer), id.ToString());
            if (entity.Status != OfferStatus.Draft)
                throw new ValidationException("id",
                    $"A {entity.Status} offer is part of the record — withdraw instead of deleting.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted JobOffer {Id}", id);
        }
    }
}
