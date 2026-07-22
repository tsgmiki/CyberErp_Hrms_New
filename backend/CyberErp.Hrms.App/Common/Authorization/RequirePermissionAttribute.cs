namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// Requires the caller's role(s) to have <c>CanView</c> on at least ONE of the named menu
    /// operations (matched by <see cref="Dom.Entities.Core.Operation.Link"/>, slash/case-insensitive).
    /// Head Office bypasses. Enforced by the global <c>PermissionAuthorizationFilter</c>.
    /// Enforcement is OPT-IN: an endpoint with no <see cref="RequirePermissionAttribute"/> is not
    /// permission-gated (it keeps whatever data-scoping its handler applies). Placing multiple links
    /// means "any of these" — use it for endpoints shared by more than one screen (e.g. a list that
    /// also feeds another screen's dropdown).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class RequirePermissionAttribute(params string[] operationLinks) : Attribute
    {
        public IReadOnlyList<string> OperationLinks { get; } = operationLinks ?? [];
    }
}
