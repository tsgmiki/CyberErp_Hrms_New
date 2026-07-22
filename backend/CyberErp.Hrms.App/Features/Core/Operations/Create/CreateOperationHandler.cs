using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Operations.Create;

public class CreateOperationHandler(
    IRepository<Operation> repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateOperationRequest> validator,
    ILogger<CreateOperationHandler> logger)
    : IFeatureHandler<CreateOperationRequest, OperationResult>
{
    public async Task<OperationResult> Handle(CreateOperationRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new AppValidationException(validationResult.Errors);

        var operation = Operation.Create(request.ModuleId, request.Name, request.Link, request.Filter, request.Icon, request.SortOrder);

        await repository.AddAsync(operation);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Operation created with Id: {Id}", operation.Id);

        return new OperationResult
        {
            Id = operation.Id,
            Name = operation.Name,
            Link = operation.Link,
            Icon = operation.Icon
        };
    }
}