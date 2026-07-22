using CyberErp.Hrms.App.Common.DTOs;
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
    // ---- Tax bracket DTOs ---------------------------------------------------
    public class TaxBracketDto
    {
        public Guid Id { get; set; }
        public decimal LowerBound { get; set; }
        public decimal? UpperBound { get; set; }
        public decimal RatePercent { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveTaxBracketDto
    {
        public Guid? Id { get; set; }
        public decimal LowerBound { get; set; }
        public decimal? UpperBound { get; set; }
        public decimal RatePercent { get; set; }
        public int SortOrder { get; set; }
    }

    public class SaveTaxBracketDtoValidator : AbstractValidator<SaveTaxBracketDto>
    {
        public SaveTaxBracketDtoValidator()
        {
            RuleFor(x => x.LowerBound).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RatePercent).InclusiveBetween(0, 100);
            RuleFor(x => x).Must(x => !x.UpperBound.HasValue || x.UpperBound.Value > x.LowerBound)
                .WithMessage("Upper bound must exceed the lower bound.");
        }
    }

    // ---- Deductions preview DTO (HC231/HC232) -------------------------------
    public class PayrollDeductionsDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TaxableGross { get; set; }
        public decimal IncomeTax { get; set; }
        public decimal EmployeeBenefitContributions { get; set; }
        public decimal EmployerBenefitContributions { get; set; }
        /// <summary>HC254 — total monthly loan repayment deducted this cycle (across active loans).</summary>
        public decimal LoanRepayments { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public List<DeductionLineDto> Lines { get; set; } = [];
    }

    public class DeductionLineDto
    {
        public string Label { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty; // Tax | BenefitContribution | LoanInstallment
        public decimal Amount { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTaxBracket { Task<Guid> SaveAsync(SaveTaxBracketDto dto); }
    public interface IDeleteTaxBracket { Task DeleteAsync(Guid id); }
    public interface IGetAllTaxBrackets { Task<PaginatedResponse<TaxBracketDto>> GetAsync(GetAllRequest request); }
    public interface IGetTaxBracketById { Task<TaxBracketDto> GetAsync(Guid id); }
    public interface IGetPayrollDeductions { Task<PayrollDeductionsDto> GetAsync(Guid employeeId); }

    // ---- Progressive tax calculator (shared) --------------------------------
    internal static class TaxCalculator
    {
        /// <summary>
        /// Marginal progressive tax: for each band, the slice of income between lower and upper is
        /// taxed at the band rate. Brackets must be supplied ordered by lower bound.
        /// </summary>
        internal static decimal Compute(IReadOnlyList<TaxBracket> orderedBrackets, decimal income)
        {
            if (income <= 0) return 0m;
            decimal tax = 0m;
            foreach (var b in orderedBrackets)
            {
                if (income <= b.LowerBound) break;
                var upper = b.UpperBound ?? income;
                var slice = Math.Min(income, upper) - b.LowerBound;
                if (slice > 0) tax += slice * b.RatePercent / 100m;
            }
            return Math.Round(tax, 2);
        }
    }

    // ---- Tax bracket handlers -----------------------------------------------
    public class SaveTaxBracket(
        IRepository<TaxBracket> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveTaxBracketDto> validator) : ISaveTaxBracket
    {
        public async Task<Guid> SaveAsync(SaveTaxBracketDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can configure tax brackets.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TaxBracket), dto.Id.Value.ToString());
                entity.Update(dto.LowerBound, dto.UpperBound, dto.RatePercent, dto.SortOrder);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = TaxBracket.Create(dto.LowerBound, dto.UpperBound, dto.RatePercent, dto.SortOrder);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeleteTaxBracket(
        IRepository<TaxBracket> repository,
        IPerformanceVisibilityService visibility) : IDeleteTaxBracket
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can configure tax brackets.");
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(TaxBracket), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetAllTaxBrackets(IRepository<TaxBracket> repository) : IGetAllTaxBrackets
    {
        public async Task<PaginatedResponse<TaxBracketDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 50;

            var query = repository.GetAll().AsNoTracking();
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.LowerBound)
                .Skip(skip).Take(take)
                .Select(x => new TaxBracketDto
                {
                    Id = x.Id, LowerBound = x.LowerBound, UpperBound = x.UpperBound,
                    RatePercent = x.RatePercent, SortOrder = x.SortOrder
                }).ToListAsync();

            return new PaginatedResponse<TaxBracketDto> { Total = total, Data = data };
        }
    }

    public class GetTaxBracketById(IRepository<TaxBracket> repository) : IGetTaxBracketById
    {
        public async Task<TaxBracketDto> GetAsync(Guid id) =>
            await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(x => new TaxBracketDto
                {
                    Id = x.Id, LowerBound = x.LowerBound, UpperBound = x.UpperBound,
                    RatePercent = x.RatePercent, SortOrder = x.SortOrder
                }).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(TaxBracket), id.ToString());
    }

    // ---- Deductions preview (HC232) — ties comp summary + tax + benefits ----
    public class GetPayrollDeductions(
        IGetCompensationSummary summaryHandler,
        IRepository<TaxBracket> taxBracketRepository,
        IRepository<EmployeeBenefitEnrollment> enrollmentRepository,
        IRepository<BenefitPlan> planRepository,
        IRepository<Loan> loanRepository,
        IRepository<LoanRepaymentScheduleLine> loanScheduleRepository) : IGetPayrollDeductions
    {
        public async Task<PayrollDeductionsDto> GetAsync(Guid employeeId)
        {
            // Visibility is enforced inside the compensation summary handler.
            var comp = await summaryHandler.GetAsync(employeeId);

            var brackets = await taxBracketRepository.GetAll().AsNoTracking()
                .OrderBy(b => b.LowerBound).ToListAsync();
            var incomeTax = TaxCalculator.Compute(brackets, comp.TaxableGross);

            // Active benefit contributions (non-terminated enrollments), resolved against base salary.
            var plans = planRepository.GetAll();
            var enrollments = await enrollmentRepository.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == employeeId && e.Status == BenefitEnrollmentStatus.Enrolled)
                .Select(e => new
                {
                    e.ElectedEmployeeContribution,
                    PlanName = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => p.Name).FirstOrDefault(),
                    EmpMethod = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (AllowanceCalcMethod?)p.EmployeeContributionMethod).FirstOrDefault(),
                    EmpRate = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (decimal?)p.EmployeeContributionRate).FirstOrDefault(),
                    ErMethod = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (AllowanceCalcMethod?)p.EmployerContributionMethod).FirstOrDefault(),
                    ErRate = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (decimal?)p.EmployerContributionRate).FirstOrDefault()
                })
                .ToListAsync();

            var lines = new List<DeductionLineDto>();
            if (incomeTax > 0)
                lines.Add(new DeductionLineDto { Label = "Income tax", Kind = "Tax", Amount = incomeTax });

            decimal empBenefits = 0m, erBenefits = 0m;
            foreach (var e in enrollments)
            {
                var empShare = e.ElectedEmployeeContribution
                    ?? (e.EmpMethod.HasValue ? CompensationShared.Resolve(e.EmpMethod.Value, e.EmpRate ?? 0, comp.BaseSalary) : 0);
                var erShare = e.ErMethod.HasValue ? CompensationShared.Resolve(e.ErMethod.Value, e.ErRate ?? 0, comp.BaseSalary) : 0;
                empBenefits += empShare;
                erBenefits += erShare;
                if (empShare > 0)
                    lines.Add(new DeductionLineDto { Label = $"{e.PlanName} contribution", Kind = "BenefitContribution", Amount = empShare });
            }

            // HC254 — each active loan's monthly installment is an automated payroll deduction
            // (capped at the loan's outstanding balance). Payroll module is out of scope, so the
            // deductions engine is the loan-repayment hand-off point.
            var activeLoans = await loanRepository.GetAll().AsNoTracking()
                .Where(l => l.EmployeeId == employeeId && l.Status == LoanStatus.Active)
                .Select(l => new { l.Id, l.LoanNumber, l.MonthlyInstallment })
                .ToListAsync();
            decimal loanRepayments = 0m;
            foreach (var l in activeLoans)
            {
                var outstanding = await loanScheduleRepository.GetAll().AsNoTracking()
                    .Where(sl => sl.LoanId == l.Id && sl.Status == LoanInstallmentStatus.Pending)
                    .SumAsync(sl => (decimal?)sl.Amount) ?? 0m;
                var due = Math.Min(l.MonthlyInstallment, outstanding);
                if (due <= 0) continue;
                loanRepayments += due;
                lines.Add(new DeductionLineDto { Label = $"Loan repayment ({l.LoanNumber})", Kind = "LoanInstallment", Amount = due });
            }

            var totalDeductions = incomeTax + empBenefits + loanRepayments;
            return new PayrollDeductionsDto
            {
                EmployeeId = employeeId,
                EmployeeName = comp.EmployeeName,
                BaseSalary = comp.BaseSalary,
                GrossPay = comp.GrossPay,
                TaxableGross = comp.TaxableGross,
                IncomeTax = incomeTax,
                EmployeeBenefitContributions = empBenefits,
                EmployerBenefitContributions = erBenefits,
                LoanRepayments = loanRepayments,
                TotalDeductions = totalDeductions,
                NetPay = comp.GrossPay - totalDeductions,
                Lines = lines
            };
        }
    }
}
