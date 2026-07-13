using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.EmployeeFields;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- DTOs (candidate-keyed mirrors of the employee education/experience DTOs) --------

    public class CandidateEducationDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string EducationLevel { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string? FieldOfStudy { get; set; }
        public string? Qualification { get; set; }
        public int? GraduationYear { get; set; }
        public string? Remark { get; set; }
        public int DocumentCount { get; set; }
        /// <summary>Dynamic custom-field values (HC021, OwnerType=Education) — shared with the employee form.</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveCandidateEducationDto
    {
        public Guid? Id { get; set; }
        public Guid CandidateId { get; set; }
        public string EducationLevel { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string? FieldOfStudy { get; set; }
        public string? Qualification { get; set; }
        public int? GraduationYear { get; set; }
        public string? Remark { get; set; }
        /// <summary>Submitted values for the Education form's dynamic custom fields (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveCandidateEducationDtoValidator : AbstractValidator<SaveCandidateEducationDto>
    {
        public SaveCandidateEducationDtoValidator()
        {
            RuleFor(x => x.CandidateId).NotEmpty();
            RuleFor(x => x.EducationLevel).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Institution).NotEmpty().MaximumLength(300);
            RuleFor(x => x.GraduationYear).InclusiveBetween(1900, 2100).When(x => x.GraduationYear.HasValue);
        }
    }

    public class CandidateExperienceDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string Organization { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Responsibilities { get; set; }
        public bool IsExternal { get; set; }
        public bool IsGovernmental { get; set; }
        public int DocumentCount { get; set; }
        /// <summary>Dynamic custom-field values (HC021, OwnerType=Experience) — shared with the employee form.</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveCandidateExperienceDto
    {
        public Guid? Id { get; set; }
        public Guid CandidateId { get; set; }
        public string Organization { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Responsibilities { get; set; }
        public bool IsExternal { get; set; }
        public bool IsGovernmental { get; set; }
        /// <summary>Submitted values for the Experience form's dynamic custom fields (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveCandidateExperienceDtoValidator : AbstractValidator<SaveCandidateExperienceDto>
    {
        public SaveCandidateExperienceDtoValidator()
        {
            RuleFor(x => x.CandidateId).NotEmpty();
            RuleFor(x => x.Organization).NotEmpty().MaximumLength(300);
            RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(200);
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate!.Value)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("End date cannot be before the start date.");
        }
    }

    // ---- Interfaces -----------------------------------------------------------

    public interface IGetCandidateEducations { Task<List<CandidateEducationDto>> GetAsync(Guid candidateId); }
    public interface ISaveCandidateEducation { Task<Guid> SaveAsync(SaveCandidateEducationDto dto); }
    public interface IDeleteCandidateEducation { Task DeleteAsync(Guid id); }
    public interface IGetCandidateExperiences { Task<List<CandidateExperienceDto>> GetAsync(Guid candidateId); }
    public interface ISaveCandidateExperience { Task<Guid> SaveAsync(SaveCandidateExperienceDto dto); }
    public interface IDeleteCandidateExperience { Task DeleteAsync(Guid id); }

    // Attachments on candidate education/experience rows — the SAME EmployeeDocument table the
    // employee profile reads, so the files follow the row (and the person) to the employee at hire.
    public interface IUploadCandidateBackgroundDocument
    {
        Task<Guid> UploadAsync(Guid candidateId, string ownerType, Guid ownerId, Stream content, string fileName, string contentType, long length);
    }
    public interface IGetCandidateBackgroundDocuments { Task<List<EmployeeDocumentDto>> GetAsync(Guid candidateId, string ownerType, Guid ownerId); }
    public interface IDownloadCandidateBackgroundDocument { Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid documentId); }
    public interface IDeleteCandidateBackgroundDocument { Task DeleteAsync(Guid documentId); }

    internal static class CandidateBackgroundGuard
    {
        /// <summary>
        /// Candidate background rows live on the SAME person-owned tables the employee uses, so at
        /// hire the shared PersonId makes them the employee's automatically. Resolves the candidate's
        /// PersonId; an editing operation on an INTERNAL candidate is rejected — those rows belong to
        /// the employee master and must be maintained there (the person is the employee's).
        /// </summary>
        internal static async Task<Guid> ResolvePersonAsync(
            IRepository<Candidate> candidates, Guid candidateId, bool forWrite)
        {
            var candidate = await candidates.GetAll()
                .Where(c => c.Id == candidateId)
                .Select(c => new { c.PersonId, c.Source, c.AnonymizedAt })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Candidate), candidateId.ToString());

            if (candidate.PersonId is null)
                throw new ValidationException("candidateId",
                    "The candidate has no person record yet — save the candidate once first.");
            if (forWrite && candidate.AnonymizedAt.HasValue)
                throw new ValidationException("candidateId", "An anonymized candidate record can no longer change.");
            if (forWrite && candidate.Source == CandidateSource.Internal)
                throw new ValidationException("candidateId",
                    "This is an internal employee — their education and experience are maintained on the employee record.");

            return candidate.PersonId.Value;
        }

        /// <summary>Verifies a person-owned row belongs to the (person of the) candidate before mutating it.</summary>
        internal static async Task<Guid> ResolveOwningCandidatePersonAsync(
            IRepository<Candidate> candidates, Guid ownerPersonId)
        {
            var exists = await candidates.GetAll().AnyAsync(c => c.PersonId == ownerPersonId);
            if (!exists)
                throw new NotFoundException(nameof(Candidate), ownerPersonId.ToString());
            return ownerPersonId;
        }

        /// <summary>Writes to a person-owned row are rejected when the person belongs to an internal employee.</summary>
        internal static async Task EnsurePersonWritableAsync(IRepository<Candidate> candidates, Guid ownerPersonId)
        {
            if (await candidates.GetAll().AnyAsync(c => c.PersonId == ownerPersonId && c.Source == CandidateSource.Internal))
                throw new ValidationException("id",
                    "This record belongs to an internal employee and is maintained on the employee record.");
        }

        /// <summary>Resolves the PersonId owning an education/experience row (throws NotFound when absent).</summary>
        internal static async Task<Guid> ResolveOwnerRowPersonAsync(
            IRepository<EmployeeEducation> educations,
            IRepository<EmployeeExperience> experiences,
            EmployeeDocumentOwner owner, Guid ownerId)
        {
            var personId = owner == EmployeeDocumentOwner.Education
                ? await educations.GetAll().Where(e => e.Id == ownerId).Select(e => (Guid?)e.PersonId).FirstOrDefaultAsync()
                : await experiences.GetAll().Where(x => x.Id == ownerId).Select(x => (Guid?)x.PersonId).FirstOrDefaultAsync();
            return personId ?? throw new NotFoundException(owner.ToString(), ownerId.ToString());
        }

        internal static EmployeeDocumentOwner ParseOwner(string ownerType)
        {
            if (!Enum.TryParse<EmployeeDocumentOwner>(ownerType, true, out var owner) ||
                (owner != EmployeeDocumentOwner.Education && owner != EmployeeDocumentOwner.Experience))
                throw new ValidationException("ownerType", "Owner type must be Education or Experience.");
            return owner;
        }
    }

    // ---- Education (candidate-scoped) --------------------------------------------------

    public class GetCandidateEducations(
        IRepository<EmployeeEducation> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeDocument> documentRepository,
        ICustomFieldService customFields) : IGetCandidateEducations
    {
        public async Task<List<CandidateEducationDto>> GetAsync(Guid candidateId)
        {
            var personId = await CandidateBackgroundGuard.ResolvePersonAsync(candidateRepository, candidateId, forWrite: false);
            var list = await repository.GetAll()
                .Where(x => x.PersonId == personId)
                .OrderByDescending(x => x.GraduationYear)
                .Select(x => new CandidateEducationDto
                {
                    Id = x.Id,
                    CandidateId = candidateId,
                    EducationLevel = x.EducationLevel,
                    Institution = x.Institution,
                    FieldOfStudy = x.FieldOfStudy,
                    Qualification = x.Qualification,
                    GraduationYear = x.GraduationYear,
                    Remark = x.Remark,
                    DocumentCount = documentRepository.GetAll()
                        .Count(d => d.OwnerType == EmployeeDocumentOwner.Education && d.OwnerId == x.Id)
                })
                .ToListAsync();

            // Same OwnerType=Education pool the employee form reads → definitions & values are shared.
            var byOwner = await customFields.GetValuesForOwnersAsync(
                EmployeeFieldOwnerType.Education, list.Select(x => x.Id).ToList());
            foreach (var item in list)
                item.CustomFields = byOwner.TryGetValue(item.Id, out var m) ? m : new();
            return list;
        }
    }

    public class SaveCandidateEducation(
        IRepository<EmployeeEducation> repository,
        IRepository<Candidate> candidateRepository,
        ICustomFieldService customFields,
        IValidator<SaveCandidateEducationDto> validator,
        ILogger<SaveCandidateEducation> logger) : ISaveCandidateEducation
    {
        public async Task<Guid> SaveAsync(SaveCandidateEducationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            var personId = await CandidateBackgroundGuard.ResolvePersonAsync(candidateRepository, dto.CandidateId, forWrite: true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.PersonId == personId)
                    ?? throw new NotFoundException(nameof(EmployeeEducation), dto.Id.Value.ToString());
                entity.Update(dto.EducationLevel, dto.Institution, dto.FieldOfStudy, dto.Qualification, dto.GraduationYear, dto.Remark);
                repository.UpdateAsync(entity);
                await customFields.ApplyAsync(EmployeeFieldOwnerType.Education, entity.Id, dto.CustomFields);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated candidate education {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeEducation.Create(personId, dto.EducationLevel, dto.Institution,
                dto.FieldOfStudy, dto.Qualification, dto.GraduationYear, dto.Remark);
            await repository.AddAsync(created);
            await customFields.ApplyAsync(EmployeeFieldOwnerType.Education, created.Id, dto.CustomFields);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created candidate education {Id} on Person {PersonId}", created.Id, personId);
            return created.Id;
        }
    }

    public class DeleteCandidateEducation(
        IRepository<EmployeeEducation> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeDocument> documentRepository,
        ICustomFieldService customFields,
        ILogger<DeleteCandidateEducation> logger) : IDeleteCandidateEducation
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeEducation), id.ToString());
            await CandidateBackgroundGuard.ResolveOwningCandidatePersonAsync(candidateRepository, entity.PersonId);
            // Reject deletes on an internal candidate's shared record (employee master owns it).
            await CandidateBackgroundGuard.EnsurePersonWritableAsync(candidateRepository, entity.PersonId);

            // Attached documents and custom-field values cascade with the record (polymorphic, no FK).
            await DocumentStorage.DeleteForOwnerAsync(documentRepository, EmployeeDocumentOwner.Education, id);
            await customFields.DeleteForOwnerAsync(EmployeeFieldOwnerType.Education, id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted candidate education {Id}", id);
        }
    }

    // ---- Experience (candidate-scoped) ------------------------------------------------

    public class GetCandidateExperiences(
        IRepository<EmployeeExperience> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeDocument> documentRepository,
        ICustomFieldService customFields) : IGetCandidateExperiences
    {
        public async Task<List<CandidateExperienceDto>> GetAsync(Guid candidateId)
        {
            var personId = await CandidateBackgroundGuard.ResolvePersonAsync(candidateRepository, candidateId, forWrite: false);
            var list = await repository.GetAll()
                .Where(x => x.PersonId == personId)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new CandidateExperienceDto
                {
                    Id = x.Id,
                    CandidateId = candidateId,
                    Organization = x.Organization,
                    JobTitle = x.JobTitle,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Responsibilities = x.Responsibilities,
                    IsExternal = x.IsExternal,
                    IsGovernmental = x.IsGovernmental,
                    DocumentCount = documentRepository.GetAll()
                        .Count(d => d.OwnerType == EmployeeDocumentOwner.Experience && d.OwnerId == x.Id)
                })
                .ToListAsync();

            var byOwner = await customFields.GetValuesForOwnersAsync(
                EmployeeFieldOwnerType.Experience, list.Select(x => x.Id).ToList());
            foreach (var item in list)
                item.CustomFields = byOwner.TryGetValue(item.Id, out var m) ? m : new();
            return list;
        }
    }

    public class SaveCandidateExperience(
        IRepository<EmployeeExperience> repository,
        IRepository<Candidate> candidateRepository,
        ICustomFieldService customFields,
        IValidator<SaveCandidateExperienceDto> validator,
        ILogger<SaveCandidateExperience> logger) : ISaveCandidateExperience
    {
        public async Task<Guid> SaveAsync(SaveCandidateExperienceDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            var personId = await CandidateBackgroundGuard.ResolvePersonAsync(candidateRepository, dto.CandidateId, forWrite: true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.PersonId == personId)
                    ?? throw new NotFoundException(nameof(EmployeeExperience), dto.Id.Value.ToString());
                // Identical to the employee Experience form — the user-set external/governmental flags are honored.
                entity.Update(dto.Organization, dto.JobTitle, dto.StartDate, dto.EndDate, dto.Responsibilities,
                    isExternal: dto.IsExternal, isGovernmental: dto.IsGovernmental);
                repository.UpdateAsync(entity);
                await customFields.ApplyAsync(EmployeeFieldOwnerType.Experience, entity.Id, dto.CustomFields);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated candidate experience {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeExperience.Create(personId, dto.Organization, dto.JobTitle,
                dto.StartDate, dto.EndDate, dto.Responsibilities,
                isExternal: dto.IsExternal, isGovernmental: dto.IsGovernmental);
            await repository.AddAsync(created);
            await customFields.ApplyAsync(EmployeeFieldOwnerType.Experience, created.Id, dto.CustomFields);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created candidate experience {Id} on Person {PersonId}", created.Id, personId);
            return created.Id;
        }
    }

    public class DeleteCandidateExperience(
        IRepository<EmployeeExperience> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeDocument> documentRepository,
        ICustomFieldService customFields,
        ILogger<DeleteCandidateExperience> logger) : IDeleteCandidateExperience
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeExperience), id.ToString());
            await CandidateBackgroundGuard.ResolveOwningCandidatePersonAsync(candidateRepository, entity.PersonId);
            await CandidateBackgroundGuard.EnsurePersonWritableAsync(candidateRepository, entity.PersonId);

            await DocumentStorage.DeleteForOwnerAsync(documentRepository, EmployeeDocumentOwner.Experience, id);
            await customFields.DeleteForOwnerAsync(EmployeeFieldOwnerType.Experience, id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted candidate experience {Id}", id);
        }
    }

    // ---- Background-row attachments (candidate-scoped) ---------------------------------
    // Same EmployeeDocument storage as the employee profile — the attachment belongs to the
    // education/experience ROW (OwnerType+OwnerId), which belongs to the shared person, so at hire
    // it is already on the employee's profile. Until then EmployeeId anchors to the CANDIDATE id;
    // HireCandidate re-anchors it to the new employee (AssignEmployee).

    public class UploadCandidateBackgroundDocument(
        IRepository<EmployeeDocument> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<EmployeeExperience> experienceRepository,
        ILogger<UploadCandidateBackgroundDocument> logger) : IUploadCandidateBackgroundDocument
    {
        public async Task<Guid> UploadAsync(Guid candidateId, string ownerType, Guid ownerId, Stream content, string fileName, string contentType, long length)
        {
            var owner = CandidateBackgroundGuard.ParseOwner(ownerType);

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !DocumentStorage.AllowedExtensions.Contains(ext))
                throw new ValidationException("file", "Unsupported file type. Allowed: PDF, Office documents, images and text.");
            if (length <= 0 || length > DocumentStorage.MaxBytes)
                throw new ValidationException("file", "File must be between 1 byte and 10 MB.");

            // Write-guarded: rejects anonymized + internal candidates (employee master owns those rows).
            var personId = await CandidateBackgroundGuard.ResolvePersonAsync(candidateRepository, candidateId, forWrite: true);
            var ownerPersonId = await CandidateBackgroundGuard.ResolveOwnerRowPersonAsync(educationRepository, experienceRepository, owner, ownerId);
            if (ownerPersonId != personId) throw new NotFoundException(owner.ToString(), ownerId.ToString());

            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // Anchored to the candidate until hire re-anchors to the employee.
            var doc = EmployeeDocument.Create(candidateId, owner, ownerId, Path.GetFileName(fileName), contentType, bytes);
            await repository.AddAsync(doc);
            await repository.SaveChangesAsync();
            logger.LogInformation("Uploaded candidate background document {Id} for {Owner} {OwnerId}", doc.Id, owner, ownerId);
            return doc.Id;
        }
    }

    public class GetCandidateBackgroundDocuments(
        IRepository<EmployeeDocument> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<EmployeeExperience> experienceRepository) : IGetCandidateBackgroundDocuments
    {
        public async Task<List<EmployeeDocumentDto>> GetAsync(Guid candidateId, string ownerType, Guid ownerId)
        {
            var owner = CandidateBackgroundGuard.ParseOwner(ownerType);
            // Read allowed for internal candidates too (view of the employee's attachments).
            var personId = await CandidateBackgroundGuard.ResolvePersonAsync(candidateRepository, candidateId, forWrite: false);
            var ownerPersonId = await CandidateBackgroundGuard.ResolveOwnerRowPersonAsync(educationRepository, experienceRepository, owner, ownerId);
            if (ownerPersonId != personId) throw new NotFoundException(owner.ToString(), ownerId.ToString());

            var rows = await repository.GetAll()
                .Where(d => d.OwnerType == owner && d.OwnerId == ownerId)
                .OrderBy(d => d.FileName)
                .Select(d => new { d.Id, d.FileName, d.ContentType, d.FileSize, d.CreatedAt })
                .ToListAsync();

            return rows.Select(r => new EmployeeDocumentDto
            {
                Id = r.Id,
                FileName = r.FileName,
                ContentType = r.ContentType,
                FileSize = r.FileSize,
                UploadedAt = r.CreatedAt.ToDateTimeUtc()
            }).ToList();
        }
    }

    public class DownloadCandidateBackgroundDocument(
        IRepository<EmployeeDocument> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<EmployeeExperience> experienceRepository) : IDownloadCandidateBackgroundDocument
    {
        public async Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid documentId)
        {
            var doc = await repository.GetAll().FirstOrDefaultAsync(d => d.Id == documentId)
                ?? throw new NotFoundException(nameof(EmployeeDocument), documentId.ToString());
            // Authorized through the owner ROW's person → a candidate on that person
            // (doc.EmployeeId may still be the candidate anchor, so it can't gate access here).
            var ownerPersonId = await CandidateBackgroundGuard.ResolveOwnerRowPersonAsync(
                educationRepository, experienceRepository, doc.OwnerType, doc.OwnerId);
            await CandidateBackgroundGuard.ResolveOwningCandidatePersonAsync(candidateRepository, ownerPersonId);
            return (doc.Content, doc.ContentType, doc.FileName);
        }
    }

    public class DeleteCandidateBackgroundDocument(
        IRepository<EmployeeDocument> repository,
        IRepository<Candidate> candidateRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<EmployeeExperience> experienceRepository,
        ILogger<DeleteCandidateBackgroundDocument> logger) : IDeleteCandidateBackgroundDocument
    {
        public async Task DeleteAsync(Guid documentId)
        {
            var doc = await repository.GetAll().FirstOrDefaultAsync(d => d.Id == documentId)
                ?? throw new NotFoundException(nameof(EmployeeDocument), documentId.ToString());
            var ownerPersonId = await CandidateBackgroundGuard.ResolveOwnerRowPersonAsync(
                educationRepository, experienceRepository, doc.OwnerType, doc.OwnerId);
            await CandidateBackgroundGuard.ResolveOwningCandidatePersonAsync(candidateRepository, ownerPersonId);
            await CandidateBackgroundGuard.EnsurePersonWritableAsync(candidateRepository, ownerPersonId);

            repository.Delete(doc);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted candidate background document {Id}", documentId);
        }
    }
}
