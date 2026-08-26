using System.Security.Cryptography;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Notifications;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    /// <summary>What was provisioned, so the caller can surface it if it wants to.</summary>
    public sealed record ProvisionedAccount(Guid UserId, string UserName, string Email, bool Notified);

    public interface IEmployeeAccountProvisioner
    {
        /// <summary>
        /// Creates the login for a newly registered employee: user + tenant membership + the default
        /// role, then e-mails the credentials. NEVER throws — the employee record is already saved
        /// and must stand even if the account cannot be created.
        /// </summary>
        Task<ProvisionedAccount?> ProvisionAsync(Guid employeeId);
    }

    /// <summary>
    /// Automatic account creation on employee registration.
    ///
    /// <para>Idempotent by the employee link: if a user is already linked to this employee, nothing
    /// happens. Employee creation is the trigger, but nothing stops it being retried.</para>
    /// </summary>
    public class EmployeeAccountProvisioner(
        IRepository<Employee> employees,
        IRepository<User> users,
        IRepository<TenantUser> tenantUsers,
        IRepository<TenantUserRole> tenantUserRoles,
        IRepository<TenantRole> tenantRoles,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser,
        IPasswordHasher passwordHasher,
        INotificationDispatcher dispatcher,
        ILogger<EmployeeAccountProvisioner> logger) : IEmployeeAccountProvisioner
    {
        /// <summary>Domain for the generated address when the employee gave none.</summary>
        private const string DefaultEmailDomain = "cybersoft.com";

        /// <summary>The role every new account starts with, by CODE — see HrRoles on why not Name.</summary>
        private const string DefaultRoleCode = "USERROLE";

        public async Task<ProvisionedAccount?> ProvisionAsync(Guid employeeId)
        {
            try
            {
                var emp = await employees.GetAll().AsNoTracking()
                    .Where(e => e.Id == employeeId)
                    .Select(e => new
                    {
                        e.EmployeeNumber,
                        e.Email,
                        First = e.Person != null ? e.Person.FirstName : null,
                        Last = e.Person != null ? e.Person.GrandFatherName : null,
                        Phone = e.Person != null ? e.Person.PhoneNumber : null,
                    })
                    .FirstOrDefaultAsync();
                if (emp is null)
                {
                    logger.LogWarning("Account provisioning skipped: employee {Id} not found.", employeeId);
                    return null;
                }

                // Already has a login (a re-run, or HR made one by hand) — do nothing.
                if (await users.GetAllWithoutTenantFilter().AnyAsync(u => u.EmployeeId == employeeId))
                {
                    logger.LogInformation("Account provisioning skipped: employee {Id} already has a login.", employeeId);
                    return null;
                }

                var tenantId = currentTenant.GetCurrentTenantId();
                if (tenantId is null || tenantId == Guid.Empty)
                {
                    logger.LogError("Account provisioning skipped for employee {Id}: no current tenant.", employeeId);
                    return null;
                }

                var role = await tenantRoles.GetAll().FirstOrDefaultAsync(r => r.Code == DefaultRoleCode);
                if (role is null)
                {
                    // Without the role the account exists but sees nothing, which is worse than no
                    // account at all: it LOOKS provisioned and is not.
                    logger.LogError(
                        "Account provisioning skipped for employee {Id}: role '{Code}' does not exist in this tenant.",
                        employeeId, DefaultRoleCode);
                    return null;
                }

                var userName = await BuildUniqueUserNameAsync(emp.First, emp.Last, emp.EmployeeNumber);

                // TWO different addresses, and conflating them was a bug (§12.58):
                //   loginEmail   — goes on the account. MUST be unique (IX_User_NormalizedEmail).
                //   contactEmail — where the welcome mail goes. The employee's OWN address whenever
                //                  they have one, because that is the inbox they actually read.
                // They differ only when the employee's address is already some other account's
                // login, which is a data clash, not a reason to mail them somewhere they cannot see.
                var loginEmail = await ResolveUniqueEmailAsync(emp.Email, userName);
                var contactEmail = string.IsNullOrWhiteSpace(emp.Email) ? loginEmail : emp.Email.Trim();
                var password = GeneratePassword();

                var fullName = string.Join(" ",
                    new[] { emp.First, emp.Last }.Where(n => !string.IsNullOrWhiteSpace(n))).Trim();
                if (string.IsNullOrWhiteSpace(fullName)) fullName = emp.EmployeeNumber;

                // User.Create rejects a blank phone number, and Person.PhoneNumber is optional.
                var phone = string.IsNullOrWhiteSpace(emp.Phone) ? "-" : emp.Phone.Trim();

                var user = User.Create(fullName, loginEmail, phone, userName, passwordHasher.Hash(password));
                user.LinkEmployee(employeeId);
                await users.AddAsync(user);

                var membership = TenantUser.Create(tenantId.Value, user.Id);
                await tenantUsers.AddAsync(membership);
                await tenantUserRoles.AddAsync(
                    TenantUserRole.Create(membership.Id, role.Id, currentUser.GetCurrentUserId()));

                await users.SaveChangesAsync();
                logger.LogInformation(
                    "Provisioned account {UserName} ({UserId}) for employee {Number} with role {Role}.",
                    userName, user.Id, emp.EmployeeNumber, DefaultRoleCode);

                var notified = await NotifyAsync(
                    employeeId, emp.EmployeeNumber, fullName, userName, loginEmail, contactEmail, password);
                return new ProvisionedAccount(user.Id, userName, loginEmail, notified);
            }
            catch (Exception ex)
            {
                // The employee is already saved. Losing the account is recoverable by hand; failing
                // the registration because of it is not what was asked for.
                logger.LogError(ex,
                    "Account provisioning failed for employee {Id}; the employee record stands.", employeeId);
                return null;
            }
        }

        /// <summary>
        /// [first letter of first name][last name], lowercased, non-alphanumerics stripped, so
        /// "John Doe" becomes "jdoe".
        ///
        /// <para>The surname here is <c>Person.GrandFatherName</c>, NOT <c>FatherName</c>. That is the
        /// field this system already treats as the family name: <c>Person.Create</c> takes first +
        /// grandfather as the two REQUIRED names, and display names are built as
        /// <c>FirstName + " " + GrandFatherName</c> ("Berhan Meshesha").</para>
        ///
        /// <para>Collisions take a numeric suffix (jdoe, jdoe2, jdoe3). Uniqueness is checked ACROSS
        /// tenants, because a username is what a person types to log in.</para>
        /// </summary>
        private async Task<string> BuildUniqueUserNameAsync(string? first, string? last, string employeeNumber)
        {
            static string Clean(string? s) => new((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());

            var f = Clean(first);
            var l = Clean(last);
            var baseName = (f.Length > 0 ? f.Substring(0, 1) : string.Empty) + l;
            if (string.IsNullOrWhiteSpace(baseName)) baseName = Clean(employeeNumber);
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "user";
            baseName = baseName.ToLowerInvariant();

            var prefix = baseName.ToUpperInvariant();
            var taken = await users.GetAllWithoutTenantFilter()
                .Where(u => u.NormalizedUserName.StartsWith(prefix))
                .Select(u => u.NormalizedUserName)
                .ToListAsync();
            var set = taken.ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!set.Contains(baseName)) return baseName;
            for (var i = 2; i < 1000; i++)
            {
                var candidate = baseName + i;
                if (!set.Contains(candidate)) return candidate;
            }
            return baseName + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        /// <summary>
        /// The account's address: the one the employee was registered with, or
        /// <c>[username]@cybersoft.com</c> when none was given.
        ///
        /// <para>⚠️ It must also be UNIQUE. <c>IX_User_NormalizedEmail</c> is a unique index (filtered
        /// to non-blank) and is NOT tenant-scoped, so two logins cannot share an address. Checking
        /// only the username — which is what the first version did — meant that registering an
        /// employee whose e-mail already belonged to another account threw
        /// <c>DbUpdateException 2601</c> and lost the whole provisioning.</para>
        ///
        /// <para>On a clash the account falls back to the generated address, which is unique because
        /// the username already is. The employee keeps their address on the EMPLOYEE record; it is
        /// only the login that has to differ.</para>
        /// </summary>
        private async Task<string> ResolveUniqueEmailAsync(string? requested, string userName)
        {
            var generated = userName + "@" + DefaultEmailDomain;
            var wanted = string.IsNullOrWhiteSpace(requested) ? generated : requested.Trim();

            if (!await EmailTakenAsync(wanted)) return wanted;

            // The fallback applies to the LOGIN address only. The welcome mail still goes to the
            // employee's own address — see the contactEmail note in ProvisionAsync.
            var owner = await users.GetAllWithoutTenantFilter()
                .Where(u => u.NormalizedEmail == wanted.ToUpperInvariant())
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();
            logger.LogWarning(
                "E-mail {Requested} is already the LOGIN address of user {Owner}, so account {UserName} signs in as "
                + "{Fallback} instead. The credentials are still sent to {Requested}. Reconcile the duplicate address.",
                wanted, owner ?? "(unknown)", userName, generated);

            if (!await EmailTakenAsync(generated)) return generated;
            for (var i = 2; i < 1000; i++)
            {
                var candidate = userName + "-" + i + "@" + DefaultEmailDomain;
                if (!await EmailTakenAsync(candidate)) return candidate;
            }
            return userName + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + "@" + DefaultEmailDomain;
        }

        private async Task<bool> EmailTakenAsync(string email)
        {
            var normalized = email.ToUpperInvariant();
            return await users.GetAllWithoutTenantFilter().AnyAsync(u => u.NormalizedEmail == normalized);
        }

        private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";   // no I or O
        private const string Lower = "abcdefghijkmnopqrstuvwxyz";  // no l
        private const string Digits = "23456789";                  // no 0 or 1
        private const string Symbols = "!@#$%*?";

        /// <summary>
        /// A random 14-character password with at least one of each class, so it satisfies any
        /// reasonable policy in Core.Setting. Look-alike characters are excluded because this
        /// password is read off an e-mail and typed by hand.
        /// </summary>
        private static string GeneratePassword()
        {
            var all = Upper + Lower + Digits + Symbols;
            var chars = new List<char>
            {
                Upper[RandomNumberGenerator.GetInt32(Upper.Length)],
                Lower[RandomNumberGenerator.GetInt32(Lower.Length)],
                Digits[RandomNumberGenerator.GetInt32(Digits.Length)],
                Symbols[RandomNumberGenerator.GetInt32(Symbols.Length)],
            };
            while (chars.Count < 14) chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);

            // Fisher-Yates with a crypto RNG, so the guaranteed classes are not always at the front.
            for (var i = chars.Count - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Sends the credentials through the administrator's template.
        ///
        /// <para>The address rides in as <c>SubjectAddresses</c> (the EventSubject rule): on a
        /// brand-new employee no other recipient rule can resolve them yet.</para>
        ///
        /// <para>⚠️ It is delivered to <paramref name="contactEmail"/> — the employee's OWN address —
        /// which is not always the address the account signs in with. The <c>{{Email}}</c> token
        /// carries the LOGIN address, so the message tells them what to type even when the two
        /// differ.</para>
        /// </summary>
        private async Task<bool> NotifyAsync(
            Guid employeeId, string employeeNumber, string fullName, string userName,
            string loginEmail, string contactEmail, string password)
        {
            if (!string.Equals(loginEmail, contactEmail, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation(
                    "Credentials for {UserName} go to {Contact} (the employee's own address); the account signs in as {Login}.",
                    userName, contactEmail, loginEmail);
            }

            var sent = await dispatcher.DispatchAsync(new NotificationContext(
                NotificationEvents.EmployeeAccountCreated,
                new Dictionary<string, string?>
                {
                    ["EmployeeName"] = fullName,
                    ["EmployeeNumber"] = employeeNumber,
                    ["UserName"] = userName,
                    ["Password"] = password,
                    ["Email"] = loginEmail,
                },
                RequesterEmployeeId: employeeId,
                EntityType: nameof(Employee),
                EntityId: employeeId,
                SubjectAddresses: [contactEmail]));

            if (sent == 0)
            {
                // Deliberately NOT falling back to a hardcoded mail: credentials are exactly the
                // content an administrator should have approved the wording of. Logged loudly,
                // because the employee is now waiting for a message nobody is going to send.
                logger.LogWarning(
                    "Account {UserName} was created but NO credentials e-mail was sent - configure a template for "
                    + "'{EventKey}' under Email Templates, with a 'Who the event is about' recipient rule. "
                    + "Until then the password has to be delivered by hand.",
                    userName, NotificationEvents.EmployeeAccountCreated);
            }
            return sent > 0;
        }
    }
}
