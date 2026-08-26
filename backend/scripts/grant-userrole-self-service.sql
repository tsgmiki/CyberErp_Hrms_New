/*
 * Gives ordinary staff (the UserRole role) the My Training screen:
 *   /hrms/myTraining → View + Add   (CPD summary, own enrollments and certificates; self-enroll)
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
 * WHY VIEW + ADD, AND NOT MORE
 *   `myTraining` gates controllers SHARED with the HR-side training registers
 *   ([RequirePermission("trainingSession", "myTraining")] and ("trainingCertificate",
 *   "myTraining")), and a privilege is derived from the HTTP verb, so one grant covers every
 *   endpoint deriving it. Both privileges granted here are safe because every endpoint behind them
 *   re-checks the caller in its own handler:
 *
 *     View → GetAllTrainingEnrollments, GetAllTrainingCertificates and GetCpdSummary all call
 *            CanAccessEmployeeAsync, which for a non-manager means SELF ONLY.
 *     Add  → exactly two endpoints derive it ("enroll" is an Add token, so is a bare POST):
 *              POST /TrainingEnrollment   → EnrollTraining calls CanAccessEmployeeAsync, so a staff
 *                                           member can enroll THEMSELVES (a manager, their team).
 *              POST /TrainingCertificate  → SaveTrainingCertificate calls
 *                                           TrainingCertificateShared.EnsureAdminAsync, so a
 *                                           non-HR caller is refused by the handler.
 *
 *   ⚠️ Still withheld:
 *     Edit   → RecordParticipation (marking attendance), certificate issue/renew, and withdraw.
 *              The first three are HR functions. CONSEQUENCE: a staff member can enroll but cannot
 *              WITHDRAW themselves — withdraw derives Edit, and granting it would unlock the other
 *              three too. Splitting that needs an explicit Access on the action, not a wider grant.
 *     Delete → not needed by any self-service screen.
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
       1, 1, 0, 0, 0, 0,
       SYSUTCDATETIME(), SYSUTCDATETIME(), @by, @by, @rv
FROM Core.TenantOperation o
CROSS JOIN Core.TenantRole r
WHERE o.Link = @link AND r.Name = @role
  AND NOT EXISTS (
      SELECT 1 FROM Core.TenantRolePermission x
      WHERE x.TenantRoleId = r.Id AND x.TenantOperationId = o.Id);

-- Top up an existing row; never downgrade a privilege the tenant granted deliberately.
UPDATE p
SET CanView = 1, CanAdd = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @by
FROM Core.TenantRolePermission p
JOIN Core.TenantRole r ON r.Id = p.TenantRoleId AND r.Name = @role
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId AND o.Link = @link
WHERE p.CanView = 0 OR p.CanAdd = 0;

COMMIT;

SELECT r.Name AS RoleName, o.Link,
       p.CanView, p.CanAdd, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport
FROM Core.TenantRolePermission p
JOIN Core.TenantRole r ON r.Id = p.TenantRoleId
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId
WHERE o.Link IN (N'/hrms/myTraining', N'/myExit')
ORDER BY o.Link, r.Name;
