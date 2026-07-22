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
    public class GoalActionItemDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
    }

    public class EmployeeGoalDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid ReviewCycleId { get; set; }
        public string? ReviewCycleName { get; set; }
        public Guid? OrganizationalObjectiveId { get; set; }
        public string? ObjectiveTitle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Measure { get; set; }
        public decimal? TargetValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Weight { get; set; }
        public int ProgressPercent { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool SetByManager { get; set; }
        public List<GoalActionItemDto> ActionItems { get; set; } = [];
    }

    public class SaveGoalActionItemDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveEmployeeGoalDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid ReviewCycleId { get; set; }
        public Guid? OrganizationalObjectiveId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Measure { get; set; }
        public decimal? TargetValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Weight { get; set; }
        public int ProgressPercent { get; set; }
        public string Status { get; set; } = nameof(GoalStatus.Draft);
        public bool SetByManager { get; set; }
        public List<SaveGoalActionItemDto> ActionItems { get; set; } = [];
    }

    public class SaveEmployeeGoalDtoValidator : AbstractValidator<SaveEmployeeGoalDto>
    {
        public SaveEmployeeGoalDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.ReviewCycleId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.Measure).MaximumLength(500);
            RuleFor(x => x.Weight).InclusiveBetween(0, 100);
            RuleFor(x => x.ProgressPercent).InclusiveBetween(0, 100);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.DueDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("Due date cannot be before the start date.");
            RuleFor(x => x.Status).NotEmpty()
                .Must(v => Enum.TryParse<GoalStatus>(v, out _))
                .WithMessage("Status must be one of: Draft, Active, Completed, Cancelled.");
            RuleForEach(x => x.ActionItems).ChildRules(i =>
                i.RuleFor(y => y.Description).NotEmpty().MaximumLength(500));
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveEmployeeGoal { Task<Guid> SaveAsync(SaveEmployeeGoalDto dto); }
    public interface IDeleteEmployeeGoal { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeGoalById { Task<EmployeeGoalDto> GetAsync(Guid id); }
    public interface IGetAllEmployeeGoals { Task<PaginatedResponse<EmployeeGoalDto>> GetAsync(GetAllRequest request); }

    internal static class EmployeeGoalMapper
    {
        internal static EmployeeGoalDto Map(EmployeeGoal x, string? employeeName, string? cycleName, string? objectiveTitle) => new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            EmployeeName = employeeName,
            ReviewCycleId = x.ReviewCycleId,
            ReviewCycleName = cycleName,
            OrganizationalObjectiveId = x.OrganizationalObjectiveId,
            ObjectiveTitle = objectiveTitle,
            Title = x.Title,
            Description = x.Description,
            Measure = x.Measure,
            TargetValue = x.TargetValue,
            StartDate = x.StartDate,
            DueDate = x.DueDate,
            Weight = x.Weight,
            ProgressPercent = x.ProgressPercent,
            Status = x.Status.ToString(),
            SetByManager = x.SetByManager,
            ActionItems = x.ActionItems.OrderBy(a => a.SortOrder).Select(a => new GoalActionItemDto
            {
                Id = a.Id,
                Description = a.Description,
                DueDate = a.DueDate,
                IsCompleted = a.IsCompleted,
                SortOrder = a.SortOrder
            }).ToList()
        };

        /// <summary>The repository stamps only aggregate roots — cascade-inserted action items copy it here.</summary>
        internal static void StampActionItemTenant(EmployeeGoal goal)
        {
            foreach (var item in goal.ActionItems)
                if (string.IsNullOrEmpty(item.TenantId))
                    item.TenantId = goal.TenantId;
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveEmployeeGoal(
        IRepository<EmployeeGoal> repository,
        IRepository<GoalActionItem> actionItemRepository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationalObjective> objectiveRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveEmployeeGoalDto> validator,
        ILogger<SaveEmployeeGoal> logger) : ISaveEmployeeGoal
    {
        public async Task<Guid> SaveAsync(SaveEmployeeGoalDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Scope gate: an employee manages only their OWN goals; a manager their subtree's; HR all.
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(dto.EmployeeId),
                    "You can only manage goals for yourself or for employees in your unit.");

            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            if (!await reviewCycleRepository.GetAll().AnyAsync(c => c.Id == dto.ReviewCycleId))
                throw new NotFoundException(nameof(ReviewCycle), dto.ReviewCycleId.ToString());
            if (dto.OrganizationalObjectiveId.HasValue &&
                !await objectiveRepository.GetAll().AnyAsync(o => o.Id == dto.OrganizationalObjectiveId.Value))
                throw new NotFoundException(nameof(OrganizationalObjective), dto.OrganizationalObjectiveId.Value.ToString());

            var status = Enum.Parse<GoalStatus>(dto.Status);
            var specs = dto.ActionItems.Select(a => new GoalActionItemSpec(
                a.Id, a.Description, a.DueDate, a.IsCompleted, a.SortOrder)).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.ActionItems)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(EmployeeGoal), dto.Id.Value.ToString());
                // The EXISTING record must also be in scope (blocks hijacking another employee's goal).
                if (!await visibility.CanAccessEmployeeAsync(entity.EmployeeId))
                    throw new ValidationException(nameof(dto.Id), "You do not have access to this goal.");

                // Old action-item rows are replaced wholesale.
                foreach (var old in entity.ActionItems.ToList())
                    actionItemRepository.Delete(old);

                entity.Update(dto.EmployeeId, dto.ReviewCycleId, dto.Title, dto.StartDate, dto.DueDate,
                    dto.Description, dto.Measure, dto.TargetValue, dto.OrganizationalObjectiveId,
                    dto.Weight, dto.ProgressPercent, status, dto.SetByManager);
                entity.SetActionItems(specs);
                EmployeeGoalMapper.StampActionItemTenant(entity);
                // Replacement items are new rows: mark them Added explicitly (see RatingScale/DynamicForm).
                foreach (var item in entity.ActionItems)
                    await actionItemRepository.AddAsync(item);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeGoal {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeGoal.Create(dto.EmployeeId, dto.ReviewCycleId, dto.Title, dto.StartDate, dto.DueDate,
                dto.Description, dto.Measure, dto.TargetValue, dto.OrganizationalObjectiveId,
                dto.Weight, dto.ProgressPercent, status, dto.SetByManager);
            created.SetActionItems(specs);
            await repository.AddAsync(created);   // stamps the root's TenantId
            EmployeeGoalMapper.StampActionItemTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeGoal {Id} ({Title})", created.Id, created.Title);
            return created.Id;
        }
    }

    public class DeleteEmployeeGoal(
        IRepository<EmployeeGoal> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteEmployeeGoal> logger) : IDeleteEmployeeGoal
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeGoal), id.ToString());
            if (!await visibility.CanAccessEmployeeAsync(entity.EmployeeId))
                throw new ValidationException(nameof(id), "You do not have access to this goal.");
            repository.Delete(entity);   // action items cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeGoal {Id}", id);
        }
    }

    public class GetEmployeeGoalById(
        IRepository<EmployeeGoal> repository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationalObjective> objectiveRepository,
        IPerformanceVisibilityService visibility) : IGetEmployeeGoalById
    {
        public async Task<EmployeeGoalDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().Include(x => x.ActionItems).AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeGoal), id.ToString());
            if (!await visibility.CanAccessEmployeeAsync(entity.EmployeeId))
                throw new ValidationException("access", "You do not have access to this goal.");
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == entity.EmployeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
            var cycleName = await reviewCycleRepository.GetAll().Where(c => c.Id == entity.ReviewCycleId).Select(c => c.Name).FirstOrDefaultAsync();
            var objectiveTitle = entity.OrganizationalObjectiveId.HasValue
                ? await objectiveRepository.GetAll().Where(o => o.Id == entity.OrganizationalObjectiveId.Value).Select(o => o.Title).FirstOrDefaultAsync()
                : null;
            return EmployeeGoalMapper.Map(entity, employeeName, cycleName, objectiveTitle);
        }
    }

    public class GetAllEmployeeGoals(
        IRepository<EmployeeGoal> repository,
        IRepository<Employee> employeeRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationalObjective> objectiveRepository,
        IPerformanceVisibilityService visibility) : IGetAllEmployeeGoals
    {
        public async Task<PaginatedResponse<EmployeeGoalDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().Include(x => x.ActionItems).AsNoTracking();

            // Role-based visibility as a single SQL predicate: admin → all; manager → own + their unit
            // subtree's employees; employee → own goals only.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var unitIds = scope.UnitIds;
                    var emps = employeeRepository.GetAll();
                    query = query.Where(g => g.EmployeeId == myEmp ||
                        emps.Any(e => e.Id == g.EmployeeId && e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(g => g.EmployeeId == myEmp);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Title.Contains(request.SearchText.Trim()));
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (request.ReviewCycleId.HasValue)
                query = query.Where(x => x.ReviewCycleId == request.ReviewCycleId.Value);
            if (request.ObjectiveId.HasValue)
                query = query.Where(x => x.OrganizationalObjectiveId == request.ObjectiveId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<GoalStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var goals = await query.OrderByDescending(x => x.StartDate).ThenBy(x => x.Title)
                .Skip(skip).Take(take).ToListAsync();

            // PERFORMANCE: batch-load the display names for the page in 3 queries total (was 3 PER ROW).
            var empIds = goals.Select(g => g.EmployeeId).Distinct().ToList();
            var cycleIds = goals.Select(g => g.ReviewCycleId).Distinct().ToList();
            var objIds = goals.Where(g => g.OrganizationalObjectiveId.HasValue)
                .Select(g => g.OrganizationalObjectiveId!.Value).Distinct().ToList();

            var employeeNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => empIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            var cycleNames = await reviewCycleRepository.GetAll().AsNoTracking()
                .Where(c => cycleIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
            var objectiveTitles = objIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await objectiveRepository.GetAll().AsNoTracking()
                    .Where(o => objIds.Contains(o.Id))
                    .Select(o => new { o.Id, o.Title })
                    .ToDictionaryAsync(x => x.Id, x => x.Title);

            var data = new List<EmployeeGoalDto>(goals.Count);
            foreach (var g in goals)
            {
                data.Add(EmployeeGoalMapper.Map(
                    g,
                    employeeNames.GetValueOrDefault(g.EmployeeId, ""),
                    cycleNames.GetValueOrDefault(g.ReviewCycleId),
                    g.OrganizationalObjectiveId.HasValue
                        ? objectiveTitles.GetValueOrDefault(g.OrganizationalObjectiveId.Value)
                        : null));
            }

            return new PaginatedResponse<EmployeeGoalDto> { Total = total, Data = data };
        }
    }
}
