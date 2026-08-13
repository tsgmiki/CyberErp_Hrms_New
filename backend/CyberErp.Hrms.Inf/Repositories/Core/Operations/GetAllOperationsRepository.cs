using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Operations.GetAll;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CyberErp.Hrms.Inf.Repositories.Core.Operations;

public class GetAllOperationsRepository(
    IRepository<Operation> operationsRepository,
    IRepository<Subsystem> subsystems,
    ILogger<GetAllOperationsRepository> logger) : IGetAllOperationsRepository
{
    private readonly IRepository<Operation> _operationsRepository = operationsRepository;
    private readonly IRepository<Subsystem> _subsystems = subsystems;
    private readonly ILogger<GetAllOperationsRepository> _logger = logger;

    public async Task<PaginatedResponse<OperationDto>> GetAllAsync(GetAllOperationsRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting all Operations");

        var query = _operationsRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            query = query.Where(x => x.Name.Contains(request.SearchText) || x.Link.Contains(request.SearchText));
        }

        // Cascading central-administration filters: Subsystem → parent group (both optional).
        // SubSystemId is carried on the row itself now, so this no longer joins through Core.Module.
        if (request.SubsystemId.HasValue)
            query = query.Where(x => x.SubSystemId == request.SubsystemId.Value);
        if (request.ModuleId.HasValue)
            query = query.Where(x => x.ModuleId == request.ModuleId.Value);

        var totalCount = await query.CountAsync(ct);

        // Natural menu order: group, then screen within it. A group sorts with its own children by
        // falling back to its own DisplayOrder when it has no parent.
        query = query
            .OrderBy(x => x.Parent != null ? x.Parent.DisplayOrder : x.DisplayOrder)
            .ThenBy(x => x.ModuleId == null ? 0 : 1)   // the group itself leads its children
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name);

        var skip = int.Parse(request.Skip ?? "0");
        var take = int.Parse(request.Take ?? "10");

        var items = await query
            .Skip(skip)
            .Take(take)
            .Select(x => new OperationDto
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                Name = x.Name,
                Module = x.Parent != null ? x.Parent.Name : string.Empty,
                SubsystemId = x.SubSystemId,
                SubSystem = _subsystems.GetAll().Where(s => s.Id == x.SubSystemId)
                    .Select(s => s.Name).FirstOrDefault() ?? string.Empty,
                Link = x.Link,
                Filter = x.Filter,
                Icon = x.Icon,
                SortOrder = x.DisplayOrder
            })
            .ToListAsync(ct);

        return new PaginatedResponse<OperationDto>
        {
            Total = totalCount,
            Data = items
        };
    }
}