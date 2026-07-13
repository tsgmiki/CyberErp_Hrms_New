using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;

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

        /// <summary>Operational dashboard at <c>/hangfire</c> — authenticated users only.</summary>
        public static WebApplication UseHrmsBackgroundJobsDashboard(this WebApplication app)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [],
                AsyncAuthorization = [new HangfireDashboardAuthorizationFilter()],
                DisplayStorageConnectionString = false,
                DashboardTitle = "CyberErp HRMS — Background Jobs"
            });
            return app;
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

    /// <summary>
    /// The dashboard is an operational surface — it must never be public. Access requires the same
    /// authenticated cookie session the API itself uses (log in through the app first). The cookie
    /// scheme is authenticated EXPLICITLY because the app's default authenticate scheme is JWT —
    /// outside controllers (which name the scheme via [Authorize]) the cookie would otherwise
    /// never populate the user.
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAsyncAuthorizationFilter
    {
        public async Task<bool> AuthorizeAsync(DashboardContext context)
        {
            var result = await context.GetHttpContext().AuthenticateAsync("Cookies");
            return result.Succeeded;
        }
    }
}
