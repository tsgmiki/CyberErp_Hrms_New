using CyberErp.Hrms.App.Features.Core.Leaves;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Leaves;

/// <summary>
/// The return-from-leave rules: classification, the comment requirement, and the ledger-safe lifecycle.
///
/// <para>The central rule is that the LEDGER ONLY MOVES ON AN APPROVED DECISION. An early return does
/// not credit anything until an approver accepts it, and a rejected adjustment leaves the balance
/// exactly as it was — so the balance is always the sum of decisions somebody actually made.</para>
/// </summary>
public class AnnualLeaveReturnTests
{
    private static readonly Guid Header = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime PlannedEnd = new(2026, 8, 14);

    private static AnnualLeaveReturn Confirm(decimal approved, decimal actual, string? comment = "because",
        DateTime? actualEnd = null) =>
        AnnualLeaveReturn.Create(Header, PlannedEnd, actualEnd ?? PlannedEnd, approved, actual, comment);

    // ---- Classification -----------------------------------------------------

    [Fact]
    public void Returning_exactly_as_approved_is_on_time_and_needs_no_approval()
    {
        var r = Confirm(approved: 5m, actual: 5m, comment: null);

        Assert.Equal(AnnualLeaveReturnType.OnTime, r.ReturnType);
        Assert.Equal(AnnualLeaveReturnStatus.Approved, r.Status);
        Assert.Equal(0m, r.AdjustmentDays);
    }

    [Fact]
    public void Returning_early_is_a_negative_adjustment_awaiting_approval()
    {
        // The scenario from the spec: asked for 5, came back after 3.
        var r = Confirm(approved: 5m, actual: 3m);

        Assert.Equal(AnnualLeaveReturnType.Early, r.ReturnType);
        Assert.Equal(-2m, r.AdjustmentDays);
        Assert.Equal(AnnualLeaveReturnStatus.PendingApproval, r.Status);
    }

    [Fact]
    public void Returning_late_is_a_positive_adjustment_awaiting_approval()
    {
        var r = Confirm(approved: 5m, actual: 7m);

        Assert.Equal(AnnualLeaveReturnType.Late, r.ReturnType);
        Assert.Equal(2m, r.AdjustmentDays);
        Assert.Equal(AnnualLeaveReturnStatus.PendingApproval, r.Status);
    }

    [Fact]
    public void Half_days_survive_the_comparison()
    {
        // 4.5 approved, 4 taken — a half-day difference is still an early return, not a rounding artefact.
        var r = Confirm(approved: 4.5m, actual: 4m);

        Assert.Equal(AnnualLeaveReturnType.Early, r.ReturnType);
        Assert.Equal(-0.5m, r.AdjustmentDays);
    }

    // ---- Counting the days actually taken -----------------------------------

    /// <summary>Mon–Fri working week, no holidays — enough to prove the overrun is counted properly.</summary>
    private sealed class MonFriCalendar : IWorkingCalendar
    {
        public Task<decimal> CountWorkingDaysAsync(DateTime startDate, DateTime endDate, bool halfDay = false)
        {
            decimal n = 0;
            for (var d = startDate.Date; d <= endDate.Date; d = d.AddDays(1))
                if (d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) n++;
            return Task.FromResult(halfDay ? n / 2 : n);
        }
        public Task<bool> IsWorkingDayAsync(DateTime date) =>
            Task.FromResult(date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday));
        public Task<IReadOnlyList<DateTime>> GetNonWorkingDaysAsync(DateTime startDate, DateTime endDate) =>
            Task.FromResult<IReadOnlyList<DateTime>>([]);
    }

    /// <summary>Mon 10 Aug 2026 → Fri 14 Aug 2026, five working days.</summary>
    private static AnnualLeaveDetail[] FiveDayWeek() =>
        [AnnualLeaveDetail.Create(Header, AnnualLeaveUsage.FullDay,
            new DateTime(2026, 8, 10), new DateTime(2026, 8, 14), 5m)];

    private static decimal ActualDays(DateTime returnedOn) =>
        AnnualLeaveReturnShared.ActualDaysAsync(new MonFriCalendar(), FiveDayWeek(), returnedOn).Result;

    [Fact]
    public void Returning_on_the_approved_last_day_counts_the_approved_days()
    {
        Assert.Equal(5m, ActualDays(new DateTime(2026, 8, 14)));
    }

    [Fact]
    public void Returning_early_counts_only_the_days_up_to_the_return()
    {
        Assert.Equal(3m, ActualDays(new DateTime(2026, 8, 12)));   // Mon–Wed
    }

    [Fact]
    public void Returning_late_counts_the_overrun_beyond_the_approved_range()
    {
        // REGRESSION: the first implementation stopped at the approved detail rows, so a late return
        // reported the approved total and every overrun silently cost nothing.
        Assert.Equal(7m, ActualDays(new DateTime(2026, 8, 18)));   // + Mon 17, Tue 18
    }

    [Fact]
    public void The_weekend_inside_an_overrun_is_not_charged()
    {
        // Back on Monday after the approved Friday: Sat and Sun are free, so it costs exactly one day.
        Assert.Equal(6m, ActualDays(new DateTime(2026, 8, 17)));
    }

    [Fact]
    public void A_half_day_row_the_return_lands_inside_keeps_its_half()
    {
        // A half day is atomic: it was taken, so it counts, and the calendar must not re-count it as 1.
        var details = new[]
        {
            AnnualLeaveDetail.Create(Header, AnnualLeaveUsage.HalfDay,
                new DateTime(2026, 8, 10), new DateTime(2026, 8, 10), 0.5m, HalfDayPart.Morning)
        };

        var days = AnnualLeaveReturnShared
            .ActualDaysAsync(new MonFriCalendar(), details, new DateTime(2026, 8, 10)).Result;

        Assert.Equal(0.5m, days);
    }

    // ---- The comment rule ---------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void An_adjusted_return_must_be_explained(string? comment)
    {
        // The comment is what the approver reads to judge it, so an unexplained adjustment is refused.
        Assert.Throws<ArgumentException>(() => Confirm(approved: 5m, actual: 3m, comment: comment));
    }

    [Fact]
    public void An_on_time_return_needs_no_explanation()
    {
        var r = Confirm(approved: 5m, actual: 5m, comment: null);

        Assert.Null(r.Comment);
    }

    [Fact]
    public void The_comment_is_trimmed()
    {
        Assert.Equal("family emergency", Confirm(5m, 3m, "  family emergency  ").Comment);
    }

    // ---- Lifecycle ----------------------------------------------------------

    [Fact]
    public void An_approved_adjustment_cannot_be_actioned_twice()
    {
        var r = Confirm(5m, 3m);
        r.Approve();

        Assert.Throws<InvalidOperationException>(() => r.Approve());
        Assert.Throws<InvalidOperationException>(() => r.Reject());
    }

    [Fact]
    public void An_on_time_return_is_already_settled_so_cannot_be_approved_again()
    {
        // It never went to an approver, so there is no decision left to make.
        Assert.Throws<InvalidOperationException>(() => Confirm(5m, 5m, null).Approve());
    }

    [Fact]
    public void The_snapshot_of_what_was_approved_is_kept()
    {
        // Stored, not derived: the approver signed off on THESE numbers, and a later edit to the
        // request must not rewrite what they saw.
        var r = Confirm(approved: 5m, actual: 3m, actualEnd: new DateTime(2026, 8, 12));

        Assert.Equal(PlannedEnd, r.PlannedEndDate);
        Assert.Equal(new DateTime(2026, 8, 12), r.ActualEndDate);
        Assert.Equal(5m, r.ApprovedDays);
        Assert.Equal(3m, r.ActualDays);
    }
}

/// <summary>The header's side of the same lifecycle — the states an approver and the ledger depend on.</summary>
public class AnnualLeaveHeaderReturnTests
{
    private static AnnualLeaveHeader Approved()
    {
        var h = AnnualLeaveHeader.Create(Guid.NewGuid(), Guid.NewGuid(), new DateTime(2026, 8, 1), null);
        h.AddDetail(AnnualLeaveUsage.FullDay, new DateTime(2026, 8, 10), new DateTime(2026, 8, 14), 5m);
        h.Approve();
        return h;
    }

    [Fact]
    public void Only_an_approved_request_can_have_its_return_confirmed()
    {
        var pending = AnnualLeaveHeader.Create(Guid.NewGuid(), Guid.NewGuid(), new DateTime(2026, 8, 1), null);

        Assert.False(pending.CanConfirmReturn);
        Assert.True(Approved().CanConfirmReturn);
    }

    [Fact]
    public void An_on_time_return_closes_the_request_directly()
    {
        var h = Approved();

        h.CloseOnTimeReturn(5m);

        Assert.Equal(AnnualLeaveStatus.Closed, h.Status);
        Assert.Equal(5m, h.ActualLeaveDays);
        Assert.Equal(5m, h.TotalLeaveDays);       // the approved figure is preserved for comparison
    }

    [Fact]
    public void An_adjustment_parks_the_request_and_a_rejection_hands_it_back()
    {
        var h = Approved();

        h.BeginReturnAdjustment();
        Assert.Equal(AnnualLeaveStatus.ReturnPending, h.Status);
        Assert.False(h.CanConfirmReturn);         // no second confirmation while one is pending

        h.RejectReturn();
        Assert.Equal(AnnualLeaveStatus.Approved, h.Status);
        Assert.True(h.CanConfirmReturn);          // ...but they may try again with a corrected date
        Assert.Null(h.ActualLeaveDays);
    }

    [Fact]
    public void Settling_records_the_days_actually_taken()
    {
        var h = Approved();
        h.BeginReturnAdjustment();

        h.SettleReturn(3m);

        Assert.Equal(AnnualLeaveStatus.Closed, h.Status);
        Assert.Equal(3m, h.ActualLeaveDays);
        Assert.Equal(5m, h.TotalLeaveDays);
    }

    [Fact]
    public void A_request_still_holds_its_ledger_debit_through_every_return_state()
    {
        // Cancellation reverses the debit, so a state that has one must say so — otherwise a cancel
        // during ReturnPending would leave the days deducted forever.
        var h = Approved();
        Assert.True(h.HoldsBalance);

        h.BeginReturnAdjustment();
        Assert.True(h.HoldsBalance);

        h.SettleReturn(3m);
        Assert.True(h.HoldsBalance);
    }

    [Fact]
    public void A_closed_request_cannot_be_confirmed_or_adjusted_again()
    {
        var h = Approved();
        h.CloseOnTimeReturn(5m);

        Assert.False(h.CanConfirmReturn);
        Assert.Throws<InvalidOperationException>(() => h.BeginReturnAdjustment());
        Assert.Throws<InvalidOperationException>(() => h.CloseOnTimeReturn(5m));
    }
}
