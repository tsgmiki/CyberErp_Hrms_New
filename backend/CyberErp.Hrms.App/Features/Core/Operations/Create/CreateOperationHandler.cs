using CyberErp.Hrms.App.Common.Services;
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
    IRepository<TenantOperation> tenantOperations,
    ICurrentTenantService currentTenant,
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

        // No parent means this row IS one: a menu group, which carries no route and therefore needs
        // its subsystem stated outright. A child takes its parent's, so the branch always agrees.
        Operation operation;
        if (request.ModuleId is null || request.ModuleId == Guid.Empty)
        {
            if (request.SubsystemId is null || request.SubsystemId == Guid.Empty)
                throw new Common.Exceptions.ValidationException("subsystemId",
                    "A menu group needs a subsystem, because it has no parent to inherit one from.");

            operation = Operation.CreateParent(request.SubsystemId.Value, request.Name,
                request.Icon, request.SortOrder);
        }
        else
        {
            // SubSystemId is backed by a real FK, so it must resolve to a live subsystem — an unset
            // Guid would be rejected by the database, not silently stored.
            var subSystemId = await ResolveSubSystemAsync(repository, request.ModuleId!.Value, ct);
            operation = Operation.Create(request.ModuleId.Value, request.Name, request.Link,
                request.Filter, request.Icon, request.SortOrder, subSystemId);
        }

        await repository.AddAsync(operation);
        await unitOfWork.SaveChangesAsync(ct);

        // ⚠️ The tenant copy is created HERE, not by the projector. Core.Operation went global on
        // 2026-08-13, so the projector can no longer tell which templates are ours and only updates
        // copies that already exist. Menu operations are the unit of permission — a screen the
        // runtime cannot resolve is a screen nobody can reach — so this must not be skipped.
        var tenantId = currentTenant.GetCurrentTenantId();
        if (tenantId is not null && tenantId != Guid.Empty)
        {
            await tenantOperations.AddAsync(TenantOperation.Create(
                tenantId.Value, operation.SubSystemId, operation.Id, operation.ModuleId,
                operation.Name, operation.Link, operation.Icon,
                operation.DisplayOrder, operation.IsActive));
            await unitOfWork.SaveChangesAsync(ct);
        }

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

    /// <summary>
    /// The PARENT group's subsystem, so a child always agrees with the branch it hangs off.
    /// Reads Core.Operation, not Core.Module — the parent is an operation with a null ModuleId.
    /// </summary>
    internal static async Task<Guid> ResolveSubSystemAsync(
        IRepository<Operation> operations, Guid parentId, CancellationToken ct)
    {
        var parent = await operations.GetAll()
            .Where(o => o.Id == parentId)
            .Select(o => new { o.SubSystemId, o.ModuleId })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Operation), parentId.ToString());

        // Only a group can hold children. Letting a screen become a parent would build a menu the
        // sidebar cannot render, because it only ever descends one level.
        if (parent.ModuleId is not null)
            throw new Common.Exceptions.ValidationException("moduleId",
                "The selected parent is a screen, not a menu group. Pick a group instead.");

        return parent.SubSystemId;
    }
}