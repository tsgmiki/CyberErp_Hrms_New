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
    ILogger<GetAllOperationsRepository> logger) : IGetAllOperationsRepository
{
    private readonly IRepository<Operation> _operationsRepository = operationsRepository;
    private readonly ILogger<GetAllOperationsRepository> _logger = logger;

    public async Task<PaginatedResponse<OperationDto>> GetAllAsync(GetAllOperationsRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting all Operations");

        var query = _operationsRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            query = query.Where(x => x.Name.Contains(request.SearchText) || x.Link.Contains(request.SearchText));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderByDescending(x => x.CreatedAt);

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
                Module = x.Module.Name,
                Link = x.Link,
                Filter = x.Filter,
                Icon = x.Icon
            })
            .ToListAsync(ct);

        return new PaginatedResponse<OperationDto>
        {
            Total = totalCount,
            Data = items
        };
    }
}