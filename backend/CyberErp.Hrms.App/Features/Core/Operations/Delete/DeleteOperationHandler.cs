using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Operations.Delete;

public class DeleteOperationHandler(
    IRepository<Operation> repository,
    IRepository<TenantOperation> tenantOperationRepository,
    IRepository<TenantRolePermission> tenantRolePermissionRepository,
    IUnitOfWork unitOfWork,
    ITenantAuthorizationProjector projector,
    ILogger<DeleteOperationHandler> logger)
    : IFeatureHandler<DeleteOperationRequest, OperationResult?>
{
    public async Task<OperationResult?> Handle(DeleteOperationRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting Operation with Id: {Id}", request.Id);

        var operation = await repository.GetAll()
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync();
        if (operation == null)
        {
            logger.LogWarning("Operation with ID: {Id} not found", request.Id);
            return null;
        }

        // A group cannot be deleted while screens still hang off it. The self-referencing foreign key
        // is NoAction — SQL Server will not allow a cascading one — so the database would simply
        // refuse, with an error naming a constraint rather than the actual problem. Say it plainly.
        if (operation.IsParent)
        {
            var childCount = await repository.GetAll().CountAsync(o => o.ModuleId == request.Id, ct);
            if (childCount > 0)
                throw new ValidationException(nameof(request.Id),
                    $"This menu group still contains {childCount} screen(s). Move or delete them first.");
        }

        // TenantOperation -> Operation is Restrict, so the tenant copy and its grants must go FIRST
        // or the database refuses the delete outright. The projector runs after the save and would
        // never get the chance.
        var instances = await tenantOperationRepository.GetAll()
            .Where(o => o.OperationId == request.Id).ToListAsync(ct);
        foreach (var instance in instances)
        {
            var grants = await tenantRolePermissionRepository.GetAll()
                .Where(p => p.TenantOperationId == instance.Id).ToListAsync(ct);
            foreach (var grant in grants)
                tenantRolePermissionRepository.Delete(grant);
            tenantOperationRepository.Delete(instance);
        }

        repository.Delete(operation);
        await unitOfWork.SaveChangesAsync(ct);
        await projector.SyncAsync(ct);

        logger.LogInformation("Operation deleted successfully with ID: {Id} ({Count} tenant copy/copies removed)",
            operation.Id, instances.Count);

        return new OperationResult
        {
            Id = operation.Id,
            Name = operation.Name,
            Link = operation.Link,
            Icon = operation.Icon
        };
    }
}
