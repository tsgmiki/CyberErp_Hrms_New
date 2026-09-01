using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Performance;

namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// "May this caller act on a record belonging to THIS organization unit?" — HR administrators
    /// anywhere, a manager only inside their own unit subtree.
    ///
    /// <para>This is not a new policy. <c>SaveHiringRequest</c> has always guarded creation exactly
    /// this way; the rest of the recruitment and workforce-planning lifecycle simply never asked.
    /// Extracted so seventeen endpoints share one definition rather than seventeen copies that drift
    /// (logic §12.63).</para>
    /// </summary>
    public static class UnitScopeGuard
    {
        /// <summary>
        /// Throws unless the caller is an HR administrator, or a manager whose subtree contains
        /// <paramref name="organizationUnitId"/>.
        ///
        /// <para>⚠️ A NULL unit means the record is organisation-wide (a workforce plan with no unit),
        /// and no single manager owns it — HR only. Treating null as "no unit to check, therefore
        /// allowed" would make the widest records the least protected.</para>
        /// </summary>
        /// <param name="action">Completes the sentence "You can only … for your own department and its sub-departments."</param>
        public static async Task EnsureCanActOnUnitAsync(
            IPerformanceVisibilityService visibility,
            Guid? organizationUnitId,
            string action,
            string field = "organizationUnitId")
        {
            var scope = await visibility.GetScopeAsync();
            if (scope.IsAdmin) return;

            // Its own sentence rather than splicing the action in: the action reads as a verb phrase
            // ("delete workforce plans", "plan headcount") and no single connector fits both.
            if (organizationUnitId is null || organizationUnitId == Guid.Empty)
                throw new ValidationException(field,
                    "This record is not tied to a single department, so only HR can change it.");

            if (!scope.UnitIds.Contains(organizationUnitId.Value))
                throw new ValidationException(field,
                    $"You can only {action} for your own department and its sub-departments.");
        }

        /// <summary>
        /// The units whose records the caller may READ, or <c>null</c> meaning "no restriction".
        ///
        /// <para>The read-side counterpart of <see cref="EnsureCanActOnUnitAsync"/>: HR sees the whole
        /// organization, a manager sees their own unit subtree, and anyone else sees an EMPTY set —
        /// which filters a list down to no rows rather than opening it up. That is the right default
        /// here because raising the record already requires unit standing, so a person with no units
        /// cannot own one of these records in the first place.</para>
        ///
        /// <para>⚠️ Null means unrestricted and an EMPTY SET means nothing. They are opposite answers
        /// and a caller that treats a null as "nothing to filter on" inverts the rule — hence the
        /// nullable return rather than an empty-set sentinel.</para>
        /// </summary>
        public static async Task<HashSet<Guid>?> ReadableUnitIdsAsync(IPerformanceVisibilityService visibility)
        {
            var scope = await visibility.GetScopeAsync();
            return scope.IsAdmin ? null : scope.UnitIds;
        }

        /// <summary>
        /// Throws unless the caller may READ a record belonging to <paramref name="organizationUnitId"/>.
        ///
        /// <para>The single-record twin of <see cref="ReadableUnitIdsAsync"/>. Filtering the list alone
        /// is not enough: ids leak legitimately — the approval inbox hands out the EntityId of every
        /// instance it lists — so an unscoped by-id read stays reachable by anyone who has seen one.</para>
        /// </summary>
        public static async Task EnsureCanReadUnitAsync(
            IPerformanceVisibilityService visibility,
            Guid? organizationUnitId,
            string what)
        {
            var readable = await ReadableUnitIdsAsync(visibility);
            if (readable is null) return;
            if (organizationUnitId is null || !readable.Contains(organizationUnitId.Value))
                throw new ValidationException("access",
                    $"You do not have access to this {what}. It belongs to another department.");
        }
    }
}
