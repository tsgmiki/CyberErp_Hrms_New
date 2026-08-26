/*
 * Gives ordinary staff (the UserRole role) the My Training screen:
 *   /hrms/myTraining → View   (CPD summary, own enrollments, own certificates)
 *
 * WHY ONLY myTraining
 *   myExit was ALSO requested and needs NO grant: UserRole already holds `/myExit` with all six
 *   privileges. ⚠️ Permission links are matched through
 *   `IEndpointPermissionService.Normalize()`, which strips the leading slash AND the `hrms/`
 *   namespace — so `/myExit` and `/hrms/myExit` are the SAME permission, and grants across rows are
 *   unioned. A namespaced duplicate would have added nothing and could not have restricted anything.
 *   Verified: with `/myExit` alone, UserRole already reaches the Edit endpoints on
 *   EmployeeTerminationController.
 *
 *   The same is true of most of UserRole's catalogue — `/annualLeave`, `/grievance`,
 *   `/hiringRequest`, `/transferRequest`, `/appraisal`, `/disciplinaryCase`, `/clearanceApprovals`
 *   and more are all held UN-namespaced with all six privileges. Query for `/hrms/x` alone and you
 *   will wrongly conclude the role has nothing.
 *
 * WHY VIEW ONLY
 *   `myTraining` gates controllers SHARED with the HR-side training registers
 *   ([RequirePermission("trainingSession", "myTraining")] and ("trainingCertificate",
 *   "myTraining")), and a privilege is derived from the HTTP verb, so one grant covers every
 *   endpoint deriving it. The reads are safe — GetAllTrainingEnrollments, GetAllTrainingCertificates
 *   and GetCpdSummary all re-check the caller with CanAccessEmployeeAsync, which for a non-manager
 *   means SELF ONLY. These are deliberately withheld:
 *
 *     Add    → POST /TrainingCertificate (SaveTrainingCertificate) is UNSCOPED, so Add would let
 *              staff create training certificates for anybody.
 *     Edit   → participation, issue, renew, withdraw — HR functions, unscoped.
 *     Delete → not needed by any self-service screen.
 *
 *   CONSEQUENCE: staff can SEE their training record but cannot self-enroll. To allow that, add an
 *   ownership check to SaveTrainingCertificate first, then grant Add.
 *
 * Idempotent: guarded by NOT EXISTS, and an existing row is topped up rather than duplicated.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rv   varbinary(8)  = 0x0000000000000001;
DECLARE @role nvarchar(200) = N'UserRole';
DECLARE @link nvarchar(200) = N'/hrms/myTraining';
DECLARE @by   nvarchar(200) = N'grant-userrole-self-service.sql';

BEGIN TRAN;

INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId,
     CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion)
SELECT NEWID(), r.Id, o.Id,
       1, 0, 0, 0, 0, 0,
       SYSUTCDATETIME(), SYSUTCDATETIME(), @by, @by, @rv
FROM Core.TenantOperation o
CROSS JOIN Core.TenantRole r
WHERE o.Link = @link AND r.Name = @role
  AND NOT EXISTS (
      SELECT 1 FROM Core.TenantRolePermission x
      WHERE x.TenantRoleId = r.Id AND x.TenantOperationId = o.Id);

-- Top up an existing row; never downgrade a privilege the tenant granted deliberately.
UPDATE p
SET CanView = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @by
FROM Core.TenantRolePermission p
JOIN Core.TenantRole r ON r.Id = p.TenantRoleId AND r.Name = @role
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId AND o.Link = @link
WHERE p.CanView = 0;

COMMIT;

SELECT r.Name AS RoleName, o.Link,
       p.CanView, p.CanAdd, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport
FROM Core.TenantRolePermission p
JOIN Core.TenantRole r ON r.Id = p.TenantRoleId
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId
WHERE o.Link IN (N'/hrms/myTraining', N'/myExit')
ORDER BY o.Link, r.Name;
