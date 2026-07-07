using CyberErp.Hrms.App.Features.Core.Modules.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Modules.GetOperations;

public interface IGetModuleWithOperationsRepository
{
    Task<IEnumerable<GetModuleWithOperationResult>> GetAsync(Guid? userId, CancellationToken ct = default);
}