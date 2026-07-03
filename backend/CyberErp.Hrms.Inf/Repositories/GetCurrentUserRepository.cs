namespace CyberErp.Hrms.Inf.Repositories
{
    using CyberErp.Hrms.App.Features.Core.Users.GetCurrentUser;
    using CyberErp.Hrms.App.Features.Core.Users.DTOs;
    using CyberErp.Hrms.App.Common.Services;
    using Microsoft.AspNetCore.Http;

    public class GetCurrentUserRepository : IGetCurrentUserRepository
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetCurrentUserRepository(
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<CurrentUserResult> GetAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.GetCurrentUserId()?.ToString();
            var name = _currentUserService.GetCurrentUserName();

            // Get additional claims from HttpContext.User (which comes from the cookie token)
            var context = _httpContextAccessor.HttpContext;
            var email = context?.User?.FindFirst("email")?.Value
                       ?? context?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                       ?? name;
            var tenantId = context?.User?.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Task.FromResult(new CurrentUserResult
                {
                    IsAuthenticated = false
                });
            }

            return Task.FromResult(new CurrentUserResult
            {
                UserId = userId,
                Email = email ?? string.Empty,
                Name = name ?? string.Empty,
                TenantId = tenantId,
                IsAuthenticated = true
            });
        }
    }
}
