using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class TrainingBudgetDto
    {
        public Guid Id { get; set; }
        public int FiscalYear { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveTrainingBudgetDto
    {
        public Guid? Id { get; set; }
        public int FiscalYear { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>HC190 — a budget envelope against what training actually costs / commits.</summary>
    public class TrainingBudgetUtilizationDto
    {
        public int FiscalYear { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public decimal BudgetAmount { get; set; }
        /// <summary>Actual session costs (trainer + materials + venue) for sessions starting in the year.</summary>
        public decimal SessionCosts { get; set; }
        /// <summary>Estimates of approved-but-unfulfilled needs (committed spend).</summary>
        public decimal CommittedNeedEstimates { get; set; }
        public decimal Utilized => SessionCosts + CommittedNeedEstimates;
        public decimal Remaining => BudgetAmount - Utilized;
    }

    public class SaveTrainingBudgetDtoValidator : AbstractValidator<SaveTrainingBudgetDto>
    {
        public SaveTrainingBudgetDtoValidator()
        {
            RuleFor(x => x.FiscalYear).InclusiveBetween(2000, 2100);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTrainingBudget { Task<Guid> SaveAsync(SaveTrainingBudgetDto dto); }
    public interface IDeleteTrainingBudget { Task DeleteAsync(Guid id); }
    public interface IGetAllTrainingBudgets { Task<PaginatedResponse<TrainingBudgetDto>> GetAsync(GetAllRequest request); }
    public interface IGetTrainingBudgetUtilization { Task<TrainingBudgetUtilizationDto> GetAsync(int fiscalYear, Guid? organizationUnitId); }

    internal static class TrainingBudgetShared
    {
        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage training budgets.");
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTrainingBudget(
        IRepository<TrainingBudget> repository,
        IRepository<OrganizationUnit> unitRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveTrainingBudgetDto> validator,
        ILogger<SaveTrainingBudget> logger) : ISaveTrainingBudget
    {
        public async Task<Guid> SaveAsync(SaveTrainingBudgetDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            await TrainingBudgetShared.EnsureAdminAsync(visibility);

            if (dto.OrganizationUnitId.HasValue &&
                !await unitRepository.GetAll().AnyAsync(u => u.Id == dto.OrganizationUnitId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.OrganizationUnitId.Value.ToString());
            if (await repository.GetAll().AnyAsync(b => b.FiscalYear == dto.FiscalYear
                    && b.OrganizationUnitId == dto.OrganizationUnitId && b.Id != dto.Id))
                throw new DuplicateException(nameof(TrainingBudget), nameof(dto.FiscalYear),
                    $"{dto.FiscalYear}{(dto.OrganizationUnitId.HasValue ? " (unit)" : " (org-wide)")}");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TrainingBudget), dto.Id.Value.ToString());
                entity.Update(dto.FiscalYear, dto.OrganizationUnitId, dto.Amount, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated TrainingBudget {Id}", entity.Id);
                return entity.Id;
            }

            var created = TrainingBudget.Create(dto.FiscalYear, dto.OrganizationUnitId, dto.Amount, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created TrainingBudget {Id} ({Year})", created.Id, created.FiscalYear);
            return created.Id;
        }
    }

    public class DeleteTrainingBudget(
        IRepository<TrainingBudget> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteTrainingBudget> logger) : IDeleteTrainingBudget
    {
        public async Task DeleteAsync(Guid id)
        {
            await TrainingBudgetShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(TrainingBudget), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted TrainingBudget {Id}", id);
        }
    }

    public class GetAllTrainingBudgets(
        IRepository<TrainingBudget> repository,
        IRepository<OrganizationUnit> unitRepository,
        IPerformanceVisibilityService visibility) : IGetAllTrainingBudgets
    {
        public async Task<PaginatedResponse<TrainingBudgetDto>> GetAsync(GetAllRequest request)
        {
            await TrainingBudgetShared.EnsureAdminAsync(visibility);

            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (request.OrganizationUnitId.HasValue)
                query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);

            var units = unitRepository.GetAll();
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.FiscalYear).Skip(skip).Take(take)
                .Select(x => new TrainingBudgetDto
                {
                    Id = x.Id,
                    FiscalYear = x.FiscalYear,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = units.Where(u => u.Id == x.OrganizationUnitId)
                        .Select(u => u.Name).FirstOrDefault(),
                    Amount = x.Amount,
                    Notes = x.Notes
                }).ToListAsync();

            return new PaginatedResponse<TrainingBudgetDto> { Total = total, Data = data };
        }
    }

    public class GetTrainingBudgetUtilization(
        IRepository<TrainingBudget> repository,
        IRepository<TrainingSession> sessionRepository,
        IRepository<TrainingNeed> needRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetTrainingBudgetUtilization
    {
        public async Task<TrainingBudgetUtilizationDto> GetAsync(int fiscalYear, Guid? organizationUnitId)
        {
            await TrainingBudgetShared.EnsureAdminAsync(visibility);

            var amount = await repository.GetAll().AsNoTracking()
                .Where(b => b.FiscalYear == fiscalYear && b.OrganizationUnitId == organizationUnitId)
                .Select(b => (decimal?)b.Amount).FirstOrDefaultAsync() ?? 0m;

            var from = new DateTime(fiscalYear, 1, 1);
            var to = new DateTime(fiscalYear + 1, 1, 1);

            // Actual spend: indexed SUM over sessions starting in the year. Sessions are org-wide —
            // a unit-scoped envelope tracks only its unit's committed need estimates.
            var sessionCosts = organizationUnitId is null
                ? await sessionRepository.GetAll().AsNoTracking()
                    .Where(x => x.StartDate >= from && x.StartDate < to && x.Status != TrainingSessionStatus.Cancelled)
                    .SumAsync(x => (decimal?)((x.TrainerCost ?? 0) + (x.MaterialsCost ?? 0) + (x.VenueCost ?? 0))) ?? 0m
                : 0m;

            var needQuery = needRepository.GetAll().AsNoTracking()
                .Where(n => n.Status == TrainingNeedStatus.Approved && n.EstimatedCost != null
                    && n.DecidedOn >= from && n.DecidedOn < to);
            if (organizationUnitId.HasValue)
            {
                var emps = employeeRepository.GetAll();
                needQuery = needQuery.Where(n => emps.Any(e => e.Id == n.EmployeeId && e.Position != null
                    && e.Position.OrganizationUnitId == organizationUnitId.Value));
            }
            var committed = await needQuery.SumAsync(n => (decimal?)n.EstimatedCost) ?? 0m;

            return new TrainingBudgetUtilizationDto
            {
                FiscalYear = fiscalYear,
                OrganizationUnitId = organizationUnitId,
                BudgetAmount = amount,
                SessionCosts = sessionCosts,
                CommittedNeedEstimates = committed
            };
        }
    }
}
