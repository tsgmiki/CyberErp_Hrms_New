using System.Text.Json;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- Append-only version history / audit trail (HC132) ------------------

    /// <summary>
    /// Writes one immutable <see cref="PerformanceHistory"/> row per significant transition. It only
    /// enlists the row (AddAsync); the calling handler's SaveChanges commits it in the same unit of work,
    /// so the snapshot is atomic with the state change it records. Who/when are stamped by the audit
    /// interceptor (CreatedBy/CreatedAt).
    /// </summary>
    public interface IPerformanceHistoryWriter
    {
        Task WriteAsync(string entityType, Guid entityId, string action, string summary, object? snapshot = null);
    }

    public class PerformanceHistoryWriter(IRepository<PerformanceHistory> repository) : IPerformanceHistoryWriter
    {
        public async Task WriteAsync(string entityType, Guid entityId, string action, string summary, object? snapshot = null)
        {
            var json = snapshot is null ? null : JsonSerializer.Serialize(snapshot);
            var entry = PerformanceHistory.Record(entityType, entityId, action, summary, json);
            await repository.AddAsync(entry);
        }
    }

    public class PerformanceHistoryDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? SnapshotJson { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IGetPerformanceHistory { Task<List<PerformanceHistoryDto>> GetAsync(string entityType, Guid entityId); }

    public class GetPerformanceHistory(
        IRepository<PerformanceHistory> repository,
        IRepository<Appraisal> appraisals,
        IRepository<Achievement> achievements,
        IRepository<IndividualDevelopmentPlan> developmentPlans,
        IRepository<PerformanceImprovementPlan> improvementPlans,
        IRepository<EmployeeRecognition> recognitions,
        IPerformanceVisibilityService visibility) : IGetPerformanceHistory
    {
        public async Task<List<PerformanceHistoryDto>> GetAsync(string entityType, Guid entityId)
        {
            // The audit trail follows the RECORD it documents: if you may see the appraisal, you may see
            // its history. It used to be HR-only, which was invisible while every session resolved to
            // admin — once that was fixed (00EM) an employee opening their own appraisal was refused the
            // history panel on their own record.
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var ownerId = await ResolveOwnerEmployeeAsync(entityType, entityId);
                // Unknown entity type => fail CLOSED: no owner to check means no basis to grant access.
                if (ownerId is null || !await visibility.CanAccessEmployeeAsync(ownerId.Value))
                    throw new ValidationException("access", "You do not have access to this record's history.");
            }

            return await repository.GetAll().AsNoTracking()
                .Where(h => h.EntityType == entityType && h.EntityId == entityId)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new PerformanceHistoryDto
                {
                    Id = h.Id,
                    EntityType = h.EntityType,
                    EntityId = h.EntityId,
                    Action = h.Action,
                    Summary = h.Summary,
                    SnapshotJson = h.SnapshotJson,
                    CreatedBy = h.CreatedBy,
                    CreatedAt = h.CreatedAt.ToDateTimeUtc()
                })
                .ToListAsync();
        }

        /// <summary>
        /// The employee a history row's subject belongs to. Every type written by
        /// <see cref="IPerformanceHistoryWriter"/> is listed here — add new ones as they appear, or they
        /// silently become HR-only.
        /// </summary>
        private async Task<Guid?> ResolveOwnerEmployeeAsync(string entityType, Guid entityId) =>
            entityType switch
            {
                "Appraisal" => await FirstOwnerAsync(appraisals.GetAll().Where(x => x.Id == entityId).Select(x => (Guid?)x.EmployeeId)),
                "Achievement" => await FirstOwnerAsync(achievements.GetAll().Where(x => x.Id == entityId).Select(x => (Guid?)x.EmployeeId)),
                "DevelopmentPlan" => await FirstOwnerAsync(developmentPlans.GetAll().Where(x => x.Id == entityId).Select(x => (Guid?)x.EmployeeId)),
                "ImprovementPlan" => await FirstOwnerAsync(improvementPlans.GetAll().Where(x => x.Id == entityId).Select(x => (Guid?)x.EmployeeId)),
                "Recognition" => await FirstOwnerAsync(recognitions.GetAll().Where(x => x.Id == entityId).Select(x => (Guid?)x.EmployeeId)),
                // "Calibration" and "SalaryRevision" are deliberately absent, for the same reason: their
                // history rows describe a record that spans a COHORT (a calibration session, a revision
                // covering many employees), so there is no individual owner to authorise against. They
                // stay HR-only through the fail-closed default below — which is the intended access
                // rule for both, not an oversight. Every other handler on those records is HR-only too.
                _ => null,
            };

        private static Task<Guid?> FirstOwnerAsync(IQueryable<Guid?> query) => query.FirstOrDefaultAsync();
    }
}
