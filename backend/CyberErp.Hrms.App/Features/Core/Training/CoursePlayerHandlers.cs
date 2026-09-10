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

    /// <summary>
    /// The rule that decides when an enrolment is finished, in one place because two paths reach it:
    /// a learner marking a content module done, and a learner PASSING a quiz.
    /// </summary>
    internal static class EnrollmentCompletion
    {
        /// <summary>The published version a session's course is currently serving, if any.</summary>
        internal static async Task<CourseVersion?> LiveVersionAsync(
            IRepository<TrainingSession> sessions,
            IRepository<CourseVersion> versions,
            Guid trainingSessionId)
        {
            var courseId = await sessions.GetAll().AsNoTracking()
                .Where(s => s.Id == trainingSessionId)
                .Select(s => s.TrainingCourseId)
                .FirstOrDefaultAsync();
            if (courseId == Guid.Empty) return null;

            return await versions.GetAll().AsNoTracking()
                .Include(v => v.Modules)
                .Where(v => v.TrainingCourseId == courseId && v.Status == CourseVersionStatus.Published)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Completes the enrolment once every REQUIRED module of the live version is done.
        ///
        /// <para>⚠️ Attendance is set to 100 because the learner demonstrably worked through all of
        /// it. The assessment score is NOT written here — it is recorded by the attempt handler from
        /// a measured result, and a course with no quiz simply keeps a null score rather than a
        /// number nobody measured (logic §12.87).</para>
        /// </summary>
        internal static async Task<bool> CompleteIfFinishedAsync(
            TrainingEnrollment enrollment,
            CourseVersion version,
            IRepository<TrainingEnrollment> enrollments,
            IRepository<ModuleProgress> progress,
            ILogger logger)
        {
            if (enrollment.Status != TrainingEnrollmentStatus.Enrolled) return false;

            var required = version.Modules.Where(m => m.IsRequired).Select(m => m.Id).ToList();
            if (required.Count == 0) return false;

            var done = await progress.GetAll().AsNoTracking()
                .CountAsync(p => p.TrainingEnrollmentId == enrollment.Id
                    && required.Contains(p.ContentModuleId) && p.CompletedOn != null);
            if (done < required.Count) return false;

            enrollment.RecordParticipation(TrainingEnrollmentStatus.Completed, 100m,
                enrollment.AssessmentScore, DateTime.UtcNow);
            enrollments.UpdateAsync(enrollment);
            await enrollments.SaveChangesAsync();
            logger.LogInformation(
                "Enrollment {Id} completed automatically — all {Count} required module(s) of v{Version} finished",
                enrollment.Id, required.Count, version.VersionNumber);
            return true;
        }
    }

    /// <summary>
    /// Builds the player view. A class rather than a static because it needs seven repositories and
    /// both handlers below render the same thing.
    /// </summary>
    internal sealed class PlayerBuilder(
        IRepository<TrainingEnrollment> enrollments,
        IRepository<TrainingSession> sessions,
        IRepository<TrainingCourse> courses,
        IRepository<CourseVersion> versions,
        IRepository<ModuleProgress> progress,
        IRepository<Assessment> assessments,
        IRepository<AssessmentAttempt> attempts,
        IRepository<User> users,
        ICurrentUserService currentUser)
    {
        internal async Task<CoursePlayerDto> BuildAsync(Guid trainingEnrollmentId)
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
            var progressRows = await progress.GetAll().AsNoTracking()
                .Where(p => p.TrainingEnrollmentId == enrollment.Id && moduleIds.Contains(p.ContentModuleId))
                .Select(p => new { p.ContentModuleId, p.CompletedOn, p.SecondsSpent, p.LastPosition })
                .ToListAsync();

            var quizStates = await QuizStatesAsync(version, enrollment.Id);

            dto.Modules = [.. version.Modules.OrderBy(m => m.SortOrder).Select(m =>
            {
                var p = progressRows.FirstOrDefault(x => x.ContentModuleId == m.Id);
                quizStates.TryGetValue(m.Id, out var quiz);
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
                    LastPosition = p?.LastPosition,
                    Assessment = quiz
                };
            })];

            dto.RequiredTotal = dto.Modules.Count(m => m.IsRequired);
            dto.RequiredDone = dto.Modules.Count(m => m.IsRequired && m.IsComplete);
            dto.PercentComplete = dto.RequiredTotal == 0
                ? 0
                : (int)Math.Round(dto.RequiredDone * 100m / dto.RequiredTotal);
            return dto;
        }

        /// <summary>
        /// How each quiz module stands for this learner — attempts used, best score, whether they
        /// may start another. Read in two batched queries rather than per module, so a course with
        /// several quizzes does not fan out.
        /// </summary>
        private async Task<Dictionary<Guid, AssessmentStateDto>> QuizStatesAsync(
            CourseVersion version, Guid enrollmentId)
        {
            var quizModuleIds = version.Modules
                .Where(m => m.Kind == ContentModuleKind.Quiz).Select(m => m.Id).ToList();
            if (quizModuleIds.Count == 0) return [];

            var quizzes = await assessments.GetAll().AsNoTracking()
                .Include(a => a.Questions)
                .Where(a => quizModuleIds.Contains(a.ContentModuleId))
                .ToListAsync();
            if (quizzes.Count == 0) return [];

            var quizIds = quizzes.Select(a => a.Id).ToList();
            var rows = await attempts.GetAll().AsNoTracking()
                .Where(a => a.TrainingEnrollmentId == enrollmentId && quizIds.Contains(a.AssessmentId))
                .Select(a => new { a.Id, a.AssessmentId, a.Status, a.Passed, a.ScorePercent })
                .ToListAsync();

            var states = new Dictionary<Guid, AssessmentStateDto>();
            foreach (var quiz in quizzes)
            {
                var mine = rows.Where(r => r.AssessmentId == quiz.Id).ToList();
                var passed = mine.Any(r => r.Passed == true);
                var used = mine.Count;

                var state = new AssessmentStateDto
                {
                    AssessmentId = quiz.Id,
                    Title = quiz.Title,
                    PassMark = quiz.PassMark,
                    TimeLimitMinutes = quiz.TimeLimitMinutes,
                    QuestionCount = quiz.Questions.Count,
                    TotalPoints = quiz.TotalPoints,
                    MaxAttempts = quiz.MaxAttempts,
                    AttemptsUsed = used,
                    AttemptsLeft = quiz.MaxAttempts.HasValue
                        ? Math.Max(0, quiz.MaxAttempts.Value - used)
                        : null,
                    BestScore = mine.Count == 0 ? null : mine.Max(r => r.ScorePercent),
                    Passed = passed,
                    InProgressAttemptId = mine.FirstOrDefault(r => r.Status == AttemptStatus.InProgress)?.Id
                };

                // The same reasons StartAssessmentAttempt would refuse, said before the click rather
                // than after it — a dead button with no explanation is the thing to avoid.
                state.BlockedReason =
                    quiz.Questions.Count == 0 ? "This quiz has no questions yet."
                    : passed ? "You have already passed this quiz."
                    : state.AttemptsLeft == 0 ? $"You have used all {quiz.MaxAttempts} attempt(s)."
                    : null;

                states[quiz.ContentModuleId] = state;
            }
            return states;
        }
    }

    public class GetCoursePlayer(
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<CourseVersion> versionRepository,
        IRepository<ModuleProgress> progressRepository,
        IRepository<Assessment> assessmentRepository,
        IRepository<AssessmentAttempt> attemptRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser) : IGetCoursePlayer
    {
        public Task<CoursePlayerDto> GetAsync(Guid trainingEnrollmentId) =>
            new PlayerBuilder(enrollmentRepository, sessionRepository, courseRepository, versionRepository,
                progressRepository, assessmentRepository, attemptRepository, userRepository, currentUser)
                .BuildAsync(trainingEnrollmentId);
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
        IRepository<Assessment> assessmentRepository,
        IRepository<AssessmentAttempt> attemptRepository,
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

            // ⚠️ A quiz is completed by PASSING it, never by asserting it. Without this the whole of
            // phase 4 is decorative: any learner could post `complete: true` against the quiz module
            // and finish the course without answering a question (logic §12.87).
            if (module.Kind == ContentModuleKind.Quiz && dto.Complete)
                throw new ValidationException("contentModuleId",
                    "A quiz is completed by passing it. Open the quiz and submit an attempt.");

            // The module must belong to the version this enrolment's course actually publishes —
            // otherwise a learner could post progress against any module id in the tenant.
            var liveVersion = await EnrollmentCompletion.LiveVersionAsync(
                    sessionRepository, versionRepository, enrollment.TrainingSessionId)
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

            await EnrollmentCompletion.CompleteIfFinishedAsync(
                enrollment, liveVersion, enrollmentRepository, progressRepository, logger);

            return await new PlayerBuilder(enrollmentRepository, sessionRepository, courseRepository,
                    versionRepository, progressRepository, assessmentRepository, attemptRepository,
                    userRepository, currentUser)
                .BuildAsync(enrollment.Id);
        }
    }
}
