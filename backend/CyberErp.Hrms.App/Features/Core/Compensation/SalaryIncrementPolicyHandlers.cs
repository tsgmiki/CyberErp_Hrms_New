using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    public class SalaryIncrementPolicyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MinimumServiceMonths { get; set; }
        public bool ProrateFirstYear { get; set; } = true;
        public bool ExcludeActiveDisciplinary { get; set; } = true;
        /// <summary>Move an employee onto the next grade when a step increment clears their ceiling.</summary>
        public bool PromoteOnGradeCeiling { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveSalaryIncrementPolicyDtoValidator : AbstractValidator<SalaryIncrementPolicyDto>
    {
        public SaveSalaryIncrementPolicyDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.MinimumServiceMonths)
                .InclusiveBetween(0, 60)
                .WithMessage("Minimum service must be between 0 and 60 months (0 = no tenure gate).");
        }
    }

    public interface IGetSalaryIncrementPolicy { Task<SalaryIncrementPolicyDto?> GetAsync(); }
    public interface ISaveSalaryIncrementPolicy { Task<Guid> SaveAsync(SalaryIncrementPolicyDto dto); }

    /// <summary>Reads the tenant's active policy; null means no rules are configured yet.</summary>
    public class GetSalaryIncrementPolicy(IRepository<SalaryIncrementPolicy> repository)
        : IGetSalaryIncrementPolicy
    {
        public Task<SalaryIncrementPolicyDto?> GetAsync() =>
            repository.GetAll().AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SalaryIncrementPolicyDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    MinimumServiceMonths = p.MinimumServiceMonths,
                    ProrateFirstYear = p.ProrateFirstYear,
                    ExcludeActiveDisciplinary = p.ExcludeActiveDisciplinary,
                    PromoteOnGradeCeiling = p.PromoteOnGradeCeiling,
                    IsActive = p.IsActive
                })
                .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Upserts the tenant's policy. One is active per tenant (same rule as WorkWeekConfiguration), so
    /// saving without an id updates the existing active row rather than stacking a second one that
    /// would silently compete to be "the" policy.
    /// </summary>
    public class SaveSalaryIncrementPolicy(
        IRepository<SalaryIncrementPolicy> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SalaryIncrementPolicyDto> validator) : ISaveSalaryIncrementPolicy
    {
        public async Task<Guid> SaveAsync(SalaryIncrementPolicyDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("scope", "Only HR can configure the salary increment policy.");

            var existing = dto.Id != Guid.Empty
                ? await repository.GetAll().FirstOrDefaultAsync(p => p.Id == dto.Id)
                : await repository.GetAll().Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync();

            if (existing is not null)
            {
                existing.Update(dto.Name, dto.MinimumServiceMonths, dto.ProrateFirstYear,
                    dto.ExcludeActiveDisciplinary, dto.IsActive, dto.PromoteOnGradeCeiling);
                repository.UpdateAsync(existing);
                await repository.SaveChangesAsync();
                return existing.Id;
            }

            var created = SalaryIncrementPolicy.Create(dto.Name, dto.MinimumServiceMonths,
                dto.ProrateFirstYear, dto.ExcludeActiveDisciplinary, dto.IsActive, dto.PromoteOnGradeCeiling);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }
}
