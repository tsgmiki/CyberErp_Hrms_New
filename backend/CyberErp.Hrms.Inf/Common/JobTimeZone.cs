using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>The zone every recurring job's cron expression is read in.</summary>
    public interface IJobTimeZone
    {
        TimeZoneInfo Zone { get; }
    }

    /// <summary>
    /// Resolves the scheduling time zone once, from <c>Hangfire:TimeZone</c>.
    ///
    /// <para>⚠️ HANGFIRE READS A CRON IN UTC UNLESS YOU TELL IT OTHERWISE, and nothing told it
    /// otherwise. The report-schedule form asks the user for an hour of day — "send this at 07:00" —
    /// and then fired it at 07:00 UTC, which for an East African organisation is 10:00 local. Every
    /// scheduled report has been arriving three hours late, and the three nightly sweeps have been
    /// running at 04:00–06:00 local rather than in the small hours (logic §12.92).</para>
    ///
    /// <para>A single organisation-wide zone rather than a per-user one: a schedule is an
    /// organisational artefact, and resolving it from whoever happened to press Save would mean the
    /// same schedule fired at different times depending on who last enabled it. A per-schedule zone
    /// is the honest end state, but the schedule header is written through a stored procedure, so it
    /// needs a table and proc change rather than a code change.</para>
    /// </summary>
    public sealed class JobTimeZone : IJobTimeZone
    {
        /// <summary>
        /// Matches the default already used for user preferences, so the two agree out of the box.
        /// </summary>
        public const string DefaultZoneId = "Africa/Nairobi";

        public TimeZoneInfo Zone { get; }

        public JobTimeZone(IConfiguration configuration, ILogger<JobTimeZone> logger)
        {
            var configured = configuration["Hangfire:TimeZone"];
            var id = string.IsNullOrWhiteSpace(configured) ? DefaultZoneId : configured.Trim();

            // ⚠️ Accepts BOTH an IANA id ("Africa/Nairobi") and a Windows id ("E. Africa Standard
            // Time"): on .NET 8+ the lookup is ICU-backed and cross-recognises them. It stops doing
            // so under InvariantGlobalization, where an IANA id silently fails to resolve and every
            // schedule quietly reverts to UTC — hence the error log rather than a shrug.
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
            {
                Zone = zone;
                // ⚠️ Plain strings, not a format specifier. TimeSpan does NOT support the
                // positive;negative;zero section syntax that numeric formats do, so the obvious
                // "UTC{Offset:+hh\:mm;-hh\:mm}" throws while rendering and the line vanishes — which
                // is how this very message went missing on its first run.
                var offset = zone.BaseUtcOffset;
                logger.LogInformation("Recurring jobs are scheduled in {Zone} (UTC{Sign}{Offset}).",
                    zone.Id, offset < TimeSpan.Zero ? "-" : "+", offset.Duration().ToString(@"hh\:mm"));
                return;
            }

            Zone = TimeZoneInfo.Utc;
            logger.LogError(
                "Hangfire:TimeZone '{Configured}' is not a time zone this machine recognises — recurring jobs " +
                "will run in UTC, so every schedule will fire at the wrong local time. Set a valid IANA id " +
                "(e.g. '{Default}') or a Windows id.", id, DefaultZoneId);
        }
    }
}
