using CyberErp.Hrms.App.Common.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CyberErp.Hrms.App.Features.Core.Users.Logout
{
    public class LogoutCookieHandler(
        ILogger<LogoutCookieHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        ITokenStore tokenStore) : ILogoutCookieUser
    {
        private readonly ILogger<LogoutCookieHandler> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ITokenStore _tokenStore = tokenStore;

        public async Task<bool> LogoutAsync()
        {
            _logger.LogInformation("Cookie logout initiated");
            
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                {
                    _logger.LogWarning("No HttpContext available for cookie logout");
                    return false;
                }

                // Get token ID from claims if available
                var tokenId = context.User?.FindFirst("jti")?.Value;

                // Revoke token if we have one
                if (!string.IsNullOrEmpty(tokenId))
                {
                    await _tokenStore.RevokeAsync(tokenId);
                    _logger.LogInformation("Token {TokenId} revoked during cookie logout", tokenId);
                }

                // Sign out of cookie authentication
                await context.SignOutAsync("Cookies");
                
                // Clear specific cookies
                context.Response.Cookies.Delete("TenantId");
                context.Response.Cookies.Delete("UserId");
                context.Response.Cookies.Delete("UserName");
                context.Response.Cookies.Delete(".CyberErp.Hrms.Auth");
                context.Response.Cookies.Delete(".CyberErp.Hrms.Session");
                
                _logger.LogInformation("Cookie logout completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cookie logout");
                return false;
            }
        }
    }
}
