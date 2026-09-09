using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    /// <summary>
    /// The learner's course player: the published content of the course they are enrolled on, with
    /// their own progress against each module.
    ///
    /// <para>⚠️ SELF ONLY. The enrolment must belong to the caller. Everything here is a learner
    /// action — reading their own material, recording their own progress — and none of it is
    /// something a manager or HR does on someone else's behalf; participation recorded BY someone
    /// else already has its own endpoint (logic §12.86).</para>
    ///
    /// <para>⚠️ Content attaches to the course, and progress to an existing ENROLMENT. Enrolment is
    /// still session-based, so this is blended delivery — the session is how a learner gets on the
    /// course, the modules are the material. Session-free self-paced enrolment needs
    /// <c>TrainingEnrollment.TrainingSessionId</c> to become nullable, which is a migration against a
    /// unique index and every query that reads it, and is deliberately NOT part of this phase.</para>
    /// </summary>
    internal static class PlayerAccess
    {
        /// <summary>The caller's own employee id, or null for an unlinked account.</summary>
        internal static async Task<Guid?> MyEmployeeIdAsync(IRepository<User> users, Guid? userId)
        {
            if (!userId.HasValue) return null;
            return await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        }
    }

    public class GetCoursePlayer(
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<CourseVersion> versionRepository,
        IRepository<ModuleProgress> progressRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser) : IGetCoursePlayer
    {
        public Task<CoursePlayerDto> GetAsync(Guid trainingEnrollmentId) =>
            BuildAsync(trainingEnrollmentId, enrollmentRepository, sessionRepository, courseRepository,
                versionRepository, progressRepository, userRepository, currentUser);

        internal static async Task<CoursePlayerDto> BuildAsync(
            Guid trainingEnrollmentId,
            IRepository<TrainingEnrollment> enrollments,
            IRepository<TrainingSession> sessions,
            IRepository<TrainingCourse> courses,
            IRepository<CourseVersion> versions,
            IRepository<ModuleProgress> progressRepo,
            IRepository<User> users,
            ICurrentUserService currentUser)
        {
            var myEmployeeId = await PlayerAccess.MyEmployeeIdAsync(users, currentUser.GetCurrentUserId());

            var enrollment = await enrollments.GetAll().AsNoTracking()
                    .Where(e => e.Id == trainingEnrollmentId)
                    .Select(e => new { e.Id, e.EmployeeId, e.TrainingSessionId, e.Status })
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TrainingEnrollment), trainingEnrollmentId.ToString());

            if (myEmployeeId is null || enrollment.EmployeeId != myEmployeeId.Value)
                throw new ValidationException("access", "This is not your enrolment.");

            var courseId = await sessions.GetAll().AsNoTracking()
                .Where(s => s.Id == enrollment.TrainingSessionId)
                .Select(s => s.TrainingCourseId)
                .FirstOrDefaultAsync();

            var courseName = await courses.GetAll().AsNoTracking()
                .Where(c => c.Id == courseId).Select(c => c.Name).FirstOrDefaultAsync() ?? "Course";

            var dto = new CoursePlayerDto
            {
                TrainingEnrollmentId = enrollment.Id,
                TrainingCourseId = courseId,
                CourseName = courseName,
                EnrollmentStatus = enrollment.Status.ToString(),
                // Finished enrolments stay readable but stop recording — re-opening a completed
                // course to look something up must not reopen the record.
                IsTrackable = enrollment.Status == TrainingEnrollmentStatus.Enrolled
            };

            // The live version only. A retired version keeps the progress already earned against it,
            // but nobody new is put on it.
            var version = await versions.GetAll().AsNoTracking()
                .Include(v => v.Modules)
                .Where(v => v.TrainingCourseId == courseId && v.Status == CourseVersionStatus.Published)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            if (version is null)
            {
                dto.UnavailableReason = "This course has no published content yet.";
                return dto;
            }

            dto.CourseVersionId = version.Id;
            dto.VersionNumber = version.VersionNumber;

            var moduleIds = version.Modules.Select(m => m.Id).ToList();
            var progress = await progressRepo.GetAll().AsNoTracking()
                .Where(p => p.TrainingEnrollmentId == enrollment.Id && moduleIds.Contains(p.ContentModuleId))
                .Select(p => new { p.ContentModuleId, p.CompletedOn, p.SecondsSpent, p.LastPosition })
                .ToListAsync();

            dto.Modules = [.. version.Modules.OrderBy(m => m.SortOrder).Select(m =>
            {
                var p = progress.FirstOrDefault(x => x.ContentModuleId == m.Id);
                return new PlayerModuleDto
                {
                    Id = m.Id,
                    SortOrder = m.SortOrder,
                    Title = m.Title,
                    Kind = m.Kind.ToString(),
                    Body = m.Body,
                    ExternalUrl = m.ExternalUrl,
                    DocumentId = m.DocumentId,
                    EstimatedMinutes = m.EstimatedMinutes,
                    IsRequired = m.IsRequired,
                    IsStarted = p is not null,
                    IsComplete = p?.CompletedOn is not null,
                    SecondsSpent = p?.SecondsSpent ?? 0,
                    LastPosition = p?.LastPosition
                };
            })];

            dto.RequiredTotal = dto.Modules.Count(m => m.IsRequired);
            dto.RequiredDone = dto.Modules.Count(m => m.IsRequired && m.IsComplete);
            dto.PercentComplete = dto.RequiredTotal == 0
                ? 0
                : (int)Math.Round(dto.RequiredDone * 100m / dto.RequiredTotal);
            return dto;
        }
    }

    /// <summary>
    /// Records a visit to one module and, when that was the last required one, COMPLETES THE
    /// ENROLMENT.
    ///
    /// <para>⚠️ This is the point of phase 3. Completion used to be an assertion — HR typed it in
    /// through <c>RecordParticipation</c> — and is now an observation derived from what the learner
    /// actually did. The manual route still exists for instructor-led sessions, which have no
    /// content to observe.</para>
    /// </summary>
    public class RecordModuleProgress(
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<CourseVersion> versionRepository,
        IRepository<ContentModule> moduleRepository,
        IRepository<ModuleProgress> progressRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser,
        ILogger<RecordModuleProgress> logger) : IRecordModuleProgress
    {
        /// <summary>
        /// A single recorded visit is capped at two hours. The client sends elapsed time, so a tab
        /// left open overnight would otherwise book sixteen hours against one module and make the
        /// time evidence worthless.
        /// </summary>
        private const int MaxSecondsPerCall = 2 * 60 * 60;

        public async Task<CoursePlayerDto> RecordAsync(RecordModuleProgressDto dto)
        {
            var myEmployeeId = await PlayerAccess.MyEmployeeIdAsync(userRepository, currentUser.GetCurrentUserId());

            var enrollment = await enrollmentRepository.GetAll()
                    .FirstOrDefaultAsync(e => e.Id == dto.TrainingEnrollmentId)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), dto.TrainingEnrollmentId.ToString());

            if (myEmployeeId is null || enrollment.EmployeeId != myEmployeeId.Value)
                throw new ValidationException("access", "This is not your enrolment.");
            if (enrollment.Status != TrainingEnrollmentStatus.Enrolled)
                throw new ValidationException("trainingEnrollmentId",
                    $"This enrolment is {enrollment.Status} — its progress is closed.");

            var module = await moduleRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == dto.ContentModuleId)
                ?? throw new NotFoundException(nameof(ContentModule), dto.ContentModuleId.ToString());

            // The module must belong to the version this enrolment's course actually publishes —
            // otherwise a learner could post progress against any module id in the tenant.
            var courseId = await sessionRepository.GetAll().AsNoTracking()
                .Where(s => s.Id == enrollment.TrainingSessionId)
                .Select(s => s.TrainingCourseId).FirstOrDefaultAsync();
            var liveVersion = await versionRepository.GetAll().AsNoTracking()
                .Include(v => v.Modules)
                .Where(v => v.TrainingCourseId == courseId && v.Status == CourseVersionStatus.Published)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync()
                ?? throw new ValidationException("contentModuleId", "This course has no published content.");

            if (liveVersion.Modules.All(m => m.Id != module.Id))
                throw new ValidationException("contentModuleId", "That module is not part of this course.");

            var row = await progressRepository.GetAll()
                .FirstOrDefaultAsync(p => p.TrainingEnrollmentId == enrollment.Id
                    && p.ContentModuleId == module.Id);

            var seconds = Math.Clamp(dto.AddSeconds, 0, MaxSecondsPerCall);
            if (row is null)
            {
                row = ModuleProgress.Start(enrollment.Id, module.Id);
                row.Record(seconds, dto.LastPosition, dto.Complete);
                if (string.IsNullOrEmpty(row.TenantId)) row.TenantId = enrollment.TenantId;
                await progressRepository.AddAsync(row);
            }
            else
            {
                row.Record(seconds, dto.LastPosition, dto.Complete);
                progressRepository.UpdateAsync(row);
            }
            await progressRepository.SaveChangesAsync();

            await CompleteIfFinishedAsync(enrollment, liveVersion);

            return await GetCoursePlayer.BuildAsync(enrollment.Id, enrollmentRepository, sessionRepository,
                courseRepository, versionRepository, progressRepository, userRepository, currentUser);
        }

        /// <summary>
        /// Completes the enrolment once every REQUIRED module of the live version is done.
        ///
        /// <para>⚠️ Attendance is set to 100 because the learner demonstrably worked through all of
        /// it; the assessment score is deliberately left NULL. There is no assessment in this phase,
        /// and writing a score nobody measured would put a fabricated number on a training record
        /// (phase 4 is what fills it).</para>
        /// </summary>
        private async Task CompleteIfFinishedAsync(TrainingEnrollment enrollment, CourseVersion version)
        {
            var required = version.Modules.Where(m => m.IsRequired).Select(m => m.Id).ToList();
            if (required.Count == 0) return;

            var done = await progressRepository.GetAll().AsNoTracking()
                .CountAsync(p => p.TrainingEnrollmentId == enrollment.Id
                    && required.Contains(p.ContentModuleId) && p.CompletedOn != null);
            if (done < required.Count) return;

            enrollment.RecordParticipation(TrainingEnrollmentStatus.Completed, 100m, null, DateTime.UtcNow);
            enrollmentRepository.UpdateAsync(enrollment);
            await enrollmentRepository.SaveChangesAsync();
            logger.LogInformation(
                "Enrollment {Id} completed automatically — all {Count} required module(s) of v{Version} finished",
                enrollment.Id, required.Count, version.VersionNumber);
        }
    }
}
