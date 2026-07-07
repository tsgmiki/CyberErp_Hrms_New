
namespace CyberErp.Hrms.Dom.Entities.Core;

public class User : BaseEntity, IAggregateRoot
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;

    /// <summary>Branch this user administers (null for Head Office / unassigned).</summary>
    public Guid? BranchId { get; private set; }
    /// <summary>Head Office users have global visibility across all branches.</summary>
    public bool IsHeadOffice { get; private set; }

    private User() : base() { }

    /// <summary>Marks the user as Head Office (global oversight). Used for the tenant owner at signup.</summary>
    public void MarkAsHeadOffice()
    {
        IsHeadOffice = true;
        BranchId = null;
    }

    /// <summary>Assigns the user to a branch as a branch-scoped administrator.</summary>
    public void AssignBranch(Guid? branchId, bool isHeadOffice = false)
    {
        BranchId = branchId;
        IsHeadOffice = isHeadOffice;
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
            Password = password
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
        Password = password;
        base.Update();
    }
}

