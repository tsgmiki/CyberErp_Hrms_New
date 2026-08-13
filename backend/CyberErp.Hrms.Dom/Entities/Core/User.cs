
namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Account lifecycle state — see <see cref="User.AccountStatus"/>.</summary>
public static class UserAccountStatuses
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Locked = "Locked";
    public const string Invited = "Invited";
}

/*
 * Aligned with the SRMS platform schema (2026-08-13). `Password` is now `PasswordHash`, and the
 * account-security, profile-picture and normalised-lookup columns SRMS carries were added.
 *
 * ⚠️ `TenantId` is KEPT here, unlike SRMS, which has no such column. In SRMS a user is a GLOBAL
 * identity and tenancy lives entirely in TenantUser; dropping the discriminator here would make
 * Repository<T>'s tenant filter — which every screen depends on — stop scoping users, roles and
 * operations. That is a separate, deliberate change; see logic.md §12.5.
 */
public class User : BaseEntity, IAggregateRoot
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;

    /// <summary>PBKDF2 hash (see <c>Encryption.GenerateHash</c>) — never the plaintext.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Upper-cased <see cref="UserName"/>, so sign-in can look up case-insensitively.</summary>
    public string NormalizedUserName { get; private set; } = string.Empty;
    /// <summary>Upper-cased <see cref="Email"/>. Blank for the many accounts with no address on file.</summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>Active | Suspended | Locked | Invited — see <see cref="UserAccountStatuses"/>.</summary>
    public string AccountStatus { get; private set; } = UserAccountStatuses.Active;
    public int FailedLoginAttempts { get; private set; }
    /// <summary>When set and in the future, sign-in is refused regardless of the password.</summary>
    public DateTime? LockoutEndUtc { get; private set; }
    public bool TwoFactorEnabled { get; private set; }

    public byte[]? ProfilePicture { get; private set; }
    public string? ProfilePictureContentType { get; private set; }

    /// <summary>Platform-wide administrator, above any single tenant's roles.</summary>
    public bool IsPlatformAdministrator { get; private set; }

    /// <summary>
    /// The employee this login account belongs to (nullable — system/owner accounts have none).
    /// The relationship is owned by the User table (FK here). The user's branch scope and
    /// head-office visibility are DERIVED from this employee's branch at login.
    /// </summary>
    public Guid? EmployeeId { get; private set; }

    private User() : base() { }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>Links (or unlinks, when null) this login account to an employee record.</summary>
    public void LinkEmployee(Guid? employeeId)
    {
        EmployeeId = employeeId;
        base.Update();
    }

    public static User Create(
        string fullName,
        string email,
        string phoneNumber,
        string userName,
        string password)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty.", nameof(userName));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        return new User
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            UserName = userName,
            PasswordHash = password,
            NormalizedUserName = Normalize(userName),
            NormalizedEmail = Normalize(email),
            AccountStatus = UserAccountStatuses.Active
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(
        string fullName,
        string email,
        string phoneNumber,
        string userName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty.", nameof(userName));

        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        UserName = userName;
        NormalizedUserName = Normalize(userName);
        NormalizedEmail = Normalize(email);
        base.Update();
    }

    public void UpdateProfile(
        string? fullName = null,
        string? email = null,
        string? phoneNumber = null)
    {
        if (fullName != null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
            FullName = fullName;
        }

        if (email != null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            Email = email;
            NormalizedEmail = Normalize(email);
        }

        if (phoneNumber != null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));
            PhoneNumber = phoneNumber;
        }

        base.Update();
    }

    public void UpdateCredentials(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty.", nameof(userName));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        UserName = userName;
        NormalizedUserName = Normalize(userName);
        PasswordHash = password;
        base.Update();
    }

    // ---- Account security (the SRMS columns) --------------------------------------

    /// <summary>True when a lockout is in force right now.</summary>
    public bool IsLockedOut() => LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;

    /// <summary>Records a failed sign-in, locking the account once the threshold is reached.</summary>
    public void RegisterFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (maxAttempts > 0 && FailedLoginAttempts >= maxAttempts)
            LockoutEndUtc = DateTime.UtcNow.Add(lockoutDuration);
        base.Update();
    }

    /// <summary>Clears the failure counter and any lockout after a successful sign-in.</summary>
    public void RegisterSuccessfulLogin()
    {
        if (FailedLoginAttempts == 0 && LockoutEndUtc is null) return;
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
        base.Update();
    }

    public void SetAccountStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Account status cannot be empty.", nameof(status));
        AccountStatus = status.Trim();
        base.Update();
    }

    public void SetTwoFactor(bool enabled)
    {
        TwoFactorEnabled = enabled;
        base.Update();
    }

    public void SetPlatformAdministrator(bool isPlatformAdministrator)
    {
        IsPlatformAdministrator = isPlatformAdministrator;
        base.Update();
    }

    /// <summary>Stores (or clears, when null) the account's profile picture.</summary>
    public void SetProfilePicture(byte[]? picture, string? contentType)
    {
        ProfilePicture = picture;
        ProfilePictureContentType = picture is null ? null : contentType;
        base.Update();
    }

    /// <summary>
    /// Recomputes the normalised lookup columns. Needed only for rows written before they existed —
    /// every mutator above keeps them in step.
    /// </summary>
    public void RefreshNormalizedFields()
    {
        NormalizedUserName = Normalize(UserName);
        NormalizedEmail = Normalize(Email);
    }
}

