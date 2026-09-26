using CyberErp.Hrms.App.Common;

namespace CyberErp.Hrms.Tests.Performance;

/// <summary>
/// Reading an appraisal score as a percentage. The reported bug was a transfer assessment showing
/// <b>1820%</b>: a score of 91 recorded against the live "5 Point Competency Scale", divided by
/// that scale's top ordinal of 5.
/// </summary>
public class AppraisalScoreTests
{
    /// <summary>The live 5-point competency scale: ordinals 1–5, bands merely restating them.</summary>
    private static readonly RatingLevelBounds[] FivePoint =
    [
        new(1, 1m, 1m), new(2, 2m, 2m), new(3, 3m, 3m), new(4, 4m, 4m), new(5, 5m, 5m),
    ];

    /// <summary>The live percentage scale: ordinals 1–4, bands reaching 130.</summary>
    private static readonly RatingLevelBounds[] PercentageBands =
    [
        new(1, 0m, 59m), new(2, 60m, 79m), new(3, 80m, 100m), new(4, 101m, 130m),
    ];

    /// <summary>A scale with no bands at all — the entity's documented "pure numeric" shape.</summary>
    private static readonly RatingLevelBounds[] BandlessFivePoint =
    [
        new(1, null, null), new(2, null, null), new(3, null, null), new(4, null, null), new(5, null, null),
    ];

    // ---- the reported bug ---------------------------------------------------

    [Fact]
    public void TheReportedCase_ScoreOf91OnAFivePointScale_IsRefusedNotReportedAs1820Percent()
    {
        var result = AppraisalScore.ToPercent(91m, FivePoint);

        Assert.Null(result.Percent);
        Assert.NotNull(result.Problem);
        Assert.Contains("91", result.Problem);
        Assert.Contains("1–5", result.Problem);
    }

    /// <summary>
    /// Clamping was the other tempting fix, and it is worse: 100% reads as a top performer, which
    /// is a plausible wrong answer rather than an obviously broken one.
    /// </summary>
    [Fact]
    public void AnOutOfRangeScore_IsNotClampedTo100()
    {
        Assert.Null(AppraisalScore.ToPercent(91m, FivePoint).Percent);
        Assert.Null(AppraisalScore.ToPercent(1000m, FivePoint).Percent);
    }

    // ---- numeric scales -----------------------------------------------------

    [Theory]
    [InlineData(5, 100)]
    [InlineData(4, 80)]
    [InlineData(3, 60)]
    [InlineData(1, 20)]
    public void NumericScale_NormalisesAgainstTheTopOrdinal(int score, int expected)
    {
        Assert.Equal(expected, AppraisalScore.ToPercent(score, FivePoint).Percent);
    }

    [Fact]
    public void NumericScale_WorksWithNoBandsConfiguredAtAll()
    {
        Assert.Equal(60m, AppraisalScore.ToPercent(3m, BandlessFivePoint).Percent);
    }

    [Fact]
    public void NumericScale_AcceptsAFractionalScore()
    {
        // Weighted averages rarely land on a whole level.
        Assert.Equal(74m, AppraisalScore.ToPercent(3.7m, FivePoint).Percent);
    }

    // ---- percentage band scales ---------------------------------------------

    /// <summary>
    /// ⚠️ The case that made one shared implementation necessary. Dividing by max(Value)=4 gives
    /// 2250%; dividing by the band ceiling of 130 gives 69%; the truth is that the score already
    /// IS the percentage.
    /// </summary>
    [Fact]
    public void PercentageScale_TreatsTheScoreAsAlreadyBeingAPercentage()
    {
        Assert.Equal(90m, AppraisalScore.ToPercent(90m, PercentageBands).Percent);
    }

    [Fact]
    public void PercentageScale_AllowsAStretchScoreAbove100()
    {
        // "Stretch Achieved" is defined as 101–130; reporting 120% attainment as 92% would be a lie.
        Assert.Equal(120m, AppraisalScore.ToPercent(120m, PercentageBands).Percent);
    }

    [Fact]
    public void PercentageScale_StillRefusesAScoreBeyondItsTopBand()
    {
        Assert.Null(AppraisalScore.ToPercent(131m, PercentageBands).Percent);
    }

    // ---- range detection ----------------------------------------------------

    [Fact]
    public void RangeOf_RecognisesBandsThatMerelyRestateTheOrdinals_AsNumeric()
    {
        var (low, high, alreadyPercentage) = AppraisalScore.RangeOf(FivePoint);

        Assert.Equal(1m, low);
        Assert.Equal(5m, high);
        Assert.False(alreadyPercentage);
    }

    [Fact]
    public void RangeOf_RecognisesBandsReachingBeyondTheOrdinals_AsAPercentageScale()
    {
        var (low, high, alreadyPercentage) = AppraisalScore.RangeOf(PercentageBands);

        Assert.Equal(0m, low);
        Assert.Equal(130m, high);
        Assert.True(alreadyPercentage);
    }

    // ---- degenerate configuration -------------------------------------------

    [Fact]
    public void NoLevels_ReportsWhyRatherThanDividingByZero()
    {
        var result = AppraisalScore.ToPercent(3m, []);

        Assert.Null(result.Percent);
        Assert.NotNull(result.Problem);
    }

    [Fact]
    public void IsInRange_GuardsScoreEntryOnBothScaleShapes()
    {
        Assert.True(AppraisalScore.IsInRange(5m, FivePoint));
        Assert.False(AppraisalScore.IsInRange(91m, FivePoint));
        Assert.True(AppraisalScore.IsInRange(130m, PercentageBands));
        Assert.False(AppraisalScore.IsInRange(131m, PercentageBands));
    }

    /// <summary>An unconfigured scale must not block somebody saving their scores.</summary>
    [Fact]
    public void IsInRange_WithNoLevels_DoesNotBlockTheSave()
    {
        Assert.True(AppraisalScore.IsInRange(91m, []));
    }
}
