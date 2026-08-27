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
    }
}
