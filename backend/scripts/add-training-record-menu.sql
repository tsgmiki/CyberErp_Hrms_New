/*
 * Adds the Home portal's "My Training Record" entry — where a learner signs their completed
 * training (GxP option B, logic 12.90).
 *
 * WHY
 *   A Part 11 signature is an assertion by a named person at a recorded moment. It needs a surface
 *   that shows the exact wording being agreed to and asks for the password again; that surface is
 *   this screen, and it belongs beside the learner's other self-service pages rather than in the HR
 *   console.
 *
 * WHERE
 *   The SSMS ("Self Service") subsystem's module, beside /myLearning and /myObligations.
 *
 * BARE LINK, like every other SSMS operation. That is load-bearing: the HRMS permission gate strips
 *   only its OWN hrms/ prefix, so a bare link is what lets a Home screen authorise against the HRMS
 *   API (logic 12.69). The screen calls endpoints gated on `myTraining`, which staff already hold,
 *   and every one of them answers only for the signed-in employee.
 *
 * NOTE: this row grants nothing by itself. It controls whether the link is visible, never what the
 *   data allows -- and it certainly does not permit signing anything, which requires the password.
 *
 * GRANTS -- mirrors /myLearning exactly: everyone who sees their own training sees their own record.
 *
 * Idempotent: guarded by NOT EXISTS on the operation and on each grant.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by nvarchar(200) = N'add-training-record-menu.sql';
DECLARE @rv varbinary(8)  = 0x0000000000000001;
DECLARE @link nvarchar(200) = N'/myTrainingRecord';
DECLARE @source nvarchar(200) = N'/myLearning';

DECLARE @module uniqueidentifier =
    (SELECT TOP 1 o.ModuleId FROM Core.TenantOperation o WHERE o.Link = @source);

IF @module IS NULL
BEGIN
    RAISERROR(N'ABORTED: could not locate the self-service module via /myLearning. Check the operation catalogue before running.', 16, 1);
    RETURN;
END

SELECT 'BEFORE' AS Stage, @link AS Link,
       CASE WHEN EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = @link)
            THEN 'already present' ELSE 'to add' END AS State;

BEGIN TRAN;

-- NOTE: Core.TenantOperation and Core.TenantRolePermission carry NO TenantId column -- the operation
-- catalogue is SHARED across tenants; scoping happens through TenantRole.
INSERT INTO Core.TenantOperation
    (Id, ModuleId, Name, Link, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), @module, N'My Training Record', @link, N'FileSignature', 105, 1,
       SYSUTCDATETIME(), @by, @rv
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = @link);

INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), src.TenantRoleId, tgt.Id, src.CanView, src.CanAdd, 0, 0, 0, 0,
       SYSUTCDATETIME(), @by, @rv
FROM Core.TenantRolePermission src
JOIN Core.TenantOperation srcOp ON srcOp.Id = src.TenantOperationId AND srcOp.Link = @source
JOIN Core.TenantOperation tgt ON tgt.Link = @link
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantOperationId = tgt.Id AND x.TenantRoleId = src.TenantRoleId);

COMMIT;

SELECT 'AFTER: operation' AS Stage, o.Link, o.Name, o.Icon, o.DisplayOrder, o.IsActive
FROM Core.TenantOperation o WHERE o.Link = @link;

SELECT 'AFTER: who can see it' AS Stage, tr.Name AS RoleName, rp.CanView, rp.CanAdd
FROM Core.TenantRolePermission rp
JOIN Core.TenantOperation o ON o.Id = rp.TenantOperationId
JOIN Core.TenantRole tr ON tr.Id = rp.TenantRoleId
WHERE o.Link = @link
ORDER BY tr.Name;
