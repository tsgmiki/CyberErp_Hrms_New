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
        IPerformanceVisibilityService visibility) : IGetPerformanceHistory
    {
        public async Task<List<PerformanceHistoryDto>> GetAsync(string entityType, Guid entityId)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("access", "Only HR can view performance history.");

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
    }
}
