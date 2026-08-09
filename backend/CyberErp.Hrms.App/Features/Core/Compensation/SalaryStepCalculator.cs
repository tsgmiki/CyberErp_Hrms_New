using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    /// <summary>One rung of a job grade's pay ladder. <paramref name="ScaleId"/> is the SalaryScale row,
    /// which is what an applied promotion actually writes onto the employee.</summary>
    internal readonly record struct ScaleRung(int Ordinal, decimal Salary, Guid ScaleId = default);

    /// <summary>A grade's ladder plus the identity needed to name it in a promotion note.</summary>
    internal sealed record GradeLadder(Guid GradeId, string Code, ScaleRung[] Rungs)
    {
        public decimal Ceiling => Rungs[^1].Salary;
        public decimal Base => Rungs[0].Salary;
    }

    /// <summary>Outcome of moving an employee along their grade's ladder.</summary>
    public readonly record struct StepResolution(
        decimal Salary,
        decimal ResolvedStep,
        bool Interpolated,
        bool Capped,
        string? Reason)
    {
        /// <summary>Completed months of service at the effective date; null when it could not be assessed.</summary>
        public int? MonthsOfService { get; init; }

        /// <summary>Share of the increment earned: 1 = full, 0.5 = six months into the first year, 0 = excluded.</summary>
        public decimal ProrationFactor { get; init; } = 1m;

        /// <summary>True when the ceiling was cleared by moving the employee onto the next grade up.</summary>
        public bool Promoted { get; init; }

        /// <summary>The SalaryScale row to move the employee onto when applied; null when the grade is unchanged.</summary>
        public Guid? PromotedToScaleId { get; init; }

        /// <summary>Code of the grade they are promoted into, for the note and the grid.</summary>
        public string? PromotedToGradeCode { get; init; }

        public static StepResolution Unchanged(decimal salary, string? reason) =>
            new(salary, 0m, false, false, reason);
    }

    public interface ISalaryScaleLadder
    {
        /// <summary>
        /// Salary for <paramref name="currentStep"/> + <paramref name="increment"/> on this grade.
        /// With <paramref name="promoteOnCeiling"/>, an increment that overshoots the top rung carries
        /// the employee onto the next grade up instead of stopping at the ceiling.
        /// </summary>
        StepResolution Resolve(Guid? jobGradeId, int? currentStep, decimal increment, decimal currentSalary,
            bool promoteOnCeiling = false);
    }

    public interface ISalaryScaleLadderFactory
    {
        /// <summary>
        /// Loads every grade's ladder once for a revision run. <paramref name="targetJobGradeId"/> no
        /// longer narrows the load — promotion needs to see the grades above the employee's own — and
        /// it does not need to: <c>TargetsAsync</c> has already restricted the population to that grade.
        /// </summary>
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
    internal sealed class SalaryScaleLadder(Dictionary<Guid, GradeLadder> ladders) : ISalaryScaleLadder
    {
        /// <summary>
        /// Grades ordered by what they PAY, cheapest first — the sequence a promotion walks.
        ///
        /// <para>JobGrade carries no level or sort order, so "the next grade" has to be derived. Pay is
        /// the only ordering that is both present in the data and safe: grade CODE order does not track
        /// pay in practice (a live tenant has code 001 paying 10,000-12,000 and 002 paying 2,501-5,529),
        /// so following codes would promote people into a pay cut. Ordering by ceiling also drops
        /// grades with no scale rows for free — there is nowhere to place anyone on them.</para>
        /// </summary>
        private readonly GradeLadder[] byPay = [.. ladders.Values
            .OrderBy(g => g.Ceiling).ThenBy(g => g.Base).ThenBy(g => g.GradeId)];

        /// <summary>Single-grade ladders, where no promotion target exists and the code is unused.</summary>
        public SalaryScaleLadder(Dictionary<Guid, ScaleRung[]> rungsByGrade)
            : this(rungsByGrade.ToDictionary(
                kv => kv.Key,
                kv => new GradeLadder(kv.Key, kv.Key.ToString()[..4], kv.Value)))
        { }

        public StepResolution Resolve(Guid? jobGradeId, int? currentStep, decimal increment, decimal currentSalary,
            bool promoteOnCeiling = false)
        {
            if (jobGradeId is null || currentStep is null)
                return StepResolution.Unchanged(currentSalary, "Employee is not on a salary scale.");
            if (!ladders.TryGetValue(jobGradeId.Value, out var grade) || grade.Rungs.Length == 0)
                return StepResolution.Unchanged(currentSalary, "The job grade has no salary scale rows.");

            var rungs = grade.Rungs;
            var target = currentStep.Value + increment;

            // Clamp to the ladder. Landing above the ceiling pays the ceiling — it must never
            // extrapolate past the top of the scale, which would invent a salary the grade has no
            // authority to pay.
            var min = rungs[0].Ordinal;
            var max = rungs[^1].Ordinal;
            var capped = target > max || target < min;
            if (target > max && promoteOnCeiling)
            {
                var promoted = Promote(grade, target - max, currentSalary);
                if (promoted is not null) return promoted.Value;
                // No grade above this one, or none that pays more than they already earn: fall through
                // and cap, which is the honest outcome rather than a promotion that costs them money.
            }
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

        /// <summary>
        /// Moves an employee who has run off the top of their grade onto the next grade up.
        ///
        /// <para><paramref name="overshoot"/> is how far past the ceiling the increment reached. The
        /// promotion itself consumes one of those steps — "one more step" from the top of a grade means
        /// the bottom of the next one — and any remainder is spent climbing the new ladder, so the
        /// increment the employee earned is not silently discarded at the grade boundary.</para>
        ///
        /// <para>Returns null when there is no grade above, or when the landing rung would not actually
        /// pay more than the employee earns today. Grade bands can overlap, so a promotion is only a
        /// promotion if the money moves; the caller then caps at the ceiling instead.</para>
        /// </summary>
        private StepResolution? Promote(GradeLadder from, decimal overshoot, decimal currentSalary)
        {
            var next = byPay.FirstOrDefault(g => g.Ceiling > from.Ceiling && g.GradeId != from.GradeId);
            if (next is null) return null;

            // Index-based, not ordinal arithmetic: ladders can be gapped (1,2,3,5,8), so "one rung up"
            // is the next DEFINED rung rather than ordinal + 1.
            var index = (int)Math.Floor(overshoot) - 1;
            if (index < 0) index = 0;
            if (index > next.Rungs.Length - 1) index = next.Rungs.Length - 1;

            // Never land on a rung that pays the same or less than today — climb until it does.
            while (index < next.Rungs.Length && next.Rungs[index].Salary <= currentSalary) index++;
            if (index >= next.Rungs.Length) return null;

            var rung = next.Rungs[index];
            return new StepResolution(rung.Salary, rung.Ordinal, false, false,
                $"Promoted to grade {next.Code} step {rung.Ordinal} on reaching the ceiling of grade {from.Code}.")
            {
                Promoted = true,
                PromotedToScaleId = rung.ScaleId,
                PromotedToGradeCode = next.Code
            };
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
            // NOT filtered to the target grade any more. A ceiling promotion has to see the grade ABOVE
            // the employee's own, so the ladder set must span every grade even when the revision itself
            // is aimed at one. The scale is grades x steps — tens of rows here, a few hundred at
            // enterprise size — so loading all of it stays one small query either way.
            var rows = await scaleRepository.GetAll().AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.JobGradeId,
                    GradeCode = s.JobGrade.Code,
                    Ordinal = s.Step.Ordinal,
                    s.Salary
                })
                .ToListAsync(ct);

            var ladders = rows
                .GroupBy(r => r.JobGradeId)
                .ToDictionary(
                    g => g.Key,
                    g => new GradeLadder(
                        g.Key,
                        g.First().GradeCode ?? string.Empty,
                        [.. g.GroupBy(r => r.Ordinal)                    // defensive: duplicate rungs
                             .Select(x => x.OrderByDescending(r => r.Salary).First())
                             .Select(x => new ScaleRung(x.Ordinal, x.Salary, x.Id))
                             .OrderBy(r => r.Ordinal)]));

            return new SalaryScaleLadder(ladders);
        }
    }
}

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    /// <summary>A resolved band for one employee.</summary>
    public readonly record struct PerformanceAward(decimal? Score, decimal Value, string? BandLabel, string? Reason);

    /// <summary>Score bands + the appraisal scores of the targeted population, resolved once per run.</summary>
    public interface IPerformanceAwardResolver
    {
        /// <summary>Band value for this employee, or a reason why none applies.</summary>
        PerformanceAward Resolve(Guid employeeId);
        /// <summary>Lowest/highest score actually seen — lets the UI expose a mis-scaled band set.</summary>
        (decimal? Min, decimal? Max) ObservedScoreRange { get; }
        int ScoredEmployeeCount { get; }
    }

    public interface IPerformanceAwardResolverFactory
    {
        Task<IPerformanceAwardResolver> BuildAsync(
            IReadOnlyCollection<Guid> employeeIds,
            IReadOnlyCollection<(decimal MinScore, decimal Value, string? Label)> bands,
            Guid? reviewCycleId,
            CancellationToken ct = default);
    }

    internal sealed class PerformanceAwardResolver(
        Dictionary<Guid, decimal> scores,
        (decimal MinScore, decimal Value, string? Label)[] bandsDesc) : IPerformanceAwardResolver
    {
        public (decimal? Min, decimal? Max) ObservedScoreRange =>
            scores.Count == 0 ? (null, null) : (scores.Values.Min(), scores.Values.Max());

        public int ScoredEmployeeCount => scores.Count;

        public PerformanceAward Resolve(Guid employeeId)
        {
            // No completed appraisal is NOT the same as a low score: awarding the bottom band would
            // quietly hand out (or withhold) money based on missing data. Leave them out and report it.
            if (!scores.TryGetValue(employeeId, out var score))
                return new PerformanceAward(null, 0m, null, "No completed appraisal score for this employee.");

            // Bands are held highest-first, so the first threshold at or below the score wins.
            foreach (var b in bandsDesc)
                if (score >= b.MinScore)
                    return new PerformanceAward(score, b.Value, b.Label, null);

            return new PerformanceAward(score, 0m,  null,
                $"Score {score} is below every band threshold; no award.");
        }
    }

    public sealed class PerformanceAwardResolverFactory(IRepository<Appraisal> appraisals)
        : IPerformanceAwardResolverFactory
    {
        public async Task<IPerformanceAwardResolver> BuildAsync(
            IReadOnlyCollection<Guid> employeeIds,
            IReadOnlyCollection<(decimal MinScore, decimal Value, string? Label)> bands,
            Guid? reviewCycleId,
            CancellationToken ct = default)
        {
            var bandsDesc = bands.OrderByDescending(b => b.MinScore).ToArray();
            if (employeeIds.Count == 0)
                return new PerformanceAwardResolver([], bandsDesc);

            // ONE query for the whole population. Scoring each employee with their own query would be
            // an N+1 (10k employees -> 10k round trips); this projects just the three columns needed
            // and picks the latest completed appraisal per employee in memory, which is cheap because
            // the row count is bounded by employees x cycles, not by anything unbounded.
            var q = appraisals.GetAll().AsNoTracking()
                .Where(a => employeeIds.Contains(a.EmployeeId)
                            && a.CompletedAt != null
                            && a.OverallScore != null);
            if (reviewCycleId.HasValue)
                q = q.Where(a => a.ReviewCycleId == reviewCycleId.Value);

            var rows = await q
                .Select(a => new { a.EmployeeId, a.OverallScore, a.CompletedAt })
                .ToListAsync(ct);

            var scores = rows
                .GroupBy(r => r.EmployeeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CompletedAt!.Value).First().OverallScore!.Value);

            return new PerformanceAwardResolver(scores, bandsDesc);
        }
    }
}
