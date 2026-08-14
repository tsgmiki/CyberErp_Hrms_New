namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// The non-secret SMTP relay settings for one send.
    ///
    /// <para>⚠️ THERE IS NO PASSWORD HERE, DELIBERATELY. These values travel into a Hangfire job, and
    /// Hangfire persists job arguments in its own database tables — a password put in this record
    /// would be written to disk in clear text and kept for the life of the job history. The
    /// credential stays in configuration (user-secrets locally, environment variables elsewhere) and
    /// is read inside the job, which never leaves it anywhere.</para>
    ///
    /// <para>That split is what <c>Core.Setting</c> already encodes: it has SmtpHost, SmtpPort,
    /// SmtpUser and SmtpUseTls, and pointedly no password column.</para>
    /// </summary>
    public record SmtpSettings(string? Host, int Port, string? UserName, bool UseTls);

    /// <summary>
    /// Resolves the SMTP settings for the CURRENT tenant.
    ///
    /// <para>⚠️ Must be called IN-REQUEST. <c>Core.Setting</c> is tenant-scoped and a background job
    /// has no tenant context, so a resolve attempted from inside the dispatch job would quietly find
    /// nothing and fall back to configuration — which is exactly the bug this change exists to fix,
    /// in a place nobody would look.</para>
    /// </summary>
    public interface ISmtpSettingsResolver
    {
        /// <summary>
        /// The tenant's stored relay settings, falling back to the <c>Email</c> configuration section
        /// field by field for anything the tenant has not set.
        /// </summary>
        Task<SmtpSettings> ResolveAsync();
    }
}
