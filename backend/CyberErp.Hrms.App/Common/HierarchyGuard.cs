namespace CyberErp.Hrms.App.Common
{
    /// <summary>
    /// Multi-level cycle detection for self-referencing hierarchies (org units, work
    /// locations, reporting lines). Neither the entity nor the DB can express "the new
    /// parent must not be a descendant of this node", so handlers enforce it here.
    /// </summary>
    public static class HierarchyGuard
    {
        /// <summary>
        /// True if pointing <paramref name="entityId"/> at <paramref name="proposedParentId"/>
        /// would create a cycle, given a map of every node's id → parentId.
        /// </summary>
        public static bool WouldCreateCycle(
            IReadOnlyDictionary<Guid, Guid?> parentMap,
            Guid entityId,
            Guid? proposedParentId)
        {
            var current = proposedParentId;
            var hops = 0;
            while (current.HasValue)
            {
                if (current.Value == entityId) return true;
                if (++hops > parentMap.Count) return true; // defensive: pre-existing cycle
                current = parentMap.TryGetValue(current.Value, out var parent) ? parent : null;
            }
            return false;
        }
    }
}
