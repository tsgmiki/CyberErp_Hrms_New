using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.AuditLogs.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.AuditLogs
{
    public interface IGetAllAuditLogs { Task<PaginatedResponse<AuditLogDto>> GetAsync(GetAllRequest request); }

    /// <summary>
    /// Read-only audit-trail query. Entries are immutable (written by the SaveChanges interceptor).
    /// Branch-scoped: branch admins only see their branch's trail; Head Office sees the whole log.
    /// </summary>
    public class GetAllAuditLogs(IRepository<AuditLog> repository) : IGetAllAuditLogs
    {
        public async Task<PaginatedResponse<AuditLogDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            // Filter by entity type (spare ReportName field) and specific record (UserId field reused as entityId).
            if (!string.IsNullOrWhiteSpace(request.ReportName))
                query = query.Where(x => x.EntityType == request.ReportName);
            if (request.UserId.HasValue)
                query = query.Where(x => x.PerformedByUserId == request.UserId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x =>
                    x.EntityType.Contains(term) ||
                    (x.EntityName != null && x.EntityName.Contains(term)) ||
                    (x.PerformedBy != null && x.PerformedBy.Contains(term)));
            }

            var total = await query.CountAsync();

            // Materialize then map (NodaTime Instant → DateTime, enum → string) in memory.
            var page = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .ToListAsync();

            var data = page.Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                EntityName = a.EntityName,
                Action = a.Action.ToString(),
                Changes = a.Changes,
                PerformedByUserId = a.PerformedByUserId,
                PerformedBy = a.PerformedBy,
                BranchId = a.BranchId,
                Timestamp = a.CreatedAt.ToDateTimeUtc()
            }).ToList();

            return new PaginatedResponse<AuditLogDto> { Total = total, Data = data };
        }
    }
}
