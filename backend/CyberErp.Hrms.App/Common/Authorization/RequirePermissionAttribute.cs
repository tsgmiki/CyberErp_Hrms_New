namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// Requires the caller's role(s) to hold the needed privilege on at least ONE of the named menu
    /// operations (matched by <see cref="Dom.Entities.Core.Operation.Link"/>, namespace/slash/case
    /// insensitive). Enforced by the global <c>PermissionAuthorizationFilter</c>.
    ///
    /// <para><b>Which privilege</b> is normally DERIVED from the endpoint — the HTTP verb plus the
    /// route suffix — so a controller states <i>what screen</i> it belongs to and the filter works out
    /// <i>what kind of access</i> each action needs. See
    /// <c>PermissionAuthorizationFilter.DeriveAccess</c> for the mapping. Set <see cref="Access"/>
    /// only where the derivation is wrong for that action.</para>
    ///
    /// <para>Enforcement is OPT-IN: an endpoint with no attribute is not permission-gated (it keeps
    /// whatever data-scoping its handler applies). Placing multiple links means "any of these" — use
    /// it for endpoints shared by more than one screen (e.g. a list that also feeds another screen's
    /// dropdown).</para>
    /// </summary>
    /// <remarks>
    /// ⚠️ Before 2026-08-19 this attribute could only express "you may view this screen", because the
    /// service behind it looked at <c>CanView</c> alone. Any endpoint carrying it was reachable by
    /// anyone who could open the screen, whatever it did.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class RequirePermissionAttribute(params string[] operationLinks) : Attribute
    {
        public IReadOnlyList<string> OperationLinks { get; } = operationLinks ?? [];

        /// <summary>
        /// The privilege this endpoint needs. Leave unset to derive it from the verb and route
        /// suffix; set it when the derivation would be wrong — a POST that decides rather than
        /// creates, a GET that extracts data, a "cancel my own request" that must not demand Edit.
        /// </summary>
        public PermissionAccess Access
        {
            get => AccessOrNull ?? PermissionAccess.View;
            set => AccessOrNull = value;
        }

        /// <summary>Null when the attribute did not set <see cref="Access"/> — i.e. derive it.</summary>
        public PermissionAccess? AccessOrNull { get; private set; }
    }
}
