using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Modules.GetAll
{
    public interface IGetAllModules
    {
        Task<PaginatedResponse<GetModuleDto>> GetAllAsync(GetAllRequest request);
    }
}

