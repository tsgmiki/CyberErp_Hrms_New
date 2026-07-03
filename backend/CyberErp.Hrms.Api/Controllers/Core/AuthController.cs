using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberErp.Hrms.App.Features.Core.Users.Login;
using CyberErp.Hrms.App.Features.Core.Users.Logout;
using CyberErp.Hrms.App.Features.Core.Users.Register;
using CyberErp.Hrms.App.Features.Core.Users.GetCurrentUser;
using CyberErp.Hrms.App.Features.Core.Users.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Inf.Common;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginUser _loginUser;
        private readonly ILogoutUser _logoutUser;
        private readonly ILogoutCookieUser _logoutCookieUser;
        private readonly IRegisterUser _registerUser;
        private readonly IRegisterWithGoogle _registerWithGoogle;
        private readonly IGetCurrentUser _getCurrentUser;
        private readonly ITokenParser _tokenParser;
        private readonly IAuthentication _authentication;

        public AuthController(
            ILoginUser loginUser,
            ILogoutUser logoutUser,
            ILogoutCookieUser logoutCookieUser,
            IRegisterUser registerUser,
            IRegisterWithGoogle registerWithGoogle,
            IGetCurrentUser getCurrentUser,
            ITokenParser tokenParser,
            IAuthentication authentication)
        {
            _loginUser = loginUser;
            _logoutUser = logoutUser;
            _logoutCookieUser = logoutCookieUser;
            _registerUser = registerUser;
            _registerWithGoogle = registerWithGoogle;
            _getCurrentUser = getCurrentUser;
            _tokenParser = tokenParser;
            _authentication = authentication;
        }

        /// <summary>
        /// Authenticate user with username/password (returns JWT token)
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<UserResult> Login([FromBody] LoginUserDto dto)
        {
            return await _loginUser.Loginsync(dto);
        }

        /// <summary>
        /// Authenticate user with username/password and create cookie-based session
        /// </summary>
        [HttpPost("login/cookie")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithCookie([FromBody] LoginUserDto dto)
        {
            var result = await _loginUser.Loginsync(dto);

            // Parse the JWT token to extract claims
            var jwtToken = _tokenParser.ParseToken(result.Token);
            
            // Create claims identity from JWT token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
                new Claim(ClaimTypes.Name, result.UserName),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim("FullName", result.FullName),
                new Claim("PhoneNo", result.PhoneNumber),
                new Claim("TenantId", result.TenantId?.ToString() ?? Guid.Empty.ToString()),
                new Claim("UserId", result.Id.ToString()),
                new Claim("UserName", result.UserName),
                new Claim("Email", result.Email),
                new Claim("FullName", result.FullName),
            };

            // Add all claims from JWT token
            foreach (var claim in jwtToken.Claims)
            {
                if (!claims.Any(c => c.Type == claim.Type))
                {
                    claims.Add(new Claim(claim.Type, claim.Value));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Ok(result);
        }

        /// <summary>
        /// Logout user and invalidate token
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<bool> Logout([FromBody] LogoutRequestDto dto)
        {
            return await _logoutUser.LogoutAsync(dto.TokenId);
        }

        /// <summary>
        /// Logout user and clear cookie session
        /// </summary>
        [HttpPost("logout/cookie")]
        public async Task<IActionResult> LogoutCookie()
        {
            await _logoutCookieUser.LogoutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Register new user with tenant
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<RegisterResult> Register([FromBody] RegisterUserDto dto)
        {
            return await _registerUser.RegisterAsync(dto);
        }

        /// <summary>
        /// Register new user with Google
        /// </summary>
        [HttpPost("register/google")]
        [AllowAnonymous]
        public async Task<RegisterResult> RegisterWithGoogle([FromBody] RegisterWithGoogleDto dto)
        {
            return await _registerWithGoogle.RegisterWithGoogleAsync(dto);
        }

        /// <summary>
        /// Check if user is authenticated (for React useAuth pattern)
        /// </summary>
        [HttpGet("loginStatus")]
        [AllowAnonymous]
        public async Task<IActionResult> Me()
        {
            var result = await _getCurrentUser.GetAsync();
            
            if (!result.IsAuthenticated)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                result.UserId,
                result.Email,
                result.Name,
                result.TenantId,
                result.IsAuthenticated
            });
        }

        /// <summary>
        /// Verify cookie-based session is valid
        /// </summary>
        [HttpGet("verify-cookie")]
        [Authorize(AuthenticationSchemes = "Cookies")]
        public IActionResult VerifyCookie()
        {
            return Ok(new
            {
                UserId = User.FindFirst("UserId")?.Value,
                UserName = User.FindFirst("UserName")?.Value,
                Email = User.FindFirst("Email")?.Value,
                FullName = User.FindFirst("FullName")?.Value,
                TenantId = User.FindFirst("TenantId")?.Value,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }
    }

    public class LogoutRequestDto
    {
        public string TokenId { get; set; } = string.Empty;
    }
}
