using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    // ---- DTOs ---------------------------------------------------------------
    public class AnnualLeaveSettingDto
    {
        public Guid Id { get; set; }
        public Guid FiscalYearId { get; set; }
        public string? FiscalYearName { get; set; }
        public int MinExperienceMonths { get; set; }
        public int NewEmployeeLeaveDays { get; set; }
        public int BaseLeaveDays { get; set; }
        public int ManagerialLeaveDays { get; set; }
        public int IncrementDays { get; set; }
        public int IncrementIntervalYears { get; set; }
        public int MaxLeaveDays { get; set; }
        public int ExpiryYears { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public bool ConsiderExternalExperience { get; set; }
        public DateTime? MilestoneDate { get; set; }
        public int PreMilestoneBaseLeaveDays { get; set; }
        public int PreMilestoneIncrementDays { get; set; }
        public int PreMilestoneIntervalYears { get; set; }
        public decimal DefaultAnnualEntitlement { get; set; }
        public decimal? CarryForwardMaxDays { get; set; }
        public int? MaxConsecutiveDays { get; set; }
        public bool AllowHalfDay { get; set; } = true;
        public bool IsActive { get; set; }
    }

    public class SaveAnnualLeaveSettingDto
    {
        public Guid? Id { get; set; }
        public Guid FiscalYearId { get; set; }

        public int MinExperienceMonths { get; set; }
        public int NewEmployeeLeaveDays { get; set; }
        public int BaseLeaveDays { get; set; } = 16;
        public int ManagerialLeaveDays { get; set; } = 16;
        public int IncrementDays { get; set; } = 1;
        public int IncrementIntervalYears { get; set; } = 2;
        public int MaxLeaveDays { get; set; } = 35;
        public int ExpiryYears { get; set; } = 2;
        /// <summary>ServiceMilestone | ServiceYears | FiscalYears</summary>
        public string RuleType { get; set; } = nameof(LeaveAccrualRuleType.ServiceYears);
        public bool ConsiderExternalExperience { get; set; }
        public DateTime? MilestoneDate { get; set; }
        public int PreMilestoneBaseLeaveDays { get; set; } = 14;
        public int PreMilestoneIncrementDays { get; set; } = 1;
        public int PreMilestoneIntervalYears { get; set; } = 1;
        /// <summary>Fallback entitlement for balances the accrual engine has not generated (was LeaveType.DefaultAnnualEntitlement).</summary>
        public decimal DefaultAnnualEntitlement { get; set; }
        /// <summary>Rollover carry cap; null = unlimited, 0 = none (was LeaveType.CarryForwardMaxDays).</summary>
        public decimal? CarryForwardMaxDays { get; set; }
        /// <summary>Cap on one continuous request; null = no cap (was LeaveType.MaxConsecutiveDays).</summary>
        public int? MaxConsecutiveDays { get; set; }
        /// <summary>Allow half-day request lines (was LeaveType.AllowHalfDay).</summary>
        public bool AllowHalfDay { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }

    public class SaveAnnualLeaveSettingDtoValidator : AbstractValidator<SaveAnnualLeaveSettingDto>
    {
        public SaveAnnualLeaveSettingDtoValidator()
        {
            RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("Fiscal year is required.");
            RuleFor(x => x.MinExperienceMonths).GreaterThanOrEqualTo(0);
            RuleFor(x => x.NewEmployeeLeaveDays).GreaterThanOrEqualTo(0);
            RuleFor(x => x.BaseLeaveDays).GreaterThan(0);
            RuleFor(x => x.ManagerialLeaveDays).GreaterThan(0);
            RuleFor(x => x.IncrementDays).GreaterThanOrEqualTo(0);
            RuleFor(x => x.IncrementIntervalYears).GreaterThanOrEqualTo(1);
            // 0 = uncapped; otherwise the cap must not sit below the base entitlement.
            RuleFor(x => x.MaxLeaveDays).Must((dto, max) => max == 0 || max >= dto.BaseLeaveDays)
                .WithMessage("Maximum leave days cannot be below the base entitlement.");
            RuleFor(x => x.ExpiryYears).GreaterThanOrEqualTo(1);
            RuleFor(x => x.RuleType).NotEmpty()
                .Must(v => Enum.TryParse<LeaveAccrualRuleType>(v, out _))
                .WithMessage("Rule type must be ServiceMilestone, ServiceYears or FiscalYears.");
            RuleFor(x => x.MilestoneDate).NotNull()
                .When(x => x.RuleType == nameof(LeaveAccrualRuleType.ServiceMilestone))
                .WithMessage("A milestone date is required for the service-milestone rule.");
            RuleFor(x => x.PreMilestoneBaseLeaveDays).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PreMilestoneIncrementDays).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DefaultAnnualEntitlement).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CarryForwardMaxDays).GreaterThanOrEqualTo(0).When(x => x.CarryForwardMaxDays.HasValue);
            RuleFor(x => x.MaxConsecutiveDays).GreaterThan(0).When(x => x.MaxConsecutiveDays.HasValue);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveAnnualLeaveSetting { Task<Guid> SaveAsync(SaveAnnualLeaveSettingDto dto); }
    public interface IGetAnnualLeaveSettingById { Task<AnnualLeaveSettingDto> GetAsync(Guid id); }
    public interface IGetAllAnnualLeaveSettings { Task<PaginatedResponse<AnnualLeaveSettingDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteAnnualLeaveSetting { Task DeleteAsync(Guid id); }

    internal static class AnnualLeaveSettingMapper
    {
        public static readonly System.Linq.Expressions.Expression<Func<AnnualLeaveSetting, AnnualLeaveSettingDto>> Projection = s => new AnnualLeaveSettingDto
        {
            Id = s.Id,
            FiscalYearId = s.FiscalYearId,
            FiscalYearName = s.FiscalYear != null ? s.FiscalYear.Name : null,


            MinExperienceMonths = s.MinExperienceMonths,
            NewEmployeeLeaveDays = s.NewEmployeeLeaveDays,
            BaseLeaveDays = s.BaseLeaveDays,
            ManagerialLeaveDays = s.ManagerialLeaveDays,
            IncrementDays = s.IncrementDays,
            IncrementIntervalYears = s.IncrementIntervalYears,
            MaxLeaveDays = s.MaxLeaveDays,
            ExpiryYears = s.ExpiryYears,
            RuleType = s.RuleType.ToString(),
            ConsiderExternalExperience = s.ConsiderExternalExperience,
            MilestoneDate = s.MilestoneDate,
            PreMilestoneBaseLeaveDays = s.PreMilestoneBaseLeaveDays,
            PreMilestoneIncrementDays = s.PreMilestoneIncrementDays,
            PreMilestoneIntervalYears = s.PreMilestoneIntervalYears,
            DefaultAnnualEntitlement = s.DefaultAnnualEntitlement,
            CarryForwardMaxDays = s.CarryForwardMaxDays,
            MaxConsecutiveDays = s.MaxConsecutiveDays,
            AllowHalfDay = s.AllowHalfDay,
            IsActive = s.IsActive
        };
    }

    // ---- Save ---------------------------------------------------------------
    public class SaveAnnualLeaveSetting(
        IRepository<AnnualLeaveSetting> repository,
        IRepository<FiscalYear> fiscalYears,
        IValidator<SaveAnnualLeaveSettingDto> validator,
        ILogger<SaveAnnualLeaveSetting> logger) : ISaveAnnualLeaveSetting
    {
        public async Task<Guid> SaveAsync(SaveAnnualLeaveSettingDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await fiscalYears.GetAll().AnyAsync(f => f.Id == dto.FiscalYearId))
                throw new NotFoundException(nameof(FiscalYear), dto.FiscalYearId.ToString());
            // One ANNUAL policy per fiscal year (the LeaveType relationship moved to Other Leave Settings).
            if (await repository.GetAll().AnyAsync(s => s.FiscalYearId == dto.FiscalYearId && s.Id != dto.Id))
                throw new ValidationException("fiscalYearId", "A setting for this fiscal year already exists.");

            var ruleType = Enum.Parse<LeaveAccrualRuleType>(dto.RuleType);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(AnnualLeaveSetting), dto.Id.Value.ToString());
                entity.Update(dto.FiscalYearId, dto.MinExperienceMonths, dto.NewEmployeeLeaveDays,
                    dto.BaseLeaveDays, dto.ManagerialLeaveDays, dto.IncrementDays, dto.IncrementIntervalYears,
                    dto.MaxLeaveDays, dto.ExpiryYears, ruleType, dto.ConsiderExternalExperience, dto.MilestoneDate,
                    dto.PreMilestoneBaseLeaveDays, dto.PreMilestoneIncrementDays, dto.PreMilestoneIntervalYears,
                    dto.DefaultAnnualEntitlement, dto.CarryForwardMaxDays, dto.MaxConsecutiveDays, dto.IsActive, dto.AllowHalfDay);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = AnnualLeaveSetting.Create(dto.FiscalYearId, dto.MinExperienceMonths,
                dto.NewEmployeeLeaveDays, dto.BaseLeaveDays, dto.ManagerialLeaveDays, dto.IncrementDays,
                dto.IncrementIntervalYears, dto.MaxLeaveDays, dto.ExpiryYears, ruleType, dto.ConsiderExternalExperience,
                dto.MilestoneDate, dto.PreMilestoneBaseLeaveDays, dto.PreMilestoneIncrementDays,
                dto.PreMilestoneIntervalYears, dto.DefaultAnnualEntitlement, dto.CarryForwardMaxDays,
                dto.MaxConsecutiveDays, dto.IsActive, dto.AllowHalfDay);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created AnnualLeaveSetting {Id}", created.Id);
            return created.Id;
        }
    }

    // ---- Reads / delete -------------------------------------------------------
    public class GetAnnualLeaveSettingById(IRepository<AnnualLeaveSetting> repository) : IGetAnnualLeaveSettingById
    {
        public async Task<AnnualLeaveSettingDto> GetAsync(Guid id)
        {
            return await repository.GetAll().Where(x => x.Id == id)
                .Select(AnnualLeaveSettingMapper.Projection).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(AnnualLeaveSetting), id.ToString());
        }
    }

    public class GetAllAnnualLeaveSettings(IRepository<AnnualLeaveSetting> repository) : IGetAllAnnualLeaveSettings
    {
        public async Task<PaginatedResponse<AnnualLeaveSettingDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(AnnualLeaveSettingMapper.Projection)
                .ToListAsync();
            return new PaginatedResponse<AnnualLeaveSettingDto> { Total = total, Data = data };
        }
    }

    public class DeleteAnnualLeaveSetting(
        IRepository<AnnualLeaveSetting> repository,
        ILogger<DeleteAnnualLeaveSetting> logger) : IDeleteAnnualLeaveSetting
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(AnnualLeaveSetting), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted AnnualLeaveSetting {Id}", id);
        }
    }
}
