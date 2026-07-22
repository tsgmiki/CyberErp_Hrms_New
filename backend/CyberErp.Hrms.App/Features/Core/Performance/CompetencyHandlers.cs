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
    public class CompetencyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CompetencyCategoryId { get; set; }
        public string? CompetencyCategoryName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCompetencyDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid CompetencyCategoryId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompetencyDto : CreateCompetencyDto
    {
        public Guid Id { get; set; }
    }

    public class CreateCompetencyDtoValidator : AbstractValidator<CreateCompetencyDto>
    {
        public CreateCompetencyDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CompetencyCategoryId).NotEmpty();
            RuleFor(x => x.Description).MaximumLength(2000);
        }
    }

    public class UpdateCompetencyDtoValidator : AbstractValidator<UpdateCompetencyDto>
    {
        public UpdateCompetencyDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CompetencyCategoryId).NotEmpty();
            RuleFor(x => x.Description).MaximumLength(2000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ICreateCompetency { Task<Guid> CreateAsync(CreateCompetencyDto dto); }
    public interface IUpdateCompetency { Task UpdateAsync(UpdateCompetencyDto dto); }
    public interface IDeleteCompetency { Task DeleteAsync(Guid id); }
    public interface IGetCompetencyById { Task<CompetencyDto> GetAsync(Guid id); }
    public interface IGetAllCompetencies { Task<PaginatedResponse<CompetencyDto>> GetAsync(GetAllRequest request); }

    internal static class CompetencyMapper
    {
        // Category name resolved via the nav-less join in the query (kept out of the entity).
        internal static CompetencyDto Map(Competency c, string? categoryName) => new()
        {
            Id = c.Id,
            Name = c.Name,
            CompetencyCategoryId = c.CompetencyCategoryId,
            CompetencyCategoryName = categoryName,
            Description = c.Description,
            IsActive = c.IsActive
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class CreateCompetency(
        IRepository<Competency> repository,
        IRepository<CompetencyCategory> categoryRepository,
        IValidator<CreateCompetencyDto> validator,
        ILogger<CreateCompetency> logger) : ICreateCompetency
    {
        public async Task<Guid> CreateAsync(CreateCompetencyDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await categoryRepository.GetAll().AnyAsync(c => c.Id == dto.CompetencyCategoryId))
                throw new NotFoundException(nameof(CompetencyCategory), dto.CompetencyCategoryId.ToString());
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name))
                throw new DuplicateException(nameof(Competency), nameof(dto.Name), dto.Name);

            var entity = Competency.Create(dto.Name, dto.CompetencyCategoryId, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Competency {Id} ({Name})", entity.Id, entity.Name);
            return entity.Id;
        }
    }

    public class UpdateCompetency(
        IRepository<Competency> repository,
        IRepository<CompetencyCategory> categoryRepository,
        IValidator<UpdateCompetencyDto> validator,
        ILogger<UpdateCompetency> logger) : IUpdateCompetency
    {
        public async Task UpdateAsync(UpdateCompetencyDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(Competency), dto.Id.ToString());
            if (!await categoryRepository.GetAll().AnyAsync(c => c.Id == dto.CompetencyCategoryId))
                throw new NotFoundException(nameof(CompetencyCategory), dto.CompetencyCategoryId.ToString());
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(Competency), nameof(dto.Name), dto.Name);

            entity.Update(dto.Name, dto.CompetencyCategoryId, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated Competency {Id}", entity.Id);
        }
    }

    public class DeleteCompetency(
        IRepository<Competency> repository,
        IRepository<PositionCompetency> positionCompetencyRepository,
        ILogger<DeleteCompetency> logger) : IDeleteCompetency
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Competency), id.ToString());

            if (await positionCompetencyRepository.GetAll().AnyAsync(p => p.CompetencyId == id))
                throw new ValidationException(nameof(id), "Cannot delete a competency that is assigned to one or more positions.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Competency {Id}", id);
        }
    }

    public class GetCompetencyById(
        IRepository<Competency> repository,
        IRepository<CompetencyCategory> categoryRepository) : IGetCompetencyById
    {
        public async Task<CompetencyDto> GetAsync(Guid id)
        {
            var row = await repository.GetAll().Where(x => x.Id == id)
                .Join(categoryRepository.GetAll(), c => c.CompetencyCategoryId, cat => cat.Id,
                    (c, cat) => new { c, CategoryName = cat.Name })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Competency), id.ToString());
            return CompetencyMapper.Map(row.c, row.CategoryName);
        }
    }

    public class GetAllCompetencies(
        IRepository<Competency> repository,
        IRepository<CompetencyCategory> categoryRepository) : IGetAllCompetencies
    {
        public async Task<PaginatedResponse<CompetencyDto>> GetAsync(GetAllRequest request)
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
            if (request.CategoryId.HasValue)
                query = query.Where(x => x.CompetencyCategoryId == request.CategoryId.Value);

            var total = await query.CountAsync();
            var rows = await query.OrderBy(x => x.Name).Skip(skip).Take(take)
                .Join(categoryRepository.GetAll(), c => c.CompetencyCategoryId, cat => cat.Id,
                    (c, cat) => new { c, CategoryName = cat.Name })
                .ToListAsync();

            return new PaginatedResponse<CompetencyDto>
            {
                Total = total,
                Data = rows.Select(r => CompetencyMapper.Map(r.c, r.CategoryName)).ToList()
            };
        }
    }
}
