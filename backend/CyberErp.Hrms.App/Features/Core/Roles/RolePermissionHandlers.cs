using CyberErp.Hrms.App.Common.Authorization;
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

    /// <summary>
    /// Writes DIRECTLY to Core.TenantRolePermission, the table the runtime actually reads.
    ///
    /// <para>Core.RolePermission was retired on 2026-08-13. Until then this handler edited that global
    /// table and a projector copied the result across; now there is nothing in between, so a save is
    /// live the moment it commits.</para>
    ///
    /// <para>The WIRE CONTRACT is unchanged: the screen still sends a global <c>RoleId</c> and
    /// <c>OperationId</c>, which are resolved here to this tenant's instances. Everything is scoped by
    /// the Finbuckle discriminator, so a save can only ever touch the caller's own tenant.</para>
    /// </summary>
    public class SaveRolePermissions(
        IRepository<TenantRolePermission> repository,
        IRepository<TenantRole> tenantRoles,
        IRepository<TenantOperation> tenantOperations,
        IUnitOfWork unitOfWork,
        IEndpointPermissionService permissions,
        ILogger<SaveRolePermissions> logger) : ISaveRolePermissions
    {
        public async Task<int> SaveAsync(SaveRolePermissionsDto dto)
        {
            if (dto.RoleId == Guid.Empty)
                throw new ValidationException(nameof(dto.RoleId), "Role is required.");
            if (dto.Items.Count == 0)
                throw new ValidationException(nameof(dto.Items), "At least one permission row is required.");

            // The role arrives as a TEMPLATE id; find this tenant's instance of it.
            var tenantRoleId = await tenantRoles.GetAll()
                .Where(r => r.SourceTemplateId == dto.RoleId)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync()
                ?? throw new ValidationException(nameof(dto.RoleId), "Role not found.");

            var operationIds = dto.Items.Select(i => i.OperationId).Distinct().ToList();
            var operationMap = await tenantOperations.GetAll()
                .Where(o => operationIds.Contains(o.OperationId))
                .ToDictionaryAsync(o => o.OperationId, o => o.Id);

            var unknown = operationIds.Except(operationMap.Keys).FirstOrDefault();
            if (unknown != Guid.Empty)
                throw new ValidationException(nameof(dto.Items), $"Operation {unknown} not found.");

            var tenantOperationIds = operationMap.Values.ToList();
            var existing = await repository.GetAll()
                .Where(p => p.TenantRoleId == tenantRoleId && tenantOperationIds.Contains(p.TenantOperationId))
                .ToListAsync();

            var touched = 0;
            foreach (var item in dto.Items)
            {
                var tenantOperationId = operationMap[item.OperationId];
                var row = existing.FirstOrDefault(p => p.TenantOperationId == tenantOperationId);
                if (row is null)
                {
                    // CanExport has no field on this screen, so a new grant never carries it —
                    // inventing a privilege nobody ticked is worse than withholding a new one.
                    await repository.AddAsync(TenantRolePermission.Create(
                        tenantRoleId, tenantOperationId,
                        item.CanView, item.CanAdd, item.CanEdit, item.CanDelete, item.CanApprove));
                }
                else
                {
                    // ...and an edit PRESERVES whatever CanExport was already set, rather than
                    // silently clearing a privilege this screen cannot even display.
                    row.Set(item.CanView, item.CanAdd, item.CanEdit, item.CanDelete, item.CanApprove,
                        row.CanExport);
                    repository.UpdateAsync(row);
                }
                touched++;
            }

            await unitOfWork.SaveChangesAsync();
            // Granted links are cached for 60s per user; without this an admin would not see the
            // effect of their own save.
            permissions.InvalidateAll();
            logger.LogInformation("Saved {Count} permission rows for role {RoleId}", touched, dto.RoleId);
            return touched;
        }
    }

    // ---- GetAll (paged, joined names) --------------------------------------

    public interface IGetAllRolePermissions { Task<PaginatedResponse<RolePermissionDto>> GetAsync(GetAllRequest request); }

    /// <summary>
    /// Reads the grid from Core.TenantRolePermission, reporting TEMPLATE ids so the screen's save call
    /// keeps working unchanged. <c>categoryId</c> is still the global role id.
    /// </summary>
    public class GetAllRolePermissions(
        IRepository<TenantRolePermission> repository,
        IRepository<TenantRole> tenantRoles,
        IRepository<TenantOperation> tenantOperations) : IGetAllRolePermissions
    {
        public async Task<PaginatedResponse<RolePermissionDto>> GetAsync(GetAllRequest request)
        {
            var roles = tenantRoles.GetAll();
            var operations = tenantOperations.GetAll();

            var query =
                from p in repository.GetAll()
                join r in roles on p.TenantRoleId equals r.Id
                join o in operations on p.TenantOperationId equals o.Id
                select new { p, r, o };

            // Scope to a single role when the screen asks for one (the role id is sent as categoryId,
            // and it is the TEMPLATE id). Without this the matrix returns EVERY role's rows, so a
            // brand-new role appears pre-granted with whatever the admin role has configured.
            if (request.CategoryId.HasValue)
                query = query.Where(x => x.r.SourceTemplateId == request.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x =>
                    x.r.Name.Contains(request.SearchText) ||
                    x.o.Name.Contains(request.SearchText));

            var total = await query.CountAsync();

            int skip = int.TryParse(request.Skip, out var s) ? s : 0;
            int take = int.TryParse(request.Take, out var t) ? t : 15;

            var rows = await query
                .OrderBy(x => x.r.Name)
                .ThenBy(x => x.o.DisplayOrder)
                .ThenBy(x => x.o.Name)
                .Skip(skip).Take(take)
                .Select(x => new
                {
                    x.p.Id,
                    RoleTemplateId = x.r.SourceTemplateId,
                    RoleName = x.r.Name,
                    OperationTemplateId = x.o.OperationId,
                    OperationName = x.o.Name,
                    x.o.ModuleId,
                    x.p.CanView, x.p.CanAdd, x.p.CanEdit, x.p.CanDelete, x.p.CanApprove
                })
                .ToListAsync();

            // The parent group's name, resolved in one extra round trip rather than a correlated
            // subquery per row.
            var parentIds = rows.Where(r => r.ModuleId.HasValue).Select(r => r.ModuleId!.Value).Distinct().ToList();
            var parentNames = await operations
                .Where(o => parentIds.Contains(o.OperationId) && o.ModuleId == null)
                .ToDictionaryAsync(o => o.OperationId, o => o.Name);

            var data = rows.Select(x => new RolePermissionDto
            {
                Id = x.Id,
                RoleId = x.RoleTemplateId ?? Guid.Empty,
                Role = x.RoleName,
                OperationId = x.OperationTemplateId,
                Operation = x.OperationName,
                // Blank when the operation IS a group.
                Module = x.ModuleId.HasValue && parentNames.TryGetValue(x.ModuleId.Value, out var pn)
                    ? pn : string.Empty,
                CanView = x.CanView,
                CanAdd = x.CanAdd,
                CanEdit = x.CanEdit,
                CanDelete = x.CanDelete,
                CanApprove = x.CanApprove
            }).ToList();

            return new PaginatedResponse<RolePermissionDto> { Total = total, Data = data };
        }
    }

    // ---- Delete -------------------------------------------------------------

    public interface IDeleteRolePermission { Task DeleteAsync(Guid id); }

    public class DeleteRolePermission(
        IRepository<TenantRolePermission> repository,
        IUnitOfWork unitOfWork,
        IEndpointPermissionService permissions) : IDeleteRolePermission
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new ValidationException(nameof(id), "Permission row not found.");
            repository.Delete(entity);
            await unitOfWork.SaveChangesAsync();
            // The revocation is live immediately; only the cache stands between it and the caller.
            permissions.InvalidateAll();
        }
    }
}
