/*
  Give every employee-linked account the ordinary employee role.

  WHY THIS EXISTS
  ---------------
  480 of the 490 employee accounts created by the NVI migration held NO ROLE AT ALL. They could still
  use the system only because `PerformanceVisibilityService.IsAdminAsync` short-circuits on
  `IsHeadOffice()`, which is true for every employee in a single-branch tenant — i.e. a bug was
  standing in for their permissions (see logic.md section 11).

  That made the system impossible to harden: `IEndpointPermissionService.HasAnyAsync` needs a role
  carrying CanView, so adding [RequirePermission] to a controller — or repointing IsAdminAsync off
  IsHeadOffice — returns 403 for those users. Assigning the role is the PREREQUISITE for both.

  WHAT IT DOES
  ------------
  1. Grants the ordinary role the employee-facing screens it was missing.
  2. Assigns that role to every employee-linked account that has none.

  Idempotent: safe to re-run. Only ADDS access; nothing is revoked, so it cannot lock anyone out.

  Run against each environment's CERP database. Take a backup first.
*/

SET NOCOUNT ON;

DECLARE @role uniqueidentifier = (SELECT Id FROM Core.Role WHERE Name = 'UserRole');
IF @role IS NULL
BEGIN
    RAISERROR('Role "UserRole" not found — check the ordinary-employee role name for this tenant.', 16, 1);
    RETURN;
END

DECLARE @permTenant nvarchar(64) = (SELECT TOP 1 TenantId FROM Core.RolePermission WHERE RoleId = @role);
DECLARE @userTenant nvarchar(64) = (SELECT TOP 1 TenantId FROM Core.UserRole);
DECLARE @rv varbinary(8) = 0x0000000000000001;

/* ---------------------------------------------------------------------------
   1. Employee-facing screens the ordinary role was missing.

   HR and manager screens are deliberately EXCLUDED even though they look
   self-service adjacent:
     /employeeGuarantee   -> the HR register; employees use /myGuarantees
     /transferRequest     -> "Manager Requests" module
     /exitQuestionnaire   -> Personnel (HR); employees use /myExit
     /compensationRequest -> employees already hold /myCompensation
   --------------------------------------------------------------------------- */
DECLARE @selfService TABLE (Link nvarchar(200) PRIMARY KEY);
INSERT INTO @selfService (Link) VALUES
    ('/myGuarantees'),      -- own guarantee commitments
    ('/myInsuranceClaims'), -- own insurance claims
    ('/myTraining'),        -- own training record
    ('/notifications'),     -- portal notification feed
    ('/workflow'),          -- Workflow Tracking: My Approvals / My Submissions
    ('/surveyTake'),        -- responding to a survey
    ('/recognitionWall'),   -- company-wide recognition wall
    ('/learningCommunity'), -- learning communities
    ('/appraisalAppeal');   -- appealing one's OWN appraisal

BEGIN TRAN;

/* The role usually already has a row per operation sitting at CanView = 0, so this is normally an
   UPDATE rather than an INSERT. Both paths are covered because a fresh tenant may have neither. */
INSERT INTO Core.RolePermission
    (Id, RoleId, OperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, TenantId, CreatedAt, RowVersion)
SELECT NEWID(), @role, o.Id, 1, 1, 1, 0, 0, @permTenant, SYSUTCDATETIME(), @rv
FROM Core.Operation o
WHERE o.Link IN (SELECT Link FROM @selfService)
  AND NOT EXISTS (SELECT 1 FROM Core.RolePermission rp WHERE rp.RoleId = @role AND rp.OperationId = o.Id);

UPDATE rp SET rp.CanView = 1, rp.CanAdd = 1, rp.CanEdit = 1
FROM Core.RolePermission rp
JOIN Core.Operation o ON o.Id = rp.OperationId
WHERE rp.RoleId = @role
  AND o.Link IN (SELECT Link FROM @selfService)
  AND rp.CanView = 0;

/* ---------------------------------------------------------------------------
   2. Assign the role to employee-linked accounts that have none.

   Accounts with NO EmployeeId are left alone on purpose: those are system /
   tenant-owner logins, and what they should hold is a separate decision.
   --------------------------------------------------------------------------- */
INSERT INTO Core.UserRole (Id, UserId, RoleId, TenantId, CreatedAt, RowVersion)
SELECT NEWID(), u.Id, @role, ISNULL(NULLIF(u.TenantId, ''), @userTenant), SYSUTCDATETIME(), @rv
FROM Core.[User] u
WHERE u.EmployeeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Core.UserRole ur WHERE ur.UserId = u.Id);

COMMIT;

/* ---- Result ---- */
SELECT
    (SELECT COUNT(DISTINCT ur.UserId) FROM Core.UserRole ur WHERE ur.RoleId = @role)          AS users_with_employee_role,
    (SELECT COUNT(*) FROM Core.[User] u
      WHERE u.EmployeeId IS NOT NULL
        AND NOT EXISTS (SELECT 1 FROM Core.UserRole ur WHERE ur.UserId = u.Id))               AS roleless_employees_remaining,
    (SELECT COUNT(*) FROM Core.[User] u
      WHERE u.EmployeeId IS NULL
        AND NOT EXISTS (SELECT 1 FROM Core.UserRole ur WHERE ur.UserId = u.Id))               AS roleless_system_accounts;
