using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- DTOs ---------------------------------------------------------------
    public class PipObjectiveDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public int SortOrder { get; set; }
    }

    public class ImprovementPlanDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? AppraisalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public string? OutcomeNotes { get; set; }
        public DateTime? OutcomeRecordedAt { get; set; }
        public List<PipObjectiveDto> Objectives { get; set; } = [];
    }

    public class SavePipObjectiveDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public string Status { get; set; } = nameof(PipObjectiveStatus.NotStarted);
        public int ProgressPercent { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveImprovementPlanDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? AppraisalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = nameof(PipStatus.Draft);
        public List<SavePipObjectiveDto> Objectives { get; set; } = [];
    }

    public class RecordImprovementPlanOutcomeDto
    {
        public Guid Id { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class SaveImprovementPlanDtoValidator : AbstractValidator<SaveImprovementPlanDto>
    {
        public SaveImprovementPlanDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date cannot be before the start date.");
            RuleFor(x => x.Status).NotEmpty()
                .Must(v => Enum.TryParse<PipStatus>(v, out _))
                .WithMessage("Status must be one of: Draft, Active, UnderReview, Completed.");
            RuleForEach(x => x.Objectives).ChildRules(o =>
            {
                o.RuleFor(y => y.Description).NotEmpty().MaximumLength(500);
                o.RuleFor(y => y.Status).Must(v => Enum.TryParse<PipObjectiveStatus>(v, out _))
                    .WithMessage("Objective status must be NotStarted, InProgress, Met or NotMet.");
                o.RuleFor(y => y.ProgressPercent).InclusiveBetween(0, 100);
            });
        }
    }

    public class RecordImprovementPlanOutcomeDtoValidator : AbstractValidator<RecordImprovementPlanOutcomeDto>
    {
        public RecordImprovementPlanOutcomeDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Outcome).NotEmpty()
                .Must(v => Enum.TryParse<PipOutcome>(v, out var o) && o != PipOutcome.Pending)
                .WithMessage("Outcome must be Successful, Unsuccessful or Extended.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveImprovementPlan { Task<Guid> SaveAsync(SaveImprovementPlanDto dto); }
    public interface IRecordImprovementPlanOutcome { Task RecordAsync(RecordImprovementPlanOutcomeDto dto); }
    public interface IDeleteImprovementPlan { Task DeleteAsync(Guid id); }
    public interface IGetImprovementPlanById { Task<ImprovementPlanDto> GetAsync(Guid id); }
    public interface IGetAllImprovementPlans { Task<PaginatedResponse<ImprovementPlanDto>> GetAsync(GetAllRequest request); }

    internal static class ImprovementPlanMapper
    {
        internal static ImprovementPlanDto Map(PerformanceImprovementPlan x, string? employeeName) => new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            EmployeeName = employeeName,
            AppraisalId = x.AppraisalId,
            Title = x.Title,
            Reason = x.Reason,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Status = x.Status.ToString(),
            Outcome = x.Outcome.ToString(),
            OutcomeNotes = x.OutcomeNotes,
            OutcomeRecordedAt = x.OutcomeRecordedAt,
            Objectives = x.Objectives.OrderBy(o => o.SortOrder).Select(o => new PipObjectiveDto
            {
                Id = o.Id,
                Description = o.Description,
                TargetDate = o.TargetDate,
                Status = o.Status.ToString(),
                ProgressPercent = o.ProgressPercent,
                SortOrder = o.SortOrder
            }).ToList()
        };

        internal static void StampObjectiveTenant(PerformanceImprovementPlan plan)
        {
            foreach (var o in plan.Objectives)
                if (string.IsNullOrEmpty(o.TenantId)) o.TenantId = plan.TenantId;
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveImprovementPlan(
        IRepository<PerformanceImprovementPlan> repository,
        IRepository<PipObjective> objectiveRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Appraisal> appraisalRepository,
        IPerformanceHistoryWriter history,
        IValidator<SaveImprovementPlanDto> validator,
        ILogger<SaveImprovementPlan> logger) : ISaveImprovementPlan
    {
        public async Task<Guid> SaveAsync(SaveImprovementPlanDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            if (dto.AppraisalId.HasValue && !await appraisalRepository.GetAll().AnyAsync(a => a.Id == dto.AppraisalId.Value))
                throw new NotFoundException(nameof(Appraisal), dto.AppraisalId.Value.ToString());

            var status = Enum.Parse<PipStatus>(dto.Status);
            var specs = dto.Objectives.Select(o => new PipObjectiveSpec(o.Id, o.Description, o.TargetDate,
                Enum.Parse<PipObjectiveStatus>(o.Status), o.ProgressPercent, o.SortOrder)).ToList();

            Guid id;
            string action;
            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.Objectives).FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(PerformanceImprovementPlan), dto.Id.Value.ToString());
                if (entity.Status == PipStatus.Completed)
                    throw new ValidationException(nameof(dto.Id), "A completed PIP can no longer be edited.");
                foreach (var old in entity.Objectives.ToList())
                    objectiveRepository.Delete(old);
                entity.Update(dto.EmployeeId, dto.Title, dto.Reason, dto.StartDate, dto.EndDate, dto.AppraisalId, status);
                entity.SetObjectives(specs);
                ImprovementPlanMapper.StampObjectiveTenant(entity);
                foreach (var o in entity.Objectives)
                    await objectiveRepository.AddAsync(o);
                repository.UpdateAsync(entity);
                id = entity.Id; action = "Updated";
            }
            else
            {
                var created = PerformanceImprovementPlan.Create(dto.EmployeeId, dto.Title, dto.Reason,
                    dto.StartDate, dto.EndDate, dto.AppraisalId, status);
                created.SetObjectives(specs);
                await repository.AddAsync(created);
                ImprovementPlanMapper.StampObjectiveTenant(created);
                id = created.Id; action = "Created";
            }

            await history.WriteAsync("ImprovementPlan", id, action, $"PIP '{dto.Title}' {action.ToLower()}.");
            await repository.SaveChangesAsync();
            logger.LogInformation("{Action} ImprovementPlan {Id}", action, id);
            return id;
        }
    }

    public class RecordImprovementPlanOutcome(
        IRepository<PerformanceImprovementPlan> repository,
        IPerformanceHistoryWriter history,
        IValidator<RecordImprovementPlanOutcomeDto> validator,
        ILogger<RecordImprovementPlanOutcome> logger) : IRecordImprovementPlanOutcome
    {
        public async Task RecordAsync(RecordImprovementPlanOutcomeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(PerformanceImprovementPlan), dto.Id.ToString());
            if (entity.Status == PipStatus.Completed)
                throw new ValidationException(nameof(dto.Id), "This PIP's outcome has already been recorded.");

            var outcome = Enum.Parse<PipOutcome>(dto.Outcome);
            entity.RecordOutcome(outcome, dto.Notes);
            await history.WriteAsync("ImprovementPlan", dto.Id, "OutcomeRecorded", $"Outcome recorded: {outcome}.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Recorded outcome {Outcome} for ImprovementPlan {Id}", outcome, dto.Id);
        }
    }

    public class DeleteImprovementPlan(
        IRepository<PerformanceImprovementPlan> repository,
        ILogger<DeleteImprovementPlan> logger) : IDeleteImprovementPlan
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(PerformanceImprovementPlan), id.ToString());
            repository.Delete(entity);   // objectives cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted ImprovementPlan {Id}", id);
        }
    }

    public class GetImprovementPlanById(
        IRepository<PerformanceImprovementPlan> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetImprovementPlanById
    {
        public async Task<ImprovementPlanDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().Include(x => x.Objectives).AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(PerformanceImprovementPlan), id.ToString());
            if (!await visibility.CanAccessEmployeeAsync(entity.EmployeeId))
                throw new ValidationException("access", "You do not have access to this improvement plan.");
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == entity.EmployeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
            return ImprovementPlanMapper.Map(entity, employeeName);
        }
    }

    public class GetAllImprovementPlans(
        IRepository<PerformanceImprovementPlan> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllImprovementPlans
    {
        public async Task<PaginatedResponse<ImprovementPlanDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().Include(x => x.Objectives).AsNoTracking();

            // Role-based visibility: admin → all; manager → own + unit-subtree; employee → own only.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var unitIds = scope.UnitIds;
                    var emps = employeeRepository.GetAll();
                    query = query.Where(p => p.EmployeeId == myEmp ||
                        emps.Any(e => e.Id == p.EmployeeId && e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(p => p.EmployeeId == myEmp);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Title.Contains(request.SearchText.Trim()));
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<PipStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.StartDate).Skip(skip).Take(take).ToListAsync();

            // PERFORMANCE: batch-load the employee names for the page in ONE query (was one per row).
            var empIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
            var employeeNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => empIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var data = new List<ImprovementPlanDto>(rows.Count);
            foreach (var r in rows)
            {
                data.Add(ImprovementPlanMapper.Map(r, employeeNames.GetValueOrDefault(r.EmployeeId)));
            }
            return new PaginatedResponse<ImprovementPlanDto> { Total = total, Data = data };
        }
    }
}
