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
    IRepository<TenantModule> tenantModuleRepository,
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

        // Only active rows: hiding a screen or a group for a tenant removes it from the menu.
        // Groups moved OUT of TenantOperation into TenantModule on 2026-08-15 (SRMS parity), so this
        // is two reads again rather than one self-referencing one.
        // ⚠️ SCOPED BY GROUP. TenantOperation lost its TenantId on 2026-08-15, so an unscoped read
        // here would pull every tenant's screens into memory before the join discarded them.
        var groupIds = await tenantModuleRepository.GetAll().Select(m => m.Id).ToListAsync(ct);
        var operations = await tenantOperationRepository.GetAll()
            .Where(o => o.IsActive && groupIds.Contains(o.ModuleId))
            .ToListAsync(ct);

        var groups = await tenantModuleRepository.GetAll()
            .Where(m => m.IsActive)
            .ToListAsync(ct);

        // Name AND abbreviation. The sidebar scopes on the ABBREVIATION because the NAME is a
        // display label an administrator can rename at will — and did: "HRMS" became
        // "Human Resource Management System", which silently emptied this application's menu.
        var subsystems = await subsystemRepository.GetAll()
            .Select(s => new { s.Id, s.Name, s.Abbreviation })
            .ToDictionaryAsync(s => s.Id, s => s, ct);

        var result = groups
            .OrderBy(m => m.DisplayOrder).ThenBy(m => m.Name)
            .Select(m => new GetModuleWithOperationResult
            {
                // The TENANT group's own id. It reported the template id until 2026-08-15, when the
                // link column was dropped for SRMS parity. Nothing joins on it — both SPAs use it as
                // a sidebar-group key and match permissions by LINK.
                Id = m.Id,
                Name = m.Name ?? string.Empty,
                SubsystemId = m.SubSystemId,
                SubSystem = subsystems.TryGetValue(m.SubSystemId, out var ss) ? ss.Name : string.Empty,
                // Falls back to the name when a row has no abbreviation, so scoping still resolves.
                SubSystemAbbreviation = subsystems.TryGetValue(m.SubSystemId, out var ssa)
                    ? (string.IsNullOrWhiteSpace(ssa.Abbreviation) ? ssa.Name : ssa.Abbreviation)
                    : string.Empty,
                Icon = m.Icon,
                SortOrder = m.DisplayOrder,
                Operations = operations
                    // ⚠️ Matched on the TENANT row's Id now. TenantOperation.ModuleId names a
                    // TenantModule, not a template — the old code matched it against the template id,
                    // which is exactly the mistake that returned an empty menu in August.
                    .Where(op => op.ModuleId == m.Id)
                    .OrderBy(op => op.DisplayOrder).ThenBy(op => op.Name)
                    .Select(op =>
                    {
                        var permission = grants.FirstOrDefault(p => p.TenantOperationId == op.Id);
                        return new OperationRecord
                        {
                            // The TENANT row's own id. It used to report the template id via
                            // OperationId, which SRMS has no column for and CERP dropped on
                            // 2026-08-15. Nothing joins on it: every permission consumer in both
                            // SPAs matches on LINK (permissionGate, formPermissions, gridAction,
                            // useListPermissions), so this is a React key and nothing more.
                            Id = op.Id,
                            Name = op.Name,
                            Link = op.Link,
                            Icon = op.Icon,
                            SortOrder = op.DisplayOrder,
                            CanAdd = permission?.CanAdd ?? false,
                            CanEdit = permission?.CanEdit ?? false,
                            CanDelete = permission?.CanDelete ?? false,
                            CanApprove = permission?.CanApprove ?? false,
                            CanView = permission?.CanView ?? false,
                            CanExport = permission?.CanExport ?? false
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
