using CyberErp.Hrms.App.Common;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.OrganizationUnits.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.OrganizationUnits
{
    public interface ICreateOrganizationUnit { Task<Guid> CreateAsync(CreateOrganizationUnitDto dto); }
    public interface IUpdateOrganizationUnit { Task UpdateAsync(UpdateOrganizationUnitDto dto); }
    public interface IDeleteOrganizationUnit { Task DeleteAsync(Guid id); }
    public interface IGetOrganizationUnitById { Task<OrganizationUnitDto> GetAsync(Guid id); }
    public interface IGetAllOrganizationUnits { Task<PaginatedResponse<OrganizationUnitDto>> GetAsync(GetAllRequest request); }
    public interface IGetOrganizationTree { Task<List<OrgUnitTreeNodeDto>> GetAsync(); }

    public class CreateOrganizationUnit(
        IRepository<OrganizationUnit> repository,
        ICurrentUserService currentUser,
        IValidator<CreateOrganizationUnitDto> validator,
        ILogger<CreateOrganizationUnit> logger) : ICreateOrganizationUnit
    {
        public async Task<Guid> CreateAsync(CreateOrganizationUnitDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Branch admins are pinned to their branch; Head Office chooses the branch freely.
            var branchId = currentUser.IsHeadOffice() ? dto.BranchId : currentUser.GetCurrentBranchId();

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.BranchId == branchId))
                throw new DuplicateException(nameof(OrganizationUnit), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue && !await repository.GetAll().AnyAsync(x => x.Id == dto.ParentId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.ParentId.Value.ToString(), "Parent organization unit was not found.");

            var type = Enum.Parse<OrganizationUnitType>(dto.UnitType);
            var entity = OrganizationUnit.Create(dto.Code, dto.Name, type, branchId, dto.ParentId, dto.WorkLocationId, dto.AllocatedHeadcount, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created OrganizationUnit {Id} ({Code}) in branch {BranchId}", entity.Id, entity.Code, branchId);
            return entity.Id;
        }
    }

    public class UpdateOrganizationUnit(
        IRepository<OrganizationUnit> repository,
        ICurrentUserService currentUser,
        IValidator<UpdateOrganizationUnitDto> validator,
        ILogger<UpdateOrganizationUnit> logger) : IUpdateOrganizationUnit
    {
        public async Task UpdateAsync(UpdateOrganizationUnitDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Load tracked so EF keeps the real original RowVersion for the concurrency check.
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(OrganizationUnit), dto.Id.ToString());

            // Head Office may reassign to another branch; branch admins keep it in their branch.
            var branchId = currentUser.IsHeadOffice() ? dto.BranchId : entity.BranchId;

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.BranchId == branchId && x.Id != dto.Id))
                throw new DuplicateException(nameof(OrganizationUnit), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue)
            {
                var parentMap = await repository.GetAll()
                    .Select(x => new { x.Id, x.ParentId })
                    .ToDictionaryAsync(x => x.Id, x => x.ParentId);
                if (HierarchyGuard.WouldCreateCycle(parentMap, dto.Id, dto.ParentId))
                    throw new ValidationException(nameof(dto.ParentId), "The selected parent would create a cycle in the organization hierarchy.");
            }

            var type = Enum.Parse<OrganizationUnitType>(dto.UnitType);
            entity.Update(dto.Code, dto.Name, type, branchId, dto.ParentId, dto.WorkLocationId, dto.AllocatedHeadcount, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated OrganizationUnit {Id}", entity.Id);
        }
    }

    public class DeleteOrganizationUnit(
        IRepository<OrganizationUnit> repository,
        IRepository<Position> positionRepository,
        ILogger<DeleteOrganizationUnit> logger) : IDeleteOrganizationUnit
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(OrganizationUnit), id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.ParentId == id))
                throw new ValidationException(nameof(id), "Cannot delete an organization unit that has child units. Remove or reassign them first.");

            if (await positionRepository.GetAll().AnyAsync(p => p.OrganizationUnitId == id))
                throw new ValidationException(nameof(id), "Cannot delete an organization unit that has positions. Remove or reassign them first.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted OrganizationUnit {Id}", id);
        }
    }

    public class GetOrganizationUnitById(IRepository<OrganizationUnit> repository) : IGetOrganizationUnitById
    {
        public async Task<OrganizationUnitDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll()
                .Where(o => o.Id == id)
                .Select(o => new OrganizationUnitDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    Name = o.Name,
                    UnitType = o.UnitType.ToString(),
                    BranchId = o.BranchId,
                    BranchName = o.Branch != null ? o.Branch.Name : null,
                    ParentId = o.ParentId,
                    ParentName = o.Parent != null ? o.Parent.Name : null,
                    WorkLocationId = o.WorkLocationId,
                    WorkLocationName = o.WorkLocation != null ? o.WorkLocation.Name : null,
                    AllocatedHeadcount = o.AllocatedHeadcount,
                    Description = o.Description,
                    IsActive = o.IsActive
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(OrganizationUnit), id.ToString());

            dto.HasChildren = await repository.GetAll().AnyAsync(x => x.ParentId == id);
            return dto;
        }
    }

    public class GetAllOrganizationUnits(IRepository<OrganizationUnit> repository) : IGetAllOrganizationUnits
    {
        public async Task<PaginatedResponse<OrganizationUnitDto>> GetAsync(GetAllRequest request)
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
                .OrderBy(x => x.UnitType).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(o => new OrganizationUnitDto
                {
                    Id = o.Id,
                    Code = o.Code,
                    Name = o.Name,
                    UnitType = o.UnitType.ToString(),
                    BranchId = o.BranchId,
                    BranchName = o.Branch != null ? o.Branch.Name : null,
                    ParentId = o.ParentId,
                    ParentName = o.Parent != null ? o.Parent.Name : null,
                    WorkLocationId = o.WorkLocationId,
                    WorkLocationName = o.WorkLocation != null ? o.WorkLocation.Name : null,
                    AllocatedHeadcount = o.AllocatedHeadcount,
                    Description = o.Description,
                    IsActive = o.IsActive
                })
                .ToListAsync();

            var pageIds = data.Select(d => d.Id).ToList();
            var parentsWithChildren = await repository.GetAll()
                .Where(x => x.ParentId != null && pageIds.Contains(x.ParentId.Value))
                .Select(x => x.ParentId!.Value)
                .Distinct()
                .ToListAsync();
            foreach (var row in data)
                row.HasChildren = parentsWithChildren.Contains(row.Id);

            return new PaginatedResponse<OrganizationUnitDto> { Total = total, Data = data };
        }
    }

    public class GetOrganizationTree(IRepository<OrganizationUnit> repository) : IGetOrganizationTree
    {
        public async Task<List<OrgUnitTreeNodeDto>> GetAsync()
        {
            // One query, assembled into a tree in memory (org structures are small).
            var all = await repository.GetAll()
                .OrderBy(x => x.UnitType).ThenBy(x => x.Name)
                .Select(o => new { o.Id, o.Code, o.Name, o.UnitType, o.AllocatedHeadcount, o.ParentId })
                .ToListAsync();

            var nodes = all.ToDictionary(
                x => x.Id,
                x => new OrgUnitTreeNodeDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    UnitType = x.UnitType.ToString(),
                    AllocatedHeadcount = x.AllocatedHeadcount
                });

            var roots = new List<OrgUnitTreeNodeDto>();
            foreach (var x in all)
            {
                var node = nodes[x.Id];
                if (x.ParentId.HasValue && nodes.TryGetValue(x.ParentId.Value, out var parent))
                    parent.Children.Add(node);
                else
                    roots.Add(node);
            }
            return roots;
        }
    }
}
