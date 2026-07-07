using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Modules.Create;

public class CreateModuleHandler(
    IRepository<Module> repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateModuleRequest> validator,
    ILogger<CreateModuleHandler> logger)
    : IFeatureHandler<CreateModuleRequest, ModuleResult>
{
    public async Task<ModuleResult> Handle(CreateModuleRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new AppValidationException(validationResult.Errors);

        var module = Module.Create(request.SubSystem, request.Name, request.Icon);

        await repository.AddAsync(module);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Module created with Id: {Id}", module.Id);

        return new ModuleResult
        {
            Id = module.Id,
            SubSystem = module.SubSystem,
            Name = module.Name,
            Icon = module.Icon,
            CreatedAt = module.CreatedAt.InUtc().ToString()
        };
    }
}