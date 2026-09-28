using CyberErp.Hrms.App.Common;
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
    /// <para>⚠️ …BUT THE TWO HALVES OVERLAP, so they are MERGED, never added. An
    /// <c>EmployeeExperience</c> row describing the job somebody still holds is ordinary data
    /// entry, and adding its length to their service counted that time twice. Both sources go into
    /// one <see cref="ExperienceSpan"/> merge.</para>
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
            var (totalYears, internalYears, extraYears) =
                await ExperienceYearsAsync(to.PersonId, to.HireDate);

            if (policy.MinDelegateExperienceYears > 0 && totalYears < policy.MinDelegateExperienceYears)
                reasons.Add(
                    $"The delegate has {totalYears:0.#} year(s) of experience "
                    + $"({internalYears:0.#} in service, {extraYears:0.#} prior), below the "
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
        /// The delegate's experience: internal service and prior employment, counted once.
        /// </summary>
        /// <returns>
        /// The total, the internal-service part, and how much the prior rows ADD BEYOND service —
        /// not their raw length. The three always agree, so the sentence shown to a user adds up.
        /// </returns>
        /// <remarks>
        /// <para>⚠️ INTERNAL SERVICE IS JUST ANOTHER PERIOD IN THE SAME MERGE. It used to be added
        /// to the prior-row total, and prior rows were merged only against each other — so an
        /// <c>EmployeeExperience</c> row describing the job the person STILL HOLDS was counted
        /// twice. That is not a corner case: it was the only experience row in the production
        /// database, and it doubled a twenty-year veteran to forty years. Overlap between the two
        /// sources is the normal case, not the exception, and the merge has to see both.</para>
        ///
        /// <para>An open-ended row (no end date) counts to today; a row with no start date cannot
        /// be measured and is skipped; a hire date in the future contributes nothing.</para>
        /// </remarks>
        private async Task<(decimal Total, decimal Internal, decimal Extra)> ExperienceYearsAsync(
            Guid personId, DateTime? hireDate)
        {
            var rows = await experiences.GetAll().AsNoTracking()
                .Where(x => x.PersonId == personId && x.StartDate != null)
                .Select(x => new { Start = x.StartDate!.Value, x.EndDate })
                .ToListAsync();

            return Compose(hireDate, [.. rows.Select(r => (r.Start, r.EndDate))], DateTime.UtcNow.Date);
        }

        /// <summary>
        /// Combine internal service with prior employment rows. Pure, so the composition itself can
        /// be tested — it is the part that was wrong, not the span arithmetic underneath it.
        /// </summary>
        /// <param name="hireDate">Start of current service. Null, or in the future, contributes nothing.</param>
        /// <param name="priorRows">Prior employment. A null end date means "still open" and runs to <paramref name="today"/>.</param>
        public static (decimal Total, decimal Internal, decimal Extra) Compose(
            DateTime? hireDate,
            IReadOnlyList<(DateTime Start, DateTime? End)> priorRows,
            DateTime today)
        {
            var service = hireDate is DateTime hired && hired.Date < today
                ? new List<(DateTime, DateTime)> { (hired.Date, today) }
                : [];

            var prior = priorRows.Select(r => (r.Start.Date, (r.End ?? today).Date));

            var internalYears = decimal.Round(ExperienceSpan.Years(service), 1);

            // ⚠️ ONE merge over BOTH sources. Rounding each side and adding would reintroduce the
            // original fault in miniature, and a prior row overlapping service would inflate again.
            var totalYears = decimal.Round(ExperienceSpan.Years([.. service, .. prior]), 1);

            // What the prior rows ADD. A row that merely restates current employment adds nothing,
            // and saying "0 prior" is the honest reading of it.
            return (totalYears, internalYears, totalYears - internalYears);
        }
    }
}
