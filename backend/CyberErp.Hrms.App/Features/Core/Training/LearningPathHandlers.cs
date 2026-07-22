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
    public class LearningPathStepDto
    {
        public Guid TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public string? DeliveryMode { get; set; }
        public decimal CpdHours { get; set; }
        public int SortOrder { get; set; }
        public bool IsRequired { get; set; }
    }

    public class LearningPathDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? TargetPositionId { get; set; }
        public string? TargetPositionName { get; set; }
        public bool IsActive { get; set; }
        public int StepCount { get; set; }
        public List<LearningPathStepDto> Steps { get; set; } = [];
    }

    public class SaveLearningPathStepDto
    {
        public Guid TrainingCourseId { get; set; }
        public bool IsRequired { get; set; } = true;
    }

    public class SaveLearningPathDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? TargetPositionId { get; set; }
        public bool IsActive { get; set; } = true;
        /// <summary>Ordered course sequence.</summary>
        public List<SaveLearningPathStepDto> Steps { get; set; } = [];
    }

    /// <summary>An employee's progress along a path — a step completes via a completed enrollment.</summary>
    public class LearningPathProgressDto
    {
        public Guid LearningPathId { get; set; }
        public Guid EmployeeId { get; set; }
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
        public int RequiredSteps { get; set; }
        public int CompletedRequiredSteps { get; set; }
        public decimal ProgressPercent { get; set; }
        public List<LearningPathProgressStepDto> Steps { get; set; } = [];
    }

    public class LearningPathProgressStepDto
    {
        public Guid TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public int SortOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool Completed { get; set; }
    }

    public class SaveLearningPathDtoValidator : AbstractValidator<SaveLearningPathDto>
    {
        public SaveLearningPathDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.Steps).NotEmpty().WithMessage("A learning path needs at least one course.");
            RuleForEach(x => x.Steps).ChildRules(s => s.RuleFor(y => y.TrainingCourseId).NotEmpty());
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveLearningPath { Task<Guid> SaveAsync(SaveLearningPathDto dto); }
    public interface IDeleteLearningPath { Task DeleteAsync(Guid id); }
    public interface IGetLearningPathById { Task<LearningPathDto> GetAsync(Guid id); }
    public interface IGetAllLearningPaths { Task<PaginatedResponse<LearningPathDto>> GetAsync(GetAllRequest request); }
    public interface IGetLearningPathProgress { Task<LearningPathProgressDto> GetAsync(Guid id, Guid employeeId); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveLearningPath(
        IRepository<LearningPath> repository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveLearningPathDto> validator,
        ILogger<SaveLearningPath> logger) : ISaveLearningPath
    {
        public async Task<Guid> SaveAsync(SaveLearningPathDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage learning paths.");

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(LearningPath), nameof(dto.Name), dto.Name);

            var courseIds = dto.Steps.Select(s => s.TrainingCourseId).ToList();
            var known = await courseRepository.GetAll()
                .Where(c => courseIds.Contains(c.Id)).Select(c => c.Id).ToListAsync();
            var missing = courseIds.Except(known).FirstOrDefault();
            if (missing != Guid.Empty)
                throw new NotFoundException(nameof(TrainingCourse), missing.ToString());

            var specs = dto.Steps.Select(s => new LearningPathStepSpec(s.TrainingCourseId, s.IsRequired));

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.Steps)
                    .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(LearningPath), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Description, dto.TargetPositionId, dto.IsActive);
                entity.SetSteps(specs);
                foreach (var step in entity.Steps)
                    if (string.IsNullOrEmpty(step.TenantId)) step.TenantId = entity.TenantId;
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated LearningPath {Id}", entity.Id);
                return entity.Id;
            }

            var created = LearningPath.Create(dto.Name, dto.Description, dto.TargetPositionId, dto.IsActive);
            created.SetSteps(specs);
            await repository.AddAsync(created);
            // The repository stamps only the aggregate root — cascade-inserted steps copy its tenant.
            foreach (var step in created.Steps)
                if (string.IsNullOrEmpty(step.TenantId)) step.TenantId = created.TenantId;
            await repository.SaveChangesAsync();
            logger.LogInformation("Created LearningPath {Id} ({Name}, {Steps} steps)", created.Id, created.Name, created.Steps.Count);
            return created.Id;
        }
    }

    public class DeleteLearningPath(
        IRepository<LearningPath> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteLearningPath> logger) : IDeleteLearningPath
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage learning paths.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(LearningPath), id.ToString());
            repository.Delete(entity); // steps cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted LearningPath {Id}", id);
        }
    }

    internal static class LearningPathQueryShared
    {
        internal static IQueryable<LearningPathDto> Project(
            IQueryable<LearningPath> query,
            IQueryable<LearningPathStep> steps,
            IQueryable<TrainingCourse> courses,
            IQueryable<Position> positions,
            bool includeSteps)
        {
            return query.Select(p => new LearningPathDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                TargetPositionId = p.TargetPositionId,
                TargetPositionName = positions.Where(x => x.Id == p.TargetPositionId)
                    .Select(x => x.Code + (x.PositionClass != null ? " — " + x.PositionClass.Title : ""))
                    .FirstOrDefault(),
                IsActive = p.IsActive,
                StepCount = steps.Count(s => s.LearningPathId == p.Id),
                Steps = includeSteps
                    ? steps.Where(s => s.LearningPathId == p.Id).OrderBy(s => s.SortOrder)
                        .Select(s => new LearningPathStepDto
                        {
                            TrainingCourseId = s.TrainingCourseId,
                            CourseName = courses.Where(c => c.Id == s.TrainingCourseId).Select(c => c.Name).FirstOrDefault(),
                            DeliveryMode = courses.Where(c => c.Id == s.TrainingCourseId)
                                .Select(c => c.DeliveryMode.ToString()).FirstOrDefault(),
                            CpdHours = courses.Where(c => c.Id == s.TrainingCourseId).Select(c => c.CpdHours).FirstOrDefault(),
                            SortOrder = s.SortOrder,
                            IsRequired = s.IsRequired
                        }).ToList()
                    : new List<LearningPathStepDto>()
            });
        }
    }

    public class GetLearningPathById(
        IRepository<LearningPath> repository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Position> positionRepository) : IGetLearningPathById
    {
        public async Task<LearningPathDto> GetAsync(Guid id)
        {
            return await LearningPathQueryShared.Project(
                    repository.GetAll().AsNoTracking().Where(x => x.Id == id),
                    stepRepository.GetAll(), courseRepository.GetAll(), positionRepository.GetAll(), includeSteps: true)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(LearningPath), id.ToString());
        }
    }

    public class GetAllLearningPaths(
        IRepository<LearningPath> repository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Position> positionRepository) : IGetAllLearningPaths
    {
        public async Task<PaginatedResponse<LearningPathDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await LearningPathQueryShared.Project(
                    query.OrderBy(x => x.Name).Skip(skip).Take(take),
                    stepRepository.GetAll(), courseRepository.GetAll(), positionRepository.GetAll(), includeSteps: false)
                .ToListAsync();

            return new PaginatedResponse<LearningPathDto> { Total = total, Data = data };
        }
    }

    public class GetLearningPathProgress(
        IRepository<LearningPath> repository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<TrainingEnrollment> enrollmentRepository,
        IRepository<TrainingSession> sessionRepository,
        IPerformanceVisibilityService visibility) : IGetLearningPathProgress
    {
        public async Task<LearningPathProgressDto> GetAsync(Guid id, Guid employeeId)
        {
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(employeeId), "The employee is outside your scope.");
            if (!await repository.GetAll().AnyAsync(p => p.Id == id))
                throw new NotFoundException(nameof(LearningPath), id.ToString());

            var courses = courseRepository.GetAll();
            var steps = await stepRepository.GetAll().AsNoTracking()
                .Where(x => x.LearningPathId == id)
                .OrderBy(x => x.SortOrder)
                .Select(x => new LearningPathProgressStepDto
                {
                    TrainingCourseId = x.TrainingCourseId,
                    CourseName = courses.Where(c => c.Id == x.TrainingCourseId).Select(c => c.Name).FirstOrDefault(),
                    SortOrder = x.SortOrder,
                    IsRequired = x.IsRequired
                }).ToListAsync();

            // One set-based query: catalog courses this employee has COMPLETED a session of.
            var sessions = sessionRepository.GetAll();
            var completedCourseIds = (await enrollmentRepository.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == employeeId && e.Status == TrainingEnrollmentStatus.Completed)
                .Join(sessions, e => e.TrainingSessionId, ss => ss.Id, (e, ss) => ss.TrainingCourseId)
                .Distinct().ToListAsync()).ToHashSet();

            foreach (var step in steps)
                step.Completed = completedCourseIds.Contains(step.TrainingCourseId);

            var required = steps.Where(x => x.IsRequired).ToList();
            return new LearningPathProgressDto
            {
                LearningPathId = id,
                EmployeeId = employeeId,
                TotalSteps = steps.Count,
                CompletedSteps = steps.Count(x => x.Completed),
                RequiredSteps = required.Count,
                CompletedRequiredSteps = required.Count(x => x.Completed),
                ProgressPercent = steps.Count == 0 ? 0 : Math.Round(steps.Count(x => x.Completed) * 100m / steps.Count, 1),
                Steps = steps
            };
        }
    }
}
