using CyberErp.Hrms.App.Features.Core.Compensation;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// Step-revision interpolation. This maths decides people's pay, so the cases below are the ones a
/// reviewer should be able to check by hand — each expected number is derived in its comment.
/// </summary>
public class SalaryScaleLadderTests
{
    private static readonly Guid GradeA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid GradeB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Unknown = Guid.Parse("99999999-9999-9999-9999-999999999999");

    /// <summary>Contiguous ladder 1..4.</summary>
    private static SalaryScaleLadder Contiguous() => Build((GradeA, new[]
    {
        (1, 45000m), (2, 48000m), (3, 51000m), (4, 54000m)
    }));

    /// <summary>Real ladders have holes — this one is missing rung 4 (mirrors live grade "01"/"13").</summary>
    private static SalaryScaleLadder Gapped() => Build((GradeA, new[]
    {
        (1, 45000m), (2, 48000m), (3, 51000m), (5, 60000m)
    }));

    private static SalaryScaleLadder Build(params (Guid Grade, (int Ordinal, decimal Salary)[] Rungs)[] grades)
    {
        var map = grades.ToDictionary(
            g => g.Grade,
            g => g.Rungs.Select(r => new ScaleRung(r.Ordinal, r.Salary)).OrderBy(r => r.Ordinal).ToArray());
        return new SalaryScaleLadder(map);
    }

    // ---- Whole-step landings ------------------------------------------------

    [Theory]
    [InlineData(1, 1, 48000)]   // rung 1 + 1 -> rung 2
    [InlineData(1, 2, 51000)]   // rung 1 + 2 -> rung 3
    [InlineData(2, 1, 51000)]   // rung 2 + 1 -> rung 3
    public void Whole_increment_reads_the_rung_directly(int from, int increment, decimal expected)
    {
        var r = Contiguous().Resolve(GradeA, from, increment, currentSalary: 1m);

        Assert.Equal(expected, r.Salary);
        Assert.False(r.Interpolated);
        Assert.False(r.Capped);
        Assert.Null(r.Reason);
        Assert.Equal(from + increment, r.ResolvedStep);
    }

    [Fact]
    public void Zero_increment_returns_the_current_rung_value()
    {
        var r = Contiguous().Resolve(GradeA, 3, 0m, currentSalary: 1m);

        Assert.Equal(51000m, r.Salary);
        Assert.False(r.Interpolated);
        Assert.Equal(3m, r.ResolvedStep);
    }

    // ---- Fractional landings (the feature) ----------------------------------

    [Theory]
    // rung 1 + 1.5 = 2.5 -> halfway 48000..51000 = 49500
    [InlineData(1, 1.5, 2.5, 49500)]
    // rung 1 + 0.5 = 1.5 -> halfway 45000..48000 = 46500
    [InlineData(1, 0.5, 1.5, 46500)]
    // rung 2 + 1.25 = 3.25 -> 51000 + (54000-51000)*0.25 = 51750
    [InlineData(2, 1.25, 3.25, 51750)]
    // rung 1 + 2.75 = 3.75 -> 51000 + 3000*0.75 = 53250
    [InlineData(1, 2.75, 3.75, 53250)]
    public void Fractional_increment_interpolates_between_the_two_neighbouring_rungs(
        int from, double increment, double expectedStep, decimal expectedSalary)
    {
        var r = Contiguous().Resolve(GradeA, from, (decimal)increment, currentSalary: 1m);

        Assert.Equal(expectedSalary, r.Salary);
        Assert.True(r.Interpolated);
        Assert.False(r.Capped);
        Assert.Equal((decimal)expectedStep, r.ResolvedStep);
    }

    [Fact]
    public void Interpolation_is_linear_and_symmetric_about_the_midpoint()
    {
        var ladder = Contiguous();
        var quarter = ladder.Resolve(GradeA, 1, 0.25m, 1m).Salary;   // 45000 + 3000*.25
        var half = ladder.Resolve(GradeA, 1, 0.5m, 1m).Salary;       // 45000 + 3000*.50
        var threeQ = ladder.Resolve(GradeA, 1, 0.75m, 1m).Salary;    // 45000 + 3000*.75

        Assert.Equal(45750m, quarter);
        Assert.Equal(46500m, half);
        Assert.Equal(47250m, threeQ);
        Assert.Equal(half - quarter, threeQ - half);
    }

    // ---- Gapped ladders -----------------------------------------------------

    [Fact]
    public void Landing_inside_a_gap_interpolates_between_the_nearest_DEFINED_rungs()
    {
        // Ladder has no rung 4. Landing on 3.5 must span 3 -> 5 (width 2), NOT assume a rung 4 exists:
        // 51000 + (60000-51000) * ((3.5-3)/2) = 51000 + 9000*0.25 = 53250
        var r = Gapped().Resolve(GradeA, 1, 2.5m, currentSalary: 1m);

        Assert.Equal(53250m, r.Salary);
        Assert.True(r.Interpolated);
        Assert.Equal(3.5m, r.ResolvedStep);
    }

    [Fact]
    public void Landing_exactly_on_a_missing_rung_still_interpolates_across_the_gap()
    {
        // Rung 4 does not exist: 51000 + 9000 * ((4-3)/2) = 55500
        var r = Gapped().Resolve(GradeA, 1, 3m, currentSalary: 1m);

        Assert.Equal(55500m, r.Salary);
        Assert.True(r.Interpolated);
        Assert.Equal(4m, r.ResolvedStep);
    }

    [Fact]
    public void Landing_on_a_defined_rung_beyond_a_gap_is_exact_not_interpolated()
    {
        var r = Gapped().Resolve(GradeA, 1, 4m, currentSalary: 1m);   // -> rung 5

        Assert.Equal(60000m, r.Salary);
        Assert.False(r.Interpolated);
    }

    // ---- Ceiling and base ---------------------------------------------------

    [Fact]
    public void Landing_above_the_top_rung_is_capped_and_reported()
    {
        var r = Contiguous().Resolve(GradeA, 3, 10m, currentSalary: 1m);

        Assert.Equal(54000m, r.Salary);        // top rung, never extrapolated past it
        Assert.True(r.Capped);
        Assert.Equal(4m, r.ResolvedStep);
        Assert.NotNull(r.Reason);
    }

    [Fact]
    public void Landing_exactly_on_the_top_rung_is_not_flagged_as_capped()
    {
        var r = Contiguous().Resolve(GradeA, 1, 3m, currentSalary: 1m);

        Assert.Equal(54000m, r.Salary);
        Assert.False(r.Capped);
        Assert.Null(r.Reason);
    }

    [Fact]
    public void Landing_below_the_base_rung_is_floored_and_reported()
    {
        var r = Contiguous().Resolve(GradeA, 2, -5m, currentSalary: 1m);

        Assert.Equal(45000m, r.Salary);
        Assert.True(r.Capped);
        Assert.Equal(1m, r.ResolvedStep);
        Assert.NotNull(r.Reason);
    }

    [Fact]
    public void A_single_rung_ladder_always_pays_that_rung()
    {
        var ladder = Build((GradeA, new[] { (7, 30000m) }));

        var up = ladder.Resolve(GradeA, 7, 3.5m, currentSalary: 1m);
        var down = ladder.Resolve(GradeA, 7, -2m, currentSalary: 1m);

        Assert.Equal(30000m, up.Salary);
        Assert.Equal(30000m, down.Salary);
        Assert.False(up.Interpolated);
    }

    // ---- Rounding -----------------------------------------------------------

    [Fact]
    public void Interpolated_salary_is_rounded_to_two_decimals()
    {
        // 1000 + (2000-1000) * ((2-1)/3) = 1333.333... -> 1333.33
        var ladder = Build((GradeA, new[] { (1, 1000m), (4, 2000m) }));

        var r = ladder.Resolve(GradeA, 1, 1m, currentSalary: 1m);

        Assert.Equal(1333.33m, r.Salary);
        Assert.True(r.Interpolated);
    }

    // ---- Grades are independent --------------------------------------------

    [Fact]
    public void Each_grade_interpolates_on_its_own_ladder_only()
    {
        var ladder = Build(
            (GradeA, new[] { (1, 45000m), (2, 48000m) }),
            (GradeB, new[] { (1, 10000m), (2, 12000m) }));

        Assert.Equal(46500m, ladder.Resolve(GradeA, 1, 0.5m, 1m).Salary);
        Assert.Equal(11000m, ladder.Resolve(GradeB, 1, 0.5m, 1m).Salary);
    }

    // ---- Employees the scale cannot move ------------------------------------

    [Fact]
    public void Employee_with_no_grade_is_left_alone_with_a_reason()
    {
        var r = Contiguous().Resolve(jobGradeId: null, currentStep: 2, increment: 1m, currentSalary: 41000m);

        Assert.Equal(41000m, r.Salary);
        Assert.NotNull(r.Reason);
        Assert.False(r.Interpolated);
    }

    [Fact]
    public void Employee_with_no_current_step_is_left_alone_with_a_reason()
    {
        var r = Contiguous().Resolve(GradeA, currentStep: null, increment: 1m, currentSalary: 41000m);

        Assert.Equal(41000m, r.Salary);
        Assert.NotNull(r.Reason);
    }

    [Fact]
    public void Grade_without_any_scale_rows_is_left_alone_with_a_reason()
    {
        var r = Contiguous().Resolve(Unknown, currentStep: 1, increment: 1m, currentSalary: 41000m);

        Assert.Equal(41000m, r.Salary);
        Assert.NotNull(r.Reason);
    }

    [Fact]
    public void Grade_present_but_with_an_empty_ladder_is_left_alone()
    {
        var ladder = Build((GradeA, Array.Empty<(int, decimal)>()));

        var r = ladder.Resolve(GradeA, 1, 1m, currentSalary: 41000m);

        Assert.Equal(41000m, r.Salary);
        Assert.NotNull(r.Reason);
    }
}
