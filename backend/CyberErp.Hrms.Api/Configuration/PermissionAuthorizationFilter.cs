using CyberErp.Hrms.App.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CyberErp.Hrms.Api.Configuration
{
    /// <summary>
    /// Global authorization filter enforcing per-operation permission (<c>RolePermission.CanView</c>)
    /// for controller actions annotated with <see cref="RequirePermissionAttribute"/> — the server-side
    /// counterpart of the sidebar/route permission model. OPT-IN: unannotated actions are untouched.
    /// Head Office bypasses (in the service). Unauthenticated requests are left to <c>[Authorize]</c> (401).
    /// </summary>
    public sealed class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Anonymous endpoints (login, etc.) are never permission-gated.
            if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
                return;

            // Let [Authorize] produce the 401 for unauthenticated callers; only gate authenticated ones.
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
                return;

            var required = ResolveRequiredLinks(context);
            if (required is null || required.Count == 0)
                return; // opt-in: no attribute → not permission-gated

            var service = context.HttpContext.RequestServices.GetRequiredService<IEndpointPermissionService>();
            if (!await service.HasAnyAsync(required))
            {
                context.Result = new ObjectResult(new { message = "You do not have permission to perform this action." })
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                };
            }
        }

        /// <summary>Action-level attribute wins over controller-level.</summary>
        private static IReadOnlyList<string>? ResolveRequiredLinks(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor cad) return null;

            var onAction = cad.MethodInfo
                .GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: true)
                .OfType<RequirePermissionAttribute>()
                .FirstOrDefault();
            if (onAction is not null) return onAction.OperationLinks;

            var onController = cad.ControllerTypeInfo
                .GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: true)
                .OfType<RequirePermissionAttribute>()
                .FirstOrDefault();
            return onController?.OperationLinks;
        }
    }
}
