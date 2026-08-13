using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.GetOperations;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Repositories.Core.Modules;

/// <summary>
/// The navigation feed, read from the TENANT-SCOPED authorization model (SRMS phase 2).
///
/// <para>Operations come from <c>TenantOperation</c> rather than the global <c>Operation</c>, so the
/// sidebar shows the tenant's own copy: its name, link, icon and order, and only the entries it has
/// left active. Modules are still global — they are the grouping, and nothing per-tenant hangs off
/// them yet. <c>Operation.Id</c> is reported as the id, NOT the tenant row's, because the id is what
/// the role-permission screen sends back and that screen still edits the templates.</para>
/// </summary>
public class GetModuleWithOperationsRepository(
    IRepository<Module> moduleRepository,
    IRepository<TenantUser> tenantUserRepository,
    IRepository<TenantUserRole> tenantUserRoleRepository,
    IRepository<TenantRolePermission> tenantRolePermissionRepository,
    IRepository<TenantOperation> tenantOperationRepository,
    ILogger<GetModuleWithOperationsRepository> logger) : IGetModuleWithOperationsRepository
{
    public async Task<IEnumerable<GetModuleWithOperationResult>> GetAsync(Guid? userId, CancellationToken ct = default)
    {
        logger.LogInformation("Getting Modules with Operations for user {UserId}", userId);

        // The caller's roles WITHIN THIS TENANT. Every query below is already tenant-filtered by the
        // Finbuckle discriminator, so a user belonging to several tenants sees only this one's menu.
        var tenantRoleIds = new List<Guid>();
        if (userId.HasValue)
        {
            tenantRoleIds = await tenantUserRepository.GetAll()
                .Where(tu => tu.UserId == userId.Value && tu.Status == TenantUserStatuses.Active)
                .Join(tenantUserRoleRepository.GetAll(),
                    tu => tu.Id, tur => tur.TenantUserId, (tu, tur) => tur.TenantRoleId)
                .Distinct()
                .ToListAsync(ct);
        }

        // Strictly role-based (deny-by-default): an operation appears ONLY when one of the user's
        // roles grants CanView on it. No branch/head-office bypass — "admin" = a role granted the
        // permissions, NOT a user who happens to have no branch (IsHeadOffice is a branch-data flag).
        var grants = await tenantRolePermissionRepository.GetAll()
            .Where(p => tenantRoleIds.Contains(p.TenantRoleId))
            .ToListAsync(ct);

        // Only active operations: hiding a screen for a tenant removes it from the menu.
        var operations = await tenantOperationRepository.GetAll()
            .Where(o => o.IsActive)
            .ToListAsync(ct);

        var modules = await moduleRepository.GetAll()
            .Include(m => m.Subsystem)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .ToListAsync(ct);

        var result = modules
            .Select(m => new GetModuleWithOperationResult
            {
                Id = m.Id,
                Name = m.Name ?? string.Empty,
                SubsystemId = m.SubsystemId,
                SubSystem = m.Subsystem?.Name ?? string.Empty,
                Icon = m.Icon,
                SortOrder = m.SortOrder,
                Operations = operations
                    .Where(op => op.ModuleId == m.Id)
                    .OrderBy(op => op.DisplayOrder).ThenBy(op => op.Name)
                    .Select(op =>
                    {
                        var permission = grants.FirstOrDefault(p => p.TenantOperationId == op.Id);
                        return new OperationRecord
                        {
                            Id = op.OperationId,           // the template id the UI already works with
                            Name = op.Name,
                            Link = op.Link,
                            Icon = op.Icon,
                            SortOrder = op.DisplayOrder,
                            CanAdd = permission?.CanAdd ?? false,
                            CanEdit = permission?.CanEdit ?? false,
                            CanDelete = permission?.CanDelete ?? false,
                            CanApprove = permission?.CanApprove ?? false,
                            CanView = permission?.CanView ?? false
                        };
                    })
                    .Where(op => op.CanView)   // hide operations the role can't view
                    .ToList()
            })
            .Where(m => m.Operations.Any())    // drop modules left with no visible operations
            .ToList();

        return result;
    }
}
