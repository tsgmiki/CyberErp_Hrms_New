using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    public class PositionEntitlementDto
    {
        public Guid Id { get; set; }
        public Guid PositionClassId { get; set; }
        public string? PositionClassTitle { get; set; }
        public string Kind { get; set; } = nameof(EntitlementKind.Allowance);
        public Guid? AllowanceTypeId { get; set; }
        public Guid? BenefitPlanId { get; set; }
        /// <summary>The catalogue row's name, whichever side it is on.</summary>
        public string? ReferenceName { get; set; }
        public decimal? Value { get; set; }
        /// <summary>Shown when the entitlement names no value of its own.</summary>
        public decimal? DefaultRate { get; set; }
        /// <summary>Fixed | PercentOfBase — decides how Value reads.</summary>
        public string? CalcMethod { get; set; }
        public bool GrantedWhenActing { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class SavePositionEntitlementDto
    {
        public Guid? Id { get; set; }
        public Guid PositionClassId { get; set; }
        public string Kind { get; set; } = nameof(EntitlementKind.Allowance);
        public Guid? AllowanceTypeId { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public decimal? Value { get; set; }
        public bool GrantedWhenActing { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class SavePositionEntitlementDtoValidator : AbstractValidator<SavePositionEntitlementDto>
    {
        public SavePositionEntitlementDtoValidator()
        {
            RuleFor(x => x.PositionClassId).NotEmpty();
            RuleFor(x => x.Kind)
                .NotEmpty()
                .Must(k => Enum.TryParse<EntitlementKind>(k, out _))
                .WithMessage("Kind must be Allowance or BenefitPlan.");
            RuleFor(x => x.Value).GreaterThanOrEqualTo(0).When(x => x.Value.HasValue);
            RuleFor(x => x.Notes).MaximumLength(500);
            RuleFor(x => x.AllowanceTypeId).NotEmpty()
                .When(x => x.Kind == nameof(EntitlementKind.Allowance))
                .WithMessage("Choose the allowance this post carries.");
            RuleFor(x => x.BenefitPlanId).NotEmpty()
                .When(x => x.Kind == nameof(EntitlementKind.BenefitPlan))
                .WithMessage("Choose the benefit plan this post carries.");
        }
    }

    public interface ISavePositionEntitlement { Task<Guid> SaveAsync(SavePositionEntitlementDto dto); }
    public interface IDeletePositionEntitlement { Task DeleteAsync(Guid id); }
    public interface IGetPositionEntitlements { Task<List<PositionEntitlementDto>> GetAsync(Guid positionClassId); }

    /// <summary>
    /// What a post carries beyond its salary, as HR defines it.
    /// </summary>
    public class SavePositionEntitlement(
        IRepository<PositionEntitlement> repository,
        IRepository<PositionClass> positionClasses,
        IRepository<AllowanceType> allowanceTypes,
        IRepository<BenefitPlan> benefitPlans,
        IValidator<SavePositionEntitlementDto> validator,
        ILogger<SavePositionEntitlement> logger) : ISavePositionEntitlement
    {
        public async Task<Guid> SaveAsync(SavePositionEntitlementDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var kind = Enum.Parse<EntitlementKind>(dto.Kind);

            if (!await positionClasses.GetAll().AnyAsync(p => p.Id == dto.PositionClassId))
                throw new NotFoundException(nameof(PositionClass), dto.PositionClassId.ToString());

            if (kind == EntitlementKind.Allowance
                && !await allowanceTypes.GetAll().AnyAsync(a => a.Id == dto.AllowanceTypeId))
                throw new NotFoundException(nameof(AllowanceType), dto.AllowanceTypeId.ToString() ?? "");
            if (kind == EntitlementKind.BenefitPlan
                && !await benefitPlans.GetAll().AnyAsync(b => b.Id == dto.BenefitPlanId))
                throw new NotFoundException(nameof(BenefitPlan), dto.BenefitPlanId.ToString() ?? "");

            // ⚠️ One entitlement per post per catalogue row. Two rows for the same allowance would
            // grant it twice to a deputy, at two different values, with no way to say which is right.
            var reference = kind == EntitlementKind.Allowance ? dto.AllowanceTypeId : dto.BenefitPlanId;
            var duplicate = await repository.GetAll().AnyAsync(e =>
                e.PositionClassId == dto.PositionClassId
                && e.Id != (dto.Id ?? Guid.Empty)
                && (kind == EntitlementKind.Allowance
                        ? e.AllowanceTypeId == reference
                        : e.BenefitPlanId == reference));
            if (duplicate)
                throw new ValidationException("reference", "This post already carries that entitlement.");

            PositionEntitlement entity;
            if (dto.Id is Guid id && id != Guid.Empty)
            {
                entity = await repository.GetAll().FirstOrDefaultAsync(e => e.Id == id)
                    ?? throw new NotFoundException(nameof(PositionEntitlement), id.ToString());
                try
                {
                    entity.Update(kind, dto.AllowanceTypeId, dto.BenefitPlanId, dto.Value,
                        dto.GrantedWhenActing, dto.IsActive, dto.Notes);
                }
                catch (ArgumentException ex) { throw new ValidationException("entitlement", ex.Message); }
                repository.UpdateAsync(entity);
            }
            else
            {
                try
                {
                    entity = PositionEntitlement.Create(dto.PositionClassId, kind,
                        dto.AllowanceTypeId, dto.BenefitPlanId, dto.Value,
                        dto.GrantedWhenActing, dto.IsActive, dto.Notes);
                }
                catch (ArgumentException ex) { throw new ValidationException("entitlement", ex.Message); }
                await repository.AddAsync(entity);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Position entitlement {Id} saved for class {ClassId}",
                entity.Id, dto.PositionClassId);
            return entity.Id;
        }
    }

    public class DeletePositionEntitlement(
        IRepository<PositionEntitlement> repository,
        ILogger<DeletePositionEntitlement> logger) : IDeletePositionEntitlement
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(PositionEntitlement), id.ToString());

            // ⚠️ Deleting the DEFINITION does not touch anything already granted. Allowances handed
            // to a deputy are dated rows on their record with their own windows; removing what a
            // post carries from now on must not reach back and rewrite somebody's pay history.
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Position entitlement {Id} deleted", id);
        }
    }

    public class GetPositionEntitlements(
        IRepository<PositionEntitlement> repository,
        IRepository<PositionClass> positionClasses,
        IRepository<AllowanceType> allowanceTypes,
        IRepository<BenefitPlan> benefitPlans) : IGetPositionEntitlements
    {
        public async Task<List<PositionEntitlementDto>> GetAsync(Guid positionClassId)
        {
            var rows = await repository.GetAll().AsNoTracking()
                .Where(e => e.PositionClassId == positionClassId)
                .ToListAsync();
            if (rows.Count == 0) return [];

            var title = await positionClasses.GetAll().AsNoTracking()
                .Where(p => p.Id == positionClassId).Select(p => p.Title).FirstOrDefaultAsync();

            // Both catalogues in one read each, rather than a lookup per row.
            var allowanceIds = rows.Where(r => r.AllowanceTypeId.HasValue)
                .Select(r => r.AllowanceTypeId!.Value).Distinct().ToList();
            var planIds = rows.Where(r => r.BenefitPlanId.HasValue)
                .Select(r => r.BenefitPlanId!.Value).Distinct().ToList();

            var allowances = allowanceIds.Count == 0 ? [] : await allowanceTypes.GetAll().AsNoTracking()
                .Where(a => allowanceIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Name, a.DefaultRate, a.CalcMethod })
                .ToListAsync();
            var plans = planIds.Count == 0 ? [] : await benefitPlans.GetAll().AsNoTracking()
                .Where(b => planIds.Contains(b.Id))
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            return [.. rows.Select(r =>
            {
                var allowance = r.AllowanceTypeId is Guid aid ? allowances.FirstOrDefault(a => a.Id == aid) : null;
                var plan = r.BenefitPlanId is Guid bid ? plans.FirstOrDefault(b => b.Id == bid) : null;
                return new PositionEntitlementDto
                {
                    Id = r.Id,
                    PositionClassId = r.PositionClassId,
                    PositionClassTitle = title,
                    Kind = r.Kind.ToString(),
                    AllowanceTypeId = r.AllowanceTypeId,
                    BenefitPlanId = r.BenefitPlanId,
                    ReferenceName = allowance?.Name ?? plan?.Name,
                    Value = r.Value,
                    DefaultRate = allowance?.DefaultRate,
                    CalcMethod = allowance?.CalcMethod.ToString(),
                    GrantedWhenActing = r.GrantedWhenActing,
                    IsActive = r.IsActive,
                    Notes = r.Notes
                };
            })];
        }
    }
}
