using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- Interfaces -----------------------------------------------------------

    public interface IUploadCandidateDocument
    {
        Task<Guid> UploadAsync(Guid candidateId, string documentType, Stream content, string fileName, string contentType, long length);
    }
    public interface IGetCandidateDocuments { Task<List<CandidateDocumentDto>> GetAsync(Guid candidateId); }
    public interface IDownloadCandidateDocument { Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid documentId); }
    public interface IDeleteCandidateDocument { Task DeleteAsync(Guid documentId); }
    public interface IHireCandidate { Task<Guid> HireAsync(HireCandidateDto dto); }

    // ---- Candidate documents (compliance + credentials) --------------------------

    public class UploadCandidateDocument(
        IRepository<CandidateDocument> repository,
        IRepository<Candidate> candidateRepository,
        ILogger<UploadCandidateDocument> logger) : IUploadCandidateDocument
    {
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public async Task<Guid> UploadAsync(
            Guid candidateId, string documentType, Stream content, string fileName, string contentType, long length)
        {
            if (!Enum.TryParse<CandidateDocumentType>(documentType, true, out var type))
                throw new ValidationException("documentType", "Unknown candidate document type.");

            var candidate = await candidateRepository.GetAll().FirstOrDefaultAsync(c => c.Id == candidateId)
                ?? throw new NotFoundException(nameof(Candidate), candidateId.ToString());
            if (candidate.AnonymizedAt.HasValue)
                throw new ValidationException("document", "An anonymized candidate record can no longer change.");
            if (length <= 0 || length > MaxBytes)
                throw new ValidationException("document", "The document must be between 1 byte and 5 MB.");

            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer);

            var document = CandidateDocument.Create(candidateId, type, fileName,
                contentType, buffer.ToArray());
            await repository.AddAsync(document);
            await repository.SaveChangesAsync();
            logger.LogInformation("Stored {Type} document for Candidate {Id} ({File})", type, candidateId, fileName);
            return document.Id;
        }
    }

    public class GetCandidateDocuments(IRepository<CandidateDocument> repository) : IGetCandidateDocuments
    {
        public async Task<List<CandidateDocumentDto>> GetAsync(Guid candidateId) =>
            await repository.GetAll()
                .Where(d => d.CandidateId == candidateId)
                .OrderBy(d => d.DocumentType)
                .Select(d => new CandidateDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType.ToString(),
                    FileName = d.FileName,
                    FileSize = d.FileSize,
                    UploadedAt = d.CreatedAt.ToDateTimeUtc()
                })
                .ToListAsync();
    }

    public class DownloadCandidateDocument(IRepository<CandidateDocument> repository) : IDownloadCandidateDocument
    {
        public async Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid documentId)
        {
            var document = await repository.GetAll().FirstOrDefaultAsync(d => d.Id == documentId)
                ?? throw new NotFoundException(nameof(CandidateDocument), documentId.ToString());
            return (document.Content, document.ContentType, document.FileName);
        }
    }

    public class DeleteCandidateDocument(
        IRepository<CandidateDocument> repository,
        ILogger<DeleteCandidateDocument> logger) : IDeleteCandidateDocument
    {
        public async Task DeleteAsync(Guid documentId)
        {
            var document = await repository.GetAll().FirstOrDefaultAsync(d => d.Id == documentId)
                ?? throw new NotFoundException(nameof(CandidateDocument), documentId.ToString());
            repository.Delete(document);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted candidate document {Id}", documentId);
        }
    }

    // ---- Hire: candidate → employee on the SAME person, documents migrated ----------------

    /// <summary>
    /// Converts a selected candidate into a placement with no data re-entry, taking one of two paths:
    /// <list type="bullet">
    /// <item><b>EXTERNAL</b> applicant → a NEW employee on the candidate's existing CorePerson record;
    /// candidate documents (plus the resume) migrate to the employee's permanent history; the seat is
    /// marked occupied (HC115–HC117). Blocked until the mandatory compliance documents are complete.</item>
    /// <item><b>INTERNAL</b> applicant (an existing employee who applied) → NO second employee record.
    /// The placement is recorded as a Promotion / Transfer / Demotion against their existing employee
    /// via the movement module, which approves it, swaps the seat and logs internal tenure — preserving
    /// leave balances, appraisals, movement history, loans and documents.</item>
    /// </list>
    /// Either way the winning application moves to Hired and the vacancy closes out.
    /// </summary>
    public class HireCandidate(
        IRepository<Candidate> candidateRepository,
        IRepository<CandidateDocument> candidateDocumentRepository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobApplicationStageLog> stageLogRepository,
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeDocument> employeeDocumentRepository,
        IRepository<Position> positionRepository,
        IRepository<SalaryScale> salaryScaleRepository,
        IRepository<JobOffer> offerRepository,
        IRepository<JobRequisition> requisitionRepository,
        IGetApplicationRanking rankingHandler,
        ISaveEmployeeMovement saveMovement,
        IApproveEmployeeMovement approveMovement,
        IWorkflowGate workflowGate,
        IConfiguration configuration,
        ICurrentUserService currentUser,
        IValidator<HireCandidateDto> validator,
        ILogger<HireCandidate> logger) : IHireCandidate
    {
        public async Task<Guid> HireAsync(HireCandidateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var candidate = await candidateRepository.GetAll().FirstOrDefaultAsync(c => c.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Candidate), dto.Id.ToString());
            if (candidate.HiredEmployeeId.HasValue)
                throw new ValidationException("id", "This candidate has already been hired.");
            if (candidate.AnonymizedAt.HasValue)
                throw new ValidationException("id", "An anonymized candidate cannot be hired.");
            if (!candidate.PersonId.HasValue)
                throw new ValidationException("id",
                    "The candidate has no person record yet — save the candidate once to establish it, then hire.");

            // An INTERNAL applicant is an existing employee: hiring must NEVER create a second employee
            // record on their person — it routes to a promotion/transfer instead (see PlaceInternalAsync).
            var isInternal = candidate.InternalEmployeeId.HasValue;

            // Mandatory documentation gate (ID, guarantor form, medical certificate, signed offer/contract)
            // applies to NEW hires only — an internal employee already holds these from their engagement.
            if (!isInternal)
            {
                var presentTypes = await candidateDocumentRepository.GetAll()
                    .Where(d => d.CandidateId == dto.Id)
                    .Select(d => d.DocumentType)
                    .ToListAsync();
                var missing = CandidateShared.MissingComplianceDocuments(presentTypes);
                if (missing.Count > 0)
                    throw new ValidationException("documents",
                        $"Mandatory compliance documents are missing: {string.Join(", ", missing)}.");
            }

            // The hire executes a SELECTED application — or one at OfferPending / OfferAccepted when
            // the offer process drives it (Phase 2, HC111–HC114).
            var application = await applicationRepository.GetAll()
                    .Include(a => a.StageLog)
                    .Where(a => a.CandidateId == dto.Id &&
                        (a.Stage == ApplicationStage.Selected || a.Stage == ApplicationStage.OfferPending ||
                         a.Stage == ApplicationStage.OfferAccepted))
                    .OrderByDescending(a => a.AppliedAt)
                    .FirstOrDefaultAsync()
                ?? throw new ValidationException("id",
                    "The candidate has no application at the Selected stage — complete the selection first.");

            // Offer gate (HC114): once the formal offer process is used, hire requires acceptance.
            var latestOffer = await offerRepository.GetAll()
                .Where(o => o.ApplicationId == application.Id)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
            if (latestOffer is not null && latestOffer.Status != OfferStatus.Accepted)
                throw new ValidationException("id",
                    $"The latest offer ({latestOffer.OfferNumber}) is {latestOffer.Status} — an ACCEPTED offer is required to hire.");

            // Ranking gate: when the vacancy is scored against weighted criteria, only the top-N
            // eligible candidates can be hired (N = open positions); the rest are waitlisted and
            // slide up only when a higher-ranked candidate drops out (offer declined/expired).
            var rankingRows = await rankingHandler.GetAsync(application.RequisitionId);
            var myRow = rankingRows.FirstOrDefault(r => r.ApplicationId == application.Id);
            if (myRow is not null && myRow.TotalCriteria > 0 && myRow.HireEligibility != "Eligible")
                throw new ValidationException("id", myRow.HireEligibility switch
                {
                    "Waitlisted" =>
                        $"The candidate is ranked #{myRow.Rank} and WAITLISTED — only the top {rankingRows.Count(r => r.HireEligibility == "Eligible")} eligible candidate(s) can be hired for this vacancy.",
                    "NotScored" =>
                        "The candidate has not been scored against the vacancy's weighted criteria — complete the evaluation first.",
                    "FailsMandatory" =>
                        "The candidate failed a mandatory criterion and cannot be hired for this vacancy.",
                    "OfferRejected" =>
                        "The candidate rejected the offer for this vacancy and is out of contention.",
                    _ => $"The candidate is not hire-eligible for this vacancy ({myRow.HireEligibility})."
                });

            // Auto-populate placement so HR need not re-key Position/Salary: pay terms come from the
            // OFFER (the agreed figure) then the REQUISITION (the vacancy's scale); the Position is a
            // still-vacant slot of the requisition's role, preferring its own unit. A DTO value wins.
            var vacancy = await requisitionRepository.GetAll()
                .Where(q => q.Id == application.RequisitionId)
                .Select(q => new { q.PositionClassId, q.OrganizationUnitId, q.SalaryScaleId })
                .FirstOrDefaultAsync();

            var salaryScaleId = dto.SalaryScaleId ?? latestOffer?.SalaryScaleId ?? vacancy?.SalaryScaleId;

            var positionId = dto.PositionId;
            if (!positionId.HasValue && vacancy is not null)
                positionId = await positionRepository.GetAll()
                    .Where(p => p.PositionClassId == vacancy.PositionClassId && p.IsVacant)
                    .OrderByDescending(p => p.OrganizationUnitId == vacancy.OrganizationUnitId ? 1 : 0)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync();

            Guid? branchId = null;
            if (positionId.HasValue)
            {
                var position = await positionRepository.GetAll().FirstOrDefaultAsync(p => p.Id == positionId.Value)
                    ?? throw new NotFoundException(nameof(Position), positionId.Value.ToString());
                if (!position.IsVacant)
                    throw new ValidationException("positionId", "The selected position is no longer vacant.");
                branchId = position.BranchId;
            }

            var salary = dto.Salary ?? latestOffer?.Salary;
            if (!salary.HasValue && salaryScaleId.HasValue)
                salary = await salaryScaleRepository.GetAll()
                    .Where(s => s.Id == salaryScaleId.Value)
                    .Select(s => (decimal?)s.Salary)
                    .FirstOrDefaultAsync();

            var requisition = await requisitionRepository.GetAll()
                .FirstOrDefaultAsync(q => q.Id == application.RequisitionId);

            return isInternal
                ? await PlaceInternalAsync(candidate, application, latestOffer, requisition, dto, positionId, salaryScaleId, salary)
                : await HireExternalAsync(candidate, application, latestOffer, requisition, dto, positionId, salaryScaleId, branchId, salary);
        }

        // ---- EXTERNAL: a brand-new employee on the candidate's shared person ---------------------
        private async Task<Guid> HireExternalAsync(
            Candidate candidate, JobApplication application, JobOffer? latestOffer, JobRequisition? requisition,
            HireCandidateDto dto, Guid? positionId, Guid? salaryScaleId, Guid? branchId, decimal? salary)
        {
            if (string.IsNullOrWhiteSpace(dto.EmployeeNumber))
                throw new ValidationException("employeeNumber", "An employee number is required for a new hire.");
            if (await employeeRepository.GetAll().AnyAsync(e => e.EmployeeNumber == dto.EmployeeNumber))
                throw new DuplicateException(nameof(Employee), nameof(dto.EmployeeNumber), dto.EmployeeNumber);

            // Safety net: never mint a SECOND employee on a person who is already actively employed —
            // that would strand their leave, appraisal, movement, loan and payroll history on the old
            // row. Such a person must move via a promotion/transfer, not a fresh hire.
            if (await employeeRepository.GetAll().AnyAsync(e => e.PersonId == candidate.PersonId!.Value &&
                    e.EmploymentStatus != EmploymentStatus.Terminated && e.EmploymentStatus != EmploymentStatus.Retired))
                throw new ValidationException("id",
                    "This person is already an active employee — record an internal move (promotion or transfer) from Employee Movements instead of a new hire.");

            var nature = Enum.Parse<EmploymentNature>(dto.EmploymentNature, true);

            // Employee on the candidate's EXISTING person — zero re-entry.
            var employee = Employee.Create(
                candidate.PersonId!.Value,
                dto.EmployeeNumber,
                dto.IsProbation ? EmploymentStatus.Probation : EmploymentStatus.Active,
                email: candidate.Email,
                hireDate: dto.HireDate ?? DateTime.UtcNow.Date,
                positionId: positionId,
                salary: salary,
                branchId: branchId,
                employmentNature: nature,
                contractPeriod: dto.ContractPeriod,
                isProbation: dto.IsProbation,
                probationEndDate: dto.ProbationEndDate,
                salaryScaleId: salaryScaleId);
            await employeeRepository.AddAsync(employee);

            await MigrateCandidateDocumentsAsync(candidate, employee.Id);

            // Education/experience attachments were anchored to the candidate id while the person was
            // only a candidate — re-anchor them to the new employee so the employee-side document
            // guards recognize them (the rows themselves follow the shared person).
            var backgroundDocs = await employeeDocumentRepository.GetAll()
                .Where(d => d.EmployeeId == candidate.Id &&
                            (d.OwnerType == EmployeeDocumentOwner.Education || d.OwnerType == EmployeeDocumentOwner.Experience))
                .ToListAsync();
            foreach (var d in backgroundDocs)
            {
                d.AssignEmployee(employee.Id);
                employeeDocumentRepository.UpdateAsync(d);
            }

            await CloseRecruitmentPipelineAsync(application, candidate, latestOffer, requisition, employee.Id,
                $"Hired as employee {dto.EmployeeNumber}");

            // The assigned seat is now occupied (the resolved position, auto-picked or supplied).
            await EmployeeShared.MarkPositionOccupiedAsync(positionId, positionRepository);

            await candidateRepository.SaveChangesAsync();   // one transaction end-to-end
            logger.LogInformation("Hired Candidate {CandidateId} as new Employee {EmployeeId} ({Number})",
                candidate.Id, employee.Id, dto.EmployeeNumber);
            return employee.Id;
        }

        // ---- INTERNAL: a promotion/transfer against the existing employee (no new record) --------
        private async Task<Guid> PlaceInternalAsync(
            Candidate candidate, JobApplication application, JobOffer? latestOffer, JobRequisition? requisition,
            HireCandidateDto dto, Guid? positionId, Guid? salaryScaleId, decimal? salary)
        {
            var employeeId = candidate.InternalEmployeeId!.Value;
            var employee = await employeeRepository.GetAll()
                .Where(e => e.Id == employeeId)
                .Select(e => new { e.Salary, e.EmploymentStatus, e.PositionId })
                .FirstOrDefaultAsync()
                ?? throw new ValidationException("id", "The linked internal employee record no longer exists.");
            if (employee.EmploymentStatus is EmploymentStatus.Terminated or EmploymentStatus.Retired)
                throw new ValidationException("id",
                    "The linked internal employee is no longer active — record a fresh hire instead.");
            if (!positionId.HasValue)
                throw new ValidationException("positionId",
                    "No vacant target position could be resolved for this vacancy — choose one before placing the internal employee.");
            if (positionId == employee.PositionId)
                throw new ValidationException("positionId", "The employee already holds the target position.");

            // Movement kind: an explicit choice wins; otherwise derive it from the pay change agreed in
            // the offer — a higher salary is a Promotion, a lower one a Demotion, an equal one a Transfer.
            var newSalary = salary ?? employee.Salary;
            MovementType type;
            if (!string.IsNullOrWhiteSpace(dto.MovementType))
                type = Enum.Parse<MovementType>(dto.MovementType, true);
            else if (employee.Salary.HasValue && newSalary.HasValue && newSalary.Value > employee.Salary.Value)
                type = MovementType.Promotion;
            else if (employee.Salary.HasValue && newSalary.HasValue && newSalary.Value < employee.Salary.Value)
                type = MovementType.Demotion;
            else
                type = MovementType.Transfer;

            var isTransfer = type == MovementType.Transfer;

            // Route through the movement module: it re-checks the target seat, runs the movement approval
            // chain (EmployeeMovement.{type}), and on execution swaps the position/pay and logs internal
            // tenure. Pay may only change on a promotion/demotion — a transfer leaves compensation intact.
            var movementId = await saveMovement.SaveAsync(new SaveEmployeeMovementDto
            {
                EmployeeId = employeeId,
                MovementType = type.ToString(),
                EffectiveDate = dto.HireDate ?? DateTime.UtcNow.Date,
                ToPositionId = positionId,
                ToSalaryScaleId = isTransfer ? null : salaryScaleId,
                ToSalary = isTransfer ? null : salary,
                Reason = $"Internal placement via recruitment — vacancy {requisition?.RequisitionNumber ?? application.RequisitionId.ToString()}",
                TransferKind = isTransfer ? nameof(Dom.Entities.Core.TransferKind.Role) : null,
            });

            // When no movement approval chain is configured, apply it now (or park it until the effective
            // date) exactly as HR would from the movement screen — mirrors the offer/requisition idiom.
            if (!await workflowGate.HasRunningAsync("EmployeeMovement", movementId))
                await approveMovement.ApproveAsync(movementId);

            // Candidate documents (and resume) still migrate onto the EXISTING employee's history.
            await MigrateCandidateDocumentsAsync(candidate, employeeId);
            await CloseRecruitmentPipelineAsync(application, candidate, latestOffer, requisition, employeeId,
                $"Internal {type} recorded (movement {movementId})");

            await candidateRepository.SaveChangesAsync();
            logger.LogInformation(
                "Internal Candidate {CandidateId} placed via EmployeeMovement {MovementId} ({Type}) on existing Employee {EmployeeId}",
                candidate.Id, movementId, type, employeeId);
            return employeeId;
        }

        /// <summary>Migrates every candidate document and the resume file onto an employee's permanent history.</summary>
        private async Task MigrateCandidateDocumentsAsync(Candidate candidate, Guid employeeId)
        {
            var documents = await candidateDocumentRepository.GetAll()
                .Where(d => d.CandidateId == candidate.Id)
                .ToListAsync();
            foreach (var d in documents)
                await employeeDocumentRepository.AddAsync(EmployeeDocument.Create(
                    employeeId, EmployeeDocumentOwner.Recruitment, employeeId,
                    d.FileName, d.ContentType, d.Content));

            if (!string.IsNullOrEmpty(candidate.ResumeFileName))
            {
                var resumePath = Path.Combine(ResumeStorage.ResolveRoot(configuration), candidate.ResumeFileName);
                if (File.Exists(resumePath))
                    await employeeDocumentRepository.AddAsync(EmployeeDocument.Create(
                        employeeId, EmployeeDocumentOwner.Recruitment, employeeId,
                        $"Resume-{candidate.CandidateNumber}{Path.GetExtension(resumePath)}",
                        ResumeStorage.AllowedTypes.GetValueOrDefault(Path.GetExtension(resumePath), "application/octet-stream"),
                        await File.ReadAllBytesAsync(resumePath)));
            }
        }

        /// <summary>
        /// Closes the recruitment pipeline once a candidate is placed (as a new hire or an internal move):
        /// the winning application → Hired (logged), the candidate + accepted offer link the resulting
        /// employee, the vacancy auto-closes when its last seat is filled (dispositioning the rest), and
        /// the candidate's other active applications are withdrawn.
        /// </summary>
        private async Task CloseRecruitmentPipelineAsync(
            JobApplication application, Candidate candidate, JobOffer? latestOffer,
            JobRequisition? requisition, Guid resolvedEmployeeId, string hiredNote)
        {
            var before = application.StageLog.Select(l => l.Id).ToHashSet();
            application.MoveToStage(ApplicationStage.Hired, hiredNote, currentUser.GetCurrentUserName());
            foreach (var log in application.StageLog.Where(l => !before.Contains(l.Id)))
            {
                if (string.IsNullOrEmpty(log.TenantId))
                    log.TenantId = application.TenantId;
                await stageLogRepository.AddAsync(log);
            }
            applicationRepository.UpdateAsync(application);

            candidate.MarkHired(resolvedEmployeeId);
            candidateRepository.UpdateAsync(candidate);

            // The accepted offer records the employee its acceptance became (HC114 handoff).
            if (latestOffer is not null && latestOffer.Status == OfferStatus.Accepted)
            {
                latestOffer.AssignHiredEmployee(resolvedEmployeeId);
                offerRepository.UpdateAsync(latestOffer);
            }

            // Vacancy fill tracking: when this placement fills the LAST open position, the requisition
            // auto-closes and the remaining active applicants are dispositioned (logged).
            if (requisition is not null)
            {
                var hiredCount = await applicationRepository.GetAll()
                    .CountAsync(a => a.RequisitionId == requisition.Id && a.Stage == ApplicationStage.Hired) + 1;
                if (hiredCount >= requisition.NumberOfPositions &&
                    requisition.Status is RequisitionStatus.Approved or RequisitionStatus.Posted)
                {
                    requisition.Close();
                    requisitionRepository.UpdateAsync(requisition);
                    var remaining = await applicationRepository.GetAll()
                        .Include(a => a.StageLog)
                        .Where(a => a.RequisitionId == requisition.Id && a.Id != application.Id)
                        .ToListAsync();
                    await PipelineDisposition.CloseOutAsync(applicationRepository, stageLogRepository,
                        offerRepository, remaining, ApplicationStage.Rejected,
                        $"Position filled — vacancy {requisition.RequisitionNumber} closed",
                        currentUser.GetCurrentUserName());
                }
            }

            // The placed person's OTHER active applications (other vacancies) are withdrawn.
            var otherApplications = await applicationRepository.GetAll()
                .Include(a => a.StageLog)
                .Where(a => a.CandidateId == candidate.Id && a.Id != application.Id)
                .ToListAsync();
            await PipelineDisposition.CloseOutAsync(applicationRepository, stageLogRepository,
                offerRepository, otherApplications, ApplicationStage.Withdrawn,
                $"Placed on vacancy {requisition?.RequisitionNumber ?? application.RequisitionId.ToString()}",
                currentUser.GetCurrentUserName());
        }
    }
}
