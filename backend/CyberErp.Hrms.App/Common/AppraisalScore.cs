namespace CyberErp.Hrms.App.Common
{
    /// <summary>The bounds one rating-scale level contributes. Mirrors <c>RatingScaleLevel</c>.</summary>
    /// <param name="Value">The ordinal level (1, 2, 3…).</param>
    /// <param name="MinScore">Inclusive band floor, or null on a pure numeric level.</param>
    /// <param name="MaxScore">Inclusive band ceiling, or null on a pure numeric level.</param>
    public readonly record struct RatingLevelBounds(int Value, decimal? MinScore, decimal? MaxScore);

    /// <summary>
    /// An appraisal score expressed as a percentage — or the reason it could not be.
    /// </summary>
    /// <param name="Percent">
    /// null when the score cannot be interpreted against its scale. May legitimately exceed 100 on a
    /// stretch scale whose top band does (e.g. "Stretch Achieved, 101–130").
    /// </param>
    /// <param name="Problem">A sentence fit to show a user, or null when <paramref name="Percent"/> is set.</param>
    public readonly record struct ScorePercent(decimal? Percent, string? Problem);

    /// <summary>
    /// Turns an appraisal's overall score into a percentage, against the rating scale it was scored on.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ THIS EXISTS BECAUSE FIVE CALLERS DISAGREED. Transfer assessment, training suggestions and
    /// reward auto-grant divided by <c>max(Value)</c>; the two career-development handlers divided by
    /// <c>max(MaxScore)</c> and clamped. On a pure 1–5 scale those agree. On the "Percentage Goal
    /// Attainment" scale in the live data — ordinals 1–4, bands 0–130 — they differ by a factor of
    /// thirty: a score of 100 reads 2500% one way and 76.9% the other. One employee could show a
    /// different performance figure on every screen that mentioned it. There is now one answer.</para>
    ///
    /// <para>⚠️ AND AN OUT-OF-RANGE SCORE IS NOT CLAMPED. Clamping 1820% to 100% swaps an obviously
    /// broken number for a plausible wrong one — it reads as "top performer" when the truth is that
    /// the score does not belong to the scale it was recorded against. Callers get null and a reason
    /// to show, which is the only answer that leads anybody to the actual problem.</para>
    /// </remarks>
    public static class AppraisalScore
    {
        /// <summary>
        /// The usable score range of a scale, and whether the score is ALREADY a percentage.
        /// </summary>
        /// <remarks>
        /// ⚠️ The entity documents <c>MinScore</c>/<c>MaxScore</c> as "inclusive score band
        /// (percentage scales) — null for pure numeric levels", but the seeded numeric scales fill
        /// them in anyway, level 3 carrying the band 3–3. So "has bands" cannot mean "is a
        /// percentage scale". What distinguishes them is that a real percentage scale's bands reach
        /// BEYOND its ordinals (1–4 with bands to 130), while a numeric scale's merely restate them
        /// (1–5 with bands to 5).
        /// </remarks>
        public static (decimal Low, decimal High, bool AlreadyPercentage) RangeOf(
            IReadOnlyCollection<RatingLevelBounds> levels)
        {
            var valueLow = levels.Min(l => (decimal)l.Value);
            var valueHigh = levels.Max(l => (decimal)l.Value);

            var bandHighs = levels.Where(l => l.MaxScore.HasValue).Select(l => l.MaxScore!.Value).ToList();
            if (bandHighs.Count > 0)
            {
                var bandHigh = bandHighs.Max();
                if (bandHigh > valueHigh)
                {
                    var bandLows = levels.Where(l => l.MinScore.HasValue).Select(l => l.MinScore!.Value).ToList();
                    return (bandLows.Count > 0 ? bandLows.Min() : 0m, bandHigh, true);
                }
            }

            return (valueLow, valueHigh, false);
        }

        /// <summary>
        /// Express <paramref name="score"/> as a percentage of the scale described by
        /// <paramref name="levels"/>.
        /// </summary>
        public static ScorePercent ToPercent(decimal score, IReadOnlyCollection<RatingLevelBounds> levels)
        {
            if (levels.Count == 0)
                return new ScorePercent(null, "No rating scale is configured for this review cycle.");

            var (low, high, alreadyPercentage) = RangeOf(levels);
            if (high <= 0 || high < low)
                return new ScorePercent(null, "This review cycle's rating scale has no usable score range.");

            if (score < low || score > high)
                return new ScorePercent(null,
                    $"The recorded appraisal score ({score:0.##}) is outside its rating scale ({low:0.##}–{high:0.##}), "
                    + "so it cannot be read as a percentage. Re-score the appraisal against the scale its cycle uses.");

            // A percentage scale's score IS the percentage. Dividing it by the band ceiling again
            // would understate every result — 100% attainment would report as 77%.
            return new ScorePercent(
                alreadyPercentage ? decimal.Round(score, 1) : decimal.Round(score / high * 100m, 1),
                null);
        }

        /// <summary>
        /// Is a single line score (a goal or competency rating) valid on this scale? Used at entry,
        /// which is the only place that can stop a bad score being stored in the first place.
        /// </summary>
        public static bool IsInRange(decimal score, IReadOnlyCollection<RatingLevelBounds> levels)
        {
            if (levels.Count == 0) return true;   // nothing to check against; don't block the save
            var (low, high, _) = RangeOf(levels);
            return score >= low && score <= high;
        }
    }
}
