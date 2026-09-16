using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.App.Features.Core.Training;
using CyberErp.Hrms.App.Features.Core.Trips;
using CyberErp.Hrms.Dom.Entities.Core;
using CyberErp.Hrms.Inf.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// The queues jobs are dispatched to.
    /// </summary>
    /// <remarks>
    /// ⚠️ EVERYTHING USED TO RUN ON ONE QUEUE with a handful of shared workers. The nightly sweeps
    /// are long — they loop every tenant, and each tenant's pass walks its whole population — while a
    /// scheduled report is a short job somebody is waiting for. On a shared queue a sweep occupied a
    /// worker for as long as it took and the reports queued behind it (logic §12.92).
    /// </remarks>
    public static class HrmsQueues
    {
        /// <summary>Short, latency-sensitive work: report schedules, ad-hoc jobs.</summary>
        public const string Default = "default";

        /// <summary>The long nightly per-tenant sweeps. Served by their own single-worker server.</summary>
        public const string Sweeps = "sweeps";
    }

    /// <summary>The recurring sweeps that must run once per tenant.</summary>
    /// <remarks>
    /// An enum rather than a delegate or a type argument because Hangfire SERIALISES the job's
    /// arguments into its own tables: a lambda cannot be stored, and an assembly-qualified type name
    /// would break the moment a class moved. An enum member is stable, reads well on the dashboard,
    /// and cannot be got wrong at the call site.
    /// </remarks>
    public enum TenantSweep
    {
        /// <summary>HC176 — apply approved movements whose effective date has arrived.</summary>
        DueMovements = 0,
        /// <summary>HC263 — chase travel advances past their settlement deadline.</summary>
        TripSettlementReminders = 1,
        /// <summary>§12.88 — reconcile mandatory-training obligations, then chase the outstanding ones.</summary>
        LearningCompliance = 2
    }

    /// <summary>
    /// Runs a nightly sweep ONCE PER ACTIVE TENANT, each in its own DI scope with that tenant
    /// established as the ambient Finbuckle context.
    ///
    /// <para>⚠️ THIS EXISTS BECAUSE TENANT ISOLATION IS FAIL-OPEN. <c>Repository.ApplyTenantFilter</c>
    /// applies its filter only when a tenant is resolved, and a Hangfire job has no request — so
    /// every read in an unscoped sweep silently spanned ALL tenants. With a single tenant that is
    /// invisible; with two it is a data-crossing bug, and the training sweep was the worst of them:
    /// an <c>Everyone</c> audience would have selected every employee in every tenant and created
    /// obligations for them stamped with the *assignment's* tenant (logic §12.91).</para>
    ///
    /// <para>⚠️ Lives in Inf, not App, for two reasons: it sets the Finbuckle ambient context, and it
    /// carries a Hangfire attribute. Neither belongs in the application layer.</para>
    /// </summary>
    public interface ITenantJobRunner
    {
        /// <summary>
        /// ⚠️ <c>Attempts = 0</c> IS THE POINT. These sweeps message people: the trip reminder
        /// e-mails every employee with an overdue advance, and the compliance chase notifies everyone
        /// with training due. Hangfire's DEFAULT of 10 retries meant a sweep that threw at recipient
        /// 200 of 300 re-notified the first 199 on every attempt — up to ten waves in one night. The
        /// script <c>clear-stale-settlement-reminder-jobs.sql</c> exists because exactly that
        /// happened, and it records the backoff it produced (logic §12.73).
        ///
        /// <para>A daily sweep's natural retry is TOMORROW. Failing once, loudly and visibly, is the
        /// right behaviour for an idempotent job that runs again in 24 hours.</para>
        ///
        /// <para>⚠️ BOTH ATTRIBUTES BELONG ON THE INTERFACE, NOT THE IMPLEMENTATION. The jobs are
        /// registered as <c>AddOrUpdate&lt;ITenantJobRunner&gt;(...)</c>, so what Hangfire stores and
        /// later reflects over is THIS method. Moving them to <see cref="TenantJobRunner"/> would look
        /// tidier and would silently restore the ten-retry default and the shared queue.</para>
        ///
        /// <para><c>[Queue]</c> rather than <c>RecurringJobOptions.QueueName</c> so it also applies
        /// when an operator presses Trigger on the dashboard — the option only covers the scheduled
        /// path, the attribute covers every enqueue of this method.</para>
        /// </summary>
        [AutomaticRetry(Attempts = 0)]
        [Queue(HrmsQueues.Sweeps)]
        Task RunAsync(TenantSweep sweep);
    }

    /// <inheritdoc cref="ITenantJobRunner"/>
    public class TenantJobRunner(
        IServiceScopeFactory scopeFactory,
        ILogger<TenantJobRunner> logger) : ITenantJobRunner
    {
        public async Task RunAsync(TenantSweep sweep)
        {
            var tenants = await LoadActiveTenantsAsync();
            if (tenants.Count == 0)
            {
                logger.LogWarning("Sweep {Sweep} found no active tenants — nothing to do.", sweep);
                return;
            }

            var failed = new List<string>();

            foreach (var tenant in tenants)
            {
                // A scope per tenant, so the DbContext and every scoped service is built fresh with
                // that tenant ambient. Reusing one scope would carry the first tenant's tracked
                // entities — and its resolved tenant — into the next.
                using var scope = scopeFactory.CreateScope();
                var setter = scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
                setter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenant, null, null);
                try
                {
                    await InvokeAsync(sweep, scope.ServiceProvider);
                    logger.LogInformation("Sweep {Sweep} completed for tenant {Tenant}.", sweep, tenant.Identifier);
                }
                catch (Exception ex)
                {
                    // ⚠️ One tenant's failure must never cost the others their nightly run. Recorded
                    // and rethrown together at the end so the dashboard still shows the job red.
                    logger.LogError(ex, "Sweep {Sweep} FAILED for tenant {Tenant}.", sweep, tenant.Identifier);
                    failed.Add(tenant.Identifier);
                }
                finally
                {
                    // The accessor is ambient (AsyncLocal): cleared so a tenant cannot leak into
                    // whatever this worker thread picks up next.
                    setter.MultiTenantContext = new MultiTenantContext<AppTenantInfo>(null, null, null);
                }
            }

            if (failed.Count > 0)
                throw new InvalidOperationException(
                    $"Sweep {sweep} failed for {failed.Count} of {tenants.Count} tenant(s): {string.Join(", ", failed)}. " +
                    "The remaining tenants completed; see the log for each failure.");
        }

        /// <summary>
        /// Every active tenant, read in its own scope with the filter explicitly bypassed.
        ///
        /// <para><c>GetAllWithoutTenantFilter</c> is deliberate and is the one place it is correct:
        /// enumerating tenants is precisely the operation that cannot be tenant-scoped. Saying so
        /// explicitly beats relying on the filter being inert because no tenant is resolved.</para>
        /// </summary>
        private async Task<List<AppTenantInfo>> LoadActiveTenantsAsync()
        {
            using var scope = scopeFactory.CreateScope();
            var tenants = scope.ServiceProvider.GetRequiredService<IRepository<Tenant>>();

            return await tenants.GetAllWithoutTenantFilter().AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Identifier)
                .Select(t => new AppTenantInfo
                {
                    Id = t.Id.ToString(),
                    Identifier = t.Identifier,
                    Name = t.Name,
                    IsActive = true
                })
                .ToListAsync();
        }

        /// <summary>
        /// Maps a sweep to the handler that performs it.
        ///
        /// <para>⚠️ Each of these is the UNATTENDED entry point. The on-demand siblings carry an
        /// HR-only guard that a background job — having no signed-in user — can never satisfy
        /// (logic §12.73).</para>
        /// </summary>
        private static Task InvokeAsync(TenantSweep sweep, IServiceProvider services) => sweep switch
        {
            TenantSweep.DueMovements =>
                services.GetRequiredService<IExecuteDueMovements>().ExecuteAsync(),
            TenantSweep.TripSettlementReminders =>
                services.GetRequiredService<ITripSettlementReminder>().RunUnattendedAsync(),
            TenantSweep.LearningCompliance =>
                services.GetRequiredService<ILearningComplianceChaser>().RunUnattendedAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(sweep), sweep, "Unknown tenant sweep."),
        };
    }
}
