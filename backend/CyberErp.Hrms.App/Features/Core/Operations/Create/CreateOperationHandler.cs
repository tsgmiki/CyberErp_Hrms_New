using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Operations.Create;

public class CreateOperationHandler(
    IRepository<Operation> repository,
    IRepository<Module> moduleRepository,
    IUnitOfWork unitOfWork,
    ITenantAuthorizationProjector projector,
    IValidator<CreateOperationRequest> validator,
    ILogger<CreateOperationHandler> logger)
    : IFeatureHandler<CreateOperationRequest, OperationResult>
{
    public async Task<OperationResult> Handle(CreateOperationRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new AppValidationException(validationResult.Errors);

        // SubSystemId is denormalised from the module and backed by a real FK, so it must resolve to
        // a live subsystem — an unset Guid would be rejected by the database, not silently stored.
        var subSystemId = await ResolveSubSystemAsync(moduleRepository, request.ModuleId, ct);

        var operation = Operation.Create(request.ModuleId, request.Name, request.Link, request.Filter,
            request.Icon, request.SortOrder, subSystemId);

        await repository.AddAsync(operation);
        await unitOfWork.SaveChangesAsync(ct);
        // Menu operations are the unit of permission, so the tenant copy has to follow the template
        // immediately: a screen the runtime cannot resolve is a screen nobody can reach.
        await projector.SyncAsync(ct);

        logger.LogInformation("Operation created with Id: {Id}", operation.Id);

        return new OperationResult
        {
            Id = operation.Id,
            Name = operation.Name,
            Link = operation.Link,
            Icon = operation.Icon
        };
    }

    /// <summary>The module's subsystem, or a clean validation error when the module is unknown.</summary>
    internal static async Task<Guid> ResolveSubSystemAsync(
        IRepository<Module> modules, Guid moduleId, CancellationToken ct)
    {
        var subSystemId = await modules.GetAll()
            .Where(m => m.Id == moduleId)
            .Select(m => (Guid?)m.SubsystemId)
            .FirstOrDefaultAsync(ct);

        return subSystemId
            ?? throw new NotFoundException(nameof(Module), moduleId.ToString());
    }
}