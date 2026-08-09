using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// <c>AffectsSalaryIncrement</c> is the odd one out among the three impact flags: it defaults to TRUE.
///
/// <para>Its siblings are opt-in because blocking a promotion or a reward is an extra sanction someone
/// chooses to apply. Withholding an increment was already the behaviour of EVERY active case before
/// the flag existed, so defaulting it off would have quietly started paying people mid-discipline the
/// moment the column shipped. These tests exist so that asymmetry cannot be "tidied up" by accident.</para>
/// </summary>
public class DisciplinaryIncrementFlagTests
{
    private static DisciplinaryMeasure Measure(
        bool? affectsSalaryIncrement = null, bool affectsPromotion = false, bool affectsReward = false) =>
        affectsSalaryIncrement is null
            ? DisciplinaryMeasure.Create(
                Guid.NewGuid(), new DateTime(2026, 5, 1), "Absenteeism",
                DisciplinaryMeasureType.WrittenWarning,
                affectsPromotion: affectsPromotion, affectsReward: affectsReward)
            : DisciplinaryMeasure.Create(
                Guid.NewGuid(), new DateTime(2026, 5, 1), "Absenteeism",
                DisciplinaryMeasureType.WrittenWarning,
                affectsPromotion: affectsPromotion, affectsReward: affectsReward,
                affectsSalaryIncrement: affectsSalaryIncrement.Value);

    [Fact]
    public void A_new_case_blocks_an_increment_by_default()
    {
        Assert.True(Measure().AffectsSalaryIncrement);
    }

    [Fact]
    public void The_two_sibling_flags_still_default_OFF()
    {
        // The asymmetry is deliberate; if these ever start defaulting true, that was not intended here.
        var m = Measure();

        Assert.False(m.AffectsPromotion);
        Assert.False(m.AffectsReward);
    }

    [Fact]
    public void HR_can_exempt_a_case_from_the_increment_rule()
    {
        Assert.False(Measure(affectsSalaryIncrement: false).AffectsSalaryIncrement);
    }

    [Fact]
    public void The_increment_flag_is_independent_of_the_promotion_and_reward_flags()
    {
        // A case can block pay without blocking promotion, and vice versa — they answer different
        // questions and the salary rule must not be inferred from the other two.
        var payOnly = Measure(affectsSalaryIncrement: true, affectsPromotion: false, affectsReward: false);
        var promoOnly = Measure(affectsSalaryIncrement: false, affectsPromotion: true, affectsReward: true);

        Assert.True(payOnly.AffectsSalaryIncrement);
        Assert.False(payOnly.AffectsPromotion);
        Assert.False(promoOnly.AffectsSalaryIncrement);
        Assert.True(promoOnly.AffectsPromotion);
    }

    [Fact]
    public void An_update_can_toggle_the_flag_both_ways()
    {
        var m = Measure();

        m.Update(new DateTime(2026, 5, 1), "Absenteeism", DisciplinaryMeasureType.WrittenWarning,
            DisciplinaryStatus.Open, null, null, null, null, false, false, affectsSalaryIncrement: false);
        Assert.False(m.AffectsSalaryIncrement);

        m.Update(new DateTime(2026, 5, 1), "Absenteeism", DisciplinaryMeasureType.WrittenWarning,
            DisciplinaryStatus.Open, null, null, null, null, false, false, affectsSalaryIncrement: true);
        Assert.True(m.AffectsSalaryIncrement);
    }
}
