using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class TrainingCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveTrainingCategoryDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class SaveTrainingCategoryDtoValidator : AbstractValidator<SaveTrainingCategoryDto>
    {
        public SaveTrainingCategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTrainingCategory { Task<Guid> SaveAsync(SaveTrainingCategoryDto dto); }
    public interface IDeleteTrainingCategory { Task DeleteAsync(Guid id); }
    public interface IGetTrainingCategoryById { Task<TrainingCategoryDto> GetAsync(Guid id); }
    public interface IGetAllTrainingCategories { Task<PaginatedResponse<TrainingCategoryDto>> GetAsync(GetAllRequest request); }

    internal static class TrainingCategoryMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<TrainingCategory, TrainingCategoryDto>> Projection = c => new TrainingCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            SortOrder = c.SortOrder
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTrainingCategory(
        IRepository<TrainingCategory> repository,
        IValidator<SaveTrainingCategoryDto> validator,
        ILogger<SaveTrainingCategory> logger) : ISaveTrainingCategory
    {
        public async Task<Guid> SaveAsync(SaveTrainingCategoryDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(TrainingCategory), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TrainingCategory), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Description, dto.IsActive, dto.SortOrder);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated TrainingCategory {Id}", entity.Id);
                return entity.Id;
            }

            var created = TrainingCategory.Create(dto.Name, dto.Description, dto.IsActive, dto.SortOrder);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created TrainingCategory {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteTrainingCategory(
        IRepository<TrainingCategory> repository,
        IRepository<TrainingCourse> courseRepository,
        ILogger<DeleteTrainingCategory> logger) : IDeleteTrainingCategory
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(TrainingCategory), id.ToString());
            if (await courseRepository.GetAll().AnyAsync(c => c.TrainingCategoryId == id))
                throw new ValidationException(nameof(id), "Cannot delete a category that courses still reference.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted TrainingCategory {Id}", id);
        }
    }

    public class GetTrainingCategoryById(IRepository<TrainingCategory> repository) : IGetTrainingCategoryById
    {
        public async Task<TrainingCategoryDto> GetAsync(Guid id)
        {
            return await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(TrainingCategoryMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TrainingCategory), id.ToString());
        }
    }

    public class GetAllTrainingCategories(IRepository<TrainingCategory> repository) : IGetAllTrainingCategories
    {
        public async Task<PaginatedResponse<TrainingCategoryDto>> GetAsync(GetAllRequest request)
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
                .Skip(skip).Take(take).Select(TrainingCategoryMapper.Projection).ToListAsync();

            return new PaginatedResponse<TrainingCategoryDto> { Total = total, Data = data };
        }
    }
}
