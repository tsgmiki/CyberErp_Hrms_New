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
        public Guid LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public int MinExperienceMonths { get; set; }
        public int NewEmployeeLeaveDays { get; set; }
        public int BaseLeaveDays { get; set; }
        public int ManagerialLeaveDays { get; set; }
        public int IncrementDays { get; set; }
        public int IncrementIntervalYears { get; set; }
        public int MaxLeaveDays { get; set; }
        public int ExpiryYears { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveAnnualLeaveSettingDto
    {
        public Guid? Id { get; set; }
        public Guid FiscalYearId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public int MinExperienceMonths { get; set; }
        public int NewEmployeeLeaveDays { get; set; }
        public int BaseLeaveDays { get; set; } = 16;
        public int ManagerialLeaveDays { get; set; } = 16;
        public int IncrementDays { get; set; } = 1;
        public int IncrementIntervalYears { get; set; } = 2;
        public int MaxLeaveDays { get; set; } = 35;
        public int ExpiryYears { get; set; } = 2;
        public bool IsActive { get; set; } = true;
    }

    public class SaveAnnualLeaveSettingDtoValidator : AbstractValidator<SaveAnnualLeaveSettingDto>
    {
        public SaveAnnualLeaveSettingDtoValidator()
        {
            RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("Fiscal year is required.");
            RuleFor(x => x.LeaveTypeId).NotEmpty().WithMessage("Leave type is required.");
            RuleFor(x => x.MinExperienceMonths).GreaterThanOrEqualTo(0);
            RuleFor(x => x.NewEmployeeLeaveDays).GreaterThanOrEqualTo(0);
            RuleFor(x => x.BaseLeaveDays).GreaterThan(0);
            RuleFor(x => x.ManagerialLeaveDays).GreaterThan(0);
            RuleFor(x => x.IncrementDays).GreaterThanOrEqualTo(0);
            RuleFor(x => x.IncrementIntervalYears).GreaterThanOrEqualTo(1);
            RuleFor(x => x.MaxLeaveDays).GreaterThanOrEqualTo(x => x.BaseLeaveDays)
                .WithMessage("Maximum leave days cannot be below the base entitlement.");
            RuleFor(x => x.ExpiryYears).GreaterThanOrEqualTo(1);
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
            LeaveTypeId = s.LeaveTypeId,
            LeaveTypeName = s.LeaveType != null ? s.LeaveType.Name : null,
            MinExperienceMonths = s.MinExperienceMonths,
            NewEmployeeLeaveDays = s.NewEmployeeLeaveDays,
            BaseLeaveDays = s.BaseLeaveDays,
            ManagerialLeaveDays = s.ManagerialLeaveDays,
            IncrementDays = s.IncrementDays,
            IncrementIntervalYears = s.IncrementIntervalYears,
            MaxLeaveDays = s.MaxLeaveDays,
            ExpiryYears = s.ExpiryYears,
            IsActive = s.IsActive
        };
    }

    // ---- Save ---------------------------------------------------------------
    public class SaveAnnualLeaveSetting(
        IRepository<AnnualLeaveSetting> repository,
        IRepository<FiscalYear> fiscalYears,
        IRepository<LeaveType> leaveTypes,
        IValidator<SaveAnnualLeaveSettingDto> validator,
        ILogger<SaveAnnualLeaveSetting> logger) : ISaveAnnualLeaveSetting
    {
        public async Task<Guid> SaveAsync(SaveAnnualLeaveSettingDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await fiscalYears.GetAll().AnyAsync(f => f.Id == dto.FiscalYearId))
                throw new NotFoundException(nameof(FiscalYear), dto.FiscalYearId.ToString());
            if (!await leaveTypes.GetAll().AnyAsync(t => t.Id == dto.LeaveTypeId))
                throw new NotFoundException(nameof(LeaveType), dto.LeaveTypeId.ToString());
            if (await repository.GetAll().AnyAsync(s =>
                    s.FiscalYearId == dto.FiscalYearId && s.LeaveTypeId == dto.LeaveTypeId && s.Id != dto.Id))
                throw new ValidationException("leaveTypeId", "A setting for this fiscal year and leave type already exists.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(AnnualLeaveSetting), dto.Id.Value.ToString());
                entity.Update(dto.FiscalYearId, dto.LeaveTypeId, dto.MinExperienceMonths, dto.NewEmployeeLeaveDays,
                    dto.BaseLeaveDays, dto.ManagerialLeaveDays, dto.IncrementDays, dto.IncrementIntervalYears,
                    dto.MaxLeaveDays, dto.ExpiryYears, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = AnnualLeaveSetting.Create(dto.FiscalYearId, dto.LeaveTypeId, dto.MinExperienceMonths,
                dto.NewEmployeeLeaveDays, dto.BaseLeaveDays, dto.ManagerialLeaveDays, dto.IncrementDays,
                dto.IncrementIntervalYears, dto.MaxLeaveDays, dto.ExpiryYears, dto.IsActive);
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
