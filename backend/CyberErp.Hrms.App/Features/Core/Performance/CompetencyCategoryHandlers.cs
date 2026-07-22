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
    public class CompetencyCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCompetencyCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompetencyCategoryDto : CreateCompetencyCategoryDto
    {
        public Guid Id { get; set; }
    }

    public class CreateCompetencyCategoryDtoValidator : AbstractValidator<CreateCompetencyCategoryDto>
    {
        public CreateCompetencyCategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    public class UpdateCompetencyCategoryDtoValidator : AbstractValidator<UpdateCompetencyCategoryDto>
    {
        public UpdateCompetencyCategoryDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ICreateCompetencyCategory { Task<Guid> CreateAsync(CreateCompetencyCategoryDto dto); }
    public interface IUpdateCompetencyCategory { Task UpdateAsync(UpdateCompetencyCategoryDto dto); }
    public interface IDeleteCompetencyCategory { Task DeleteAsync(Guid id); }
    public interface IGetCompetencyCategoryById { Task<CompetencyCategoryDto> GetAsync(Guid id); }
    public interface IGetAllCompetencyCategories { Task<PaginatedResponse<CompetencyCategoryDto>> GetAsync(GetAllRequest request); }

    internal static class CompetencyCategoryMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<CompetencyCategory, CompetencyCategoryDto>> Projection = c => new CompetencyCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class CreateCompetencyCategory(
        IRepository<CompetencyCategory> repository,
        IValidator<CreateCompetencyCategoryDto> validator,
        ILogger<CreateCompetencyCategory> logger) : ICreateCompetencyCategory
    {
        public async Task<Guid> CreateAsync(CreateCompetencyCategoryDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name))
                throw new DuplicateException(nameof(CompetencyCategory), nameof(dto.Name), dto.Name);

            var entity = CompetencyCategory.Create(dto.Name, dto.Description, dto.SortOrder, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created CompetencyCategory {Id} ({Name})", entity.Id, entity.Name);
            return entity.Id;
        }
    }

    public class UpdateCompetencyCategory(
        IRepository<CompetencyCategory> repository,
        IValidator<UpdateCompetencyCategoryDto> validator,
        ILogger<UpdateCompetencyCategory> logger) : IUpdateCompetencyCategory
    {
        public async Task UpdateAsync(UpdateCompetencyCategoryDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(CompetencyCategory), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(CompetencyCategory), nameof(dto.Name), dto.Name);

            entity.Update(dto.Name, dto.Description, dto.SortOrder, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated CompetencyCategory {Id}", entity.Id);
        }
    }

    public class DeleteCompetencyCategory(
        IRepository<CompetencyCategory> repository,
        IRepository<Competency> competencyRepository,
        ILogger<DeleteCompetencyCategory> logger) : IDeleteCompetencyCategory
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(CompetencyCategory), id.ToString());

            if (await competencyRepository.GetAll().AnyAsync(c => c.CompetencyCategoryId == id))
                throw new ValidationException(nameof(id), "Cannot delete a category that is referenced by one or more competencies.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted CompetencyCategory {Id}", id);
        }
    }

    public class GetCompetencyCategoryById(IRepository<CompetencyCategory> repository) : IGetCompetencyCategoryById
    {
        public async Task<CompetencyCategoryDto> GetAsync(Guid id)
        {
            return await repository.GetAll().Where(x => x.Id == id)
                .Select(CompetencyCategoryMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(CompetencyCategory), id.ToString());
        }
    }

    public class GetAllCompetencyCategories(IRepository<CompetencyCategory> repository) : IGetAllCompetencyCategories
    {
        public async Task<PaginatedResponse<CompetencyCategoryDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take).Select(CompetencyCategoryMapper.Projection).ToListAsync();

            return new PaginatedResponse<CompetencyCategoryDto> { Total = total, Data = data };
        }
    }
}
