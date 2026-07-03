using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CyberErp.Hrms.Inf.MultiTenant
{
    /// <summary>
    /// A hybrid tenant strategy that resolves tenant from multiple sources:
    /// 1. X-Tenant-Id header
    /// 2. TenantId cookie
    /// 3. JWT claims (TenantId claim)
    /// </summary>
    public class HybridTenantStrategy : IMultiTenantStrategy
    {
        public int Priority => 0;

        public Task<string?> GetIdentifierAsync(object context)
        {
            if (context is not HttpContext httpContext)
            {
                return Task.FromResult<string?>(null);
            }

            // 1. Try to get from header
            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue)
                && !string.IsNullOrEmpty(headerValue))
            {
                return Task.FromResult<string?>(headerValue.ToString());
            }

            // 2. Try to get from cookie
            if (httpContext.Request.Cookies.TryGetValue("TenantId", out var cookieValue)
                && !string.IsNullOrEmpty(cookieValue))
            {
                return Task.FromResult<string?>(cookieValue);
            }

            // 3. Try to get from JWT claims
            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = user.FindFirst("TenantId");
                if (tenantClaim != null && !string.IsNullOrEmpty(tenantClaim.Value))
                {
                    return Task.FromResult<string?>(tenantClaim.Value);
                }
            }

            return Task.FromResult<string?>(null);
        }
    }
}
