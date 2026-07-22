using System.Text.Json;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Writes an <see cref="AuditLog"/> row for every create / modify / reassign / delete of an
    /// <see cref="IAuditable"/> entity, capturing the "paper trail" for structural mutations (HC007).
    /// Runs inside the same SaveChanges transaction so the trail is atomic with the change.
    /// </summary>
    public class AuditSaveChangesInterceptor(
        ICurrentUserService currentUserService,
        ITenantService tenantService) : SaveChangesInterceptor
    {
        // Audit metadata / bookkeeping columns are not interesting business changes.
        private static readonly HashSet<string> IgnoredProperties = new()
        {
            "RowVersion", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "TenantId"
        };

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context != null) AddAuditEntries(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null) AddAuditEntries(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddAuditEntries(DbContext context)
        {
            var auditable = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAuditable &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList();

            // Nothing auditable (e.g. signup creating tenant/user) — skip before touching the
            // current-user / tenant services, which may have no context at that point.
            if (auditable.Count == 0) return;

            Guid? userId = null;
            string? userName = null;
            var currentTenantId = string.Empty;
            try
            {
                userId = currentUserService.GetCurrentUserId();
                userName = currentUserService.GetCurrentUserName();
                currentTenantId = tenantService.GetCurrentTenantId() ?? string.Empty;
            }
            catch
            {
                // Best-effort request context; still record the mutation with what we have.
            }

            var logs = new List<AuditLog>();

            foreach (var entry in auditable)
            {
                var entity = (BaseEntity)entry.Entity;
                var (action, changes) = Describe(entry);

                var branchId = entity is IBranchScoped scoped
                    ? scoped.BranchId
                    : currentUserService.GetCurrentBranchId();

                var tenantId = string.IsNullOrEmpty(entity.TenantId) ? currentTenantId : entity.TenantId;

                logs.Add(AuditLog.Record(
                    entityType: entry.Entity.GetType().Name,
                    entityId: entity.Id,
                    action: action,
                    tenantId: tenantId,
                    entityName: ReadLabel(entry),
                    changes: changes,
                    branchId: branchId,
                    performedByUserId: userId,
                    performedBy: userName));
            }

            if (logs.Count > 0)
            {
                context.Set<AuditLog>().AddRange(logs);
            }
        }

        private static (AuditAction action, string? changes) Describe(EntityEntry entry)
        {
            if (entry.State == EntityState.Added)
            {
                var snapshot = entry.Properties
                    .Where(p => !IgnoredProperties.Contains(p.Metadata.Name) && p.CurrentValue != null)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                return (AuditAction.Created, Serialize(snapshot));
            }

            if (entry.State == EntityState.Deleted)
            {
                return (AuditAction.Deleted, null);
            }

            // Modified — collect changed business fields.
            var changed = new Dictionary<string, object?>();
            var reassigned = false;
            foreach (var p in entry.Properties)
            {
                if (IgnoredProperties.Contains(p.Metadata.Name)) continue;
                if (!p.IsModified) continue;
                if (Equals(p.OriginalValue, p.CurrentValue)) continue;

                changed[p.Metadata.Name] = new { old = p.OriginalValue, @new = p.CurrentValue };
                if (p.Metadata.Name is "ParentId" or "BranchId") reassigned = true;
            }

            if (changed.Count == 0) return (AuditAction.Modified, null);
            return (reassigned ? AuditAction.Reassigned : AuditAction.Modified, Serialize(changed));
        }

        private static string? ReadLabel(EntityEntry entry)
        {
            foreach (var candidate in new[] { "Name", "Title", "FullName", "EmployeeNumber", "Code", "Institution", "Organization" })
            {
                var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == candidate);
                if (prop?.CurrentValue is string s && !string.IsNullOrEmpty(s)) return s;
            }
            return null;
        }

        private static string Serialize(object value) =>
            JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = false });
    }
}
