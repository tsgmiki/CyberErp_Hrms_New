using System.Linq.Expressions;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    // ---- DTOs ---------------------------------------------------------------
    public class TalentReviewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Cycle { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string Status { get; set; } = nameof(TalentReviewStatus.Draft);
        public string? Notes { get; set; }
    }

    public class SaveTalentReviewDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Cycle { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string Status { get; set; } = nameof(TalentReviewStatus.Draft);
        public string? Notes { get; set; }
    }

    public class SaveTalentReviewDtoValidator : AbstractValidator<SaveTalentReviewDto>
    {
        public SaveTalentReviewDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Cycle).MaximumLength(60);
            RuleFor(x => x.Status).NotEmpty().Must(v => Enum.TryParse<TalentReviewStatus>(v, out _)).WithMessage("Invalid status.");
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }

    /// <summary>One cell of the 9-box grid (HC150) — a performance × potential band and its head-count.</summary>
    public class NineBoxCellDto
    {
        public int PerformanceBand { get; set; }
        public int PotentialBand { get; set; }
        public int Count { get; set; }
    }

    public class NineBoxDto
    {
        public Guid TalentReviewId { get; set; }
        public int Total { get; set; }
        public int HiPoCount { get; set; }
        public List<NineBoxCellDto> Cells { get; set; } = [];
    }

    internal static class TalentReviewMapper
    {
        internal static readonly Expression<Func<TalentReview, TalentReviewDto>> Projection = r => new TalentReviewDto
        {
            Id = r.Id,
            Name = r.Name,
            Cycle = r.Cycle,
            OrganizationUnitId = r.OrganizationUnitId,
            Status = r.Status.ToString(),
            Notes = r.Notes
        };
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTalentReview { Task<Guid> SaveAsync(SaveTalentReviewDto dto); }
    public interface IDeleteTalentReview { Task DeleteAsync(Guid id); }
    public interface IGetTalentReviewById { Task<TalentReviewDto> GetAsync(Guid id); }
    public interface IGetAllTalentReviews { Task<PaginatedResponse<TalentReviewDto>> GetAsync(GetAllRequest request); }
    public interface IGetTalentReviewNineBox { Task<NineBoxDto> GetAsync(Guid talentReviewId); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTalentReview(
        IRepository<TalentReview> repository,
        IValidator<SaveTalentReviewDto> validator,
        ILogger<SaveTalentReview> logger) : ISaveTalentReview
    {
        public async Task<Guid> SaveAsync(SaveTalentReviewDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var status = Enum.Parse<TalentReviewStatus>(dto.Status);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TalentReview), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Cycle, dto.OrganizationUnitId, status, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = TalentReview.Create(dto.Name, dto.Cycle, dto.OrganizationUnitId, dto.Notes);
            if (status != TalentReviewStatus.Draft)
                created.Update(dto.Name, dto.Cycle, dto.OrganizationUnitId, status, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created TalentReview {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteTalentReview(
        IRepository<TalentReview> repository,
        ILogger<DeleteTalentReview> logger) : IDeleteTalentReview
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(TalentReview), id.ToString());
            repository.Delete(entity); // FK cascade removes its assessments + ratings
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted TalentReview {Id}", id);
        }
    }

    public class GetTalentReviewById(IRepository<TalentReview> repository) : IGetTalentReviewById
    {
        public async Task<TalentReviewDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id)
                .Select(TalentReviewMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(TalentReview), id.ToString());
    }

    public class GetAllTalentReviews(IRepository<TalentReview> repository) : IGetAllTalentReviews
    {
        public async Task<PaginatedResponse<TalentReviewDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || (x.Cycle != null && x.Cycle.Contains(term)));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TalentReviewStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take).Select(TalentReviewMapper.Projection).ToListAsync();

            return new PaginatedResponse<TalentReviewDto> { Total = total, Data = data };
        }
    }

    /// <summary>9-box / heat-map counts (HC150) — a single server-side GROUP BY over the covering index.</summary>
    public class GetTalentReviewNineBox(IRepository<TalentAssessment> assessmentRepository) : IGetTalentReviewNineBox
    {
        public async Task<NineBoxDto> GetAsync(Guid talentReviewId)
        {
            var baseQuery = assessmentRepository.GetAll().Where(a => a.TalentReviewId == talentReviewId);
            var cells = await baseQuery
                .GroupBy(a => new { a.PerformanceBand, a.PotentialBand })
                .Select(g => new NineBoxCellDto { PerformanceBand = g.Key.PerformanceBand, PotentialBand = g.Key.PotentialBand, Count = g.Count() })
                .ToListAsync();
            var total = cells.Sum(c => c.Count);
            var hiPo = await baseQuery.CountAsync(a => a.IsHiPo);
            return new NineBoxDto { TalentReviewId = talentReviewId, Total = total, HiPoCount = hiPo, Cells = cells };
        }
    }
}
