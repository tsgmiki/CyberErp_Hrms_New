using CyberErp.Hrms.App.Common;

namespace CyberErp.Hrms.Tests.OrganizationUnits;

/// <summary>
/// The two rules drag-and-drop makes reachable with a single gesture: where a dropped unit lands
/// among its siblings, and the refusal to drop a unit inside its own subtree.
/// </summary>
public class OrgTreeDragDropTests
{
    private static readonly Guid A = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid B = new("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Guid C = new("cccccccc-0000-0000-0000-000000000003");
    private static readonly Guid D = new("dddddddd-0000-0000-0000-000000000004");

    // ---- SiblingOrder.Place -------------------------------------------------

    [Fact]
    public void Place_WithNoAnchor_PutsTheUnitFirst()
    {
        var order = SiblingOrder.Place([A, B, C], D, afterId: null);

        Assert.Equal([D, A, B, C], order.Select(o => o.Id));
    }

    [Fact]
    public void Place_AfterAMiddleSibling_InsertsBetweenThem()
    {
        var order = SiblingOrder.Place([A, B, C], D, afterId: A);

        Assert.Equal([A, D, B, C], order.Select(o => o.Id));
    }

    [Fact]
    public void Place_AfterTheLastSibling_AppendsToTheEnd()
    {
        var order = SiblingOrder.Place([A, B, C], D, afterId: C);

        Assert.Equal([A, B, C, D], order.Select(o => o.Id));
    }

    [Fact]
    public void Place_AlwaysNumbersInEvenGaps()
    {
        var order = SiblingOrder.Place([A, B, C], D, afterId: A);

        Assert.Equal([10, 20, 30, 40], order.Select(o => o.Rank));
    }

    /// <summary>
    /// A reorder WITHIN a parent hands us a list that already contains the dragged unit. It has to
    /// be lifted out before it is inserted, or the level would end up holding it twice.
    /// </summary>
    [Fact]
    public void Place_WhenReorderingInsideTheSameParent_DoesNotDuplicateTheUnit()
    {
        var order = SiblingOrder.Place([A, B, C], A, afterId: C);

        Assert.Equal([B, C, A], order.Select(o => o.Id));
        Assert.Equal(3, order.Count);
    }

    [Fact]
    public void Place_MovingTheFirstUnitToTheFront_IsANoOp()
    {
        var order = SiblingOrder.Place([A, B, C], A, afterId: null);

        Assert.Equal([A, B, C], order.Select(o => o.Id));
    }

    /// <summary>Defensive: the handler rejects an unknown anchor, so this must stay predictable.</summary>
    [Fact]
    public void Place_WithAnAnchorThatIsNotThere_FallsBackToFirst()
    {
        var order = SiblingOrder.Place([A, B], C, afterId: D);

        Assert.Equal([C, A, B], order.Select(o => o.Id));
    }

    [Fact]
    public void Place_IntoAnEmptyLevel_GivesTheUnitTheFirstRank()
    {
        var order = SiblingOrder.Place([], A, afterId: null);

        Assert.Equal([(A, 10)], order);
    }

    // ---- HierarchyGuard, as drag-and-drop reaches it ------------------------

    /// <summary>A → B → C. Dropping A onto C would detach the whole branch from every root.</summary>
    [Fact]
    public void Cycle_DroppingAUnitOntoItsOwnGrandchild_IsRefused()
    {
        var tree = new Dictionary<Guid, Guid?> { [A] = null, [B] = A, [C] = B };

        Assert.True(HierarchyGuard.WouldCreateCycle(tree, A, C));
    }

    [Fact]
    public void Cycle_DroppingAUnitOntoItsOwnChild_IsRefused()
    {
        var tree = new Dictionary<Guid, Guid?> { [A] = null, [B] = A };

        Assert.True(HierarchyGuard.WouldCreateCycle(tree, A, B));
    }

    [Fact]
    public void Cycle_DroppingAParentOntoAnUnrelatedBranch_IsAllowed()
    {
        // A → B, and C standing alone. Moving A under C is a perfectly ordinary reorganisation.
        var tree = new Dictionary<Guid, Guid?> { [A] = null, [B] = A, [C] = null };

        Assert.False(HierarchyGuard.WouldCreateCycle(tree, A, C));
    }

    [Fact]
    public void Cycle_PromotingAUnitToTheRoot_IsAllowed()
    {
        var tree = new Dictionary<Guid, Guid?> { [A] = null, [B] = A };

        Assert.False(HierarchyGuard.WouldCreateCycle(tree, B, null));
    }

    /// <summary>
    /// A move onto a child of the dragged unit's own child — the case a one-level check misses and
    /// the reason the guard walks the whole ancestor chain rather than comparing a single parent.
    /// </summary>
    [Fact]
    public void Cycle_DepthDoesNotHelp_AFourLevelDescendantIsStillRefused()
    {
        var tree = new Dictionary<Guid, Guid?> { [A] = null, [B] = A, [C] = B, [D] = C };

        Assert.True(HierarchyGuard.WouldCreateCycle(tree, A, D));
    }
}
