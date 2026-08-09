using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// The DTO validator runs BEFORE the handler's own guards, so a rule written for one revision type
/// can reject another. These pin the interaction between RevisionType and Basis.
/// </summary>
public class SaveSalaryRevisionValidatorTests
{
    private static readonly SaveSalaryRevisionDtoValidator Validator = new();

    private static SaveSalaryRevisionDto Dto(string type, string basis, decimal rate,
        params (decimal min, decimal val)[] bands) => new()
    {
        Name = "Test revision",
        RevisionType = type,
        Basis = basis,
        Rate = rate,
        EffectiveDate = new DateTime(2026, 12, 1),
        Bands = bands.Select(b => new SalaryRevisionBandDto { MinScore = b.min, Value = b.val }).ToList(),
    };

    private static string[] Errors(SaveSalaryRevisionDto dto) =>
        Validator.Validate(dto).Errors.Select(e => e.ErrorMessage).ToArray();

    [Fact]
    public void Performance_with_Step_basis_is_valid_even_though_Rate_is_zero()
    {
        // The reported bug: the bands carry the step counts and the form hides the rate field, so
        // Rate arrives as 0 — but the step rule fired anyway and rejected the save.
        var errors = Errors(Dto(nameof(SalaryRevisionType.Performance), nameof(SalaryAdjustmentBasis.Step),
            rate: 0m, (90m, 2.5m), (70m, 2m), (0m, 1m)));

        Assert.DoesNotContain(errors, e => e.Contains("step increment", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(nameof(SalaryAdjustmentBasis.Percentage))]
    [InlineData(nameof(SalaryAdjustmentBasis.FixedAmount))]
    public void Performance_is_valid_with_a_zero_Rate_on_any_basis(string basis)
    {
        Assert.Empty(Errors(Dto(nameof(SalaryRevisionType.Performance), basis, 0m, (90m, 15m), (0m, 0m))));
    }

    [Fact]
    public void A_non_performance_Step_revision_still_requires_a_positive_increment()
    {
        // The rule must keep doing its job for the flat-rate types.
        var errors = Errors(Dto(nameof(SalaryRevisionType.Merit), nameof(SalaryAdjustmentBasis.Step), rate: 0m));

        Assert.Contains(errors, e => e.Contains("step increment", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_non_performance_Step_revision_with_a_real_increment_is_valid()
    {
        Assert.Empty(Errors(Dto(nameof(SalaryRevisionType.Merit), nameof(SalaryAdjustmentBasis.Step), rate: 1.5m)));
    }

    [Fact]
    public void Performance_still_requires_at_least_one_band()
    {
        var errors = Errors(Dto(nameof(SalaryRevisionType.Performance), nameof(SalaryAdjustmentBasis.Step), 0m));

        Assert.Contains(errors, e => e.Contains("at least one score band", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void The_band_requirement_does_not_apply_to_the_flat_rate_types()
    {
        Assert.Empty(Errors(Dto(nameof(SalaryRevisionType.CostOfLiving), nameof(SalaryAdjustmentBasis.Percentage), 5m)));
    }

    [Fact]
    public void A_negative_rate_is_rejected_for_every_type()
    {
        Assert.NotEmpty(Errors(Dto(nameof(SalaryRevisionType.Merit), nameof(SalaryAdjustmentBasis.Percentage), -1m)));
        Assert.NotEmpty(Errors(Dto(nameof(SalaryRevisionType.Performance), nameof(SalaryAdjustmentBasis.Percentage), -1m, (0m, 5m))));
    }
}
