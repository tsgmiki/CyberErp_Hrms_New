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
    public class RecognitionBadgeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateRecognitionBadgeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class UpdateRecognitionBadgeDto : CreateRecognitionBadgeDto
    {
        public Guid Id { get; set; }
    }

    public class CreateRecognitionBadgeDtoValidator : AbstractValidator<CreateRecognitionBadgeDto>
    {
        public CreateRecognitionBadgeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Color).MaximumLength(20);
            RuleFor(x => x.Icon).MaximumLength(50);
        }
    }

    public class UpdateRecognitionBadgeDtoValidator : AbstractValidator<UpdateRecognitionBadgeDto>
    {
        public UpdateRecognitionBadgeDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ICreateRecognitionBadge { Task<Guid> CreateAsync(CreateRecognitionBadgeDto dto); }
    public interface IUpdateRecognitionBadge { Task UpdateAsync(UpdateRecognitionBadgeDto dto); }
    public interface IDeleteRecognitionBadge { Task DeleteAsync(Guid id); }
    public interface IGetRecognitionBadgeById { Task<RecognitionBadgeDto> GetAsync(Guid id); }
    public interface IGetAllRecognitionBadges { Task<PaginatedResponse<RecognitionBadgeDto>> GetAsync(GetAllRequest request); }

    internal static class RecognitionBadgeMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<RecognitionBadge, RecognitionBadgeDto>> Projection = b => new RecognitionBadgeDto
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            Color = b.Color,
            Icon = b.Icon,
            IsActive = b.IsActive,
            SortOrder = b.SortOrder
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class CreateRecognitionBadge(
        IRepository<RecognitionBadge> repository,
        IValidator<CreateRecognitionBadgeDto> validator,
        ILogger<CreateRecognitionBadge> logger) : ICreateRecognitionBadge
    {
        public async Task<Guid> CreateAsync(CreateRecognitionBadgeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name))
                throw new DuplicateException(nameof(RecognitionBadge), nameof(dto.Name), dto.Name);

            var entity = RecognitionBadge.Create(dto.Name, dto.Description, dto.Color, dto.Icon, dto.IsActive, dto.SortOrder);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created RecognitionBadge {Id} ({Name})", entity.Id, entity.Name);
            return entity.Id;
        }
    }

    public class UpdateRecognitionBadge(
        IRepository<RecognitionBadge> repository,
        IValidator<UpdateRecognitionBadgeDto> validator,
        ILogger<UpdateRecognitionBadge> logger) : IUpdateRecognitionBadge
    {
        public async Task UpdateAsync(UpdateRecognitionBadgeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(RecognitionBadge), dto.Id.ToString());
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(RecognitionBadge), nameof(dto.Name), dto.Name);

            entity.Update(dto.Name, dto.Description, dto.Color, dto.Icon, dto.IsActive, dto.SortOrder);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated RecognitionBadge {Id}", entity.Id);
        }
    }

    public class DeleteRecognitionBadge(
        IRepository<RecognitionBadge> repository,
        IRepository<EmployeeRecognition> recognitionRepository,
        ILogger<DeleteRecognitionBadge> logger) : IDeleteRecognitionBadge
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(RecognitionBadge), id.ToString());
            if (await recognitionRepository.GetAll().AnyAsync(r => r.RecognitionBadgeId == id))
                throw new ValidationException(nameof(id), "Cannot delete a badge that has been granted to employees.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted RecognitionBadge {Id}", id);
        }
    }

    public class GetRecognitionBadgeById(IRepository<RecognitionBadge> repository) : IGetRecognitionBadgeById
    {
        public async Task<RecognitionBadgeDto> GetAsync(Guid id)
        {
            return await repository.GetAll().Where(x => x.Id == id)
                .Select(RecognitionBadgeMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(RecognitionBadge), id.ToString());
        }
    }

    public class GetAllRecognitionBadges(IRepository<RecognitionBadge> repository) : IGetAllRecognitionBadges
    {
        public async Task<PaginatedResponse<RecognitionBadgeDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take).Select(RecognitionBadgeMapper.Projection).ToListAsync();

            return new PaginatedResponse<RecognitionBadgeDto> { Total = total, Data = data };
        }
    }
}
