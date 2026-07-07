using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Modules.GetAll;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CyberErp.Hrms.Inf.Repositories.Core.Modules;

public class GetAllModuleRepository(
    IRepository<Module> moduleRepository,
    ILogger<GetAllModuleRepository> logger) : IGetAllModuleRepository
{
    private readonly IRepository<Module> _moduleRepository = moduleRepository;
    private readonly ILogger<GetAllModuleRepository> _logger = logger;

    public async Task<PaginatedResponse<GetModuleDto>> GetAllAsync(GetAllRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting all Modules");

        IQueryable<Module> query = _moduleRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchLower = request.SearchText.ToLower();
            query = query.Where(m => m.SubSystem.ToLower().Contains(searchLower) ||
                                     m.Name.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync(ct);

        if (!string.IsNullOrWhiteSpace(request.SortCol))
        {
            string sortCol = char.ToUpper(request.SortCol[0]) + request.SortCol.Substring(1);
            if (request.Dir?.ToLower() == "desc")
            {
                query = query.OrderByDescending(m => EF.Property<object>(m, sortCol));
            }
            else
            {
                query = query.OrderBy(m => EF.Property<object>(m, sortCol));
            }
        }

        int skip = int.TryParse(request.Skip, out var s) ? s : 0;
        int take = int.TryParse(request.Take, out var t) ? t : 10;
        query = query.Skip(skip).Take(take);

        var data = await query.Select(m => new GetModuleDto
        {
            Id = m.Id,
            SubSystem = m.SubSystem,
            Name = m.Name,
            Icon = m.Icon
        }).ToListAsync(ct);

        return new PaginatedResponse<GetModuleDto>
        {
            Total = totalCount,
            Data = data
        };
    }
}