using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Modules.GetAll;

public class GetAllModulesHandler(IGetAllModuleRepository repository)
    : IFeatureHandler<GetAllModulesRequest, PaginatedResponse<GetModuleDto>>
{
    public async Task<PaginatedResponse<GetModuleDto>> Handle(GetAllModulesRequest request, CancellationToken ct = default)
    {
        return await repository.GetAllAsync(request, ct);
    }
}
