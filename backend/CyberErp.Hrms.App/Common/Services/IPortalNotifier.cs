namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// Raises portal alerts into <c>Core.Notification</c> — the table the Home portal reads for
    /// its notification bell / dashboard. HRMS uses this to tell approvers (in Home) that a record
    /// is awaiting their decision, and to clear those alerts once the step is decided. Best-effort:
    /// a portal-notification failure must never break the governing operation, so callers wrap
    /// invocations in try/catch. The recipient <c>userIds</c> are Core.User ids (shared table).
    /// </summary>
    public interface IPortalNotifier
    {
        /// <summary>
        /// Raise one alert per distinct recipient. No-op when <paramref name="userIds"/> is empty
        /// (e.g. an open workflow step with no assigned approvers — nobody specific to alert).
        /// </summary>
        Task NotifyUsersAsync(
            IEnumerable<Guid> userIds, string title, string? body, string? linkUrl,
            string severity, string sourceEntityType, Guid sourceEntityId);

        /// <summary>Mark every unread alert for a source record read — call when it is decided/closed.</summary>
        Task ResolveAsync(string sourceEntityType, Guid sourceEntityId);
    }
}
