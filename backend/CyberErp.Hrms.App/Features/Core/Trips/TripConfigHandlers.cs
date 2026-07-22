using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Trips
{
    // ================= Per-diem rates (HC267) =================
    public class PerDiemRateDto
    {
        public Guid Id { get; set; }
        public Guid JobGradeId { get; set; }
        public string? JobGradeName { get; set; }
        public string TripType { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public string Currency { get; set; } = "ETB";
        public bool IsActive { get; set; }
    }

    public class SavePerDiemRateDto
    {
        public Guid? Id { get; set; }
        public Guid JobGradeId { get; set; }
        public string TripType { get; set; } = nameof(Dom.Entities.Core.TripType.Local);
        public decimal DailyRate { get; set; }
        public string? Currency { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SavePerDiemRateDtoValidator : AbstractValidator<SavePerDiemRateDto>
    {
        public SavePerDiemRateDtoValidator()
        {
            RuleFor(x => x.JobGradeId).NotEmpty();
            RuleFor(x => x.DailyRate).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripType).Must(v => Enum.TryParse<TripType>(v, true, out _))
                .WithMessage("Trip type must be Local or International.");
        }
    }

    public interface ISavePerDiemRate { Task<Guid> SaveAsync(SavePerDiemRateDto dto); }
    public interface IDeletePerDiemRate { Task DeleteAsync(Guid id); }
    public interface IGetPerDiemRateById { Task<PerDiemRateDto> GetAsync(Guid id); }
    public interface IGetAllPerDiemRates { Task<PaginatedResponse<PerDiemRateDto>> GetAsync(GetAllRequest request); }

    public class GetPerDiemRateById(
        IRepository<PerDiemRate> repository,
        IRepository<JobGrade> jobGradeRepository) : IGetPerDiemRateById
    {
        public async Task<PerDiemRateDto> GetAsync(Guid id)
        {
            var grades = jobGradeRepository.GetAll();
            return await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(x => new PerDiemRateDto
                {
                    Id = x.Id, JobGradeId = x.JobGradeId,
                    JobGradeName = grades.Where(g => g.Id == x.JobGradeId).Select(g => g.Name).FirstOrDefault(),
                    TripType = x.TripType.ToString(), DailyRate = x.DailyRate, Currency = x.Currency, IsActive = x.IsActive
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(PerDiemRate), id.ToString());
        }
    }

    public class SavePerDiemRate(
        IRepository<PerDiemRate> repository,
        IRepository<JobGrade> jobGradeRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SavePerDiemRateDto> validator) : ISavePerDiemRate
    {
        public async Task<Guid> SaveAsync(SavePerDiemRateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage per-diem rates.");
            if (!await jobGradeRepository.GetAll().AnyAsync(g => g.Id == dto.JobGradeId))
                throw new NotFoundException(nameof(JobGrade), dto.JobGradeId.ToString());

            var tripType = Enum.Parse<TripType>(dto.TripType, true);
            if (await repository.GetAll().AnyAsync(x => x.JobGradeId == dto.JobGradeId && x.TripType == tripType && x.Id != dto.Id))
                throw new ValidationException(nameof(dto.JobGradeId), "A per-diem rate for this grade and trip type already exists.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(PerDiemRate), dto.Id.Value.ToString());
                entity.Update(dto.JobGradeId, tripType, dto.DailyRate, dto.Currency, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }
            var created = PerDiemRate.Create(dto.JobGradeId, tripType, dto.DailyRate, dto.Currency, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeletePerDiemRate(
        IRepository<PerDiemRate> repository,
        IPerformanceVisibilityService visibility) : IDeletePerDiemRate
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage per-diem rates.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(PerDiemRate), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetAllPerDiemRates(
        IRepository<PerDiemRate> repository,
        IRepository<JobGrade> jobGradeRepository) : IGetAllPerDiemRates
    {
        public async Task<PaginatedResponse<PerDiemRateDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var grades = jobGradeRepository.GetAll();
            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TripType>(request.Status, true, out var tt))
                query = query.Where(x => x.TripType == tt);
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.TripType).Skip(skip).Take(take)
                .Select(x => new PerDiemRateDto
                {
                    Id = x.Id, JobGradeId = x.JobGradeId,
                    JobGradeName = grades.Where(g => g.Id == x.JobGradeId).Select(g => g.Name).FirstOrDefault(),
                    TripType = x.TripType.ToString(), DailyRate = x.DailyRate, Currency = x.Currency, IsActive = x.IsActive
                }).ToListAsync();
            return new PaginatedResponse<PerDiemRateDto> { Total = total, Data = data };
        }
    }

    // ================= Travel budgets (HC266) =================
    public class TripBudgetDto
    {
        public Guid Id { get; set; }
        public int FiscalYear { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveTripBudgetDto
    {
        public Guid? Id { get; set; }
        public int FiscalYear { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveTripBudgetDtoValidator : AbstractValidator<SaveTripBudgetDto>
    {
        public SaveTripBudgetDtoValidator()
        {
            RuleFor(x => x.FiscalYear).InclusiveBetween(2000, 3000);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        }
    }

    public interface ISaveTripBudget { Task<Guid> SaveAsync(SaveTripBudgetDto dto); }
    public interface IDeleteTripBudget { Task DeleteAsync(Guid id); }
    public interface IGetTripBudgetById { Task<TripBudgetDto> GetAsync(Guid id); }
    public interface IGetAllTripBudgets { Task<PaginatedResponse<TripBudgetDto>> GetAsync(GetAllRequest request); }

    public class GetTripBudgetById(
        IRepository<TripBudget> repository,
        IRepository<OrganizationUnit> unitRepository) : IGetTripBudgetById
    {
        public async Task<TripBudgetDto> GetAsync(Guid id)
        {
            var units = unitRepository.GetAll();
            return await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(x => new TripBudgetDto
                {
                    Id = x.Id, FiscalYear = x.FiscalYear, OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = units.Where(u => u.Id == x.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                    Amount = x.Amount, Notes = x.Notes
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TripBudget), id.ToString());
        }
    }

    public class SaveTripBudget(
        IRepository<TripBudget> repository,
        IRepository<OrganizationUnit> unitRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveTripBudgetDto> validator) : ISaveTripBudget
    {
        public async Task<Guid> SaveAsync(SaveTripBudgetDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage travel budgets.");
            if (dto.OrganizationUnitId.HasValue && !await unitRepository.GetAll().AnyAsync(u => u.Id == dto.OrganizationUnitId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.OrganizationUnitId.Value.ToString());

            var dup = dto.OrganizationUnitId.HasValue
                ? await repository.GetAll().AnyAsync(x => x.FiscalYear == dto.FiscalYear && x.OrganizationUnitId == dto.OrganizationUnitId.Value && x.Id != dto.Id)
                : await repository.GetAll().AnyAsync(x => x.FiscalYear == dto.FiscalYear && x.OrganizationUnitId == null && x.Id != dto.Id);
            if (dup) throw new ValidationException(nameof(dto.FiscalYear), "A travel budget for this fiscal year and unit already exists.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TripBudget), dto.Id.Value.ToString());
                entity.Update(dto.FiscalYear, dto.OrganizationUnitId, dto.Amount, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }
            var created = TripBudget.Create(dto.FiscalYear, dto.OrganizationUnitId, dto.Amount, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeleteTripBudget(
        IRepository<TripBudget> repository,
        IPerformanceVisibilityService visibility) : IDeleteTripBudget
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage travel budgets.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(TripBudget), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetAllTripBudgets(
        IRepository<TripBudget> repository,
        IRepository<OrganizationUnit> unitRepository) : IGetAllTripBudgets
    {
        public async Task<PaginatedResponse<TripBudgetDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var units = unitRepository.GetAll();
            var query = repository.GetAll().AsNoTracking();
            if (int.TryParse(request.Status, out var fy))
                query = query.Where(x => x.FiscalYear == fy);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.FiscalYear).Skip(skip).Take(take)
                .Select(x => new TripBudgetDto
                {
                    Id = x.Id, FiscalYear = x.FiscalYear, OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = units.Where(u => u.Id == x.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                    Amount = x.Amount, Notes = x.Notes
                }).ToListAsync();
            return new PaginatedResponse<TripBudgetDto> { Total = total, Data = data };
        }
    }
}
