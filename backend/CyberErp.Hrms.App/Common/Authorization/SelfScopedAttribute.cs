namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// Marks an action that returns or changes ONLY the caller's own data — "/me", "/mine",
    /// "/my-approvals". Being signed in is the whole authorisation check, so the endpoint is exempt
    /// from its controller's <see cref="RequirePermissionAttribute"/>.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ This exists because gating a controller on the screen it serves is right for the
    /// register and wrong for the self-service endpoint sitting next to it. <c>EmployeeController</c>
    /// is gated on <c>employee</c> — the HR staff register — but it also answers
    /// <c>GET /Employee/me</c>, which every employee's portal calls on sign-in. Requiring the HR
    /// grant there denied ordinary staff their own record, which surfaced as blank fields and empty
    /// comboboxes rather than as an access error.</para>
    ///
    /// <para>Only use it where the HANDLER itself scopes the result to the current user. It is not a
    /// way to make an awkward endpoint reachable: an action that can return another person's data is
    /// not self-scoped, whatever its route is called.</para>
    ///
    /// <para>Endpoints whose controller is gated on the employee's OWN screen (Annual Leave, Other
    /// Leave, My Medical Claims …) do NOT need this — that grant is exactly the right check, and
    /// revoking it should indeed close the endpoint.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class SelfScopedAttribute : Attribute;
}
