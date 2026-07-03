using CyberErp.Hrms.App.Common.Services;
using Microsoft.AspNetCore.Http;

namespace CyberErp.Hrms.Inf.Common
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly string[] UserIdCookieNames = { "UserId", "userId", "currentUserId", "CURRENTUSERID" };
        private static readonly string[] UserNameCookieNames = { "UserName", "userName", "currentUserName", "FullName", "fullName", "CURRENTUSERNAME" };

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCurrentUserId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            foreach (var cookieName in UserIdCookieNames)
            {
                var cookieValue = context.Request.Cookies[cookieName];
                if (!string.IsNullOrEmpty(cookieValue) && Guid.TryParse(cookieValue, out var userId))
                {
                    return userId;
                }
            }

            return null;
        }

        public string? GetCurrentUserName()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            foreach (var cookieName in UserNameCookieNames)
            {
                var cookieValue = context.Request.Cookies[cookieName];
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    return cookieValue;
                }
            }

            return null;
        }
    }
}

