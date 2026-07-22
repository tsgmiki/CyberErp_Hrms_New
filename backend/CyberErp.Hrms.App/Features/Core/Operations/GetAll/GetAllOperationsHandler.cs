using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Operations.GetAll;

public class GetAllOperationsHandler(IGetAllOperationsRepository repository)
    : IFeatureHandler<GetAllOperationsRequest, PaginatedResponse<OperationDto>>
{
    public async Task<PaginatedResponse<OperationDto>> Handle(GetAllOperationsRequest request, CancellationToken ct = default)
    {
        return await repository.GetAllAsync(request, ct);
    }
}
