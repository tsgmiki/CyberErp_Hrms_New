using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    /// <summary>A role you can actually hire into for a given unit, with the seats left.</summary>
    public class VacantRoleDto
    {
        /// <summary>The POSITION CLASS id — this is what a hiring request stores, so it is the dropdown's value.</summary>
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Code { get; set; }
        /// <summary>Vacant seats of this role in the unit — the ceiling HC082 checks the request against.</summary>
        public int VacantSeats { get; set; }
    }

    public interface IGetVacantRoles { Task<List<VacantRoleDto>> GetAsync(Guid organizationUnitId); }

    /// <summary>
    /// The roles with a vacant seat in one unit — what the hiring-request form's role picker offers.
    ///
    /// <para>⚠️ This deliberately mirrors <see cref="RecruitmentShared.VacantSeatsAsync"/>, the HC082
    /// establishment gate, predicate for predicate: same unit equality (EXACT unit, never the
    /// subtree — a seat belongs to the unit that holds it) and the same <c>IsVacant</c> flag. The
    /// picker previously listed the whole catalogue, so a manager could choose a role with no seat,
    /// save the draft, and only be refused at SUBMIT — the gate runs there, not on save. Offering
    /// only what the gate accepts moves that failure out of the workflow and into the dropdown.</para>
    ///
    /// <para>Because both read the same predicate, a divergence is a code change in one of two
    /// adjacent methods rather than a slow drift between a screen and a rule.</para>
    /// </summary>
    public class GetVacantRoles(
        IRepository<Position> positions,
        IPerformanceVisibilityService visibility) : IGetVacantRoles
    {
        public async Task<List<VacantRoleDto>> GetAsync(Guid organizationUnitId)
        {
            // No unit chosen yet: an empty list, not every role. The form asks for the unit first.
            if (organizationUnitId == Guid.Empty) return [];

            // Same scope rule as raising the request itself — establishment counts are another
            // department's business. HR (IsAdmin) passes through for any unit.
            await UnitScopeGuard.EnsureCanActOnUnitAsync(visibility, organizationUnitId, "view vacancies");

            // Grouped in memory ON PURPOSE: this is the vacant seats of ONE unit — tens of rows —
            // and it keeps the projection a plain translatable Select instead of a GroupBy whose
            // key spans a navigation.
            var vacant = await positions.GetAll()
                .Where(p => p.OrganizationUnitId == organizationUnitId && p.IsVacant && p.PositionClass != null)
                .Select(p => new
                {
                    p.PositionClassId,
                    Title = p.PositionClass!.Title,
                    p.PositionClass!.Code
                })
                .ToListAsync();

            return [.. vacant
                .GroupBy(v => new { v.PositionClassId, v.Title, v.Code })
                .Select(g => new VacantRoleDto
                {
                    Id = g.Key.PositionClassId,
                    Title = g.Key.Title,
                    Code = g.Key.Code,
                    VacantSeats = g.Count()
                })
                .OrderBy(r => r.Title)];
        }
    }
}
