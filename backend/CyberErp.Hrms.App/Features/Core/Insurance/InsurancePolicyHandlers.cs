using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Insurance
{
    // ---- DTOs ---------------------------------------------------------------
    public class InsurancePremiumScheduleDto
    {
        public Guid Id { get; set; }
        public int Installment { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string? PaymentReference { get; set; }
    }

    public class InsurancePolicyDto
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string InsurerName { get; set; } = string.Empty;
        public string InsuranceType { get; set; } = string.Empty;
        public string? Coverage { get; set; }
        public decimal CoverageAmount { get; set; }
        public int PolicyYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AnnualPremium { get; set; }
        public string PremiumFrequency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsRenewal { get; set; }
        public Guid? PreviousPolicyId { get; set; }
        public string? Notes { get; set; }
        public decimal PremiumPaid { get; set; }
        public decimal PremiumOutstanding { get; set; }
        public List<InsurancePremiumScheduleDto> Schedule { get; set; } = [];
    }

    public class SaveInsurancePolicyDto
    {
        public Guid? Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string InsurerName { get; set; } = string.Empty;
        public string InsuranceType { get; set; } = nameof(Dom.Entities.Core.InsuranceType.Health);
        public string? Coverage { get; set; }
        public decimal CoverageAmount { get; set; }
        public int PolicyYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AnnualPremium { get; set; }
        public string PremiumFrequency { get; set; } = nameof(Dom.Entities.Core.PremiumFrequency.Annual);
        public bool IsRenewal { get; set; }
        public Guid? PreviousPolicyId { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveInsurancePolicyDtoValidator : AbstractValidator<SaveInsurancePolicyDto>
    {
        public SaveInsurancePolicyDtoValidator()
        {
            RuleFor(x => x.PolicyNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.InsurerName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.InsuranceType).Must(v => Enum.TryParse<InsuranceType>(v, true, out _))
                .WithMessage("Insurance type must be Life, Health, Disability, Accident, WorkersCompensation or Other.");
            RuleFor(x => x.PremiumFrequency).Must(v => Enum.TryParse<PremiumFrequency>(v, true, out _))
                .WithMessage("Premium frequency must be Annual, SemiAnnual, Quarterly or Monthly.");
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
            RuleFor(x => x.CoverageAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.AnnualPremium).GreaterThanOrEqualTo(0);
        }
    }

    public class GeneratePremiumScheduleDto { public Guid InsurancePolicyId { get; set; } }
    public class AddPremiumScheduleDto { public Guid InsurancePolicyId { get; set; } public DateTime DueDate { get; set; } public decimal Amount { get; set; } }
    public class MarkPremiumPaidDto { public string? Reference { get; set; } }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveInsurancePolicy { Task<Guid> SaveAsync(SaveInsurancePolicyDto dto); }
    public interface IDeleteInsurancePolicy { Task DeleteAsync(Guid id); }
    public interface IGetInsurancePolicyById { Task<InsurancePolicyDto> GetAsync(Guid id); }
    public interface IGetAllInsurancePolicies { Task<PaginatedResponse<InsurancePolicyDto>> GetAsync(GetAllRequest request); }
    public interface IGeneratePremiumSchedule { Task GenerateAsync(Guid policyId); }
    public interface IAddPremiumSchedule { Task<Guid> AddAsync(AddPremiumScheduleDto dto); }
    public interface IRemovePremiumSchedule { Task RemoveAsync(Guid scheduleId); }
    public interface IMarkInsurancePremiumPaid { Task MarkPaidAsync(Guid scheduleId, string? reference); }

    internal static class InsuranceMappers
    {
        internal static readonly System.Linq.Expressions.Expression<Func<InsurancePremiumSchedule, InsurancePremiumScheduleDto>> Schedule = s => new InsurancePremiumScheduleDto
        {
            Id = s.Id, Installment = s.Installment, DueDate = s.DueDate, Amount = s.Amount,
            Status = s.Status.ToString(), PaidAt = s.PaidAt, PaymentReference = s.PaymentReference
        };

        internal static int InstallmentsFor(PremiumFrequency f) => f switch
        {
            PremiumFrequency.Monthly => 12,
            PremiumFrequency.Quarterly => 4,
            PremiumFrequency.SemiAnnual => 2,
            _ => 1
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveInsurancePolicy(
        IRepository<InsurancePolicy> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveInsurancePolicyDto> validator,
        ILogger<SaveInsurancePolicy> logger) : ISaveInsurancePolicy
    {
        public async Task<Guid> SaveAsync(SaveInsurancePolicyDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage insurance policies.");

            var type = Enum.Parse<InsuranceType>(dto.InsuranceType, true);
            var frequency = Enum.Parse<PremiumFrequency>(dto.PremiumFrequency, true);
            if (await repository.GetAll().AnyAsync(x => x.PolicyNumber == dto.PolicyNumber && x.Id != dto.Id))
                throw new DuplicateException(nameof(InsurancePolicy), nameof(dto.PolicyNumber), dto.PolicyNumber);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(InsurancePolicy), dto.Id.Value.ToString());
                entity.Update(dto.PolicyNumber, dto.InsurerName, type, dto.Coverage, dto.CoverageAmount, dto.PolicyYear,
                    dto.StartDate, dto.EndDate, dto.AnnualPremium, frequency, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            if (dto.PreviousPolicyId.HasValue && !await repository.GetAll().AnyAsync(x => x.Id == dto.PreviousPolicyId.Value))
                throw new NotFoundException(nameof(InsurancePolicy), dto.PreviousPolicyId.Value.ToString());

            var created = InsurancePolicy.Create(dto.PolicyNumber, dto.InsurerName, type, dto.Coverage, dto.CoverageAmount,
                dto.PolicyYear, dto.StartDate, dto.EndDate, dto.AnnualPremium, frequency, dto.IsRenewal, dto.PreviousPolicyId, dto.Notes);
            await repository.AddAsync(created);
            // Mark the prior policy renewed when this is a renewal.
            if (dto.IsRenewal && dto.PreviousPolicyId.HasValue)
            {
                var prior = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.PreviousPolicyId.Value);
                prior?.SetStatus(InsurancePolicyStatus.Renewed);
                if (prior != null) repository.UpdateAsync(prior);
            }
            await repository.SaveChangesAsync();
            logger.LogInformation("Created InsurancePolicy {Id} ({PolicyNumber})", created.Id, created.PolicyNumber);
            return created.Id;
        }
    }

    public class DeleteInsurancePolicy(
        IRepository<InsurancePolicy> repository,
        IRepository<InsurancePremiumSchedule> scheduleRepository,
        IPerformanceVisibilityService visibility) : IDeleteInsurancePolicy
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage insurance policies.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(InsurancePolicy), id.ToString());
            if (await scheduleRepository.GetAll().AnyAsync(s => s.InsurancePolicyId == id && s.Status == PremiumPaymentStatus.Paid))
                throw new ValidationException(nameof(id), "Cannot delete a policy that has paid premiums.");
            repository.Delete(entity);   // schedule rows cascade
            await repository.SaveChangesAsync();
        }
    }

    public class GetInsurancePolicyById(
        IRepository<InsurancePolicy> repository,
        IRepository<InsurancePremiumSchedule> scheduleRepository) : IGetInsurancePolicyById
    {
        public async Task<InsurancePolicyDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(p => new InsurancePolicyDto
                {
                    Id = p.Id, PolicyNumber = p.PolicyNumber, InsurerName = p.InsurerName, InsuranceType = p.InsuranceType.ToString(),
                    Coverage = p.Coverage, CoverageAmount = p.CoverageAmount, PolicyYear = p.PolicyYear, StartDate = p.StartDate,
                    EndDate = p.EndDate, AnnualPremium = p.AnnualPremium, PremiumFrequency = p.PremiumFrequency.ToString(),
                    Status = p.Status.ToString(), IsRenewal = p.IsRenewal, PreviousPolicyId = p.PreviousPolicyId, Notes = p.Notes
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(InsurancePolicy), id.ToString());

            dto.Schedule = await scheduleRepository.GetAll().AsNoTracking()
                .Where(s => s.InsurancePolicyId == id).OrderBy(s => s.Installment)
                .Select(InsuranceMappers.Schedule).ToListAsync();
            dto.PremiumPaid = dto.Schedule.Where(s => s.Status == nameof(PremiumPaymentStatus.Paid)).Sum(s => s.Amount);
            dto.PremiumOutstanding = dto.Schedule.Where(s => s.Status == nameof(PremiumPaymentStatus.Pending)).Sum(s => s.Amount);
            return dto;
        }
    }

    public class GetAllInsurancePolicies(IRepository<InsurancePolicy> repository) : IGetAllInsurancePolicies
    {
        public async Task<PaginatedResponse<InsurancePolicyDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.PolicyNumber.Contains(term) || x.InsurerName.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<InsurancePolicyStatus>(request.Status, true, out var st))
                query = query.Where(x => x.Status == st);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.StartDate).Skip(skip).Take(take)
                .Select(p => new InsurancePolicyDto
                {
                    Id = p.Id, PolicyNumber = p.PolicyNumber, InsurerName = p.InsurerName, InsuranceType = p.InsuranceType.ToString(),
                    Coverage = p.Coverage, CoverageAmount = p.CoverageAmount, PolicyYear = p.PolicyYear, StartDate = p.StartDate,
                    EndDate = p.EndDate, AnnualPremium = p.AnnualPremium, PremiumFrequency = p.PremiumFrequency.ToString(),
                    Status = p.Status.ToString(), IsRenewal = p.IsRenewal, PreviousPolicyId = p.PreviousPolicyId, Notes = p.Notes
                }).ToListAsync();
            return new PaginatedResponse<InsurancePolicyDto> { Total = total, Data = data };
        }
    }

    /// <summary>Auto-splits the annual premium into installments per the policy frequency (HC247).</summary>
    public class GeneratePremiumSchedule(
        IRepository<InsurancePolicy> repository,
        IRepository<InsurancePremiumSchedule> scheduleRepository,
        IPerformanceVisibilityService visibility) : IGeneratePremiumSchedule
    {
        public async Task GenerateAsync(Guid policyId)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(policyId), "Only HR can manage premium schedules.");
            var policy = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(p => p.Id == policyId)
                ?? throw new NotFoundException(nameof(InsurancePolicy), policyId.ToString());

            var existing = await scheduleRepository.GetAll().Where(s => s.InsurancePolicyId == policyId).ToListAsync();
            if (existing.Any(s => s.Status == PremiumPaymentStatus.Paid))
                throw new ValidationException(nameof(policyId), "Cannot regenerate a schedule that already has paid premiums.");
            foreach (var e in existing) scheduleRepository.Delete(e);

            var count = InsuranceMappers.InstallmentsFor(policy.PremiumFrequency);
            var intervalMonths = 12 / count;
            var per = Math.Round(policy.AnnualPremium / count, 2);
            var allocated = 0m;
            for (var i = 0; i < count; i++)
            {
                // The final installment absorbs any rounding remainder.
                var amount = i == count - 1 ? policy.AnnualPremium - allocated : per;
                allocated += amount;
                await scheduleRepository.AddAsync(InsurancePremiumSchedule.Create(policyId, i + 1, policy.StartDate.AddMonths(i * intervalMonths), amount));
            }
            await scheduleRepository.SaveChangesAsync();
        }
    }

    public class AddPremiumSchedule(
        IRepository<InsurancePolicy> repository,
        IRepository<InsurancePremiumSchedule> scheduleRepository,
        IPerformanceVisibilityService visibility) : IAddPremiumSchedule
    {
        public async Task<Guid> AddAsync(AddPremiumScheduleDto dto)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(dto.InsurancePolicyId), "Only HR can manage premium schedules.");
            if (!await repository.GetAll().AnyAsync(p => p.Id == dto.InsurancePolicyId))
                throw new NotFoundException(nameof(InsurancePolicy), dto.InsurancePolicyId.ToString());
            if (dto.Amount < 0) throw new ValidationException(nameof(dto.Amount), "Premium amount cannot be negative.");
            var nextNo = 1 + await scheduleRepository.GetAll().Where(s => s.InsurancePolicyId == dto.InsurancePolicyId)
                .Select(s => (int?)s.Installment).MaxAsync() ?? 1;
            var created = InsurancePremiumSchedule.Create(dto.InsurancePolicyId, nextNo, dto.DueDate, dto.Amount);
            await scheduleRepository.AddAsync(created);
            await scheduleRepository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class RemovePremiumSchedule(
        IRepository<InsurancePremiumSchedule> scheduleRepository,
        IPerformanceVisibilityService visibility) : IRemovePremiumSchedule
    {
        public async Task RemoveAsync(Guid scheduleId)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(scheduleId), "Only HR can manage premium schedules.");
            var entity = await scheduleRepository.GetByIdAsync(scheduleId) ?? throw new NotFoundException(nameof(InsurancePremiumSchedule), scheduleId.ToString());
            if (entity.Status == PremiumPaymentStatus.Paid)
                throw new ValidationException(nameof(scheduleId), "A paid premium cannot be removed.");
            scheduleRepository.Delete(entity);
            await scheduleRepository.SaveChangesAsync();
        }
    }

    /// <summary>HC250 — marks a premium installment paid (finance/CBS hand-off; live integration deferred).</summary>
    public class MarkInsurancePremiumPaid(
        IRepository<InsurancePremiumSchedule> scheduleRepository,
        IPerformanceVisibilityService visibility) : IMarkInsurancePremiumPaid
    {
        public async Task MarkPaidAsync(Guid scheduleId, string? reference)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(scheduleId), "Only HR can record premium payment.");
            var entity = await scheduleRepository.GetAll().FirstOrDefaultAsync(s => s.Id == scheduleId)
                ?? throw new NotFoundException(nameof(InsurancePremiumSchedule), scheduleId.ToString());
            if (entity.Status == PremiumPaymentStatus.Paid)
                throw new ValidationException(nameof(scheduleId), "This premium is already paid.");
            entity.MarkPaid(DateTime.UtcNow.Date, reference);
            scheduleRepository.UpdateAsync(entity);
            await scheduleRepository.SaveChangesAsync();
        }
    }
}
