using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// Reaching the top of a grade stops being a dead end: with the policy enabled the employee moves onto
/// the next grade up instead of being held at the ceiling.
///
/// <para>Grades are sequenced by what they PAY, because JobGrade carries no level field and grade CODE
/// order does not track pay in live data. The fixture mirrors that: code "A" is the most expensive
/// grade and "C" the cheapest, so any test that passes by following codes would be passing by
/// accident.</para>
/// </summary>
public class GradeCeilingPromotionTests
{
    private static readonly Guid Low = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid Mid = Guid.Parse("22222222-0000-0000-0000-000000000002");
    private static readonly Guid Top = Guid.Parse("33333333-0000-0000-0000-000000000003");

    private static readonly Guid MidRung1 = Guid.Parse("aaaa0000-0000-0000-0000-000000000001");
    private static readonly Guid MidRung2 = Guid.Parse("aaaa0000-0000-0000-0000-000000000002");

    /// <summary>Codes deliberately run OPPOSITE to pay, so only pay-ordering gives the right answer.</summary>
    private static ISalaryScaleLadder Ladder() => new SalaryScaleLadder(new Dictionary<Guid, GradeLadder>
    {
        [Low] = new(Low, "C", [new(1, 10000m), new(2, 11000m), new(3, 12000m)]),
        [Mid] = new(Mid, "B", [new(1, 20000m, MidRung1), new(2, 22000m, MidRung2), new(3, 24000m)]),
        [Top] = new(Top, "A", [new(1, 30000m), new(2, 33000m)]),
    });

    private static StepResolution Resolve(Guid grade, int step, decimal increment, decimal salary, bool promote) =>
        Ladder().Resolve(grade, step, increment, salary, promote);

    // ---- The behaviour being replaced ---------------------------------------

    [Fact]
    public void Without_the_policy_the_employee_is_still_held_at_the_ceiling()
    {
        // Unchanged default: promotion only happens when a client asks for it.
        var r = Resolve(Low, step: 3, increment: 1m, salary: 12000m, promote: false);

        Assert.Equal(12000m, r.Salary);
        Assert.True(r.Capped);
        Assert.False(r.Promoted);
        Assert.Contains("Capped at the grade ceiling", r.Reason!);
    }

    // ---- Promotion ----------------------------------------------------------

    [Fact]
    public void One_step_past_the_ceiling_lands_on_the_base_of_the_next_grade()
    {
        var r = Resolve(Low, step: 3, increment: 1m, salary: 12000m, promote: true);

        Assert.True(r.Promoted);
        Assert.Equal(20000m, r.Salary);          // grade B's base, not grade C's ceiling
        Assert.Equal(1m, r.ResolvedStep);
        Assert.Equal("B", r.PromotedToGradeCode);
        Assert.Equal(MidRung1, r.PromotedToScaleId);
        Assert.False(r.Capped);
    }

    [Fact]
    public void The_next_grade_is_chosen_by_pay_not_by_grade_code()
    {
        // Code order would send grade "C" to "A" (the alphabetical next); pay order sends it to "B".
        var r = Resolve(Low, step: 3, increment: 1m, salary: 12000m, promote: true);

        Assert.Equal("B", r.PromotedToGradeCode);
    }

    [Fact]
    public void Leftover_steps_are_spent_climbing_the_new_ladder()
    {
        // Two steps past the ceiling: one buys the grade move, the second climbs a rung in the new grade.
        var r = Resolve(Low, step: 3, increment: 2m, salary: 12000m, promote: true);

        Assert.Equal(22000m, r.Salary);
        Assert.Equal(2m, r.ResolvedStep);
        Assert.Equal(MidRung2, r.PromotedToScaleId);
    }

    [Fact]
    public void An_overshoot_beyond_the_new_grade_stops_at_its_ceiling_rather_than_promoting_twice()
    {
        // A single revision moves an employee at most one grade; chaining promotions off one increment
        // would be a surprising amount of movement to approve in one click.
        var r = Resolve(Low, step: 3, increment: 9m, salary: 12000m, promote: true);

        Assert.Equal(24000m, r.Salary);          // grade B's top rung
        Assert.Equal("B", r.PromotedToGradeCode);
    }

    [Fact]
    public void At_the_top_grade_there_is_nowhere_to_go_and_the_ceiling_still_applies()
    {
        var r = Resolve(Top, step: 2, increment: 1m, salary: 33000m, promote: true);

        Assert.False(r.Promoted);
        Assert.Equal(33000m, r.Salary);
        Assert.Contains("Capped at the grade ceiling", r.Reason!);
    }

    [Fact]
    public void A_promotion_that_would_not_raise_pay_is_refused()
    {
        // Grade bands overlap in practice. An employee red-circled at 21,000 on grade C would "move up"
        // onto grade B's 20,000 base — a pay cut dressed as a promotion. Climb to a rung that actually
        // pays more instead.
        var r = Resolve(Low, step: 3, increment: 1m, salary: 21000m, promote: true);

        Assert.True(r.Promoted);
        Assert.Equal(22000m, r.Salary);
        Assert.Equal(MidRung2, r.PromotedToScaleId);
    }

    [Fact]
    public void When_no_rung_of_the_next_grade_beats_current_pay_the_employee_is_capped()
    {
        // Paid above the WHOLE of the next grade: there is no honest promotion here.
        var r = Resolve(Low, step: 3, increment: 1m, salary: 25000m, promote: true);

        Assert.False(r.Promoted);
        Assert.Null(r.PromotedToScaleId);
    }

    [Fact]
    public void Landing_exactly_on_the_ceiling_is_not_a_promotion()
    {
        // The employee reaches the top rung but does not pass it — there is nothing to overflow.
        var r = Resolve(Low, step: 2, increment: 1m, salary: 11000m, promote: true);

        Assert.False(r.Promoted);
        Assert.Equal(12000m, r.Salary);
        Assert.Null(r.Reason);
    }

    // ---- Interaction with the other policy rules ----------------------------

    private static readonly Guid Alice = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly DateTime Effective = new(2026, 07, 01);

    private static StepResolution ProposeAtCeiling(int monthsWorked, bool promoteOnCeiling)
    {
        var policy = SalaryIncrementPolicy.Create("P", 0, prorateFirstYear: true,
            excludeActiveDisciplinary: true, isActive: true, promoteOnGradeCeiling: promoteOnCeiling);
        var row = new EmployeeCompRow
        {
            EmployeeId = Alice,
            Salary = 12000m,
            JobGradeId = Low,
            StepOrdinal = 3,                       // the top rung of grade C
            HireDate = Effective.AddMonths(-monthsWorked),
        };
        return SalaryRevisionShared.ProposeWithEligibility(
            SalaryRevisionType.Merit, SalaryAdjustmentBasis.Step, 1m, row, Ladder(), null,
            new SalaryIncrementEligibility(policy, []), Effective);
    }

    [Fact]
    public void A_full_year_of_service_earns_the_promotion()
    {
        var r = ProposeAtCeiling(monthsWorked: 24, promoteOnCeiling: true);

        Assert.True(r.Promoted);
        Assert.Equal(20000m, r.Salary);
    }

    [Fact]
    public void A_prorated_first_year_employee_is_capped_rather_than_promoted()
    {
        // A partial increment must not buy a whole grade. Promoting and THEN scaling the money down
        // would leave them on a rung of grade B while paid less than grade B's base — under-scale on
        // day one. They stay at the ceiling and earn the move at the next revision.
        var r = ProposeAtCeiling(monthsWorked: 6, promoteOnCeiling: true);

        Assert.False(r.Promoted);
        Assert.Null(r.PromotedToScaleId);
        Assert.Equal(12000m, r.Salary);
        Assert.Contains("Capped at the grade ceiling", r.Reason!);
    }

    [Fact]
    public void A_fractional_overshoot_still_promotes()
    {
        // 3 + 1.5 = 4.5 on a ladder topping out at 3: one step buys the move, the half is not enough
        // for another rung.
        var r = Resolve(Low, step: 3, increment: 1.5m, salary: 12000m, promote: true);

        Assert.True(r.Promoted);
        Assert.Equal(20000m, r.Salary);
    }
}
