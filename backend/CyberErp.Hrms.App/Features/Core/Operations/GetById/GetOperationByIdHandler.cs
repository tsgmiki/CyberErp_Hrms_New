using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Operations.GetById;

public class GetOperationByIdHandler(
    IRepository<Operation> repository,
    IRepository<Subsystem> subsystems,
    ILogger<GetOperationByIdHandler> logger)
    : IFeatureHandler<GetOperationByIdRequest, OperationDto?>
{
    public async Task<OperationDto?> Handle(GetOperationByIdRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Getting Operation with ID: {Id}", request.Id);

        var operation = await repository.GetAll()
            .Include(x => x.Module)
            .Where(x => x.Id == request.Id)
            .Select(x => new OperationDto
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                Name = x.Name,
                // The menu GROUP (Core.Module); empty on a legacy group row.
                Module = x.Module != null ? x.Module.Name : string.Empty,
                SubsystemId = x.SubSystemId,
                SubSystem = subsystems.GetAll().Where(s => s.Id == x.SubSystemId)
                    .Select(s => s.Name).FirstOrDefault() ?? string.Empty,
                Link = x.Link,
                Filter = x.Filter,
                Icon = x.Icon,
                SortOrder = x.DisplayOrder
            })
            .FirstOrDefaultAsync(ct);

        return operation;
    }
}