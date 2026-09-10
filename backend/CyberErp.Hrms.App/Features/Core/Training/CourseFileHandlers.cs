using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------

    /// <summary>A file in a course's library. Deliberately carries no bytes — see the download path.</summary>
    public class CourseFileDto
    {
        public Guid Id { get; set; }
        public Guid TrainingCourseId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public DateTime UploadedOn { get; set; }
        /// <summary>How many content modules point at it — a file in use cannot be deleted.</summary>
        public int UsedByModules { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IGetCourseFiles { Task<List<CourseFileDto>> GetAsync(Guid trainingCourseId); }
    public interface IUploadCourseFile
    {
        Task<Guid> UploadAsync(Guid trainingCourseId, Stream content, string fileName,
            string contentType, long length, string? description);
    }
    public interface IDeleteCourseFile { Task DeleteAsync(Guid id); }
    /// <summary>Author preview — gated with course authoring at the controller.</summary>
    public interface IDownloadCourseFile { Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid id); }
    /// <summary>The learner's copy — gated on their OWN enrolment. See the handler.</summary>
    public interface IDownloadCourseMaterial { Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid id); }

    // ---- Shared -------------------------------------------------------------

    internal static class CourseFileStorage
    {
        /// <summary>
        /// 25 MB — larger than the 10 MB per-employee cap, because a course handbook is a shared
        /// asset read by the whole workforce rather than one person's certificate, and it is stored
        /// once for all of them.
        /// </summary>
        internal const long MaxBytes = 25 * 1024 * 1024;

        /// <summary>
        /// Documents and decks only.
        ///
        /// <para>⚠️ NO VIDEO EXTENSIONS, deliberately. Streaming media out of SQL Server does not
        /// scale, and a Video module is a URL for exactly that reason — allowing an .mp4 upload here
        /// would quietly undo that decision one 200 MB row at a time (logic §12.89).</para>
        /// </summary>
        internal static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".csv", ".md", ".jpg", ".jpeg", ".png", ".webp", ".gif",
        };
    }

    // ---- Handlers -----------------------------------------------------------

    public class GetCourseFiles(
        IRepository<CourseFile> repository,
        IRepository<ContentModule> moduleRepository) : IGetCourseFiles
    {
        public async Task<List<CourseFileDto>> GetAsync(Guid trainingCourseId)
        {
            // The bytes are excluded from the projection on purpose: listing a library of ten PDFs
            // must not pull sixty megabytes through the ORM to render ten filenames.
            var files = await repository.GetAll().AsNoTracking()
                .Where(f => f.TrainingCourseId == trainingCourseId)
                .Select(f => new CourseFileDto
                {
                    Id = f.Id,
                    TrainingCourseId = f.TrainingCourseId,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    Description = f.Description,
                    UploadedOn = f.CreatedAt.ToDateTimeUtc()
                })
                .ToListAsync();
            if (files.Count == 0) return files;

            var ids = files.Select(f => f.Id).ToList();
            var usage = await moduleRepository.GetAll().AsNoTracking()
                .Where(m => m.CourseFileId != null && ids.Contains(m.CourseFileId.Value))
                .GroupBy(m => m.CourseFileId!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var f in files)
                f.UsedByModules = usage.FirstOrDefault(u => u.Key == f.Id)?.Count ?? 0;

            return [.. files.OrderByDescending(f => f.UploadedOn)];
        }
    }

    public class UploadCourseFile(
        IRepository<CourseFile> repository,
        IRepository<TrainingCourse> courseRepository,
        ILogger<UploadCourseFile> logger) : IUploadCourseFile
    {
        public async Task<Guid> UploadAsync(Guid trainingCourseId, Stream content, string fileName,
            string contentType, long length, string? description)
        {
            if (!await courseRepository.GetAll().AnyAsync(c => c.Id == trainingCourseId))
                throw new NotFoundException(nameof(TrainingCourse), trainingCourseId.ToString());

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !CourseFileStorage.AllowedExtensions.Contains(ext))
                throw new ValidationException("file",
                    "Unsupported file type. Allowed: PDF, Office documents, text and images. Video belongs on a Video module as a URL.");
            if (length <= 0 || length > CourseFileStorage.MaxBytes)
                throw new ValidationException("file", "File must be between 1 byte and 25 MB.");

            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // The declared length is a claim by the client; the bytes actually read are the fact.
            if (bytes.LongLength > CourseFileStorage.MaxBytes)
                throw new ValidationException("file", "File must be between 1 byte and 25 MB.");

            CourseFile file = null!;
            try
            {
                file = CourseFile.Create(trainingCourseId, fileName, contentType, bytes, description);
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException("file", ex.Message);
            }

            await repository.AddAsync(file);
            await repository.SaveChangesAsync();
            logger.LogInformation("Uploaded CourseFile {Id} '{Name}' to course {CourseId} ({Bytes} bytes)",
                file.Id, file.FileName, trainingCourseId, bytes.LongLength);
            return file.Id;
        }
    }

    /// <summary>
    /// Removes a file from a course's library.
    ///
    /// <para>⚠️ REFUSED WHILE ANY MODULE POINTS AT IT, including a retired version's. A retired
    /// version is the record of what people actually completed, so deleting its material would
    /// destroy the evidence behind every completion earned on it (logic §12.89).</para>
    /// </summary>
    public class DeleteCourseFile(
        IRepository<CourseFile> repository,
        IRepository<ContentModule> moduleRepository,
        ILogger<DeleteCourseFile> logger) : IDeleteCourseFile
    {
        public async Task DeleteAsync(Guid id)
        {
            var file = await repository.GetAll().FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new NotFoundException(nameof(CourseFile), id.ToString());

            var usedBy = await moduleRepository.GetAll().AsNoTracking()
                .Where(m => m.CourseFileId == id)
                .Select(m => m.Title)
                .Take(5)
                .ToListAsync();

            if (usedBy.Count > 0)
                throw new ValidationException("id",
                    $"This file is used by {string.Join(", ", usedBy)}. Remove it from those modules first — or, if they belong to a published version, upload a replacement and point a new draft at it.");

            repository.Delete(file);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted CourseFile {Id} '{Name}'", id, file.FileName);
        }
    }

    public class DownloadCourseFile(IRepository<CourseFile> repository) : IDownloadCourseFile
    {
        public async Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid id)
        {
            var file = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new NotFoundException(nameof(CourseFile), id.ToString());
            return (file.Content, file.ContentType, file.FileName);
        }
    }

    /// <summary>
    /// The learner's download.
    ///
    /// <para>⚠️ THIS ACCESS RULE IS THE WHOLE POINT OF THE TABLE. A learner may open a file only when
    /// it is referenced by a module of the PUBLISHED version of a course they are enrolled on. That
    /// is the sentence the per-employee document table could not express, and the reason phase 3 left
    /// the Document kind unreachable rather than ship something readable by one person or by
    /// everyone (logic §12.89).</para>
    ///
    /// <para>HR keeps its own access through the authoring endpoint, which is gated on course
    /// authoring — this one is gated on <c>myTraining</c> and answers only for the caller.</para>
    /// </summary>
    public class DownloadCourseMaterial(
        IRepository<CourseFile> repository,
        IRepository<ContentModule> moduleRepository,
        IRepository<CourseVersion> versionRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser) : IDownloadCourseMaterial
    {
        public async Task<(byte[] Content, string ContentType, string FileName)> GetAsync(Guid id)
        {
            var myEmployeeId = await PlayerAccess.MyEmployeeIdAsync(userRepository, currentUser.GetCurrentUserId());
            if (myEmployeeId is null)
                throw new ValidationException("access", "Your account is not linked to an employee record.");

            var file = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new NotFoundException(nameof(CourseFile), id.ToString());

            // Which PUBLISHED versions serve this file? A draft does not count: unpublished material
            // is not yet anybody's course.
            var servingVersionIds = await moduleRepository.GetAll().AsNoTracking()
                .Where(m => m.CourseFileId == id)
                .Select(m => m.CourseVersionId)
                .Distinct()
                .ToListAsync();

            var servedCourseIds = await versionRepository.GetAll().AsNoTracking()
                .Where(v => servingVersionIds.Contains(v.Id) && v.Status == CourseVersionStatus.Published)
                .Select(v => v.TrainingCourseId)
                .Distinct()
                .ToListAsync();

            if (servedCourseIds.Count == 0)
                throw new ValidationException("access", "This material is not part of a published course.");

            // Enrolled on any session of one of those courses — withdrawn does not count, completed
            // does: finishing a course must not lock you out of the handbook you were given.
            var enrolled = await enrollmentRepository.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == myEmployeeId.Value
                    && e.Status != TrainingEnrollmentStatus.Withdrawn)
                .Join(sessionRepository.GetAll().AsNoTracking(),
                    e => e.TrainingSessionId, s => s.Id, (e, s) => s.TrainingCourseId)
                .AnyAsync(courseId => servedCourseIds.Contains(courseId));

            if (!enrolled)
                throw new ValidationException("access", "You are not enrolled on the course this material belongs to.");

            return (file.Content, file.ContentType, file.FileName);
        }
    }
}
