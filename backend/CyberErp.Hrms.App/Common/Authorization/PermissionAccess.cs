namespace CyberErp.Hrms.App.Common.Authorization
{
    /// <summary>
    /// The kind of access an endpoint needs on a menu operation — one value per privilege column on
    /// <c>Core.TenantRolePermission</c>.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ Until 2026-08-19 the gate checked <c>CanView</c> and nothing else: every endpoint of a
    /// screen — POST, PUT, DELETE alike — was authorised by the grant that merely says "you may open
    /// this screen". Revoking Add/Edit/Delete/Approve therefore changed the buttons a SPA drew and
    /// nothing else; the request still succeeded. This enum is what makes the six columns mean
    /// something on the server.</para>
    /// </remarks>
    public enum PermissionAccess
    {
        /// <summary>Read. <c>CanView</c>.</summary>
        View,
        /// <summary>Create a record. <c>CanAdd</c>.</summary>
        Add,
        /// <summary>Change an existing record, including its state. <c>CanEdit</c>.</summary>
        Edit,
        /// <summary>Remove a record. <c>CanDelete</c>.</summary>
        Delete,
        /// <summary>Decide a record: approve, reject, endorse, sign off. <c>CanApprove</c>.</summary>
        Approve,
        /// <summary>Extract data — export, download, print. <c>CanExport</c>.</summary>
        Export,
    }
}
