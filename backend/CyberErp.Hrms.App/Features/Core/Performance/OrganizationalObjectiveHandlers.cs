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
    public class OrganizationalObjectiveDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid ReviewCycleId { get; set; }
        public string? ReviewCycleName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? ParentObjectiveId { get; set; }
        public string? ParentObjectiveTitle { get; set; }
        public decimal Weight { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SaveOrganizationalObjectiveDto
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid ReviewCycleId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? ParentObjectiveId { get; set; }
        public decimal Weight { get; set; }
        public string Status { get; set; } = nameof(ObjectiveStatus.Draft);
    }

    public class SaveOrganizationalObjectiveDtoValidator : AbstractValidator<SaveOrganizationalObjectiveDto>
    {
        public SaveOrganizationalObjectiveDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.ReviewCycleId).NotEmpty();
            RuleFor(x => x.Weight).InclusiveBetween(0, 100);
            RuleFor(x => x.Status).NotEmpty()
                .Must(v => Enum.TryParse<ObjectiveStatus>(v, out _))
                .WithMessage("Status must be one of: Draft, Active, Closed.");
            RuleFor(x => x.ParentObjectiveId)
                .Must((dto, parent) => parent != dto.Id)
                .WithMessage("An objective cannot be its own parent.")
                .When(x => x.ParentObjectiveId.HasValue);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveOrganizationalObjective { Task<Guid> SaveAsync(SaveOrganizationalObjectiveDto dto); }
    public interface IDeleteOrganizationalObjective { Task DeleteAsync(Guid id); }
    public interface IGetOrganizationalObjectiveById { Task<OrganizationalObjectiveDto> GetAsync(Guid id); }
    public interface IGetAllOrganizationalObjectives { Task<PaginatedResponse<OrganizationalObjectiveDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveOrganizationalObjective(
        IRepository<OrganizationalObjective> repository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IValidator<SaveOrganizationalObjectiveDto> validator,
        ILogger<SaveOrganizationalObjective> logger) : ISaveOrganizationalObjective
    {
        public async Task<Guid> SaveAsync(SaveOrganizationalObjectiveDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await reviewCycleRepository.GetAll().AnyAsync(c => c.Id == dto.ReviewCycleId))
                throw new NotFoundException(nameof(ReviewCycle), dto.ReviewCycleId.ToString());
            if (dto.OrganizationUnitId.HasValue &&
                !await organizationUnitRepository.GetAll().AnyAsync(u => u.Id == dto.OrganizationUnitId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.OrganizationUnitId.Value.ToString());
            if (dto.ParentObjectiveId.HasValue &&
                !await repository.GetAll().AnyAsync(o => o.Id == dto.ParentObjectiveId.Value))
                throw new NotFoundException(nameof(OrganizationalObjective), dto.ParentObjectiveId.Value.ToString());
            if (await repository.GetAll().AnyAsync(x => x.Title == dto.Title && x.ReviewCycleId == dto.ReviewCycleId
                    && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(OrganizationalObjective), nameof(dto.Title), dto.Title);

            var status = Enum.Parse<ObjectiveStatus>(dto.Status);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(OrganizationalObjective), dto.Id.Value.ToString());
                entity.Update(dto.Title, dto.ReviewCycleId, dto.Description, dto.OrganizationUnitId,
                    dto.ParentObjectiveId, dto.Weight, status);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated OrganizationalObjective {Id}", entity.Id);
                return entity.Id;
            }

            var created = OrganizationalObjective.Create(dto.Title, dto.ReviewCycleId, dto.Description,
                dto.OrganizationUnitId, dto.ParentObjectiveId, dto.Weight, status);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created OrganizationalObjective {Id} ({Title})", created.Id, created.Title);
            return created.Id;
        }
    }

    public class DeleteOrganizationalObjective(
        IRepository<OrganizationalObjective> repository,
        IRepository<EmployeeGoal> goalRepository,
        ILogger<DeleteOrganizationalObjective> logger) : IDeleteOrganizationalObjective
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(OrganizationalObjective), id.ToString());

            if (await repository.GetAll().AnyAsync(o => o.ParentObjectiveId == id))
                throw new ValidationException(nameof(id), "Cannot delete an objective that has child objectives cascading from it.");
            if (await goalRepository.GetAll().AnyAsync(g => g.OrganizationalObjectiveId == id))
                throw new ValidationException(nameof(id), "Cannot delete an objective that has employee goals linked to it.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted OrganizationalObjective {Id}", id);
        }
    }

    public class GetOrganizationalObjectiveById(
        IRepository<OrganizationalObjective> repository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationUnit> organizationUnitRepository) : IGetOrganizationalObjectiveById
    {
        public async Task<OrganizationalObjectiveDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(OrganizationalObjective), id.ToString());
            var cycleName = await reviewCycleRepository.GetAll().Where(c => c.Id == entity.ReviewCycleId).Select(c => c.Name).FirstOrDefaultAsync();
            var unitName = entity.OrganizationUnitId.HasValue
                ? await organizationUnitRepository.GetAll().Where(u => u.Id == entity.OrganizationUnitId.Value).Select(u => u.Name).FirstOrDefaultAsync()
                : null;
            var parentTitle = entity.ParentObjectiveId.HasValue
                ? await repository.GetAll().Where(o => o.Id == entity.ParentObjectiveId.Value).Select(o => o.Title).FirstOrDefaultAsync()
                : null;
            return OrganizationalObjectiveMapper.Map(entity, cycleName, unitName, parentTitle);
        }
    }

    public class GetAllOrganizationalObjectives(
        IRepository<OrganizationalObjective> repository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<OrganizationUnit> organizationUnitRepository) : IGetAllOrganizationalObjectives
    {
        public async Task<PaginatedResponse<OrganizationalObjectiveDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Title.Contains(request.SearchText.Trim()));
            if (request.ReviewCycleId.HasValue)
                query = query.Where(x => x.ReviewCycleId == request.ReviewCycleId.Value);
            if (request.OrganizationUnitId.HasValue)
                query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.ParentId.HasValue)
                query = query.Where(x => x.ParentObjectiveId == request.ParentId.Value);
            if (request.IsRoot == true)
                query = query.Where(x => x.ParentObjectiveId == null);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ObjectiveStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var cycles = reviewCycleRepository.GetAll();
            var units = organizationUnitRepository.GetAll();
            var self = repository.GetAll();
            var data = await query
                .OrderBy(x => x.Title)
                .Skip(skip).Take(take)
                .Select(x => new OrganizationalObjectiveDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    ReviewCycleId = x.ReviewCycleId,
                    ReviewCycleName = cycles.Where(c => c.Id == x.ReviewCycleId).Select(c => c.Name).FirstOrDefault(),
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = units.Where(u => u.Id == x.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                    ParentObjectiveId = x.ParentObjectiveId,
                    ParentObjectiveTitle = self.Where(o => o.Id == x.ParentObjectiveId).Select(o => o.Title).FirstOrDefault(),
                    Weight = x.Weight,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            return new PaginatedResponse<OrganizationalObjectiveDto> { Total = total, Data = data };
        }
    }

    internal static class OrganizationalObjectiveMapper
    {
        internal static OrganizationalObjectiveDto Map(OrganizationalObjective x, string? cycleName,
            string? unitName, string? parentTitle) => new()
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            ReviewCycleId = x.ReviewCycleId,
            ReviewCycleName = cycleName,
            OrganizationUnitId = x.OrganizationUnitId,
            OrganizationUnitName = unitName,
            ParentObjectiveId = x.ParentObjectiveId,
            ParentObjectiveTitle = parentTitle,
            Weight = x.Weight,
            Status = x.Status.ToString()
        };
    }
}
