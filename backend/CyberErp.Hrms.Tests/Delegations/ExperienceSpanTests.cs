using CyberErp.Hrms.App.Common;

namespace CyberErp.Hrms.Tests.Delegations;

/// <summary>
/// Experience is a span of somebody's life, not a quantity to be added up.
/// </summary>
/// <remarks>
/// These exist because the delegation seniority rule got this wrong in production: internal
/// service was ADDED to the prior-employment total, and the prior rows were merged only against
/// each other. A row describing the job the person still held was therefore counted twice, and the
/// only experience row in the live database was exactly that — it doubled a twenty-year veteran to
/// forty years. See <see cref="TheJobSomebodyStillHoldsIsNotCountedTwice"/>.
/// </remarks>
public class ExperienceSpanTests
{
    private static DateTime D(int y, int m, int d) => new(y, m, d);

    /// <summary>Years, rounded the way the eligibility message rounds them.</summary>
    private static decimal Years(params (DateTime, DateTime)[] periods) =>
        decimal.Round(ExperienceSpan.Years(periods), 1);

    // ---- the bug this was written for ----------------------------------------------------

    /// <summary>
    /// ⚠️ THE PRODUCTION CASE. Hired 2006-09-06, and an EmployeeExperience row covering
    /// 2006-09-06 → 2026-09-07 — the same employment, recorded twice. Twenty years, not forty.
    /// </summary>
    [Fact]
    public void TheJobSomebodyStillHoldsIsNotCountedTwice()
    {
        var service = (D(2006, 9, 6), D(2026, 9, 28));
        var recordedAgain = (D(2006, 9, 6), D(2026, 9, 7));

        var total = Years(service, recordedAgain);

        Assert.Equal(20.1m, total);
        Assert.Equal(Years(service), total);   // the duplicate row added nothing
    }

    /// <summary>A prior row that runs INTO current service extends it, it does not stack on it.</summary>
    [Fact]
    public void APriorRoleOverlappingServiceExtendsItRatherThanStacking()
    {
        // Prior role 2014→2019, hired 2017: eight years of life, not eleven.
        var total = Years((D(2014, 1, 1), D(2019, 1, 1)), (D(2017, 1, 1), D(2022, 1, 1)));

        Assert.Equal(8.0m, total);
    }

    // ---- merging in general ---------------------------------------------------------------

    /// <summary>Two concurrent part-time posts over three years are three years, not six.</summary>
    [Fact]
    public void ConcurrentRolesCountOnce()
    {
        var a = (D(2020, 1, 1), D(2023, 1, 1));
        var b = (D(2020, 1, 1), D(2023, 1, 1));

        Assert.Equal(Years(a), Years(a, b));
    }

    /// <summary>Genuinely separate employment does add up — the merge must not swallow real gaps.</summary>
    [Fact]
    public void SeparatePeriodsAreStillAdded()
    {
        var total = Years((D(2010, 1, 1), D(2012, 1, 1)), (D(2015, 1, 1), D(2017, 1, 1)));

        Assert.Equal(4.0m, total);
    }

    /// <summary>A role ending the day the next begins is one continuous span, not two abutting ones.</summary>
    [Fact]
    public void TouchingPeriodsMergeIntoOne()
    {
        Assert.Single(ExperienceSpan.Merge([(D(2020, 1, 1), D(2021, 1, 1)), (D(2021, 1, 1), D(2022, 1, 1))]));
    }

    /// <summary>One period wholly inside another contributes nothing of its own.</summary>
    [Fact]
    public void AContainedPeriodAddsNothing()
    {
        var outer = (D(2010, 1, 1), D(2020, 1, 1));
        var inner = (D(2012, 6, 1), D(2014, 6, 1));

        Assert.Equal(Years(outer), Years(outer, inner));
    }

    /// <summary>Order of the rows must not change the answer.</summary>
    [Fact]
    public void OrderDoesNotMatter()
    {
        var a = (D(2015, 1, 1), D(2018, 1, 1));
        var b = (D(2010, 1, 1), D(2013, 1, 1));

        Assert.Equal(Years(a, b), Years(b, a));
    }

    // ---- periods that cannot be counted ---------------------------------------------------

    [Fact]
    public void NoPeriodsIsZero() => Assert.Equal(0m, Years());

    /// <summary>
    /// ⚠️ A period ending before it starts is dropped, never counted negative — bad data must not
    /// be able to SUBTRACT from somebody's experience and push them under a threshold.
    /// </summary>
    [Fact]
    public void AnInvertedPeriodIsDroppedNotCountedNegative()
    {
        var real = (D(2015, 1, 1), D(2020, 1, 1));
        var inverted = (D(2020, 1, 1), D(2010, 1, 1));

        Assert.Equal(Years(real), Years(real, inverted));
    }

    [Fact]
    public void AZeroLengthPeriodContributesNothing()
    {
        Assert.Equal(0m, Years((D(2020, 1, 1), D(2020, 1, 1))));
    }

    /// <summary>Leap years are averaged over the cycle, so four calendar years read as 4.0.</summary>
    [Fact]
    public void FourCalendarYearsSpanningALeapDayReadAsFour()
    {
        Assert.Equal(4.0m, Years((D(2019, 1, 1), D(2023, 1, 1))));
    }

    // ---- Merge itself ----------------------------------------------------------------------

    [Fact]
    public void MergeReturnsTheSpansThatWereCounted()
    {
        var merged = ExperienceSpan.Merge(
        [
            (D(2010, 1, 1), D(2012, 1, 1)),
            (D(2011, 1, 1), D(2013, 1, 1)),   // overlaps the first
            (D(2020, 1, 1), D(2021, 1, 1)),   // separate
        ]);

        Assert.Equal(2, merged.Count);
        Assert.Equal((D(2010, 1, 1), D(2013, 1, 1)), merged[0]);
        Assert.Equal((D(2020, 1, 1), D(2021, 1, 1)), merged[1]);
    }

    [Fact]
    public void MergeReturnsSpansEarliestFirst()
    {
        var merged = ExperienceSpan.Merge([(D(2020, 1, 1), D(2021, 1, 1)), (D(2010, 1, 1), D(2011, 1, 1))]);

        Assert.Equal(D(2010, 1, 1), merged[0].Start);
    }

    /// <summary>Time of day must not leak into the arithmetic — these are dates.</summary>
    [Fact]
    public void TimeOfDayIsIgnored()
    {
        var withTime = (new DateTime(2020, 1, 1, 23, 59, 0), new DateTime(2021, 1, 1, 0, 1, 0));

        Assert.Equal(Years((D(2020, 1, 1), D(2021, 1, 1))), Years(withTime));
    }
}
