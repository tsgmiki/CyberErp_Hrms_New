using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class CompanyAssetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? SerialNo { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? AssignedToEmployeeId { get; set; }
        public string? AssignedToName { get; set; }
        public string? AssignedToNumber { get; set; }
        public DateTime? AssignedOn { get; set; }
    }

    public class SaveCompanyAssetDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>ITEquipment | AccessCard | Key | Vehicle | Tool | Other.</summary>
        public string Category { get; set; } = nameof(AssetCategory.ITEquipment);
        public string? SerialNo { get; set; }
        public string? Description { get; set; }
    }

    public class AssetRecoveryDto
    {
        public Guid Id { get; set; }
        public Guid TerminationId { get; set; }
        public Guid CompanyAssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? SerialNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ResolvedOn { get; set; }
        public string? Note { get; set; }
    }

    public class ResolveAssetRecoveryDto
    {
        /// <summary>Recover (asset returns to the pool) | Waive (asset written off).</summary>
        public string Action { get; set; } = "Recover";
        public string? Note { get; set; }
    }

    public class SaveCompanyAssetDtoValidator : AbstractValidator<SaveCompanyAssetDto>
    {
        public SaveCompanyAssetDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Category)
                .Must(v => Enum.TryParse<AssetCategory>(v, true, out _))
                .WithMessage("Category must be ITEquipment, AccessCard, Key, Vehicle, Tool or Other.");
            RuleFor(x => x.SerialNo).MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveCompanyAsset { Task<Guid> SaveAsync(SaveCompanyAssetDto dto); }
    public interface IDeleteCompanyAsset { Task DeleteAsync(Guid id); }
    public interface IAssignCompanyAsset { Task AssignAsync(Guid id, Guid employeeId); }
    public interface IReturnCompanyAsset { Task ReturnAsync(Guid id); }
    public interface IGetAllCompanyAssets { Task<PaginatedResponse<CompanyAssetDto>> GetAsync(GetAllRequest request); }
    /// <summary>The exit case's recovery checklist (HC215).</summary>
    public interface IGetAssetRecoveries { Task<List<AssetRecoveryDto>> GetAsync(Guid terminationId); }
    public interface IResolveAssetRecovery { Task ResolveAsync(Guid id, ResolveAssetRecoveryDto dto); }

    /// <summary>
    /// HC215 — generates the recovery checklist from the employee's CURRENT asset assignments the
    /// moment the case enters clearance (idempotent: an existing checklist is left untouched).
    /// </summary>
    public static class AssetRecoveryShared
    {
        public static async Task GenerateChecklistAsync(
            Guid terminationId, Guid employeeId, string tenantId,
            IRepository<CompanyAsset> assetRepository,
            IRepository<TerminationAssetRecovery> recoveryRepository)
        {
            if (await recoveryRepository.GetAll().AnyAsync(r => r.TerminationId == terminationId))
                return;

            var assigned = await assetRepository.GetAll().AsNoTracking()
                .Where(a => a.AssignedToEmployeeId == employeeId && a.Status == AssetStatus.Assigned)
                .ToListAsync();
            foreach (var asset in assigned)
            {
                var line = TerminationAssetRecovery.Create(terminationId, asset);
                if (string.IsNullOrEmpty(line.TenantId)) line.TenantId = tenantId;
                await recoveryRepository.AddAsync(line);
            }
        }
    }

    internal static class CompanyAssetShared
    {
        internal static async Task EnsureAdminAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR administrators can manage company assets.");
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveCompanyAsset(
        IRepository<CompanyAsset> repository,
        IPerformanceVisibilityService visibility,
        IValidator<SaveCompanyAssetDto> validator,
        ILogger<SaveCompanyAsset> logger) : ISaveCompanyAsset
    {
        public async Task<Guid> SaveAsync(SaveCompanyAssetDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            await CompanyAssetShared.EnsureAdminAsync(visibility);

            var category = Enum.Parse<AssetCategory>(dto.Category, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(CompanyAsset), dto.Id.Value.ToString());
                entity.Update(dto.Name, category, dto.SerialNo, dto.Description);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated CompanyAsset {Id}", entity.Id);
                return entity.Id;
            }

            var created = CompanyAsset.Create(dto.Name, category, dto.SerialNo, dto.Description);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created CompanyAsset {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteCompanyAsset(
        IRepository<CompanyAsset> repository,
        IRepository<TerminationAssetRecovery> recoveryRepository,
        IPerformanceVisibilityService visibility,
        ILogger<DeleteCompanyAsset> logger) : IDeleteCompanyAsset
    {
        public async Task DeleteAsync(Guid id)
        {
            await CompanyAssetShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(CompanyAsset), id.ToString());
            if (entity.Status == AssetStatus.Assigned)
                throw new ValidationException(nameof(id), "Return the asset before deleting it.");
            if (await recoveryRepository.GetAll().AnyAsync(r => r.CompanyAssetId == id))
                throw new ValidationException(nameof(id), "The asset appears on exit checklists — retire it instead.");
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted CompanyAsset {Id}", id);
        }
    }

    public class AssignCompanyAsset(
        IRepository<CompanyAsset> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        ILogger<AssignCompanyAsset> logger) : IAssignCompanyAsset
    {
        public async Task AssignAsync(Guid id, Guid employeeId)
        {
            await CompanyAssetShared.EnsureAdminAsync(visibility);
            var employee = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => e.Id == employeeId).Select(e => new { e.EmploymentStatus }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());
            if (employee.EmploymentStatus == EmploymentStatus.Terminated)
                throw new ValidationException(nameof(employeeId), "Assets cannot be assigned to a terminated employee.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(CompanyAsset), id.ToString());
            if (entity.Status != AssetStatus.Available)
                throw new ValidationException(nameof(id), $"Only an available asset can be assigned (current: {entity.Status}).");

            entity.AssignTo(employeeId, DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Assigned CompanyAsset {Id} to employee {Employee}", id, employeeId);
        }
    }

    public class ReturnCompanyAsset(
        IRepository<CompanyAsset> repository,
        IPerformanceVisibilityService visibility,
        ILogger<ReturnCompanyAsset> logger) : IReturnCompanyAsset
    {
        public async Task ReturnAsync(Guid id)
        {
            await CompanyAssetShared.EnsureAdminAsync(visibility);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(CompanyAsset), id.ToString());
            if (entity.Status != AssetStatus.Assigned)
                throw new ValidationException(nameof(id), $"Only an assigned asset can be returned (current: {entity.Status}).");
            entity.Return();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Returned CompanyAsset {Id}", id);
        }
    }

    public class GetAllCompanyAssets(
        IRepository<CompanyAsset> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllCompanyAssets
    {
        public async Task<PaginatedResponse<CompanyAssetDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // HR manages the registry; anyone else sees only what is assigned to THEM.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                query = query.Where(x => x.AssignedToEmployeeId == myEmp);
            }

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<AssetStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.AssignedToEmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || (x.SerialNo != null && x.SerialNo.Contains(term)));
            }

            var employees = employeeRepository.GetAll();
            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take)
                .Select(x => new CompanyAssetDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Category = x.Category.ToString(),
                    SerialNo = x.SerialNo,
                    Description = x.Description,
                    Status = x.Status.ToString(),
                    AssignedToEmployeeId = x.AssignedToEmployeeId,
                    AssignedToName = employees.Where(e => e.Id == x.AssignedToEmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber)
                        .FirstOrDefault(),
                    AssignedToNumber = employees.Where(e => e.Id == x.AssignedToEmployeeId)
                        .Select(e => e.EmployeeNumber).FirstOrDefault(),
                    AssignedOn = x.AssignedOn
                }).ToListAsync();

            return new PaginatedResponse<CompanyAssetDto> { Total = total, Data = data };
        }
    }

    public class GetAssetRecoveries(
        IRepository<TerminationAssetRecovery> repository,
        IRepository<EmployeeTermination> terminationRepository,
        IPerformanceVisibilityService visibility) : IGetAssetRecoveries
    {
        public async Task<List<AssetRecoveryDto>> GetAsync(Guid terminationId)
        {
            var employeeId = await terminationRepository.GetAll().AsNoTracking()
                .Where(t => t.Id == terminationId).Select(t => (Guid?)t.EmployeeId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(EmployeeTermination), terminationId.ToString());

            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && !await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(terminationId), "You do not have access to this exit case.");

            return await repository.GetAll().AsNoTracking()
                .Where(x => x.TerminationId == terminationId)
                .OrderBy(x => x.AssetName)
                .Select(x => new AssetRecoveryDto
                {
                    Id = x.Id,
                    TerminationId = x.TerminationId,
                    CompanyAssetId = x.CompanyAssetId,
                    AssetName = x.AssetName,
                    Category = x.Category,
                    SerialNo = x.SerialNo,
                    Status = x.Status.ToString(),
                    ResolvedOn = x.ResolvedOn,
                    Note = x.Note
                }).ToListAsync();
        }
    }

    public class ResolveAssetRecovery(
        IRepository<TerminationAssetRecovery> repository,
        IRepository<CompanyAsset> assetRepository,
        IPerformanceVisibilityService visibility,
        ILogger<ResolveAssetRecovery> logger) : IResolveAssetRecovery
    {
        public async Task ResolveAsync(Guid id, ResolveAssetRecoveryDto dto)
        {
            await CompanyAssetShared.EnsureAdminAsync(visibility);
            if (dto.Note?.Length > 500)
                throw new ValidationException(nameof(dto.Note), "A note is at most 500 characters.");

            var line = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(TerminationAssetRecovery), id.ToString());
            if (line.Status != AssetRecoveryStatus.Outstanding)
                throw new ValidationException(nameof(id), $"The item is already {line.Status}.");

            var asset = await assetRepository.GetAll().FirstOrDefaultAsync(a => a.Id == line.CompanyAssetId);

            if (dto.Action.Equals("Recover", StringComparison.OrdinalIgnoreCase))
            {
                line.MarkRecovered(DateTime.UtcNow.Date, dto.Note);
                // The recovered item goes back into the pool (HC215).
                if (asset is { Status: AssetStatus.Assigned })
                {
                    asset.Return();
                    assetRepository.UpdateAsync(asset);
                }
            }
            else if (dto.Action.Equals("Waive", StringComparison.OrdinalIgnoreCase))
            {
                line.Waive(DateTime.UtcNow.Date, dto.Note);
                // A waived item is written off — never returns to the pool.
                if (asset is not null)
                {
                    asset.Retire();
                    assetRepository.UpdateAsync(asset);
                }
            }
            else
            {
                throw new ValidationException(nameof(dto.Action), "Action must be Recover or Waive.");
            }

            repository.UpdateAsync(line);
            await repository.SaveChangesAsync();
            logger.LogInformation("Asset recovery {Id} {Action}", id, dto.Action);
        }
    }
}
