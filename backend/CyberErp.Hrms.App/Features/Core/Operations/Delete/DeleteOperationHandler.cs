using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Operations.Delete;

public class DeleteOperationHandler(
    IRepository<Operation> repository,
    IUnitOfWork unitOfWork,
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

        repository.Delete(operation);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Operation deleted successfully with ID: {Id}", operation.Id);

        return new OperationResult
        {
            Id = operation.Id,
            Name = operation.Name,
            Link = operation.Link,
            Icon = operation.Icon
        };
    }
}