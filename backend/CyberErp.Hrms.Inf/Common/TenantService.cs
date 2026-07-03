using CyberErp.Hrms.Inf.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace CyberErp.Hrms.Inf.Common
{
    public interface ITenantService
    {
        AppTenantInfo? GetCurrentTenant();
        string? GetCurrentTenantId();
        bool IsMultiTenant();

        /// <summary>
        /// Validates that the tenant is valid for database operations.
        /// Throws TenantValidationException if tenant is null or subscription is expired.
        /// Skips validation during login operations (when SkipTenantValidation context is set).
        /// </summary>
        void ValidateTenantForDatabaseOperations();

        /// <summary>
        /// Checks if the current tenant's subscription is valid.
        /// </summary>
        bool IsTenantSubscriptionValid();

        /// <summary>
        /// Checks if tenant validation should be skipped (e.g., during login).
        /// </summary>
        bool ShouldSkipTenantValidation();
    }

    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMultiTenantContextAccessor<AppTenantInfo>? _multiTenantContextAccessor;

        public TenantService(
            IHttpContextAccessor httpContextAccessor,
            IMultiTenantContextAccessor<AppTenantInfo>? multiTenantContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _multiTenantContextAccessor = multiTenantContextAccessor;
        }

        public AppTenantInfo? GetCurrentTenant()
        {
            var context = _httpContextAccessor.HttpContext;

            // First check if this is a seed operation - construct tenant info from SeedTenantId
            if (context != null && context.Items.TryGetValue("SeedTenantId", out var seedTenantId) && seedTenantId is string seedId)
            {
                return new AppTenantInfo
                {
                    Id = seedId,
                    Identifier = seedId,
                    Name = "New Tenant",
                    IsActive = true
                };
            }

            // First try to get from Finbuckle's MultiTenantContext (populated by DatabaseTenantStore)
            if (_multiTenantContextAccessor?.MultiTenantContext?.TenantInfo != null)
            {
                return _multiTenantContextAccessor.MultiTenantContext.TenantInfo;
            }

            if (context == null) return null;

            // Try to get from HttpContext.Items["TenantInfo"] (set by middleware)
            return context.Items["TenantInfo"] as AppTenantInfo;
        }

        public string? GetCurrentTenantId()
        {
            // First check if this is a seed operation - use SeedTenantId if set
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Items.TryGetValue("SeedTenantId", out var seedTenantId) && seedTenantId is string seedId)
            {
                return seedId;
            }

            // First try to get from Finbuckle's MultiTenantContext
            var tenantInfo = GetCurrentTenant();
            if (tenantInfo != null)
            {
                return tenantInfo.Id;
            }

            if (context == null) return null;

            // Try to get from HttpContext Items directly (set by middleware)
            var tenantId = context.Items["TenantId"] as string;

            // If not found, try to get from cookies
            if (string.IsNullOrEmpty(tenantId))
            {
                if (context.Request.Cookies.TryGetValue("TenantId", out var cookieTenantId))
                {
                    tenantId = cookieTenantId;
                }
            }

            // If still not found, try to get from JWT claims
            if (string.IsNullOrEmpty(tenantId))
            {
                var user = context.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var tenantClaim = user.FindFirst("TenantId");
                    if (tenantClaim != null)
                    {
                        tenantId = tenantClaim.Value;
                    }

                    // Also try sub claim as fallback
                    if (string.IsNullOrEmpty(tenantId))
                    {
                        var subClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                        if (subClaim != null)
                        {
                            tenantId = subClaim.Value;
                        }
                    }
                }
            }

            return tenantId;
        }

        public bool IsMultiTenant()
        {
            return !string.IsNullOrEmpty(GetCurrentTenantId());
        }

        public bool IsTenantSubscriptionValid()
        {
            var tenant = GetCurrentTenant();
            if (tenant == null)
                return false;

            return tenant.IsSubscriptionValid();
        }

        public bool ShouldSkipTenantValidation()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return true; // No context, skip validation

            // Normalize path - remove any base path prefix and convert to lowercase
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Check if this is a login endpoint - skip validation for login
            if (path.Contains("/login") || path.Contains("/auth/login") || path.Contains("/signin-"))
            {
                return true;
            }

            // Check if explicitly marked to skip validation
            if (context.Items.TryGetValue("SkipTenantValidation", out var skipValue) && skipValue is bool skip && skip)
            {
                return true;
            }

            return false;
        }

        public void ValidateTenantForDatabaseOperations()
        {
            // Skip validation during login or when explicitly marked
            if (ShouldSkipTenantValidation())
            {
                return;
            }

            var tenantId = GetCurrentTenantId();

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new TenantValidationException("Tenant ID is required. Please ensure you are properly authenticated with a valid tenant.");
            }

            // Get tenant info from Finbuckle context (includes subscription data from database)
            var tenant = GetCurrentTenant();
            if (tenant != null)
            {
                if (!tenant.IsActive)
                {
                    throw new TenantValidationException("Tenant account is inactive. Please contact support.");
                }

                if (tenant.SubscriptionEndDate.HasValue && tenant.SubscriptionEndDate.Value < DateTime.UtcNow)
                {
                    throw new TenantValidationException($"Tenant subscription expired on {tenant.SubscriptionEndDate.Value:yyyy-MM-dd}. Please renew your subscription.");
                }
            }
        }
    }

    public static class TenantServiceExtensions
    {
        public static IServiceCollection AddTenantService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantService, TenantService>();
            return services;
        }
    }
}
