namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// Enforces the password policy that <c>Setting</c> already stores.
    ///
    /// <para>⚠️ THE POLICY EXISTED AND WAS READ BY NOTHING. <c>MinimumPasswordLength</c>,
    /// <c>RequireUppercase</c>, <c>RequireNumbers</c>, <c>RequireSpecialCharacters</c> and
    /// <c>PasswordExpiryDays</c> were configurable on the Settings screen and enforced nowhere — the
    /// only live rule was a hardcoded six-character minimum in the registration validator. A settings
    /// screen that changes no behaviour is worse than none: it reports a control that is not there
    /// (logic §12.90).</para>
    /// </summary>
    public interface IPasswordPolicyService
    {
        /// <summary>
        /// Throws a <c>ValidationException</c> naming every rule the password fails, in one message
        /// rather than one rule at a time.
        /// </summary>
        Task EnsureMeetsPolicyAsync(string password, string field = "password");

        /// <summary>
        /// Days until this password expires under the configured policy, or null when passwords never
        /// expire. Negative means it already has.
        /// </summary>
        Task<int?> DaysUntilExpiryAsync(DateTime passwordSetOn);
    }
}
