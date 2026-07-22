namespace CyberErp.Hrms.Dom.Entities.Core;

public class Role : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() : base() { }

    public static Role Create(
        string name,
        string? code = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        return new Role
        {
            Name = name,
            Code = code
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(string? name = null, string? code = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty.", nameof(name));
            Name = name;
        }

        if (code != null)
            Code = code;

        base.Update();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name;
        base.Update();
    }

    // Aggregate methods for RolePermission

    public RolePermission AddPermission(
        Guid operationId,
        bool canAdd = false,
        bool canEdit = false,
        bool canDelete = false,
        bool canApprove = false,
        bool canView = true)
    {
        // Check if permission already exists for this operation
        if (_rolePermissions.Any(p => p.OperationId == operationId))
            throw new InvalidOperationException("Permission for this operation already exists.");

        var permission = RolePermission.Create(
            Id,
            operationId,
            canAdd, canEdit, canDelete, canApprove, canView);

        _rolePermissions.Add(permission);
        return permission;
    }

    public RolePermission? GetPermission(Guid operationId)
    {
        return _rolePermissions.FirstOrDefault(p => p.OperationId == operationId);
    }

    public void UpdatePermission(
        Guid operationId,
        bool? canAdd = null,
        bool? canEdit = null,
        bool? canDelete = null,
        bool? canApprove = null,
        bool? canView = null)
    {
        var existingPermission = GetPermission(operationId);
        if (existingPermission == null)
            throw new InvalidOperationException($"Permission for operation {operationId} not found.");

        existingPermission.UpdatePermissions(canAdd, canEdit, canDelete, canApprove, canView);
    }

    public void RemovePermission(Guid operationId)
    {
        var permission = GetPermission(operationId);
        if (permission == null)
            throw new InvalidOperationException($"Permission for operation {operationId} not found.");

        _rolePermissions.Remove(permission);
    }

    public void ClearAllPermissions()
    {
        _rolePermissions.Clear();
    }

    public void GrantAllPermissions(Guid operationId)
    {
        var permission = GetPermission(operationId);
        if (permission == null)
            throw new InvalidOperationException($"Permission for operation {operationId} not found.");

        permission.GrantAll();
    }

    public void RevokeAllPermissions(Guid operationId)
    {
        var permission = GetPermission(operationId);
        if (permission == null)
            throw new InvalidOperationException($"Permission for operation {operationId} not found.");

        permission.RevokeAll();
    }
}

