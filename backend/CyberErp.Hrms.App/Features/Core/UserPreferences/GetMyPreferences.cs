using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.UserPreferences
{
    /*
     * The signed-in user's display preferences (Core.UserPreference).
     *
     * ⚠️ READ-ONLY HERE ON PURPOSE. Preferences are EDITED in one place — the Home portal's Edit
     * Profile dialog — and the row is shared, so HRMS only needs to read it to apply the same theme
     * and language. Adding a write endpoint here would create a second editor for one row and two
     * places for validation to drift.
     *
     * Self-service: scoped to ICurrentUserService.GetCurrentUserId(), never a caller-supplied id.
     * No row is the NORMAL state for anyone who has not opened the dialog, so this returns the
     * SYSTEM DEFAULTS rather than 404 — the SPA can apply the answer unconditionally.
     */

    public class MyPreferencesDto
    {
        public string Language { get; set; } = "en";
        public string TimeZone { get; set; } = "Africa/Nairobi";
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public string NumberFormat { get; set; } = "1,234.56";
        public string LandingPage { get; set; } = "/";
        public string Theme { get; set; } = "system";
        public bool EmailNotifications { get; set; } = true;
        public bool InAppNotifications { get; set; } = true;
        public bool ApprovalNotifications { get; set; } = true;
        /// <summary>
        /// False when no row exists and these are the system defaults. Lets the SPA distinguish
        /// "this user chose the defaults" from "this user has never chosen", which matters for
        /// logging and for deciding whether to write on first save.
        /// </summary>
        public bool IsUserDefined { get; set; }
    }

    public interface IGetMyPreferences { Task<MyPreferencesDto> GetAsync(); }

    public class GetMyPreferences(
        IRepository<UserPreference> preferences,
        ICurrentUserService currentUser) : IGetMyPreferences
    {
        public async Task<MyPreferencesDto> GetAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null || userId == Guid.Empty) return new MyPreferencesDto();

            // The generic repository already scopes to the ambient tenant, which is what makes this
            // per-user-per-tenant: the unique key on the table is (UserId, TenantId).
            var row = await preferences.GetAll().AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => new MyPreferencesDto
                {
                    Language = p.Language,
                    TimeZone = p.TimeZone!,
                    DateFormat = p.DateFormat!,
                    NumberFormat = p.NumberFormat!,
                    LandingPage = p.LandingPage!,
                    Theme = p.Theme!,
                    EmailNotifications = p.EmailNotifications,
                    InAppNotifications = p.InAppNotifications,
                    ApprovalNotifications = p.ApprovalNotifications,
                    IsUserDefined = true,
                })
                .FirstOrDefaultAsync();

            // Defaults are the documented fallback, not an error path.
            return row ?? new MyPreferencesDto();
        }
    }
}
