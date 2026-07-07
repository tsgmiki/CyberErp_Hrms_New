using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.AuditLogs;
using CyberErp.Hrms.App.Features.Core.AuditLogs.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Read-only audit trail (paper trail for structural mutations).</summary>
    public class AuditLogController(IGetAllAuditLogs getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AuditLogDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);
    }
}
