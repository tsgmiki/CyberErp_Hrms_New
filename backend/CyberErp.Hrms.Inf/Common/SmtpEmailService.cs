using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
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
        public Task<bool> SendAsync(string to, string subject, string body,
            IReadOnlyList<EmailAttachment>? attachments = null) =>
            SendAsync(to, subject, body, attachments, null);

        /// <summary>
        /// <paramref name="relay"/> carries the tenant's stored Host/Port/User/TLS, resolved
        /// IN-REQUEST (Core.Setting is tenant-scoped and this may run in a background job, which has
        /// no tenant). Null means "configuration only", which is also what an older queued job
        /// deserializes to.
        ///
        /// <para>⚠️ The PASSWORD is never carried in <paramref name="relay"/> — it is read from
        /// configuration right here, keyed to whichever user name won. Hangfire persists job
        /// arguments, so a password on that path would be written to disk in clear text.</para>
        /// </summary>
        public async Task<bool> SendAsync(string to, string subject, string body,
            IReadOnlyList<EmailAttachment>? attachments, SmtpSettings? relay)
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
                // The tenant's stored relay wins over configuration; see the overload's remarks.
                var userName = relay?.UserName ?? section["UserName"];

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

                // ⚠️ HTML is DETECTED, not assumed. Notification templates are authored in a
                // rich-text editor and are always HTML; the hardcoded notifiers still build plain
                // text with newline line breaks, and sending those as HTML would collapse every one.
                // Sending HTML as plain text is what shipped, and it delivered raw "<p>" markup to
                // employees.
                var isHtml = LooksLikeHtml(body);

                // ⚠️ Body carries the PLAIN text and the HTML goes in as the single alternate view.
                // Setting Body to the HTML *and* adding alternates emits the markup twice — once as
                // a stray text/plain part — because .NET turns Body into an alternate of its own as
                // soon as the collection is non-empty.
                using var message = new MailMessage
                {
                    From = new MailAddress(fromAddress ?? "no-reply@localhost", fromName),
                    Subject = subject,
                    Body = isHtml ? HtmlToPlainText(body) : body,
                    IsBodyHtml = false
                };
                if (isHtml)
                {
                    // multipart/alternative: the plain body for clients that will not render HTML
                    // (which is what employees were seeing), the HTML for those that will.
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body, null, "text/html"));
                }
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
                    var host = relay?.Host ?? section["Host"];
                    if (string.IsNullOrWhiteSpace(host))
                    {
                        logger.LogWarning(
                            "Email enabled but no SMTP host is configured (Core.Setting.SmtpHost or Email:Host) — skipped '{Subject}'",
                            subject);
                        return false;
                    }
                    client.Host = host;
                    client.Port = relay?.Port > 0 ? relay.Port : section.GetValue("Port", 587);
                    client.EnableSsl = relay?.UseTls ?? section.GetValue("EnableSsl", true);
                    client.Timeout = 15000;

                    // ⚠️ The password comes from CONFIGURATION only, never from the database and never
                    // from the job payload. Core.Setting has no password column by design, and
                    // Hangfire persists job arguments — a credential on either path would end up
                    // stored in clear text.
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var password = section["Password"];
                        if (string.IsNullOrEmpty(password))
                            logger.LogWarning(
                                "SMTP user '{User}' is set but Email:Password is empty — the relay will almost certainly reject '{Subject}'",
                                userName, subject);
                        client.Credentials = new NetworkCredential(userName, password);
                    }
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

        /// <summary>
        /// Whether the body is HTML. Deliberately matches a SHORT LIST OF BLOCK TAGS rather than any
        /// "&lt;", so a plain-text body that happens to contain a comparison ("salary &lt; 5000")
        /// is not mistaken for markup and stripped of its line breaks.
        /// </summary>
        private static bool LooksLikeHtml(string? body) =>
            !string.IsNullOrWhiteSpace(body) &&
            Regex.IsMatch(body, @"<\s*(html|body|table|div|p|br|h[1-6]|ul|ol|li|span|strong|b|em|i|a)\b[^>]*>",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// A readable text/plain alternate for an HTML body: block tags become line breaks, the rest
        /// is dropped and entities decoded. Not a full converter — it only has to be legible in a
        /// client that refuses HTML.
        /// </summary>
        private static string HtmlToPlainText(string html)
        {
            var text = Regex.Replace(html, @"<\s*(br|/p|/div|/h[1-6]|/li|/tr)\s*/?>", "\n",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            text = Regex.Replace(text, @"<[^>]+>", string.Empty);
            text = HttpUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"[ \t]+", " ");
            text = Regex.Replace(text, @"(\s*\n\s*){3,}", "\n\n");
            return text.Trim();
        }
    }
}
