using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Employees;
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
    /// Converts a hired candidate into an employee with no data re-entry: the employee is created on
    /// the candidate's EXISTING CorePerson record; every candidate document (plus the resume file) is
    /// migrated onto the employee's permanent history (EmployeeDocument, owner Recruitment); the
    /// winning application moves to Hired; probation tracking starts when requested (HC115–HC117).
    /// Hire is BLOCKED until the mandatory compliance documents are complete.
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

            // Mandatory documentation gate: ID, guarantor form, medical certificate, signed offer/contract.
            var presentTypes = await candidateDocumentRepository.GetAll()
                .Where(d => d.CandidateId == dto.Id)
                .Select(d => d.DocumentType)
                .ToListAsync();
            var missing = CandidateShared.MissingComplianceDocuments(presentTypes);
            if (missing.Count > 0)
                throw new ValidationException("documents",
                    $"Mandatory compliance documents are missing: {string.Join(", ", missing)}.");

            // The hire executes a SELECTED application — or one at OfferPending when the offer
            // process drives it (Phase 2, HC111–HC114).
            var application = await applicationRepository.GetAll()
                    .Include(a => a.StageLog)
                    .Where(a => a.CandidateId == dto.Id &&
                        (a.Stage == ApplicationStage.Selected || a.Stage == ApplicationStage.OfferPending))
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

            if (await employeeRepository.GetAll().AnyAsync(e => e.EmployeeNumber == dto.EmployeeNumber))
                throw new DuplicateException(nameof(Employee), nameof(dto.EmployeeNumber), dto.EmployeeNumber);

            // Placement (optional at hire; onboarding may assign later).
            Guid? branchId = null;
            if (dto.PositionId.HasValue)
            {
                var position = await positionRepository.GetAll().FirstOrDefaultAsync(p => p.Id == dto.PositionId.Value)
                    ?? throw new NotFoundException(nameof(Position), dto.PositionId.Value.ToString());
                if (!position.IsVacant)
                    throw new ValidationException("positionId", "The selected position is no longer vacant.");
                branchId = position.BranchId;
            }
            var salary = dto.Salary;
            if (!salary.HasValue && dto.SalaryScaleId.HasValue)
                salary = await salaryScaleRepository.GetAll()
                    .Where(s => s.Id == dto.SalaryScaleId.Value)
                    .Select(s => (decimal?)s.Salary)
                    .FirstOrDefaultAsync();

            var nature = Enum.Parse<EmploymentNature>(dto.EmploymentNature, true);

            // Employee on the candidate's EXISTING person — zero re-entry (requirement #2).
            var employee = Employee.Create(
                candidate.PersonId.Value,
                dto.EmployeeNumber,
                dto.IsProbation ? EmploymentStatus.Probation : EmploymentStatus.Active,
                email: candidate.Email,
                hireDate: dto.HireDate ?? DateTime.UtcNow.Date,
                positionId: dto.PositionId,
                salary: salary,
                branchId: branchId,
                employmentNature: nature,
                contractPeriod: dto.ContractPeriod,
                isProbation: dto.IsProbation,
                probationEndDate: dto.ProbationEndDate,
                salaryScaleId: dto.SalaryScaleId);
            await employeeRepository.AddAsync(employee);

            // Automated document migration (requirement #3): every candidate document lands on the
            // employee's permanent history (owner Recruitment, ownerId = the employee).
            var documents = await candidateDocumentRepository.GetAll()
                .Where(d => d.CandidateId == dto.Id)
                .ToListAsync();
            foreach (var d in documents)
                await employeeDocumentRepository.AddAsync(EmployeeDocument.Create(
                    employee.Id, EmployeeDocumentOwner.Recruitment, employee.Id,
                    d.FileName, d.ContentType, d.Content));

            // Education/experience attachments were anchored to the candidate id while the person
            // was only a candidate — re-anchor them to the new employee so the employee-side
            // document guards recognize them (the rows themselves follow the shared person).
            var backgroundDocs = await employeeDocumentRepository.GetAll()
                .Where(d => d.EmployeeId == dto.Id &&
                            (d.OwnerType == EmployeeDocumentOwner.Education || d.OwnerType == EmployeeDocumentOwner.Experience))
                .ToListAsync();
            foreach (var d in backgroundDocs)
            {
                d.AssignEmployee(employee.Id);
                employeeDocumentRepository.UpdateAsync(d);
            }

            // The resume file (stored on disk) migrates too.
            if (!string.IsNullOrEmpty(candidate.ResumeFileName))
            {
                var resumePath = Path.Combine(ResumeStorage.ResolveRoot(configuration), candidate.ResumeFileName);
                if (File.Exists(resumePath))
                    await employeeDocumentRepository.AddAsync(EmployeeDocument.Create(
                        employee.Id, EmployeeDocumentOwner.Recruitment, employee.Id,
                        $"Resume-{candidate.CandidateNumber}{Path.GetExtension(resumePath)}",
                        ResumeStorage.AllowedTypes.GetValueOrDefault(Path.GetExtension(resumePath), "application/octet-stream"),
                        await File.ReadAllBytesAsync(resumePath)));
            }

            // Pipeline closure: application → Hired (logged), candidate linked + archived.
            var before = application.StageLog.Select(l => l.Id).ToHashSet();
            application.MoveToStage(ApplicationStage.Hired,
                $"Hired as employee {dto.EmployeeNumber}", currentUser.GetCurrentUserName());
            foreach (var log in application.StageLog.Where(l => !before.Contains(l.Id)))
            {
                if (string.IsNullOrEmpty(log.TenantId))
                    log.TenantId = application.TenantId;
                await stageLogRepository.AddAsync(log);
            }
            applicationRepository.UpdateAsync(application);

            candidate.MarkHired(employee.Id);
            candidateRepository.UpdateAsync(candidate);

            // The accepted offer records the employee its acceptance became (HC114 handoff).
            if (latestOffer is not null && latestOffer.Status == OfferStatus.Accepted)
            {
                latestOffer.AssignHiredEmployee(employee.Id);
                offerRepository.UpdateAsync(latestOffer);
            }

            // Vacancy fill tracking: when this hire fills the LAST open position, the requisition
            // auto-closes and the remaining active applicants are dispositioned (logged) — no
            // vacancy stays open with a pipeline nobody can hire from.
            var requisition = await requisitionRepository.GetAll()
                .FirstOrDefaultAsync(q => q.Id == application.RequisitionId);
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

            // The new employee's OTHER active applications (other vacancies) are withdrawn —
            // nobody keeps interviewing someone who has already joined.
            var otherApplications = await applicationRepository.GetAll()
                .Include(a => a.StageLog)
                .Where(a => a.CandidateId == candidate.Id && a.Id != application.Id)
                .ToListAsync();
            await PipelineDisposition.CloseOutAsync(applicationRepository, stageLogRepository,
                offerRepository, otherApplications, ApplicationStage.Withdrawn,
                $"Hired on vacancy {requisition?.RequisitionNumber ?? application.RequisitionId.ToString()}",
                currentUser.GetCurrentUserName());

            // The assigned seat is now occupied.
            await EmployeeShared.MarkPositionOccupiedAsync(dto.PositionId, positionRepository);

            await candidateRepository.SaveChangesAsync();   // one transaction end-to-end
            logger.LogInformation(
                "Hired Candidate {CandidateId} as Employee {EmployeeId} ({Number}); migrated {Docs} document(s)",
                dto.Id, employee.Id, dto.EmployeeNumber, documents.Count);
            return employee.Id;
        }
    }
}
