using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Reads the tenant's SMTP relay settings from <c>Core.Setting</c>, falling back to the
    /// <c>Email</c> configuration section for anything it has not set.
    ///
    /// <para><b>Why this exists.</b> The settings screen has written SmtpHost / SmtpPort / SmtpUser /
    /// SmtpUseTls to the database all along, and nothing ever read them — <c>SmtpEmailService</c> went
    /// straight to configuration. An administrator could change the relay, see it saved, and have it
    /// make no difference whatsoever.</para>
    ///
    /// <para>The fallback is field by field, not all-or-nothing: a tenant that has set only a host
    /// still inherits the configured port rather than dropping to a default that may be wrong.</para>
    /// </summary>
    public class SmtpSettingsResolver(
        IRepository<Setting> settings,
        IConfiguration configuration,
        ILogger<SmtpSettingsResolver> logger) : ISmtpSettingsResolver
    {
        public async Task<SmtpSettings> ResolveAsync()
        {
            var section = configuration.GetSection("Email");
            var fallback = new SmtpSettings(
                section["Host"],
                section.GetValue("Port", 587),
                section["UserName"],
                section.GetValue("EnableSsl", true));

            Setting? stored = null;
            try
            {
                stored = await settings.GetAll()
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Never let a settings lookup break a send. Configuration alone is a working mailer.
                logger.LogWarning(ex, "Could not read SMTP settings from Core.Setting — using configuration");
                return fallback;
            }

            if (stored is null) return fallback;

            return new SmtpSettings(
                string.IsNullOrWhiteSpace(stored.SmtpHost) ? fallback.Host : stored.SmtpHost.Trim(),
                stored.SmtpPort > 0 ? stored.SmtpPort : fallback.Port,
                string.IsNullOrWhiteSpace(stored.SmtpUser) ? fallback.UserName : stored.SmtpUser.Trim(),
                stored.SmtpUseTls);
        }
    }
}
