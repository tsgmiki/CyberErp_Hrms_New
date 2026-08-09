using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// The three eligibility rules: minimum service, active disciplinary cases, and first-year proration.
/// Dates are fixed so the expectations stay true regardless of when the suite runs.
/// </summary>
public class IncrementEligibilityTests
{
    private static readonly Guid Alice = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Bob = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly DateTime Effective = new(2026, 07, 01);

    private static ISalaryIncrementEligibility Build(
        int minMonths, bool prorate = true, bool excludeDisciplinary = true, params Guid[] blocked) =>
        new SalaryIncrementEligibility(
            SalaryIncrementPolicy.Create("Policy", minMonths, prorate, excludeDisciplinary),
            [.. blocked]);

    // ---- Rule 1: minimum service -------------------------------------------

    [Theory]
    [InlineData(3, 2, false)]   // hired 2 months ago, needs 3  -> out
    [InlineData(3, 3, true)]    // exactly at the gate          -> in
    [InlineData(6, 5, false)]
    [InlineData(6, 6, true)]
    [InlineData(9, 8, false)]
    [InlineData(9, 12, true)]
    public void Minimum_service_gate_admits_only_those_at_or_above_it(int minMonths, int monthsWorked, bool eligible)
    {
        var hired = Effective.AddMonths(-monthsWorked);

        var v = Build(minMonths).Evaluate(Alice, hired, Effective);

        Assert.Equal(eligible, v.IsEligible);
        Assert.Equal(monthsWorked, v.MonthsOfService);
        if (!eligible) Assert.Contains("minimum is", v.Reason!);
    }

    [Fact]
    public void A_zero_gate_admits_everyone_including_a_brand_new_hire()
    {
        var v = Build(0).Evaluate(Alice, Effective.AddDays(-3), Effective);

        Assert.True(v.IsEligible);
        Assert.Equal(0, v.MonthsOfService);
    }

    [Fact]
    public void Service_is_counted_in_completed_months_not_part_months()
    {
        // Hired 15 Jan: on 14 Jun that is 4 completed months, on 15 Jun it is 5.
        Assert.Equal(4, SalaryIncrementEligibility.CompletedMonths(new(2026, 1, 15), new(2026, 6, 14)));
        Assert.Equal(5, SalaryIncrementEligibility.CompletedMonths(new(2026, 1, 15), new(2026, 6, 15)));
        // Month lengths must not change the answer: 31 Jan -> 28 Feb is not yet a full month.
        Assert.Equal(0, SalaryIncrementEligibility.CompletedMonths(new(2026, 1, 31), new(2026, 2, 28)));
    }

    [Theory]
    // Every salaried employee on the live "salary revision 01" plan (effective 2026-08-31), pinned
    // after a report that a July-2026 hire was showing 31 months. These are the exact hire dates in
    // Hrms.Employee, and 31 belongs to the 2024-01-01 hires — a recent hire lands on 1.
    [InlineData("2026-07-09", 1)]    // the reported employee: one month, not 31
    [InlineData("2024-01-01", 31)]   // three employees share this date, and 31 is correct for it
    [InlineData("2016-12-01", 116)]
    [InlineData("2018-03-20", 101)]  // day 20 <= 31, so the part-month counts
    [InlineData("2019-01-01", 91)]
    [InlineData("2022-03-10", 53)]
    [InlineData("2023-01-02", 43)]
    public void Service_matches_the_live_plan_for_every_hire_date_on_it(string hired, int expected)
    {
        var effective = new DateTime(2026, 8, 31);

        Assert.Equal(expected, SalaryIncrementEligibility.CompletedMonths(DateTime.Parse(hired), effective));
    }

    [Fact]
    public void An_employee_with_no_hire_date_is_excluded_rather_than_assumed_eligible()
    {
        // Missing data is a data problem HR should see, not a silent pass to an increment.
        var v = Build(3).Evaluate(Alice, null, Effective);

        Assert.False(v.IsEligible);
        Assert.Contains("no hire date", v.Reason!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_hire_date_after_the_effective_date_is_excluded()
    {
        var v = Build(0).Evaluate(Alice, Effective.AddMonths(2), Effective);

        Assert.False(v.IsEligible);
        Assert.Contains("after the effective date", v.Reason!, StringComparison.OrdinalIgnoreCase);
    }

    // ---- Rule 2: active disciplinary cases ---------------------------------

    [Fact]
    public void An_active_disciplinary_case_excludes_regardless_of_service()
    {
        // 5 years in and well past any gate — the case still wins.
        var v = Build(3, blocked: Alice).Evaluate(Alice, Effective.AddYears(-5), Effective);

        Assert.False(v.IsEligible);
        Assert.Contains("disciplinary", v.Reason!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Employees_without_a_case_are_unaffected()
    {
        var e = Build(3, blocked: Alice);

        Assert.False(e.Evaluate(Alice, Effective.AddYears(-5), Effective).IsEligible);
        Assert.True(e.Evaluate(Bob, Effective.AddYears(-5), Effective).IsEligible);
    }

    [Fact]
    public void The_disciplinary_rule_can_be_switched_off_by_policy()
    {
        var v = Build(0, excludeDisciplinary: false, blocked: Alice)
            .Evaluate(Alice, Effective.AddYears(-5), Effective);

        Assert.True(v.IsEligible);
    }

    // ---- Rule 3: first-year proration --------------------------------------

    [Theory]
    [InlineData(12, 1.0)]      // a full year -> full increment
    [InlineData(24, 1.0)]
    [InlineData(6, 0.5)]       // half a year -> half the increment
    [InlineData(3, 0.25)]
    [InlineData(9, 0.75)]
    public void Proration_factor_is_months_worked_over_twelve(int monthsWorked, double expected)
    {
        var v = Build(0).Evaluate(Alice, Effective.AddMonths(-monthsWorked), Effective);

        Assert.True(v.IsEligible);
        Assert.Equal((decimal)expected, v.ProrationFactor);
    }

    [Fact]
    public void Proration_can_be_switched_off_by_policy()
    {
        var v = Build(0, prorate: false).Evaluate(Alice, Effective.AddMonths(-6), Effective);

        Assert.True(v.IsEligible);
        Assert.Equal(1m, v.ProrationFactor);
    }

    [Fact]
    public void An_excluded_employee_has_a_zero_factor()
    {
        Assert.Equal(0m, Build(6).Evaluate(Alice, Effective.AddMonths(-2), Effective).ProrationFactor);
        Assert.Equal(0m, Build(0, blocked: Alice).Evaluate(Alice, Effective.AddYears(-5), Effective).ProrationFactor);
    }

    [Fact]
    public void With_no_policy_at_all_nobody_is_gated_and_proration_still_applies()
    {
        // Absent configuration the tenure gate is 0, but a first-year hire is still prorated —
        // the safe default, since paying a full increment for two months' work is the costlier error.
        var e = new SalaryIncrementEligibility(null, []);

        var v = e.Evaluate(Alice, Effective.AddMonths(-6), Effective);
        Assert.True(v.IsEligible);
        Assert.Equal(0.5m, v.ProrationFactor);
        Assert.Equal(0, e.MinimumServiceMonths);
        Assert.False(e.HasPolicy);
    }
}
