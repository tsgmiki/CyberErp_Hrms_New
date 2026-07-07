using CyberErp.Hrms.App.Common;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Branches.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Branches
{
    public interface ICreateBranch { Task<Guid> CreateAsync(CreateBranchDto dto); }
    public interface IUpdateBranch { Task UpdateAsync(UpdateBranchDto dto); }
    public interface IDeleteBranch { Task DeleteAsync(Guid id); }
    public interface IGetBranchById { Task<BranchDto> GetAsync(Guid id); }
    public interface IGetAllBranches { Task<PaginatedResponse<BranchDto>> GetAsync(GetAllRequest request); }

    public class CreateBranch(
        IRepository<Branch> repository,
        IValidator<CreateBranchDto> validator,
        ILogger<CreateBranch> logger) : ICreateBranch
    {
        public async Task<Guid> CreateAsync(CreateBranchDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code))
                throw new DuplicateException(nameof(Branch), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue && !await repository.GetAll().AnyAsync(x => x.Id == dto.ParentId.Value))
                throw new NotFoundException(nameof(Branch), dto.ParentId.Value.ToString(), "Parent branch was not found.");

            var entity = Branch.Create(dto.Code, dto.Name, dto.ParentId, dto.Description, dto.Address, dto.IsHeadOffice, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Branch {Id} ({Code})", entity.Id, entity.Code);
            return entity.Id;
        }
    }

    public class UpdateBranch(
        IRepository<Branch> repository,
        IValidator<UpdateBranchDto> validator,
        ILogger<UpdateBranch> logger) : IUpdateBranch
    {
        public async Task UpdateAsync(UpdateBranchDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Branch), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(Branch), nameof(dto.Code), dto.Code);

            if (dto.ParentId.HasValue)
            {
                var parentMap = await repository.GetAll()
                    .Select(x => new { x.Id, x.ParentId })
                    .ToDictionaryAsync(x => x.Id, x => x.ParentId);
                if (HierarchyGuard.WouldCreateCycle(parentMap, dto.Id, dto.ParentId))
                    throw new ValidationException(nameof(dto.ParentId), "The selected parent would create a cycle in the branch hierarchy.");
            }

            entity.Update(dto.Code, dto.Name, dto.ParentId, dto.Description, dto.Address, dto.IsHeadOffice, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated Branch {Id}", entity.Id);
        }
    }

    public class DeleteBranch(
        IRepository<Branch> repository,
        IRepository<OrganizationUnit> orgUnitRepository,
        ILogger<DeleteBranch> logger) : IDeleteBranch
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Branch), id.ToString());

            if (await repository.GetAll().AnyAsync(b => b.ParentId == id))
                throw new ValidationException(nameof(id), "Cannot delete a branch that has child branches.");

            if (await orgUnitRepository.GetAll().AnyAsync(o => o.BranchId == id))
                throw new ValidationException(nameof(id), "Cannot delete a branch that still has organization units. Reassign or remove them first.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Branch {Id}", id);
        }
    }

    public class GetBranchById(IRepository<Branch> repository) : IGetBranchById
    {
        public async Task<BranchDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll()
                .Where(b => b.Id == id)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Name = b.Name,
                    Description = b.Description,
                    Address = b.Address,
                    IsHeadOffice = b.IsHeadOffice,
                    IsActive = b.IsActive,
                    ParentId = b.ParentId,
                    ParentName = b.Parent != null ? b.Parent.Name : null
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Branch), id.ToString());

            dto.HasChildren = await repository.GetAll().AnyAsync(b => b.ParentId == id);
            return dto;
        }
    }

    public class GetAllBranches(IRepository<Branch> repository) : IGetAllBranches
    {
        public async Task<PaginatedResponse<BranchDto>> GetAsync(GetAllRequest request)
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
                .OrderBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Name = b.Name,
                    Description = b.Description,
                    Address = b.Address,
                    IsHeadOffice = b.IsHeadOffice,
                    IsActive = b.IsActive,
                    ParentId = b.ParentId,
                    ParentName = b.Parent != null ? b.Parent.Name : null
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

            return new PaginatedResponse<BranchDto> { Total = total, Data = data };
        }
    }
}
