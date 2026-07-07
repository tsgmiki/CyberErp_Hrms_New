using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.ClearanceDepartments
{
    // ---- DTOs ---------------------------------------------------------------

    public class ClearanceApproverDto
    {
        public string ApproverType { get; set; } = string.Empty;  // User | Role
        public Guid ApproverId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class ClearanceDepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public List<ClearanceApproverDto> Approvers { get; set; } = [];
    }

    public class SaveClearanceDepartmentDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ClearanceApproverDto> Approvers { get; set; } = [];
    }

    public class SaveClearanceDepartmentDtoValidator : AbstractValidator<SaveClearanceDepartmentDto>
    {
        public SaveClearanceDepartmentDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500)
                .WithMessage("Describe what this department must clear (assets, loans, …).");
            RuleForEach(x => x.Approvers).ChildRules(a =>
            {
                a.RuleFor(x => x.ApproverType)
                    .Must(v => string.Equals(v, "User", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(v, "Role", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("ApproverType must be User or Role.");
                a.RuleFor(x => x.ApproverId).NotEmpty();
            });
        }
    }

    // ---- Interfaces -----------------------------------------------------------

    public interface ISaveClearanceDepartment { Task<Guid> SaveAsync(SaveClearanceDepartmentDto dto); }
    public interface IGetAllClearanceDepartments { Task<PaginatedResponse<ClearanceDepartmentDto>> GetAsync(GetAllRequest request); }
    public interface IGetClearanceDepartmentById { Task<ClearanceDepartmentDto> GetAsync(Guid id); }
    public interface IDeleteClearanceDepartment { Task DeleteAsync(Guid id); }

    internal static class ClearanceDepartmentShared
    {
        internal static ClearanceDepartmentDto ToDto(ClearanceDepartment d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            SortOrder = d.SortOrder,
            IsActive = d.IsActive,
            Approvers = d.Approvers
                .Select(a => new ClearanceApproverDto
                {
                    ApproverType = a.ApproverType.ToString(),
                    ApproverId = a.ApproverId,
                    DisplayName = a.DisplayName
                })
                .ToList()
        };

        /// <summary>The repository stamps only aggregate roots — cascade-inserted children copy it here.</summary>
        internal static void StampApproverTenant(ClearanceDepartment department)
        {
            foreach (var approver in department.Approvers)
                if (string.IsNullOrEmpty(approver.TenantId))
                    approver.TenantId = department.TenantId;
        }
    }

    // ---- Save (create / update, with approver set) ------------------------------

    public class SaveClearanceDepartment(
        IRepository<ClearanceDepartment> repository,
        IRepository<ClearanceDepartmentApprover> approverRepository,
        IRepository<Dom.Entities.Core.User> userRepository,
        IRepository<Role> roleRepository,
        IValidator<SaveClearanceDepartmentDto> validator,
        ILogger<SaveClearanceDepartment> logger) : ISaveClearanceDepartment
    {
        public async Task<Guid> SaveAsync(SaveClearanceDepartmentDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(d => d.Name == dto.Name && d.Id != dto.Id))
                throw new DuplicateException(nameof(ClearanceDepartment), nameof(dto.Name), dto.Name);

            var approvers = await BuildApproverSpecsAsync(dto);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll()
                        .Include(d => d.Approvers)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(ClearanceDepartment), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Description, dto.SortOrder, dto.IsActive);
                entity.SetApprovers(approvers);
                ClearanceDepartmentShared.StampApproverTenant(entity);
                // Replacement approvers are new rows: mark them Added explicitly, otherwise
                // context.Update(root) treats the app-generated keys as existing (Modified).
                foreach (var approver in entity.Approvers)
                    await approverRepository.AddAsync(approver);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated ClearanceDepartment {Id}", entity.Id);
                return entity.Id;
            }

            var created = ClearanceDepartment.Create(dto.Name, dto.Description, dto.SortOrder, dto.IsActive);
            created.SetApprovers(approvers);
            await repository.AddAsync(created);   // stamps the root's TenantId
            ClearanceDepartmentShared.StampApproverTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created ClearanceDepartment {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }

        /// <summary>
        /// Resolves approver display names server-side (never trusting client text) and validates
        /// that every referenced user / role exists in the tenant.
        /// </summary>
        private async Task<List<WorkflowApproverSpec>> BuildApproverSpecsAsync(SaveClearanceDepartmentDto dto)
        {
            var userIds = dto.Approvers
                .Where(a => string.Equals(a.ApproverType, "User", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.ApproverId).Distinct().ToList();
            var roleIds = dto.Approvers
                .Where(a => string.Equals(a.ApproverType, "Role", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.ApproverId).Distinct().ToList();

            var users = await userRepository.GetAll()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);
            var roles = await roleRepository.GetAll()
                .Where(r => roleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name);

            var specs = new List<WorkflowApproverSpec>();
            foreach (var a in dto.Approvers)
            {
                var isUser = string.Equals(a.ApproverType, "User", StringComparison.OrdinalIgnoreCase);
                if (isUser && !users.ContainsKey(a.ApproverId))
                    throw new NotFoundException("User", a.ApproverId.ToString());
                if (!isUser && !roles.ContainsKey(a.ApproverId))
                    throw new NotFoundException("Role", a.ApproverId.ToString());

                specs.Add(new WorkflowApproverSpec(
                    isUser ? WorkflowApproverType.User : WorkflowApproverType.Role,
                    a.ApproverId,
                    isUser ? users[a.ApproverId] : roles[a.ApproverId]));
            }
            return specs;
        }
    }

    // ---- Get all (paged) --------------------------------------------------------

    public class GetAllClearanceDepartments(IRepository<ClearanceDepartment> repository) : IGetAllClearanceDepartments
    {
        public async Task<PaginatedResponse<ClearanceDepartmentDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Description.Contains(term));
            }

            var total = await query.CountAsync();
            var rows = await query
                .Include(d => d.Approvers)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .ToListAsync();

            return new PaginatedResponse<ClearanceDepartmentDto>
            {
                Total = total,
                Data = rows.Select(ClearanceDepartmentShared.ToDto).ToList()
            };
        }
    }

    // ---- Get by id ----------------------------------------------------------------

    public class GetClearanceDepartmentById(IRepository<ClearanceDepartment> repository) : IGetClearanceDepartmentById
    {
        public async Task<ClearanceDepartmentDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll()
                    .Include(d => d.Approvers)
                    .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(ClearanceDepartment), id.ToString());
            return ClearanceDepartmentShared.ToDto(entity);
        }
    }

    // ---- Delete ---------------------------------------------------------------------

    public class DeleteClearanceDepartment(
        IRepository<ClearanceDepartment> repository,
        ILogger<DeleteClearanceDepartment> logger) : IDeleteClearanceDepartment
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(ClearanceDepartment), id.ToString());

            // Existing checklist rows keep their snapshot (Department name/description) and fall
            // back to the open behaviour — their DepartmentId FK is set null by the database.
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted ClearanceDepartment {Id}", id);
        }
    }
}
