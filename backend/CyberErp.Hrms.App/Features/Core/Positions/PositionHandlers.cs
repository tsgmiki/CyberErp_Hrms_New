using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Positions.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Positions
{
    public interface ICreatePosition { Task<Guid> CreateAsync(CreatePositionDto dto); }
    public interface IUpdatePosition { Task UpdateAsync(UpdatePositionDto dto); }
    public interface IDeletePosition { Task DeleteAsync(Guid id); }
    public interface IGetPositionById { Task<PositionDto> GetAsync(Guid id); }
    public interface IGetAllPositions { Task<PaginatedResponse<PositionDto>> GetAsync(GetAllRequest request); }

    public class CreatePosition(
        IRepository<Position> repository,
        IRepository<OrganizationUnit> orgUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IValidator<CreatePositionDto> validator,
        ILogger<CreatePosition> logger) : ICreatePosition
    {
        public async Task<Guid> CreateAsync(CreatePositionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            await EnsureReferencesExistAsync(dto, orgUnitRepository, positionClassRepository);

            // A position inherits its branch from its organization unit (isolation is transitive).
            var branchId = await orgUnitRepository.GetAll()
                .Where(o => o.Id == dto.OrganizationUnitId)
                .Select(o => o.BranchId)
                .FirstOrDefaultAsync();

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.BranchId == branchId))
                throw new DuplicateException(nameof(Position), nameof(dto.Code), dto.Code);

            var entity = Position.Create(dto.Code, dto.PositionClassId, dto.OrganizationUnitId, branchId);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Position {Id} ({Code})", entity.Id, entity.Code);
            return entity.Id;
        }

        internal static async Task EnsureReferencesExistAsync(
            CreatePositionDto dto,
            IRepository<OrganizationUnit> orgUnits,
            IRepository<PositionClass> positionClasses)
        {
            if (!await orgUnits.GetAll().AnyAsync(x => x.Id == dto.OrganizationUnitId))
                throw new NotFoundException(nameof(OrganizationUnit), dto.OrganizationUnitId.ToString());
            if (!await positionClasses.GetAll().AnyAsync(x => x.Id == dto.PositionClassId))
                throw new NotFoundException(nameof(PositionClass), dto.PositionClassId.ToString());
        }
    }

    public class UpdatePosition(
        IRepository<Position> repository,
        IRepository<OrganizationUnit> orgUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IValidator<UpdatePositionDto> validator,
        ILogger<UpdatePosition> logger) : IUpdatePosition
    {
        public async Task UpdateAsync(UpdatePositionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Position), dto.Id.ToString());

            await CreatePosition.EnsureReferencesExistAsync(dto, orgUnitRepository, positionClassRepository);

            var branchId = await orgUnitRepository.GetAll()
                .Where(o => o.Id == dto.OrganizationUnitId)
                .Select(o => o.BranchId)
                .FirstOrDefaultAsync();

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.BranchId == branchId && x.Id != dto.Id))
                throw new DuplicateException(nameof(Position), nameof(dto.Code), dto.Code);

            entity.Update(dto.Code, dto.PositionClassId, dto.OrganizationUnitId, branchId);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated Position {Id}", entity.Id);
        }
    }

    public class DeletePosition(
        IRepository<Position> repository,
        ILogger<DeletePosition> logger) : IDeletePosition
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Position), id.ToString());

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Position {Id}", id);
        }
    }

    public class GetPositionById(IRepository<Position> repository) : IGetPositionById
    {
        public async Task<PositionDto> GetAsync(Guid id)
        {
            return await repository.GetAll()
                .Where(p => p.Id == id)
                .Select(Projection)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Position), id.ToString());
        }

        /// <summary>Shared read projection — an Expression so EF Core can translate it to SQL (LEFT JOINs).</summary>
        internal static readonly System.Linq.Expressions.Expression<Func<Position, PositionDto>> Projection = p => new PositionDto
        {
            Id = p.Id,
            Code = p.Code,
            PositionClassId = p.PositionClassId,
            PositionClassTitle = p.PositionClass != null ? p.PositionClass.Title : null,
            OrganizationUnitId = p.OrganizationUnitId,
            OrganizationUnitName = p.OrganizationUnit != null ? p.OrganizationUnit.Name : null,
            BranchId = p.BranchId,
            BranchName = p.Branch != null ? p.Branch.Name : null,
            IsVacant = p.IsVacant
        };
    }

    public class GetAllPositions(IRepository<Position> repository) : IGetAllPositions
    {
        public async Task<PaginatedResponse<PositionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            // parentId scopes positions to an organization unit (drives the tree → grid view).
            if (request.ParentId.HasValue)
                query = query.Where(x => x.OrganizationUnitId == request.ParentId.Value);

            // The employee form requests vacant-only positions for its placement dropdown.
            if (request.IsVacant == true)
                query = query.Where(x => x.IsVacant);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Code.Contains(term) ||
                                         (x.PositionClass != null && x.PositionClass.Title.Contains(term)));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Code)
                .Skip(skip).Take(take)
                .Select(GetPositionById.Projection)
                .ToListAsync();

            return new PaginatedResponse<PositionDto> { Total = total, Data = data };
        }
    }
}
