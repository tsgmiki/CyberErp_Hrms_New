using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.App.Features.Core.Operations.GetAll;

namespace CyberErp.Hrms.App.Features.Core.Operations.GetAll
{
    public interface IGetAllOperationsRepository
    {
        Task<PaginatedResponse<OperationDto>> GetAllAsync(GetAllOperationsRequest request, CancellationToken ct = default);
    }
}
