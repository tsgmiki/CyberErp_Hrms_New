using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
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
    public interface ITripSettlementReminder { Task<int> RunAsync(); }

    // ---- Settlement helpers -------------------------------------------------
    internal static class TripSettlement
    {
        /// <summary>Days after a trip ends by which the advance must be settled (HC263).</summary>
        internal const int SettlementDueDays = 15;

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
        IPerformanceVisibilityService visibility) : ISettleTrip
    {
        public async Task<decimal> SettleAsync(Guid id, string? reference)
        {
            var scope = await visibility.GetScopeAsync();
            var entity = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id) ?? throw new NotFoundException(nameof(TripRequest), id.ToString());
            // The traveller submits their settlement request; HR may finalize it.
            if (!scope.IsAdmin && entity.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
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
        IEmailService emailService,
        ILogger<TripSettlementReminder> logger) : ITripSettlementReminder
    {
        public async Task<int> RunAsync()
        {
            var today = DateTime.UtcNow.Date;
            var overdue = await TripSettlement.OutstandingAdvances(repository.GetAll().AsNoTracking())
                .Where(t => t.Status != TripRequestStatus.Requested)   // advance issued => already past Requested
                .Select(t => new { t.Id, t.TripNumber, t.EmployeeId, t.EndDate, t.AdvanceAmount, t.Currency })
                .ToListAsync();

            var users = userRepository.GetAll();
            var sent = 0;
            foreach (var t in overdue)
            {
                var dueBy = t.EndDate.Date.AddDays(TripSettlement.SettlementDueDays);
                if (today <= dueBy) continue;   // not yet overdue
                var email = await users.Where(u => u.EmployeeId == t.EmployeeId && u.Email != "").Select(u => u.Email).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(email)) continue;
                var ok = await emailService.SendAsync(email, $"Settle your travel advance — {t.TripNumber}",
                    $"Your travel advance of {t.AdvanceAmount:N2} {t.Currency} for trip {t.TripNumber} was due for settlement on {dueBy:yyyy-MM-dd}. Please submit your settlement.");
                if (ok) sent++;
            }
            if (overdue.Count > 0)
                logger.LogInformation("Trip settlement reminders: {Sent} sent for {Overdue} outstanding advances.", sent, overdue.Count);
            return sent;
        }
    }
}
