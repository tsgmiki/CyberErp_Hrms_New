using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Roles
{
    // ---- DTOs ---------------------------------------------------------------

    public class RolePermissionDto
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string Role { get; set; } = string.Empty;
        public Guid OperationId { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
    }

    public class RolePermissionItemDto
    {
        public Guid OperationId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
    }

    /// <summary>Bulk upsert — one save call carries a role's whole permission grid.</summary>
    public class SaveRolePermissionsDto
    {
        public Guid RoleId { get; set; }
        public List<RolePermissionItemDto> Items { get; set; } = [];
    }

    // ---- Save (bulk upsert per role) ---------------------------------------

    public interface ISaveRolePermissions { Task<int> SaveAsync(SaveRolePermissionsDto dto); }

    public class SaveRolePermissions(
        IRepository<RolePermission> repository,
        IRepository<Role> roleRepository,
        IRepository<Operation> operationRepository,
        IUnitOfWork unitOfWork,
        ILogger<SaveRolePermissions> logger) : ISaveRolePermissions
    {
        public async Task<int> SaveAsync(SaveRolePermissionsDto dto)
        {
            if (dto.RoleId == Guid.Empty)
                throw new ValidationException(nameof(dto.RoleId), "Role is required.");
            if (dto.Items.Count == 0)
                throw new ValidationException(nameof(dto.Items), "At least one permission row is required.");

            var roleExists = await roleRepository.GetAll().AnyAsync(r => r.Id == dto.RoleId);
            if (!roleExists)
                throw new ValidationException(nameof(dto.RoleId), "Role not found.");

            var operationIds = dto.Items.Select(i => i.OperationId).Distinct().ToList();
            var validOperationIds = await operationRepository.GetAll()
                .Where(o => operationIds.Contains(o.Id))
                .Select(o => o.Id)
                .ToListAsync();
            var unknown = operationIds.Except(validOperationIds).FirstOrDefault();
            if (unknown != Guid.Empty)
                throw new ValidationException(nameof(dto.Items), $"Operation {unknown} not found.");

            var existing = await repository.GetAll()
                .Where(rp => rp.RoleId == dto.RoleId && operationIds.Contains(rp.OperationId))
                .ToListAsync();

            var touched = 0;
            foreach (var item in dto.Items)
            {
                var row = existing.FirstOrDefault(rp => rp.OperationId == item.OperationId);
                if (row is null)
                {
                    await repository.AddAsync(RolePermission.Create(
                        dto.RoleId, item.OperationId,
                        item.CanAdd, item.CanEdit, item.CanDelete, item.CanApprove, item.CanView));
                }
                else
                {
                    row.UpdatePermissions(item.CanAdd, item.CanEdit, item.CanDelete, item.CanApprove, item.CanView);
                    repository.UpdateAsync(row);
                }
                touched++;
            }

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Saved {Count} permission rows for role {RoleId}", touched, dto.RoleId);
            return touched;
        }
    }

    // ---- GetAll (paged, joined names) --------------------------------------

    public interface IGetAllRolePermissions { Task<PaginatedResponse<RolePermissionDto>> GetAsync(GetAllRequest request); }

    public class GetAllRolePermissions(IRepository<RolePermission> repository) : IGetAllRolePermissions
    {
        public async Task<PaginatedResponse<RolePermissionDto>> GetAsync(GetAllRequest request)
        {
            var query = repository.GetAll();

            // Scope to a single role when the screen asks for one (the role id is sent as categoryId).
            // Without this the matrix returns EVERY role's rows, so a brand-new role appears pre-granted
            // with whatever other roles (e.g. the admin role) have configured.
            if (request.CategoryId.HasValue)
                query = query.Where(rp => rp.RoleId == request.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(rp =>
                    rp.Role.Name.Contains(request.SearchText) ||
                    rp.Operation.Name.Contains(request.SearchText) ||
                    rp.Operation.Module.Name.Contains(request.SearchText));

            var total = await query.CountAsync();

            int skip = int.TryParse(request.Skip, out var s) ? s : 0;
            int take = int.TryParse(request.Take, out var t) ? t : 15;

            var data = await query
                .OrderBy(rp => rp.Role.Name)
                .ThenBy(rp => rp.Operation.Module.SortOrder)
                .ThenBy(rp => rp.Operation.SortOrder)
                .Skip(skip).Take(take)
                .Select(rp => new RolePermissionDto
                {
                    Id = rp.Id,
                    RoleId = rp.RoleId,
                    Role = rp.Role.Name,
                    OperationId = rp.OperationId,
                    Operation = rp.Operation.Name,
                    Module = rp.Operation.Module.Name,
                    CanView = rp.CanView,
                    CanAdd = rp.CanAdd,
                    CanEdit = rp.CanEdit,
                    CanDelete = rp.CanDelete,
                    CanApprove = rp.CanApprove
                })
                .ToListAsync();

            return new PaginatedResponse<RolePermissionDto> { Total = total, Data = data };
        }
    }

    // ---- Delete -------------------------------------------------------------

    public interface IDeleteRolePermission { Task DeleteAsync(Guid id); }

    public class DeleteRolePermission(
        IRepository<RolePermission> repository,
        IUnitOfWork unitOfWork) : IDeleteRolePermission
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(rp => rp.Id == id)
                ?? throw new ValidationException(nameof(id), "Permission row not found.");
            repository.Delete(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
