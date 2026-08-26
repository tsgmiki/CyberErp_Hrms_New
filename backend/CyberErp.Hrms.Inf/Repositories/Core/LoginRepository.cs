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
    IRepository<TenantUser> tenantUserRepository,
    IRepository<Employee> employeeRepository,
    IRepository<LoginTrail> loginTrailRepository,
    IAuthentication authentication,
    ITokenStore tokenStore,
    ITokenParser tokenParser,
    IHttpContextAccessor httpContextAccessor,
    ILogger<LoginRepository> logger,
    IExceptionHandler exceptionHandler) : ILoginRepository
{
    private readonly IRepository<LoginTrail> _loginTrailRepository = loginTrailRepository;
    private readonly IRepository<User> _userRepository = userRepository;
    private readonly IRepository<TenantUser> _tenantUserRepository = tenantUserRepository;
    private readonly IRepository<Employee> _employeeRepository = employeeRepository;
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

                var user = userList.FirstOrDefault(a => _authentication.VerifyPassword(dto.Password, a.PasswordHash));

                if (user is null)
                {
                    _logger.LogWarning("Invalid credentials for UserName: {UserName }", dto.UserName);
                    // A FAILED attempt is the one most worth recording: it is how a lockout, a shared
                    // account or a credential-stuffing run becomes visible. Written before the throw,
                    // and never allowed to change the outcome — see RecordLoginEventAsync.
                    await RecordLoginEventAsync(LoginTrail.Failure(
                        dto.UserName ?? string.Empty, ClientIp(), UserAgent(),
                        userList.Count == 0 ? "Unknown user name" : "Incorrect password"));
                    throw new UnauthorizedException("Invalid username or password");
                }

                // ⚠️ Checked AFTER the password, so a wrong password and a disabled account are not
                // distinguishable to someone guessing. Until now nothing read AccountStatus at all:
                // an account could be deactivated anywhere in the product and still sign in, which
                // made "deactivate" a label rather than a control (logic §12.60).
                if (!user.AccountStatus)
                {
                    _logger.LogWarning("Sign-in refused for deactivated account {UserName}", dto.UserName);
                    await RecordLoginEventAsync(LoginTrail.Failure(
                        dto.UserName ?? string.Empty, ClientIp(), UserAgent(), "Account deactivated"));
                    throw new UnauthorizedException("This account has been deactivated. Contact your administrator.");
                }

                // Branch scope + head-office visibility are DERIVED from the linked employee's branch:
                // a user tied to a REGULAR branch is scoped to that branch, while a user assigned to the
                // branch flagged Head Office — or one with no employee at all (tenant owner / unlinked
                // system account) — keeps global visibility across every branch and department.
                //
                // Read WITHOUT the repository's tenant/branch filters and re-assert the tenant by hand:
                // those filters read the *request* cookies, which at login still describe the PREVIOUS
                // session (logout does not clear BranchId/IsHeadOffice). A stale BranchId made this
                // lookup return no row, which silently collapsed to "no branch" — the wrong answer for
                // both scoping and head-office status.
                // ⚠️ THE TENANT COMES FROM MEMBERSHIP NOW, not from the user row.
                //
                // Core.User lost its TenantId on 2026-08-13. This is the row that decides which tenant
                // the session belongs to — it sets the cookie every later request resolves against —
                // so with the column gone and nothing put in its place, every query in the app
                // silently returned nothing: empty sidebar, zero employees.
                //
                // A user can now hold several memberships, so the default one wins, then any active
                // one. Read WITHOUT the tenant filter: there is no tenant context yet, by definition.
                var membership = await _tenantUserRepository.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(tu => tu.UserId == user.Id && tu.Status)
                    .OrderByDescending(tu => tu.IsDefaultTenant)
                    .Select(tu => new { tu.TenantId })
                    .FirstOrDefaultAsync();

                if (membership is null)
                {
                    // Authentic credentials but no tenant: the account exists and cannot be used.
                    // Saying so beats dropping them into an empty application.
                    _logger.LogWarning("User {UserName} authenticated but belongs to no tenant", user.UserName);
                    await RecordLoginEventAsync(LoginTrail.Failure(
                        dto.UserName ?? string.Empty, ClientIp(), UserAgent(), "No tenant membership"));
                    throw new UnauthorizedException("This account is not assigned to any organization.");
                }

                // TenantId IS the owning tenant now — the separate OwningTenantId column was dropped
                // on 2026-08-14 once the re-key made the two hold the same Guid. It arrives here as a
                // string because BaseEntity types it that way; the column itself is a uniqueidentifier.
                var tenantId = membership.TenantId;

                Guid? branchId = null;
                var isBranchHeadOffice = false;
                if (user.EmployeeId.HasValue)
                {
                    var assignment = await _employeeRepository.GetAllWithoutTenantFilter().AsNoTracking()
                        .Where(e => e.Id == user.EmployeeId.Value && e.TenantId == tenantId)
                        .Select(e => new { e.BranchId, IsHeadOfficeBranch = e.Branch != null && e.Branch.IsHeadOffice })
                        .FirstOrDefaultAsync();
                    branchId = assignment?.BranchId;
                    isBranchHeadOffice = assignment?.IsHeadOfficeBranch ?? false;
                }

                // "No branch" only means head office for an account that is NOT tied to an employee —
                // the tenant owner / system login the comment above describes. It previously applied to
                // ANY branchless account, so every employee-linked user whose employee had no branch
                // logged in as head office; head office short-circuits IsAdminAsync, which turns off the
                // manager/self scoping in the appraisal, employee and goal queries and shows everyone
                // the whole organisation (salaries included). An employee is scoped by their branch, or
                // by the visibility service when there are no branches — never by this flag.
                var isHeadOffice = (branchId is null && !user.EmployeeId.HasValue) || isBranchHeadOffice;

                var tokenId = Guid.NewGuid();
                var userResult = new UserResult
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName,
                    TenantId = Guid.Parse(tenantId),
                    BranchId = branchId,
                    IsHeadOffice = isHeadOffice
                };

                var token = _authentication.GenerateToken(userResult, tokenId);
                var jwtToken = _tokenParser.ParseToken(token);
                userResult.Token = token;
                await _tokenStore.StoreAsync(tokenId.ToString(), jwtToken.ValidTo);

                SetTenantCookie(tenantId);

                SetUserCookies(user.Id.ToString(), user.UserName);
                SetBranchCookies(branchId, isHeadOffice);

                await RecordLoginEventAsync(LoginTrail.Success(
                    user.Id, user.UserName ?? string.Empty, ClientIp(), UserAgent()), tenantId);

                return userResult;
            });

    /// <summary>
    /// Appends one authentication event.
    ///
    /// <para>Swallows every failure by design: an audit row must never be the reason a sign-in fails,
    /// and on the failure path it runs immediately before an <c>UnauthorizedException</c> that has to
    /// reach the caller unchanged. The tenant is passed explicitly on success because the ambient
    /// tenant context is not established until the cookies above are read on the NEXT request.</para>
    /// </summary>
    private async Task RecordLoginEventAsync(LoginTrail entry, string? tenantId = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(tenantId)) entry.TenantId = tenantId;
            await _loginTrailRepository.AddAsync(entry);
            await _loginTrailRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not record the login trail for {UserName}", entry.UserNameAttempted);
        }
    }

    /// <summary>Caller IP, preferring the proxy header so a reverse-proxied deployment logs the real client.</summary>
    private string? ClientIp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return null;
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private string? UserAgent() =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.FirstOrDefault();

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
