using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Subsystems
{
    /*
     * ⚠️ READ-ONLY SINCE 2026-08-14 — the SubSystems module was removed from HRMS; SRMS owns the
     * subsystem catalogue in this same CERP database. SaveSubsystem and DeleteSubsystem are gone.
     *
     * The read stays because the catalogue is shared infrastructure: the HRMS menu filters group
     * modules by subsystem, and the landing page lists them.
     *
     * ⚠️ Url is GONE from the wire (2026-08-16) with the column — subsystem application addresses
     * are environment configuration now, resolved client-side from VITE_SUBSYSTEM_APPS by
     * Abbreviation.
     * SortOrder went the same way; DisplayOrder is the surviving ordering column.
     */

    public class SubsystemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// The subsystem's stable short identifier (SSMS, HRMS, SAMS, SRMS) —
        /// Core.Subsystem.Abbreviation. THE key the SPA matches on: the launcher's HOME exclusion
        /// and the app-URL registry.
        /// </summary>
        /// <remarks>
        /// ⚠️ Replaced `Code` on 2026-08-19, matching the Home portal. Code is not dependable as a
        /// key — the same catalogue holds 'HOME', '002', 'srms' and 'Finance', and it is re-typed by
        /// hand, so a mismatch silently mis-routes the launcher rather than failing loudly.
        /// The column is NULLABLE, hence the Code fallback where it is projected.
        /// </remarks>
        public string Abbreviation { get; set; } = string.Empty;
        /// <summary>lucide-react icon name — the landing-page cards resolve it (2026-08-14).</summary>
        public string? Icon { get; set; }
        /// <summary>Launcher ordering (Core.Subsystem.DisplayOrder).</summary>
        public int DisplayOrder { get; set; }
    }

    public interface IGetAllSubsystems { Task<PaginatedResponse<SubsystemDto>> GetAsync(GetAllRequest request); }

    public class GetAllSubsystems(IRepository<Subsystem> repository) : IGetAllSubsystems
    {
        public async Task<PaginatedResponse<SubsystemDto>> GetAsync(GetAllRequest request)
        {
            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                // Searches the ABBREVIATION (what callers now see) and still Code, so a search that matches
                // what someone remembers from the old field keeps working.
                query = query.Where(s =>
                    s.Name.Contains(request.SearchText)
                    || (s.Abbreviation != null && s.Abbreviation.Contains(request.SearchText))
                    || s.Code.Contains(request.SearchText));

            var total = await query.CountAsync();

            int skip = int.TryParse(request.Skip, out var s) ? s : 0;
            int take = int.TryParse(request.Take, out var t) ? t : 15;

            var data = await query
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(x => new SubsystemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    // Abbreviation is NULLABLE; falling back to Code keeps a subsystem addressable
                    // rather than handing the SPA an empty key it cannot match.
                    Abbreviation = string.IsNullOrWhiteSpace(x.Abbreviation) ? x.Code : x.Abbreviation,
                    Icon = x.Icon,
                    DisplayOrder = x.DisplayOrder
                })
                .ToListAsync();

            return new PaginatedResponse<SubsystemDto> { Total = total, Data = data };
        }
    }
}
