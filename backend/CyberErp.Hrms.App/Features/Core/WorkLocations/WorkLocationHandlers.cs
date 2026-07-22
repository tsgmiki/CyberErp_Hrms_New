using CyberErp.Hrms.App.Common;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.WorkLocations.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.WorkLocations
{
    public interface ICreateWorkLocation { Task<Guid> CreateAsync(CreateWorkLocationDto dto); }
    public interface IUpdateWorkLocation { Task UpdateAsync(UpdateWorkLocationDto dto); }
    public interface IDeleteWorkLocation { Task DeleteAsync(Guid id); }
    public interface IGetWorkLocationById { Task<WorkLocationDto> GetAsync(Guid id); }
    public interface IGetAllWorkLocations { Task<PaginatedResponse<WorkLocationDto>> GetAsync(GetAllRequest request); }

    public class CreateWorkLocation(
        IRepository<WorkLocation> repository,
        IValidator<CreateWorkLocationDto> validator,
        ILogger<CreateWorkLocation> logger) : ICreateWorkLocation
    {
        public async Task<Guid> CreateAsync(CreateWorkLocationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code))
                throw new DuplicateException(nameof(WorkLocation), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue && !await repository.GetAll().AnyAsync(x => x.Id == dto.ParentId.Value))
                throw new NotFoundException(nameof(WorkLocation), dto.ParentId.Value.ToString(), "Parent work location was not found.");

            var type = Enum.Parse<WorkLocationType>(dto.LocationType);
            var entity = WorkLocation.Create(dto.Code, dto.Name, type, dto.ParentId, dto.Address, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created WorkLocation {Id} ({Code})", entity.Id, entity.Code);
            return entity.Id;
        }
    }

    public class UpdateWorkLocation(
        IRepository<WorkLocation> repository,
        IValidator<UpdateWorkLocationDto> validator,
        ILogger<UpdateWorkLocation> logger) : IUpdateWorkLocation
    {
        public async Task UpdateAsync(UpdateWorkLocationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Load tracked so EF keeps the real original RowVersion for the concurrency check.
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(WorkLocation), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(WorkLocation), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue)
            {
                var parentMap = await repository.GetAll()
                    .Select(x => new { x.Id, x.ParentId })
                    .ToDictionaryAsync(x => x.Id, x => x.ParentId);
                if (HierarchyGuard.WouldCreateCycle(parentMap, dto.Id, dto.ParentId))
                    throw new ValidationException(nameof(dto.ParentId), "The selected parent would create a cycle in the location hierarchy.");
            }

            var type = Enum.Parse<WorkLocationType>(dto.LocationType);
            entity.Update(dto.Code, dto.Name, type, dto.ParentId, dto.Address, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated WorkLocation {Id}", entity.Id);
        }
    }

    public class DeleteWorkLocation(
        IRepository<WorkLocation> repository,
        ILogger<DeleteWorkLocation> logger) : IDeleteWorkLocation
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(WorkLocation), id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.ParentId == id))
                throw new ValidationException(nameof(id), "Cannot delete a work location that has child locations. Remove or reassign them first.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted WorkLocation {Id}", id);
        }
    }

    public class GetWorkLocationById(IRepository<WorkLocation> repository) : IGetWorkLocationById
    {
        public async Task<WorkLocationDto> GetAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(WorkLocation), id.ToString());

            string? parentName = null;
            if (entity.ParentId.HasValue)
                parentName = await repository.GetAll().Where(x => x.Id == entity.ParentId).Select(x => x.Name).FirstOrDefaultAsync();

            var hasChildren = await repository.GetAll().AnyAsync(x => x.ParentId == id);

            return new WorkLocationDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                LocationType = entity.LocationType.ToString(),
                ParentId = entity.ParentId,
                ParentName = parentName,
                Address = entity.Address,
                Description = entity.Description,
                IsActive = entity.IsActive,
                HasChildren = hasChildren
            };
        }
    }

    public class GetAllWorkLocations(IRepository<WorkLocation> repository) : IGetAllWorkLocations
    {
        public async Task<PaginatedResponse<WorkLocationDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (request.IsRoot == true)
                query = query.Where(x => x.ParentId == null);
            else if (request.ParentId.HasValue)
                query = query.Where(x => x.ParentId == request.ParentId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.LocationType).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(l => new WorkLocationDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                    LocationType = l.LocationType.ToString(),
                    ParentId = l.ParentId,
                    ParentName = l.Parent != null ? l.Parent.Name : null,
                    Address = l.Address,
                    Description = l.Description,
                    IsActive = l.IsActive
                })
                .ToListAsync();

            // Flag which of the returned rows are parents (one extra query, no N+1)
            var pageIds = data.Select(d => d.Id).ToList();
            var parentsWithChildren = await repository.GetAll()
                .Where(x => x.ParentId != null && pageIds.Contains(x.ParentId.Value))
                .Select(x => x.ParentId!.Value)
                .Distinct()
                .ToListAsync();
            foreach (var row in data)
                row.HasChildren = parentsWithChildren.Contains(row.Id);

            return new PaginatedResponse<WorkLocationDto> { Total = total, Data = data };
        }
    }
}
