using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Delegations;

/// <summary>
/// The delegation record's own rules: who may hold whose authority, when it is in force, what it
/// covers, and the money ceiling.
/// </summary>
public class ApprovalDelegationTests
{
    private static readonly Guid Manager = new("11111111-0000-0000-0000-000000000001");
    private static readonly Guid Deputy = new("22222222-0000-0000-0000-000000000002");

    private static ApprovalDelegation Make(
        DateTime? start = null, DateTime? end = null,
        bool allProcesses = true, IEnumerable<string>? types = null, decimal? limit = null) =>
        ApprovalDelegation.Create(Manager, Deputy,
            start ?? new DateTime(2026, 6, 1), end ?? new DateTime(2026, 6, 30),
            "Annual leave cover", allProcesses, types, limit);

    // ---- construction guards -------------------------------------------------

    [Fact]
    public void CannotDelegateToYourself()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ApprovalDelegation.Create(Manager, Manager, DateTime.Today, DateTime.Today.AddDays(5),
                null, true, null, null));

        Assert.Contains("themselves", ex.Message);
    }

    [Fact]
    public void CannotEndBeforeItStarts()
    {
        Assert.Throws<ArgumentException>(() =>
            ApprovalDelegation.Create(Manager, Deputy,
                new DateTime(2026, 6, 10), new DateTime(2026, 6, 1), null, true, null, null));
    }

    [Fact]
    public void ANegativeApprovalLimitIsRefused()
    {
        Assert.Throws<ArgumentException>(() =>
            ApprovalDelegation.Create(Manager, Deputy, DateTime.Today, DateTime.Today, null, true, null, -1m));
    }

    [Fact]
    public void ScopedDelegationNeedsAtLeastOneProcess()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ApprovalDelegation.Create(Manager, Deputy, DateTime.Today, DateTime.Today,
                null, allProcesses: false, entityTypes: [], approvalLimit: null));

        Assert.Contains("at least one process", ex.Message);
    }

    // ---- the window ----------------------------------------------------------

    [Theory]
    [InlineData(2026, 5, 31, false)]   // the day before
    [InlineData(2026, 6, 1, true)]     // first day, inclusive
    [InlineData(2026, 6, 15, true)]
    [InlineData(2026, 6, 30, true)]    // last day, inclusive
    [InlineData(2026, 7, 1, false)]    // the day after
    public void TheWindowIsInclusiveAtBothEnds(int y, int m, int d, bool expected)
    {
        Assert.Equal(expected, Make().IsEffectiveOn(new DateTime(y, m, d)));
    }

    [Fact]
    public void StatusIsDerivedFromTheDates_NeverStored()
    {
        var delegation = Make();

        Assert.Equal(DelegationStatus.Scheduled, delegation.StatusOn(new DateTime(2026, 5, 1)));
        Assert.Equal(DelegationStatus.Active, delegation.StatusOn(new DateTime(2026, 6, 15)));
        Assert.Equal(DelegationStatus.Expired, delegation.StatusOn(new DateTime(2026, 8, 1)));
    }

    [Fact]
    public void RevokingEndsItImmediately_WhateverTheDatesSay()
    {
        var delegation = Make();
        var midWindow = new DateTime(2026, 6, 15);
        Assert.True(delegation.IsEffectiveOn(midWindow));

        delegation.Revoke("hradmin", "Returned early");

        Assert.False(delegation.IsEffectiveOn(midWindow));
        Assert.Equal(DelegationStatus.Revoked, delegation.StatusOn(midWindow));
        Assert.Equal("hradmin", delegation.RevokedBy);
    }

    [Fact]
    public void RevokingTwiceKeepsTheFirstRecord()
    {
        var delegation = Make();
        delegation.Revoke("first", "reason one");
        var at = delegation.RevokedAt;

        delegation.Revoke("second", "reason two");

        Assert.Equal("first", delegation.RevokedBy);
        Assert.Equal(at, delegation.RevokedAt);
    }

    [Fact]
    public void ARevokedDelegationCannotBeAmended()
    {
        var delegation = Make();
        delegation.Revoke("hradmin", null);

        Assert.Throws<InvalidOperationException>(() =>
            delegation.Update(DateTime.Today, DateTime.Today.AddDays(1), null, true, null, null));
    }

    // ---- scope ---------------------------------------------------------------

    [Fact]
    public void AllProcesses_CoversEverything()
    {
        var delegation = Make(allProcesses: true);

        Assert.True(delegation.Covers(WorkflowEntityTypes.EmployeeLoan));
        Assert.True(delegation.Covers(WorkflowEntityTypes.AnnualLeave));
        Assert.Empty(delegation.Scopes);
    }

    [Fact]
    public void ScopedDelegation_CoversOnlyWhatItNames()
    {
        var delegation = Make(allProcesses: false,
            types: [WorkflowEntityTypes.AnnualLeave, WorkflowEntityTypes.OtherLeave]);

        Assert.True(delegation.Covers(WorkflowEntityTypes.AnnualLeave));
        Assert.False(delegation.Covers(WorkflowEntityTypes.SalaryRevision));
    }

    [Fact]
    public void ScopeMatchingIgnoresCase()
    {
        var delegation = Make(allProcesses: false, types: ["annualleave"]);

        Assert.True(delegation.Covers("AnnualLeave"));
    }

    [Fact]
    public void DuplicateScopesCollapse()
    {
        var delegation = Make(allProcesses: false,
            types: [WorkflowEntityTypes.AnnualLeave, "AnnualLeave", " AnnualLeave "]);

        Assert.Single(delegation.Scopes);
    }

    [Fact]
    public void SwitchingToAllProcessesClearsTheNamedScopes()
    {
        var delegation = Make(allProcesses: false, types: [WorkflowEntityTypes.AnnualLeave]);

        delegation.Update(delegation.StartDate, delegation.EndDate, null,
            allProcesses: true, entityTypes: null, approvalLimit: null);

        Assert.Empty(delegation.Scopes);
        Assert.True(delegation.Covers(WorkflowEntityTypes.SalaryRevision));
    }

    // ---- the money ceiling ---------------------------------------------------

    [Fact]
    public void NoLimit_CoversAnyAmount()
    {
        var delegation = Make(limit: null);

        Assert.True(delegation.CoversAmount(1_000_000m));
        Assert.True(delegation.CoversAmount(null));
    }

    [Theory]
    [InlineData(24_999, true)]
    [InlineData(25_000, true)]    // the ceiling itself is allowed
    [InlineData(25_001, false)]
    public void ALimitHoldsBackAnythingAboveIt(int amount, bool expected)
    {
        Assert.Equal(expected, Make(limit: 25_000m).CoversAmount(amount));
    }

    /// <summary>
    /// ⚠️ A request whose value cannot be read is treated as WITHIN the ceiling. The limit exists to
    /// hold back a known large amount; making "unknown" mean "blocked" would silently strand every
    /// process that has no amount provider — which is most of them.
    /// </summary>
    [Fact]
    public void AnUnknownAmountIsNotBlockedByALimit()
    {
        Assert.True(Make(limit: 25_000m).CoversAmount(null));
    }
}
