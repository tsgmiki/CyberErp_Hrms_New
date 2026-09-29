using CyberErp.Hrms.App.Common;

namespace CyberErp.Hrms.Tests.Documents;

/// <summary>
/// Turning position-change EVENTS into the dated post history an experience letter states.
/// </summary>
/// <remarks>
/// This is a letter an employee hands to a prospective employer over this organisation's
/// signature, so the dates have to survive being read sceptically: no gaps, no overlaps, and no
/// post the person did not hold.
/// </remarks>
public class ServiceHistoryTests
{
    private static DateTime D(int y, int m, int d) => new(y, m, d);

    private static List<ServicePeriod> Build(
        DateTime? hired, string? hiredAs, params PositionChange[] changes) =>
        ServiceHistory.Build(hired, hiredAs, null, changes);

    private static PositionChange Change(DateTime on, string title) => new(on, title, null);

    // ---- the shape the letter asks for ----------------------------------------------------

    /// <summary>The worked example from the request: three posts, contiguous, no overlap.</summary>
    [Fact]
    public void ProducesContiguousPeriodsForAPromotionChain()
    {
        var history = Build(D(2010, 1, 1), "Junior Developer",
            Change(D(2013, 1, 1), "Senior Developer"),
            Change(D(2016, 3, 1), "Lead Developer"));

        Assert.Equal(3, history.Count);

        Assert.Equal(("Junior Developer", D(2010, 1, 1), D(2012, 12, 31)),
            (history[0].Title, history[0].From, history[0].To));
        Assert.Equal(("Senior Developer", D(2013, 1, 1), D(2016, 2, 29)),
            (history[1].Title, history[1].From, history[1].To));
        Assert.Equal(("Lead Developer", D(2016, 3, 1)), (history[2].Title, history[2].From));
    }

    /// <summary>
    /// ⚠️ A movement records the day a post BEGINS. The previous post therefore ends the day
    /// BEFORE — an off-by-one here prints a letter saying somebody held two posts at once, which is
    /// exactly what a sceptical reader checks.
    /// </summary>
    [Fact]
    public void APostEndsTheDayBeforeTheNextBegins()
    {
        var history = Build(D(2020, 1, 1), "Clerk", Change(D(2022, 6, 15), "Senior Clerk"));

        Assert.Equal(D(2022, 6, 14), history[0].To);
        Assert.Equal(D(2022, 6, 15), history[1].From);
    }

    /// <summary>No periods may overlap, whatever the movements looked like.</summary>
    [Fact]
    public void PeriodsNeverOverlap()
    {
        var history = Build(D(2010, 1, 1), "A",
            Change(D(2012, 5, 5), "B"), Change(D(2015, 8, 20), "C"), Change(D(2020, 2, 29), "D"));

        for (var i = 1; i < history.Count; i++)
            Assert.True(history[i - 1].To < history[i].From);
    }

    /// <summary>…and leave no gaps: the letter must cover the whole of the service it attests to.</summary>
    [Fact]
    public void PeriodsLeaveNoGaps()
    {
        var history = Build(D(2010, 1, 1), "A", Change(D(2012, 5, 5), "B"), Change(D(2015, 8, 20), "C"));

        for (var i = 1; i < history.Count; i++)
            Assert.Equal(history[i - 1].To!.Value.AddDays(1), history[i].From);
    }

    // ---- still serving, or left --------------------------------------------------------------

    /// <summary>
    /// ⚠️ Somebody still in post has an OPEN period. Inventing an end date — today, most
    /// temptingly — would state that their employment ended.
    /// </summary>
    [Fact]
    public void TheCurrentPostIsLeftOpen()
    {
        var history = Build(D(2020, 1, 1), "Clerk");

        Assert.Single(history);
        Assert.Null(history[0].To);
    }

    [Fact]
    public void AFinishedServiceClosesOnTheLastWorkingDay()
    {
        var history = ServiceHistory.Build(D(2020, 1, 1), "Clerk", null,
            [Change(D(2022, 1, 1), "Senior Clerk")], serviceEnded: D(2024, 3, 31));

        Assert.Equal(D(2024, 3, 31), history[^1].To);
    }

    /// <summary>Somebody who never moved posts has exactly one period, hire date to today.</summary>
    [Fact]
    public void NoMovementsMeansOnePeriodFromHire()
    {
        var history = Build(D(2015, 7, 1), "Accountant");

        Assert.Single(history);
        Assert.Equal(D(2015, 7, 1), history[0].From);
        Assert.Equal("Accountant", history[0].Title);
    }

    // ---- data that cannot be rendered honestly ------------------------------------------------

    /// <summary>
    /// ⚠️ No hire date, no letter. A "To Whom It May Concern" letter's whole content is when
    /// service began; guessing it from the first movement would state a start nobody recorded.
    /// </summary>
    [Fact]
    public void NoHireDateYieldsNothing()
    {
        Assert.Empty(Build(null, "Clerk", Change(D(2020, 1, 1), "Senior Clerk")));
    }

    /// <summary>
    /// ⚠️ A movement dated on or before the hire date would open a period ending before it began.
    /// Those rows are data errors — a backdated correction, or a movement on the wrong employee —
    /// and are dropped rather than printed.
    /// </summary>
    [Fact]
    public void AMovementBeforeTheHireDateIsIgnored()
    {
        var history = Build(D(2020, 1, 1), "Clerk",
            Change(D(2018, 5, 1), "Ghost"), Change(D(2022, 1, 1), "Senior Clerk"));

        Assert.Equal(2, history.Count);
        Assert.DoesNotContain(history, p => p.Title == "Ghost");
    }

    [Fact]
    public void AMovementOnTheHireDateItselfIsIgnored()
    {
        var history = Build(D(2020, 1, 1), "Clerk", Change(D(2020, 1, 1), "Senior Clerk"));

        Assert.Single(history);
        Assert.Equal("Clerk", history[0].Title);
    }

    /// <summary>Two movements on one day: only the last landed, and a zero-length period is wrong.</summary>
    [Fact]
    public void TwoMovementsOnTheSameDayCollapseToTheLast()
    {
        var history = Build(D(2020, 1, 1), "Clerk",
            Change(D(2022, 1, 1), "Supervisor"), Change(D(2022, 1, 1), "Manager"));

        Assert.Equal(2, history.Count);
        Assert.Equal("Manager", history[1].Title);
        Assert.All(history, p => Assert.True(p.To is null || p.To >= p.From));
    }

    /// <summary>Movements arriving out of order must still produce a chronological history.</summary>
    [Fact]
    public void UnorderedMovementsAreSorted()
    {
        var history = Build(D(2010, 1, 1), "A",
            Change(D(2018, 1, 1), "C"), Change(D(2014, 1, 1), "B"));

        Assert.Equal(["A", "B", "C"], history.Select(p => p.Title));
    }

    /// <summary>
    /// ⚠️ A last working day before the final post began means the movement and the termination
    /// disagree. Clamping states a visibly odd one-day period — better than a period running
    /// backwards, and better than silently dropping a post the person held.
    /// </summary>
    [Fact]
    public void ALastWorkingDayBeforeTheFinalPostIsClampedNotInverted()
    {
        var history = ServiceHistory.Build(D(2020, 1, 1), "Clerk", null,
            [Change(D(2023, 6, 1), "Manager")], serviceEnded: D(2023, 1, 1));

        Assert.All(history, p => Assert.True(p.To is null || p.To >= p.From));
        Assert.Equal(history[^1].From, history[^1].To);
    }

    // ---- Amharic ------------------------------------------------------------------------------

    /// <summary>The Amharic title rides along per period, and stays null when none is recorded.</summary>
    [Fact]
    public void AmharicTitlesArePerPeriod()
    {
        var history = ServiceHistory.Build(D(2010, 1, 1), "Clerk", "ጸሐፊ",
            [new PositionChange(D(2015, 1, 1), "Manager", "ሥራ አስኪያጅ")]);

        Assert.Equal("ጸሐፊ", history[0].TitleAmharic);
        Assert.Equal("ሥራ አስኪያጅ", history[1].TitleAmharic);
    }

    [Fact]
    public void AMissingAmharicTitleStaysNullForTheCallerToDecide()
    {
        var history = Build(D(2010, 1, 1), "Clerk");

        Assert.Null(history[0].TitleAmharic);
    }
}
