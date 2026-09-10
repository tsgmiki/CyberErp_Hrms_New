using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.Inf.Common
{
    /// <inheritdoc cref="IPasswordPolicyService"/>
    public class PasswordPolicyService(IRepository<Setting> settingRepository) : IPasswordPolicyService
    {
        public async Task EnsureMeetsPolicyAsync(string password, string field = "password")
        {
            var policy = await LoadAsync();
            var failures = new List<string>();

            if (string.IsNullOrEmpty(password) || password.Length < policy.MinimumPasswordLength)
                failures.Add($"be at least {policy.MinimumPasswordLength} characters");
            if (policy.RequireUppercase && !password.Any(char.IsUpper))
                failures.Add("contain an upper-case letter");
            if (policy.RequireNumbers && !password.Any(char.IsDigit))
                failures.Add("contain a number");
            if (policy.RequireSpecialCharacters && password.All(char.IsLetterOrDigit))
                failures.Add("contain a symbol");

            if (failures.Count == 0) return;

            // Every failing rule in one sentence. Reporting them one at a time turns setting a
            // password into a guessing game played against the server.
            throw new ValidationException(field, $"The password must {Join(failures)}.");
        }

        public async Task<int?> DaysUntilExpiryAsync(DateTime passwordSetOn)
        {
            var policy = await LoadAsync();
            if (policy.PasswordExpiryDays <= 0) return null;   // 0 = never expires
            return (passwordSetOn.Date.AddDays(policy.PasswordExpiryDays) - DateTime.UtcNow.Date).Days;
        }

        /// <summary>
        /// The stored policy, or the built-in default when the deployment has no row yet.
        ///
        /// <para>Falling back to <c>CreateDefault()</c> rather than to "no rules" matters: a missing
        /// settings row must not silently mean an unrestricted password.</para>
        /// </summary>
        private async Task<Setting> LoadAsync() =>
            await settingRepository.GetAll().AsNoTracking().FirstOrDefaultAsync()
            ?? Setting.CreateDefault();

        private static string Join(List<string> parts) =>
            parts.Count == 1
                ? parts[0]
                : string.Join(", ", parts.Take(parts.Count - 1)) + " and " + parts[^1];
    }
}
