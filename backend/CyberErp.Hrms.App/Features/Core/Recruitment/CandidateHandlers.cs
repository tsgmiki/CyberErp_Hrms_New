using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- Interfaces -----------------------------------------------------------

    public interface ISaveCandidate { Task<Guid> SaveAsync(SaveCandidateDto dto); }
    public interface IGetCandidateById { Task<CandidateDto> GetAsync(Guid id); }
    public interface IGetAllCandidates { Task<PaginatedResponse<CandidateDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteCandidate { Task DeleteAsync(Guid id); }
    public interface ISetCandidateTalentPool { Task SetAsync(SetTalentPoolDto dto); }
    public interface IAnonymizeCandidate { Task AnonymizeAsync(Guid id); }
    public interface IUploadCandidateResume { Task UploadAsync(Guid candidateId, Stream content, string fileName, long length); }
    public interface IGetCandidateResume { Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid candidateId); }
    public interface IMatchCandidates { Task<List<CandidateMatchDto>> GetAsync(Guid requisitionId); }

    internal static class ResumeStorage
    {
        internal const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        internal static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".doc"] = "application/msword",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        };

        internal static string ResolveRoot(IConfiguration configuration) =>
            configuration["Storage:CandidateResumePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "candidate-resumes");
    }

    internal static class CandidateShared
    {
        /// <summary>
        /// The mandatory compliance set gating hire: ID, guarantor form, medical certificate, and a
        /// signed offer letter OR employment contract (either satisfies the contractual requirement).
        /// </summary>
        internal static List<string> MissingComplianceDocuments(IEnumerable<CandidateDocumentType> present)
        {
            var set = present.ToHashSet();
            var missing = new List<string>();
            if (!set.Contains(CandidateDocumentType.NationalId)) missing.Add("National ID");
            if (!set.Contains(CandidateDocumentType.GuarantorForm)) missing.Add("Guarantor Form");
            if (!set.Contains(CandidateDocumentType.MedicalCertificate)) missing.Add("Medical Certificate");
            if (!set.Contains(CandidateDocumentType.SignedOfferLetter) && !set.Contains(CandidateDocumentType.EmploymentContract))
                missing.Add("Signed Offer Letter or Employment Contract");
            return missing;
        }

        internal static CandidateDto ToDto(Candidate c, string? internalEmployeeName, int applicationCount) => new()
        {
            Id = c.Id,
            CandidateNumber = c.CandidateNumber,
            FirstName = c.FirstName,
            FatherName = c.FatherName,
            GrandFatherName = c.GrandFatherName,
            FullName = c.FullName,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Gender = c.Gender?.ToString(),
            Source = c.Source.ToString(),
            InternalEmployeeId = c.InternalEmployeeId,
            InternalEmployeeName = internalEmployeeName,
            EducationSummary = c.EducationSummary,
            ExperienceSummary = c.ExperienceSummary,
            SkillsSummary = c.SkillsSummary,
            YearsOfExperience = c.YearsOfExperience,
            ResumeFileName = c.ResumeFileName,
            ConsentGiven = c.ConsentGiven,
            ConsentAt = c.ConsentAt,
            IsArchived = c.IsArchived,
            AnonymizedAt = c.AnonymizedAt,
            IsInTalentPool = c.IsInTalentPool,
            TalentPoolNotes = c.TalentPoolNotes,
            ApplicationCount = applicationCount,
            PersonId = c.PersonId,
            HiredEmployeeId = c.HiredEmployeeId
        };
    }

    // ---- Save -------------------------------------------------------------------

    public class SaveCandidate(
        IRepository<Candidate> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Person> personRepository,
        INumberSequenceService numberSequence,
        IValidator<SaveCandidateDto> validator,
        ILogger<SaveCandidate> logger) : ISaveCandidate
    {
        public async Task<Guid> SaveAsync(SaveCandidateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var source = Enum.Parse<CandidateSource>(dto.Source, true);
            var gender = Enum.Parse<Gender>(dto.Gender!, true);

            // An internal candidate must reference a real employee (HC090) — and reuses that
            // employee's person record rather than creating a duplicate identity.
            if (source == CandidateSource.Internal && !dto.InternalEmployeeId.HasValue)
                throw new ValidationException("internalEmployeeId", "An internal candidate must be linked to an employee.");
            Guid? internalPersonId = null;
            if (dto.InternalEmployeeId.HasValue)
            {
                internalPersonId = await employeeRepository.GetAll()
                    .Where(e => e.Id == dto.InternalEmployeeId.Value)
                    .Select(e => (Guid?)e.PersonId)
                    .FirstOrDefaultAsync();
                if (internalPersonId is null)
                    throw new NotFoundException(nameof(Employee), dto.InternalEmployeeId.Value.ToString());
            }

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Candidate), dto.Id.Value.ToString());
                if (entity.AnonymizedAt.HasValue)
                    throw new ValidationException("id", "An anonymized candidate record can no longer change.");

                entity.Update(dto.FirstName, source, dto.FatherName, dto.GrandFatherName, dto.Email,
                    dto.PhoneNumber, gender, dto.InternalEmployeeId, dto.EducationSummary,
                    dto.ExperienceSummary, dto.SkillsSummary, dto.YearsOfExperience);

                // Keep the shared person record in sync (or backfill it for pre-feature candidates)
                // — the SAME person becomes the employee's identity at hire, with no re-entry.
                if (entity.PersonId.HasValue)
                {
                    // An internal candidate's person belongs to the employee master — leave it to HR.
                    if (source != CandidateSource.Internal)
                    {
                        var person = await personRepository.GetAll().FirstOrDefaultAsync(p => p.Id == entity.PersonId.Value);
                        person?.Update(dto.FirstName, dto.FatherName, dto.GrandFatherName!, gender,
                            person.MaritalStatusId, null, null, null, null, dto.PhoneNumber, null);
                        if (person is not null) personRepository.UpdateAsync(person);
                    }
                }
                else if (internalPersonId.HasValue)
                {
                    entity.SetPerson(internalPersonId.Value);
                }
                else
                {
                    var person = Person.Create(dto.FirstName, dto.GrandFatherName!, gender,
                        MaritalStatus.Single, dto.FatherName, phoneNumber: dto.PhoneNumber);
                    await personRepository.AddAsync(person);
                    entity.SetPerson(person.Id);
                }

                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated Candidate {Id}", entity.Id);
                return entity.Id;
            }

            var number = await RecruitmentShared.NextNumberAsync(numberSequence, "Candidate", "CND");
            var created = Candidate.Create(number, dto.FirstName, source, dto.ConsentGiven,
                dto.FatherName, dto.GrandFatherName, dto.Email, dto.PhoneNumber, gender,
                dto.InternalEmployeeId, dto.EducationSummary, dto.ExperienceSummary,
                dto.SkillsSummary, dto.YearsOfExperience);

            // Candidate + person in ONE SaveChanges (one transaction) — the person record is the
            // hire-conversion anchor the employee will reuse verbatim. Internal candidates reuse
            // their employee's existing person.
            if (internalPersonId.HasValue)
            {
                created.SetPerson(internalPersonId.Value);
            }
            else
            {
                var newPerson = Person.Create(dto.FirstName, dto.GrandFatherName!, gender,
                    MaritalStatus.Single, dto.FatherName, phoneNumber: dto.PhoneNumber);
                await personRepository.AddAsync(newPerson);
                created.SetPerson(newPerson.Id);
            }

            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Candidate {Id} ({Number}, {Source}) with Person {PersonId}",
                created.Id, number, source, created.PersonId);
            return created.Id;
        }
    }

    // ---- Get / list -----------------------------------------------------------------

    public class GetCandidateById(
        IRepository<Candidate> repository,
        IRepository<Employee> employeeRepository,
        IRepository<JobApplication> applicationRepository,
        IRepository<CandidateDocument> documentRepository) : IGetCandidateById
    {
        public async Task<CandidateDto> GetAsync(Guid id)
        {
            var c = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Candidate), id.ToString());

            var employeeName = c.InternalEmployeeId.HasValue
                ? await employeeRepository.GetAll()
                    .Where(e => e.Id == c.InternalEmployeeId.Value)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefaultAsync()
                : null;
            var applications = await applicationRepository.GetAll().CountAsync(a => a.CandidateId == id);

            var dto = CandidateShared.ToDto(c, employeeName, applications);
            // Mandatory documentation status (gates the hire conversion).
            var presentTypes = await documentRepository.GetAll()
                .Where(d => d.CandidateId == id)
                .Select(d => d.DocumentType)
                .ToListAsync();
            dto.MissingComplianceDocuments = CandidateShared.MissingComplianceDocuments(presentTypes);
            dto.ComplianceComplete = dto.MissingComplianceDocuments.Count == 0;
            return dto;
        }
    }

    public class GetAllCandidates(
        IRepository<Candidate> repository,
        IRepository<JobApplication> applicationRepository) : IGetAllCandidates
    {
        public async Task<PaginatedResponse<CandidateDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            // Archived/anonymized records stay out of the working list unless asked for.
            if (string.Equals(request.Status, "Archived", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.IsArchived);
            else if (string.Equals(request.Status, "TalentPool", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.IsInTalentPool && !c.IsArchived);
            else if (Enum.TryParse<CandidateSource>(request.Status, true, out var source))
                query = query.Where(c => c.Source == source && !c.IsArchived);
            else
                query = query.Where(c => !c.IsArchived);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(c => c.CandidateNumber.Contains(term) ||
                    c.FirstName.Contains(term) ||
                    (c.FatherName != null && c.FatherName.Contains(term)) ||
                    (c.Email != null && c.Email.Contains(term)) ||
                    (c.SkillsSummary != null && c.SkillsSummary.Contains(term)));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip).Take(take)
                .Select(c => new
                {
                    Candidate = c,
                    ApplicationCount = applicationRepository.GetAll().Count(a => a.CandidateId == c.Id)
                })
                .ToListAsync();

            return new PaginatedResponse<CandidateDto>
            {
                Total = total,
                Data = rows.Select(x => CandidateShared.ToDto(x.Candidate, null, x.ApplicationCount)).ToList()
            };
        }
    }

    public class DeleteCandidate(
        IRepository<Candidate> repository,
        IRepository<JobApplication> applicationRepository,
        IConfiguration configuration,
        ILogger<DeleteCandidate> logger) : IDeleteCandidate
    {
        public async Task DeleteAsync(Guid id)
        {
            var c = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Candidate), id.ToString());
            if (await applicationRepository.GetAll().AnyAsync(a => a.CandidateId == id))
                throw new ValidationException("id",
                    "This candidate has applications — anonymize the record instead of deleting (HC097).");

            if (!string.IsNullOrEmpty(c.ResumeFileName))
            {
                var path = Path.Combine(ResumeStorage.ResolveRoot(configuration), c.ResumeFileName);
                if (File.Exists(path)) File.Delete(path);
            }

            repository.Delete(c);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Candidate {Id}", id);
        }
    }

    // ---- Talent pool (HC089) + retention (HC097) ---------------------------------------

    public class SetCandidateTalentPool(
        IRepository<Candidate> repository,
        ILogger<SetCandidateTalentPool> logger) : ISetCandidateTalentPool
    {
        public async Task SetAsync(SetTalentPoolDto dto)
        {
            var c = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Candidate), dto.Id.ToString());

            c.SetTalentPool(dto.InPool, dto.Notes);
            repository.UpdateAsync(c);
            await repository.SaveChangesAsync();
            logger.LogInformation("Candidate {Id} talent pool = {InPool}", dto.Id, dto.InPool);
        }
    }

    public class AnonymizeCandidate(
        IRepository<Candidate> repository,
        IRepository<JobApplication> applicationRepository,
        IRepository<JobApplicationStageLog> stageLogRepository,
        IRepository<JobOffer> offerRepository,
        IConfiguration configuration,
        ICurrentUserService currentUser,
        ILogger<AnonymizeCandidate> logger) : IAnonymizeCandidate
    {
        public async Task AnonymizeAsync(Guid id)
        {
            var c = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Candidate), id.ToString());
            if (c.AnonymizedAt.HasValue)
                throw new ValidationException("id", "The candidate is already anonymized.");

            // The erasure right ends the person's participation: active applications are withdrawn
            // and live offers pulled BEFORE the scrub — no anonymous ghost stays in the pipeline.
            var applications = await applicationRepository.GetAll()
                .Include(a => a.StageLog)
                .Where(a => a.CandidateId == id)
                .ToListAsync();
            await PipelineDisposition.CloseOutAsync(applicationRepository, stageLogRepository,
                offerRepository, applications, ApplicationStage.Withdrawn,
                "Candidate anonymized (data-erasure request)", currentUser.GetCurrentUserName());

            // The stored resume is personal data — remove the file with the record's PII (HC097).
            if (!string.IsNullOrEmpty(c.ResumeFileName))
            {
                var path = Path.Combine(ResumeStorage.ResolveRoot(configuration), c.ResumeFileName);
                if (File.Exists(path)) File.Delete(path);
            }

            c.Anonymize();
            repository.UpdateAsync(c);
            await repository.SaveChangesAsync();
            logger.LogInformation("Anonymized Candidate {Id} ({Apps} active application(s) withdrawn)",
                id, applications.Count(a => PipelineDisposition.IsActive(a.Stage)));
        }
    }

    // ---- Resume upload / download (HC093) ------------------------------------------------

    public class UploadCandidateResume(
        IRepository<Candidate> repository,
        IConfiguration configuration,
        ILogger<UploadCandidateResume> logger) : IUploadCandidateResume
    {
        public async Task UploadAsync(Guid candidateId, Stream content, string fileName, long length)
        {
            var c = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == candidateId)
                ?? throw new NotFoundException(nameof(Candidate), candidateId.ToString());
            if (c.AnonymizedAt.HasValue)
                throw new ValidationException("resume", "An anonymized candidate record can no longer change.");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !ResumeStorage.AllowedTypes.ContainsKey(ext))
                throw new ValidationException("resume", "Resume must be a PDF, DOC or DOCX file.");
            if (length <= 0 || length > ResumeStorage.MaxBytes)
                throw new ValidationException("resume", "Resume must be between 1 byte and 5 MB.");

            var root = ResumeStorage.ResolveRoot(configuration);
            Directory.CreateDirectory(root);

            if (!string.IsNullOrEmpty(c.ResumeFileName))
            {
                var old = Path.Combine(root, c.ResumeFileName);
                if (File.Exists(old) && !string.Equals(Path.GetExtension(old), ext, StringComparison.OrdinalIgnoreCase))
                    File.Delete(old);
            }

            var storedName = $"{candidateId}{ext.ToLowerInvariant()}";
            var path = Path.Combine(root, storedName);
            await using (var file = File.Create(path))
            {
                await content.CopyToAsync(file);
            }

            c.SetResume(storedName);
            repository.UpdateAsync(c);
            await repository.SaveChangesAsync();
            logger.LogInformation("Stored resume for Candidate {Id} ({File})", candidateId, storedName);
        }
    }

    public class GetCandidateResume(
        IRepository<Candidate> repository,
        IConfiguration configuration) : IGetCandidateResume
    {
        public async Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid candidateId)
        {
            var resume = await repository.GetAll()
                .Where(x => x.Id == candidateId)
                .Select(x => x.ResumeFileName)
                .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(resume))
                throw new NotFoundException("CandidateResume", candidateId.ToString());

            var path = Path.Combine(ResumeStorage.ResolveRoot(configuration), resume);
            if (!File.Exists(path))
                throw new NotFoundException("CandidateResume", candidateId.ToString());

            var contentType = ResumeStorage.AllowedTypes.TryGetValue(Path.GetExtension(path), out var ct)
                ? ct
                : "application/octet-stream";
            return (await File.ReadAllBytesAsync(path), contentType, resume);
        }
    }

    // ---- Skills-based matching for a vacancy (HC090) ---------------------------------------

    public class MatchCandidates(
        IRepository<Candidate> repository,
        IRepository<JobRequisition> requisitionRepository) : IMatchCandidates
    {
        public async Task<List<CandidateMatchDto>> GetAsync(Guid requisitionId)
        {
            var requisition = await requisitionRepository.GetAll()
                    .Where(q => q.Id == requisitionId)
                    .Select(q => new { q.Skills, q.MinExperienceYears })
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(JobRequisition), requisitionId.ToString());

            var requiredSkills = Tokenize(requisition.Skills);

            // Candidate volumes are modest — score in memory (token overlap + experience + pool boost).
            var candidates = await repository.GetAll()
                .Where(c => !c.IsArchived)
                .ToListAsync();

            return candidates
                .Select(c =>
                {
                    var candidateSkills = Tokenize(c.SkillsSummary);
                    var matched = requiredSkills.Intersect(candidateSkills, StringComparer.OrdinalIgnoreCase).ToList();
                    var meetsExperience = !requisition.MinExperienceYears.HasValue
                        || (c.YearsOfExperience ?? 0) >= requisition.MinExperienceYears.Value;
                    var score =
                        (requiredSkills.Count > 0 ? (int)Math.Round(60.0 * matched.Count / requiredSkills.Count) : 0)
                        + (meetsExperience ? 25 : 0)
                        + (c.IsInTalentPool ? 10 : 0)
                        + (c.Source == CandidateSource.Internal ? 5 : 0);
                    return new CandidateMatchDto
                    {
                        CandidateId = c.Id,
                        CandidateNumber = c.CandidateNumber,
                        FullName = c.FullName,
                        Source = c.Source.ToString(),
                        IsInTalentPool = c.IsInTalentPool,
                        YearsOfExperience = c.YearsOfExperience,
                        MatchScore = score,
                        MatchedSkills = matched,
                        MeetsExperience = meetsExperience
                    };
                })
                .Where(m => m.MatchScore > 0)
                .OrderByDescending(m => m.MatchScore)
                .Take(25)
                .ToList();
        }

        private static List<string> Tokenize(string? text) =>
            (text ?? string.Empty)
                .Split([',', ';', '/', '\n', '·'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(t => t.Length > 1)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}
