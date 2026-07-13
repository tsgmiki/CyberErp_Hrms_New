using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeDocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IUploadEmployeeDocument
    {
        Task<Guid> UploadAsync(Guid employeeId, string ownerType, Guid ownerId, string? ownerField, Stream content, string fileName, string contentType, long length);
    }
    public interface IGetEmployeeDocuments { Task<List<EmployeeDocumentDto>> GetAsync(string ownerType, Guid ownerId, string? ownerField); }
    public interface IDownloadEmployeeDocument { Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid documentId); }
    public interface IDeleteEmployeeDocument { Task DeleteAsync(Guid documentId); }

    internal static class DocumentStorage
    {
        internal const long MaxBytes = 10 * 1024 * 1024; // 10 MB

        internal static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".csv", ".jpg", ".jpeg", ".png", ".webp", ".gif",
        };

        /// <summary>Removes every document attached to an owner record (cascade on owner delete).</summary>
        internal static async Task DeleteForOwnerAsync(
            IRepository<EmployeeDocument> repository, EmployeeDocumentOwner ownerType, Guid ownerId)
        {
            var docs = await repository.GetAll()
                .Where(d => d.OwnerType == ownerType && d.OwnerId == ownerId)
                .ToListAsync();
            foreach (var doc in docs) repository.Delete(doc);
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class UploadEmployeeDocument(
        IRepository<EmployeeDocument> repository,
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<EmployeeExperience> experienceRepository,
        IRepository<DynamicFormRecord> recordRepository,
        ILogger<UploadEmployeeDocument> logger) : IUploadEmployeeDocument
    {
        public async Task<Guid> UploadAsync(Guid employeeId, string ownerType, Guid ownerId, string? ownerField, Stream content, string fileName, string contentType, long length)
        {
            if (!Enum.TryParse<EmployeeDocumentOwner>(ownerType, true, out var owner))
                throw new ValidationException("ownerType", "Unknown document owner type.");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !DocumentStorage.AllowedExtensions.Contains(ext))
                throw new ValidationException("file", "Unsupported file type. Allowed: PDF, Office documents, images and text.");
            if (length <= 0 || length > DocumentStorage.MaxBytes)
                throw new ValidationException("file", "File must be between 1 byte and 10 MB.");

            var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, employeeId);
            await EnsureOwnerBelongsAsync(owner, ownerId, personId, employeeId);

            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var doc = EmployeeDocument.Create(employeeId, owner, ownerId, Path.GetFileName(fileName), contentType, bytes, ownerField);
            await repository.AddAsync(doc);
            await repository.SaveChangesAsync();
            logger.LogInformation("Uploaded EmployeeDocument {Id} for {Owner} {OwnerId} ({Bytes} bytes)", doc.Id, owner, ownerId, bytes.Length);
            return doc.Id;
        }

        /// <summary>The owner record must belong to a visible employee: education/experience are
        /// person-owned; a dynamic-form record is employee-owned (OwnerType "Employee", OwnerId = the employee).</summary>
        private async Task EnsureOwnerBelongsAsync(EmployeeDocumentOwner owner, Guid ownerId, Guid personId, Guid employeeId)
        {
            var exists = owner switch
            {
                EmployeeDocumentOwner.Education => await educationRepository.GetAll().AnyAsync(e => e.Id == ownerId && e.PersonId == personId),
                EmployeeDocumentOwner.Experience => await experienceRepository.GetAll().AnyAsync(x => x.Id == ownerId && x.PersonId == personId),
                EmployeeDocumentOwner.DynamicFormRecord => await recordRepository.GetAll()
                    .AnyAsync(r => r.Id == ownerId && r.OwnerType == "Employee" && r.OwnerId == employeeId),
                _ => false,
            };
            if (!exists) throw new NotFoundException(owner.ToString(), ownerId.ToString());
        }
    }

    public class GetEmployeeDocuments(
        IRepository<EmployeeDocument> repository,
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<EmployeeExperience> experienceRepository,
        IRepository<DynamicFormRecord> recordRepository) : IGetEmployeeDocuments
    {
        public async Task<List<EmployeeDocumentDto>> GetAsync(string ownerType, Guid ownerId, string? ownerField)
        {
            if (!Enum.TryParse<EmployeeDocumentOwner>(ownerType, true, out var owner))
                throw new ValidationException("ownerType", "Unknown document owner type.");
            var field = string.IsNullOrWhiteSpace(ownerField) ? null : ownerField;

            // Access is authorized through the owner record → a visible employee.
            if (owner == EmployeeDocumentOwner.DynamicFormRecord)
            {
                var recordEmployeeId = await recordRepository.GetAll()
                    .Where(r => r.Id == ownerId && r.OwnerType == "Employee")
                    .Select(r => (Guid?)r.OwnerId).FirstOrDefaultAsync();
                if (recordEmployeeId is null) throw new NotFoundException(owner.ToString(), ownerId.ToString());
                await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, recordEmployeeId.Value);
            }
            else
            {
                var personId = owner == EmployeeDocumentOwner.Education
                    ? await educationRepository.GetAll().Where(e => e.Id == ownerId).Select(e => (Guid?)e.PersonId).FirstOrDefaultAsync()
                    : await experienceRepository.GetAll().Where(x => x.Id == ownerId).Select(x => (Guid?)x.PersonId).FirstOrDefaultAsync();
                if (personId is null) throw new NotFoundException(owner.ToString(), ownerId.ToString());
                await EmployeeGuard.EnsurePersonVisibleAsync(employeeRepository, personId.Value);
            }

            var rows = await repository.GetAll()
                .Where(d => d.OwnerType == owner && d.OwnerId == ownerId && d.OwnerField == field)
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

    public class DownloadEmployeeDocument(
        IRepository<EmployeeDocument> repository,
        IRepository<Employee> employeeRepository) : IDownloadEmployeeDocument
    {
        public async Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid documentId)
        {
            var doc = await repository.GetAll().FirstOrDefaultAsync(d => d.Id == documentId)
                ?? throw new NotFoundException(nameof(EmployeeDocument), documentId.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, doc.EmployeeId);
            return (doc.Content, doc.ContentType, doc.FileName);
        }
    }

    public class DeleteEmployeeDocument(
        IRepository<EmployeeDocument> repository,
        IRepository<Employee> employeeRepository,
        ILogger<DeleteEmployeeDocument> logger) : IDeleteEmployeeDocument
    {
        public async Task DeleteAsync(Guid documentId)
        {
            var doc = await repository.GetAll().FirstOrDefaultAsync(d => d.Id == documentId)
                ?? throw new NotFoundException(nameof(EmployeeDocument), documentId.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, doc.EmployeeId);

            repository.Delete(doc);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeDocument {Id}", documentId);
        }
    }
}
