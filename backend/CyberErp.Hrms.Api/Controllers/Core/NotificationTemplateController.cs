using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Administrator-defined e-mail templates and recipient routing.
    ///
    /// <para>Gated on <c>notificationTemplate</c> — the screen that maintains them. ⚠️ That operation
    /// must exist in Core.TenantOperation before anyone can reach this: gating on a link nobody holds
    /// denies everyone.</para>
    /// </summary>
    [RequirePermission("notificationTemplate")]
    public class NotificationTemplateController(
        IGetNotificationEvents eventsHandler,
        IGetAllNotificationTemplates getAllHandler,
        IGetNotificationTemplateById getByIdHandler,
        ISaveNotificationTemplate saveHandler,
        IDeleteNotificationTemplate deleteHandler,
        ISeedNotificationEvents seedHandler) : BaseController
    {
        /// <summary>The event catalogue an admin picks from, each with the merge tokens it publishes.</summary>
        [HttpGet("events")]
        public Task<List<NotificationEventDto>> Events() => eventsHandler.GetAsync();

        [HttpGet]
        public Task<PaginatedResponse<NotificationTemplateDto>> GetAll([FromQuery] GetAllRequest request) =>
            getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<NotificationTemplateDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationTemplateDto dto) =>
            Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] NotificationTemplateDto dto) =>
            Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        /// <summary>Loads the code's event catalogue into this tenant. Idempotent; deletes nothing.</summary>
        [HttpPost("seed-defaults")]
        public async Task<IActionResult> SeedDefaults() =>
            Ok(new { seeded = await seedHandler.SeedAsync() });
    }
}
