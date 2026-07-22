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
    // ---- DTOs ---------------------------------------------------------------
    public class BenefitEnrollmentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid BenefitPlanId { get; set; }
        public string? BenefitPlanName { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolledOn { get; set; }
        public DateTime CoverageStart { get; set; }
        public DateTime? CoverageEnd { get; set; }
        public decimal? ElectedEmployeeContribution { get; set; }
        /// <summary>Employee contribution resolved against base salary (elected overrides the plan rule).</summary>
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public string? Remark { get; set; }
    }

    public class EnrollBenefitDto
    {
        public Guid EmployeeId { get; set; }
        public Guid BenefitPlanId { get; set; }
        public DateTime CoverageStart { get; set; }
        public decimal? ElectedEmployeeContribution { get; set; }
        public string? Remark { get; set; }
    }

    public class EnrollBenefitDtoValidator : AbstractValidator<EnrollBenefitDto>
    {
        public EnrollBenefitDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.BenefitPlanId).NotEmpty();
            RuleFor(x => x.CoverageStart).NotEmpty();
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IEnrollBenefit { Task<Guid> EnrollAsync(EnrollBenefitDto dto); }
    public interface IWaiveBenefit { Task WaiveAsync(Guid enrollmentId, string? remark); }
    public interface ITerminateBenefit { Task TerminateAsync(Guid enrollmentId, DateTime coverageEnd, string? remark); }
    public interface IGetEmployeeBenefits { Task<List<BenefitEnrollmentDto>> GetAsync(Guid employeeId); }

    // ---- Handlers -----------------------------------------------------------
    public class EnrollBenefit(
        IRepository<EmployeeBenefitEnrollment> repository,
        IRepository<BenefitPlan> planRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IValidator<EnrollBenefitDto> validator,
        ILogger<EnrollBenefit> logger) : IEnrollBenefit
    {
        public async Task<Guid> EnrollAsync(EnrollBenefitDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // HC230 — an employee may self-enroll only while the plan's enrollment window is open;
            // HR (admin) may enroll anyone at any time (e.g. onboarding).
            var scope = await visibility.GetScopeAsync();
            var isSelf = scope.EmployeeId.HasValue && scope.EmployeeId.Value == dto.EmployeeId;
            if (!scope.IsAdmin && !isSelf)
                throw new ValidationException(nameof(dto.EmployeeId), "You can only enroll yourself in a benefit plan.");
            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            var plan = await planRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.BenefitPlanId)
                ?? throw new NotFoundException(nameof(BenefitPlan), dto.BenefitPlanId.ToString());
            if (!scope.IsAdmin && !plan.IsEnrollmentOpenOn(DateTime.UtcNow.Date))
                throw new ValidationException(nameof(dto.BenefitPlanId), "Enrollment for this plan is not currently open.");

            // One active (non-terminated) enrollment per employee+plan.
            if (await repository.GetAll().AnyAsync(e => e.EmployeeId == dto.EmployeeId
                    && e.BenefitPlanId == dto.BenefitPlanId && e.Status != BenefitEnrollmentStatus.Terminated))
                throw new ValidationException(nameof(dto.BenefitPlanId), "The employee already has an active enrollment in this plan.");

            var created = EmployeeBenefitEnrollment.Create(dto.BenefitPlanId, dto.EmployeeId, DateTime.UtcNow.Date,
                dto.CoverageStart, dto.ElectedEmployeeContribution, dto.Remark);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Enrolled Employee {EmployeeId} in BenefitPlan {PlanId}", dto.EmployeeId, dto.BenefitPlanId);
            return created.Id;
        }
    }

    public class WaiveBenefit(
        IRepository<EmployeeBenefitEnrollment> repository,
        IPerformanceVisibilityService visibility) : IWaiveBenefit
    {
        public async Task WaiveAsync(Guid enrollmentId, string? remark)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(e => e.Id == enrollmentId)
                ?? throw new NotFoundException(nameof(EmployeeBenefitEnrollment), enrollmentId.ToString());
            var scope = await visibility.GetScopeAsync();
            var isSelf = scope.EmployeeId.HasValue && scope.EmployeeId.Value == entity.EmployeeId;
            if (!scope.IsAdmin && !isSelf)
                throw new ValidationException(nameof(enrollmentId), "You cannot change this enrollment.");

            entity.Waive(remark);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class TerminateBenefit(
        IRepository<EmployeeBenefitEnrollment> repository,
        IPerformanceVisibilityService visibility) : ITerminateBenefit
    {
        public async Task TerminateAsync(Guid enrollmentId, DateTime coverageEnd, string? remark)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException(nameof(enrollmentId), "Only HR can terminate coverage.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(e => e.Id == enrollmentId)
                ?? throw new NotFoundException(nameof(EmployeeBenefitEnrollment), enrollmentId.ToString());
            entity.Terminate(coverageEnd, remark);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetEmployeeBenefits(
        IRepository<EmployeeBenefitEnrollment> repository,
        IRepository<BenefitPlan> planRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetEmployeeBenefits
    {
        public async Task<List<BenefitEnrollmentDto>> GetAsync(Guid employeeId)
        {
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(employeeId), "You do not have access to this employee's benefits.");

            var baseSalary = await CompensationSummaryHelper.ResolveBaseAsync(employeeRepository, employeeId);
            var plans = planRepository.GetAll();

            var rows = await repository.GetAll().AsNoTracking()
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.EnrolledOn)
                .Select(e => new
                {
                    e.Id, e.EmployeeId, e.BenefitPlanId, e.Status, e.EnrolledOn, e.CoverageStart, e.CoverageEnd,
                    e.ElectedEmployeeContribution, e.Remark,
                    PlanName = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => p.Name).FirstOrDefault(),
                    Category = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (BenefitCategory?)p.Category).FirstOrDefault(),
                    EmpMethod = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (AllowanceCalcMethod?)p.EmployeeContributionMethod).FirstOrDefault(),
                    EmpRate = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (decimal?)p.EmployeeContributionRate).FirstOrDefault(),
                    ErMethod = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (AllowanceCalcMethod?)p.EmployerContributionMethod).FirstOrDefault(),
                    ErRate = plans.Where(p => p.Id == e.BenefitPlanId).Select(p => (decimal?)p.EmployerContributionRate).FirstOrDefault()
                })
                .ToListAsync();

            return rows.Select(r => new BenefitEnrollmentDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                BenefitPlanId = r.BenefitPlanId,
                BenefitPlanName = r.PlanName,
                Category = r.Category?.ToString(),
                Status = r.Status.ToString(),
                EnrolledOn = r.EnrolledOn,
                CoverageStart = r.CoverageStart,
                CoverageEnd = r.CoverageEnd,
                ElectedEmployeeContribution = r.ElectedEmployeeContribution,
                EmployeeContribution = r.ElectedEmployeeContribution
                    ?? (r.EmpMethod.HasValue ? CompensationShared.Resolve(r.EmpMethod.Value, r.EmpRate ?? 0, baseSalary) : 0),
                EmployerContribution = r.ErMethod.HasValue ? CompensationShared.Resolve(r.ErMethod.Value, r.ErRate ?? 0, baseSalary) : 0,
                Remark = r.Remark
            }).ToList();
        }
    }
}
