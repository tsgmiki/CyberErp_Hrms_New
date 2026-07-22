using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Medical
{
    // ================= Medical Provider (HC238) =================
    public class MedicalProviderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveMedicalProviderDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProviderType { get; set; } = nameof(MedicalProviderType.Hospital);
        public string? Specialization { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveMedicalProviderDtoValidator : AbstractValidator<SaveMedicalProviderDto>
    {
        public SaveMedicalProviderDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ProviderType).Must(v => Enum.TryParse<MedicalProviderType>(v, true, out _))
                .WithMessage("Provider type must be Hospital, Clinic, Laboratory, Pharmacy or Other.");
            RuleFor(x => x.Email).MaximumLength(150);
            RuleFor(x => x.Address).MaximumLength(500);
        }
    }

    public interface ISaveMedicalProvider { Task<Guid> SaveAsync(SaveMedicalProviderDto dto); }
    public interface IDeleteMedicalProvider { Task DeleteAsync(Guid id); }
    public interface IGetMedicalProviderById { Task<MedicalProviderDto> GetAsync(Guid id); }
    public interface IGetAllMedicalProviders { Task<PaginatedResponse<MedicalProviderDto>> GetAsync(GetAllRequest request); }

    internal static class MedicalMappers
    {
        internal static readonly System.Linq.Expressions.Expression<Func<MedicalProvider, MedicalProviderDto>> Provider = p => new MedicalProviderDto
        {
            Id = p.Id, Name = p.Name, ProviderType = p.ProviderType.ToString(), Specialization = p.Specialization,
            PhoneNumber = p.PhoneNumber, Email = p.Email, Address = p.Address, IsActive = p.IsActive
        };
    }

    public class SaveMedicalProvider(
        IRepository<MedicalProvider> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveMedicalProviderDto> validator,
        ILogger<SaveMedicalProvider> logger) : ISaveMedicalProvider
    {
        public async Task<Guid> SaveAsync(SaveMedicalProviderDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage medical providers.");

            var type = Enum.Parse<MedicalProviderType>(dto.ProviderType, true);
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(MedicalProvider), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(MedicalProvider), dto.Id.Value.ToString());
                entity.Update(dto.Name, type, dto.Specialization, dto.PhoneNumber, dto.Email, dto.Address, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }
            var created = MedicalProvider.Create(dto.Name, type, dto.Specialization, dto.PhoneNumber, dto.Email, dto.Address, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created MedicalProvider {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteMedicalProvider(
        IRepository<MedicalProvider> repository,
        IRepository<MedicalServiceContract> contractRepository,
        IPerformanceVisibilityService visibility) : IDeleteMedicalProvider
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage medical providers.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(MedicalProvider), id.ToString());
            if (await contractRepository.GetAll().AnyAsync(c => c.MedicalProviderId == id))
                throw new ValidationException(nameof(id), "Cannot delete a provider with service contracts.");
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetMedicalProviderById(IRepository<MedicalProvider> repository) : IGetMedicalProviderById
    {
        public async Task<MedicalProviderDto> GetAsync(Guid id) =>
            await repository.GetAll().AsNoTracking().Where(x => x.Id == id).Select(MedicalMappers.Provider).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(MedicalProvider), id.ToString());
    }

    public class GetAllMedicalProviders(IRepository<MedicalProvider> repository) : IGetAllMedicalProviders
    {
        public async Task<PaginatedResponse<MedicalProviderDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || (x.Specialization != null && x.Specialization.Contains(term)));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take).Select(MedicalMappers.Provider).ToListAsync();
            return new PaginatedResponse<MedicalProviderDto> { Total = total, Data = data };
        }
    }

    // ================= Medical Plan (HC235) =================
    public class MedicalPlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? AnnualCoverageLimit { get; set; }
        public decimal CoveragePercent { get; set; }
        public bool CoversDependents { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveMedicalPlanDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? AnnualCoverageLimit { get; set; }
        public decimal CoveragePercent { get; set; } = 100m;
        public bool CoversDependents { get; set; } = true;
        public Guid? BenefitPlanId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveMedicalPlanDtoValidator : AbstractValidator<SaveMedicalPlanDto>
    {
        public SaveMedicalPlanDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CoveragePercent).InclusiveBetween(0, 100);
            RuleFor(x => x.AnnualCoverageLimit).GreaterThanOrEqualTo(0).When(x => x.AnnualCoverageLimit.HasValue);
        }
    }

    public interface ISaveMedicalPlan { Task<Guid> SaveAsync(SaveMedicalPlanDto dto); }
    public interface IDeleteMedicalPlan { Task DeleteAsync(Guid id); }
    public interface IGetMedicalPlanById { Task<MedicalPlanDto> GetAsync(Guid id); }
    public interface IGetAllMedicalPlans { Task<PaginatedResponse<MedicalPlanDto>> GetAsync(GetAllRequest request); }

    public class SaveMedicalPlan(
        IRepository<MedicalPlan> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveMedicalPlanDto> validator) : ISaveMedicalPlan
    {
        public async Task<Guid> SaveAsync(SaveMedicalPlanDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage medical plans.");

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(MedicalPlan), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(MedicalPlan), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Description, dto.AnnualCoverageLimit, dto.CoveragePercent, dto.CoversDependents, dto.BenefitPlanId, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }
            var created = MedicalPlan.Create(dto.Name, dto.Description, dto.AnnualCoverageLimit, dto.CoveragePercent, dto.CoversDependents, dto.BenefitPlanId, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeleteMedicalPlan(
        IRepository<MedicalPlan> repository,
        IPerformanceVisibilityService visibility) : IDeleteMedicalPlan
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage medical plans.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(MedicalPlan), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetMedicalPlanById(IRepository<MedicalPlan> repository) : IGetMedicalPlanById
    {
        public async Task<MedicalPlanDto> GetAsync(Guid id) =>
            await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(p => new MedicalPlanDto { Id = p.Id, Name = p.Name, Description = p.Description, AnnualCoverageLimit = p.AnnualCoverageLimit, CoveragePercent = p.CoveragePercent, CoversDependents = p.CoversDependents, BenefitPlanId = p.BenefitPlanId, IsActive = p.IsActive })
                .FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(MedicalPlan), id.ToString());
    }

    public class GetAllMedicalPlans(IRepository<MedicalPlan> repository) : IGetAllMedicalPlans
    {
        public async Task<PaginatedResponse<MedicalPlanDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take)
                .Select(p => new MedicalPlanDto { Id = p.Id, Name = p.Name, Description = p.Description, AnnualCoverageLimit = p.AnnualCoverageLimit, CoveragePercent = p.CoveragePercent, CoversDependents = p.CoversDependents, BenefitPlanId = p.BenefitPlanId, IsActive = p.IsActive })
                .ToListAsync();
            return new PaginatedResponse<MedicalPlanDto> { Total = total, Data = data };
        }
    }

    // ================= Medical Service Contract (HC236) =================
    public class MedicalContractDto
    {
        public Guid Id { get; set; }
        public Guid MedicalProviderId { get; set; }
        public string? ProviderName { get; set; }
        public string? ContractNumber { get; set; }
        public string? Terms { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? CreditLimit { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class SaveMedicalContractDto
    {
        public Guid? Id { get; set; }
        public Guid MedicalProviderId { get; set; }
        public string? ContractNumber { get; set; }
        public string? Terms { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? CreditLimit { get; set; }
        public string Status { get; set; } = nameof(MedicalContractStatus.Active);
        public string? Notes { get; set; }
    }

    public class SaveMedicalContractDtoValidator : AbstractValidator<SaveMedicalContractDto>
    {
        public SaveMedicalContractDtoValidator()
        {
            RuleFor(x => x.MedicalProviderId).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.Status).Must(v => Enum.TryParse<MedicalContractStatus>(v, true, out _))
                .WithMessage("Status must be Active, Expired or Terminated.");
        }
    }

    public interface ISaveMedicalContract { Task<Guid> SaveAsync(SaveMedicalContractDto dto); }
    public interface IDeleteMedicalContract { Task DeleteAsync(Guid id); }
    public interface IGetMedicalContractById { Task<MedicalContractDto> GetAsync(Guid id); }
    public interface IGetAllMedicalContracts { Task<PaginatedResponse<MedicalContractDto>> GetAsync(GetAllRequest request); }

    public class GetMedicalContractById(
        IRepository<MedicalServiceContract> repository,
        IRepository<MedicalProvider> providerRepository) : IGetMedicalContractById
    {
        public async Task<MedicalContractDto> GetAsync(Guid id)
        {
            var providers = providerRepository.GetAll();
            return await repository.GetAll().AsNoTracking().Where(c => c.Id == id)
                .Select(c => new MedicalContractDto
                {
                    Id = c.Id, MedicalProviderId = c.MedicalProviderId,
                    ProviderName = providers.Where(p => p.Id == c.MedicalProviderId).Select(p => p.Name).FirstOrDefault(),
                    ContractNumber = c.ContractNumber, Terms = c.Terms, StartDate = c.StartDate,
                    RenewalDate = c.RenewalDate, EndDate = c.EndDate, CreditLimit = c.CreditLimit,
                    Status = c.Status.ToString(), Notes = c.Notes
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(MedicalServiceContract), id.ToString());
        }
    }

    public class SaveMedicalContract(
        IRepository<MedicalServiceContract> repository,
        IRepository<MedicalProvider> providerRepository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveMedicalContractDto> validator) : ISaveMedicalContract
    {
        public async Task<Guid> SaveAsync(SaveMedicalContractDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage medical contracts.");
            if (!await providerRepository.GetAll().AnyAsync(p => p.Id == dto.MedicalProviderId))
                throw new NotFoundException(nameof(MedicalProvider), dto.MedicalProviderId.ToString());

            var status = Enum.Parse<MedicalContractStatus>(dto.Status, true);
            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(MedicalServiceContract), dto.Id.Value.ToString());
                entity.Update(dto.MedicalProviderId, dto.ContractNumber, dto.Terms, dto.StartDate, dto.RenewalDate, dto.EndDate, dto.CreditLimit, status, dto.Notes);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }
            var created = MedicalServiceContract.Create(dto.MedicalProviderId, dto.ContractNumber, dto.Terms, dto.StartDate, dto.RenewalDate, dto.EndDate, dto.CreditLimit, status, dto.Notes);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            return created.Id;
        }
    }

    public class DeleteMedicalContract(
        IRepository<MedicalServiceContract> repository,
        IPerformanceVisibilityService visibility) : IDeleteMedicalContract
    {
        public async Task DeleteAsync(Guid id)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can manage medical contracts.");
            var entity = await repository.GetByIdAsync(id) ?? throw new NotFoundException(nameof(MedicalServiceContract), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetAllMedicalContracts(
        IRepository<MedicalServiceContract> repository,
        IRepository<MedicalProvider> providerRepository) : IGetAllMedicalContracts
    {
        public async Task<PaginatedResponse<MedicalContractDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;
            var providers = providerRepository.GetAll();
            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<MedicalContractStatus>(request.Status, true, out var st))
                query = query.Where(x => x.Status == st);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.StartDate).Skip(skip).Take(take)
                .Select(c => new MedicalContractDto
                {
                    Id = c.Id, MedicalProviderId = c.MedicalProviderId,
                    ProviderName = providers.Where(p => p.Id == c.MedicalProviderId).Select(p => p.Name).FirstOrDefault(),
                    ContractNumber = c.ContractNumber, Terms = c.Terms, StartDate = c.StartDate,
                    RenewalDate = c.RenewalDate, EndDate = c.EndDate, CreditLimit = c.CreditLimit,
                    Status = c.Status.ToString(), Notes = c.Notes
                }).ToListAsync();
            return new PaginatedResponse<MedicalContractDto> { Total = total, Data = data };
        }
    }
}
