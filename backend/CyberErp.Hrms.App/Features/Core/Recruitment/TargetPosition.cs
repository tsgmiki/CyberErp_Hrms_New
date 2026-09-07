using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    /// <summary>
    /// Which vacant seat a vacancy's hire lands on.
    ///
    /// <para>Extracted so the HIRE and the QUEUE that offers the choice cannot disagree. The queue
    /// pre-selects what this returns, and the hire falls back to the same call when the form sends
    /// nothing — if the two ever drifted, the position shown to HR would not be the one filled.</para>
    /// </summary>
    internal static class TargetPosition
    {
        /// <summary>
        /// A still-vacant slot of the vacancy's role, preferring one in the vacancy's own unit.
        ///
        /// <para>⚠️ The role is the hard filter and the unit only a preference: establishment seats for
        /// the same role can sit in sibling units, and refusing to place someone because the seat is
        /// next door would block a hire the establishment plainly allows.</para>
        /// </summary>
        internal static Task<Guid?> ResolveAsync(
            IRepository<Position> positions, Guid positionClassId, Guid organizationUnitId) =>
            positions.GetAll()
                .Where(p => p.PositionClassId == positionClassId && p.IsVacant)
                .OrderByDescending(p => p.OrganizationUnitId == organizationUnitId ? 1 : 0)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync();
    }
}
