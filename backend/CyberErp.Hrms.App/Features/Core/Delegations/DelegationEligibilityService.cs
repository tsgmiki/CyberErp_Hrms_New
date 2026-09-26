using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    /// <summary>Why a proposed delegate may or may not hold somebody's approval authority.</summary>
    /// <param name="IsEligible">False when any rule failed.</param>
    /// <param name="Reasons">One sentence per failed rule, fit to show a user.</param>
    /// <param name="DelegateExperienceYears">Total experience found, for display.</param>
    /// <param name="SalaryRatioPercent">Delegate salary as a percentage of the delegator's, or null when unknown.</param>
    public record DelegationEligibilityResult(
        bool IsEligible,
        IReadOnlyList<string> Reasons,
        decimal DelegateExperienceYears,
        decimal? SalaryRatioPercent);

    public interface IDelegationEligibilityService
    {
        /// <summary>Evaluate the seniority rules for one delegator/delegate pair.</summary>
        Task<DelegationEligibilityResult> EvaluateAsync(Guid fromEmployeeId, Guid toEmployeeId);

        /// <summary>The tenant's policy, creating the shipped defaults on first use.</summary>
        Task<DelegationPolicy> GetPolicyAsync();
    }

    /// <summary>
    /// Decides whether a proposed delegate is senior enough to hold another approver's authority.
    /// </summary>
    /// <remarks>
    /// <para>This is where delegation meets the employee record the client asked it to respect:
    /// <b>Employee Experience</b> and <b>Salary</b>.</para>
    ///
    /// <para>⚠️ EXPERIENCE IS BOTH HALVES. Internal service (from <c>Employee.HireDate</c>) plus
    /// prior <c>EmployeeExperience</c> rows. Counting only internal service would rule out an
    /// experienced senior hire in their first year — exactly the person most likely to be asked to
    /// stand in — while counting only prior experience would ignore a twenty-year veteran who has
    /// never worked anywhere else.</para>
    ///
    /// <para>⚠️ SALARY, NOT JOB GRADE. "Within N grades" is the obvious rule and it is not
    /// computable here: <c>JobGrade</c> carries a name and a code and no rank at all, so grades
    /// cannot be ordered or subtracted. Salary is the ordered measure this schema actually has, and
    /// it is what a grade ultimately expresses (<c>SalaryScale.Salary</c> per grade and step).</para>
    ///
    /// <para>⚠️ A MISSING FIGURE NEVER SILENTLY PASSES A RULE. If either salary is absent the ratio
    /// cannot be computed, and the rule reports that rather than treating the unknown as a pass —
    /// the failure mode of the opposite choice is that incomplete records are the easiest ones to
    /// delegate through.</para>
    /// </remarks>
    public class DelegationEligibilityService(
        IRepository<Employee> employees,
        IRepository<EmployeeExperience> experiences,
        IRepository<DelegationPolicy> policies) : IDelegationEligibilityService
    {
        public async Task<DelegationPolicy> GetPolicyAsync()
        {
            var existing = await policies.GetAll().FirstOrDefaultAsync();
            if (existing is not null) return existing;

            // First use in this tenant: persist the shipped defaults so the policy is visible and
            // editable rather than an invisible set of constants.
            var created = DelegationPolicy.CreateDefault();
            await policies.AddAsync(created);
            await policies.SaveChangesAsync();
            return created;
        }

        public async Task<DelegationEligibilityResult> EvaluateAsync(Guid fromEmployeeId, Guid toEmployeeId)
        {
            var policy = await GetPolicyAsync();

            var pair = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == fromEmployeeId || e.Id == toEmployeeId)
                .Select(e => new { e.Id, e.PersonId, e.HireDate, e.Salary, e.IsManagerial })
                .ToListAsync();

            var from = pair.FirstOrDefault(e => e.Id == fromEmployeeId);
            var to = pair.FirstOrDefault(e => e.Id == toEmployeeId);

            var reasons = new List<string>();
            if (from is null) reasons.Add("The delegating approver was not found.");
            if (to is null) reasons.Add("The proposed delegate was not found.");
            if (from is null || to is null)
                return new DelegationEligibilityResult(false, reasons, 0m, null);

            // ---- Experience: internal service + prior employment ------------------------------
            var priorYears = await PriorExperienceYearsAsync(to.PersonId);
            var internalYears = YearsSince(to.HireDate);
            var totalYears = decimal.Round(internalYears + priorYears, 1);

            if (policy.MinDelegateExperienceYears > 0 && totalYears < policy.MinDelegateExperienceYears)
                reasons.Add(
                    $"The delegate has {totalYears:0.#} year(s) of experience "
                    + $"({internalYears:0.#} in service, {priorYears:0.#} prior), below the "
                    + $"{policy.MinDelegateExperienceYears} year(s) this policy requires.");

            // ---- Salary parity -----------------------------------------------------------------
            decimal? ratio = null;
            if (policy.MinSalaryRatioPercent > 0)
            {
                if (from.Salary is not decimal fromSalary || fromSalary <= 0m || to.Salary is not decimal toSalary)
                {
                    reasons.Add("Salary is not recorded for both employees, so the seniority rule "
                        + "cannot be checked. Record their salaries, or relax the salary rule in the delegation policy.");
                }
                else
                {
                    ratio = decimal.Round(toSalary / fromSalary * 100m, 1);
                    if (ratio < policy.MinSalaryRatioPercent)
                        reasons.Add(
                            $"The delegate's salary is {ratio:0.#}% of the approver's, below the "
                            + $"{policy.MinSalaryRatioPercent}% this policy requires.");
                }
            }

            // ---- Managerial parity --------------------------------------------------------------
            if (policy.RequireManagerialDelegate && from.IsManagerial && !to.IsManagerial)
                reasons.Add("The approver holds a managerial post, so the delegate must hold one too.");

            return new DelegationEligibilityResult(reasons.Count == 0, reasons, totalYears, ratio);
        }

        /// <summary>
        /// Years of prior employment from <c>EmployeeExperience</c>.
        /// </summary>
        /// <remarks>
        /// ⚠️ Overlapping engagements are NOT summed twice — the rows are merged into continuous
        /// intervals first. Two concurrent part-time posts over the same three years are three
        /// years of experience, not six, and people with several roles at one employer routinely
        /// have overlapping rows. An open-ended row (no end date) counts to today; a row with no
        /// start date cannot be measured and is skipped.
        /// </remarks>
        private async Task<decimal> PriorExperienceYearsAsync(Guid personId)
        {
            var rows = await experiences.GetAll().AsNoTracking()
                .Where(x => x.PersonId == personId && x.StartDate != null)
                .Select(x => new { Start = x.StartDate!.Value, x.EndDate })
                .ToListAsync();
            if (rows.Count == 0) return 0m;

            var today = DateTime.UtcNow.Date;
            var intervals = rows
                .Select(r => (Start: r.Start.Date, End: (r.EndDate ?? today).Date))
                .Where(r => r.End > r.Start)
                .OrderBy(r => r.Start)
                .ToList();
            if (intervals.Count == 0) return 0m;

            var merged = new List<(DateTime Start, DateTime End)>();
            var current = intervals[0];
            foreach (var next in intervals.Skip(1))
            {
                if (next.Start <= current.End)
                    current = (current.Start, next.End > current.End ? next.End : current.End);
                else
                {
                    merged.Add(current);
                    current = next;
                }
            }
            merged.Add(current);

            var days = merged.Sum(m => (m.End - m.Start).TotalDays);
            return (decimal)(days / 365.25);
        }

        private static decimal YearsSince(DateTime? from)
        {
            if (from is not DateTime start) return 0m;
            var days = (DateTime.UtcNow.Date - start.Date).TotalDays;
            return days <= 0 ? 0m : (decimal)(days / 365.25);
        }
    }
}
