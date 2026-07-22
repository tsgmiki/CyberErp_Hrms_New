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
    IRepository<Subsystem> subsystemRepository,
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

        var subsystemExists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .AnyAsync(subsystemRepository.GetAll(), s => s.Id == request.SubsystemId, ct);
        if (!subsystemExists)
            throw new Common.Exceptions.ValidationException(nameof(request.SubsystemId), "Subsystem not found.");

        var module = Module.Create(request.SubsystemId, request.Name, request.Icon, request.SortOrder);

        await repository.AddAsync(module);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Module created with Id: {Id}", module.Id);

        return new ModuleResult
        {
            Id = module.Id,
            SubsystemId = module.SubsystemId,
            Name = module.Name,
            Icon = module.Icon,
            CreatedAt = module.CreatedAt.InUtc().ToString()
        };
    }
}