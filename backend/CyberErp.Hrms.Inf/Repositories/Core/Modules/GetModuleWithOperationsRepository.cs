using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.GetOperations;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Repositories.Core.Modules;

/// <summary>
/// The navigation feed, read from the TENANT-SCOPED authorization model (SRMS phase 2) and, since
/// 2026-08-13, from the SELF-REFERENCING operation hierarchy rather than Core.Module.
///
/// <para>A group is a <c>TenantOperation</c> whose <c>ModuleId</c> is null; a screen is one whose
/// <c>ModuleId</c> names that group. Both live in the same table, so one query returns the whole
/// menu. The wire contract is unchanged — the outer objects are still reported as "modules" — so
/// neither SPA needed to change.</para>
///
/// <para>Only the tenant's own copy is read: its name, link, icon and order, and only the entries it
/// has left active. <c>Operation.Id</c> is reported as the id, NOT the tenant row's, because that is
/// what the role-permission screen sends back and that screen still edits the templates.</para>
/// </summary>
public class GetModuleWithOperationsRepository(
    IRepository<Subsystem> subsystemRepository,
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
        // One query brings back the whole tree — groups and screens are the same table now.
        var operations = await tenantOperationRepository.GetAll()
            .Where(o => o.IsActive)
            .ToListAsync(ct);

        var subsystemNames = await subsystemRepository.GetAll()
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        // A group is a row with no parent; a screen is a row naming one.
        var groups = operations.Where(o => o.ModuleId == null)
            .OrderBy(o => o.DisplayOrder).ThenBy(o => o.Name);

        var result = groups
            .Select(m => new GetModuleWithOperationResult
            {
                // ⚠️ The TEMPLATE id, not the tenant row's. TenantOperation.ModuleId is copied
                // straight from the template, so it names a Core.Operation — matching it against
                // this tenant copy's own Id would never join and the menu would come back empty.
                Id = m.OperationId,
                Name = m.Name ?? string.Empty,
                SubsystemId = m.SubSystemId,
                SubSystem = subsystemNames.TryGetValue(m.SubSystemId, out var ssName) ? ssName : string.Empty,
                Icon = m.Icon,
                SortOrder = m.DisplayOrder,
                Operations = operations
                    .Where(op => op.ModuleId == m.OperationId)
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
