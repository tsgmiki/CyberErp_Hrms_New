using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>
    /// Annual-leave accrual engine (corrected replacement for the legacy setting/ledger logic):
    /// computes each employee's entitlement from service length + the fiscal-year policy, posts it
    /// as ledger transactions (idempotently), and performs fiscal-year rollover with carry-forward
    /// caps and expiry of over-aged carry.
    /// </summary>
    public interface ILeaveAccrualService
    {
        /// <summary>Pure entitlement calculation — exposed for previews and tests.</summary>
        decimal CalculateEntitlement(DateTime? hireDate, bool isManagerial, AnnualLeaveSetting setting, DateTime fyStart, DateTime fyEnd);
        /// <summary>Generates opening entitlements for every active employee under a policy. Idempotent (skips employees already generated).</summary>
        Task<int> GenerateEntitlementsAsync(Guid settingId);
        /// <summary>Rolls remaining balances of a fiscal year into the next one (carry-forward + expiry), then closes the source year.</summary>
        Task<RolloverResult> RolloverAsync(Guid fromFiscalYearId);
    }

    public record RolloverResult(int BalancesRolled, decimal TotalCarried, decimal TotalExpired);

    public class LeaveAccrualService(
        IRepository<AnnualLeaveSetting> settings,
        IRepository<FiscalYear> fiscalYears,
        IRepository<Employee> employees,
        IRepository<LeaveBalance> balances,
        IRepository<LeaveBalanceTransaction> transactions,
        IRepository<LeaveType> leaveTypes,
        ILogger<LeaveAccrualService> logger) : ILeaveAccrualService
    {
        public decimal CalculateEntitlement(DateTime? hireDate, bool isManagerial, AnnualLeaveSetting setting, DateTime fyStart, DateTime fyEnd)
        {
            if (!hireDate.HasValue) return 0m; // no hire date on record → nothing accrues
            var hire = hireDate.Value.Date;
            if (hire > fyEnd.Date) return 0m;  // hired after the year ends

            var serviceMonthsAtStart = MonthsBetween(hire, fyStart.Date);

            if (serviceMonthsAtStart < 12)
            {
                // Under a year of service: prorate the new-employee basis by months served in this FY.
                var from = hire > fyStart.Date ? hire : fyStart.Date;
                var monthsInYear = Math.Min(12, MonthsBetween(from, fyEnd.Date) + 1);
                if (monthsInYear <= 0) return 0m;
                var basis = setting.NewEmployeeLeaveDays > 0 ? setting.NewEmployeeLeaveDays : setting.BaseLeaveDays;
                // Half-day precision, rounded down (conservative).
                return Math.Floor(basis * monthsInYear / 12m * 2) / 2;
            }

            var serviceYears = serviceMonthsAtStart / 12;
            var baseDays = isManagerial ? setting.ManagerialLeaveDays : setting.BaseLeaveDays;
            var extra = ((serviceYears - 1) / setting.IncrementIntervalYears) * setting.IncrementDays;
            return Math.Min(baseDays + extra, setting.MaxLeaveDays);
        }

        public async Task<int> GenerateEntitlementsAsync(Guid settingId)
        {
            var setting = await settings.GetAll()
                .Include(s => s.FiscalYear)
                .FirstOrDefaultAsync(s => s.Id == settingId)
                ?? throw new NotFoundException(nameof(AnnualLeaveSetting), settingId.ToString());
            if (!setting.IsActive)
                throw new ValidationException("id", "This leave setting is inactive.");
            var fy = setting.FiscalYear
                ?? throw new ValidationException("id", "The setting's fiscal year could not be loaded.");
            if (fy.IsClosed)
                throw new ValidationException("id", "The fiscal year is closed.");

            var fyStart = fy.StartDate.ToDateTimeUtc().Date;
            var fyEnd = fy.EndDate.ToDateTimeUtc().Date;

            // Employees already generated for this FY + type are skipped (idempotency).
            var existing = await balances.GetAll()
                .Where(b => b.FiscalYearId == setting.FiscalYearId && b.LeaveTypeId == setting.LeaveTypeId)
                .Select(b => b.EmployeeId)
                .ToListAsync();
            var existingSet = existing.ToHashSet();

            var staff = await employees.GetAll()
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
                .Select(e => new { e.Id, e.HireDate, e.IsManagerial })
                .ToListAsync();

            var created = 0;
            foreach (var emp in staff)
            {
                if (existingSet.Contains(emp.Id)) continue;

                var entitled = CalculateEntitlement(emp.HireDate, emp.IsManagerial, setting, fyStart, fyEnd);
                var balance = LeaveBalance.Create(emp.Id, setting.LeaveTypeId, setting.FiscalYearId, entitled);
                await balances.AddAsync(balance);
                if (entitled > 0)
                {
                    await transactions.AddAsync(LeaveBalanceTransaction.Create(
                        emp.Id, setting.LeaveTypeId, setting.FiscalYearId,
                        LeaveBalanceTransactionType.Entitlement, entitled, entitled,
                        $"Annual entitlement {fy.Name}", setting.Id));
                }
                created++;
            }

            if (created > 0) await balances.SaveChangesAsync();
            logger.LogInformation("Generated {Count} entitlement(s) for setting {SettingId} ({FY})", created, settingId, fy.Name);
            return created;
        }

        public async Task<RolloverResult> RolloverAsync(Guid fromFiscalYearId)
        {
            var from = await fiscalYears.GetAll().FirstOrDefaultAsync(f => f.Id == fromFiscalYearId)
                ?? throw new NotFoundException(nameof(FiscalYear), fromFiscalYearId.ToString());
            if (from.IsClosed)
                throw new ValidationException("id", "This fiscal year is already closed.");

            var to = await fiscalYears.GetAll()
                .Where(f => !f.IsClosed && f.StartDate > from.EndDate)
                .OrderBy(f => f.StartDate)
                .FirstOrDefaultAsync()
                ?? throw new ValidationException("id", "No following fiscal year exists to roll into. Create it first.");

            var sourceBalances = await balances.GetAll()
                .Where(b => b.FiscalYearId == from.Id)
                .ToListAsync();
            var typeCaps = await leaveTypes.GetAll()
                .ToDictionaryAsync(t => t.Id, t => t.CarryForwardMaxDays);

            int rolled = 0;
            decimal totalCarried = 0, totalExpired = 0;

            foreach (var src in sourceBalances)
            {
                var remaining = src.Available;
                if (remaining <= 0) continue;

                // Days that already arrived here as carry-forward may not be carried again
                // (Ethiopian labour law: leave must be used within the following period).
                var expired = Math.Min(remaining, src.CarriedForward);
                var carriable = remaining - expired;

                var cap = typeCaps.TryGetValue(src.LeaveTypeId, out var c) ? c : null;
                if (cap.HasValue && carriable > cap.Value)
                {
                    expired += carriable - cap.Value;
                    carriable = cap.Value;
                }

                if (expired > 0)
                {
                    await transactions.AddAsync(LeaveBalanceTransaction.Create(
                        src.EmployeeId, src.LeaveTypeId, from.Id,
                        LeaveBalanceTransactionType.Expiry, -expired, remaining - expired,
                        $"Expired on rollover to {to.Name}", null));
                    totalExpired += expired;
                }

                if (carriable > 0)
                {
                    var dest = await balances.GetAll().FirstOrDefaultAsync(b =>
                        b.EmployeeId == src.EmployeeId && b.LeaveTypeId == src.LeaveTypeId && b.FiscalYearId == to.Id);
                    if (dest is null)
                    {
                        dest = LeaveBalance.Create(src.EmployeeId, src.LeaveTypeId, to.Id);
                        await balances.AddAsync(dest);
                    }
                    dest.AddCarryForward(carriable);
                    await transactions.AddAsync(LeaveBalanceTransaction.Create(
                        src.EmployeeId, src.LeaveTypeId, to.Id,
                        LeaveBalanceTransactionType.CarryForward, carriable, dest.Available,
                        $"Carried forward from {from.Name}", null));
                    totalCarried += carriable;
                }

                rolled++;
            }

            from.Close();
            fiscalYears.UpdateAsync(from);
            await balances.SaveChangesAsync();

            logger.LogInformation("Rolled over {Count} balance(s) from {From} to {To}: carried {Carried}, expired {Expired}",
                rolled, from.Name, to.Name, totalCarried, totalExpired);
            return new RolloverResult(rolled, totalCarried, totalExpired);
        }

        /// <summary>Whole months between two dates (0 when to &lt; from).</summary>
        private static int MonthsBetween(DateTime fromDate, DateTime toDate)
        {
            if (toDate < fromDate) return 0;
            var months = (toDate.Year - fromDate.Year) * 12 + toDate.Month - fromDate.Month;
            if (toDate.Day < fromDate.Day) months--;
            return Math.Max(0, months);
        }
    }
}
