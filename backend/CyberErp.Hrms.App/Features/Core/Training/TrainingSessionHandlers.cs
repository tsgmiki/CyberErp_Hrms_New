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
    public class TrainingSessionDto
    {
        public Guid Id { get; set; }
        public Guid TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public string? DeliveryMode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Venue { get; set; }
        public string TrainerType { get; set; } = string.Empty;
        public string? TrainerName { get; set; }
        public string? ProviderName { get; set; }
        public string? MeetingUrl { get; set; }
        public int? MaxParticipants { get; set; }
        public int EnrolledCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? TrainerCost { get; set; }
        public decimal? MaterialsCost { get; set; }
        public decimal? VenueCost { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveTrainingSessionDto
    {
        public Guid? Id { get; set; }
        public Guid TrainingCourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Venue { get; set; }
        /// <summary>Internal | External.</summary>
        public string TrainerType { get; set; } = nameof(Dom.Entities.Core.TrainerType.Internal);
        public string? TrainerName { get; set; }
        public string? ProviderName { get; set; }
        public string? MeetingUrl { get; set; }
        public int? MaxParticipants { get; set; }
        public decimal? TrainerCost { get; set; }
        public decimal? MaterialsCost { get; set; }
        public decimal? VenueCost { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>HC197 — materialize a bounded recurring series from one session blueprint.</summary>
    public class CreateSessionSeriesDto : SaveTrainingSessionDto
    {
        /// <summary>Weekly | Monthly.</summary>
        public string Recurrence { get; set; } = "Weekly";
        /// <summary>Total sessions to create (2–26).</summary>
        public int Occurrences { get; set; } = 2;
    }

    public class RescheduleSessionDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SaveTrainingSessionDtoValidator : AbstractValidator<SaveTrainingSessionDto>
    {
        public SaveTrainingSessionDtoValidator()
        {
            RuleFor(x => x.TrainingCourseId).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date cannot precede the start date.");
            RuleFor(x => x.Venue).MaximumLength(300);
            RuleFor(x => x.TrainerType)
                .Must(v => Enum.TryParse<TrainerType>(v, true, out _))
                .WithMessage("Trainer type must be Internal or External.");
            RuleFor(x => x.TrainerName).MaximumLength(200);
            RuleFor(x => x.ProviderName).MaximumLength(200);
            RuleFor(x => x.MeetingUrl).MaximumLength(500);
            RuleFor(x => x.MaxParticipants).GreaterThanOrEqualTo(1).When(x => x.MaxParticipants.HasValue);
            RuleFor(x => x.TrainerCost).GreaterThanOrEqualTo(0).When(x => x.TrainerCost.HasValue);
            RuleFor(x => x.MaterialsCost).GreaterThanOrEqualTo(0).When(x => x.MaterialsCost.HasValue);
            RuleFor(x => x.VenueCost).GreaterThanOrEqualTo(0).When(x => x.VenueCost.HasValue);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTrainingSession { Task<Guid> SaveAsync(SaveTrainingSessionDto dto); }
    public interface ICreateTrainingSessionSeries { Task<List<Guid>> CreateAsync(CreateSessionSeriesDto dto); }
    public interface IRescheduleTrainingSession { Task RescheduleAsync(Guid id, RescheduleSessionDto dto); }
    public interface ICompleteTrainingSession { Task CompleteAsync(Guid id); }
    public interface ICancelTrainingSession { Task CancelAsync(Guid id); }
    public interface IDeleteTrainingSession { Task DeleteAsync(Guid id); }
    public interface IGetTrainingSessionById { Task<TrainingSessionDto> GetAsync(Guid id); }
    public interface IGetAllTrainingSessions { Task<PaginatedResponse<TrainingSessionDto>> GetAsync(GetAllRequest request); }

    internal static class TrainingSessionShared
    {
        /// <summary>Scheduling / cost entry is an HR function — sessions are admin-managed.</summary>
        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage training sessions.");
        }

        internal static IQueryable<TrainingSessionDto> Project(
            IQueryable<TrainingSession> query,
            IQueryable<TrainingCourse> courses,
            IQueryable<TrainingEnrollment> enrollments)
        {
            return query.Select(x => new TrainingSessionDto
            {
                Id = x.Id,
                TrainingCourseId = x.TrainingCourseId,
                CourseName = courses.Where(c => c.Id == x.TrainingCourseId).Select(c => c.Name).FirstOrDefault(),
                DeliveryMode = courses.Where(c => c.Id == x.TrainingCourseId)
                    .Select(c => c.DeliveryMode.ToString()).FirstOrDefault(),
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Venue = x.Venue,
                TrainerType = x.TrainerType.ToString(),
                TrainerName = x.TrainerName,
                ProviderName = x.ProviderName,
                MeetingUrl = x.MeetingUrl,
                MaxParticipants = x.MaxParticipants,
                EnrolledCount = enrollments.Count(e => e.TrainingSessionId == x.Id
                    && e.Status != TrainingEnrollmentStatus.Withdrawn),
                Status = x.Status.ToString(),
                TrainerCost = x.TrainerCost,
                MaterialsCost = x.MaterialsCost,
                VenueCost = x.VenueCost,
                Notes = x.Notes
            });
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTrainingSession(
        IRepository<TrainingSession> repository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveTrainingSessionDto> validator,
        ILogger<SaveTrainingSession> logger) : ISaveTrainingSession
    {
        public async Task<Guid> SaveAsync(SaveTrainingSessionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            await TrainingSessionShared.EnsureAdminAsync(visibility);

            var course = await courseRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == dto.TrainingCourseId)
                ?? throw new NotFoundException(nameof(TrainingCourse), dto.TrainingCourseId.ToString());
            if (!course.IsActive)
                throw new ValidationException(nameof(dto.TrainingCourseId), "The selected course is inactive.");

            var trainerType = Enum.Parse<TrainerType>(dto.TrainerType, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TrainingSession), dto.Id.Value.ToString());
                entity.Update(dto.TrainingCourseId, dto.StartDate, dto.EndDate, dto.Venue, trainerType,
                    dto.TrainerName, dto.ProviderName, dto.MeetingUrl, dto.MaxParticipants,
                    dto.TrainerCost, dto.MaterialsCost, dto.VenueCost, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated TrainingSession {Id}", entity.Id);
                return entity.Id;
            }

            var created = TrainingSession.Create(dto.TrainingCourseId, dto.StartDate, dto.EndDate, dto.Venue,
                trainerType, dto.TrainerName, dto.ProviderName, dto.MeetingUrl, dto.MaxParticipants,
                dto.TrainerCost, dto.MaterialsCost, dto.VenueCost, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created TrainingSession {Id} for course {Course}", created.Id, course.Id);
            return created.Id;
        }
    }

    public class CreateTrainingSessionSeries(
        IRepository<TrainingSession> repository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveTrainingSessionDto> validator,
        ILogger<CreateTrainingSessionSeries> logger) : ICreateTrainingSessionSeries
    {
        public async Task<List<Guid>> CreateAsync(CreateSessionSeriesDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            await TrainingSessionShared.EnsureAdminAsync(visibility);

            if (dto.Occurrences is < 2 or > 26)
                throw new ValidationException(nameof(dto.Occurrences), "A series is 2 to 26 sessions.");
            var monthly = dto.Recurrence.Equals("Monthly", StringComparison.OrdinalIgnoreCase);
            if (!monthly && !dto.Recurrence.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException(nameof(dto.Recurrence), "Recurrence must be Weekly or Monthly.");

            if (!await courseRepository.GetAll().AnyAsync(c => c.Id == dto.TrainingCourseId && c.IsActive))
                throw new ValidationException(nameof(dto.TrainingCourseId), "The selected course is missing or inactive.");

            var trainerType = Enum.Parse<TrainerType>(dto.TrainerType, true);
            var ids = new List<Guid>(dto.Occurrences);
            for (var i = 0; i < dto.Occurrences; i++)
            {
                var start = monthly ? dto.StartDate.AddMonths(i) : dto.StartDate.AddDays(7 * i);
                var end = monthly ? dto.EndDate.AddMonths(i) : dto.EndDate.AddDays(7 * i);
                var session = TrainingSession.Create(dto.TrainingCourseId, start, end, dto.Venue, trainerType,
                    dto.TrainerName, dto.ProviderName, dto.MeetingUrl, dto.MaxParticipants,
                    dto.TrainerCost, dto.MaterialsCost, dto.VenueCost, dto.Notes);
                await repository.AddAsync(session);
                ids.Add(session.Id);
            }
            await repository.SaveChangesAsync();
            logger.LogInformation("Created a {Recurrence} series of {Count} sessions for course {Course}",
                dto.Recurrence, ids.Count, dto.TrainingCourseId);
            return ids;
        }
    }

    public class RescheduleTrainingSession(
        IRepository<TrainingSession> repository,
        IPerformanceVisibilityService visibility,
        ILogger<RescheduleTrainingSession> logger) : IRescheduleTrainingSession
    {
        public async Task RescheduleAsync(Guid id, RescheduleSessionDto dto)
        {
            await TrainingSessionShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingSession), id.ToString());
            entity.Reschedule(dto.StartDate, dto.EndDate);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Rescheduled TrainingSession {Id} to {Start:d}", id, dto.StartDate);
        }
    }

    public class CompleteTrainingSession(
        IRepository<TrainingSession> repository,
        IRepository<TrainingProviderPayment> paymentRepository,
        IPerformanceVisibilityService visibility,
        ILogger<CompleteTrainingSession> logger) : ICompleteTrainingSession
    {
        public async Task CompleteAsync(Guid id)
        {
            await TrainingSessionShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingSession), id.ToString());
            entity.MarkCompleted();
            repository.UpdateAsync(entity);

            // HC202 — a completed provider-delivered session raises the finance hand-off row
            // (idempotent per session; amount = the session's cost lines).
            if (!string.IsNullOrWhiteSpace(entity.ProviderName) && entity.TotalCost > 0
                && !await paymentRepository.GetAll().AnyAsync(p => p.TrainingSessionId == entity.Id))
            {
                var payment = TrainingProviderPayment.Create(entity.ProviderName!, entity.TotalCost, entity.Id,
                    "Auto-raised on session completion");
                if (string.IsNullOrEmpty(payment.TenantId)) payment.TenantId = entity.TenantId;
                await paymentRepository.AddAsync(payment);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Completed TrainingSession {Id}", id);
        }
    }

    public class CancelTrainingSession(
        IRepository<TrainingSession> repository,
        IPerformanceVisibilityService visibility,
        ILogger<CancelTrainingSession> logger) : ICancelTrainingSession
    {
        public async Task CancelAsync(Guid id)
        {
            await TrainingSessionShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingSession), id.ToString());
            entity.Cancel();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cancelled TrainingSession {Id}", id);
        }
    }

    public class DeleteTrainingSession(
        IRepository<TrainingSession> repository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteTrainingSession> logger) : IDeleteTrainingSession
    {
        public async Task DeleteAsync(Guid id)
        {
            await TrainingSessionShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingSession), id.ToString());
            if (await enrollmentRepository.GetAll().AnyAsync(e => e.TrainingSessionId == id))
                throw new ValidationException(nameof(id), "Cannot delete a session with enrollments — cancel it instead.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted TrainingSession {Id}", id);
        }
    }

    public class GetTrainingSessionById(
        IRepository<TrainingSession> repository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<TrainingEnrollment> enrollmentRepository) : IGetTrainingSessionById
    {
        public async Task<TrainingSessionDto> GetAsync(Guid id)
        {
            return await TrainingSessionShared.Project(
                    repository.GetAll().AsNoTracking().Where(x => x.Id == id),
                    courseRepository.GetAll(), enrollmentRepository.GetAll())
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TrainingSession), id.ToString());
        }
    }

    public class GetAllTrainingSessions(
        IRepository<TrainingSession> repository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<TrainingEnrollment> enrollmentRepository) : IGetAllTrainingSessions
    {
        public async Task<PaginatedResponse<TrainingSessionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            // The schedule is company-visible (employees browse and join); mutations are admin-gated.
            var query = repository.GetAll().AsNoTracking();
            if (request.CourseId.HasValue)
                query = query.Where(x => x.TrainingCourseId == request.CourseId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<TrainingSessionStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (DateTime.TryParse(request.FromDate, out var from))
                query = query.Where(x => x.StartDate >= from);
            if (DateTime.TryParse(request.ToDate, out var to))
                query = query.Where(x => x.StartDate <= to);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                var courses = courseRepository.GetAll();
                query = query.Where(x => (x.Venue != null && x.Venue.Contains(term))
                    || (x.TrainerName != null && x.TrainerName.Contains(term))
                    || courses.Any(c => c.Id == x.TrainingCourseId && c.Name.Contains(term)));
            }

            var total = await query.CountAsync();
            var data = await TrainingSessionShared.Project(
                    query.OrderByDescending(x => x.StartDate).Skip(skip).Take(take),
                    courseRepository.GetAll(), enrollmentRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<TrainingSessionDto> { Total = total, Data = data };
        }
    }
}
