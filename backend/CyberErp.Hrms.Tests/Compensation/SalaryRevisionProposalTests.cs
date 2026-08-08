using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// The layer above the ladder: which basis is applied, and the policy that a step revision must
/// never reduce someone's pay.
/// </summary>
public class SalaryRevisionProposalTests
{
    private static readonly Guid Grade = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static SalaryScaleLadder Ladder() => new(new Dictionary<Guid, ScaleRung[]>
    {
        [Grade] = [new(1, 45000m), new(2, 48000m), new(3, 51000m), new(5, 60000m)]
    });

    private static EmployeeCompRow Employee(decimal salary, int? step = 1, Guid? grade = null) => new()
    {
        EmployeeId = Guid.NewGuid(),
        Salary = salary,
        JobGradeId = grade ?? Grade,
        StepOrdinal = step
    };

    // ---- Basis dispatch -----------------------------------------------------

    [Fact]
    public void Percentage_uplifts_the_current_salary_and_ignores_the_ladder()
    {
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Percentage, 10m, Employee(50000m), Ladder());

        Assert.Equal(55000m, r.Salary);
        Assert.False(r.Interpolated);
        Assert.Null(r.Reason);
    }

    [Fact]
    public void FixedAmount_adds_to_the_current_salary_and_ignores_the_ladder()
    {
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.FixedAmount, 2500m, Employee(50000m), Ladder());

        Assert.Equal(52500m, r.Salary);
        Assert.Null(r.Reason);
    }

    [Fact]
    public void Percentage_rounds_to_two_decimals()
    {
        // 33333 * 1.075 = 35832.975 -> 35832.98
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Percentage, 7.5m, Employee(33333m), null);

        Assert.Equal(35832.98m, r.Salary);
    }

    [Fact]
    public void Step_basis_without_a_loaded_ladder_leaves_pay_untouched()
    {
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Step, 1.5m, Employee(50000m), ladder: null);

        Assert.Equal(50000m, r.Salary);
        Assert.NotNull(r.Reason);
    }

    // ---- Step basis reads the ladder ---------------------------------------

    [Fact]
    public void Step_basis_takes_the_interpolated_scale_value_when_it_is_a_rise()
    {
        // paid 40000, rung 1 + 1.5 = 2.5 -> 49500, which is a rise
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Step, 1.5m, Employee(40000m), Ladder());

        Assert.Equal(49500m, r.Salary);
        Assert.True(r.Interpolated);
        Assert.Equal(2.5m, r.ResolvedStep);
        Assert.Null(r.Reason);
    }

    // ---- The no-cut policy --------------------------------------------------

    [Fact]
    public void Step_basis_never_reduces_pay_for_an_employee_paid_above_their_rung()
    {
        // Live-data shape: paid 52000 while rung 1 pays 45000. Advancing 1 step lands on rung 2
        // (48000) — BELOW current pay. Without the policy this is a 4000 pay cut.
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Step, 1m, Employee(52000m), Ladder());

        Assert.Equal(52000m, r.Salary);
        Assert.NotNull(r.Reason);
        Assert.Contains("below current pay", r.Reason!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Held_pay_still_reports_the_step_it_would_have_landed_on()
    {
        // Transparency: HR should see the calculation even when the outcome is "no change".
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Step, 1.5m, Employee(52000m), Ladder());

        Assert.Equal(52000m, r.Salary);
        Assert.Equal(2.5m, r.ResolvedStep);
        Assert.True(r.Interpolated);
        Assert.NotNull(r.Reason);
    }

    [Fact]
    public void Step_basis_pays_the_scale_when_it_exceeds_current_pay_even_at_the_ceiling()
    {
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Step, 99m, Employee(52000m), Ladder());

        Assert.Equal(60000m, r.Salary);      // ceiling rung, and it is a rise
        Assert.True(r.Capped);
    }

    [Fact]
    public void An_employee_at_the_ceiling_already_paid_above_it_keeps_their_pay()
    {
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Step, 1m, Employee(65000m, step: 5), Ladder());

        Assert.Equal(65000m, r.Salary);      // 60000 ceiling would have been a cut
        Assert.NotNull(r.Reason);
    }

    [Fact]
    public void Employee_off_the_scale_keeps_their_pay_rather_than_dropping_to_zero()
    {
        var r = SalaryRevisionShared.Propose(
            SalaryAdjustmentBasis.Step, 2m, Employee(47000m, step: null), Ladder());

        Assert.Equal(47000m, r.Salary);
        Assert.NotNull(r.Reason);
    }

    // ---- Rate guards --------------------------------------------------------

    [Fact]
    public void Percentage_over_one_hundred_is_rejected()
    {
        Assert.Throws<ValidationException>(
            () => SalaryRevisionShared.GuardRate(SalaryAdjustmentBasis.Percentage, 101m));
    }

    [Fact]
    public void A_fractional_step_increment_is_not_treated_as_a_percentage()
    {
        // Regression guard: the percentage ceiling must not leak onto the Step basis, and a step
        // increment far above 100 is nonsense but 2.5 is entirely ordinary.
        SalaryRevisionShared.GuardRate(SalaryAdjustmentBasis.Step, 2.5m);
        SalaryRevisionShared.GuardRate(SalaryAdjustmentBasis.Percentage, 100m);
    }

    [Fact]
    public void An_absurd_step_increment_is_rejected()
    {
        Assert.Throws<ValidationException>(
            () => SalaryRevisionShared.GuardRate(
                SalaryAdjustmentBasis.Step, SalaryRevisionShared.MaxStepIncrement + 1m));
    }
}
