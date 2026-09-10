using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------

    /// <summary>One signature as it appears on a record or a printed copy.</summary>
    public class SignatureDto
    {
        public Guid Id { get; set; }
        public string Meaning { get; set; } = string.Empty;
        public string SignedByName { get; set; } = string.Empty;
        public DateTime SignedOn { get; set; }
        public string SignedStatement { get; set; } = string.Empty;
        public string? Note { get; set; }
        /// <summary>
        /// False when the record has changed since it was signed — the stored hash no longer matches
        /// the facts. Recomputed on every read rather than stored, because a stored "valid" flag is
        /// exactly the thing that would go stale.
        /// </summary>
        public bool Intact { get; set; }
    }

    /// <summary>A completion, everything asserted about it, and who has signed.</summary>
    public class SignableRecordDto
    {
        public Guid TrainingEnrollmentId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? EmployeeNumber { get; set; }
        public Guid TrainingCourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public int? CourseVersionNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedOn { get; set; }
        public decimal? AttendancePercent { get; set; }
        public decimal? AssessmentScore { get; set; }
        public Guid? AssignmentObligationId { get; set; }
        public string? AssignmentName { get; set; }
        /// <summary>The exact wording the learner is asked to agree to.</summary>
        public string StatementForSigning { get; set; } = string.Empty;
        public bool RequiresVerification { get; set; }
        public bool SignedByLearner { get; set; }
        public bool Verified { get; set; }
        /// <summary>Null when it can be signed; otherwise why not, in the signer's terms.</summary>
        public string? BlockedReason { get; set; }
        public List<SignatureDto> Signatures { get; set; } = [];
    }

    public class SignRecordDto
    {
        public Guid TrainingEnrollmentId { get; set; }
        /// <summary>Re-authentication. The signer's own password, at the moment of signing.</summary>
        public string Password { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IGetSignableRecord { Task<SignableRecordDto> GetAsync(Guid trainingEnrollmentId); }
    public interface IGetMySignableRecords { Task<List<SignableRecordDto>> GetAsync(); }
    public interface ISignTrainingRecord { Task<SignableRecordDto> SignAsync(SignRecordDto dto); }
    public interface IVerifyTrainingRecord { Task<SignableRecordDto> VerifyAsync(SignRecordDto dto); }

    // ---- The signed facts ---------------------------------------------------

    /// <summary>
    /// Builds the canonical statement of a training record and its hash.
    ///
    /// <para>⚠️ THE CANONICAL FORM MUST NEVER DRIFT. The hash stored with a signature is only
    /// meaningful while this function produces the same string for the same facts. Changing the
    /// field order, the separator or the date format silently invalidates every signature ever taken
    /// — they will read as tampered when nothing was tampered with. If a field must be added, it goes
    /// on the END and the version marker changes, so old signatures keep verifying under their own
    /// version (logic §12.90).</para>
    /// </summary>
    internal static class SignedFacts
    {
        /// <summary>Bumped only when the canonical form changes. Stored inside the hashed string.</summary>
        private const string CanonicalVersion = "v1";

        internal static string Canonical(SignableRecordDto r) => string.Join("|",
            CanonicalVersion,
            r.TrainingEnrollmentId.ToString("D"),
            r.EmployeeId.ToString("D"),
            r.TrainingCourseId.ToString("D"),
            r.CourseName,
            r.CourseVersionNumber?.ToString(CultureInfo.InvariantCulture) ?? "-",
            r.Status,
            r.CompletedOn?.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture) ?? "-",
            r.AttendancePercent?.ToString("F2", CultureInfo.InvariantCulture) ?? "-",
            r.AssessmentScore?.ToString("F2", CultureInfo.InvariantCulture) ?? "-",
            r.AssignmentObligationId?.ToString("D") ?? "-");

        internal static string Hash(SignableRecordDto r) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical(r))));

        /// <summary>The wording a learner is asked to agree to. Stored verbatim with the signature.</summary>
        internal static string LearnerStatement(SignableRecordDto r) =>
            $"I confirm that I completed '{r.CourseName}'" +
            (r.CourseVersionNumber.HasValue ? $" (version {r.CourseVersionNumber})" : "") +
            (r.CompletedOn.HasValue ? $" on {r.CompletedOn:dd MMM yyyy}" : "") +
            (r.AssessmentScore.HasValue ? $", scoring {r.AssessmentScore:0.#}%" : "") +
            ", and that this record is accurate.";

        internal static string VerifierStatement(SignableRecordDto r) =>
            $"I confirm that the training record for {r.EmployeeName} on '{r.CourseName}'" +
            (r.CompletedOn.HasValue ? $", completed {r.CompletedOn:dd MMM yyyy}" : "") +
            ", is accurate and complete.";
    }

    // ---- Loading ------------------------------------------------------------

    /// <summary>
    /// Assembles a training record from the facts that will be hashed, and reports who has signed.
    ///
    /// <para>Shared by every path here so the thing displayed, the thing signed and the thing
    /// verified are assembled by one piece of code. Three separate assemblers would drift, and a
    /// drifted assembler makes every existing signature read as tampered.</para>
    /// </summary>
    internal sealed class RecordLoader(
        IRepository<TrainingEnrollment> enrollments,
        IRepository<TrainingSession> sessions,
        IRepository<TrainingCourse> courses,
        IRepository<CourseVersion> versions,
        IRepository<Employee> employees,
        IRepository<AssignmentObligation> obligations,
        IRepository<LearningAssignment> assignments,
        IRepository<TrainingRecordSignature> signatures)
    {
        internal async Task<SignableRecordDto> LoadAsync(Guid trainingEnrollmentId)
        {
            var enrollment = await enrollments.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == trainingEnrollmentId)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), trainingEnrollmentId.ToString());

            var courseId = await sessions.GetAll().AsNoTracking()
                .Where(s => s.Id == enrollment.TrainingSessionId)
                .Select(s => s.TrainingCourseId).FirstOrDefaultAsync();

            var course = await courses.GetAll().AsNoTracking()
                .Where(c => c.Id == courseId)
                .Select(c => new { c.Name, c.Code })
                .FirstOrDefaultAsync();

            var person = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == enrollment.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .FirstOrDefaultAsync();

            // The version LIVE at completion is not recoverable after a later publish, so the
            // published one is reported and the fact that it is the current one is what the hash
            // pins. A course whose content is revised afterwards produces a new record, not a
            // silently altered one.
            var versionNumber = await versions.GetAll().AsNoTracking()
                .Where(v => v.TrainingCourseId == courseId && v.Status == CourseVersionStatus.Published)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => (int?)v.VersionNumber)
                .FirstOrDefaultAsync();

            var obligation = await obligations.GetAll().AsNoTracking()
                .Where(o => o.TrainingEnrollmentId == trainingEnrollmentId)
                .Select(o => new { o.Id, o.LearningAssignmentId })
                .FirstOrDefaultAsync();

            var assignment = obligation is null
                ? null
                : await assignments.GetAll().AsNoTracking()
                    .Where(a => a.Id == obligation.LearningAssignmentId)
                    .Select(a => new { a.Name, a.RequiresVerification })
                    .FirstOrDefaultAsync();

            var dto = new SignableRecordDto
            {
                TrainingEnrollmentId = enrollment.Id,
                EmployeeId = enrollment.EmployeeId,
                EmployeeName = person?.Name ?? "",
                EmployeeNumber = person?.EmployeeNumber,
                TrainingCourseId = courseId,
                CourseName = course?.Name ?? "",
                CourseCode = course?.Code,
                CourseVersionNumber = versionNumber,
                Status = enrollment.Status.ToString(),
                CompletedOn = enrollment.CompletedOn,
                AttendancePercent = enrollment.AttendancePercent,
                AssessmentScore = enrollment.AssessmentScore,
                AssignmentObligationId = obligation?.Id,
                AssignmentName = assignment?.Name,
                RequiresVerification = assignment?.RequiresVerification ?? false
            };
            dto.StatementForSigning = SignedFacts.LearnerStatement(dto);

            var currentHash = SignedFacts.Hash(dto);
            var taken = await signatures.GetAll().AsNoTracking()
                .Where(s => s.TrainingEnrollmentId == trainingEnrollmentId)
                .OrderBy(s => s.SignedOn)
                .ToListAsync();

            dto.Signatures = [.. taken.Select(s => new SignatureDto
            {
                Id = s.Id,
                Meaning = s.Meaning.ToString(),
                SignedByName = s.SignedByName,
                SignedOn = s.SignedOn,
                SignedStatement = s.SignedStatement,
                Note = s.Note,
                // Recomputed every read: this is the check that makes the signature mean something.
                Intact = string.Equals(s.ContentHash, currentHash, StringComparison.Ordinal)
            })];

            dto.SignedByLearner = taken.Any(s => s.Meaning == SignatureMeaning.Completion);
            dto.Verified = taken.Any(s => s.Meaning == SignatureMeaning.Verification);

            dto.BlockedReason =
                enrollment.Status != TrainingEnrollmentStatus.Completed
                    ? "This training is not completed yet."
                    : dto.SignedByLearner ? "You have already signed this record." : null;

            return dto;
        }
    }

    // ---- Handlers -----------------------------------------------------------

    public class GetSignableRecord(
        IRepository<TrainingEnrollment> enrollments,
        IRepository<TrainingSession> sessions,
        IRepository<TrainingCourse> courses,
        IRepository<CourseVersion> versions,
        IRepository<Employee> employees,
        IRepository<AssignmentObligation> obligations,
        IRepository<LearningAssignment> assignments,
        IRepository<TrainingRecordSignature> signatures,
        IPerformanceVisibilityService visibility,
        IRepository<User> users,
        ICurrentUserService currentUser) : IGetSignableRecord
    {
        public async Task<SignableRecordDto> GetAsync(Guid trainingEnrollmentId)
        {
            var record = await new RecordLoader(enrollments, sessions, courses, versions, employees,
                obligations, assignments, signatures).LoadAsync(trainingEnrollmentId);

            // Own record, or HR. A training record names a person and what they scored, so it is not
            // browsable by whoever holds the link.
            var mine = await PlayerAccess.MyEmployeeIdAsync(users, currentUser.GetCurrentUserId());
            if (mine == record.EmployeeId) return record;
            if ((await visibility.GetScopeAsync()).IsAdmin) return record;

            throw new ValidationException("access", "This is not your training record.");
        }
    }

    /// <summary>The signed-in learner's completed training, with what still needs their signature.</summary>
    public class GetMySignableRecords(
        IRepository<TrainingEnrollment> enrollments,
        IRepository<TrainingSession> sessions,
        IRepository<TrainingCourse> courses,
        IRepository<CourseVersion> versions,
        IRepository<Employee> employees,
        IRepository<AssignmentObligation> obligations,
        IRepository<LearningAssignment> assignments,
        IRepository<TrainingRecordSignature> signatures,
        IRepository<User> users,
        ICurrentUserService currentUser) : IGetMySignableRecords
    {
        public async Task<List<SignableRecordDto>> GetAsync()
        {
            var mine = await PlayerAccess.MyEmployeeIdAsync(users, currentUser.GetCurrentUserId());
            if (mine is null) return [];

            var ids = await enrollments.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == mine.Value && e.Status == TrainingEnrollmentStatus.Completed)
                .OrderByDescending(e => e.CompletedOn)
                .Select(e => e.Id)
                .Take(100)
                .ToListAsync();

            var loader = new RecordLoader(enrollments, sessions, courses, versions, employees,
                obligations, assignments, signatures);

            var result = new List<SignableRecordDto>();
            foreach (var id in ids) result.Add(await loader.LoadAsync(id));
            return result;
        }
    }

    /// <summary>
    /// The learner's own attestation.
    ///
    /// <para>⚠️ RE-AUTHENTICATION IS THE SIGNATURE. Part 11 wants a signing to be an act of identity,
    /// not a click by whoever happens to be at the keyboard — so the password is checked here even
    /// though the caller already has a session. Without it this is a button, not a signature
    /// (logic §12.90).</para>
    /// </summary>
    public class SignTrainingRecord(
        IRepository<TrainingEnrollment> enrollments,
        IRepository<TrainingSession> sessions,
        IRepository<TrainingCourse> courses,
        IRepository<CourseVersion> versions,
        IRepository<Employee> employees,
        IRepository<AssignmentObligation> obligations,
        IRepository<LearningAssignment> assignments,
        IRepository<TrainingRecordSignature> signatures,
        IRepository<User> users,
        ICurrentUserService currentUser,
        IAuthentication authentication,
        ILogger<SignTrainingRecord> logger) : ISignTrainingRecord
    {
        public async Task<SignableRecordDto> SignAsync(SignRecordDto dto)
        {
            var signer = await SignatureAuth.ReauthenticateAsync(users, currentUser, authentication, dto.Password);

            var loader = new RecordLoader(enrollments, sessions, courses, versions, employees,
                obligations, assignments, signatures);
            var record = await loader.LoadAsync(dto.TrainingEnrollmentId);

            if (signer.EmployeeId != record.EmployeeId)
                throw new ValidationException("access",
                    "Only the person the training belongs to can sign this record.");
            if (record.Status != nameof(TrainingEnrollmentStatus.Completed))
                throw new ValidationException("trainingEnrollmentId", "This training is not completed yet.");
            if (record.SignedByLearner)
                throw new ValidationException("trainingEnrollmentId", "You have already signed this record.");

            var signature = TrainingRecordSignature.Create(
                record.TrainingEnrollmentId, record.AssignmentObligationId, record.EmployeeId,
                SignatureMeaning.Completion, signer.UserId, signer.FullName,
                SignedFacts.Hash(record), SignedFacts.LearnerStatement(record), dto.Note);
            if (string.IsNullOrEmpty(signature.TenantId)) signature.TenantId = signer.TenantId;

            await signatures.AddAsync(signature);
            await signatures.SaveChangesAsync();
            logger.LogInformation("Training record {EnrollmentId} signed by {User} (completion)",
                record.TrainingEnrollmentId, signer.FullName);

            return await loader.LoadAsync(dto.TrainingEnrollmentId);
        }
    }

    /// <summary>
    /// A verifier's confirmation — HR, or whoever the deployment treats as quality oversight.
    ///
    /// <para>⚠️ A verifier cannot sign their OWN record. Self-verification is not oversight, and a
    /// record carrying two signatures from one person is weaker evidence than one carrying a single
    /// honest one.</para>
    /// </summary>
    public class VerifyTrainingRecord(
        IRepository<TrainingEnrollment> enrollments,
        IRepository<TrainingSession> sessions,
        IRepository<TrainingCourse> courses,
        IRepository<CourseVersion> versions,
        IRepository<Employee> employees,
        IRepository<AssignmentObligation> obligations,
        IRepository<LearningAssignment> assignments,
        IRepository<TrainingRecordSignature> signatures,
        IRepository<User> users,
        ICurrentUserService currentUser,
        IAuthentication authentication,
        IPerformanceVisibilityService visibility,
        ILogger<VerifyTrainingRecord> logger) : IVerifyTrainingRecord
    {
        public async Task<SignableRecordDto> VerifyAsync(SignRecordDto dto)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin)
                throw new ValidationException("access", "Only HR can verify a training record.");

            var signer = await SignatureAuth.ReauthenticateAsync(users, currentUser, authentication, dto.Password);

            var loader = new RecordLoader(enrollments, sessions, courses, versions, employees,
                obligations, assignments, signatures);
            var record = await loader.LoadAsync(dto.TrainingEnrollmentId);

            if (signer.EmployeeId == record.EmployeeId)
                throw new ValidationException("access", "You cannot verify your own training record.");
            if (!record.SignedByLearner)
                throw new ValidationException("trainingEnrollmentId",
                    "The learner has not signed this record yet.");
            if (record.Verified)
                throw new ValidationException("trainingEnrollmentId", "This record has already been verified.");

            var signature = TrainingRecordSignature.Create(
                record.TrainingEnrollmentId, record.AssignmentObligationId, record.EmployeeId,
                SignatureMeaning.Verification, signer.UserId, signer.FullName,
                SignedFacts.Hash(record), SignedFacts.VerifierStatement(record), dto.Note);
            if (string.IsNullOrEmpty(signature.TenantId)) signature.TenantId = signer.TenantId;

            await signatures.AddAsync(signature);
            await signatures.SaveChangesAsync();
            logger.LogInformation("Training record {EnrollmentId} verified by {User}",
                record.TrainingEnrollmentId, signer.FullName);

            return await loader.LoadAsync(dto.TrainingEnrollmentId);
        }
    }

    internal sealed record Signer(Guid UserId, string FullName, Guid? EmployeeId, string TenantId);

    internal static class SignatureAuth
    {
        /// <summary>
        /// Confirms the caller's password before a signing.
        ///
        /// <para>⚠️ The message is deliberately the same whether the account is missing or the
        /// password is wrong — a signing endpoint must not become a way to probe which accounts
        /// exist.</para>
        /// </summary>
        internal static async Task<Signer> ReauthenticateAsync(
            IRepository<User> users, ICurrentUserService currentUser,
            IAuthentication authentication, string password)
        {
            var userId = currentUser.GetCurrentUserId()
                ?? throw new ValidationException("password", "Sign in again before signing this record.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("password", "Enter your password to sign.");

            var user = await users.GetAll().AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null || !authentication.VerifyPassword(password, user.PasswordHash))
                throw new ValidationException("password", "That password is not correct.");

            return new Signer(user.Id, user.FullName, user.EmployeeId, user.TenantId);
        }
    }
}
