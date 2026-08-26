/*
 * Lets ordinary staff act on approvals: grants the UserRole role View + Approve on /hrms/workflow.
 *
 * WHY THIS IS NEEDED
 *   Approvals do not run through each module's own controller. Every approve/reject goes through
 *   the generic WorkflowController, which is gated on the "workflow" operation. HR Admin,
 *   HR Officer and Department Manager hold /hrms/workflow; UserRole did not — so a line manager
 *   who IS the designated approver for a step was refused with 403 before the engine ever got to
 *   check whether they were authorised for that particular instance.
 *
 * WHAT THIS DOES NOT DO
 *   It does not let everyone approve everything. IWorkflowApproverAuth still decides who may act
 *   on a given instance, and a non-approver gets 400 "You are not an authorized approver for
 *   step X". This grant only gets them past the endpoint gate.
 *
 *   ⚠️ It DOES give every UserRole holder the workflow TRACKING list (GET /Workflow) and its stats,
 *   i.e. visibility of runs across the tenant. That is the trade-off of gating approvals on a
 *   single shared operation; narrowing it would mean splitting the controller.
 *
 * PRIVILEGES: View + Approve only. "approve" and "reject" both derive PermissionAccess.Approve,
 * so those two cover the whole approver journey. Add/Edit/Delete/Export are deliberately withheld.
 *
 * Idempotent: guarded by NOT EXISTS, and an existing row is topped up rather than duplicated.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rv   varbinary(8)  = 0x0000000000000001;
DECLARE @link nvarchar(200) = N'/hrms/workflow';
DECLARE @role nvarchar(200) = N'UserRole';
DECLARE @by   nvarchar(200) = N'grant-userrole-workflow.sql';

BEGIN TRAN;

-- One row per (role, operation) pair; a tenant may hold several of either.
;WITH pairs AS (
    SELECT r.Id AS TenantRoleId, o.Id AS TenantOperationId
    FROM Core.TenantRole r
    CROSS JOIN Core.TenantOperation o
    WHERE r.Name = @role AND o.Link = @link
)
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId,
     CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion)
SELECT NEWID(), p.TenantRoleId, p.TenantOperationId,
       1, 0, 0, 0, 1, 0,
       SYSUTCDATETIME(), SYSUTCDATETIME(), @by, @by, @rv
FROM pairs p
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantRoleId = p.TenantRoleId AND x.TenantOperationId = p.TenantOperationId);

-- A row that already existed (from an earlier partial grant) is topped up, never downgraded.
UPDATE p
SET CanView = 1, CanApprove = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @by
FROM Core.TenantRolePermission p
JOIN Core.TenantRole r ON r.Id = p.TenantRoleId
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId
WHERE r.Name = @role AND o.Link = @link AND (p.CanView = 0 OR p.CanApprove = 0);

COMMIT;

SELECT r.Name AS RoleName, o.Link,
       p.CanView, p.CanAdd, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport
FROM Core.TenantRolePermission p
JOIN Core.TenantRole r ON r.Id = p.TenantRoleId
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId
WHERE o.Link = @link
ORDER BY r.Name;
