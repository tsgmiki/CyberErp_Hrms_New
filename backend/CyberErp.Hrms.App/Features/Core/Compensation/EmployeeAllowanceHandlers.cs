using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeAllowanceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public string AllowanceTypeName { get; set; } = string.Empty;
        public string CalcMethod { get; set; } = string.Empty;
        public bool IsTaxable { get; set; }
        /// <summary>The entered value — a fixed amount or a percent, per the type's calc method.</summary>
        public decimal Value { get; set; }
        /// <summary>The value resolved to a monetary figure against the employee's base salary.</summary>
        public decimal ResolvedAmount { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public string? Remark { get; set; }
    }

    public class SaveEmployeeAllowanceDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public decimal Value { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? Remark { get; set; }
    }

    public class SaveEmployeeAllowanceDtoValidator : AbstractValidator<SaveEmployeeAllowanceDto>
    {
        public SaveEmployeeAllowanceDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.AllowanceTypeId).NotEmpty();
            RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
            RuleFor(x => x.EffectiveFrom).NotEmpty();
        }
    }

    /// <summary>Resolved compensation snapshot for one employee (HC226/HC233).</summary>
    public class CompensationSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public decimal BaseSalary { get; set; }
        public string? JobGradeName { get; set; }
        public string? StepName { get; set; }
        public List<EmployeeAllowanceDto> Allowances { get; set; } = [];
        public decimal TotalAllowances { get; set; }
        public decimal TaxableAllowances { get; set; }
        public decimal NonTaxableAllowances { get; set; }
        /// <summary>Base + all currently-active allowances.</summary>
        public decimal GrossPay { get; set; }
        /// <summary>Base (taxable) + taxable allowances — the input to tax computation (HC231/232).</summary>
        public decimal TaxableGross { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveEmployeeAllowance { Task<Guid> SaveAsync(SaveEmployeeAllowanceDto dto); }
    public interface IGetEmployeeAllowances { Task<List<EmployeeAllowanceDto>> GetAsync(Guid employeeId); }
    public interface IDeleteEmployeeAllowance { Task DeleteAsync(Guid id); }
    public interface IGetCompensationSummary { Task<CompensationSummaryDto> GetAsync(Guid employeeId); }

    // ---- Shared resolution --------------------------------------------------
    internal static class CompensationShared
    {
        /// <summary>Resolves a raw allowance value to money: Fixed = as-is; PercentOfBase = base × pct/100.</summary>
        internal static decimal Resolve(AllowanceCalcMethod method, decimal value, decimal baseSalary) =>
            method == AllowanceCalcMethod.PercentOfBase
                ? Math.Round(baseSalary * value / 100m, 2)
                : value;

        internal static bool IsActiveOn(DateTime from, DateTime? to, DateTime on) =>
            from.Date <= on && (to is null || to.Value.Date >= on);
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveEmployeeAllowance(
        IRepository<EmployeeAllowance> repository,
        IRepository<Employee> employeeRepository,
        IRepository<AllowanceType> allowanceTypeRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveEmployeeAllowanceDto> validator,
        ILogger<SaveEmployeeAllowance> logger) : ISaveEmployeeAllowance
    {
        public async Task<Guid> SaveAsync(SaveEmployeeAllowanceDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Compensation is HR-managed — only admins assign/adjust allowances.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(dto.EmployeeId), "Only HR can manage employee allowances.");

            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            var type = await allowanceTypeRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == dto.AllowanceTypeId)
                ?? throw new NotFoundException(nameof(AllowanceType), dto.AllowanceTypeId.ToString());
            if (!type.IsActive)
                throw new ValidationException(nameof(dto.AllowanceTypeId), "The selected allowance type is inactive.");
            if (type.CalcMethod == AllowanceCalcMethod.PercentOfBase && dto.Value > 100)
                throw new ValidationException(nameof(dto.Value), "A percent-of-base value cannot exceed 100.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.EmployeeId == dto.EmployeeId)
                    ?? throw new NotFoundException(nameof(EmployeeAllowance), dto.Id.Value.ToString());
                entity.Update(dto.AllowanceTypeId, dto.Value, dto.EffectiveFrom, dto.EffectiveTo, dto.Remark);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeAllowance {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeAllowance.Create(dto.EmployeeId, dto.AllowanceTypeId, dto.Value,
                dto.EffectiveFrom, dto.EffectiveTo, dto.Remark);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeAllowance {Id} for Employee {EmployeeId}", created.Id, dto.EmployeeId);
            return created.Id;
        }
    }

    public class GetEmployeeAllowances(
        IRepository<EmployeeAllowance> repository,
        IRepository<Employee> employeeRepository,
        IRepository<AllowanceType> allowanceTypeRepository,
        IPerformanceVisibilityService visibility) : IGetEmployeeAllowances
    {
        public async Task<List<EmployeeAllowanceDto>> GetAsync(Guid employeeId)
        {
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(employeeId), "You do not have access to this employee's compensation.");

            var baseSalary = await CompensationSummaryHelper.ResolveBaseAsync(employeeRepository, employeeId);
            var today = DateTime.UtcNow.Date;
            var types = allowanceTypeRepository.GetAll();

            var rows = await repository.GetAll().AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => new
                {
                    x.Id, x.EmployeeId, x.AllowanceTypeId, x.Value, x.EffectiveFrom, x.EffectiveTo, x.Remark,
                    TypeName = types.Where(t => t.Id == x.AllowanceTypeId).Select(t => t.Name).FirstOrDefault(),
                    Method = types.Where(t => t.Id == x.AllowanceTypeId).Select(t => t.CalcMethod).FirstOrDefault(),
                    Taxable = types.Where(t => t.Id == x.AllowanceTypeId).Select(t => t.IsTaxable).FirstOrDefault()
                })
                .ToListAsync();

            return rows.Select(x => new EmployeeAllowanceDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                AllowanceTypeId = x.AllowanceTypeId,
                AllowanceTypeName = x.TypeName ?? string.Empty,
                CalcMethod = x.Method.ToString(),
                IsTaxable = x.Taxable,
                Value = x.Value,
                ResolvedAmount = CompensationShared.Resolve(x.Method, x.Value, baseSalary),
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                IsCurrentlyActive = CompensationShared.IsActiveOn(x.EffectiveFrom, x.EffectiveTo, today),
                Remark = x.Remark
            }).ToList();
        }
    }

    public class DeleteEmployeeAllowance(
        IRepository<EmployeeAllowance> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteEmployeeAllowance> logger) : IDeleteEmployeeAllowance
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException(nameof(id), "Only HR can manage employee allowances.");

            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeAllowance), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeAllowance {Id}", id);
        }
    }

    public class GetCompensationSummary(
        IRepository<EmployeeAllowance> repository,
        IRepository<Employee> employeeRepository,
        IRepository<AllowanceType> allowanceTypeRepository,
        IPerformanceVisibilityService visibility) : IGetCompensationSummary
    {
        public async Task<CompensationSummaryDto> GetAsync(Guid employeeId)
        {
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(employeeId), "You do not have access to this employee's compensation.");

            var head = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => new
                {
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber,
                    e.EmployeeNumber,
                    e.Salary,
                    ScaleSalary = e.SalaryScale != null ? (decimal?)e.SalaryScale.Salary : null,
                    GradeName = e.SalaryScale != null && e.SalaryScale.JobGrade != null ? e.SalaryScale.JobGrade.Name : null,
                    StepName = e.SalaryScale != null && e.SalaryScale.Step != null ? e.SalaryScale.Step.Name : null
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var baseSalary = head.Salary ?? head.ScaleSalary ?? 0m;
            var today = DateTime.UtcNow.Date;
            var types = allowanceTypeRepository.GetAll();

            var rows = await repository.GetAll().AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => new
                {
                    x.Id, x.EmployeeId, x.AllowanceTypeId, x.Value, x.EffectiveFrom, x.EffectiveTo, x.Remark,
                    TypeName = types.Where(t => t.Id == x.AllowanceTypeId).Select(t => t.Name).FirstOrDefault(),
                    Method = types.Where(t => t.Id == x.AllowanceTypeId).Select(t => t.CalcMethod).FirstOrDefault(),
                    Taxable = types.Where(t => t.Id == x.AllowanceTypeId).Select(t => t.IsTaxable).FirstOrDefault()
                })
                .ToListAsync();

            var allowances = rows.Select(x => new EmployeeAllowanceDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                AllowanceTypeId = x.AllowanceTypeId,
                AllowanceTypeName = x.TypeName ?? string.Empty,
                CalcMethod = x.Method.ToString(),
                IsTaxable = x.Taxable,
                Value = x.Value,
                ResolvedAmount = CompensationShared.Resolve(x.Method, x.Value, baseSalary),
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                IsCurrentlyActive = CompensationShared.IsActiveOn(x.EffectiveFrom, x.EffectiveTo, today),
                Remark = x.Remark
            }).ToList();

            var active = allowances.Where(a => a.IsCurrentlyActive).ToList();
            var taxable = active.Where(a => a.IsTaxable).Sum(a => a.ResolvedAmount);
            var nonTaxable = active.Where(a => !a.IsTaxable).Sum(a => a.ResolvedAmount);

            return new CompensationSummaryDto
            {
                EmployeeId = employeeId,
                EmployeeName = head.Name,
                EmployeeNumber = head.EmployeeNumber,
                BaseSalary = baseSalary,
                JobGradeName = head.GradeName,
                StepName = head.StepName,
                Allowances = allowances,
                TotalAllowances = taxable + nonTaxable,
                TaxableAllowances = taxable,
                NonTaxableAllowances = nonTaxable,
                GrossPay = baseSalary + taxable + nonTaxable,
                TaxableGross = baseSalary + taxable
            };
        }
    }

    internal static class CompensationSummaryHelper
    {
        /// <summary>Base pay = the individual's Salary, else the salary-scale amount, else 0.</summary>
        internal static async Task<decimal> ResolveBaseAsync(IRepository<Employee> employees, Guid employeeId)
        {
            var head = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => new { e.Salary, ScaleSalary = e.SalaryScale != null ? (decimal?)e.SalaryScale.Salary : null })
                .FirstOrDefaultAsync();
            return head?.Salary ?? head?.ScaleSalary ?? 0m;
        }
    }
}
