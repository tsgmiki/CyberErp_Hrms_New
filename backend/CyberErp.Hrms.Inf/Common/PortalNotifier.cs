using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Writes portal alerts into the Home-owned <c>Core.Notification</c> table. Lives in Inf
    /// because it needs the DbContext + tenant context directly (the row is not a
    /// <see cref="Dom.Entities.BaseEntity"/>, so it bypasses the generic repository); TenantId is
    /// stamped from the current tenant so Home's per-tenant query filter surfaces it to the user.
    /// </summary>
    public class PortalNotifier(
        HrmsDbContext context,
        ITenantService tenantService,
        ILogger<PortalNotifier> logger) : IPortalNotifier
    {
        /// <summary>The subsystem code this app raises alerts as (matches Core.Subsystem.Code).</summary>
        private const string SourceSubsystemCode = "HRMS";

        public async Task NotifyUsersAsync(
            IEnumerable<Guid> userIds, string title, string? body, string? linkUrl,
            string severity, string sourceEntityType, Guid sourceEntityId)
        {
            var recipients = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (recipients.Count == 0) return; // open step / no assigned approver — nobody to alert

            var tenantId = tenantService.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                // Without a tenant the row would be invisible behind Home's query filter — skip loudly.
                logger.LogWarning("PortalNotifier: no current tenant; skipped {Count} alert(s) for {Type} {Id}",
                    recipients.Count, sourceEntityType, sourceEntityId);
                return;
            }

            foreach (var userId in recipients)
            {
                var notification = CoreNotification.Create(
                    userId, SourceSubsystemCode, title, body, linkUrl, severity, sourceEntityType, sourceEntityId);
                notification.TenantId = tenantId;
                await context.Set<CoreNotification>().AddAsync(notification);
            }
            await context.SaveChangesAsync();
            logger.LogInformation("PortalNotifier: raised {Count} alert(s) for {Type} {Id}",
                recipients.Count, sourceEntityType, sourceEntityId);
        }

        public async Task ResolveAsync(string sourceEntityType, Guid sourceEntityId)
        {
            var tenantId = tenantService.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId)) return;

            var open = await context.Set<CoreNotification>()
                .Where(n => n.TenantId == tenantId
                    && n.SourceEntityType == sourceEntityType
                    && n.SourceEntityId == sourceEntityId
                    && !n.IsRead)
                .ToListAsync();
            if (open.Count == 0) return;

            foreach (var n in open) n.MarkRead();
            await context.SaveChangesAsync();
            logger.LogInformation("PortalNotifier: resolved {Count} alert(s) for {Type} {Id}",
                open.Count, sourceEntityType, sourceEntityId);
        }
    }
}
