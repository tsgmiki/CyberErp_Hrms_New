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
    public class LeaveTypeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameA { get; set; }
        public bool IsPaid { get; set; }
        public bool RequiresApproval { get; set; }
        public bool AllowHalfDay { get; set; }
        public string GenderEligibility { get; set; } = nameof(LeaveGenderEligibility.Any);
        public decimal DefaultAnnualEntitlement { get; set; }
        public string AccrualMethod { get; set; } = nameof(LeaveAccrualMethod.Annual);
        public decimal? CarryForwardMaxDays { get; set; }
        public int? MaxConsecutiveDays { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveLeaveTypeDto
    {
        public Guid? Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameA { get; set; }
        public bool IsPaid { get; set; } = true;
        public bool RequiresApproval { get; set; } = true;
        public bool AllowHalfDay { get; set; }
        public LeaveGenderEligibility GenderEligibility { get; set; } = LeaveGenderEligibility.Any;
        public decimal DefaultAnnualEntitlement { get; set; }
        public LeaveAccrualMethod AccrualMethod { get; set; } = LeaveAccrualMethod.Annual;
        public decimal? CarryForwardMaxDays { get; set; }
        public int? MaxConsecutiveDays { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveLeaveTypeDtoValidator : AbstractValidator<SaveLeaveTypeDto>
    {
        public SaveLeaveTypeDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NameA).MaximumLength(200);
            RuleFor(x => x.DefaultAnnualEntitlement).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CarryForwardMaxDays).GreaterThanOrEqualTo(0).When(x => x.CarryForwardMaxDays.HasValue);
            RuleFor(x => x.MaxConsecutiveDays).GreaterThan(0).When(x => x.MaxConsecutiveDays.HasValue);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveLeaveType { Task<Guid> SaveAsync(SaveLeaveTypeDto dto); }
    public interface IGetLeaveTypeById { Task<LeaveTypeDto> GetAsync(Guid id); }
    public interface IGetAllLeaveTypes { Task<PaginatedResponse<LeaveTypeDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteLeaveType { Task DeleteAsync(Guid id); }

    // ---- Save (create/update) ----------------------------------------------
    public class SaveLeaveType(
        IRepository<LeaveType> repository,
        IValidator<SaveLeaveTypeDto> validator,
        ILogger<SaveLeaveType> logger) : ISaveLeaveType
    {
        public async Task<Guid> SaveAsync(SaveLeaveTypeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(LeaveType), nameof(dto.Code), dto.Code);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(LeaveType), dto.Id.Value.ToString());
                entity.Update(dto.Code, dto.Name, dto.NameA, dto.IsPaid, dto.RequiresApproval, dto.AllowHalfDay,
                    dto.GenderEligibility, dto.DefaultAnnualEntitlement, dto.AccrualMethod, dto.CarryForwardMaxDays,
                    dto.MaxConsecutiveDays, dto.Description, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = LeaveType.Create(dto.Code, dto.Name, dto.NameA, dto.IsPaid, dto.RequiresApproval,
                dto.AllowHalfDay, dto.GenderEligibility, dto.DefaultAnnualEntitlement, dto.AccrualMethod,
                dto.CarryForwardMaxDays, dto.MaxConsecutiveDays, dto.Description, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created LeaveType {Id} ({Code})", created.Id, created.Code);
            return created.Id;
        }
    }

    // ---- Get by id ----------------------------------------------------------
    public class GetLeaveTypeById(IRepository<LeaveType> repository) : IGetLeaveTypeById
    {
        public async Task<LeaveTypeDto> GetAsync(Guid id)
        {
            var e = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(LeaveType), id.ToString());
            return LeaveTypeMapper.Map(e);
        }
    }

    // ---- Get all (paged) ----------------------------------------------------
    public class GetAllLeaveTypes(IRepository<LeaveType> repository) : IGetAllLeaveTypes
    {
        public async Task<PaginatedResponse<LeaveTypeDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Code)
                .Skip(skip).Take(take)
                .Select(LeaveTypeMapper.Projection)
                .ToListAsync();

            return new PaginatedResponse<LeaveTypeDto> { Total = total, Data = data };
        }
    }

    // ---- Delete -------------------------------------------------------------
    public class DeleteLeaveType(
        IRepository<LeaveType> repository,
        ILogger<DeleteLeaveType> logger) : IDeleteLeaveType
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(LeaveType), id.ToString());
            // Phase 2 will add guards for balances/requests that reference this type.
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted LeaveType {Id}", id);
        }
    }

    internal static class LeaveTypeMapper
    {
        public static readonly System.Linq.Expressions.Expression<Func<LeaveType, LeaveTypeDto>> Projection = e => new LeaveTypeDto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            NameA = e.NameA,
            IsPaid = e.IsPaid,
            RequiresApproval = e.RequiresApproval,
            AllowHalfDay = e.AllowHalfDay,
            GenderEligibility = e.GenderEligibility.ToString(),
            DefaultAnnualEntitlement = e.DefaultAnnualEntitlement,
            AccrualMethod = e.AccrualMethod.ToString(),
            CarryForwardMaxDays = e.CarryForwardMaxDays,
            MaxConsecutiveDays = e.MaxConsecutiveDays,
            Description = e.Description,
            IsActive = e.IsActive
        };

        public static LeaveTypeDto Map(LeaveType e) => Projection.Compile()(e);
    }
}
