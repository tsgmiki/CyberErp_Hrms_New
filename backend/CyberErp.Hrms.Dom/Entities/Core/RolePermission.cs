
namespace CyberErp.Hrms.Dom.Entities.Core;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid OperationId { get; private set; }
    public bool CanAdd { get; private set; }
    public bool CanEdit { get; private set; }
    public bool CanDelete { get; private set; }
    public bool CanApprove { get; private set; }
    public bool CanView { get; private set; }
    public Role Role { get; private set; } = null!;
    public Operation Operation { get; private set; } = null!;

    private RolePermission() : base() { }

    public static RolePermission Create(
        Guid roleId,
        Guid operationId,
        bool canAdd = false,
        bool canEdit = false,
        bool canDelete = false,
        bool canApprove = false,
        bool canView = true)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        if (operationId == Guid.Empty)
            throw new ArgumentException("Operation ID cannot be empty.", nameof(operationId));

        return new RolePermission
        {
            RoleId = roleId,
            OperationId = operationId,
            CanAdd = canAdd,
            CanEdit = canEdit,
            CanDelete = canDelete,
            CanApprove = canApprove,
            CanView = canView
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(
        Guid roleId,
        Guid operationId,
        bool canAdd,
        bool canEdit,
        bool canDelete,
        bool canApprove,
        bool canView)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        if (operationId == Guid.Empty)
            throw new ArgumentException("Operation ID cannot be empty.", nameof(operationId));

        RoleId = roleId;
        OperationId = operationId;
        CanAdd = canAdd;
        CanEdit = canEdit;
        CanDelete = canDelete;
        CanApprove = canApprove;
        CanView = canView;
        base.Update();
    }

    public void UpdatePermissions(
        bool? canAdd = null,
        bool? canEdit = null,
        bool? canDelete = null,
        bool? canApprove = null,
        bool? canView = null)
    {
        if (canAdd.HasValue) CanAdd = canAdd.Value;
        if (canEdit.HasValue) CanEdit = canEdit.Value;
        if (canDelete.HasValue) CanDelete = canDelete.Value;
        if (canApprove.HasValue) CanApprove = canApprove.Value;
        if (canView.HasValue) CanView = canView.Value;
        base.Update();
    }

    public void GrantAll()
    {
        CanAdd = true;
        CanEdit = true;
        CanDelete = true;
        CanApprove = true;
        CanView = true;
        base.Update();
    }

    public void RevokeAll()
    {
        CanAdd = false;
        CanEdit = false;
        CanDelete = false;
        CanApprove = false;
        CanView = true; // View is usually retained
        base.Update();
    }
}

