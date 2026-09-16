using CyberErp.Hrms.Inf.Common;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;

namespace CyberErp.Hrms.Api.Configuration
{
    /// <summary>
    /// Background-job processing (Hangfire, SQL Server storage in the existing CERP database under
    /// its own <c>HangFire</c> schema — no separate infrastructure). Tuned per the Hangfire 1.8
    /// recommendations so job processing never becomes the bottleneck the feature was built to avoid:
    /// <list type="bullet">
    /// <item><c>SlidingInvisibilityTimeout</c> + <c>QueuePollInterval = Zero</c> — the modern
    /// fetch loop: near-instant pickup via long-polling semantics instead of tight sp_ polling,
    /// with abandoned jobs (e.g. a killed server) re-appearing after the timeout.</item>
    /// <item><c>UseRecommendedIsolationLevel</c> + <c>DisableGlobalLocks</c> — READ COMMITTED
    /// instead of the legacy serializable transactions and no applock contention, the two classic
    /// Hangfire-on-SQL-Server bottlenecks.</item>
    /// <item>A SMALL fixed worker pool — e-mail dispatch is light I/O; the default
    /// (5 × processor count) would idle dozens of workers each holding SQL connections from the
    /// same pool the API's request path uses.</item>
    /// <item>Jobs are activated through the ASP.NET Core integration: each execution gets its own
    /// DI scope, created and disposed per job — scoped services (DbContext etc.) cannot leak.</item>
    /// </list>
    /// </summary>
    public static class HangfireConfiguration
    {
        public static IServiceCollection AddHrmsBackgroundJobs(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer(options =>
            {
                // E-mail dispatch is light, latency-tolerant I/O — a handful of workers clears any
                // realistic backlog while capping the background claim on the SQL connection pool.
                options.WorkerCount = Math.Clamp(Environment.ProcessorCount, 2, 4);
                options.Queues = ["default"];
                options.ServerName = $"hrms-{Environment.MachineName}";
            });

            return services;
        }

/// <summary>
        /// Operational dashboard at <c>/hangfire</c>.
        ///
        /// <para>⚠️ Gated on the System Settings privilege, not merely on being signed in, and
        /// read-only for anyone without Edit — see <see cref="HangfireDashboardAuthorizationFilter"/>
        /// for what an authentication-only check exposed (logic §12.91).</para>
        /// </summary>
        public static WebApplication UseHrmsBackgroundJobsDashboard(this WebApplication app)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [],
                AsyncAuthorization = [new HangfireDashboardAuthorizationFilter()],
                IsReadOnlyFunc = HangfireDashboardAuthorizationFilter.IsReadOnly,
                DisplayStorageConnectionString = false,
                DashboardTitle = "CyberErp HRMS — Background Jobs"
            });

            return app;
        }

        /// <summary>
        /// The nightly recurring sweeps.
        ///
        /// <para>⚠️ SEPARATE from the dashboard. These used to be registered inside
        /// <see cref="UseHrmsBackgroundJobsDashboard"/>, so turning the dashboard off would have
        /// silently stopped SCHEDULING as well as hiding the UI (logic §12.91).</para>
        ///
        /// <para>⚠️ Every one of them goes through <see cref="ITenantJobRunner"/>, which runs the
        /// sweep once per active tenant with that tenant ambient. Registering the handlers directly
        /// left them running with NO tenant — and the repository's filter is inert without one, so
        /// they read across every tenant at once.</para>
        /// </summary>
        public static WebApplication UseHrmsRecurringJobs(this WebApplication app)
        {

            // HC176: apply workflow-approved, future-dated personnel movements ON their effective
            // date. One indexed sweep per tenant per day (Status='Approved' AND EffectiveDate <= today);
            // AddOrUpdate keeps it a single recurring job across restarts.
            RecurringJob.AddOrUpdate<ITenantJobRunner>(
                "employee-movements-due",
                job => job.RunAsync(TenantSweep.DueMovements), Cron.Daily(1));   // 01:00 UTC daily

            // HC263: remind employees whose trip advance is past its settlement deadline.
            //
            // ⚠️ The runner calls RunUnattendedAsync, NOT RunAsync — that one carries the HR-only
            // guard for the on-demand endpoint, which a job with no HTTP context can never satisfy
            // (logic §12.73). The mapping lives in TenantJobRunner.InvokeAsync.
            RecurringJob.AddOrUpdate<ITenantJobRunner>(
                "trip-settlement-reminders",
                job => job.RunAsync(TenantSweep.TripSettlementReminders), Cron.Daily(2));   // 02:00 UTC daily

            // Phase 5: reconcile mandatory-training obligations, then chase the outstanding ones
            // (logic §12.88). One nightly pass per tenant materialises new obligations for people who
            // have joined or moved, closes the ones a completion now satisfies, opens the next
            // recertification cycle, reminds learners and escalates to managers.
            RecurringJob.AddOrUpdate<ITenantJobRunner>(
                "learning-compliance-sweep",
                job => job.RunAsync(TenantSweep.LearningCompliance), Cron.Daily(3));   // 03:00 UTC daily

            // ⚠️ AddOrUpdate only ever ADDS. A renamed job id would leave its old definition
            // scheduled for ever — which is how a stale definition survived a rename before and had
            // to be purged by hand. Anything not registered above is removed here, so the schedule in
            // the database always matches the schedule in this file (logic §12.91).
            ReconcileRecurringJobs([
                "employee-movements-due",
                "trip-settlement-reminders",
                "learning-compliance-sweep",
            ]);

            return app;
        }

        /// <summary>
        /// Drops recurring jobs this application no longer declares.
        /// </summary>
        /// <remarks>
        /// Report schedules are user-created at runtime under the <c>report-schedule:</c> prefix and
        /// are emphatically NOT ours to remove — they are owned by rows in the database, not by this
        /// file.
        /// </remarks>
        private static void ReconcileRecurringJobs(IReadOnlyCollection<string> declared)
        {
            using var connection = JobStorage.Current.GetConnection();
            var stale = connection.GetRecurringJobs()
                .Select(j => j.Id)
                .Where(id => !declared.Contains(id)
                             && !id.StartsWith("report-schedule:", StringComparison.Ordinal))
                .ToList();

            foreach (var id in stale) RecurringJob.RemoveIfExists(id);
        }
    }

    /// <summary>
    /// Response compression for the large JSON list payloads this HRMS serves (paged grids,
    /// rankings, exports) — Brotli first, gzip fallback; typically a 5–10× wire-size reduction.
    /// </summary>
    public static class ResponseCompressionConfiguration
    {
        public static IServiceCollection AddHrmsResponseCompression(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });
            services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(o =>
                o.Level = System.IO.Compression.CompressionLevel.Fastest);
            services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(o =>
                o.Level = System.IO.Compression.CompressionLevel.Fastest);
            return services;
        }
    }

}
