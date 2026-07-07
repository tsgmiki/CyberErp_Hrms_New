using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Modules.Update;

public class UpdateModuleHandler(
    IRepository<Module> repository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateModuleRequest> validator,
    ILogger<UpdateModuleHandler> logger)
    : IFeatureHandler<UpdateModuleRequest, ModuleResult>
{
    public async Task<ModuleResult> Handle(UpdateModuleRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new AppValidationException(validationResult.Errors);

        var module = await repository.GetAll()
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync();
        if (module == null)
            throw new NotFoundException(nameof(Module), request.Id.ToString());

        module.Update(request.SubSystem, request.Name, request.Icon);

        repository.UpdateAsync(module);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Module updated with Id: {Id}", module.Id);

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