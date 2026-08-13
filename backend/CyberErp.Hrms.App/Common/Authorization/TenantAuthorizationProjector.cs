using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// Keeps the tenant-scoped authorization tables in step with the global templates they are
    /// projected from.
    ///
    /// <para><b>Why this exists.</b> Since the SRMS phase-2 flip the runtime READS authorization from
    /// <c>TenantUser / TenantUserRole / TenantRolePermission / TenantOperation</c>, but the admin
    /// screens still EDIT the global <c>Role / Operation / RolePermission / UserRole</c> tables. Without
    /// a projection in between, saving a permission would update a table nobody reads and the change
    /// would appear to do nothing — a silent, severe regression. Every admin write therefore calls
    /// <see cref="SyncAsync"/> before returning.</para>
    ///
    /// <para><b>Why a full reconcile rather than a surgical per-row update.</b> One tenant has ~150
    /// operations, 8 roles and ~600 grants; reconciling the lot costs a handful of set comparisons on
    /// an operation a human performs a few times a day. In exchange it is <i>self-healing</i>: a write
    /// path that forgets to call it is corrected by the next sync, which a surgical projection could
    /// never do. The one thing it cannot see is a change made directly in SQL — that needs a manual
    /// run of <c>seed-tenant-authorization.sql</c> (which is idempotent and does the same job).</para>
    ///
    /// <para>Scoped to the CURRENT tenant only. <c>IRepository.GetAll()</c> already filters by the
    /// Finbuckle discriminator, so both sides of every comparison are this tenant's rows.</para>
    /// </summary>
    public interface ITenantAuthorizationProjector
    {
        /// <summary>
        /// Reconciles the current tenant's authorization tables with the global templates.
        /// Idempotent. Returns the number of rows written (0 when everything already matched).
        /// </summary>
        Task<int> SyncAsync(CancellationToken ct = default);
    }

    public class TenantAuthorizationProjector(
        ICurrentTenantService currentTenant,
        IRepository<Role> roles,
        IRepository<Operation> operations,
        IRepository<Module> modules,
        IRepository<RolePermission> rolePermissions,
        IRepository<UserRole> userRoles,
        IRepository<Subsystem> subsystems,
        IRepository<TenantRole> tenantRoles,
        IRepository<TenantOperation> tenantOperations,
        IRepository<TenantRolePermission> tenantRolePermissions,
        IRepository<TenantUser> tenantUsers,
        IRepository<TenantUserRole> tenantUserRoles,
        IRepository<TenantSubSystem> tenantSubSystems,
        IUnitOfWork unitOfWork,
        IEndpointPermissionService permissions,
        ILogger<TenantAuthorizationProjector> logger) : ITenantAuthorizationProjector
    {
        public async Task<int> SyncAsync(CancellationToken ct = default)
        {
            var tenantId = currentTenant.GetCurrentTenantId();
            if (tenantId is null || tenantId == Guid.Empty)
            {
                // No tenant context (a background job, or a request before resolution). Projecting
                // would either write rows with an empty FK or leak one tenant's grants into another,
                // so do nothing and say so.
                logger.LogWarning("Tenant authorization sync skipped: no tenant in context.");
                return 0;
            }

            var written = 0;
            written += await SyncRolesAsync(tenantId.Value, ct);
            written += await SyncOperationsAsync(tenantId.Value, ct);
            // Permissions and memberships reference the rows created above, so they must be persisted
            // before those joins are resolved.
            if (written > 0) await unitOfWork.SaveChangesAsync();

            written += await SyncPermissionsAsync(ct);
            written += await SyncMembershipsAsync(tenantId.Value, ct);
            written += await SyncSubsystemsAsync(tenantId.Value, ct);
            if (written > 0) await unitOfWork.SaveChangesAsync();

            if (written > 0)
            {
                // The granted-link set is cached for 60s per user. A permission change the admin can
                // still see on screen must not sit behind that window.
                permissions.InvalidateAll();
                logger.LogInformation("Tenant authorization sync wrote {Count} rows for tenant {TenantId}",
                    written, tenantId.Value);
            }
            return written;
        }

        /// <summary>Role -> TenantRole, keyed by SourceTemplateId.</summary>
        private async Task<int> SyncRolesAsync(Guid tenantId, CancellationToken ct)
        {
            var templates = await roles.GetAll().ToListAsync(ct);
            var existing = await tenantRoles.GetAll().ToListAsync(ct);
            var written = 0;

            foreach (var template in templates)
            {
                var row = existing.FirstOrDefault(r => r.SourceTemplateId == template.Id);
                if (row is null)
                {
                    await tenantRoles.AddAsync(TenantRole.Create(
                        tenantId, template.Code ?? template.Name, template.Name, template.Id));
                    written++;
                }
                else if (row.SyncFromTemplate(template.Name, template.Code, null))
                {
                    tenantRoles.UpdateAsync(row);
                    written++;
                }
            }

            // A template that no longer exists leaves an instance granting access to nothing anyone can
            // see; drop it so the two models stay comparable.
            var templateIds = templates.Select(t => t.Id).ToHashSet();
            var orphans = existing
                .Where(r => r.SourceTemplateId.HasValue && !templateIds.Contains(r.SourceTemplateId.Value))
                .ToList();
            foreach (var orphan in orphans) { tenantRoles.Delete(orphan); written++; }

            return written;
        }

        /// <summary>Operation -> TenantOperation, keyed by OperationId. A copy, not a reference.</summary>
        private async Task<int> SyncOperationsAsync(Guid tenantId, CancellationToken ct)
        {
            var templates = await operations.GetAll().ToListAsync(ct);
            var existing = await tenantOperations.GetAll().ToListAsync(ct);
            var moduleSubsystems = await modules.GetAll()
                .Select(m => new { m.Id, m.SubsystemId })
                .ToDictionaryAsync(m => m.Id, m => m.SubsystemId, ct);
            var written = 0;

            foreach (var template in templates)
            {
                // CERP's Operation names its ordering column SortOrder and has no IsActive, so
                // DisplayOrder maps from SortOrder and a projected operation is always active.
                var subSystemId = moduleSubsystems.TryGetValue(template.ModuleId, out var ssId)
                    ? ssId : Guid.Empty;

                var row = existing.FirstOrDefault(o => o.OperationId == template.Id);
                if (row is null)
                {
                    await tenantOperations.AddAsync(TenantOperation.Create(
                        tenantId, subSystemId, template.Id, template.ModuleId,
                        template.Name ?? string.Empty, template.Link ?? string.Empty, template.Icon,
                        template.SortOrder, isActive: true));
                    written++;
                }
                else if (row.SyncFromTemplate(subSystemId, template.ModuleId, template.Name ?? string.Empty,
                    template.Link ?? string.Empty, template.Icon, template.SortOrder, template.Filter))
                {
                    tenantOperations.UpdateAsync(row);
                    written++;
                }
            }

            var templateIds = templates.Select(t => t.Id).ToHashSet();
            var orphans = existing.Where(o => !templateIds.Contains(o.OperationId)).ToList();
            foreach (var orphan in orphans)
            {
                // Grants pointing at it must go first — the FK is NoAction, not cascade.
                var grants = await tenantRolePermissions.GetAll()
                    .Where(p => p.TenantOperationId == orphan.Id).ToListAsync(ct);
                foreach (var grant in grants) { tenantRolePermissions.Delete(grant); written++; }
                tenantOperations.Delete(orphan);
                written++;
            }

            return written;
        }

        /// <summary>RolePermission -> TenantRolePermission, resolved through the template ids.</summary>
        private async Task<int> SyncPermissionsAsync(CancellationToken ct)
        {
            var roleMap = await tenantRoles.GetAll()
                .Where(r => r.SourceTemplateId != null)
                .ToDictionaryAsync(r => r.SourceTemplateId!.Value, r => r.Id, ct);
            var operationMap = await tenantOperations.GetAll()
                .ToDictionaryAsync(o => o.OperationId, o => o.Id, ct);

            var templates = await rolePermissions.GetAll().ToListAsync(ct);
            var existing = await tenantRolePermissions.GetAll().ToListAsync(ct);
            var written = 0;
            var wanted = new HashSet<(Guid, Guid)>();

            // Only roles that came FROM a template are reconciled. A bespoke tenant role has no
            // counterpart in the global tables, so "not in the templates" would read as "revoked" and
            // the projection would quietly strip its grants on the next admin save.
            var projectedRoleIds = roleMap.Values.ToHashSet();

            foreach (var template in templates)
            {
                if (!roleMap.TryGetValue(template.RoleId, out var tenantRoleId)) continue;
                if (!operationMap.TryGetValue(template.OperationId, out var tenantOperationId)) continue;
                wanted.Add((tenantRoleId, tenantOperationId));

                var row = existing.FirstOrDefault(p =>
                    p.TenantRoleId == tenantRoleId && p.TenantOperationId == tenantOperationId);
                if (row is null)
                {
                    // CanExport has no counterpart in the global model, so a projected grant never
                    // carries it: inventing access nobody assigned is worse than withholding a new one.
                    await tenantRolePermissions.AddAsync(TenantRolePermission.Create(
                        tenantRoleId, tenantOperationId, template.CanView, template.CanAdd,
                        template.CanEdit, template.CanDelete, template.CanApprove));
                    written++;
                }
                else if (row.CanView != template.CanView || row.CanAdd != template.CanAdd
                    || row.CanEdit != template.CanEdit || row.CanDelete != template.CanDelete
                    || row.CanApprove != template.CanApprove)
                {
                    row.Set(template.CanView, template.CanAdd, template.CanEdit, template.CanDelete,
                        template.CanApprove, row.CanExport);
                    tenantRolePermissions.UpdateAsync(row);
                    written++;
                }
            }

            // A revoked grant has to be REMOVED, not just left behind — otherwise revoking access
            // through the admin screen would have no effect at all.
            var revoked = existing
                .Where(p => projectedRoleIds.Contains(p.TenantRoleId))
                .Where(p => !wanted.Contains((p.TenantRoleId, p.TenantOperationId)))
                .ToList();
            foreach (var row in revoked) { tenantRolePermissions.Delete(row); written++; }

            return written;
        }

        /// <summary>UserRole -> TenantUser (membership) + TenantUserRole (the roles held in it).</summary>
        private async Task<int> SyncMembershipsAsync(Guid tenantId, CancellationToken ct)
        {
            var roleMap = await tenantRoles.GetAll()
                .Where(r => r.SourceTemplateId != null)
                .ToDictionaryAsync(r => r.SourceTemplateId!.Value, r => r.Id, ct);

            var assignments = await userRoles.GetAll().ToListAsync(ct);
            var members = await tenantUsers.GetAll().ToListAsync(ct);
            var held = await tenantUserRoles.GetAll().ToListAsync(ct);
            var written = 0;

            // 1. Membership rows for everyone holding a role in this tenant.
            foreach (var userId in assignments.Select(a => a.UserId).Distinct())
            {
                if (members.Any(m => m.UserId == userId)) continue;
                var member = TenantUser.Create(tenantId, userId);
                await tenantUsers.AddAsync(member);
                members.Add(member);
                written++;
            }
            if (written > 0) await unitOfWork.SaveChangesAsync();

            // 2. The roles each member holds.
            var wanted = new HashSet<(Guid, Guid)>();
            foreach (var assignment in assignments)
            {
                if (!roleMap.TryGetValue(assignment.RoleId, out var tenantRoleId)) continue;
                var member = members.FirstOrDefault(m => m.UserId == assignment.UserId);
                if (member is null) continue;
                wanted.Add((member.Id, tenantRoleId));

                if (held.Any(h => h.TenantUserId == member.Id && h.TenantRoleId == tenantRoleId)) continue;
                await tenantUserRoles.AddAsync(TenantUserRole.Create(member.Id, tenantRoleId, "projection"));
                written++;
            }

            // Same guard as the grants: only template-backed roles are reconciled, so a bespoke role
            // held by a user survives a projection instead of looking like a revoked assignment.
            var projectedRoleIds = roleMap.Values.ToHashSet();
            var revoked = held
                .Where(h => projectedRoleIds.Contains(h.TenantRoleId))
                .Where(h => !wanted.Contains((h.TenantUserId, h.TenantRoleId)))
                .ToList();
            foreach (var row in revoked) { tenantUserRoles.Delete(row); written++; }

            // 3. A membership with no roles left grants nothing; drop it so the tables stay honest.
            var stillHolding = wanted.Select(w => w.Item1)
                .Concat(held.Except(revoked).Select(h => h.TenantUserId))
                .ToHashSet();
            foreach (var member in members.Where(m => !stillHolding.Contains(m.Id)).ToList())
            {
                tenantUsers.Delete(member);
                written++;
            }

            return written;
        }

        /// <summary>Every subsystem the tenant can reach today stays reachable.</summary>
        private async Task<int> SyncSubsystemsAsync(Guid tenantId, CancellationToken ct)
        {
            var all = await subsystems.GetAll().Select(s => s.Id).ToListAsync(ct);
            var existing = await tenantSubSystems.GetAll().Select(s => s.SubSystemId).ToListAsync(ct);
            var written = 0;

            foreach (var subsystemId in all.Except(existing))
            {
                await tenantSubSystems.AddAsync(TenantSubSystem.Create(
                    tenantId, subsystemId, TenantSubSystemSources.Plan, SubscriptionStatuses.Active,
                    DateTime.UtcNow.Date));
                written++;
            }
            return written;
        }
    }
}
