using System.Net;
using System.Net.Mail;
using CyberErp.Hrms.App.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// SMTP implementation of <see cref="IEmailService"/>, driven by the "Email" configuration
    /// section:
    ///   Enabled          — master switch (false = every send is a logged no-op)
    ///   Host/Port/EnableSsl/UserName/Password — SMTP relay settings
    ///   FromAddress/FromName                  — sender identity
    ///   PickupDirectory  — when set, messages are written as .eml files instead of network
    ///                      delivery (development / testing without a mail server)
    /// NEVER throws: notification mail must never break the operation that triggered it.
    /// </summary>
    public class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger) : IEmailService
    {
        public async Task<bool> SendAsync(string to, string subject, string body,
            IReadOnlyList<EmailAttachment>? attachments = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    logger.LogInformation("Email '{Subject}' skipped — no recipient address", subject);
                    return false;
                }

                var section = configuration.GetSection("Email");
                if (!section.GetValue("Enabled", false))
                {
                    logger.LogInformation("Email disabled — skipped '{Subject}' → {To}", subject, to);
                    return false;
                }

                var configuredFrom = section["FromAddress"];
                var fromName = section["FromName"] ?? "CyberErp HRMS";
                var userName = section["UserName"];

                // Authenticated relays (Gmail, Microsoft 365, …) reject a From that is not the
                // authenticated mailbox or a verified alias — the message silently fails to send.
                // When the login IS an e-mail address that differs from the branded From, send AS
                // the account (so it is accepted) and keep the branded address as Reply-To so
                // replies still reach it. Non-address logins (e.g. SendGrid's "apikey") are left
                // alone — the configured From stands.
                string? replyTo = null;
                var fromAddress = configuredFrom;
                if (LooksLikeEmail(userName) &&
                    !string.Equals(userName, configuredFrom, StringComparison.OrdinalIgnoreCase))
                {
                    fromAddress = userName;
                    if (LooksLikeEmail(configuredFrom)) replyTo = configuredFrom;
                }

                using var message = new MailMessage
                {
                    From = new MailAddress(fromAddress ?? "no-reply@localhost", fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(to);
                if (replyTo is not null) message.ReplyToList.Add(new MailAddress(replyTo, fromName));
                foreach (var a in attachments ?? [])
                    message.Attachments.Add(new Attachment(new MemoryStream(a.Content), a.FileName, a.ContentType));

                using var client = new SmtpClient();
                var pickupDirectory = section["PickupDirectory"];
                if (!string.IsNullOrWhiteSpace(pickupDirectory))
                {
                    Directory.CreateDirectory(pickupDirectory);
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = pickupDirectory;
                }
                else
                {
                    var host = section["Host"];
                    if (string.IsNullOrWhiteSpace(host))
                    {
                        logger.LogWarning("Email enabled but Email:Host is not configured — skipped '{Subject}'", subject);
                        return false;
                    }
                    client.Host = host;
                    client.Port = section.GetValue("Port", 587);
                    client.EnableSsl = section.GetValue("EnableSsl", true);
                    client.Timeout = 15000;
                    if (!string.IsNullOrEmpty(section["UserName"]))
                        client.Credentials = new NetworkCredential(section["UserName"], section["Password"]);
                }

                await client.SendMailAsync(message);
                logger.LogInformation("Email sent: '{Subject}' → {To}", subject, to);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Email send failed: '{Subject}' → {To} — the business operation is unaffected", subject, to);
                return false;
            }
        }

        /// <summary>A minimal address check — enough to tell a mailbox login from an API-key login.</summary>
        private static bool LooksLikeEmail(string? value) =>
            !string.IsNullOrWhiteSpace(value) && value.Contains('@') && value.IndexOf('@') < value.LastIndexOf('.');
    }
}
