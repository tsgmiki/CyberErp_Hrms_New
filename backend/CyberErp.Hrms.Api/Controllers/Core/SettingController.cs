using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Features.Core.Settings;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Deployment operations settings (Core.Setting): the SMTP relay and the backup schedule.
    ///
    /// <para>Gated on <c>setting</c>. There is no menu operation with that link yet, so the endpoints
    /// are administrator-only in the strictest sense — nobody can reach them until the link is
    /// granted, which is the safe default for a screen that redirects the organisation's mail.</para>
    ///
    /// <para>⚠️ The SMTP PASSWORD is not part of this API in either direction. It lives in
    /// configuration (user-secrets locally, environment variables elsewhere); the DTO reports only
    /// whether one exists.</para>
    /// </summary>
    [RequirePermission("setting")]
    public class SettingController(
        IGetSetting getHandler,
        ISaveSetting saveHandler,
        ISendTestEmail testEmailHandler) : BaseController
    {
        [HttpGet]
        public Task<SettingDto> Get() => getHandler.GetAsync();

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveSettingDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Settings saved" });
        }

        /// <summary>
        /// Queues one message to prove the relay works. Reports which host and user were actually
        /// resolved, which is the whole point — the stored settings and the configured fallback are
        /// easy to confuse.
        /// </summary>
        [HttpPost("test-email")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailDto dto) =>
            Ok(await testEmailHandler.SendAsync(dto));
    }
}
