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
     * are environment configuration now, resolved client-side from VITE_SUBSYSTEM_APPS by Code.
     * SortOrder went the same way; DisplayOrder is the surviving ordering column.
     */

    public class SubsystemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
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
                query = query.Where(s => s.Name.Contains(request.SearchText) || s.Code.Contains(request.SearchText));

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
                    Code = x.Code,
                    Icon = x.Icon,
                    DisplayOrder = x.DisplayOrder
                })
                .ToListAsync();

            return new PaginatedResponse<SubsystemDto> { Total = total, Data = data };
        }
    }
}
