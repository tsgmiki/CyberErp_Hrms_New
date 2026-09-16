using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
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
        decimal CalculateEntitlement(EmployeeAccrualInput input, AnnualLeaveSetting setting, DateTime fyStart, DateTime fyEnd);
        /// <summary>Generates opening entitlements for every active employee under a policy. Idempotent (skips employees already generated).</summary>
        Task<int> GenerateEntitlementsAsync(Guid settingId);
        /// <summary>
        /// Re-applies the policy to entitlements ALREADY generated for its fiscal year, correcting
        /// <c>Entitled</c> where the recomputed figure differs.
        /// </summary>
        /// <remarks>
        /// ⚠️ <see cref="GenerateEntitlementsAsync"/> SKIPS anyone who already has a balance, which is
        /// right for repeat runs and useless after a calculation fix — the wrong figures simply stay.
        /// This is the missing counterpart: it exists so a corrected rule can reach rows that were
        /// generated under the old one (logic §12.94).
        /// </remarks>
        Task<RecalculateResult> RecalculateEntitlementsAsync(Guid settingId);
        /// <summary>Rolls remaining balances of a fiscal year into the next one (carry-forward + expiry), then closes the source year.</summary>
        Task<RolloverResult> RolloverAsync(Guid fromFiscalYearId);
    }

    public record RolloverResult(int BalancesRolled, decimal TotalCarried, decimal TotalExpired);

    /// <summary>Outcome of re-applying a policy to already-generated entitlements.</summary>
    /// <param name="Examined">Active employees considered.</param>
    /// <param name="Raised">Balances whose entitlement went up.</param>
    /// <param name="Lowered">Balances whose entitlement went down.</param>
    /// <param name="Created">Employees who had no balance yet (hired since generation) and now do.</param>
    /// <param name="NetChange">Sum of every delta applied, in days.</param>
    /// <param name="OverTaken">
    /// Employees whose recomputed entitlement is now BELOW what they have already taken. Reported
    /// rather than clamped — see <c>RecalculateEntitlementsAsync</c>.
    /// </param>
    public record RecalculateResult(
        int Examined, int Raised, int Lowered, int Created, decimal NetChange, List<string> OverTaken);

    /// <summary>Per-employee facts the accrual engine needs, resolved once by the caller.</summary>
    /// <param name="ExternalExperienceMonths">Total qualifying external (government) experience in months.</param>
    /// <param name="FiscalYearsOfService">Completed fiscal years since hire (for the fiscal-year rule).</param>
    public record EmployeeAccrualInput(
        DateTime? HireDate, bool IsManagerial, int ExternalExperienceMonths, int FiscalYearsOfService);

    public class LeaveAccrualService(
        IRepository<AnnualLeaveSetting> settings,
        IRepository<FiscalYear> fiscalYears,
        IRepository<Employee> employees,
        IRepository<EmployeeExperience> experiences,
        IRepository<LeaveBalance> balances,
        IRepository<LeaveBalanceTransaction> transactions,
        ILogger<LeaveAccrualService> logger) : ILeaveAccrualService
    {
        public decimal CalculateEntitlement(EmployeeAccrualInput input, AnnualLeaveSetting setting, DateTime fyStart, DateTime fyEnd)
        {
            if (!input.HireDate.HasValue) return 0m; // no hire date on record → nothing accrues
            var hire = input.HireDate.Value.Date;
            if (hire > fyEnd.Date) return 0m;  // hired after the year ends

            // Service is measured to the start of the fiscal year being calculated.
            var asOf = fyStart.Date;
            var actualServiceMonths = MonthsBetween(hire, asOf);

            // Below the minimum-service gate: prorate the new-employee basis by months served this FY.
            if (actualServiceMonths < setting.MinExperienceMonths)
            {
                var from = hire > fyStart.Date ? hire : fyStart.Date;
                var monthsInYear = Math.Min(12, MonthsBetween(from, fyEnd.Date) + 1);
                if (monthsInYear <= 0) return 0m;
                var basis = setting.NewEmployeeLeaveDays > 0 ? setting.NewEmployeeLeaveDays : setting.BaseLeaveDays;
                return Math.Floor(basis * monthsInYear / 12m * 2) / 2; // half-day precision, rounded down
            }

            var baseDays = input.IsManagerial ? setting.ManagerialLeaveDays : setting.BaseLeaveDays;
            decimal days;

            switch (setting.RuleType)
            {
                case LeaveAccrualRuleType.ServiceMilestone:
                {
                    var milestone = (setting.MilestoneDate ?? asOf).Date;

                    // Rule B — the ordinary entitlement, and the FLOOR for everyone (see below).
                    var ordinary = baseDays
                        + (CompletedYears(hire, asOf) / setting.IncrementIntervalYears) * setting.IncrementDays;

                    if (hire <= milestone)
                    {
                        // Rule A — external experience may extend the pre-milestone service (toggle).
                        var extMonths = setting.ConsiderExternalExperience ? input.ExternalExperienceMonths : 0;
                        var effStart = hire.AddMonths(-extMonths);
                        var preYears = CompletedYears(effStart, milestone);
                        var postYears = CompletedYears(milestone, asOf);
                        var preInterval = setting.PreMilestoneIntervalYears < 1 ? 1 : setting.PreMilestoneIntervalYears;

                        // ⚠️ MANAGERIAL STAFF KEEP THEIR MANAGERIAL BASE HERE TOO. This branch used to
                        // hardcode PreMilestoneBaseLeaveDays for everyone, so a managerial pre-milestone
                        // hire silently dropped from the 20-day managerial base to the 14-day legacy one
                        // — baseDays was computed above and then never read (logic §12.94).
                        var preBase = input.IsManagerial ? setting.ManagerialLeaveDays : setting.PreMilestoneBaseLeaveDays;

                        var grandfathered = preBase
                             + (preYears / preInterval) * setting.PreMilestoneIncrementDays
                             + (postYears / setting.IncrementIntervalYears) * setting.IncrementDays;

                        // ⚠️ THE GRANDFATHERED RULE IS A FLOOR, NOT A REPLACEMENT. Rule A credits +1/yr
                        // only for service BEFORE the cutover, so someone hired shortly before it got the
                        // lower legacy base with almost no pre-milestone credit — less than a colleague
                        // hired weeks LATER received under Rule B. Taking the better of the two is what
                        // "grandfathering" is supposed to mean: nobody is made worse off by having been
                        // there longer (logic §12.94).
                        days = Math.Max(grandfathered, ordinary);
                    }
                    else
                    {
                        // Post-milestone hires never count external experience.
                        days = ordinary;
                    }
                    break;
                }
                case LeaveAccrualRuleType.FiscalYears:
                {
                    // Rule C — increment per N completed fiscal years (external optional via the toggle).
                    var extYears = setting.ConsiderExternalExperience ? input.ExternalExperienceMonths / 12 : 0;
                    var fiscalYears = input.FiscalYearsOfService + extYears;
                    days = baseDays + (fiscalYears / setting.IncrementIntervalYears) * setting.IncrementDays;
                    break;
                }
                default: // ServiceYears — single-phase service-based (external optional via the toggle).
                {
                    var extMonths = setting.ConsiderExternalExperience ? input.ExternalExperienceMonths : 0;
                    var years = CompletedYears(hire.AddMonths(-extMonths), asOf);
                    days = baseDays + (years / setting.IncrementIntervalYears) * setting.IncrementDays;
                    break;
                }
            }

            // 0 = uncapped.
            return setting.MaxLeaveDays > 0 ? Math.Min(days, setting.MaxLeaveDays) : days;
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

            // Annual leave has no LeaveType: its entitlement comes from this per-FY setting, and its
            // balance rows are the ones with a null LeaveTypeId (see AnnualLeave).
            var annualLeaveTypeId = AnnualLeave.LeaveTypeId;

            // Employees already generated for this FY are skipped (idempotency).
            var existing = await balances.GetAll()
                .Where(b => b.FiscalYearId == setting.FiscalYearId && b.LeaveTypeId == null)
                .Select(b => b.EmployeeId)
                .ToListAsync();
            var existingSet = existing.ToHashSet();

            var staff = await employees.GetAll()
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
                .Select(e => new { e.Id, e.PersonId, e.HireDate, e.IsManagerial })
                .ToListAsync();

            // Per-employee facts the flexible rules need, resolved in bulk.
            var externalMonthsByPerson = await LoadExternalExperienceMonthsAsync(staff.Select(s => s.PersonId));
            var fyStartDates = await fiscalYears.GetAll().Select(f => f.StartDate).ToListAsync();

            var created = 0;
            foreach (var emp in staff)
            {
                if (existingSet.Contains(emp.Id)) continue;

                var input = new EmployeeAccrualInput(
                    emp.HireDate, emp.IsManagerial,
                    externalMonthsByPerson.GetValueOrDefault(emp.PersonId),
                    CountFiscalYearsOfService(fyStartDates, emp.HireDate, fyStart));
                var entitled = CalculateEntitlement(input, setting, fyStart, fyEnd);
                var balance = LeaveBalance.Create(emp.Id, annualLeaveTypeId, setting.FiscalYearId, entitled);
                await balances.AddAsync(balance);
                if (entitled > 0)
                {
                    await transactions.AddAsync(LeaveBalanceTransaction.Create(
                        emp.Id, annualLeaveTypeId, setting.FiscalYearId,
                        LeaveBalanceTransactionType.Entitlement, entitled, entitled,
                        $"Annual entitlement {fy.Name}", setting.Id));
                }
                created++;
            }

            if (created > 0) await balances.SaveChangesAsync();
            logger.LogInformation("Generated {Count} entitlement(s) for setting {SettingId} ({FY})", created, settingId, fy.Name);
            return created;
        }

        /// <inheritdoc cref="ILeaveAccrualService.RecalculateEntitlementsAsync"/>
        /// <remarks>
        /// <para>⚠️ IT DOES NOT DELETE AND RE-CREATE THE BALANCES. <c>CarriedForward</c>, <c>Adjusted</c>
        /// and <c>Taken</c> are real history — carried days from last year's rollover, manual HR
        /// corrections, and leave people have already been granted. Only <c>Entitled</c> is the
        /// policy's to restate; deleting the row would destroy the other three and orphan approved
        /// leave.</para>
        ///
        /// <para>⚠️ EVERY CHANGE POSTS AN <c>Adjustment</c> TRANSACTION. The balance table is a
        /// fast-read aggregate over an append-only ledger; silently rewriting <c>Entitled</c> would
        /// leave the ledger unable to explain the figure it now shows.</para>
        ///
        /// <para>⚠️ A LOWERED ENTITLEMENT IS APPLIED, NOT CLAMPED, even when it falls below what the
        /// employee has already taken. Clamping would quietly hide that somebody is over-drawn; the
        /// affected employees are returned instead so HR can deal with each one.</para>
        /// </remarks>
        public async Task<RecalculateResult> RecalculateEntitlementsAsync(Guid settingId)
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
                throw new ValidationException("id", "The fiscal year is closed — its entitlements can no longer be restated.");

            var fyStart = fy.StartDate.ToDateTimeUtc().Date;
            var fyEnd = fy.EndDate.ToDateTimeUtc().Date;
            var annualLeaveTypeId = AnnualLeave.LeaveTypeId;

            var staff = await employees.GetAll()
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
                .Select(e => new { e.Id, e.PersonId, e.EmployeeNumber, e.HireDate, e.IsManagerial })
                .ToListAsync();

            // Tracked, because the entitlement is written back onto these rows.
            var existing = (await balances.GetAll()
                    .Where(b => b.FiscalYearId == setting.FiscalYearId && b.LeaveTypeId == null)
                    .ToListAsync())
                .ToDictionary(b => b.EmployeeId);

            var externalMonthsByPerson = await LoadExternalExperienceMonthsAsync(staff.Select(s => s.PersonId));
            var fyStartDates = await fiscalYears.GetAll().Select(f => f.StartDate).ToListAsync();

            int raised = 0, lowered = 0, created = 0;
            decimal net = 0;
            var overTaken = new List<string>();

            foreach (var emp in staff)
            {
                var input = new EmployeeAccrualInput(
                    emp.HireDate, emp.IsManagerial,
                    externalMonthsByPerson.GetValueOrDefault(emp.PersonId),
                    CountFiscalYearsOfService(fyStartDates, emp.HireDate, fyStart));
                var entitled = CalculateEntitlement(input, setting, fyStart, fyEnd);

                if (!existing.TryGetValue(emp.Id, out var balance))
                {
                    // Hired since the ledger was generated — bring them in rather than leaving a gap.
                    balance = LeaveBalance.Create(emp.Id, annualLeaveTypeId, setting.FiscalYearId, entitled);
                    await balances.AddAsync(balance);
                    if (entitled > 0)
                        await transactions.AddAsync(LeaveBalanceTransaction.Create(
                            emp.Id, annualLeaveTypeId, setting.FiscalYearId,
                            LeaveBalanceTransactionType.Entitlement, entitled, entitled,
                            $"Annual entitlement {fy.Name}", setting.Id));
                    created++;
                    continue;
                }

                var delta = entitled - balance.Entitled;
                if (delta == 0) continue;

                balance.SetOpening(entitled, balance.CarriedForward, balance.Adjusted);
                await transactions.AddAsync(LeaveBalanceTransaction.Create(
                    emp.Id, annualLeaveTypeId, setting.FiscalYearId,
                    LeaveBalanceTransactionType.Adjustment, delta, balance.Available,
                    $"Entitlement restated by policy recalculation ({fy.Name})", setting.Id));

                net += delta;
                if (delta > 0) raised++; else lowered++;
                if (balance.Available < 0) overTaken.Add(emp.EmployeeNumber);
            }

            if (raised + lowered + created > 0) await balances.SaveChangesAsync();

            logger.LogInformation(
                "Recalculated entitlements for setting {SettingId} ({FY}): {Raised} raised, {Lowered} lowered, " +
                "{Created} created, net {Net} day(s){OverTaken}",
                settingId, fy.Name, raised, lowered, created, net,
                overTaken.Count > 0 ? $"; {overTaken.Count} employee(s) now over-taken" : string.Empty);

            return new RecalculateResult(staff.Count, raised, lowered, created, net, overTaken);
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
            // Carry cap now lives on the CLOSING fiscal year's policy (moved from LeaveType).
            var carryCap = await settings.GetAll()
                .Where(s => s.FiscalYearId == from.Id && s.IsActive)
                .Select(s => s.CarryForwardMaxDays).FirstOrDefaultAsync();

            // Preload the destination year's balances ONCE (tracked, so mutations below persist) —
            // the loop previously issued one lookup query per carriable balance, i.e. thousands of
            // round-trips on an all-employee rollover. Keyed by (employee, leave type).
            var destByKey = (await balances.GetAll().Where(b => b.FiscalYearId == to.Id).ToListAsync())
                .ToDictionary(b => (b.EmployeeId, b.LeaveTypeId));

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

                if (carryCap.HasValue && carriable > carryCap.Value)
                {
                    expired += carriable - carryCap.Value;
                    carriable = carryCap.Value;
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
                    if (!destByKey.TryGetValue((src.EmployeeId, src.LeaveTypeId), out var dest))
                    {
                        dest = LeaveBalance.Create(src.EmployeeId, src.LeaveTypeId, to.Id);
                        await balances.AddAsync(dest);
                        destByKey[(src.EmployeeId, src.LeaveTypeId)] = dest;
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

        /// <summary>Completed whole years between two dates.</summary>
        private static int CompletedYears(DateTime fromDate, DateTime toDate) => MonthsBetween(fromDate, toDate) / 12;

        /// <summary>Total qualifying external (government) experience months per person, resolved in bulk.</summary>
        private async Task<Dictionary<Guid, int>> LoadExternalExperienceMonthsAsync(IEnumerable<Guid> personIds)
        {
            var ids = personIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, int>();
            var rows = await experiences.GetAll()
                .Where(x => ids.Contains(x.PersonId) && x.IsExternal && x.IsGovernmental
                    && x.StartDate != null && x.EndDate != null)
                .Select(x => new { x.PersonId, x.StartDate, x.EndDate })
                .ToListAsync();
            return rows
                .GroupBy(r => r.PersonId)
                .ToDictionary(g => g.Key, g => g.Sum(r => MonthsBetween(r.StartDate!.Value.Date, r.EndDate!.Value.Date)));
        }

        /// <summary>Completed fiscal years of service = fiscal-year starts falling after hire, up to the current FY start.</summary>
        private static int CountFiscalYearsOfService(List<Instant> fiscalYearStarts, DateTime? hireDate, DateTime currentFyStart)
        {
            if (!hireDate.HasValue) return 0;
            var hire = hireDate.Value.Date;
            var current = currentFyStart.Date;
            return fiscalYearStarts.Count(s =>
            {
                var d = s.ToDateTimeUtc().Date;
                return d > hire && d <= current;
            });
        }
    }
}
