using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>What a <see cref="LoginTrail"/> row records.</summary>
public static class LoginEventTypes
{
    public const string LoginSucceeded = "LoginSucceeded";
    public const string LoginFailed = "LoginFailed";
    public const string Logout = "Logout";
}

/// <summary>
/// An authentication event — ported from the SRMS platform schema. Append-only; never updated.
///
/// <para>The system had NO record of who signed in, from where, or of failed attempts. That is the
/// gap this closes: a lockout, a shared account or a credential-stuffing run is invisible without it,
/// and "who was logged in when that changed?" had no answer beyond the row's CreatedBy.</para>
///
/// <para><see cref="UserNameAttempted"/> is stored SEPARATELY from <see cref="UserId"/> and is always
/// populated: a failed attempt frequently has no user to point at (the name may not exist), and that
/// is precisely the case worth recording. <see cref="FailureReason"/> holds a short classification,
/// never the supplied password.</para>
/// </summary>
public class LoginTrail : BaseEntity
{
    /// <summary>Null when the attempt could not be resolved to a user (unknown name).</summary>
    public Guid? UserId { get; private set; }
    /// <summary>What the caller typed — kept even when it matches no account.</summary>
    public string UserNameAttempted { get; private set; } = string.Empty;
    public string EventType { get; private set; } = LoginEventTypes.LoginSucceeded;
    public DateTime Date { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    /// <summary>Free-text outcome, e.g. "Success" / "Failed".</summary>
    public string? Status { get; private set; }
    /// <summary>Why it failed, classified. NEVER the submitted credential.</summary>
    public string? FailureReason { get; private set; }
    public string? UserAgent { get; private set; }

    private LoginTrail() : base() { }

    public static LoginTrail Success(Guid userId, string userName, string? ipAddress, string? userAgent) =>
        new()
        {
            UserId = userId,
            UserNameAttempted = Trim(userName, 400),
            EventType = LoginEventTypes.LoginSucceeded,
            Status = "Success",
            Date = DateTime.UtcNow,
            IpAddress = Trim(ipAddress ?? "", 90),
            UserAgent = Trim(userAgent, 2000),
        };

    public static LoginTrail Failure(string userName, string? ipAddress, string? userAgent, string reason) =>
        new()
        {
            UserId = null,
            UserNameAttempted = Trim(userName, 400),
            EventType = LoginEventTypes.LoginFailed,
            Status = "Failed",
            Date = DateTime.UtcNow,
            IpAddress = Trim(ipAddress ?? "", 90),
            UserAgent = Trim(userAgent, 2000),
            FailureReason = Trim(reason, 1000),
        };

    public static LoginTrail Logout(Guid? userId, string userName, string? ipAddress, string? userAgent) =>
        new()
        {
            UserId = userId,
            UserNameAttempted = Trim(userName, 400),
            EventType = LoginEventTypes.Logout,
            Status = "Success",
            Date = DateTime.UtcNow,
            IpAddress = Trim(ipAddress ?? "", 90),
            UserAgent = Trim(userAgent, 2000),
        };

    /// <summary>Truncate rather than throw: an audit row must never be the reason a login fails.</summary>
    private static string Trim(string? value, int max)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= max ? value : value[..max];
    }
}

/// <summary>
/// Platform-wide security and operational policy — ported from the SRMS platform schema.
///
/// <para>A SINGLETON: exactly one row governs the deployment, which is why it carries no tenant
/// discriminator and no soft-delete. Password rules, lockout thresholds and session limits currently
/// live nowhere — they are implicit in code or absent entirely — so this is the row that makes them
/// configurable rather than hard-coded.</para>
///
/// <para>⚠️ The SMTP fields OVERLAP <c>appsettings.json</c>'s <c>Email</c> section, which is what
/// <c>SmtpEmailService</c> actually reads today. Nothing has been repointed at these columns: doing so
/// silently would change where mail is sent from. Treat them as the intended future home, not the
/// current source of truth.</para>
/// </summary>
public class Setting : BaseEntity, IAggregateRoot
{
    // ---- Password policy ---------------------------------------------------
    public int MinimumPasswordLength { get; private set; } = 8;
    public bool RequireUppercase { get; private set; } = true;
    public bool RequireNumbers { get; private set; } = true;
    public bool RequireSpecialCharacters { get; private set; } = true;
    /// <summary>0 = passwords never expire.</summary>
    public int PasswordExpiryDays { get; private set; }
    /// <summary>0 = previous passwords are not remembered.</summary>
    public int PasswordHistoryCount { get; private set; }

    // ---- Session / lockout -------------------------------------------------
    public int SessionTimeoutMinutes { get; private set; } = 30;
    public int MaxConcurrentSessions { get; private set; } = 3;
    public int MaxLoginAttempts { get; private set; } = 5;
    public int LockoutDurationMinutes { get; private set; } = 30;
    public bool EnforceTwoFactorForAll { get; private set; }
    public bool EnforceTwoFactorForAdmins { get; private set; }

    // ---- Mail (see the class remarks — NOT yet the source of truth) --------
    public string SmtpHost { get; private set; } = string.Empty;
    public int SmtpPort { get; private set; }
    public string SmtpUser { get; private set; } = string.Empty;
    public bool SmtpUseTls { get; private set; } = true;

    // ---- Backup ------------------------------------------------------------
    public bool AutoBackup { get; private set; }
    public string BackupFrequency { get; private set; } = "daily";
    public int BackupRetentionDays { get; private set; } = 30;

    private Setting() : base() { }

    /// <summary>The out-of-the-box policy, used when the deployment has no row yet.</summary>
    public static Setting CreateDefault() => new();

    public void UpdatePasswordPolicy(int minimumLength, bool requireUppercase, bool requireNumbers,
        bool requireSpecial, int expiryDays, int historyCount)
    {
        if (minimumLength is < 4 or > 128)
            throw new ArgumentException("Minimum password length must be between 4 and 128.", nameof(minimumLength));
        if (expiryDays < 0) throw new ArgumentException("Expiry days cannot be negative.", nameof(expiryDays));
        if (historyCount < 0) throw new ArgumentException("History count cannot be negative.", nameof(historyCount));
        MinimumPasswordLength = minimumLength;
        RequireUppercase = requireUppercase;
        RequireNumbers = requireNumbers;
        RequireSpecialCharacters = requireSpecial;
        PasswordExpiryDays = expiryDays;
        PasswordHistoryCount = historyCount;
        base.Update();
    }

    public void UpdateSessionPolicy(int sessionTimeoutMinutes, int maxConcurrentSessions,
        int maxLoginAttempts, int lockoutDurationMinutes, bool twoFactorForAll, bool twoFactorForAdmins)
    {
        if (sessionTimeoutMinutes <= 0)
            throw new ArgumentException("Session timeout must be positive.", nameof(sessionTimeoutMinutes));
        if (maxLoginAttempts <= 0)
            throw new ArgumentException("Max login attempts must be positive.", nameof(maxLoginAttempts));
        SessionTimeoutMinutes = sessionTimeoutMinutes;
        MaxConcurrentSessions = maxConcurrentSessions;
        MaxLoginAttempts = maxLoginAttempts;
        LockoutDurationMinutes = lockoutDurationMinutes;
        EnforceTwoFactorForAll = twoFactorForAll;
        EnforceTwoFactorForAdmins = twoFactorForAdmins;
        base.Update();
    }

    public void UpdateOperations(string smtpHost, int smtpPort, string smtpUser, bool smtpUseTls,
        bool autoBackup, string backupFrequency, int backupRetentionDays)
    {
        SmtpHost = smtpHost?.Trim() ?? string.Empty;
        SmtpPort = smtpPort;
        SmtpUser = smtpUser?.Trim() ?? string.Empty;
        SmtpUseTls = smtpUseTls;
        AutoBackup = autoBackup;
        BackupFrequency = string.IsNullOrWhiteSpace(backupFrequency) ? "daily" : backupFrequency.Trim();
        BackupRetentionDays = backupRetentionDays < 0 ? 0 : backupRetentionDays;
        base.Update();
    }
}

/// <summary>
/// One user's interface preferences — ported from the SRMS platform schema.
///
/// <para>Language, timezone, date and number format, landing page and theme are currently held in the
/// browser, so they are lost on a new device and cannot be reported on. Tenant-scoped, because the
/// same person may hold different preferences in different tenants.</para>
/// </summary>
public class UserPreference : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Language { get; private set; } = "en";
    public string? TimeZone { get; private set; }
    public string? DateFormat { get; private set; }
    public string? NumberFormat { get; private set; }
    /// <summary>Route the user lands on after sign-in, e.g. "/".</summary>
    public string? LandingPage { get; private set; }
    public string? Theme { get; private set; }
    public bool EmailNotifications { get; private set; } = true;
    public bool InAppNotifications { get; private set; } = true;
    public bool ApprovalNotifications { get; private set; } = true;

    private UserPreference() : base() { }

    public static UserPreference CreateDefault(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User is required.", nameof(userId));
        return new UserPreference { UserId = userId };
    }

    public void Update(string language, string? timeZone, string? dateFormat, string? numberFormat,
        string? landingPage, string? theme, bool emailNotifications, bool inAppNotifications,
        bool approvalNotifications)
    {
        Language = string.IsNullOrWhiteSpace(language) ? "en" : language.Trim();
        TimeZone = timeZone?.Trim();
        DateFormat = dateFormat?.Trim();
        NumberFormat = numberFormat?.Trim();
        LandingPage = landingPage?.Trim();
        Theme = theme?.Trim();
        EmailNotifications = emailNotifications;
        InAppNotifications = inAppNotifications;
        ApprovalNotifications = approvalNotifications;
        base.Update();
    }
}
