namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// The roles that carry ORGANISATION-WIDE HR authority — the people who may act for any unit,
    /// not just their own.
    ///
    /// <para>⚠️ This exists because a SCREEN GRANT cannot express it. "HR admin" was previously
    /// inferred from holding the employee register (<see cref="HrScreens.EmployeeRegister"/>) on the
    /// assumption that only Administrator and HR Admin would ever hold that screen. Clients do not
    /// grant permissions that way: in CERP the Department Manager role holds 141 of the 142 screens
    /// HR Admin holds, employee register included, which silently made every department head an
    /// organisation-wide administrator (logic §12.47).</para>
    ///
    /// <para>Matching is on <c>TenantRole.Code</c>, not <c>Name</c>: the code is a seeded slug
    /// mirrored from the global <c>Core.Role</c> catalogue, so renaming a role in the UI — which
    /// tenants do — cannot quietly revoke or grant organisation-wide access.</para>
    /// </summary>
    public static class HrRoles
    {
        /// <summary>
        /// Role codes whose holders act for the WHOLE organisation. A department head is deliberately
        /// absent: their reach is their own unit subtree, resolved from the org structure.
        /// </summary>
        public static readonly string[] OrganizationWide =
        [
            "ADMINISTRATOR",
            "HR-ADMIN",
            "HR-OFFICER",
        ];
    }
}
