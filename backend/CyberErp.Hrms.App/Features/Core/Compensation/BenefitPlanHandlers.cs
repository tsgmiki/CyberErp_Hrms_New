using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    // ---- DTOs ---------------------------------------------------------------
    public class BenefitPlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string EmployeeContributionMethod { get; set; } = string.Empty;
        public decimal EmployeeContributionRate { get; set; }
        public string EmployerContributionMethod { get; set; } = string.Empty;
        public decimal EmployerContributionRate { get; set; }
        public DateTime? EnrollmentOpenFrom { get; set; }
        public DateTime? EnrollmentOpenTo { get; set; }
        public bool IsActive { get; set; }
        public bool IsEnrollmentOpen { get; set; }
    }

    public class SaveBenefitPlanDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = nameof(BenefitCategory.Health);
        public string? Description { get; set; }
        public string EmployeeContributionMethod { get; set; } = nameof(AllowanceCalcMethod.Fixed);
        public decimal EmployeeContributionRate { get; set; }
        public string EmployerContributionMethod { get; set; } = nameof(AllowanceCalcMethod.Fixed);
        public decimal EmployerContributionRate { get; set; }
        public DateTime? EnrollmentOpenFrom { get; set; }
        public DateTime? EnrollmentOpenTo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveBenefitPlanDtoValidator : AbstractValidator<SaveBenefitPlanDto>
    {
        public SaveBenefitPlanDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Category).Must(v => Enum.TryParse<BenefitCategory>(v, true, out _))
                .WithMessage("Category must be Health, Life, Disability, Pension or Other.");
            RuleFor(x => x.EmployeeContributionMethod).Must(v => Enum.TryParse<AllowanceCalcMethod>(v, true, out _));
            RuleFor(x => x.EmployerContributionMethod).Must(v => Enum.TryParse<AllowanceCalcMethod>(v, true, out _));
            RuleFor(x => x.EmployeeContributionRate).GreaterThanOrEqualTo(0);
            RuleFor(x => x.EmployerContributionRate).GreaterThanOrEqualTo(0);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveBenefitPlan { Task<Guid> SaveAsync(SaveBenefitPlanDto dto); }
    public interface IDeleteBenefitPlan { Task DeleteAsync(Guid id); }
    public interface IGetBenefitPlanById { Task<BenefitPlanDto> GetAsync(Guid id); }
    public interface IGetAllBenefitPlans { Task<PaginatedResponse<BenefitPlanDto>> GetAsync(GetAllRequest request); }

    internal static class BenefitPlanMapper
    {
        internal static BenefitPlanDto Map(BenefitPlan b, DateTime today) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Category = b.Category.ToString(),
            Description = b.Description,
            EmployeeContributionMethod = b.EmployeeContributionMethod.ToString(),
            EmployeeContributionRate = b.EmployeeContributionRate,
            EmployerContributionMethod = b.EmployerContributionMethod.ToString(),
            EmployerContributionRate = b.EmployerContributionRate,
            EnrollmentOpenFrom = b.EnrollmentOpenFrom,
            EnrollmentOpenTo = b.EnrollmentOpenTo,
            IsActive = b.IsActive,
            IsEnrollmentOpen = b.IsEnrollmentOpenOn(today)
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveBenefitPlan(
        IRepository<BenefitPlan> repository,
        Performance.IPerformanceVisibilityService visibility,
        IValidator<SaveBenefitPlanDto> validator,
        ILogger<SaveBenefitPlan> logger) : ISaveBenefitPlan
    {
        public async Task<Guid> SaveAsync(SaveBenefitPlanDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can manage benefit plans.");

            var category = Enum.Parse<BenefitCategory>(dto.Category, true);
            var em = Enum.Parse<AllowanceCalcMethod>(dto.EmployeeContributionMethod, true);
            var pm = Enum.Parse<AllowanceCalcMethod>(dto.EmployerContributionMethod, true);

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(BenefitPlan), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(BenefitPlan), dto.Id.Value.ToString());
                entity.Update(dto.Name, category, dto.Description, em, dto.EmployeeContributionRate, pm,
                    dto.EmployerContributionRate, dto.EnrollmentOpenFrom, dto.EnrollmentOpenTo, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated BenefitPlan {Id}", entity.Id);
                return entity.Id;
            }

            var created = BenefitPlan.Create(dto.Name, category, dto.Description, em, dto.EmployeeContributionRate, pm,
                dto.EmployerContributionRate, dto.EnrollmentOpenFrom, dto.EnrollmentOpenTo, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created BenefitPlan {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteBenefitPlan(
        IRepository<BenefitPlan> repository,
        IRepository<EmployeeBenefitEnrollment> enrollmentRepository,
        Performance.IPerformanceVisibilityService visibility,
        ILogger<DeleteBenefitPlan> logger) : IDeleteBenefitPlan
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can manage benefit plans.");

            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(BenefitPlan), id.ToString());
            if (await enrollmentRepository.GetAll().AnyAsync(e => e.BenefitPlanId == id))
                throw new ValidationException(nameof(id), "Cannot delete a plan that employees are enrolled in.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted BenefitPlan {Id}", id);
        }
    }

    public class GetBenefitPlanById(IRepository<BenefitPlan> repository) : IGetBenefitPlanById
    {
        public async Task<BenefitPlanDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(BenefitPlan), id.ToString());
            return BenefitPlanMapper.Map(entity, DateTime.UtcNow.Date);
        }
    }

    public class GetAllBenefitPlans(IRepository<BenefitPlan> repository) : IGetAllBenefitPlans
    {
        public async Task<PaginatedResponse<BenefitPlanDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var today = DateTime.UtcNow.Date;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();
            var rows = await query.OrderBy(x => x.Name).Skip(skip).Take(take).ToListAsync();

            return new PaginatedResponse<BenefitPlanDto>
            {
                Total = total,
                Data = rows.Select(b => BenefitPlanMapper.Map(b, today)).ToList()
            };
        }
    }
}
