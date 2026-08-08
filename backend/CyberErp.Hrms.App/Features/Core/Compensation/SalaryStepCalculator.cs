using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    /// <summary>One rung of a job grade's pay ladder.</summary>
    internal readonly record struct ScaleRung(int Ordinal, decimal Salary);

    /// <summary>Outcome of moving an employee along their grade's ladder.</summary>
    public readonly record struct StepResolution(
        decimal Salary,
        decimal ResolvedStep,
        bool Interpolated,
        bool Capped,
        string? Reason)
    {
        public static StepResolution Unchanged(decimal salary, string reason) =>
            new(salary, 0m, false, false, reason);
    }

    public interface ISalaryScaleLadder
    {
        /// <summary>Salary for <paramref name="currentStep"/> + <paramref name="increment"/> on this grade.</summary>
        StepResolution Resolve(Guid? jobGradeId, int? currentStep, decimal increment, decimal currentSalary);
    }

    public interface ISalaryScaleLadderFactory
    {
        /// <summary>Loads the ladders once for a revision run (all grades, or just the targeted one).</summary>
        Task<ISalaryScaleLadder> BuildAsync(Guid? targetJobGradeId, CancellationToken ct = default);
    }

    /// <summary>
    /// In-memory pay ladders keyed by job grade.
    ///
    /// <para><b>Why the whole grid is loaded up front.</b> A step revision needs, per employee, the two
    /// scale rows bracketing their landing step. Querying that per employee is a textbook N+1: a
    /// 10 000-employee revision would issue 10 000+ round trips. The scale itself is *tiny and
    /// bounded* — it is grades × steps (7 × 10 = 19 rows here; a few hundred at enterprise size) and
    /// changes rarely — so ONE projected, tenant-indexed read gives every lookup that follows for
    /// free. Cost goes from O(employees) queries to exactly 1, and each employee then resolves in
    /// O(log steps) over a sorted array. That is the optimisation; a cleverer per-row SQL join would
    /// still be strictly worse than not going to the database at all.</para>
    /// </summary>
    internal sealed class SalaryScaleLadder(Dictionary<Guid, ScaleRung[]> ladders) : ISalaryScaleLadder
    {
        public StepResolution Resolve(Guid? jobGradeId, int? currentStep, decimal increment, decimal currentSalary)
        {
            if (jobGradeId is null || currentStep is null)
                return StepResolution.Unchanged(currentSalary, "Employee is not on a salary scale.");
            if (!ladders.TryGetValue(jobGradeId.Value, out var rungs) || rungs.Length == 0)
                return StepResolution.Unchanged(currentSalary, "The job grade has no salary scale rows.");

            var target = currentStep.Value + increment;

            // Clamp to the ladder. Landing above the ceiling pays the ceiling — it must never
            // extrapolate past the top of the scale, which would invent a salary the grade has no
            // authority to pay.
            var min = rungs[0].Ordinal;
            var max = rungs[^1].Ordinal;
            var capped = target > max || target < min;
            if (target >= max) return new StepResolution(rungs[^1].Salary, max, false, target > max, target > max ? "Capped at the grade ceiling." : null);
            if (target <= min) return new StepResolution(rungs[0].Salary, min, false, target < min, target < min ? "Floored at the grade base." : null);

            // Bracket the landing point. Binary search over ordinals, so gapped ladders
            // (1,2,3,5,8) interpolate between the two nearest DEFINED rungs rather than assuming
            // every integer exists.
            var i = LowerBound(rungs, target);
            var lo = rungs[i];
            if (lo.Ordinal == target) return new StepResolution(lo.Salary, target, false, false, null);

            var hi = rungs[i + 1];
            var span = hi.Ordinal - lo.Ordinal;             // > 0 by construction
            var fraction = (target - lo.Ordinal) / span;
            var salary = lo.Salary + (hi.Salary - lo.Salary) * fraction;

            return new StepResolution(Math.Round(salary, 2), target, true, capped, null);
        }

        /// <summary>Index of the last rung whose ordinal is &lt;= target (target is strictly inside the ladder).</summary>
        private static int LowerBound(ScaleRung[] rungs, decimal target)
        {
            int lo = 0, hi = rungs.Length - 1;
            while (lo < hi)
            {
                var mid = (lo + hi + 1) / 2;
                if (rungs[mid].Ordinal <= target) lo = mid; else hi = mid - 1;
            }
            return lo;
        }
    }

    public sealed class SalaryScaleLadderFactory(IRepository<SalaryScale> scaleRepository) : ISalaryScaleLadderFactory
    {
        public async Task<ISalaryScaleLadder> BuildAsync(Guid? targetJobGradeId, CancellationToken ct = default)
        {
            var q = scaleRepository.GetAll().AsNoTracking();
            if (targetJobGradeId.HasValue) q = q.Where(s => s.JobGradeId == targetJobGradeId.Value);

            // Projected to three columns so the read is covered by
            // IX_coreSalaryScale_TenantId_JobGradeId_StepId INCLUDE (Salary) — index-only, no lookups.
            // Tenant filtering is applied by the repository; ordering is done in memory because the
            // sort key (Step.Ordinal) lives on the joined lookup and the row count is trivial.
            var rows = await q
                .Select(s => new { s.JobGradeId, Ordinal = s.Step.Ordinal, s.Salary })
                .ToListAsync(ct);

            var ladders = rows
                .GroupBy(r => r.JobGradeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(r => r.Ordinal)                       // defensive: duplicate rungs
                          .Select(x => new ScaleRung(x.Key, x.Max(r => r.Salary)))
                          .OrderBy(r => r.Ordinal)
                          .ToArray());

            return new SalaryScaleLadder(ladders);
        }
    }
}
