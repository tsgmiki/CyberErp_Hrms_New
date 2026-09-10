using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------

    public class ContentModuleDto
    {
        public Guid Id { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? ExternalUrl { get; set; }
        public Guid? CourseFileId { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsRequired { get; set; }
    }

    public class CourseVersionDto
    {
        public Guid Id { get; set; }
        public Guid TrainingCourseId { get; set; }
        public int VersionNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ChangeNote { get; set; }
        public DateTime? PublishedOn { get; set; }
        public int ModuleCount { get; set; }
        public int RequiredModuleCount { get; set; }
        public int TotalMinutes { get; set; }
        public List<ContentModuleDto> Modules { get; set; } = [];
    }

    public class SaveCourseVersionModulesDto
    {
        public Guid CourseVersionId { get; set; }
        public List<ContentModuleSpecDto> Modules { get; set; } = [];
    }

    public class ContentModuleSpecDto
    {
        /// <summary>
        /// The module being edited, when the screen round-trips an existing one. Sending it back
        /// keeps the module's id — and therefore any quiz hanging off it — across a save.
        /// </summary>
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        /// <summary>Text | Document | Video | Link | Quiz.</summary>
        public string Kind { get; set; } = nameof(ContentModuleKind.Text);
        public string? Body { get; set; }
        public string? ExternalUrl { get; set; }
        public Guid? CourseFileId { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsRequired { get; set; } = true;
    }

    /// <summary>One module as the learner's player sees it, carrying their own progress.</summary>
    public class PlayerModuleDto : ContentModuleDto
    {
        public bool IsStarted { get; set; }
        public bool IsComplete { get; set; }
        public int SecondsSpent { get; set; }
        public int? LastPosition { get; set; }
        /// <summary>
        /// Set on a Quiz module: how this learner stands against it. Carries no questions and no
        /// answers — the quiz itself is only served through an attempt.
        /// </summary>
        public AssessmentStateDto? Assessment { get; set; }
    }

    /// <summary>Everything the player needs for one enrolment.</summary>
    public class CoursePlayerDto
    {
        public Guid TrainingEnrollmentId { get; set; }
        public Guid TrainingCourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public Guid? CourseVersionId { get; set; }
        public int? VersionNumber { get; set; }
        public string EnrollmentStatus { get; set; } = string.Empty;
        public int RequiredTotal { get; set; }
        public int RequiredDone { get; set; }
        public int PercentComplete { get; set; }
        /// <summary>False once the enrolment is finished — the player becomes read-only.</summary>
        public bool IsTrackable { get; set; }
        /// <summary>Set when the course has no published content yet, so the UI can say why.</summary>
        public string? UnavailableReason { get; set; }
        public List<PlayerModuleDto> Modules { get; set; } = [];
    }

    public class RecordModuleProgressDto
    {
        public Guid TrainingEnrollmentId { get; set; }
        public Guid ContentModuleId { get; set; }
        /// <summary>Seconds to ADD to the running total for this module.</summary>
        public int AddSeconds { get; set; }
        public int? LastPosition { get; set; }
        public bool Complete { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IGetCourseVersions { Task<List<CourseVersionDto>> GetAsync(Guid trainingCourseId); }
    public interface ICreateCourseVersion { Task<Guid> CreateAsync(Guid trainingCourseId, string? changeNote); }
    public interface ISetCourseVersionModules { Task SetAsync(SaveCourseVersionModulesDto dto); }
    public interface IPublishCourseVersion { Task PublishAsync(Guid courseVersionId); }
    public interface IGetCoursePlayer { Task<CoursePlayerDto> GetAsync(Guid trainingEnrollmentId); }
    public interface IRecordModuleProgress { Task<CoursePlayerDto> RecordAsync(RecordModuleProgressDto dto); }

    // ---- Authoring ----------------------------------------------------------

    public class GetCourseVersions(IRepository<CourseVersion> repository) : IGetCourseVersions
    {
        public async Task<List<CourseVersionDto>> GetAsync(Guid trainingCourseId)
        {
            var versions = await repository.GetAll().AsNoTracking()
                .Include(v => v.Modules)
                .Where(v => v.TrainingCourseId == trainingCourseId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            return [.. versions.Select(ContentShared.ToDto)];
        }
    }

    public class CreateCourseVersion(
        IRepository<CourseVersion> repository,
        IRepository<TrainingCourse> courseRepository,
        ILogger<CreateCourseVersion> logger) : ICreateCourseVersion
    {
        public async Task<Guid> CreateAsync(Guid trainingCourseId, string? changeNote)
        {
            if (!await courseRepository.GetAll().AnyAsync(c => c.Id == trainingCourseId))
                throw new NotFoundException(nameof(TrainingCourse), trainingCourseId.ToString());

            // One draft at a time. Two open drafts of the same course is a question nobody can
            // answer — which one publishes? — and the authoring screen has no way to show it.
            if (await repository.GetAll().AnyAsync(v =>
                    v.TrainingCourseId == trainingCourseId && v.Status == CourseVersionStatus.Draft))
                throw new ValidationException("trainingCourseId",
                    "This course already has a draft version — publish or finish that one first.");

            var next = await repository.GetAll()
                .Where(v => v.TrainingCourseId == trainingCourseId)
                .Select(v => (int?)v.VersionNumber).MaxAsync() ?? 0;

            var version = CourseVersion.Create(trainingCourseId, next + 1, changeNote);
            await repository.AddAsync(version);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created draft v{Version} of course {CourseId}", next + 1, trainingCourseId);
            return version.Id;
        }
    }

    public class SetCourseVersionModules(
        IRepository<CourseVersion> repository,
        IRepository<ContentModule> moduleRepository,
        IRepository<CourseFile> fileRepository,
        ILogger<SetCourseVersionModules> logger) : ISetCourseVersionModules
    {
        public async Task SetAsync(SaveCourseVersionModulesDto dto)
        {
            var version = await repository.GetAll()
                    .Include(v => v.Modules)
                    .FirstOrDefaultAsync(v => v.Id == dto.CourseVersionId)
                ?? throw new NotFoundException(nameof(CourseVersion), dto.CourseVersionId.ToString());

            var specs = (dto.Modules ?? []).Select(m =>
            {
                if (!Enum.TryParse<ContentModuleKind>(m.Kind, true, out var kind))
                    throw new ValidationException("kind", $"'{m.Kind}' is not a module kind.");
                return new ContentModuleSpec(m.Title, kind, m.Body, m.ExternalUrl,
                    m.CourseFileId, m.EstimatedMinutes, m.IsRequired, m.Id);
            }).ToList();

            await EnsureFilesBelongToCourseAsync(specs, version.TrainingCourseId);

            // ⚠️ Modules that came back with their id are UPDATED IN PLACE, so a save does not churn
            // module ids. It used to delete and recreate the lot, which was harmless until a Quiz
            // module started owning an assessment keyed on its id (logic §12.87).
            var before = version.Modules.ToDictionary(m => m.Id);
            IReadOnlyList<ContentModule> removed;
            try
            {
                removed = version.SetModules(specs);
            }
            catch (InvalidOperationException ex)
            {
                // "A published version cannot be edited" is a rule the user should see, not a 500.
                throw new ValidationException("courseVersionId", ex.Message);
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException("modules", ex.Message);
            }

            // Dropped rows go explicitly: EF will not delete children it was never told about on an
            // owned-by-FK collection. Any quiz on a dropped module cascades with it in the database.
            foreach (var gone in removed)
                moduleRepository.Delete(gone);

            foreach (var module in version.Modules)
            {
                if (string.IsNullOrEmpty(module.TenantId)) module.TenantId = version.TenantId;
                if (before.ContainsKey(module.Id)) moduleRepository.UpdateAsync(module);
                else await moduleRepository.AddAsync(module);
            }

            repository.UpdateAsync(version);
            await repository.SaveChangesAsync();
            logger.LogInformation("Version {VersionId} now has {Count} module(s); {Removed} dropped",
                version.Id, specs.Count, removed.Count);
        }

        /// <summary>
        /// A Document module must point at a file in ITS OWN course's library.
        ///
        /// <para>Checked because <c>CourseFileId</c> arrives from the client: without it an author
        /// could reference another course's material, and the learner's download rule — which grants
        /// access through the course a file is served by — would then hand it out to the wrong
        /// audience (logic §12.89).</para>
        /// </summary>
        private async Task EnsureFilesBelongToCourseAsync(List<ContentModuleSpec> specs, Guid trainingCourseId)
        {
            var wanted = specs
                .Where(s => s.Kind == ContentModuleKind.Document && s.CourseFileId.HasValue)
                .Select(s => s.CourseFileId!.Value)
                .Distinct()
                .ToList();
            if (wanted.Count == 0) return;

            var mine = await fileRepository.GetAll().AsNoTracking()
                .Where(f => wanted.Contains(f.Id) && f.TrainingCourseId == trainingCourseId)
                .Select(f => f.Id)
                .ToListAsync();

            var stranger = wanted.Except(mine).ToList();
            if (stranger.Count > 0)
                throw new ValidationException("modules",
                    "A document module references a file that does not belong to this course. Upload it to this course's material library first.");
        }
    }

    public class PublishCourseVersion(
        IRepository<CourseVersion> repository,
        IRepository<Assessment> assessmentRepository,
        ILogger<PublishCourseVersion> logger) : IPublishCourseVersion
    {
        public async Task PublishAsync(Guid courseVersionId)
        {
            var version = await repository.GetAll()
                    .Include(v => v.Modules)
                    .FirstOrDefaultAsync(v => v.Id == courseVersionId)
                ?? throw new NotFoundException(nameof(CourseVersion), courseVersionId.ToString());

            await EnsureQuizzesAreBuiltAsync(version);

            try
            {
                version.Publish();
            }
            catch (InvalidOperationException ex)
            {
                throw new ValidationException("courseVersionId", ex.Message);
            }

            // The previous live version is retired in the same transaction — two published versions
            // of one course would leave "which one does a new learner get?" undefined.
            var previous = await repository.GetAll()
                .Where(v => v.TrainingCourseId == version.TrainingCourseId
                    && v.Id != version.Id && v.Status == CourseVersionStatus.Published)
                .ToListAsync();
            foreach (var old in previous)
            {
                old.Retire();
                repository.UpdateAsync(old);
            }

            repository.UpdateAsync(version);
            await repository.SaveChangesAsync();
            logger.LogInformation("Published v{Version} of course {CourseId}; retired {Count} previous",
                version.VersionNumber, version.TrainingCourseId, previous.Count);
        }

        /// <summary>
        /// A Quiz module with no questions is the assessment equivalent of an empty version: the
        /// learner opens it, finds nothing to answer, and — because it is required — can never
        /// complete the course. The check lives here rather than on the aggregate because the
        /// assessment is a separate root that <see cref="CourseVersion"/> cannot see.
        /// </summary>
        private async Task EnsureQuizzesAreBuiltAsync(CourseVersion version)
        {
            var quizModules = version.Modules
                .Where(m => m.Kind == ContentModuleKind.Quiz)
                .ToList();
            if (quizModules.Count == 0) return;

            var moduleIds = quizModules.Select(m => m.Id).ToList();
            var built = await assessmentRepository.GetAll().AsNoTracking()
                .Include(a => a.Questions)
                .Where(a => moduleIds.Contains(a.ContentModuleId))
                .Select(a => new { a.ContentModuleId, Count = a.Questions.Count })
                .ToListAsync();

            var unbuilt = quizModules
                .Where(m => (built.FirstOrDefault(b => b.ContentModuleId == m.Id)?.Count ?? 0) == 0)
                .Select(m => m.Title)
                .ToList();

            if (unbuilt.Count > 0)
                throw new ValidationException("modules",
                    $"These quiz modules have no questions yet: {string.Join(", ", unbuilt)}.");
        }
    }

    // ---- Shared -------------------------------------------------------------

    internal static class ContentShared
    {
        internal static CourseVersionDto ToDto(CourseVersion v) => new()
        {
            Id = v.Id,
            TrainingCourseId = v.TrainingCourseId,
            VersionNumber = v.VersionNumber,
            Status = v.Status.ToString(),
            ChangeNote = v.ChangeNote,
            PublishedOn = v.PublishedOn,
            ModuleCount = v.Modules.Count,
            RequiredModuleCount = v.Modules.Count(m => m.IsRequired),
            TotalMinutes = v.Modules.Sum(m => m.EstimatedMinutes ?? 0),
            Modules = [.. v.Modules.OrderBy(m => m.SortOrder).Select(m => new ContentModuleDto
            {
                Id = m.Id,
                SortOrder = m.SortOrder,
                Title = m.Title,
                Kind = m.Kind.ToString(),
                Body = m.Body,
                ExternalUrl = m.ExternalUrl,
                CourseFileId = m.CourseFileId,
                EstimatedMinutes = m.EstimatedMinutes,
                IsRequired = m.IsRequired
            })]
        };
    }
}
