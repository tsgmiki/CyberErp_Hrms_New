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
    public class PositionCompetencyDto
    {
        public Guid Id { get; set; }
        public Guid CompetencyId { get; set; }
        public string? CompetencyName { get; set; }
        public string? CompetencyCategoryName { get; set; }
        public decimal Weight { get; set; }
    }

    public class SavePositionCompetencyItemDto
    {
        public Guid CompetencyId { get; set; }
        public decimal Weight { get; set; }
    }

    public class SavePositionCompetenciesDto
    {
        public Guid PositionId { get; set; }
        public List<SavePositionCompetencyItemDto> Items { get; set; } = [];
    }

    public class SavePositionCompetenciesDtoValidator : AbstractValidator<SavePositionCompetenciesDto>
    {
        public SavePositionCompetenciesDtoValidator()
        {
            RuleFor(x => x.PositionId).NotEmpty();
            RuleForEach(x => x.Items).ChildRules(i =>
            {
                i.RuleFor(y => y.CompetencyId).NotEmpty();
                i.RuleFor(y => y.Weight).InclusiveBetween(0, 100);
            });
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetPositionCompetencies { Task<List<PositionCompetencyDto>> GetAsync(Guid positionId); }
    public interface ISavePositionCompetencies { Task SaveAsync(SavePositionCompetenciesDto dto); }

    // ---- Handlers -----------------------------------------------------------
    public class GetPositionCompetencies(
        IRepository<PositionCompetency> repository,
        IRepository<Competency> competencyRepository,
        IRepository<CompetencyCategory> categoryRepository) : IGetPositionCompetencies
    {
        public async Task<List<PositionCompetencyDto>> GetAsync(Guid positionId)
        {
            return await repository.GetAll()
                .Where(pc => pc.PositionId == positionId)
                .Join(competencyRepository.GetAll(), pc => pc.CompetencyId, c => c.Id, (pc, c) => new { pc, c })
                .Join(categoryRepository.GetAll(), x => x.c.CompetencyCategoryId, cat => cat.Id, (x, cat) => new { x.pc, x.c, cat })
                .OrderBy(x => x.cat.SortOrder).ThenBy(x => x.c.Name)
                .Select(x => new PositionCompetencyDto
                {
                    Id = x.pc.Id,
                    CompetencyId = x.pc.CompetencyId,
                    CompetencyName = x.c.Name,
                    CompetencyCategoryName = x.cat.Name,
                    Weight = x.pc.Weight
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }

    /// <summary>
    /// Replaces the full competency set assigned to a position (HC123/HC124). Each competency appears
    /// once with a weighting factor; unlisted mappings are removed. One position's set is small, so the
    /// replace is a bounded delete + insert in a single transaction.
    /// </summary>
    public class SavePositionCompetencies(
        IRepository<PositionCompetency> repository,
        IRepository<Position> positionRepository,
        IRepository<Competency> competencyRepository,
        IValidator<SavePositionCompetenciesDto> validator,
        ILogger<SavePositionCompetencies> logger) : ISavePositionCompetencies
    {
        public async Task SaveAsync(SavePositionCompetenciesDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await positionRepository.GetAll().AnyAsync(p => p.Id == dto.PositionId))
                throw new NotFoundException(nameof(Position), dto.PositionId.ToString());

            // De-dupe by competency (last weight wins) and validate the competencies exist.
            var items = dto.Items
                .GroupBy(i => i.CompetencyId)
                .ToDictionary(g => g.Key, g => g.Last().Weight);
            if (items.Count > 0)
            {
                var known = await competencyRepository.GetAll()
                    .Where(c => items.Keys.Contains(c.Id)).Select(c => c.Id).ToListAsync();
                var unknown = items.Keys.Except(known).ToList();
                if (unknown.Count > 0)
                    throw new NotFoundException(nameof(Competency), string.Join(", ", unknown));
            }

            var existing = await repository.GetAll().Where(pc => pc.PositionId == dto.PositionId).ToListAsync();
            foreach (var old in existing) repository.Delete(old);
            foreach (var (competencyId, weight) in items)
                await repository.AddAsync(PositionCompetency.Create(dto.PositionId, competencyId, weight));

            await repository.SaveChangesAsync();
            logger.LogInformation("Saved {Count} competencies for Position {PositionId}", items.Count, dto.PositionId);
        }
    }
}
