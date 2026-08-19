using CyberErp.Hrms.App.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CyberErp.Hrms.Api.Configuration
{
    /// <summary>
    /// Global authorization filter enforcing per-operation, per-PRIVILEGE permission for controller
    /// actions annotated with <see cref="RequirePermissionAttribute"/> — the server-side counterpart
    /// of the sidebar/route permission model. OPT-IN: unannotated actions are untouched.
    /// Unauthenticated requests are left to <c>[Authorize]</c> (401).
    /// </summary>
    public sealed class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        /// <summary>
        /// Route suffixes that DECIDE a record rather than change it — these need CanApprove.
        /// Matched on the last literal segment of the action's route.
        /// </summary>
        private static readonly HashSet<string> ApproveTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            "approve", "approved", "reject", "rejected", "decide", "decision", "endorse",
            "authorize", "authorise", "sign", "signoff", "acknowledge", "calibrate", "verify",
        };

        /// <summary>
        /// Route tokens that CREATE records even though the route has a suffix, so the plain
        /// "suffixed POST = Edit" rule would understate what they do.
        /// </summary>
        private static readonly HashSet<string> AddTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            "generate", "build", "create", "clone", "duplicate", "seed", "enroll", "enrol",
        };

        /// <summary>Route suffixes that EXTRACT data — these need CanExport, whatever the verb.</summary>
        private static readonly HashSet<string> ExportTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            "export", "download", "print", "pdf", "excel", "csv",
        };

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Anonymous endpoints (login, etc.) are never permission-gated.
            if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
                return;

            // Let [Authorize] produce the 401 for unauthenticated callers; only gate authenticated ones.
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
                return;

            var attribute = ResolveAttribute(context);
            if (attribute is null || attribute.OperationLinks.Count == 0)
                return; // opt-in: no attribute → not permission-gated

            var access = attribute.AccessOrNull ?? DeriveAccess(context);

            var service = context.HttpContext.RequestServices.GetRequiredService<IEndpointPermissionService>();
            if (!await service.HasAnyAsync(attribute.OperationLinks, access))
            {
                context.Result = new ObjectResult(new
                {
                    message = $"You do not have permission to {Describe(access)} here.",
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                };
            }
        }

        /// <summary>Action-level attribute wins over controller-level.</summary>
        private static RequirePermissionAttribute? ResolveAttribute(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor cad) return null;

            var onAction = cad.MethodInfo
                .GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: true)
                .OfType<RequirePermissionAttribute>()
                .FirstOrDefault();
            if (onAction is not null) return onAction;

            return cad.ControllerTypeInfo
                .GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: true)
                .OfType<RequirePermissionAttribute>()
                .FirstOrDefault();
        }

        /// <summary>
        /// Which privilege this endpoint needs, from the HTTP verb and the route suffix.
        ///
        /// <para>Derivation rather than annotation is what makes strict enforcement tractable: this
        /// API has ~795 endpoints across 52 controllers, and hand-labelling every one of them is both
        /// a large change and a standing invitation to forget one — a forgotten label on a POST is an
        /// ungated write. A controller declares WHICH SCREEN it serves; the verb already says what
        /// kind of access each action is.</para>
        ///
        /// <list type="bullet">
        ///   <item>GET/HEAD → View, unless the suffix extracts data → Export</item>
        ///   <item>DELETE → Delete</item>
        ///   <item>PUT/PATCH → Edit</item>
        ///   <item>POST with no suffix → Add (the create endpoint)</item>
        ///   <item>POST with a deciding suffix (approve/reject/…) → Approve</item>
        ///   <item>POST with any other suffix → Edit: it acts on a record that already exists
        ///         (submit, cancel, close, settle, mark-paid …), which is a change, not a create</item>
        /// </list>
        ///
        /// <para>Set <c>Access</c> on the attribute wherever this is wrong for a given action — the
        /// derivation is a sane default, not a claim to be right everywhere.</para>
        /// </summary>
        private static PermissionAccess DeriveAccess(AuthorizationFilterContext context)
        {
            var method = context.HttpContext.Request.Method;
            var suffix = RouteSuffix(context);

            // Tokenised, not whole-string: real routes are compound ("reviewer-signoff",
            // "create-development-plan", "mark-paid"), and an exact-match set silently mis-derives
            // every one of them — "reviewer-signoff" would have asked for Edit, not Approve.
            var tokens = suffix?.Split('-', StringSplitOptions.RemoveEmptyEntries) ?? [];

            if (tokens.Any(ExportTokens.Contains)) return PermissionAccess.Export;

            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method)) return PermissionAccess.View;
            if (HttpMethods.IsDelete(method)) return PermissionAccess.Delete;
            if (HttpMethods.IsPut(method) || HttpMethods.IsPatch(method)) return PermissionAccess.Edit;

            if (HttpMethods.IsPost(method))
            {
                if (suffix is null) return PermissionAccess.Add;
                if (tokens.Any(ApproveTokens.Contains)) return PermissionAccess.Approve;
                if (tokens.Any(AddTokens.Contains)) return PermissionAccess.Add;
                return PermissionAccess.Edit;
            }

            // Anything else (OPTIONS/TRACE) is a read as far as authorisation is concerned.
            return PermissionAccess.View;
        }

        /// <summary>
        /// The last LITERAL segment of the action's route, or null when the route ends at the
        /// controller (i.e. the plain collection endpoint).
        ///
        /// <para>Route parameters are skipped, so <c>{id:guid}/approve</c> yields "approve" and
        /// <c>{id:guid}</c> yields the controller segment — which reads as "no suffix", correctly:
        /// <c>GET /Employee/{id}</c> is still a read of the Employee screen.</para>
        /// </summary>
        private static string? RouteSuffix(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor cad) return null;
            var template = cad.AttributeRouteInfo?.Template;
            if (string.IsNullOrWhiteSpace(template)) return null;

            var literal = template
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(segment => !segment.Contains('{'))
                .LastOrDefault();

            if (literal is null) return null;

            // The controller's own segment is not a suffix. `[controller]` is already substituted in
            // the descriptor, so compare against the controller name.
            return literal.Equals(cad.ControllerName, StringComparison.OrdinalIgnoreCase)
                ? null
                : literal;
        }

        private static string Describe(PermissionAccess access) => access switch
        {
            PermissionAccess.Add => "create records",
            PermissionAccess.Edit => "change records",
            PermissionAccess.Delete => "delete records",
            PermissionAccess.Approve => "approve or reject records",
            PermissionAccess.Export => "export data",
            _ => "view this",
        };
    }
}
