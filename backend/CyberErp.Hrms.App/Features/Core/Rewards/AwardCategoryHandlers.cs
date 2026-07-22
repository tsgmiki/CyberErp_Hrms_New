using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Rewards
{
    // ---- DTOs ---------------------------------------------------------------
    public class AwardCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Criteria { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveAwardCategoryDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Criteria { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class SaveAwardCategoryDtoValidator : AbstractValidator<SaveAwardCategoryDto>
    {
        public SaveAwardCategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Criteria).MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveAwardCategory { Task<Guid> SaveAsync(SaveAwardCategoryDto dto); }
    public interface IDeleteAwardCategory { Task DeleteAsync(Guid id); }
    public interface IGetAwardCategoryById { Task<AwardCategoryDto> GetAsync(Guid id); }
    public interface IGetAllAwardCategories { Task<PaginatedResponse<AwardCategoryDto>> GetAsync(GetAllRequest request); }

    internal static class AwardCategoryMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<AwardCategory, AwardCategoryDto>> Projection = c => new AwardCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Criteria = c.Criteria,
            IsActive = c.IsActive,
            SortOrder = c.SortOrder
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveAwardCategory(
        IRepository<AwardCategory> repository,
        IValidator<SaveAwardCategoryDto> validator,
        ILogger<SaveAwardCategory> logger) : ISaveAwardCategory
    {
        public async Task<Guid> SaveAsync(SaveAwardCategoryDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(AwardCategory), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(AwardCategory), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Description, dto.Criteria, dto.IsActive, dto.SortOrder);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated AwardCategory {Id}", entity.Id);
                return entity.Id;
            }

            var created = AwardCategory.Create(dto.Name, dto.Description, dto.Criteria, dto.IsActive, dto.SortOrder);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created AwardCategory {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteAwardCategory(
        IRepository<AwardCategory> repository,
        IRepository<RecognitionBadge> badgeRepository,
        ILogger<DeleteAwardCategory> logger) : IDeleteAwardCategory
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(AwardCategory), id.ToString());
            if (await badgeRepository.GetAll().AnyAsync(b => b.AwardCategoryId == id))
                throw new ValidationException(nameof(id), "Cannot delete a category that awards still reference.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted AwardCategory {Id}", id);
        }
    }

    public class GetAwardCategoryById(IRepository<AwardCategory> repository) : IGetAwardCategoryById
    {
        public async Task<AwardCategoryDto> GetAsync(Guid id)
        {
            return await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(AwardCategoryMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(AwardCategory), id.ToString());
        }
    }

    public class GetAllAwardCategories(IRepository<AwardCategory> repository) : IGetAllAwardCategories
    {
        public async Task<PaginatedResponse<AwardCategoryDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take).Select(AwardCategoryMapper.Projection).ToListAsync();

            return new PaginatedResponse<AwardCategoryDto> { Total = total, Data = data };
        }
    }
}
