using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Delegations;

/// <summary>
/// The acting-compensation record: its lifecycle, and the two salary snapshots that decide what a
/// deputy is paid and what they go back to.
/// </summary>
public class ActingAssignmentTests
{
    private static readonly Guid Delegation = new("dddddddd-0000-0000-0000-000000000001");
    private static readonly Guid Deputy = new("eeeeeeee-0000-0000-0000-000000000002");
    private static readonly Guid Covered = new("ffffffff-0000-0000-0000-000000000003");
    private static readonly Guid Post = new("aaaaaaaa-0000-0000-0000-000000000004");

    private static ActingAssignment Make(decimal acting = 58_000m, decimal? original = 40_000m) =>
        ActingAssignment.Create(Delegation, Deputy, Covered, Post, "Finance Director",
            new DateTime(2026, 1, 1), new DateTime(2026, 6, 30), acting, original);

    // ---- construction --------------------------------------------------------

    [Fact]
    public void ItStartsPending_SoNoPayHasChanged()
    {
        Assert.Equal(ActingAssignmentStatus.PendingApproval, Make().Status);
    }

    [Fact]
    public void CannotEndBeforeItStarts()
    {
        Assert.Throws<ArgumentException>(() =>
            ActingAssignment.Create(Delegation, Deputy, Covered, Post, "X",
                new DateTime(2026, 6, 30), new DateTime(2026, 1, 1), 1m, null));
    }

    [Fact]
    public void ANegativeActingSalaryIsRefused()
    {
        Assert.Throws<ArgumentException>(() =>
            ActingAssignment.Create(Delegation, Deputy, Covered, Post, "X",
                DateTime.Today, DateTime.Today, -1m, null));
    }

    [Fact]
    public void DurationIsInclusiveOfBothEnds()
    {
        var a = ActingAssignment.Create(Delegation, Deputy, Covered, Post, "X",
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 10), 1m, null);

        Assert.Equal(10, a.DurationDays);
    }

    // ---- lifecycle -----------------------------------------------------------

    [Fact]
    public void ActivateMovesItLive()
    {
        var a = Make();
        a.Activate();

        Assert.Equal(ActingAssignmentStatus.Active, a.Status);
        Assert.NotNull(a.ActivatedAt);
    }

    [Fact]
    public void ItCannotBeActivatedTwice()
    {
        var a = Make();
        a.Activate();

        Assert.Throws<InvalidOperationException>(() => a.Activate());
    }

    [Fact]
    public void ARejectedAssignmentCannotLaterActivate()
    {
        var a = Make();
        a.Reject("not approved");

        Assert.Equal(ActingAssignmentStatus.Rejected, a.Status);
        Assert.Throws<InvalidOperationException>(() => a.Activate());
    }

    [Fact]
    public void OnlyAnActiveAssignmentCanConclude()
    {
        Assert.Throws<InvalidOperationException>(() => Make().Conclude(experienceRecorded: true));
    }

    [Fact]
    public void ConcludingRecordsWhetherExperienceWasWritten()
    {
        var a = Make();
        a.Activate();
        a.Conclude(experienceRecorded: true);

        Assert.Equal(ActingAssignmentStatus.Concluded, a.Status);
        Assert.True(a.ExperienceRecorded);
        Assert.NotNull(a.ConcludedAt);
    }

    // ---- cancellation: the caller needs to know whether pay had started -------

    /// <summary>
    /// ⚠️ The return value is the whole point. An assignment cancelled while still PENDING never
    /// touched the employee, so unwinding pay would put them on a salary they were never moved off.
    /// </summary>
    [Fact]
    public void CancellingAPendingAssignment_ReportsThatPayNeverStarted()
    {
        Assert.False(Make().Cancel("delegation withdrawn"));
    }

    [Fact]
    public void CancellingAnActiveAssignment_ReportsThatPayMustBeUnwound()
    {
        var a = Make();
        a.Activate();

        Assert.True(a.Cancel("delegation withdrawn"));
        Assert.Equal(ActingAssignmentStatus.Cancelled, a.Status);
    }

    [Fact]
    public void CancellingATerminalAssignmentChangesNothing()
    {
        var a = Make();
        a.Activate();
        a.Conclude(experienceRecorded: false);

        Assert.False(a.Cancel("late"));
        Assert.Equal(ActingAssignmentStatus.Concluded, a.Status);
    }

    // ---- the original-salary snapshot ----------------------------------------

    /// <summary>
    /// ⚠️ Approval can land weeks after the assignment was raised. Re-capturing means an ordinary
    /// increment in between survives the eventual revert instead of being erased by it.
    /// </summary>
    [Fact]
    public void TheOriginalSalaryCanBeRecapturedBeforeActivation()
    {
        var a = Make(original: 40_000m);
        a.CaptureOriginalSalary(42_000m);   // they got an increment while it sat pending

        Assert.Equal(42_000m, a.OriginalSalary);
    }

    [Fact]
    public void TheOriginalSalaryCannotBeChangedOnceActive()
    {
        var a = Make();
        a.Activate();

        Assert.Throws<InvalidOperationException>(() => a.CaptureOriginalSalary(99_000m));
    }

    /// <summary>A deputy with no recorded salary reverts to none, not to zero.</summary>
    [Fact]
    public void ADeputyWithNoSalaryKeepsANullOriginal()
    {
        Assert.Null(Make(original: null).OriginalSalary);
    }
}
