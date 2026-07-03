using CyberErp.Hrms.App.Common.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CyberErp.Hrms.App.Features.Core.Users.Logout
{
    public class LogoutUser(
        ITokenStore tokenStore,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LogoutUser> logger) : ILogoutUser
    {
        private readonly ITokenStore _tokenStore = tokenStore;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<LogoutUser> _logger = logger;

        public async Task<bool> LogoutAsync(string tokenId)
        {
            _logger.LogInformation("Logging out user with token ID: {TokenId}", tokenId);
            
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                _logger.LogWarning("Logout attempted with empty token ID");
                return false;
            }

            try
            {
                await _tokenStore.RevokeAsync(tokenId);
                
                // Clear cookies
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    // Sign out of cookie authentication
                    await context.SignOutAsync("Cookies");
                    
                    // Clear specific cookies
                    context.Response.Cookies.Delete("TenantId");
                    context.Response.Cookies.Delete("UserId");
                    context.Response.Cookies.Delete("UserName");
                    context.Response.Cookies.Delete(".CyberErp.Hrms.Auth");
                    context.Response.Cookies.Delete(".CyberErp.Hrms.Session");
                }
                
                _logger.LogInformation("User successfully logged out with token ID: {TokenId}", tokenId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for token ID: {TokenId}", tokenId);
                return false;
            }
        }
    }
}
