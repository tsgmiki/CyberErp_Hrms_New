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
     * The read stays because the catalogue is shared infrastructure: the Home portal deep-links through
     * Subsystem.Url, and the HRMS menu filters group modules by subsystem.
     */

    public class SubsystemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        /// <summary>Where the subsystem's app lives — the Home portal launcher deep-links here.</summary>
        public string? Url { get; set; }
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
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(x => new SubsystemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    SortOrder = x.SortOrder,
                    Url = x.Url
                })
                .ToListAsync();

            return new PaginatedResponse<SubsystemDto> { Total = total, Data = data };
        }
    }
}
