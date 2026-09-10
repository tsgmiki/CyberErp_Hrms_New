using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- Learner-facing DTOs ------------------------------------------------
    //
    // ⚠️ NOTHING IN THIS SECTION CARRIES THE ANSWER KEY while an attempt can still be improved.
    // `IsCorrect` on an option, `CorrectOptionIds` and `Explanation` are populated only once the
    // attempt is submitted AND the learner can no longer retake it. A quiz whose correct answers
    // are readable in the page certifies nothing (logic §12.87).

    public class AttemptOptionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        /// <summary>Only ever true on a revealed result. Absent (false) while the quiz is live.</summary>
        public bool IsCorrect { get; set; }
    }

    public class AttemptQuestionDto
    {
        public Guid Id { get; set; }
        public int SortOrder { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public decimal Points { get; set; }
        /// <summary>Only on a revealed result.</summary>
        public string? Explanation { get; set; }
        public List<AttemptOptionDto> Options { get; set; } = [];
        /// <summary>What the learner picked — filled when resuming or reviewing.</summary>
        public List<Guid> SelectedOptionIds { get; set; } = [];
        /// <summary>Null until the attempt is graded.</summary>
        public bool? IsCorrect { get; set; }
        public decimal? PointsAwarded { get; set; }
    }

    public class AttemptDto
    {
        public Guid Id { get; set; }
        public Guid AssessmentId { get; set; }
        public Guid TrainingEnrollmentId { get; set; }
        public Guid ContentModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public decimal PassMark { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public int AttemptNumber { get; set; }
        public int? MaxAttempts { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartedOn { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public decimal? ScorePercent { get; set; }
        public decimal? PointsAwarded { get; set; }
        public decimal? PointsPossible { get; set; }
        public bool? Passed { get; set; }
        /// <summary>True when the answer key is included below — see the section note.</summary>
        public bool AnswersRevealed { get; set; }
        /// <summary>Set when this submission completed the whole course.</summary>
        public bool CourseCompleted { get; set; }
        public List<AttemptQuestionDto> Questions { get; set; } = [];
    }

    /// <summary>How a quiz module stands for one learner, shown on the module before they open it.</summary>
    public class AssessmentStateDto
    {
        public Guid AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal PassMark { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public int QuestionCount { get; set; }
        public decimal TotalPoints { get; set; }
        public int? MaxAttempts { get; set; }
        public int AttemptsUsed { get; set; }
        /// <summary>Null when attempts are unlimited.</summary>
        public int? AttemptsLeft { get; set; }
        public decimal? BestScore { get; set; }
        public bool Passed { get; set; }
        /// <summary>An unfinished attempt to resume, if there is one.</summary>
        public Guid? InProgressAttemptId { get; set; }
        /// <summary>Why the quiz cannot be started right now, in the learner's terms.</summary>
        public string? BlockedReason { get; set; }
    }

    public class SubmitAttemptAnswerDto
    {
        public Guid QuestionId { get; set; }
        public List<Guid> SelectedOptionIds { get; set; } = [];
    }

    public class SubmitAttemptDto
    {
        public Guid AttemptId { get; set; }
        public List<SubmitAttemptAnswerDto> Answers { get; set; } = [];
    }

    public class StartAttemptDto
    {
        public Guid TrainingEnrollmentId { get; set; }
        public Guid AssessmentId { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IStartAssessmentAttempt { Task<AttemptDto> StartAsync(StartAttemptDto dto); }
    public interface IGetAssessmentAttempt { Task<AttemptDto> GetAsync(Guid attemptId); }
    public interface ISubmitAssessmentAttempt { Task<AttemptDto> SubmitAsync(SubmitAttemptDto dto); }

    // ---- Shared -------------------------------------------------------------

    internal static class AttemptShared
    {
        /// <summary>
        /// The one place that decides whether a learner may see the answer key: the attempt is
        /// finished, the assessment allows it, and there is nothing left to gain — they passed, or
        /// they are out of retakes. Revealing after a failure with attempts remaining would hand
        /// them the answers to the next go.
        /// </summary>
        internal static bool MayReveal(Assessment assessment, AssessmentAttempt attempt, int attemptsUsed)
        {
            if (attempt.Status == AttemptStatus.InProgress) return false;
            if (!assessment.RevealAnswers) return false;
            if (attempt.Passed == true) return true;
            return assessment.MaxAttempts.HasValue && attemptsUsed >= assessment.MaxAttempts.Value;
        }

        /// <summary>
        /// A stable shuffle: the same attempt always sees the same order, so resuming does not
        /// scramble the paper mid-exam, while two learners get different orders.
        /// </summary>
        internal static IEnumerable<Question> Order(Assessment assessment, IEnumerable<Question> questions, Guid attemptId)
        {
            if (!assessment.ShuffleQuestions) return questions.OrderBy(q => q.SortOrder);
            var seed = attemptId.GetHashCode();
            return questions.OrderBy(q => unchecked(q.Id.GetHashCode() * 397) ^ seed).ThenBy(q => q.Id);
        }

        internal static AttemptDto ToDto(Assessment assessment, AssessmentAttempt attempt, int attemptsUsed,
            IReadOnlyDictionary<Guid, AttemptAnswer> answers)
        {
            var reveal = MayReveal(assessment, attempt, attemptsUsed);

            var dto = new AttemptDto
            {
                Id = attempt.Id,
                AssessmentId = assessment.Id,
                TrainingEnrollmentId = attempt.TrainingEnrollmentId,
                ContentModuleId = assessment.ContentModuleId,
                Title = assessment.Title,
                Instructions = assessment.Instructions,
                PassMark = assessment.PassMark,
                TimeLimitMinutes = assessment.TimeLimitMinutes,
                AttemptNumber = attempt.AttemptNumber,
                MaxAttempts = assessment.MaxAttempts,
                Status = attempt.Status.ToString(),
                StartedOn = attempt.StartedOn,
                SubmittedOn = attempt.SubmittedOn,
                ScorePercent = attempt.ScorePercent,
                PointsAwarded = attempt.PointsAwarded,
                PointsPossible = attempt.PointsPossible,
                Passed = attempt.Passed,
                AnswersRevealed = reveal
            };

            foreach (var q in Order(assessment, assessment.Questions, attempt.Id))
            {
                answers.TryGetValue(q.Id, out var answer);
                dto.Questions.Add(new AttemptQuestionDto
                {
                    Id = q.Id,
                    SortOrder = dto.Questions.Count + 1,
                    Text = q.Text,
                    Kind = q.Kind.ToString(),
                    Points = q.Points,
                    Explanation = reveal ? q.Explanation : null,
                    IsCorrect = answer?.IsCorrect,
                    PointsAwarded = answer?.PointsAwarded,
                    SelectedOptionIds = [.. answer?.Selected.Select(s => s.QuestionOptionId) ?? []],
                    Options = [.. q.Options.OrderBy(o => o.SortOrder).Select(o => new AttemptOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        // The answer key, gated on the one rule above.
                        IsCorrect = reveal && o.IsCorrect
                    })]
                });
            }

            return dto;
        }
    }

    /// <summary>
    /// Loads an assessment with everything the attempt path needs, and enforces that the enrolment
    /// belongs to the caller.
    /// </summary>
    internal class AttemptContext
    {
        internal required Assessment Assessment { get; init; }
        internal required TrainingEnrollment Enrollment { get; init; }
        internal required int AttemptsUsed { get; init; }
        internal required bool AlreadyPassed { get; init; }
    }

    internal static class AttemptAccess
    {
        internal static async Task<AttemptContext> LoadAsync(
            IRepository<Assessment> assessments,
            IRepository<TrainingEnrollment> enrollments,
            IRepository<AssessmentAttempt> attempts,
            IRepository<User> users,
            ICurrentUserService currentUser,
            Guid assessmentId,
            Guid trainingEnrollmentId,
            bool trackEnrollment)
        {
            var myEmployeeId = await PlayerAccess.MyEmployeeIdAsync(users, currentUser.GetCurrentUserId());

            var enrollmentQuery = trackEnrollment
                ? enrollments.GetAll()
                : enrollments.GetAll().AsNoTracking();
            var enrollment = await enrollmentQuery.FirstOrDefaultAsync(e => e.Id == trainingEnrollmentId)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), trainingEnrollmentId.ToString());

            if (myEmployeeId is null || enrollment.EmployeeId != myEmployeeId.Value)
                throw new ValidationException("access", "This is not your enrolment.");

            var assessment = await assessments.GetAll().AsNoTracking()
                    .Include(a => a.Questions).ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(a => a.Id == assessmentId)
                ?? throw new NotFoundException(nameof(Assessment), assessmentId.ToString());

            var prior = await attempts.GetAll().AsNoTracking()
                .Where(a => a.AssessmentId == assessmentId && a.TrainingEnrollmentId == trainingEnrollmentId)
                .Select(a => new { a.Status, a.Passed })
                .ToListAsync();

            return new AttemptContext
            {
                Assessment = assessment,
                Enrollment = enrollment,
                // An unfinished attempt still counts against the limit — otherwise "start, peek,
                // abandon" is an unlimited supply of question papers.
                AttemptsUsed = prior.Count,
                AlreadyPassed = prior.Any(a => a.Passed == true)
            };
        }
    }

    // ---- Starting -----------------------------------------------------------

    public class StartAssessmentAttempt(
        IRepository<Assessment> assessmentRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<AssessmentAttempt> attemptRepository,
        IRepository<AttemptAnswer> answerRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser,
        ILogger<StartAssessmentAttempt> logger) : IStartAssessmentAttempt
    {
        public async Task<AttemptDto> StartAsync(StartAttemptDto dto)
        {
            var ctx = await AttemptAccess.LoadAsync(assessmentRepository, enrollmentRepository,
                attemptRepository, userRepository, currentUser, dto.AssessmentId, dto.TrainingEnrollmentId, false);

            // Resume rather than start again: reloading the page mid-quiz must not burn a retake.
            var open = await attemptRepository.GetAll()
                .FirstOrDefaultAsync(a => a.AssessmentId == dto.AssessmentId
                    && a.TrainingEnrollmentId == dto.TrainingEnrollmentId
                    && a.Status == AttemptStatus.InProgress);
            if (open is not null)
                return AttemptShared.ToDto(ctx.Assessment, open, ctx.AttemptsUsed, await AnswersOf(open.Id));

            if (ctx.Enrollment.Status != TrainingEnrollmentStatus.Enrolled)
                throw new ValidationException("trainingEnrollmentId",
                    $"This enrolment is {ctx.Enrollment.Status} — its quizzes are closed.");
            if (ctx.Assessment.Questions.Count == 0)
                throw new ValidationException("assessmentId", "This quiz has no questions yet.");
            if (ctx.AlreadyPassed)
                throw new ValidationException("assessmentId", "You have already passed this quiz.");
            if (ctx.Assessment.MaxAttempts.HasValue && ctx.AttemptsUsed >= ctx.Assessment.MaxAttempts.Value)
                throw new ValidationException("assessmentId",
                    $"You have used all {ctx.Assessment.MaxAttempts.Value} attempt(s) at this quiz.");

            var attempt = AssessmentAttempt.Start(dto.AssessmentId, dto.TrainingEnrollmentId, ctx.AttemptsUsed + 1);
            if (string.IsNullOrEmpty(attempt.TenantId)) attempt.TenantId = ctx.Enrollment.TenantId;
            await attemptRepository.AddAsync(attempt);
            await attemptRepository.SaveChangesAsync();

            logger.LogInformation("Attempt {Number} started on assessment {AssessmentId} for enrolment {EnrollmentId}",
                attempt.AttemptNumber, dto.AssessmentId, dto.TrainingEnrollmentId);

            return AttemptShared.ToDto(ctx.Assessment, attempt, ctx.AttemptsUsed + 1, new Dictionary<Guid, AttemptAnswer>());
        }

        private async Task<Dictionary<Guid, AttemptAnswer>> AnswersOf(Guid attemptId)
        {
            var rows = await answerRepository.GetAll().AsNoTracking()
                .Include(a => a.Selected)
                .Where(a => a.AssessmentAttemptId == attemptId)
                .ToListAsync();
            return rows.ToDictionary(a => a.QuestionId);
        }
    }

    // ---- Reading ------------------------------------------------------------

    public class GetAssessmentAttempt(
        IRepository<Assessment> assessmentRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<AssessmentAttempt> attemptRepository,
        IRepository<AttemptAnswer> answerRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser) : IGetAssessmentAttempt
    {
        public async Task<AttemptDto> GetAsync(Guid attemptId)
        {
            var attempt = await attemptRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == attemptId)
                ?? throw new NotFoundException(nameof(AssessmentAttempt), attemptId.ToString());

            var ctx = await AttemptAccess.LoadAsync(assessmentRepository, enrollmentRepository,
                attemptRepository, userRepository, currentUser,
                attempt.AssessmentId, attempt.TrainingEnrollmentId, false);

            var answers = await answerRepository.GetAll().AsNoTracking()
                .Include(a => a.Selected)
                .Where(a => a.AssessmentAttemptId == attemptId)
                .ToListAsync();

            return AttemptShared.ToDto(ctx.Assessment, attempt, ctx.AttemptsUsed,
                answers.ToDictionary(a => a.QuestionId));
        }
    }

    // ---- Submitting and grading ---------------------------------------------

    /// <summary>
    /// Grades an attempt and, on a pass, completes the quiz module — which can in turn complete the
    /// whole enrolment.
    ///
    /// <para>⚠️ GRADING IS SERVER-SIDE AND THE CLIENT SENDS ONLY CHOICES. The learner's browser never
    /// holds the key, and nothing it posts is trusted beyond "these option ids were ticked": option
    /// ids that do not belong to the question are discarded rather than credited.</para>
    ///
    /// <para>⚠️ Multiple-choice is graded ALL OR NOTHING — the ticked set must equal the correct set.
    /// Partial credit needs a scheme (per-option? negative marking for wrong ticks?) that is a policy
    /// choice nobody has made, and picking one silently would put an arbitrary number on a
    /// certificate.</para>
    /// </summary>
    public class SubmitAssessmentAttempt(
        IRepository<Assessment> assessmentRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<AssessmentAttempt> attemptRepository,
        IRepository<AttemptAnswer> answerRepository,
        IRepository<ModuleProgress> progressRepository,
        IRepository<CourseVersion> versionRepository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUser,
        ILogger<SubmitAssessmentAttempt> logger) : ISubmitAssessmentAttempt
    {
        public async Task<AttemptDto> SubmitAsync(SubmitAttemptDto dto)
        {
            var attempt = await attemptRepository.GetAll()
                    .FirstOrDefaultAsync(a => a.Id == dto.AttemptId)
                ?? throw new NotFoundException(nameof(AssessmentAttempt), dto.AttemptId.ToString());

            if (attempt.Status != AttemptStatus.InProgress)
                throw new ValidationException("attemptId", "This attempt has already been submitted.");

            var ctx = await AttemptAccess.LoadAsync(assessmentRepository, enrollmentRepository,
                attemptRepository, userRepository, currentUser,
                attempt.AssessmentId, attempt.TrainingEnrollmentId, true);

            var assessment = ctx.Assessment;
            var posted = (dto.Answers ?? []).GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.SelectMany(a => a.SelectedOptionIds ?? []).Distinct().ToList());

            var graded = new List<AttemptAnswer>();
            decimal awarded = 0m, possible = 0m;

            foreach (var question in assessment.Questions)
            {
                possible += question.Points;

                posted.TryGetValue(question.Id, out var rawSelection);
                // Only ids that really belong to this question survive: a client posting another
                // question's option id must not be able to change what is graded.
                var valid = question.Options.Select(o => o.Id).ToHashSet();
                var selected = (rawSelection ?? []).Where(valid.Contains).ToList();
                if (selected.Count == 0) continue;   // unanswered scores nothing and records nothing

                var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                var isCorrect = question.Kind == QuestionKind.MultipleChoice
                    ? selected.Count == correctIds.Count && selected.All(correctIds.Contains)
                    // Single-choice and true/false: exactly one tick, and it is the right one.
                    : selected.Count == 1 && correctIds.Contains(selected[0]);

                var points = isCorrect ? question.Points : 0m;
                awarded += points;

                var answer = AttemptAnswer.Create(attempt.Id, question.Id, selected, isCorrect, points);
                if (string.IsNullOrEmpty(answer.TenantId)) answer.TenantId = attempt.TenantId;
                foreach (var s in answer.Selected)
                    if (string.IsNullOrEmpty(s.TenantId)) s.TenantId = attempt.TenantId;
                graded.Add(answer);
            }

            var timedOut = attempt.IsOverdue(assessment.TimeLimitMinutes, DateTime.UtcNow);
            attempt.Submit(graded, awarded, possible, assessment.PassMark, timedOut);

            foreach (var answer in graded) await answerRepository.AddAsync(answer);
            attemptRepository.UpdateAsync(attempt);
            await attemptRepository.SaveChangesAsync();

            logger.LogInformation(
                "Attempt {Id} submitted: {Awarded}/{Possible} = {Score}% against a pass mark of {Pass}% — {Result}{TimedOut}",
                attempt.Id, awarded, possible, attempt.ScorePercent, assessment.PassMark,
                attempt.Passed == true ? "PASS" : "FAIL", timedOut ? " (late)" : "");

            // The measured score lands on the enrolment — this is the value phase 3 left NULL. The
            // entity keeps the best of the attempts, so a worse retake cannot erase an earned pass.
            var enrollment = ctx.Enrollment;
            enrollment.RecordAssessmentResult(attempt.ScorePercent ?? 0m);
            enrollmentRepository.UpdateAsync(enrollment);
            await enrollmentRepository.SaveChangesAsync();

            var courseCompleted = false;
            if (attempt.Passed == true)
                courseCompleted = await CompleteQuizModuleAsync(enrollment, assessment.ContentModuleId);

            var answersById = graded.ToDictionary(a => a.QuestionId);
            var result = AttemptShared.ToDto(assessment, attempt, ctx.AttemptsUsed, answersById);
            result.CourseCompleted = courseCompleted;
            return result;
        }

        /// <summary>
        /// Passing IS completing the quiz module. The learner never marks a quiz complete by hand —
        /// <c>RecordModuleProgress</c> refuses that — so this is the only way a Quiz module closes.
        /// </summary>
        /// <returns>True when this was the last required module and the enrolment completed.</returns>
        private async Task<bool> CompleteQuizModuleAsync(TrainingEnrollment enrollment, Guid contentModuleId)
        {
            var row = await progressRepository.GetAll()
                .FirstOrDefaultAsync(p => p.TrainingEnrollmentId == enrollment.Id
                    && p.ContentModuleId == contentModuleId);

            if (row is null)
            {
                row = ModuleProgress.Start(enrollment.Id, contentModuleId);
                row.Record(0, null, true);
                if (string.IsNullOrEmpty(row.TenantId)) row.TenantId = enrollment.TenantId;
                await progressRepository.AddAsync(row);
            }
            else
            {
                row.Record(0, null, true);
                progressRepository.UpdateAsync(row);
            }
            await progressRepository.SaveChangesAsync();

            var version = await EnrollmentCompletion.LiveVersionAsync(
                sessionRepository, versionRepository, enrollment.TrainingSessionId);
            if (version is null) return false;

            return await EnrollmentCompletion.CompleteIfFinishedAsync(
                enrollment, version, enrollmentRepository, progressRepository, logger);
        }
    }
}
