using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class TrainingEnrollmentDto
    {
        public Guid Id { get; set; }
        public Guid TrainingSessionId { get; set; }
        public string? CourseName { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public string? SessionStatus { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid? TrainingNeedId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? AttendancePercent { get; set; }
        public decimal? AssessmentScore { get; set; }
        public DateTime? CompletedOn { get; set; }
        public int? FeedbackRating { get; set; }
        public string? FeedbackComments { get; set; }
        public decimal CpdHours { get; set; }
    }

    public class EnrollTrainingDto
    {
        public Guid TrainingSessionId { get; set; }
        public Guid EmployeeId { get; set; }
        /// <summary>The approved need this enrollment fulfils (HC188) — optional.</summary>
        public Guid? TrainingNeedId { get; set; }
    }

    public class RecordParticipationDto
    {
        public Guid Id { get; set; }
        /// <summary>Enrolled | Completed | NoShow.</summary>
        public string Status { get; set; } = nameof(TrainingEnrollmentStatus.Enrolled);
        public decimal? AttendancePercent { get; set; }
        public decimal? AssessmentScore { get; set; }
        public DateTime? CompletedOn { get; set; }
    }

    public class TrainingFeedbackDto
    {
        public int Rating { get; set; }
        public string? Comments { get; set; }
    }

    public class RecordParticipationDtoValidator : AbstractValidator<RecordParticipationDto>
    {
        public RecordParticipationDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Status)
                .Must(v => Enum.TryParse<TrainingEnrollmentStatus>(v, true, out var st)
                    && st != TrainingEnrollmentStatus.Withdrawn)
                .WithMessage("Status must be Enrolled, Completed or NoShow (withdraw via its own action).");
            RuleFor(x => x.AttendancePercent).InclusiveBetween(0, 100).When(x => x.AttendancePercent.HasValue);
            RuleFor(x => x.AssessmentScore).InclusiveBetween(0, 100).When(x => x.AssessmentScore.HasValue);
        }
    }

    public class TrainingFeedbackDtoValidator : AbstractValidator<TrainingFeedbackDto>
    {
        public TrainingFeedbackDtoValidator()
        {
            RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
            RuleFor(x => x.Comments).MaximumLength(2000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IEnrollTraining { Task<Guid> EnrollAsync(EnrollTrainingDto dto); }
    public interface IRecordTrainingParticipation { Task RecordAsync(RecordParticipationDto dto); }
    public interface ISubmitTrainingFeedback { Task SubmitAsync(Guid id, TrainingFeedbackDto dto); }
    public interface IWithdrawTrainingEnrollment { Task WithdrawAsync(Guid id); }
    public interface IDeleteTrainingEnrollment { Task DeleteAsync(Guid id); }
    public interface IGetAllTrainingEnrollments { Task<PaginatedResponse<TrainingEnrollmentDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class EnrollTraining(
        IRepository<TrainingEnrollment> repository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingNeed> needRepository,
        IPerformanceVisibilityService visibility,
        ILogger<EnrollTraining> logger) : IEnrollTraining
    {
        public async Task<Guid> EnrollAsync(EnrollTrainingDto dto)
        {
            // Self-enrollment, a manager for their subtree, or HR for anyone (HC198).
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(dto.EmployeeId), "The employee is outside your scope.");

            var session = await sessionRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.TrainingSessionId)
                ?? throw new NotFoundException(nameof(TrainingSession), dto.TrainingSessionId.ToString());
            if (session.Status != TrainingSessionStatus.Scheduled)
                throw new ValidationException(nameof(dto.TrainingSessionId), $"Enrollment is only open on a scheduled session (current: {session.Status}).");

            if (await repository.GetAll().AnyAsync(e => e.TrainingSessionId == dto.TrainingSessionId
                    && e.EmployeeId == dto.EmployeeId && e.Status != TrainingEnrollmentStatus.Withdrawn))
                throw new ValidationException(nameof(dto.EmployeeId), "The employee is already enrolled in this session.");

            // Capacity guard — withdrawn seats free up.
            if (session.MaxParticipants.HasValue)
            {
                var taken = await repository.GetAll().CountAsync(e => e.TrainingSessionId == dto.TrainingSessionId
                    && e.Status != TrainingEnrollmentStatus.Withdrawn);
                if (taken >= session.MaxParticipants.Value)
                    throw new ValidationException(nameof(dto.TrainingSessionId),
                        $"The session is full ({taken}/{session.MaxParticipants}).");
            }

            if (dto.TrainingNeedId.HasValue)
            {
                var need = await needRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(n => n.Id == dto.TrainingNeedId.Value)
                    ?? throw new NotFoundException(nameof(TrainingNeed), dto.TrainingNeedId.Value.ToString());
                if (need.EmployeeId != dto.EmployeeId)
                    throw new ValidationException(nameof(dto.TrainingNeedId), "The linked need belongs to a different employee.");
                if (need.Status != TrainingNeedStatus.Approved)
                    throw new ValidationException(nameof(dto.TrainingNeedId), $"Only an approved need can be fulfilled (current: {need.Status}).");
            }

            // Re-joining after a withdrawal reuses the row (the unique (session, employee) index allows one).
            var existing = await repository.GetAll().FirstOrDefaultAsync(e =>
                e.TrainingSessionId == dto.TrainingSessionId && e.EmployeeId == dto.EmployeeId);
            if (existing is not null)
            {
                existing.Rejoin();
                repository.UpdateAsync(existing);
                await repository.SaveChangesAsync();
                logger.LogInformation("Re-enrolled employee {Employee} in session {Session}", dto.EmployeeId, dto.TrainingSessionId);
                return existing.Id;
            }

            var created = TrainingEnrollment.Create(dto.TrainingSessionId, dto.EmployeeId, dto.TrainingNeedId);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Enrolled employee {Employee} in session {Session}", dto.EmployeeId, dto.TrainingSessionId);
            return created.Id;
        }
    }

    public class RecordTrainingParticipation(
        IRepository<TrainingEnrollment> repository,
        IRepository<TrainingNeed> needRepository,
        IPerformanceVisibilityService visibility,
        IValidator<RecordParticipationDto> validator,
        ILogger<RecordTrainingParticipation> logger) : IRecordTrainingParticipation
    {
        public async Task RecordAsync(RecordParticipationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), dto.Id.ToString());

            // Attendance/scores are recorded by HR or the employee's manager — never the participant.
            var scope = await visibility.GetScopeAsync();
            var isManagerOfEmployee = scope.IsManager && entity.EmployeeId != scope.EmployeeId
                && await visibility.CanAccessEmployeeAsync(entity.EmployeeId);
            if (!scope.IsAdmin && !isManagerOfEmployee)
                throw new ValidationException(nameof(dto.Id), "Only HR or the employee's manager can record participation.");

            var status = Enum.Parse<TrainingEnrollmentStatus>(dto.Status, true);
            entity.RecordParticipation(status, dto.AttendancePercent, dto.AssessmentScore, dto.CompletedOn);
            repository.UpdateAsync(entity);

            // HC188 — a completed enrollment fulfils its linked approved need (request → fulfillment).
            if (status == TrainingEnrollmentStatus.Completed && entity.TrainingNeedId.HasValue)
            {
                var need = await needRepository.GetAll()
                    .FirstOrDefaultAsync(n => n.Id == entity.TrainingNeedId.Value);
                if (need is not null && need.Status == TrainingNeedStatus.Approved)
                {
                    need.MarkFulfilled(entity.CompletedOn ?? DateTime.UtcNow.Date);
                    needRepository.UpdateAsync(need);
                }
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Recorded participation for enrollment {Id} ({Status})", dto.Id, status);
        }
    }

    public class SubmitTrainingFeedback(
        IRepository<TrainingEnrollment> repository,
        IPerformanceVisibilityService visibility,
        IValidator<TrainingFeedbackDto> validator,
        ILogger<SubmitTrainingFeedback> logger) : ISubmitTrainingFeedback
    {
        public async Task SubmitAsync(Guid id, TrainingFeedbackDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), id.ToString());

            // HC199 — effectiveness feedback comes from the PARTICIPANT only.
            var scope = await visibility.GetScopeAsync();
            if (entity.EmployeeId != scope.EmployeeId)
                throw new ValidationException(nameof(id), "Only the participant can submit training feedback.");

            entity.SubmitFeedback(dto.Rating, string.IsNullOrWhiteSpace(dto.Comments) ? null : dto.Comments.Trim());
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Feedback recorded for enrollment {Id} (rating {Rating})", id, dto.Rating);
        }
    }

    public class WithdrawTrainingEnrollment(
        IRepository<TrainingEnrollment> repository,
        IPerformanceVisibilityService visibility,
        ILogger<WithdrawTrainingEnrollment> logger) : IWithdrawTrainingEnrollment
    {
        public async Task WithdrawAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), id.ToString());

            var scope = await visibility.GetScopeAsync();
            if (entity.EmployeeId != scope.EmployeeId && !await visibility.CanAccessEmployeeAsync(entity.EmployeeId))
                throw new ValidationException(nameof(id), "You cannot withdraw this enrollment.");

            entity.Withdraw();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Withdrew enrollment {Id}", id);
        }
    }

    public class DeleteTrainingEnrollment(
        IRepository<TrainingEnrollment> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteTrainingEnrollment> logger) : IDeleteTrainingEnrollment
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(id), "Only HR administrators can delete enrollments.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingEnrollment), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted enrollment {Id}", id);
        }
    }

    public class GetAllTrainingEnrollments(
        IRepository<TrainingEnrollment> repository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllTrainingEnrollments
    {
        public async Task<PaginatedResponse<TrainingEnrollmentDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // Role scoping as one SQL predicate: HR all, manager subtree, employee own.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var emps = employeeRepository.GetAll();
                    var unitIds = scope.UnitIds;
                    query = query.Where(x => x.EmployeeId == myEmp
                        || emps.Any(e => e.Id == x.EmployeeId && e.Position != null
                            && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(x => x.EmployeeId == myEmp);
                }
            }

            if (request.SessionId.HasValue)
                query = query.Where(x => x.TrainingSessionId == request.SessionId.Value);
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<TrainingEnrollmentStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);

            var sessions = sessionRepository.GetAll();
            var courses = courseRepository.GetAll();
            var employees = employeeRepository.GetAll();

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take)
                .Select(x => new TrainingEnrollmentDto
                {
                    Id = x.Id,
                    TrainingSessionId = x.TrainingSessionId,
                    CourseName = sessions.Where(ss => ss.Id == x.TrainingSessionId)
                        .Join(courses, ss => ss.TrainingCourseId, c => c.Id, (ss, c) => c.Name).FirstOrDefault(),
                    SessionStartDate = sessions.Where(ss => ss.Id == x.TrainingSessionId)
                        .Select(ss => ss.StartDate).FirstOrDefault(),
                    SessionEndDate = sessions.Where(ss => ss.Id == x.TrainingSessionId)
                        .Select(ss => ss.EndDate).FirstOrDefault(),
                    SessionStatus = sessions.Where(ss => ss.Id == x.TrainingSessionId)
                        .Select(ss => ss.Status.ToString()).FirstOrDefault(),
                    EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    EmployeeNumber = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.EmployeeNumber).FirstOrDefault(),
                    TrainingNeedId = x.TrainingNeedId,
                    Status = x.Status.ToString(),
                    AttendancePercent = x.AttendancePercent,
                    AssessmentScore = x.AssessmentScore,
                    CompletedOn = x.CompletedOn,
                    FeedbackRating = x.FeedbackRating,
                    FeedbackComments = x.FeedbackComments,
                    CpdHours = sessions.Where(ss => ss.Id == x.TrainingSessionId)
                        .Join(courses, ss => ss.TrainingCourseId, c => c.Id, (ss, c) => c.CpdHours).FirstOrDefault()
                }).ToListAsync();

            return new PaginatedResponse<TrainingEnrollmentDto> { Total = total, Data = data };
        }
    }
}
