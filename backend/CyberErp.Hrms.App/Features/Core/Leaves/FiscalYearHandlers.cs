using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    // ---- DTOs ---------------------------------------------------------------
    public class FiscalYearDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsClosed { get; set; }
    }

    public class SaveFiscalYearDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveFiscalYearDtoValidator : AbstractValidator<SaveFiscalYearDto>
    {
        public SaveFiscalYearDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after the start date.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveFiscalYear { Task<Guid> SaveAsync(SaveFiscalYearDto dto); }
    public interface IGetFiscalYearById { Task<FiscalYearDto> GetAsync(Guid id); }
    public interface IGetAllFiscalYears { Task<PaginatedResponse<FiscalYearDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteFiscalYear { Task DeleteAsync(Guid id); }

    /// <summary>Resolves which open fiscal year a business date belongs to.</summary>
    public interface IFiscalYearResolver
    {
        Task<FiscalYear> ResolveForDateAsync(DateTime date);
    }

    internal static class FiscalYearMapper
    {
        public static Instant ToInstant(DateTime d) =>
            Instant.FromDateTimeUtc(DateTime.SpecifyKind(d.Date, DateTimeKind.Utc));

        public static readonly System.Linq.Expressions.Expression<Func<FiscalYear, FiscalYearDto>> Projection = f => new FiscalYearDto
        {
            Id = f.Id,
            Name = f.Name,
            StartDate = f.StartDate.ToDateTimeUtc(),
            EndDate = f.EndDate.ToDateTimeUtc(),
            IsActive = f.IsActive,
            IsClosed = f.IsClosed
        };
    }

    // ---- Save ---------------------------------------------------------------
    public class SaveFiscalYear(
        IRepository<FiscalYear> repository,
        IValidator<SaveFiscalYearDto> validator,
        ILogger<SaveFiscalYear> logger) : ISaveFiscalYear
    {
        public async Task<Guid> SaveAsync(SaveFiscalYearDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(FiscalYear), nameof(dto.Name), dto.Name);

            var start = FiscalYearMapper.ToInstant(dto.StartDate);
            var end = FiscalYearMapper.ToInstant(dto.EndDate);

            // Fiscal years must not overlap.
            if (await repository.GetAll().AnyAsync(x => x.Id != dto.Id && x.StartDate <= end && x.EndDate >= start))
                throw new ValidationException("startDate", "The date range overlaps an existing fiscal year.");

            Guid id;
            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(FiscalYear), dto.Id.Value.ToString());
                if (entity.IsClosed)
                    throw new ValidationException("id", "A closed fiscal year cannot be modified.");
                entity.Update(dto.Name, start, end, dto.IsActive);
                repository.UpdateAsync(entity);
                id = entity.Id;
            }
            else
            {
                var created = FiscalYear.Create(dto.Name, start, end, dto.IsActive);
                await repository.AddAsync(created);
                id = created.Id;
            }

            // Only one active fiscal year at a time.
            if (dto.IsActive)
            {
                var others = await repository.GetAll().Where(x => x.IsActive && x.Id != id).ToListAsync();
                foreach (var other in others)
                {
                    other.SetActive(false);
                    repository.UpdateAsync(other);
                }
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Saved FiscalYear {Id} ({Name})", id, dto.Name);
            return id;
        }
    }

    // ---- Reads / delete -------------------------------------------------------
    public class GetFiscalYearById(IRepository<FiscalYear> repository) : IGetFiscalYearById
    {
        public async Task<FiscalYearDto> GetAsync(Guid id)
        {
            return await repository.GetAll().Where(x => x.Id == id)
                .Select(FiscalYearMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(FiscalYear), id.ToString());
        }
    }

    public class GetAllFiscalYears(IRepository<FiscalYear> repository) : IGetAllFiscalYears
    {
        public async Task<PaginatedResponse<FiscalYearDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.StartDate)
                .Skip(skip).Take(take)
                .Select(FiscalYearMapper.Projection)
                .ToListAsync();
            return new PaginatedResponse<FiscalYearDto> { Total = total, Data = data };
        }
    }

    public class DeleteFiscalYear(
        IRepository<FiscalYear> repository,
        IRepository<LeaveBalance> balances,
        IRepository<LeaveRequest> requests,
        ILogger<DeleteFiscalYear> logger) : IDeleteFiscalYear
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(FiscalYear), id.ToString());
            if (await balances.GetAll().AnyAsync(b => b.FiscalYearId == id) ||
                await requests.GetAll().AnyAsync(r => r.FiscalYearId == id))
                throw new ValidationException(nameof(id), "This fiscal year has leave activity and cannot be deleted. Close it instead.");
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted FiscalYear {Id}", id);
        }
    }

    // ---- Resolver -------------------------------------------------------------
    public class FiscalYearResolver(IRepository<FiscalYear> repository) : IFiscalYearResolver
    {
        public async Task<FiscalYear> ResolveForDateAsync(DateTime date)
        {
            var instant = FiscalYearMapper.ToInstant(date);
            var year = await repository.GetAll()
                .FirstOrDefaultAsync(x => !x.IsClosed && x.StartDate <= instant && x.EndDate >= instant);
            return year
                ?? throw new ValidationException("startDate",
                    $"No open fiscal year covers {date:yyyy-MM-dd}. Configure the fiscal year first.");
        }
    }
}
