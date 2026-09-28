using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Delegations;

/// <summary>
/// What a POST carries beyond its salary — the record that made "the salary AND BENEFITS associated
/// with the deputized position" expressible at all.
/// </summary>
public class PositionEntitlementTests
{
    private static readonly Guid Class = new("11110000-0000-0000-0000-000000000001");
    private static readonly Guid Allowance = new("22220000-0000-0000-0000-000000000002");
    private static readonly Guid Plan = new("33330000-0000-0000-0000-000000000003");

    private static PositionEntitlement Allowanced(decimal? value = 2_000m) =>
        PositionEntitlement.Create(Class, EntitlementKind.Allowance, Allowance, null, value);

    private static PositionEntitlement Benefited() =>
        PositionEntitlement.Create(Class, EntitlementKind.BenefitPlan, null, Plan, null);

    // ---- exactly one reference, matching the kind ----------------------------

    [Fact]
    public void AnAllowanceEntitlementNeedsAnAllowanceType()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PositionEntitlement.Create(Class, EntitlementKind.Allowance, null, null, 100m));

        Assert.Contains("allowance type", ex.Message);
    }

    [Fact]
    public void ABenefitEntitlementNeedsAPlan()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PositionEntitlement.Create(Class, EntitlementKind.BenefitPlan, null, null, null));

        Assert.Contains("benefit plan", ex.Message);
    }

    /// <summary>
    /// ⚠️ A row carrying BOTH references would read as valid and then resolve to whichever side the
    /// reader happened to check — at the moment somebody is being paid.
    /// </summary>
    [Fact]
    public void TheIrrelevantReferenceIsDiscarded_NotStored()
    {
        var e = PositionEntitlement.Create(Class, EntitlementKind.Allowance, Allowance, Plan, 500m);

        Assert.Equal(Allowance, e.AllowanceTypeId);
        Assert.Null(e.BenefitPlanId);
    }

    [Fact]
    public void ChangingKindClearsTheOldSide()
    {
        var e = Allowanced();
        e.Update(EntitlementKind.BenefitPlan, Allowance, Plan, null, true, true, null);

        Assert.Null(e.AllowanceTypeId);
        Assert.Equal(Plan, e.BenefitPlanId);
    }

    [Fact]
    public void ReferenceIdFollowsTheKind()
    {
        Assert.Equal(Allowance, Allowanced().ReferenceId);
        Assert.Equal(Plan, Benefited().ReferenceId);
    }

    [Fact]
    public void ANegativeValueIsRefused()
    {
        Assert.Throws<ArgumentException>(() =>
            PositionEntitlement.Create(Class, EntitlementKind.Allowance, Allowance, null, -1m));
    }

    [Fact]
    public void APositionClassIsRequired()
    {
        Assert.Throws<ArgumentException>(() =>
            PositionEntitlement.Create(Guid.Empty, EntitlementKind.Allowance, Allowance, null, 1m));
    }

    // ---- acting behaviour ----------------------------------------------------

    /// <summary>The point of acting compensation is the deputy gets the post's package.</summary>
    [Fact]
    public void EntitlementsFollowADeputyByDefault()
    {
        Assert.True(Allowanced().GrantedWhenActing);
    }

    /// <summary>
    /// ⚠️ …but not everything should. A long-service award or a relocation benefit is tied to the
    /// substantive holder, and there must be a way to say so that is not "delete the entitlement".
    /// </summary>
    [Fact]
    public void AnEntitlementCanBeWithheldFromDeputiesWithoutBeingRemoved()
    {
        var e = Allowanced();
        e.Update(EntitlementKind.Allowance, Allowance, null, e.Value,
            grantedWhenActing: false, isActive: true, notes: "Substantive holder only");

        Assert.False(e.GrantedWhenActing);
        Assert.True(e.IsActive);   // still what the post carries — just not to a stand-in
    }

    /// <summary>A null value means "use the catalogue's default rate", not "grant zero".</summary>
    [Fact]
    public void ANullValueIsDeferralToTheCatalogue_NotZero()
    {
        Assert.Null(Allowanced(value: null).Value);
    }
}
