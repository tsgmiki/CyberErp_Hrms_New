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
    public class RatingScaleLevelDto
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public int SortOrder { get; set; }
    }

    public class RatingScaleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ScoreType { get; set; } = nameof(RatingScoreType.Numeric);
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public List<RatingScaleLevelDto> Levels { get; set; } = [];
    }

    public class SaveRatingScaleLevelDto
    {
        public Guid? Id { get; set; }
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveRatingScaleDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ScoreType { get; set; } = nameof(RatingScoreType.Numeric);
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public List<SaveRatingScaleLevelDto> Levels { get; set; } = [];
    }

    public class SaveRatingScaleLevelDtoValidator : AbstractValidator<SaveRatingScaleLevelDto>
    {
        public SaveRatingScaleLevelDtoValidator()
        {
            RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }

    public class SaveRatingScaleDtoValidator : AbstractValidator<SaveRatingScaleDto>
    {
        public SaveRatingScaleDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ScoreType).NotEmpty()
                .Must(v => Enum.TryParse<RatingScoreType>(v, out _))
                .WithMessage("ScoreType must be Numeric or Percentage.");
            RuleFor(x => x.Levels).NotEmpty().WithMessage("At least one rating level is required.");
            RuleFor(x => x.Levels)
                .Must(levels => levels.Select(l => l.Value).Distinct().Count() == levels.Count)
                .WithMessage("Rating level values must be unique.")
                .When(x => x.Levels.Count > 0);
            RuleForEach(x => x.Levels).SetValidator(new SaveRatingScaleLevelDtoValidator());
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveRatingScale { Task<Guid> SaveAsync(SaveRatingScaleDto dto); }
    public interface IDeleteRatingScale { Task DeleteAsync(Guid id); }
    public interface IGetRatingScaleById { Task<RatingScaleDto> GetAsync(Guid id); }
    public interface IGetAllRatingScales { Task<PaginatedResponse<RatingScaleDto>> GetAsync(GetAllRequest request); }

    internal static class RatingScaleMapper
    {
        internal static RatingScaleDto Map(RatingScale x) => new()
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            ScoreType = x.ScoreType.ToString(),
            IsActive = x.IsActive,
            SortOrder = x.SortOrder,
            Levels = x.Levels.OrderBy(l => l.SortOrder).Select(l => new RatingScaleLevelDto
            {
                Id = l.Id,
                Value = l.Value,
                Label = l.Label,
                Description = l.Description,
                MinScore = l.MinScore,
                MaxScore = l.MaxScore,
                SortOrder = l.SortOrder
            }).ToList()
        };

        /// <summary>The repository stamps only aggregate roots — cascade-inserted levels copy it here.</summary>
        internal static void StampLevelTenant(RatingScale scale)
        {
            foreach (var level in scale.Levels)
                if (string.IsNullOrEmpty(level.TenantId))
                    level.TenantId = scale.TenantId;
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveRatingScale(
        IRepository<RatingScale> repository,
        IRepository<RatingScaleLevel> levelRepository,
        IValidator<SaveRatingScaleDto> validator,
        ILogger<SaveRatingScale> logger) : ISaveRatingScale
    {
        public async Task<Guid> SaveAsync(SaveRatingScaleDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(RatingScale), nameof(dto.Name), dto.Name);

            var scoreType = Enum.Parse<RatingScoreType>(dto.ScoreType);
            var specs = dto.Levels.Select(l => new RatingScaleLevelSpec(
                l.Id, l.Value, l.Label, l.Description, l.MinScore, l.MaxScore, l.SortOrder)).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().Include(x => x.Levels)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(RatingScale), dto.Id.Value.ToString());

                // Old level rows are replaced wholesale (values may change).
                foreach (var old in entity.Levels.ToList())
                    levelRepository.Delete(old);

                entity.Update(dto.Name, scoreType, dto.Description, dto.IsActive, dto.SortOrder);
                entity.SetLevels(specs);
                RatingScaleMapper.StampLevelTenant(entity);
                // Replacement levels are new rows: mark them Added explicitly (see DynamicForm/ClearanceDepartment).
                foreach (var level in entity.Levels)
                    await levelRepository.AddAsync(level);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated RatingScale {Id}", entity.Id);
                return entity.Id;
            }

            var created = RatingScale.Create(dto.Name, scoreType, dto.Description, dto.IsActive, dto.SortOrder);
            created.SetLevels(specs);
            await repository.AddAsync(created);   // stamps the root's TenantId
            RatingScaleMapper.StampLevelTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created RatingScale {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteRatingScale(
        IRepository<RatingScale> repository,
        IRepository<ReviewCycle> reviewCycleRepository,
        ILogger<DeleteRatingScale> logger) : IDeleteRatingScale
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(RatingScale), id.ToString());

            if (await reviewCycleRepository.GetAll().AnyAsync(c => c.RatingScaleId == id))
                throw new ValidationException(nameof(id), "Cannot delete a rating scale that is used by one or more review cycles.");

            repository.Delete(entity);   // levels cascade
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted RatingScale {Id}", id);
        }
    }

    public class GetRatingScaleById(IRepository<RatingScale> repository) : IGetRatingScaleById
    {
        public async Task<RatingScaleDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().Include(x => x.Levels).AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(RatingScale), id.ToString());
            return RatingScaleMapper.Map(entity);
        }
    }

    public class GetAllRatingScales(IRepository<RatingScale> repository) : IGetAllRatingScales
    {
        public async Task<PaginatedResponse<RatingScaleDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().Include(x => x.Levels).AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var scales = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take).ToListAsync();

            return new PaginatedResponse<RatingScaleDto> { Total = total, Data = scales.Select(RatingScaleMapper.Map).ToList() };
        }
    }
}
