using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace CyberErp.Hrms.Api.Configuration
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddHrmsAuthentication(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("Jwt").Get<JwtConfiguration>() ?? new JwtConfiguration();
           
            services.AddAuthentication(options =>
            {
                // Default scheme for authentication - supports both JWT Bearer for API and Cookies for browser
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie("Cookies", options =>
            {
                options.Cookie.Name = ".CyberErp.Hrms.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        // For API requests, return 401 instead of redirecting
                        if (context.Request.Path.StartsWithSegments("/api") && 
                            context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        // For API requests, return 403 instead of redirecting
                        if (context.Request.Path.StartsWithSegments("/api") && 
                            context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    }
                };
            })
      
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig.Key))
                };
            });

            return services;
        }

        public static IApplicationBuilder UseHrmsAuthentication(
            this IApplicationBuilder app)
        {
            app.UseAuthentication();

            // Custom middleware to extract tenant from JWT claims or cookies after authentication
            app.Use(async (context, next) =>
            {
                string? tenantId = null;
                
                // First, try to get tenant ID from cookies
                if (context.Request.Cookies.TryGetValue("TenantId", out var cookieTenantId))
                {
                    tenantId = cookieTenantId;
                }
                
                // If not in cookies, try to get from JWT claims (from authenticated user)
                if (string.IsNullOrEmpty(tenantId) && context.User?.Identity?.IsAuthenticated == true)
                {
                    var tenantIdClaim = context.User.FindFirst("TenantId");
                    if (tenantIdClaim != null)
                    {
                        tenantId = tenantIdClaim.Value;
                    }
                    
                    // Fallback: derive tenant from email for external logins (Google, etc.)
                    if (string.IsNullOrEmpty(tenantId))
                    {
                        var email = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") 
                            ?? context.User.FindFirst(ClaimTypes.Email)
                            ?? context.User.FindFirst("email");
                        if (email != null && !string.IsNullOrEmpty(email.Value))
                        {
                            tenantId = email.Value.ToLower();
                        }
                    }
                }
                
                // Store tenant ID in HttpContext.Items for later use
                if (!string.IsNullOrEmpty(tenantId))
                {
                    context.Items["TenantId"] = tenantId;
                }

                await next();
            });

            return app;
        }
        
        /// <summary>
        /// Sets the tenant ID in a cookie for subsequent requests.
        /// </summary>
        public static void SetTenantCookie(this HttpContext context, string tenantId)
        {
            if (!string.IsNullOrEmpty(tenantId))
            {
                context.Response.Cookies.Append("TenantId", tenantId, new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Secure = context.Request.IsHttps,
                    Expires = DateTimeOffset.UtcNow.AddDays(14)
                });
            }
        }
        
        /// <summary>
        /// Clears the tenant ID cookie.
        /// </summary>
        public static void ClearTenantCookie(this HttpContext context)
        {
            context.Response.Cookies.Delete("TenantId");
        }
    }
}
