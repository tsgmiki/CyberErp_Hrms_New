using CyberErp.Hrms.App.Common.Authorization;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;

namespace CyberErp.Hrms.Api.Configuration
{
    /// <summary>
    /// Gates the Hangfire dashboard on the SAME permission model as the rest of the API.
    ///
    /// <para>⚠️ IT USED TO CHECK AUTHENTICATION ONLY — <c>result.Succeeded</c> and nothing else. Any
    /// signed-in employee could open <c>/hangfire</c>, and the dashboard is read-WRITE by default:
    /// they could read job arguments (which for the e-mail job carry the recipient, subject and the
    /// full message body — disciplinary notices, medical correspondence, salary letters), requeue
    /// jobs to re-send those messages, and delete jobs outright. It was the one surface the
    /// permission work never reached (logic §12.91).</para>
    ///
    /// <para>Two tiers, because the dashboard has two kinds of action:</para>
    /// <list type="bullet">
    /// <item><b>Seeing it</b> needs <c>View</c> on the System Settings operation — the existing
    /// operational-administration privilege. Anyone who cannot open System Settings has no business
    /// reading the job queue.</item>
    /// <item><b>Acting on it</b> (requeue, delete, trigger) additionally needs <c>Edit</c>. Everyone
    /// else gets the dashboard in Hangfire's own read-only mode rather than a hidden landmine.</item>
    /// </list>
    ///
    /// <para>⚠️ <c>View</c> rather than <c>Edit</c> for entry is deliberate: gating entry on Edit
    /// risks locking out a legitimate operator whose role holds the screen read-only, and the
    /// read/write split below already removes the dangerous half.</para>
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAsyncAuthorizationFilter
    {
        /// <summary>
        /// The operation whose grant confers job-dashboard access. Bare (no <c>hrms/</c> prefix) —
        /// <see cref="IEndpointPermissionService"/> normalises both sides (logic §12.69).
        /// </summary>
        private const string OperatorLink = "setting";

        /// <summary>
        /// Set by <see cref="AuthorizeAsync"/> and read by the read-only predicate, so the
        /// synchronous Hangfire callback never has to block on an async permission check.
        /// </summary>
        internal const string MayWriteItem = "hangfire.may-write";

        public async Task<bool> AuthorizeAsync(DashboardContext context)
        {
            var http = context.GetHttpContext();

            // The cookie scheme is authenticated EXPLICITLY because the app's default authenticate
            // scheme is JWT — outside controllers (which name the scheme via [Authorize]) the cookie
            // would otherwise never populate the user.
            var result = await http.AuthenticateAsync("Cookies");
            if (!result.Succeeded) return false;

            var permissions = http.RequestServices.GetRequiredService<IEndpointPermissionService>();

            if (!await permissions.HasAnyAsync([OperatorLink], PermissionAccess.View))
                return false;

            // Resolved once here so IsReadOnlyFunc — which Hangfire calls synchronously — can read a
            // plain bool instead of blocking a thread on the database.
            http.Items[MayWriteItem] = await permissions.HasAnyAsync([OperatorLink], PermissionAccess.Edit);
            return true;
        }

        /// <summary>
        /// True when this caller may only look. Runs after <see cref="AuthorizeAsync"/> on the same
        /// request, so the flag is always present; a missing flag is treated as read-only, which is
        /// the safe direction.
        /// </summary>
        internal static bool IsReadOnly(DashboardContext context) =>
            context.GetHttpContext().Items.TryGetValue(MayWriteItem, out var mayWrite)
                ? mayWrite is not true
                : true;
    }
}
