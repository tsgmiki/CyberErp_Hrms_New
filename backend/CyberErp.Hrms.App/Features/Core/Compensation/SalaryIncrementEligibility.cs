using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    /// <summary>Why an employee is out, or how much of the increment they keep.</summary>
    public readonly record struct IncrementEligibility(
        bool IsEligible,
        string? Reason,
        int? MonthsOfService,
        /// <summary>1 = full increment; 0.5 = six months' worth. Only below 1 inside the first year.</summary>
        decimal ProrationFactor);

    public interface ISalaryIncrementEligibility
    {
        IncrementEligibility Evaluate(Guid employeeId, DateTime? hireDate, DateTime effectiveDate);
        int MinimumServiceMonths { get; }
        bool HasPolicy { get; }
        /// <summary>Move an employee onto the next grade when a step increment clears their ceiling.</summary>
        bool PromoteOnGradeCeiling { get; }
    }

    public interface ISalaryIncrementEligibilityFactory
    {
        /// <summary>Loads the policy and the disciplinary blocks for the whole target set, once.</summary>
        Task<ISalaryIncrementEligibility> BuildAsync(
            IReadOnlyCollection<Guid> employeeIds, DateTime effectiveDate, CancellationToken ct = default);
    }

    internal sealed class SalaryIncrementEligibility(
        SalaryIncrementPolicy? policy,
        HashSet<Guid> disciplinaryBlocked) : ISalaryIncrementEligibility
    {
        public int MinimumServiceMonths => policy?.MinimumServiceMonths ?? 0;
        public bool HasPolicy => policy is not null;
        // Defaults to FALSE with no policy, unlike the other rules: changing someone's grade is a
        // bigger act than adjusting their pay, so it only happens when a client has asked for it.
        public bool PromoteOnGradeCeiling => policy?.PromoteOnGradeCeiling ?? false;

        public IncrementEligibility Evaluate(Guid employeeId, DateTime? hireDate, DateTime effectiveDate)
        {
            // Rule 2 first: a disciplinary block is absolute, so there is no point costing the rest.
            if ((policy?.ExcludeActiveDisciplinary ?? true) && disciplinaryBlocked.Contains(employeeId))
                return new IncrementEligibility(false, "Excluded: an active disciplinary case.", null, 0m);

            var minMonths = MinimumServiceMonths;
            var prorate = policy?.ProrateFirstYear ?? true;

            // No hire date means service cannot be assessed. Excluding is the safe reading — assuming
            // eligibility would hand an increment to someone who may not qualify, and the gap is a data
            // problem HR should see rather than have papered over.
            if (hireDate is null)
            {
                if (minMonths == 0 && !prorate)
                    return new IncrementEligibility(true, null, null, 1m);
                return new IncrementEligibility(false, "Excluded: no hire date, so service cannot be assessed.", null, 0m);
            }

            var months = CompletedMonths(hireDate.Value, effectiveDate);

            if (months < 0)
                return new IncrementEligibility(false, "Excluded: hire date is after the effective date.", months, 0m);

            if (minMonths > 0 && months < minMonths)
                return new IncrementEligibility(false,
                    $"Excluded: {months} month(s) of service, minimum is {minMonths}.", months, 0m);

            // Rule 3: inside the first year the increment is earned pro rata.
            if (prorate && months < 12)
                return new IncrementEligibility(true, null, months, Math.Round(months / 12m, 6));

            return new IncrementEligibility(true, null, months, 1m);
        }

        /// <summary>
        /// Whole months elapsed, not a day count: an employee hired on 15 Jan has 5 completed months on
        /// 14 Jun and 6 on 15 Jun. Anchoring on the day-of-month keeps "3 months of service" meaning the
        /// same thing regardless of month lengths.
        /// </summary>
        internal static int CompletedMonths(DateTime from, DateTime to)
        {
            var months = ((to.Year - from.Year) * 12) + to.Month - from.Month;
            if (to.Day < from.Day) months--;
            return months;
        }
    }

    public sealed class SalaryIncrementEligibilityFactory(
        IRepository<SalaryIncrementPolicy> policies,
        IRepository<DisciplinaryMeasure> disciplinary) : ISalaryIncrementEligibilityFactory
    {
        public async Task<ISalaryIncrementEligibility> BuildAsync(
            IReadOnlyCollection<Guid> employeeIds, DateTime effectiveDate, CancellationToken ct = default)
        {
            var policy = await policies.GetAll().AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(ct);

            var blocked = new HashSet<Guid>();
            if ((policy?.ExcludeActiveDisciplinary ?? true) && employeeIds.Count > 0)
            {
                // ONE query for the whole population. IDisciplinaryEligibilityService answers this per
                // employee, which is right for a profile screen and an N+1 here — a 10k-employee
                // revision would issue 10k queries. Same batching rule as the pay ladder and the
                // performance-band resolver.
                //
                // "Active" = non-cancelled and still inside its lifetime, AND flagged
                // AffectsSalaryIncrement. That flag defaults to TRUE (unlike AffectsPromotion /
                // AffectsReward, which are opt-in), so the default behaviour is unchanged — every
                // active case still blocks — but HR can now exempt an individual case without
                // turning the rule off for the whole tenant.
                var day = effectiveDate.Date;
                var ids = await disciplinary.GetAll().AsNoTracking()
                    .Where(d => employeeIds.Contains(d.EmployeeId)
                                && d.AffectsSalaryIncrement
                                && d.Status != DisciplinaryStatus.Cancelled
                                && (d.ValidUntil == null || d.ValidUntil >= day))
                    .Select(d => d.EmployeeId)
                    .Distinct()
                    .ToListAsync(ct);
                blocked = [.. ids];
            }

            return new SalaryIncrementEligibility(policy, blocked);
        }
    }
}
