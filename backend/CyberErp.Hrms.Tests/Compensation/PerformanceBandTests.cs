using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// Performance-banded revisions: the score picks a band, the band's value is fed into whichever basis
/// was chosen. The worked examples below are the ones from the requirement.
/// </summary>
public class PerformanceBandTests
{
    private static readonly Guid Grade = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Alice = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Bob = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid Carol = Guid.Parse("cccccccc-0000-0000-0000-000000000003");
    private static readonly Guid NoAppraisal = Guid.Parse("dddddddd-0000-0000-0000-000000000004");

    private static SalaryScaleLadder Ladder() => new(new Dictionary<Guid, ScaleRung[]>
    {
        [Grade] = [new(1, 45000m), new(2, 48000m), new(3, 51000m), new(4, 54000m), new(5, 60000m)]
    });

    private static EmployeeCompRow Row(Guid id, decimal salary, int? step = 1) => new()
    {
        EmployeeId = id, Salary = salary, JobGradeId = Grade, StepOrdinal = step
    };

    /// <summary>Scores: Alice 95 (top), Bob 80 (middle), Carol 55 (bottom), NoAppraisal absent.</summary>
    private static IPerformanceAwardResolver Awards(params (decimal MinScore, decimal Value, string? Label)[] bands)
        => new PerformanceAwardResolver(
            new Dictionary<Guid, decimal> { [Alice] = 95m, [Bob] = 80m, [Carol] = 55m },
            bands.OrderByDescending(b => b.MinScore).ToArray());

    // The requirement's three band sets, one per basis.
    private static (decimal, decimal, string?)[] StepBands() =>
        [(90m, 2.5m, "Outstanding"), (70m, 2m, "Strong"), (0m, 1m, "Standard")];
    private static (decimal, decimal, string?)[] PercentBands() =>
        [(90m, 15m, "Outstanding"), (70m, 10m, "Strong"), (0m, 0m, "Standard")];
    private static (decimal, decimal, string?)[] AmountBands() =>
        [(90m, 3000m, "Outstanding"), (70m, 2500m, "Strong"), (0m, 0m, "Standard")];

    // ---- Band selection -----------------------------------------------------

    [Fact]
    public void Highest_matching_band_wins()
    {
        var a = Awards(StepBands());

        Assert.Equal(2.5m, a.Resolve(Alice).Value);   // 95 -> >= 90
        Assert.Equal(2m, a.Resolve(Bob).Value);       // 80 -> >= 70
        Assert.Equal(1m, a.Resolve(Carol).Value);     // 55 -> >= 0
    }

    [Fact]
    public void Band_label_and_score_are_reported_for_transparency()
    {
        var award = Awards(StepBands()).Resolve(Bob);

        Assert.Equal(80m, award.Score);
        Assert.Equal("Strong", award.BandLabel);
        Assert.Null(award.Reason);
    }

    [Theory]
    [InlineData(90, 2.5)]    // exactly on the top threshold -> top band (MinScore is INCLUSIVE)
    [InlineData(70, 2.0)]    // exactly on the middle threshold -> middle band
    [InlineData(89.99, 2.0)]
    [InlineData(69.99, 1.0)]
    public void Thresholds_are_inclusive_lower_bounds(double score, double expected)
    {
        var a = new PerformanceAwardResolver(
            new Dictionary<Guid, decimal> { [Alice] = (decimal)score },
            StepBands().OrderByDescending(b => b.Item1).ToArray());

        Assert.Equal((decimal)expected, a.Resolve(Alice).Value);
    }

    [Fact]
    public void Bands_supplied_out_of_order_are_still_matched_highest_first()
    {
        var a = Awards([(0m, 1m, "Standard"), (90m, 2.5m, "Outstanding"), (70m, 2m, "Strong")]);

        Assert.Equal(2.5m, a.Resolve(Alice).Value);
        Assert.Equal(2m, a.Resolve(Bob).Value);
    }

    [Fact]
    public void A_score_below_every_threshold_yields_no_award_with_a_reason()
    {
        // No catch-all band at 0 — Carol's 55 matches nothing.
        var a = Awards([(90m, 2.5m, null), (70m, 2m, null)]);

        var award = a.Resolve(Carol);
        Assert.Equal(0m, award.Value);
        Assert.NotNull(award.Reason);
    }

    [Fact]
    public void Missing_appraisal_is_not_treated_as_a_low_score()
    {
        // Awarding the bottom band here would hand out money (or withhold it) on missing data.
        var award = Awards(StepBands()).Resolve(NoAppraisal);

        Assert.Null(award.Score);
        Assert.NotNull(award.Reason);
    }

    [Fact]
    public void Observed_score_range_is_exposed_so_a_mis_scaled_band_set_is_visible()
    {
        // Rating scales are per-tenant (1-5 up to 0-130); this is what shows "> 90" bands set against
        // scores that top out at 5.
        var a = Awards(StepBands());

        Assert.Equal((55m, 95m), a.ObservedScoreRange);
        Assert.Equal(3, a.ScoredEmployeeCount);
    }

    // ---- Band value feeds the chosen basis ----------------------------------

    [Fact]
    public void Step_basis_advances_by_the_band_value_and_interpolates()
    {
        var awards = Awards(StepBands());
        var ladder = Ladder();

        // Alice: 95 -> 2.5 steps. Rung 1 + 2.5 = 3.5 -> between 51000 and 54000 = 52500.
        var alice = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Step, 0m, Row(Alice, 40000m), ladder, awards);
        Assert.Equal(52500m, alice.Salary);
        Assert.True(alice.Interpolated);
        Assert.Equal(3.5m, alice.ResolvedStep);

        // Bob: 80 -> 2 steps. Rung 1 + 2 = 3 -> exactly 51000, no interpolation.
        var bob = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Step, 0m, Row(Bob, 40000m), ladder, awards);
        Assert.Equal(51000m, bob.Salary);
        Assert.False(bob.Interpolated);
    }

    [Fact]
    public void Percentage_basis_uplifts_by_the_band_percent()
    {
        var awards = Awards(PercentBands());

        var alice = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, 0m, Row(Alice, 50000m), null, awards);
        var bob = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, 0m, Row(Bob, 50000m), null, awards);

        Assert.Equal(57500m, alice.Salary);   // +15%
        Assert.Equal(55000m, bob.Salary);     // +10%
    }

    [Fact]
    public void FixedAmount_basis_adds_the_band_amount()
    {
        var awards = Awards(AmountBands());

        var alice = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.FixedAmount, 0m, Row(Alice, 50000m), null, awards);
        var bob = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.FixedAmount, 0m, Row(Bob, 50000m), null, awards);

        Assert.Equal(53000m, alice.Salary);
        Assert.Equal(52500m, bob.Salary);
    }

    [Fact]
    public void A_zero_band_is_a_deliberate_no_award_not_an_error()
    {
        // The requirement's "< 70: 0%" / "< 70: 0" case: pay unchanged, and NOT flagged as a problem.
        var awards = Awards(PercentBands());

        var carol = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, 0m, Row(Carol, 50000m), null, awards);

        Assert.Equal(50000m, carol.Salary);
        Assert.Null(carol.Reason);
    }

    [Fact]
    public void An_employee_without_an_appraisal_keeps_their_pay_and_is_flagged()
    {
        var awards = Awards(PercentBands());

        var r = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, 0m, Row(NoAppraisal, 50000m), null, awards);

        Assert.Equal(50000m, r.Salary);
        Assert.NotNull(r.Reason);
    }

    [Fact]
    public void The_flat_Rate_is_ignored_entirely_when_the_type_is_Performance()
    {
        // Regression guard: a leftover Rate on the form must not leak into a banded revision.
        var awards = Awards(PercentBands());

        var r = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage,
            rate: 99m, Row(Bob, 50000m), null, awards);

        Assert.Equal(55000m, r.Salary);   // band's 10%, not 99%
    }

    [Fact]
    public void Non_performance_types_ignore_the_bands_entirely()
    {
        var awards = Awards(PercentBands());

        var r = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Merit, SalaryAdjustmentBasis.Percentage, rate: 5m, Row(Alice, 50000m), null, awards);

        Assert.Equal(52500m, r.Salary);   // flat +5%, not the band's +15%
    }

    [Fact]
    public void Performance_type_without_a_resolver_leaves_pay_untouched()
    {
        var r = SalaryRevisionShared.ProposeFor(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, 0m, Row(Alice, 50000m), null, awards: null);

        Assert.Equal(50000m, r.Salary);
        Assert.NotNull(r.Reason);
    }

    // ---- Band validation ----------------------------------------------------

    private static List<SalaryRevisionBandDto> Dtos(params (decimal min, decimal val)[] b) =>
        b.Select(x => new SalaryRevisionBandDto { MinScore = x.min, Value = x.val }).ToList();

    [Fact]
    public void Performance_revision_requires_at_least_one_band()
    {
        Assert.Throws<ValidationException>(() => SalaryRevisionShared.GuardBands(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, []));
    }

    [Fact]
    public void Duplicate_thresholds_are_rejected()
    {
        Assert.Throws<ValidationException>(() => SalaryRevisionShared.GuardBands(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, Dtos((70m, 5m), (70m, 9m))));
    }

    [Fact]
    public void Band_values_inherit_the_basis_bounds()
    {
        // 150% is as invalid in a band as it is as a flat rate.
        Assert.Throws<ValidationException>(() => SalaryRevisionShared.GuardBands(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage, Dtos((90m, 150m))));
        // ...and an absurd step count too.
        Assert.Throws<ValidationException>(() => SalaryRevisionShared.GuardBands(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.Step,
            Dtos((90m, SalaryRevisionShared.MaxStepIncrement + 1m))));
    }

    [Fact]
    public void Negative_band_values_and_scores_are_rejected()
    {
        Assert.Throws<ValidationException>(() => SalaryRevisionShared.GuardBands(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.FixedAmount, Dtos((90m, -1m))));
        Assert.Throws<ValidationException>(() => SalaryRevisionShared.GuardBands(
            SalaryRevisionType.Performance, SalaryAdjustmentBasis.FixedAmount, Dtos((-5m, 100m))));
    }

    [Fact]
    public void The_requirement_band_sets_all_validate()
    {
        SalaryRevisionShared.GuardBands(SalaryRevisionType.Performance, SalaryAdjustmentBasis.Step,
            Dtos((90m, 2.5m), (70m, 2m), (0m, 1m)));
        SalaryRevisionShared.GuardBands(SalaryRevisionType.Performance, SalaryAdjustmentBasis.Percentage,
            Dtos((90m, 15m), (70m, 10m), (0m, 0m)));
        SalaryRevisionShared.GuardBands(SalaryRevisionType.Performance, SalaryAdjustmentBasis.FixedAmount,
            Dtos((90m, 3000m), (70m, 2500m), (0m, 0m)));
    }

    [Fact]
    public void Bands_are_not_validated_for_the_flat_rate_types()
    {
        // Merit/Market/COLA ignore bands, so a stale band set must not block the save.
        SalaryRevisionShared.GuardBands(SalaryRevisionType.Merit, SalaryAdjustmentBasis.Percentage, []);
    }
}
