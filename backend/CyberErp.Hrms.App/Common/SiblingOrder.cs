namespace CyberErp.Hrms.App.Common
{
    /// <summary>
    /// Places a node among its siblings after a drag-and-drop, and hands back the new ranks.
    /// </summary>
    /// <remarks>
    /// Pure and separate from the handler so the arithmetic can be tested without a database. The
    /// handler owns the questions only the data can answer — does the parent exist, would this be a
    /// cycle, is the anchor really a sibling — and this owns the ordering itself.
    /// </remarks>
    public static class SiblingOrder
    {
        /// <summary>
        /// Distance between adjacent siblings. Sparse on purpose: a later insertion between two
        /// nodes has somewhere to go, so an ordinary drop usually rewrites one row rather than a
        /// level. <see cref="Place"/> still renumbers the level outright, which keeps the gaps even
        /// no matter how many times the same level is rearranged.
        /// </summary>
        public const int Gap = 10;

        /// <summary>
        /// Insert <paramref name="movedId"/> into <paramref name="siblingsInOrder"/> immediately
        /// after <paramref name="afterId"/> — or first when that is <c>null</c> — and return every
        /// sibling's new rank, in order.
        /// </summary>
        /// <param name="siblingsInOrder">
        /// The destination's current children, already ordered. May or may not contain
        /// <paramref name="movedId"/>: a reorder within the same parent does, a reparent does not,
        /// and the caller should not have to care — it is removed before insertion either way.
        /// </param>
        /// <remarks>
        /// An anchor that is not in the list places the node FIRST rather than throwing. The
        /// handler rejects an unknown anchor before it gets here; this is the defensive branch, and
        /// a predictable position beats an exception deep inside a resequence.
        /// </remarks>
        public static List<(Guid Id, int Rank)> Place(
            IEnumerable<Guid> siblingsInOrder,
            Guid movedId,
            Guid? afterId)
        {
            var ordered = siblingsInOrder.Where(id => id != movedId).ToList();

            var insertAt = 0;
            if (afterId.HasValue && afterId.Value != movedId)
            {
                var anchor = ordered.IndexOf(afterId.Value);
                if (anchor >= 0) insertAt = anchor + 1;
            }

            ordered.Insert(insertAt, movedId);
            return [.. ordered.Select((id, i) => (id, (i + 1) * Gap))];
        }
    }
}
