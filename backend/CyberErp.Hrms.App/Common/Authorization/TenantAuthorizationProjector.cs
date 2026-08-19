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
    /// screens still EDIT the global <c>Role / Operation / UserRole</c> tables. Without a projection in
    /// between, creating a role would leave the runtime unable to resolve it. Every admin write
    /// therefore calls <see cref="SyncAsync"/> before returning.</para>
    ///
    /// <para>⚠️ <b>PERMISSIONS ARE NOT PROJECTED.</b> <c>Core.RolePermission</c> was retired on
    /// 2026-08-13; the Role Permissions screen writes <c>TenantRolePermission</c> directly, so there is
    /// no longer anything to project them from. Do not add a sweep back — with no template table
    /// behind it, every hand-edited grant would look orphaned and be deleted.</para>
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
        IRepository<UserRole> userRoles,
        IRepository<Subsystem> subsystems,
        IRepository<TenantRole> tenantRoles,
        IRepository<TenantModule> tenantModules,
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
            // Modules BEFORE operations: a screen's ModuleId now points at the tenant's group row,
            // so the group has to exist first.
            written += await SyncModulesAsync(tenantId.Value, ct);
            written += await SyncOperationsAsync(tenantId.Value, ct);
            // Permissions and memberships reference the rows created above, so they must be persisted
            // before those joins are resolved.
            if (written > 0) await unitOfWork.SaveChangesAsync();

            // ⚠️ NEITHER PERMISSIONS NOR MEMBERSHIPS ARE PROJECTED, for the same reason twice over.
            //
            // Core.RolePermission was retired on 2026-08-13, and Core.UserRole lost its TenantId on
            // 2026-08-15 — which makes it GLOBAL. Projecting memberships from it would have created a
            // TenantUser in THIS tenant for every user of EVERY tenant, and no join exists that could
            // re-scope it: a UserRole row carries only UserId and RoleId, and both of those tables are
            // global too. Nothing writes Core.UserRole from HRMS any more either — the User Roles
            // screen went in handoff 0107 — so there was nothing left to project FROM.
            //
            // Memberships are written directly into TenantUser / TenantUserRole by SRMS. Do not add a
            // sweep back for either: with no template table behind them, every hand-written row would
            // look orphaned and be deleted.
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

            // ⚠️ UPDATES ONLY — it no longer instantiates missing templates.
            //
            // Core.Role lost its TenantId on 2026-08-13, so `templates` is now every tenant's roles.
            // Creating an instance for each would hand this tenant every other tenant's roles, which
            // is both wrong and visible (the Roles screen lists what has an instance). A role is
            // instantiated where it is CREATED instead — see SaveRole.
            foreach (var row in existing)
            {
                var template = templates.FirstOrDefault(t => t.Id == row.SourceTemplateId);
                if (template is null) continue;
                if (row.SyncFromTemplate(template.Name, template.Code, null))
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

            // ⚠️ THE CHILDREN ARE DELETED BY HAND, because the database no longer does it.
            //
            // TenantRolePermission -> TenantRole used to CASCADE, and TenantUserRole -> TenantRole
            // used to BLOCK the delete outright. SRMS dropped BOTH foreign keys on 2026-08-15 and
            // CERP followed for parity, so deleting a role now silently leaves its grants and its
            // holders' assignments behind — rows that grant nothing and point at nothing.
            foreach (var orphan in orphans)
            {
                var grants = await tenantRolePermissions.GetAll()
                    .Where(p => p.TenantRoleId == orphan.Id).ToListAsync(ct);
                foreach (var grant in grants) { tenantRolePermissions.Delete(grant); written++; }

                var assignments = await tenantUserRoles.GetAll()
                    .Where(a => a.TenantRoleId == orphan.Id).ToListAsync(ct);
                foreach (var assignment in assignments) { tenantUserRoles.Delete(assignment); written++; }

                tenantRoles.Delete(orphan);
                written++;
            }

            return written;
        }

        /// <summary>
        /// Module -> TenantModule, keyed by ModuleId. The tenant's copy of each menu GROUP.
        ///
        /// <para>Unlike roles and operations this DOES create rows. It can, because the link is
        /// unambiguous: a tenant that holds a copy of a screen must hold its group, so the set is
        /// derived rather than guessed. Without that, a screen projected into the tenant would have
        /// nowhere to hang and the sidebar would lose the whole group.</para>
        /// </summary>
        private async Task<int> SyncModulesAsync(Guid tenantId, CancellationToken ct)
        {
            var templates = await modules.GetAll().ToListAsync(ct);
            var existing = await tenantModules.GetAll().ToListAsync(ct);
            var written = 0;

            // Keyed on (subsystem, name): TenantModule carries no template link, because SRMS carries
            // none. The pair is unique in both tables — verified 0 duplicates across all 24 rows.
            var templateByKey = templates
                .ToDictionary(m => (m.SubsystemId, (m.Name ?? string.Empty).Trim()), m => m);

            foreach (var row in existing)
            {
                if (!templateByKey.TryGetValue((row.SubSystemId, row.Name.Trim()), out var template))
                    continue;

                if (row.SyncFromTemplate(template.SubsystemId, template.Name, template.Icon,
                        template.DisplayOrder, template.Filter, template.IsActive))
                {
                    tenantModules.UpdateAsync(row);
                    written++;
                }
            }

            return written;
        }

        /// <summary>
        /// Operation -> TenantOperation, keyed by (module, LINK).
        ///
        /// <para>⚠️ The key used to be <c>OperationId</c>, the template link on the copy. SRMS has no
        /// such column and CERP dropped it on 2026-08-15, so the natural key does the job: a link is
        /// unique within a group, and it is what every permission check already matches on.</para>
        ///
        /// <para>⚠️ <c>TenantOperation</c> also lost its TenantId, so <c>GetAll()</c> now spans EVERY
        /// tenant. Everything below is scoped through THIS tenant's group ids — without that the
        /// orphan sweep at the end would delete other tenants' screens.</para>
        /// </summary>
        private async Task<int> SyncOperationsAsync(Guid tenantId, CancellationToken ct)
        {
            var templates = await operations.GetAll().ToListAsync(ct);
            var written = 0;

            // Template module -> THIS tenant's copy of that group, keyed on (subsystem, name) since
            // there is no template link column any more.
            var groups = await tenantModules.GetAll().ToListAsync(ct);
            var groupByTemplate = groups
                .ToDictionary(m => (m.SubSystemId, m.Name.Trim()), m => m.Id);
            var myGroupIds = groups.Select(m => m.Id).ToHashSet();
            var moduleById = await modules.GetAll().ToDictionaryAsync(m => m.Id, m => m, ct);

            // ⚠️ SCOPED BY GROUP, not by tenant filter — the filter no longer exists on this table.
            var existing = await tenantOperations.GetAll()
                .Where(o => myGroupIds.Contains(o.ModuleId))
                .ToListAsync(ct);

            // What this tenant SHOULD have: one copy per template whose group it holds, keyed by
            // (tenant group, link).
            // ModuleId is nullable on the template (SRMS leaves the column nullable), but no code
            // path can produce a null and there are none in either database — skip defensively.
            var wanted = new Dictionary<(Guid, string), Dom.Entities.Core.Operation>();
            foreach (var t in templates)
            {
                if (!t.ModuleId.HasValue || !moduleById.TryGetValue(t.ModuleId.Value, out var mod)) continue;
                if (!groupByTemplate.TryGetValue((mod.SubsystemId, mod.Name.Trim()), out var groupId)) continue;
                wanted[(groupId, (t.Link ?? string.Empty).Trim())] = t;
            }

            foreach (var row in existing)
            {
                if (!wanted.TryGetValue((row.ModuleId, row.Link.Trim()), out var template)) continue;

                var changed = row.SyncFromTemplate(row.ModuleId,
                    template.Name ?? string.Empty, template.Link ?? string.Empty, template.Icon,
                    template.DisplayOrder, template.Filter);

                // IsActive is a template-level kill switch, and the readers filter on the TENANT
                // copy — so deactivating a screen has no effect at all unless it propagates here.
                if (row.IsActive != template.IsActive)
                {
                    row.SetActive(template.IsActive);
                    changed = true;
                }

                if (changed)
                {
                    tenantOperations.UpdateAsync(row);
                    written++;
                }
            }

            // A copy whose template is gone grants access to a screen nobody can reach. Scoped to
            // this tenant's groups above, so it can only ever remove OUR rows.
            var orphans = existing
                .Where(o => !wanted.ContainsKey((o.ModuleId, o.Link.Trim())))
                .ToList();
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

        private async Task<int> SyncSubsystemsAsync(Guid tenantId, CancellationToken ct)
        {
            var all = await subsystems.GetAll().Select(s => s.Id).ToListAsync(ct);
            var existing = await tenantSubSystems.GetAll().Select(s => s.SubSystemId).ToListAsync(ct);
            var written = 0;

            foreach (var subsystemId in all.Except(existing))
            {
                await tenantSubSystems.AddAsync(TenantSubSystem.Create(
                    tenantId, subsystemId, TenantSubSystemSources.Plan, status: true,
                    DateTime.UtcNow.Date));
                written++;
            }
            return written;
        }
    }
}
