using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Modules.Delete;

public class DeleteModuleHandler(
    IRepository<Module> repository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteModuleHandler> logger)
    : IFeatureHandler<DeleteModuleRequest, ModuleResult?>
{
    public async Task<ModuleResult?> Handle(DeleteModuleRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting Module with Id: {Id}", request.Id);

        var module = await repository.GetAll()
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync();
        if (module == null)
        {
            logger.LogWarning("Module with ID: {Id} not found", request.Id);
            return null;
        }

        repository.Delete(module);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Module deleted successfully with ID: {Id}", module.Id);

        return new ModuleResult
        {
            Id = module.Id,
            SubSystem = module.SubSystem,
            Name = module.Name,
            Icon = module.Icon,
            CreatedBy = module.CreatedBy,
            CreatedAt = module.CreatedAt.InUtc().ToString(),
            UpdatedBy = module.UpdatedBy,
            UpdatedAt = module.UpdatedAt?.InUtc().ToString()
        };
    }
}