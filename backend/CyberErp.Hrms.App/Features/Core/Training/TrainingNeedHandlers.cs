using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class TrainingNeedDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid? TrainingCourseId { get; set; }
        public string? CourseName { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string NeedType { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? CompetencyId { get; set; }
        public string? CompetencyName { get; set; }
        public decimal? EstimatedCost { get; set; }
        public DateTime? NeededBy { get; set; }
        public Guid? RequestedByEmployeeId { get; set; }
        public string? RequestedByName { get; set; }
        public DateTime? DecidedOn { get; set; }
        public DateTime? FulfilledOn { get; set; }
    }

    public class SaveTrainingNeedDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? TrainingCourseId { get; set; }
        public string Topic { get; set; } = string.Empty;
        /// <summary>Local | Abroad (HC187).</summary>
        public string NeedType { get; set; } = nameof(TrainingNeedType.Local);
        public string Justification { get; set; } = string.Empty;
        /// <summary>Low | Medium | High | Critical.</summary>
        public string Priority { get; set; } = nameof(TrainingNeedPriority.Medium);
        /// <summary>Manual | CompetencyGap | Appraisal | Goal (HC189) — set by the suggestions flow.</summary>
        public string Source { get; set; } = nameof(TrainingNeedSource.Manual);
        public Guid? CompetencyId { get; set; }
        public decimal? EstimatedCost { get; set; }
        public DateTime? NeededBy { get; set; }
    }

    public class SaveTrainingNeedDtoValidator : AbstractValidator<SaveTrainingNeedDto>
    {
        public SaveTrainingNeedDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Topic).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Justification).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.NeedType)
                .Must(v => Enum.TryParse<TrainingNeedType>(v, true, out _))
                .WithMessage("Need type must be Local or Abroad.");
            RuleFor(x => x.Priority)
                .Must(v => Enum.TryParse<TrainingNeedPriority>(v, true, out _))
                .WithMessage("Priority must be Low, Medium, High or Critical.");
            RuleFor(x => x.Source)
                .Must(v => Enum.TryParse<TrainingNeedSource>(v, true, out _))
                .WithMessage("Source must be Manual, CompetencyGap, Appraisal or Goal.");
            RuleFor(x => x.EstimatedCost).GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTrainingNeed { Task<Guid> SaveAsync(SaveTrainingNeedDto dto); }
    public interface IDeleteTrainingNeed { Task DeleteAsync(Guid id); }
    public interface ICancelTrainingNeed { Task CancelAsync(Guid id); }
    public interface IGetTrainingNeedById { Task<TrainingNeedDto> GetAsync(Guid id); }
    public interface IGetAllTrainingNeeds { Task<PaginatedResponse<TrainingNeedDto>> GetAsync(GetAllRequest request); }

    /// <summary>Applies the approval outcome — called by the workflow handler or (no workflow) by HR directly.</summary>
    public interface ITrainingNeedDecision
    {
        Task ApproveAsync(Guid id);
        Task RejectAsync(Guid id);
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTrainingNeed(
        IRepository<TrainingNeed> repository,
        IRepository<Employee> employeeRepository,
        IRepository<TrainingCourse> courseRepository,
        IPerformanceVisibilityService visibility,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        IValidator<SaveTrainingNeedDto> validator,
        ILogger<SaveTrainingNeed> logger) : ISaveTrainingNeed
    {
        public async Task<Guid> SaveAsync(SaveTrainingNeedDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // HC187 — employees request for themselves, managers for their subtree, HR for anyone.
            var scope = await visibility.GetScopeAsync();
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(dto.EmployeeId), "The employee is outside your scope.");

            var employee = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeNumber,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            if (dto.TrainingCourseId.HasValue)
            {
                var course = await courseRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dto.TrainingCourseId.Value)
                    ?? throw new NotFoundException(nameof(TrainingCourse), dto.TrainingCourseId.Value.ToString());
                if (!course.IsActive)
                    throw new ValidationException(nameof(dto.TrainingCourseId), "The selected course is inactive.");
            }

            var needType = Enum.Parse<TrainingNeedType>(dto.NeedType, true);
            var priority = Enum.Parse<TrainingNeedPriority>(dto.Priority, true);
            var source = Enum.Parse<TrainingNeedSource>(dto.Source, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TrainingNeed), dto.Id.Value.ToString());
                if (!scope.IsAdmin && entity.RequestedByEmployeeId != scope.EmployeeId && entity.EmployeeId != scope.EmployeeId)
                    throw new ValidationException(nameof(dto.Id), "Only the requester, the employee or HR can edit a training need.");
                if (entity.Status != TrainingNeedStatus.Pending)
                    throw new ValidationException(nameof(dto.Id), "Only a pending training need can be edited.");
                if (entity.EmployeeId != dto.EmployeeId)
                    throw new ValidationException(nameof(dto.EmployeeId), "The employee on a training need cannot change — raise a new request.");
                await workflowGate.EnsureNoRunningAsync("TrainingNeed", entity.Id);

                entity.UpdateRequest(dto.Topic, needType, dto.Justification, priority,
                    dto.TrainingCourseId, dto.CompetencyId, dto.EstimatedCost, dto.NeededBy);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated TrainingNeed {Id}", entity.Id);
                return entity.Id;
            }

            var created = TrainingNeed.Create(dto.EmployeeId, dto.Topic, needType, dto.Justification, priority,
                source, dto.TrainingCourseId, dto.CompetencyId, dto.EstimatedCost, dto.NeededBy, scope.EmployeeId);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();

            // HC188/HC201 — the chain is configured PER need type ("TrainingNeed.Local" / "TrainingNeed.Abroad");
            // without a definition the module operates directly (HR decides from the list).
            await workflowService.StartIfDefinedAsync(
                created.WorkflowEntityType, created.Id, dto.EmployeeId,
                $"{needType} training — {employee.Name}: {dto.Topic}");

            logger.LogInformation("Created TrainingNeed {Id} ({Type}) for employee {Employee}", created.Id, needType, employee.Id);
            return created.Id;
        }
    }

    public class TrainingNeedDecision(
        IRepository<TrainingNeed> repository,
        ILogger<TrainingNeedDecision> logger) : ITrainingNeedDecision
    {
        public async Task ApproveAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingNeed), id.ToString());
            if (entity.Status == TrainingNeedStatus.Approved) return; // idempotent
            entity.MarkApproved(DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Approved TrainingNeed {Id}", id);
        }

        public async Task RejectAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingNeed), id.ToString());
            if (entity.Status == TrainingNeedStatus.Rejected) return;
            entity.MarkRejected(DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Rejected TrainingNeed {Id}", id);
        }
    }

    public class CancelTrainingNeed(
        IRepository<TrainingNeed> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate,
        ILogger<CancelTrainingNeed> logger) : ICancelTrainingNeed
    {
        public async Task CancelAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingNeed), id.ToString());

            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && entity.RequestedByEmployeeId != scope.EmployeeId && entity.EmployeeId != scope.EmployeeId)
                throw new ValidationException(nameof(id), "Only the requester, the employee or HR can cancel a training need.");
            await workflowGate.EnsureNoRunningAsync("TrainingNeed", entity.Id);

            entity.Cancel();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cancelled TrainingNeed {Id}", id);
        }
    }

    public class DeleteTrainingNeed(
        IRepository<TrainingNeed> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate,
        ILogger<DeleteTrainingNeed> logger) : IDeleteTrainingNeed
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TrainingNeed), id.ToString());

            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && entity.RequestedByEmployeeId != scope.EmployeeId)
                throw new ValidationException(nameof(id), "Only the requester or HR can delete a training need.");
            if (entity.Status != TrainingNeedStatus.Pending)
                throw new ValidationException(nameof(id), "Only a pending training need can be deleted — cancel it instead.");
            await workflowGate.EnsureNoRunningAsync("TrainingNeed", entity.Id);

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted TrainingNeed {Id}", id);
        }
    }

    internal static class TrainingNeedQueryShared
    {
        internal static IQueryable<TrainingNeedDto> Project(
            IQueryable<TrainingNeed> query,
            IQueryable<Employee> employees,
            IQueryable<TrainingCourse> courses,
            IQueryable<Competency> competencies)
        {
            return query.Select(n => new TrainingNeedDto
            {
                Id = n.Id,
                EmployeeId = n.EmployeeId,
                EmployeeName = employees.Where(e => e.Id == n.EmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefault(),
                EmployeeNumber = employees.Where(e => e.Id == n.EmployeeId)
                    .Select(e => e.EmployeeNumber).FirstOrDefault(),
                TrainingCourseId = n.TrainingCourseId,
                CourseName = courses.Where(c => c.Id == n.TrainingCourseId).Select(c => c.Name).FirstOrDefault(),
                Topic = n.Topic,
                NeedType = n.NeedType.ToString(),
                Justification = n.Justification,
                Priority = n.Priority.ToString(),
                Source = n.Source.ToString(),
                Status = n.Status.ToString(),
                CompetencyId = n.CompetencyId,
                CompetencyName = competencies.Where(c => c.Id == n.CompetencyId).Select(c => c.Name).FirstOrDefault(),
                EstimatedCost = n.EstimatedCost,
                NeededBy = n.NeededBy,
                RequestedByEmployeeId = n.RequestedByEmployeeId,
                RequestedByName = employees.Where(e => e.Id == n.RequestedByEmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                    .FirstOrDefault(),
                DecidedOn = n.DecidedOn,
                FulfilledOn = n.FulfilledOn
            });
        }
    }

    public class GetTrainingNeedById(
        IRepository<TrainingNeed> repository,
        IRepository<Employee> employeeRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Competency> competencyRepository,
        IPerformanceVisibilityService visibility) : IGetTrainingNeedById
    {
        public async Task<TrainingNeedDto> GetAsync(Guid id)
        {
            var dto = await TrainingNeedQueryShared.Project(
                    repository.GetAll().AsNoTracking().Where(x => x.Id == id),
                    employeeRepository.GetAll(), courseRepository.GetAll(), competencyRepository.GetAll())
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TrainingNeed), id.ToString());

            var scope = await visibility.GetScopeAsync();
            var allowed = scope.IsAdmin
                || dto.RequestedByEmployeeId == scope.EmployeeId
                || await visibility.CanAccessEmployeeAsync(dto.EmployeeId);
            if (!allowed)
                throw new ValidationException(nameof(id), "You do not have access to this training need.");
            return dto;
        }
    }

    public class GetAllTrainingNeeds(
        IRepository<TrainingNeed> repository,
        IRepository<Employee> employeeRepository,
        IRepository<TrainingCourse> courseRepository,
        IRepository<Competency> competencyRepository,
        IPerformanceVisibilityService visibility) : IGetAllTrainingNeeds
    {
        public async Task<PaginatedResponse<TrainingNeedDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // Role scoping as one SQL predicate: HR all, manager subtree + own requests, employee own.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var emps = employeeRepository.GetAll();
                    var unitIds = scope.UnitIds;
                    query = query.Where(x => x.EmployeeId == myEmp || x.RequestedByEmployeeId == myEmp
                        || emps.Any(e => e.Id == x.EmployeeId && e.Position != null
                            && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(x => x.EmployeeId == myEmp);
                }
            }

            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<TrainingNeedStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(request.NeedType) &&
                Enum.TryParse<TrainingNeedType>(request.NeedType, true, out var needType))
                query = query.Where(x => x.NeedType == needType);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                var emps = employeeRepository.GetAll();
                query = query.Where(x => x.Topic.Contains(term)
                    || emps.Any(e => e.Id == x.EmployeeId && (e.EmployeeNumber.Contains(term)
                        || (e.Person != null && (e.Person.FirstName.Contains(term) || e.Person.GrandFatherName.Contains(term))))));
            }

            var total = await query.CountAsync();
            var data = await TrainingNeedQueryShared.Project(
                    query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take),
                    employeeRepository.GetAll(), courseRepository.GetAll(), competencyRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<TrainingNeedDto> { Total = total, Data = data };
        }
    }
}
