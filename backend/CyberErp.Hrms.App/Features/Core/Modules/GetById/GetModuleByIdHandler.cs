using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Modules.GetById;

public class GetModuleByIdHandler(
    IRepository<Module> repository,
    ILogger<GetModuleByIdHandler> logger)
    : IFeatureHandler<GetModuleByIdRequest, GetModuleDto?>
{
    public async Task<GetModuleDto?> Handle(GetModuleByIdRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Getting Module with ID: {Id}", request.Id);

        var module = await repository.GetAll()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetModuleDto
            {
                Id = x.Id,
                SubSystem = x.SubSystem,
                Name = x.Name,
                Icon = x.Icon
            })
            .FirstOrDefaultAsync(ct);

        return module;
    }
}