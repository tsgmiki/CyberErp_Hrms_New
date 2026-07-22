
namespace CyberErp.Hrms.Dom.Entities.Core;

public class UserRole : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid UserId { get; private set; }
    public Role Role { get; private set; } = null!;
    public User User { get; private set; } = null!;
    private UserRole() : base() { }
    public static UserRole Create(
        Guid roleId,
        Guid userId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new UserRole
        {
            RoleId = roleId,
            UserId = userId
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }
    public void Update(Guid roleId, Guid userId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        RoleId = roleId;
        UserId = userId;
        base.Update();
    }

    public void UpdateRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        RoleId = roleId;
        base.Update();
    }

    public void UpdateUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        UserId = userId;
        base.Update();
    }
}

