using System.ComponentModel;
using CyberErp.Hrms.App.Common.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// The Hangfire job that performs the actual SMTP send, OFF the request path. The full message
    /// payload (recipient, subject, body, attachments) is composed in-request and serialized into
    /// the job — the job itself touches NO tenant-scoped data, which keeps it safe in this
    /// multi-tenant app (background jobs have no request, hence no Finbuckle tenant context).
    /// <para>
    /// Unlike <see cref="SmtpEmailService"/> (which never throws), this job THROWS on a failed
    /// send so Hangfire's automatic retry takes over — delivery becomes durable: a transient SMTP
    /// outage delays the e-mail instead of losing it.
    /// </para>
    /// </summary>
    public class EmailDispatchJob(
        SmtpEmailService smtp,
        IConfiguration configuration,
        ILogger<EmailDispatchJob> logger)
    {
        /// <summary>Spread retries out: 1 min, 5 min, 15 min, 1 h, 2 h — then the job parks as Failed
        /// (visible on the /hangfire dashboard) instead of hammering a dead relay.</summary>
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = [60, 300, 900, 3600, 7200])]
        [DisplayName("E-mail: {1} → {0}")]
        public async Task SendAsync(string to, string subject, string body, List<EmailAttachment>? attachments)
        {
            // Config may have changed between enqueue and execution — a disabled mailer is a
            // deliberate no-op, not a failure to retry.
            if (!configuration.GetSection("Email").GetValue("Enabled", false))
            {
                logger.LogInformation("Email disabled at dispatch time — dropped '{Subject}' → {To}", subject, to);
                return;
            }

            var sent = await smtp.SendAsync(to, subject, body, attachments);
            if (!sent)
                throw new InvalidOperationException(
                    $"SMTP send failed for '{subject}' → {to} — Hangfire will retry per the backoff schedule.");
        }
    }
}
