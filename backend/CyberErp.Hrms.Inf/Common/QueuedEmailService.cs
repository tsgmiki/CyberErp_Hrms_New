using CyberErp.Hrms.App.Common.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// The <see cref="IEmailService"/> the application consumes: instead of blocking the request on
    /// SMTP network I/O (up to the 15 s timeout), it enqueues an <see cref="EmailDispatchJob"/> and
    /// returns immediately — Hangfire delivers in the background with automatic retries.
    /// <para>
    /// The cheap business guards stay HERE, in-request, so callers keep their semantics: a missing
    /// recipient or a disabled mailer returns <c>false</c> synchronously (e.g. an approved offer
    /// stays Approved for manual handling) and nothing is enqueued. A <c>true</c> return now means
    /// "durably queued for delivery" — stronger than the old one-shot attempt, since transient
    /// failures are retried instead of lost.
    /// </para>
    /// </summary>
    public class QueuedEmailService(
        IBackgroundJobClient jobs,
        ISmtpSettingsResolver smtpSettings,
        IConfiguration configuration,
        ILogger<QueuedEmailService> logger) : IEmailService
    {
        public async Task<bool> SendAsync(string to, string subject, string body,
            IReadOnlyList<EmailAttachment>? attachments = null)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                logger.LogInformation("Email '{Subject}' skipped — no recipient address", subject);
                return false;
            }
            if (!configuration.GetSection("Email").GetValue("Enabled", false))
            {
                logger.LogInformation("Email disabled — skipped '{Subject}' → {To}", subject, to);
                return false;
            }

            // ⚠️ The relay settings are resolved HERE, in-request, for the same reason the payload is
            // materialized here: Core.Setting is tenant-scoped and the job has no tenant context.
            // Resolving inside the job would silently fall back to configuration and the tenant's
            // stored host would be ignored — the exact bug this change fixes.
            var relay = await smtpSettings.ResolveAsync();

            // The payload is fully materialized here (attachments included) so the job carries no
            // tenant-scoped dependencies; List<> keeps the Hangfire argument serialization simple.
            var payload = attachments?.ToList();
            var jobId = jobs.Enqueue<EmailDispatchJob>(j => j.SendAsync(to, subject, body, payload, relay));
            logger.LogInformation("Email queued (job {JobId}) via {Host}: '{Subject}' → {To}",
                jobId, relay.Host ?? "<no host>", subject, to);
            return true;
        }
    }
}
