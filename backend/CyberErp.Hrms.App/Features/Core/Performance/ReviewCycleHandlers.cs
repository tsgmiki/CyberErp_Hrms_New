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
    public class ReviewCycleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PeriodType { get; set; } = string.Empty;
        public Guid? FiscalYearId { get; set; }
        public string? FiscalYearName { get; set; }
        public Guid RatingScaleId { get; set; }
        public string? RatingScaleName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? SelfReviewDue { get; set; }
        public DateTime? ManagerReviewDue { get; set; }
        public bool EnableSelfAssessment { get; set; }
        public bool EnablePeerAssessment { get; set; }
        public bool EnableCalibration { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ProbationDurationMonths { get; set; }
    }

    public class SaveReviewCycleDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PeriodType { get; set; } = nameof(ReviewPeriodType.Annual);
        public Guid? FiscalYearId { get; set; }
        public Guid RatingScaleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? SelfReviewDue { get; set; }
        public DateTime? ManagerReviewDue { get; set; }
        public bool EnableSelfAssessment { get; set; } = true;
        public bool EnablePeerAssessment { get; set; }
        public bool EnableCalibration { get; set; }
        /// <summary>Probation length in months (probation cycles only) — computes period end from hire date.</summary>
        public int? ProbationDurationMonths { get; set; }
    }

    public class SaveReviewCycleDtoValidator : AbstractValidator<SaveReviewCycleDto>
    {
        public SaveReviewCycleDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.PeriodType).NotEmpty()
                .Must(v => Enum.TryParse<ReviewPeriodType>(v, out _))
                .WithMessage("PeriodType must be one of: Annual, BiAnnual, Quarterly, Probation, Custom.");
            RuleFor(x => x.RatingScaleId).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date cannot be before the start date.");
            RuleFor(x => x.ProbationDurationMonths).InclusiveBetween(1, 24)
                .When(x => x.ProbationDurationMonths.HasValue)
                .WithMessage("Probation duration must be between 1 and 24 months.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveReviewCycle { Task<Guid> SaveAsync(SaveReviewCycleDto dto); }
    public interface IDeleteReviewCycle { Task DeleteAsync(Guid id); }
    public interface IGetReviewCycleById { Task<ReviewCycleDto> GetAsync(Guid id); }
    public interface IGetAllReviewCycles { Task<PaginatedResponse<ReviewCycleDto>> GetAsync(GetAllRequest request); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveReviewCycle(
        IRepository<ReviewCycle> repository,
        IRepository<RatingScale> ratingScaleRepository,
        IRepository<FiscalYear> fiscalYearRepository,
        IValidator<SaveReviewCycleDto> validator,
        ILogger<SaveReviewCycle> logger) : ISaveReviewCycle
    {
        public async Task<Guid> SaveAsync(SaveReviewCycleDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await ratingScaleRepository.GetAll().AnyAsync(r => r.Id == dto.RatingScaleId))
                throw new NotFoundException(nameof(RatingScale), dto.RatingScaleId.ToString());
            if (dto.FiscalYearId.HasValue && !await fiscalYearRepository.GetAll().AnyAsync(f => f.Id == dto.FiscalYearId.Value))
                throw new NotFoundException(nameof(FiscalYear), dto.FiscalYearId.Value.ToString());
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(ReviewCycle), nameof(dto.Name), dto.Name);

            var periodType = Enum.Parse<ReviewPeriodType>(dto.PeriodType);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(ReviewCycle), dto.Id.Value.ToString());
                entity.Update(dto.Name, periodType, dto.RatingScaleId, dto.StartDate, dto.EndDate, dto.FiscalYearId,
                    dto.SelfReviewDue, dto.ManagerReviewDue, dto.EnableSelfAssessment, dto.EnablePeerAssessment, dto.EnableCalibration,
                    dto.ProbationDurationMonths);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated ReviewCycle {Id}", entity.Id);
                return entity.Id;
            }

            var created = ReviewCycle.Create(dto.Name, periodType, dto.RatingScaleId, dto.StartDate, dto.EndDate,
                dto.FiscalYearId, dto.SelfReviewDue, dto.ManagerReviewDue,
                dto.EnableSelfAssessment, dto.EnablePeerAssessment, dto.EnableCalibration,
                dto.ProbationDurationMonths);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created ReviewCycle {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteReviewCycle(
        IRepository<ReviewCycle> repository,
        ILogger<DeleteReviewCycle> logger) : IDeleteReviewCycle
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(ReviewCycle), id.ToString());
            // Appraisals/goals reference the cycle from a later phase; that delete guard is added then.
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted ReviewCycle {Id}", id);
        }
    }

    public class GetReviewCycleById(
        IRepository<ReviewCycle> repository,
        IRepository<RatingScale> ratingScaleRepository,
        IRepository<FiscalYear> fiscalYearRepository) : IGetReviewCycleById
    {
        public async Task<ReviewCycleDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(ReviewCycle), id.ToString());
            var scaleName = await ratingScaleRepository.GetAll().Where(r => r.Id == entity.RatingScaleId).Select(r => r.Name).FirstOrDefaultAsync();
            var fyName = entity.FiscalYearId.HasValue
                ? await fiscalYearRepository.GetAll().Where(f => f.Id == entity.FiscalYearId.Value).Select(f => f.Name).FirstOrDefaultAsync()
                : null;
            return ReviewCycleMapper.Map(entity, scaleName, fyName);
        }
    }

    public class GetAllReviewCycles(
        IRepository<ReviewCycle> repository,
        IRepository<RatingScale> ratingScaleRepository,
        IRepository<FiscalYear> fiscalYearRepository) : IGetAllReviewCycles
    {
        public async Task<PaginatedResponse<ReviewCycleDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ReviewCycleStatus>(request.Status, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var scales = ratingScaleRepository.GetAll();
            var fys = fiscalYearRepository.GetAll();
            var data = await query
                .OrderByDescending(x => x.StartDate)
                .Skip(skip).Take(take)
                .Select(x => new ReviewCycleDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    PeriodType = x.PeriodType.ToString(),
                    FiscalYearId = x.FiscalYearId,
                    FiscalYearName = fys.Where(f => f.Id == x.FiscalYearId).Select(f => f.Name).FirstOrDefault(),
                    RatingScaleId = x.RatingScaleId,
                    RatingScaleName = scales.Where(r => r.Id == x.RatingScaleId).Select(r => r.Name).FirstOrDefault(),
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    SelfReviewDue = x.SelfReviewDue,
                    ManagerReviewDue = x.ManagerReviewDue,
                    EnableSelfAssessment = x.EnableSelfAssessment,
                    EnablePeerAssessment = x.EnablePeerAssessment,
                    EnableCalibration = x.EnableCalibration,
                    Status = x.Status.ToString(),
                    ProbationDurationMonths = x.ProbationDurationMonths
                })
                .ToListAsync();

            return new PaginatedResponse<ReviewCycleDto> { Total = total, Data = data };
        }
    }

    internal static class ReviewCycleMapper
    {
        internal static ReviewCycleDto Map(ReviewCycle x, string? scaleName, string? fyName) => new()
        {
        Id = x.Id,
        Name = x.Name,
        PeriodType = x.PeriodType.ToString(),
        FiscalYearId = x.FiscalYearId,
        FiscalYearName = fyName,
        RatingScaleId = x.RatingScaleId,
        RatingScaleName = scaleName,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        SelfReviewDue = x.SelfReviewDue,
        ManagerReviewDue = x.ManagerReviewDue,
        EnableSelfAssessment = x.EnableSelfAssessment,
        EnablePeerAssessment = x.EnablePeerAssessment,
        EnableCalibration = x.EnableCalibration,
        Status = x.Status.ToString(),
        ProbationDurationMonths = x.ProbationDurationMonths
        };
    }
}
