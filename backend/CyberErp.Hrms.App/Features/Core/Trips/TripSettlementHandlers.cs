using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Notifications;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Trips
{
    // ---- DTOs ---------------------------------------------------------------
    public class DisburseTripAdvanceDto { public string? Reference { get; set; } }
    public class SettleTripDto { public string? Reference { get; set; } }

    public class TripAgingItemDto
    {
        public Guid TripId { get; set; }
        public string TripNumber { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public string TripType { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public int DaysOutstanding { get; set; }
        public string Bucket { get; set; } = string.Empty;
        public decimal AdvanceAmount { get; set; }
        public string Currency { get; set; } = "ETB";
    }

    public class TripAgingRowDto
    {
        public string Bucket { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    public class TripAgingReportDto
    {
        public List<TripAgingRowDto> Buckets { get; set; } = [];
        public List<TripAgingItemDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IDisburseTripAdvance { Task DisburseAsync(Guid id, string? reference); }
    public interface ISettleTrip { Task<decimal> SettleAsync(Guid id, string? reference); }
    public interface IGetTripAgingReport { Task<TripAgingReportDto> GetAsync(); }
    public interface ITripSettlementReminder
    {
        /// <summary>On-demand run for the signed-in HR user — authorised, since it mails the whole tenant.</summary>
        Task<int> RunAsync();

        /// <summary>
        /// The daily unattended pass. NO HR check, because there is no user to check.
        ///
        /// <para>⚠️ Hangfire runs with no HTTP context, so <c>GetCurrentUserId()</c> is null,
        /// <c>IsAdminAsync</c> returns false at its first line, and the HR guard on
        /// <see cref="RunAsync"/> threw "Only HR can run the settlement reminders." EVERY night —
        /// the job had never once sent a reminder (logic §12.73). Called only by the recurring job
        /// registration; the name is meant to make any other caller look wrong.</para>
        /// </summary>
        Task<int> RunUnattendedAsync();
    }

    // ---- Settlement helpers -------------------------------------------------
    internal static class TripSettlement
    {
        /// <summary>Days after a trip ends by which the advance must be settled (HC263).</summary>
        internal const int SettlementDueDays = 15;

        /// <summary>
        /// Days to leave a traveller alone after reminding them.
        ///
        /// <para>⚠️ THE SWEEP IS NIGHTLY BUT THE DEBT IS NOT. A trip stays overdue until it is
        /// settled, so with no cooldown the same person got the same e-mail every night until they
        /// acted — indefinitely. Weekly is the cadence a human would use for a dunning notice, and it
        /// still produces four reminders inside a month (logic §12.92).</para>
        /// </summary>
        internal const int ReminderCooldownDays = 7;

        /// <summary>Trips with an issued (disbursed) advance still awaiting settlement.</summary>
        internal static IQueryable<TripRequest> OutstandingAdvances(IQueryable<TripRequest> q) =>
            q.Where(t => t.AdvanceDisbursedAt != null && t.AdvanceAmount > 0
                && t.Status != TripRequestStatus.Settled && t.Status != TripRequestStatus.Cancelled && t.Status != TripRequestStatus.Rejected);

        internal static string BucketFor(int daysOutstanding) => daysOutstanding switch
        {
            < 0 => "Not due",
            <= 15 => "0–15 days",
            <= 30 => "16–30 days",
            <= 60 => "31–60 days",
            _ => "Over 60 days"
        };
    }

    // ---- Handlers -----------------------------------------------------------
    /// <summary>HC268 — records the trip advance payment (finance/CBS hand-off; live payment deferred).</summary>
    public class DisburseTripAdvance(
        IRepository<TripRequest> repository,
        IPerformanceVisibilityService visibility,
        ILogger<DisburseTripAdvance> logger) : IDisburseTripAdvance
    {
        public async Task DisburseAsync(Guid id, string? reference)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can pay trip advances.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id) ?? throw new NotFoundException(nameof(TripRequest), id.ToString());
            if (entity.Status is not (TripRequestStatus.Approved or TripRequestStatus.InProgress))
                throw new ValidationException(nameof(id), "The advance can only be paid on an approved or in-progress trip.");
            if (entity.AdvanceDisbursedAt.HasValue)
                throw new ValidationException(nameof(id), "The advance has already been paid.");
            entity.DisburseAdvance(DateTime.UtcNow.Date, reference);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Disbursed trip advance {Id} (ref {Reference})", id, reference);
        }
    }

    /// <summary>
    /// HC264/HC268 — settles a trip: reconciles the advance against actual expenses (net = advance −
    /// expenses; positive = employee refunds, negative = company reimburses) and records the settlement.
    /// </summary>
    public class SettleTrip(
        IRepository<TripRequest> repository,
        IRepository<TripExpense> expenseRepository,
        IPerformanceVisibilityService visibility,
        IEndpointPermissionService permissions) : ISettleTrip
    {
        public async Task<decimal> SettleAsync(Guid id, string? reference)
        {
            var scope = await visibility.GetScopeAsync();
            var entity = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id) ?? throw new NotFoundException(nameof(TripRequest), id.ToString());
            // The traveller submits their settlement request; HR may finalize it.
            if (!await permissions.HasAnyAsync(HrScreens.TripRegister) && entity.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException(nameof(id), "You can only settle your own trips.");
            if (entity.Status is not (TripRequestStatus.Completed or TripRequestStatus.InProgress))
                throw new ValidationException(nameof(id), "Only a completed trip can be settled.");

            var totalExpenses = await expenseRepository.GetAll().AsNoTracking().Where(e => e.TripRequestId == id).SumAsync(e => (decimal?)e.Amount) ?? 0m;
            var net = entity.AdvanceAmount - totalExpenses;   // + => refund due from employee; − => reimbursement to employee
            entity.Settle(DateTime.UtcNow.Date, net, reference);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            return net;
        }
    }

    /// <summary>HC265 — aging report of outstanding trip advances, bucketed by days since the trip ended.</summary>
    public class GetTripAgingReport(
        IRepository<TripRequest> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetTripAgingReport
    {
        public async Task<TripAgingReportDto> GetAsync()
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can view the aging report.");
            var today = DateTime.UtcNow.Date;
            var employees = employeeRepository.GetAll();
            var rows = await TripSettlement.OutstandingAdvances(repository.GetAll().AsNoTracking())
                .Select(t => new TripAgingItemDto
                {
                    TripId = t.Id, TripNumber = t.TripNumber,
                    EmployeeName = employees.Where(e => e.Id == t.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    TripType = t.TripType.ToString(), EndDate = t.EndDate, AdvanceAmount = t.AdvanceAmount, Currency = t.Currency
                }).ToListAsync();

            foreach (var r in rows)
            {
                r.DaysOutstanding = (today - r.EndDate.Date).Days;
                r.Bucket = TripSettlement.BucketFor(r.DaysOutstanding);
            }

            var buckets = rows.GroupBy(r => r.Bucket)
                .Select(g => new TripAgingRowDto { Bucket = g.Key, Count = g.Count(), TotalOutstanding = g.Sum(x => x.AdvanceAmount) })
                .ToList();

            return new TripAgingReportDto
            {
                Buckets = buckets,
                Items = rows.OrderByDescending(r => r.DaysOutstanding).ToList(),
                TotalCount = rows.Count,
                TotalOutstanding = rows.Sum(r => r.AdvanceAmount)
            };
        }
    }

    /// <summary>
    /// HC263 — reminds employees whose trip advance is past the settlement deadline (trip end + due days).
    /// Runs daily via Hangfire (all tenants) and on demand per tenant; best-effort mail never blocks.
    /// </summary>
    public class TripSettlementReminder(
        IRepository<TripRequest> repository,
        IRepository<User> userRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IEmailService emailService,
        INotificationDispatcher dispatcher,
        ILogger<TripSettlementReminder> logger) : ITripSettlementReminder
    {
        /// <summary>
        /// On-demand: triggering this mails EVERY employee with an overdue advance, so it is an HR
        /// action. The guard belongs HERE and not in the shared body — the daily Hangfire pass has no
        /// signed-in user to satisfy it, and applying it there rejected the job on every run.
        /// </summary>
        public async Task<int> RunAsync()
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin)
                throw new ValidationException("access", "Only HR can run the settlement reminders.");

            return await RunUnattendedAsync();
        }

        public async Task<int> RunUnattendedAsync()
        {
            var today = DateTime.UtcNow.Date;
            var overdue = await TripSettlement.OutstandingAdvances(repository.GetAll().AsNoTracking())
                .Where(t => t.Status != TripRequestStatus.Requested)   // advance issued => already past Requested
                .Select(t => new
                {
                    t.Id, t.TripNumber, t.EmployeeId, t.EndDate, t.AdvanceAmount, t.Currency,
                    t.LastSettlementReminderOn
                })
                .ToListAsync();

            var users = userRepository.GetAll();
            var sent = 0;
            var reminded = new List<Guid>();
            var suppressed = 0;
            foreach (var t in overdue)
            {
                var dueBy = t.EndDate.Date.AddDays(TripSettlement.SettlementDueDays);
                if (today <= dueBy) continue;   // not yet overdue

                // ⚠️ The cooldown, and the reason this sweep is not a nightly nag. Anyone reminded
                // inside the window is left alone; the debt is still outstanding and still shows on
                // the aging report, which is where HR chases it from.
                if (t.LastSettlementReminderOn is { } last
                    && (today - last.Date).TotalDays < TripSettlement.ReminderCooldownDays)
                {
                    suppressed++;
                    continue;
                }

                // Template first, hardcoded mail as the fallback — see MovementNotifier.
                var who = await employeeRepository.GetAll().AsNoTracking()
                    .Where(e => e.Id == t.EmployeeId)
                    .Select(e => new
                    {
                        e.EmployeeNumber,
                        Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                    })
                    .FirstOrDefaultAsync();

                var dispatched = await dispatcher.DispatchAsync(new NotificationContext(
                    NotificationEvents.TripSettlementOverdue,
                    new Dictionary<string, string?>
                    {
                        ["EmployeeName"] = who?.Name,
                        ["EmployeeNumber"] = who?.EmployeeNumber,
                        ["TripNumber"] = t.TripNumber,
                        ["AdvanceAmount"] = t.AdvanceAmount.ToString("N2"),
                        ["Currency"] = t.Currency,
                        ["DueDate"] = dueBy.ToString("dd MMM yyyy"),
                    },
                    RequesterEmployeeId: t.EmployeeId,
                    EntityType: nameof(TripRequest),
                    EntityId: t.Id));
                if (dispatched > 0) { sent += dispatched; reminded.Add(t.Id); continue; }
                var email = await users.Where(u => u.EmployeeId == t.EmployeeId && u.Email != "").Select(u => u.Email).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(email)) continue;
                var ok = await emailService.SendAsync(email, $"Settle your travel advance — {t.TripNumber}",
                    $"Your travel advance of {t.AdvanceAmount:N2} {t.Currency} for trip {t.TripNumber} was due for settlement on {dueBy:yyyy-MM-dd}. Please submit your settlement.");
                if (ok) { sent++; reminded.Add(t.Id); }
            }

            // ⚠️ STAMPED ONLY FOR REMINDERS THAT ACTUALLY WENT OUT. Stamping the whole overdue set
            // would silence the cooldown for people who were never reached — a traveller with no
            // e-mail address would be marked "reminded" and then never chased again.
            if (reminded.Count > 0)
            {
                var rows = await repository.GetAll().Where(t => reminded.Contains(t.Id)).ToListAsync();
                foreach (var row in rows) row.RecordSettlementReminder(today);
                await repository.SaveChangesAsync();
            }

            if (overdue.Count > 0)
                logger.LogInformation(
                    "Trip settlement reminders: {Sent} sent, {Suppressed} inside the {Cooldown}-day cooldown, {Overdue} outstanding advances.",
                    sent, suppressed, TripSettlement.ReminderCooldownDays, overdue.Count);
            return sent;
        }
    }
}
