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
    public class DevelopmentActionDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? CompetencyId { get; set; }
        public string? CompetencyName { get; set; }
        public string? LearningIntervention { get; set; }
        public DateTime? TargetDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public int SortOrder { get; set; }
    }

    public class DevelopmentPlanDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? AppraisalId { get; set; }
        public Guid? ReviewCycleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<DevelopmentActionDto> Actions { get; set; } = [];
    }

    public class SaveDevelopmentActionDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? CompetencyId { get; set; }
        public string? LearningIntervention { get; set; }
        public DateTime? TargetDate { get; set; }
        public string Status { get; set; } = nameof(DevelopmentActionStatus.Planned);
        public int ProgressPercent { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveDevelopmentPlanDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? AppraisalId { get; set; }
        public Guid? ReviewCycleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = nameof(DevelopmentPlanStatus.Draft);
        public List<SaveDevelopmentActionDto> Actions { get; set; } = [];
    }

    public class SaveDevelopmentPlanDtoValidator : AbstractValidator<SaveDevelopmentPlanDto>
    {
        public SaveDevelopmentPlanDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date cannot be before the start date.");
            RuleFor(x => x.Status).NotEmpty()
                .Must(v => Enum.TryParse<DevelopmentPlanStatus>(v, out _))
                .WithMessage("Status must be one of: Draft, Active, Completed, Cancelled.");
            RuleForEach(x => x.Actions).ChildRules(a =>
            {
                a.RuleFor(y => y.Description).NotEmpty().MaximumLength(500);
                a.RuleFor(y => y.Status).Must(v => Enum.TryParse<DevelopmentActionStatus>(v, out _))
                    .WithMessage("Action status must be Planned, InProgress or Completed.");
                a.RuleFor(y => y.ProgressPercent).InclusiveBetween(0, 100);
            });
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveDevelopmentPlan { Task<Guid> SaveAsync(SaveDevelopmentPlanDto dto); }
    public interface IDeleteDevelopmentPlan { Task DeleteAsync(Guid id); }
    public interface IGetDevelopmentPlanById { Task<DevelopmentPlanDto> GetAsync(Guid id); }
    public interface IGetAllDevelopmentPlans { Task<PaginatedResponse<DevelopmentPlanDto>> GetAsync(GetAllRequest request); }

    internal static class DevelopmentPlanMapper
    {
        internal static DevelopmentPlanDto Map(IndividualDevelopmentPlan x, string? employeeName,
            IReadOnlyDictionary<Guid, string> competencyNames) => new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            EmployeeName = employeeName,
            AppraisalId = x.AppraisalId,
            ReviewCycleId = x.ReviewCycleId,
            Title = x.Title,
            Description = x.Description,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Status = x.Status.ToString(),
            Actions = x.Actions.OrderBy(a => a.SortOrder).Select(a => new DevelopmentActionDto
            {
                Id = a.Id,
                Description = a.Description,
                CompetencyId = a.CompetencyId,
                CompetencyName = a.CompetencyId.HasValue && competencyNames.TryGetValue(a.CompetencyId.Value, out var n) ? n : null,
                LearningIntervention = a.LearningIntervention,
                TargetDate = a.TargetDate,
                Status = a.Status.ToString(),
                ProgressPercent = a.ProgressPercent,
                SortOrder = a.SortOrder
            }).ToList()
        };

        internal static void StampActionTenant(IndividualDevelopmentPlan plan)
        {
            foreach (var a in plan.Actions)
                if (string.IsNullOrEmpty(a.TenantId)) a.TenantId = plan.TenantId;
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveDevelopmentPlan(
        IRepository<IndividualDevelopmentPlan> repository,
        IRepository<DevelopmentAction> actionRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<Competency> competencyRepository,
        IPerformanceHistoryWriter history,
        IValidator<SaveDevelopmentPlanDto> validator,
        ILogger<SaveDevelopmentPlan> logger) : ISaveDevelopmentPlan
    {
        public async Task<Guid> SaveAsync(SaveDevelopmentPlanDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            if (dto.AppraisalId.HasValue && !await appraisalRepository.GetAll().AnyAsync(a => a.Id == dto.AppraisalId.Value))
                throw new NotFoundException(nameof(Appraisal), dto.AppraisalId.Value.ToString());

            var competencyIds = dto.Actions.Where(a => a.CompetencyId.HasValue).Select(a => a.CompetencyId!.Value).Distinct().ToList();
            if (competencyIds.Count > 0)
            {
                var found = await competencyRepository.GetAll().Where(c => competencyIds.Contains(c.Id)).Select(c => c.Id).ToListAsync();
                var missing = competencyIds.Except(found).FirstOrDefault();
                if (missing != Guid.Empty) throw new NotFoundException(nameof(Competency), missing.ToString());
            }

            var status = Enum.Parse<DevelopmentPlanStatus>(dto.Status);
            var specs = dto.Actions.Select(a => new DevelopmentActionSpec(a.Id, a.Description, a.CompetencyId,
                a.LearningIntervention, a.TargetDate, Enum.Parse<DevelopmentActionStatus>(a.Status), a.ProgressPercent, a.SortOrder)).ToList();

            Guid id;
            string action;
            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.Actions).FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(IndividualDevelopmentPlan), dto.Id.Value.ToString());
                foreach (var old in entity.Actions.ToList())
                    actionRepository.Delete(old);
                entity.Update(dto.EmployeeId, dto.Title, dto.StartDate, dto.EndDate, dto.Description,
                    dto.AppraisalId, dto.ReviewCycleId, status);
                entity.SetActions(specs);
                DevelopmentPlanMapper.StampActionTenant(entity);
                foreach (var a in entity.Actions)
                    await actionRepository.AddAsync(a);
                repository.UpdateAsync(entity);
                id = entity.Id; action = "Updated";
            }
            else
            {
                var created = IndividualDevelopmentPlan.Create(dto.EmployeeId, dto.Title, dto.StartDate, dto.EndDate,
                    dto.Description, dto.AppraisalId, dto.ReviewCycleId, status);
                created.SetActions(specs);
                await repository.AddAsync(created);
                DevelopmentPlanMapper.StampActionTenant(created);
                id = created.Id; action = "Created";
            }

            await history.WriteAsync("DevelopmentPlan", id, action, $"IDP '{dto.Title}' {action.ToLower()}.");
            await repository.SaveChangesAsync();
            logger.LogInformation("{Action} DevelopmentPlan {Id}", action, id);
            return id;
        }
    }

    public class DeleteDevelopmentPlan(
        IRepository<IndividualDevelopmentPlan> repository,
        ILogger<DeleteDevelopmentPlan> logger) : IDeleteDevelopmentPlan
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(IndividualDevelopmentPlan), id.ToString());
            repository.Delete(entity);   // actions cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted DevelopmentPlan {Id}", id);
        }
    }

    public class GetDevelopmentPlanById(
        IRepository<IndividualDevelopmentPlan> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Competency> competencyRepository,
        IPerformanceVisibilityService visibility) : IGetDevelopmentPlanById
    {
        public async Task<DevelopmentPlanDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().Include(x => x.Actions).AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(IndividualDevelopmentPlan), id.ToString());
            if (!await visibility.CanAccessEmployeeAsync(entity.EmployeeId))
                throw new ValidationException("access", "You do not have access to this development plan.");
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == entity.EmployeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
            var compIds = entity.Actions.Where(a => a.CompetencyId.HasValue).Select(a => a.CompetencyId!.Value).Distinct().ToList();
            var names = await competencyRepository.GetAll().Where(c => compIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);
            return DevelopmentPlanMapper.Map(entity, employeeName, names);
        }
    }

    public class GetAllDevelopmentPlans(
        IRepository<IndividualDevelopmentPlan> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllDevelopmentPlans
    {
        public async Task<PaginatedResponse<DevelopmentPlanDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().Include(x => x.Actions).AsNoTracking();

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
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DevelopmentPlanStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.StartDate).Skip(skip).Take(take).ToListAsync();

            // PERFORMANCE: batch-load the employee names for the page in ONE query (was one per row).
            var empIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
            var employeeNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => empIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var empty = new Dictionary<Guid, string>();
            var data = new List<DevelopmentPlanDto>(rows.Count);
            foreach (var r in rows)
            {
                data.Add(DevelopmentPlanMapper.Map(r, employeeNames.GetValueOrDefault(r.EmployeeId), empty));
            }
            return new PaginatedResponse<DevelopmentPlanDto> { Total = total, Data = data };
        }
    }
}
