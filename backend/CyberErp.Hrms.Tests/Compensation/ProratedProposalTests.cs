using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// Where the eligibility rules meet the money: exclusions must leave pay untouched, and proration
/// must scale the INCREASE (not the salary) so it means the same thing on every basis.
/// </summary>
public class ProratedProposalTests
{
    private static readonly Guid Grade = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Alice = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly DateTime Effective = new(2026, 07, 01);

    private static SalaryScaleLadder Ladder() => new(new Dictionary<Guid, ScaleRung[]>
    {
        [Grade] = [new(1, 45000m), new(2, 48000m), new(3, 51000m)]
    });

    private static EmployeeCompRow Row(decimal salary, int monthsWorked, int? step = 1) => new()
    {
        EmployeeId = Alice,
        Salary = salary,
        JobGradeId = Grade,
        StepOrdinal = step,
        HireDate = Effective.AddMonths(-monthsWorked),
    };

    private static ISalaryIncrementEligibility Elig(int minMonths, bool prorate = true, params Guid[] blocked) =>
        new SalaryIncrementEligibility(
            SalaryIncrementPolicy.Create("P", minMonths, prorate, true), [.. blocked]);

    private static StepResolution Propose(
        SalaryAdjustmentBasis basis, decimal rate, EmployeeCompRow row, ISalaryIncrementEligibility e,
        ISalaryScaleLadder? ladder = null) =>
        SalaryRevisionShared.ProposeWithEligibility(
            SalaryRevisionType.Merit, basis, rate, row, ladder, null, e, Effective);

    // ---- Exclusions leave pay exactly as it was ----------------------------

    [Fact]
    public void An_employee_below_the_service_gate_keeps_their_salary()
    {
        var r = Propose(SalaryAdjustmentBasis.Percentage, 10m, Row(50000m, monthsWorked: 2), Elig(6));

        Assert.Equal(50000m, r.Salary);
        Assert.Equal(0m, r.ProrationFactor);
        Assert.Contains("minimum is 6", r.Reason!);
    }

    [Fact]
    public void An_employee_with_an_active_disciplinary_case_keeps_their_salary()
    {
        var r = Propose(SalaryAdjustmentBasis.Percentage, 10m, Row(50000m, 60), Elig(0, true, Alice));

        Assert.Equal(50000m, r.Salary);
        Assert.Equal(0m, r.ProrationFactor);
        Assert.Contains("disciplinary", r.Reason!, StringComparison.OrdinalIgnoreCase);
    }

    // ---- Proration scales the increase, on every basis ---------------------

    [Fact]
    public void Percentage_increase_is_prorated()
    {
        // 50000 + 10% = 55000, a 5000 raise. Six months in -> half of it.
        var r = Propose(SalaryAdjustmentBasis.Percentage, 10m, Row(50000m, monthsWorked: 6), Elig(0));

        Assert.Equal(52500m, r.Salary);
        Assert.Equal(0.5m, r.ProrationFactor);
        Assert.Equal(6, r.MonthsOfService);
    }

    [Fact]
    public void FixedAmount_increase_is_prorated()
    {
        // A flat 3000 raise, three months in -> 750.
        var r = Propose(SalaryAdjustmentBasis.FixedAmount, 3000m, Row(50000m, monthsWorked: 3), Elig(0));

        Assert.Equal(50750m, r.Salary);
        Assert.Equal(0.25m, r.ProrationFactor);
    }

    [Fact]
    public void Step_increase_is_prorated_on_the_money_not_the_rung()
    {
        // Rung 1 (45000) + 2 steps = rung 3 (51000) against a current salary of 45000: a 6000 raise.
        // Nine months in -> 4500, i.e. 49500. The employee still "lands" on rung 3; it is the raise
        // that is earned pro rata, which keeps the rule identical across bases.
        var r = Propose(SalaryAdjustmentBasis.Step, 2m, Row(45000m, monthsWorked: 9), Elig(0), Ladder());

        Assert.Equal(49500m, r.Salary);
        Assert.Equal(0.75m, r.ProrationFactor);
        Assert.Equal(3m, r.ResolvedStep);
    }

    [Fact]
    public void A_full_year_of_service_receives_the_whole_increment()
    {
        var r = Propose(SalaryAdjustmentBasis.Percentage, 10m, Row(50000m, monthsWorked: 12), Elig(0));

        Assert.Equal(55000m, r.Salary);
        Assert.Equal(1m, r.ProrationFactor);
    }

    [Fact]
    public void Proration_never_pushes_pay_below_its_current_value()
    {
        // The step ladder tops out below this employee's pay, so the no-cut rule already held salary.
        // Proration must not then scale a negative "raise" into a cut.
        var r = Propose(SalaryAdjustmentBasis.Step, 1m, Row(60000m, monthsWorked: 6), Elig(0), Ladder());

        Assert.Equal(60000m, r.Salary);
    }

    [Fact]
    public void Rounding_lands_on_two_decimals()
    {
        // 50000 + 7% = 53500, a 3500 raise; 5/12 of it is 1458.333... -> 1458.33
        var r = Propose(SalaryAdjustmentBasis.Percentage, 7m, Row(50000m, monthsWorked: 5), Elig(0));

        Assert.Equal(51458.33m, r.Salary);
    }

    [Fact]
    public void Without_an_eligibility_set_the_proposal_is_unchanged()
    {
        // Older callers (and the flat-rate path when no policy is loaded) must behave exactly as before.
        var r = SalaryRevisionShared.ProposeWithEligibility(
            SalaryRevisionType.Merit, SalaryAdjustmentBasis.Percentage, 10m,
            Row(50000m, 2), null, null, null, Effective);

        Assert.Equal(55000m, r.Salary);
    }

    [Fact]
    public void The_prorated_line_explains_itself()
    {
        var r = Propose(SalaryAdjustmentBasis.Percentage, 10m, Row(50000m, monthsWorked: 4), Elig(0));

        Assert.NotNull(r.Reason);
        Assert.Contains("4/12", r.Reason!);
    }
}
