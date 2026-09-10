namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// Carries a REASON for the change about to be saved, so the audit trail records not just what
    /// changed but why.
    ///
    /// <para>⚠️ Ambient rather than a parameter on every save, because the audit trail is written by
    /// an EF interceptor that has no access to handler arguments. A handler sets the reason, calls
    /// <c>SaveChangesAsync</c> as usual, and the interceptor stamps it onto every row that change
    /// produced (logic §12.90).</para>
    ///
    /// <para>⚠️ It CLEARS ITSELF once read. A reason belongs to one save; leaving it set would
    /// silently attach "corrected the completion date" to the next unrelated write in the same
    /// request, which is worse than recording no reason at all.</para>
    ///
    /// <para>Scoped to the request, so nothing leaks between users.</para>
    /// </summary>
    public interface IAuditReasonAccessor
    {
        /// <summary>Sets the reason for the next save. Null or blank clears it.</summary>
        void Set(string? reason);

        /// <summary>Reads and clears. Called by the interceptor, once per save.</summary>
        string? Consume();
    }
}
