using CyberErp.Hrms.App.Features.Core.Delegations;

namespace CyberErp.Hrms.Tests.Delegations;

/// <summary>
/// How the delegation seniority rule counts a stand-in's experience.
/// </summary>
/// <remarks>
/// <para>⚠️ THE REGRESSION THESE EXIST FOR. Internal service used to be ADDED to the prior-
/// employment total, and prior rows were merged only against each other. An
/// <c>EmployeeExperience</c> row describing the job the person STILL HOLDS was therefore counted
/// twice — and that was not hypothetical: it was the only experience row in the production
/// database, and it read a twenty-year veteran as forty years.</para>
///
/// <para>It only ever made somebody MORE eligible, so nothing was refused wrongly; but the figure
/// shown on the eligibility preview was wrong, and it would have started admitting under-qualified
/// stand-ins the moment HR raised the threshold.</para>
/// </remarks>
public class DelegationExperienceTests
{
    private static DateTime D(int y, int m, int d) => new(y, m, d);
    private static readonly DateTime Today = D(2026, 9, 28);

    private static (decimal Total, decimal Internal, decimal Extra) Compose(
        DateTime? hireDate, params (DateTime Start, DateTime? End)[] prior) =>
        DelegationEligibilityService.Compose(hireDate, prior, Today);

    // ---- the production case -----------------------------------------------------------

    /// <summary>
    /// ⚠️ Getaneh's actual record: hired 2006-09-06, with an experience row covering
    /// 2006-09-06 → 2026-09-07 — the same employment, entered twice. Twenty years, not forty.
    /// </summary>
    [Fact]
    public void AnExperienceRowRestatingCurrentServiceDoesNotDoubleTheYears()
    {
        var r = Compose(D(2006, 9, 6), (D(2006, 9, 6), D(2026, 9, 7)));

        Assert.Equal(20.1m, r.Total);
        Assert.Equal(20.1m, r.Internal);
        Assert.Equal(0m, r.Extra);     // the duplicate row adds nothing, and says so
    }

    /// <summary>An open-ended row for the current job is the same mistake, and must behave the same.</summary>
    [Fact]
    public void AnOpenEndedRowForTheCurrentJobDoesNotDoubleTheYears()
    {
        var r = Compose(D(2006, 9, 6), (D(2006, 9, 6), null));

        Assert.Equal(20.1m, r.Total);
        Assert.Equal(0m, r.Extra);
    }

    // ---- the breakdown shown to the user ------------------------------------------------

    /// <summary>
    /// ⚠️ The three figures go into one sentence — "N years (X in service, Y prior)" — so they have
    /// to add up. `Extra` is what the prior rows CONTRIBUTE, never their raw length.
    /// </summary>
    [Theory]
    [InlineData(2006, 9, 6)]   // long service, duplicate row
    [InlineData(2024, 1, 1)]   // recent hire with real prior experience
    [InlineData(2026, 9, 1)]   // hired weeks ago
    public void TheBreakdownAlwaysAddsUp(int y, int m, int d)
    {
        var r = Compose(D(y, m, d), (D(2015, 1, 1), D(2020, 1, 1)), (D(2019, 1, 1), null));

        Assert.Equal(r.Total, r.Internal + r.Extra);
    }

    /// <summary>Genuinely prior employment still counts — the fix must not swallow real experience.</summary>
    [Fact]
    public void EarlierEmploymentElsewhereStillCounts()
    {
        // Five years elsewhere, then hired here two years ago.
        var r = Compose(D(2024, 9, 28), (D(2015, 1, 1), D(2020, 1, 1)));

        Assert.Equal(2.0m, r.Internal);
        Assert.Equal(5.0m, r.Extra);
        Assert.Equal(7.0m, r.Total);
    }

    /// <summary>
    /// The reason prior experience is counted at all: a senior hire in their first year is often
    /// exactly the person asked to cover a post, and internal service alone would rule them out.
    /// </summary>
    [Fact]
    public void ASeniorHireInTheirFirstYearClearsATwoYearRule()
    {
        var r = Compose(D(2026, 6, 1), (D(2008, 1, 1), D(2026, 5, 31)));

        Assert.True(r.Internal < 1m);
        Assert.True(r.Total > 2m);
    }

    /// <summary>A prior role running into current service extends it; it does not stack on it.</summary>
    [Fact]
    public void APriorRoleOverlappingServiceIsCountedOnce()
    {
        // Row runs 2020 → 2025; hired 2023. Total span 2020 → today, not 5 + 3.
        var r = Compose(D(2023, 9, 28), (D(2020, 9, 28), D(2025, 9, 28)));

        Assert.Equal(6.0m, r.Total);
        Assert.Equal(3.0m, r.Internal);
        Assert.Equal(3.0m, r.Extra);
    }

    // ---- records that cannot be measured -------------------------------------------------

    [Fact]
    public void NoHireDateAndNoRowsIsZero()
    {
        var r = Compose(null);

        Assert.Equal(0m, r.Total);
        Assert.Equal(0m, r.Internal);
        Assert.Equal(0m, r.Extra);
    }

    /// <summary>Prior experience alone counts for somebody with no hire date recorded.</summary>
    [Fact]
    public void PriorExperienceCountsWithoutAHireDate()
    {
        var r = Compose(null, (D(2016, 9, 28), D(2020, 9, 28)));

        Assert.Equal(0m, r.Internal);
        Assert.Equal(4.0m, r.Total);
    }

    /// <summary>⚠️ A future hire date is not negative service — somebody starting next month has none.</summary>
    [Fact]
    public void AFutureHireDateContributesNothing()
    {
        var r = Compose(D(2026, 12, 1));

        Assert.Equal(0m, r.Internal);
        Assert.Equal(0m, r.Total);
    }

    /// <summary>Hired today: no service yet, and nothing negative.</summary>
    [Fact]
    public void HiredTodayIsZeroNotNegative()
    {
        Assert.Equal(0m, Compose(Today).Total);
    }

    /// <summary>
    /// ⚠️ Bad data must not SUBTRACT from somebody's experience and push them under a threshold.
    /// </summary>
    [Fact]
    public void AnInvertedPriorRowCannotReduceTheTotal()
    {
        var clean = Compose(D(2020, 9, 28));
        var withJunk = Compose(D(2020, 9, 28), (D(2015, 1, 1), D(2010, 1, 1)));

        Assert.Equal(clean.Total, withJunk.Total);
    }

    /// <summary>Several overlapping prior rows at one employer are one span, as before the fix.</summary>
    [Fact]
    public void OverlappingPriorRowsStillMergeAmongThemselves()
    {
        var r = Compose(null,
            (D(2010, 9, 28), D(2015, 9, 28)),
            (D(2012, 9, 28), D(2014, 9, 28)),
            (D(2014, 9, 28), D(2016, 9, 28)));

        Assert.Equal(6.0m, r.Total);
    }
}
