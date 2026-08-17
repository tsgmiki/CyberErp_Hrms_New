using CyberErp.Hrms.App.Features.Core.UserPreferences;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// The signed-in user's display preferences, so HRMS applies the same theme and language the
    /// user chose in the Home portal (both read one shared Core.UserPreference row).
    /// </summary>
    /// <remarks>
    /// ⚠️ Deliberately NOT permission-gated: this is self-service, like Employee/my-profile. The
    /// handler resolves the account from the auth cookie and can only ever return the caller's own
    /// row, so there is nothing here a permission could usefully protect — and gating it would mean
    /// a user without some menu grant silently loses their own theme.
    ///
    /// READ-ONLY. Preferences are edited in the Home portal's Edit Profile dialog; a second editor
    /// for one shared row is how validation drifts.
    /// </remarks>
    public class UserPreferenceController(IGetMyPreferences getMyPreferences) : BaseController
    {
        /// <summary>The caller's preferences, or the system defaults when they have never set any.</summary>
        [HttpGet("mine")]
        public Task<MyPreferencesDto> Mine() => getMyPreferences.GetAsync();
    }
}
