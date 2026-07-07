using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Users.DTOs;
using CyberErp.Hrms.App.Features.Core.Users.Login;
using CyberErp.Hrms.Dom.Entities.Core;
using CyberErp.Hrms.Inf.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Repositories.Core;

public class LoginRepository(
    IRepository<User> userRepository,
    IAuthentication authentication,
    ITokenStore tokenStore,
    ITokenParser tokenParser,
    IHttpContextAccessor httpContextAccessor,
    ILogger<LoginRepository> logger,
    IExceptionHandler exceptionHandler) : ILoginRepository
{
    private readonly IRepository<User> _userRepository = userRepository;
    private readonly IAuthentication _authentication = authentication;
    private readonly ILogger<LoginRepository> _logger = logger;
    private readonly ITokenStore _tokenStore = tokenStore;
    private readonly ITokenParser _tokenParser = tokenParser;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IExceptionHandler _exceptionHandler = exceptionHandler;

    public Task<UserResult> Loginsync(LoginUserDto dto) =>
        RepositoryExecutor.ExecuteAsync(
            _exceptionHandler,
            _logger,
            new ExceptionHandlingContext
            { OperationName = "Login", EntityType = nameof(User) },
            "Get",
            async () =>
            {
                _logger.LogInformation("Login with UserName: {UserName }", dto.UserName);

                var userList = await _userRepository.GetAll()
                    .AsNoTracking()
                    .Where(mu => mu.UserName == dto.UserName)
                    .ToListAsync();

                var user = userList.FirstOrDefault(a => _authentication.VerifyPassword(dto.Password, a.Password));

                if (user is null)
                {
                    _logger.LogWarning("Invalid credentials for UserName: {UserName }", dto.UserName);
                    throw new UnauthorizedException("Invalid username or password");
                }

                var tokenId = Guid.NewGuid();
                var userResult = new UserResult
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName,
                    TenantId = !string.IsNullOrEmpty(user.TenantId) ? Guid.Parse(user.TenantId) : null,
                    BranchId = user.BranchId,
                    IsHeadOffice = user.IsHeadOffice
                };

                var token = _authentication.GenerateToken(userResult, tokenId);
                var jwtToken = _tokenParser.ParseToken(token);
                userResult.Token = token;
                await _tokenStore.StoreAsync(tokenId.ToString(), jwtToken.ValidTo);

                if (!string.IsNullOrEmpty(user.TenantId))
                    SetTenantCookie(user.TenantId);

                SetUserCookies(user.Id.ToString(), user.UserName);
                SetBranchCookies(user.BranchId, user.IsHeadOffice);

                return userResult;
            });

    private void SetTenantCookie(string tenantId)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && !string.IsNullOrEmpty(tenantId))
        {
            context.Response.Cookies.Append("TenantId", tenantId, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddDays(14)
            });
        }
    }

    private void SetUserCookies(string userId, string userName)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null || string.IsNullOrEmpty(userId))
            return;

        context.Response.Cookies.Append("UserId", userId, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(14)
        });

        if (!string.IsNullOrEmpty(userName))
        {
            context.Response.Cookies.Append("UserName", userName, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddDays(14)
            });
        }
    }

    private void SetBranchCookies(Guid? branchId, bool isHeadOffice)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var options = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(14)
        };

        // BranchId scopes a branch admin; absent for Head Office.
        if (branchId.HasValue)
            context.Response.Cookies.Append("BranchId", branchId.Value.ToString(), options);
        else
            context.Response.Cookies.Delete("BranchId");

        context.Response.Cookies.Append("IsHeadOffice", isHeadOffice ? "true" : "false", options);
    }
}
