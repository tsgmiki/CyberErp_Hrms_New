using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Operations.Update;

public class UpdateOperationHandler(
    IRepository<Operation> repository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateOperationRequest> validator,
    ILogger<UpdateOperationHandler> logger)
    : IFeatureHandler<UpdateOperationRequest, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateOperationRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new AppValidationException(validationResult.Errors);

        var operation = await repository.GetAll()
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync();
        if (operation == null)
            throw new NotFoundException(nameof(Operation), request.Id.ToString());

        operation.Update(request.ModuleId, request.Name, request.Link, request.Filter, request.Icon, request.SortOrder);

        repository.UpdateAsync(operation);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Operation updated with Id: {Id}", operation.Id);

        return new OperationResult
        {
            Id = operation.Id,
            Name = operation.Name,
            Link = operation.Link,
            Icon = operation.Icon
        };
    }
}