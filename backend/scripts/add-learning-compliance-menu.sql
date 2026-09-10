/*
 * Adds the mandatory-training screens: "Mandatory Training" (HRMS admin) and "Required of Me"
 * (Home self-service) — logic 12.88.
 *
 * WHY
 *   Phase 5 turns training into compliance: a rule assigns a course to a population, materialises a
 *   dated obligation per person, chases it, and reports on it. HR needs somewhere to write the rules
 *   and read the dashboard; the learner needs to see what is required of them and by when.
 *
 * TWO OPERATIONS, TWO NAMESPACES, and the split is deliberate:
 *
 *   /hrms/learningCompliance  — the admin screen. NAMESPACED, like every other HRMS admin operation.
 *                               Granted to the roles that already hold the HR employee register,
 *                               NOT to the course authors: assigning mandatory training to a
 *                               population and waiving someone's obligation are compliance acts, and
 *                               whoever writes a course is not automatically the person who should
 *                               be doing them.
 *
 *   /myObligations            — the learner screen in Home. A BARE link, like every other SSMS
 *                               operation. That is load-bearing: the HRMS permission gate strips
 *                               only its OWN hrms/ prefix, so a bare link is what lets a Home screen
 *                               authorise against the HRMS API (logic 12.69). It calls an endpoint
 *                               gated on `myTraining`, which staff already hold, and that endpoint
 *                               answers only for the signed-in employee.
 *
 * NOTE: neither row grants anything by itself. They control whether a link is visible, never what
 *   the data allows.
 *
 * Idempotent: guarded by NOT EXISTS on both operations and on every grant.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by nvarchar(200) = N'add-learning-compliance-menu.sql';
DECLARE @rv varbinary(8)  = 0x0000000000000001;

DECLARE @adminLink   nvarchar(200) = N'/hrms/learningCompliance';
DECLARE @learnerLink nvarchar(200) = N'/myObligations';

-- Anchored to existing rows so neither screen can land in the wrong subsystem's sidebar.
DECLARE @learningModule uniqueidentifier =
    (SELECT TOP 1 o.ModuleId FROM Core.TenantOperation o WHERE o.Link = N'/hrms/trainingCourse');
DECLARE @selfServiceModule uniqueidentifier =
    (SELECT TOP 1 o.ModuleId FROM Core.TenantOperation o WHERE o.Link = N'/myLearning');

IF @learningModule IS NULL OR @selfServiceModule IS NULL
BEGIN
    RAISERROR(N'ABORTED: could not locate the Learning module (via the course catalogue) or the self-service module (via My Learning). Check the operation catalogue before running.', 16, 1);
    RETURN;
END

SELECT 'BEFORE' AS Stage, l.Link,
       CASE WHEN EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = l.Link)
            THEN 'already present' ELSE 'to add' END AS State
FROM (SELECT @adminLink AS Link UNION ALL SELECT @learnerLink) l;

BEGIN TRAN;

-- NOTE: Core.TenantOperation and Core.TenantRolePermission carry NO TenantId column — the operation
-- catalogue is SHARED across tenants; scoping happens through TenantRole.
INSERT INTO Core.TenantOperation
    (Id, ModuleId, Name, Link, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), @learningModule, N'Mandatory Training', @adminLink, N'ShieldCheck', 52, 1,
       SYSUTCDATETIME(), @by, @rv
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = @adminLink);

INSERT INTO Core.TenantOperation
    (Id, ModuleId, Name, Link, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), @selfServiceModule, N'Required of Me', @learnerLink, N'ShieldAlert', 104, 1,
       SYSUTCDATETIME(), @by, @rv
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = @learnerLink);

-- The admin screen mirrors the HR employee register: whoever administers people administers what
-- those people are required to be trained on. Read from that row rather than hardcoded, so the two
-- stay consistent if the client re-scopes HR.
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), src.TenantRoleId, tgt.Id, src.CanView, src.CanAdd, src.CanEdit, src.CanDelete, 0, src.CanExport,
       SYSUTCDATETIME(), @by, @rv
FROM Core.TenantRolePermission src
JOIN Core.TenantOperation srcOp ON srcOp.Id = src.TenantOperationId AND srcOp.Link = N'/hrms/employee'
JOIN Core.TenantOperation tgt ON tgt.Link = @adminLink
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantOperationId = tgt.Id AND x.TenantRoleId = src.TenantRoleId);

-- The learner screen mirrors My Learning exactly: everyone is a learner, so everyone who sees their
-- own training sees what is required of them.
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), src.TenantRoleId, tgt.Id, src.CanView, 0, 0, 0, 0, 0,
       SYSUTCDATETIME(), @by, @rv
FROM Core.TenantRolePermission src
JOIN Core.TenantOperation srcOp ON srcOp.Id = src.TenantOperationId AND srcOp.Link = N'/myLearning'
JOIN Core.TenantOperation tgt ON tgt.Link = @learnerLink
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantOperationId = tgt.Id AND x.TenantRoleId = src.TenantRoleId);

COMMIT;

SELECT 'AFTER: operations' AS Stage, o.Link, o.Name, o.Icon, o.DisplayOrder, o.IsActive
FROM Core.TenantOperation o WHERE o.Link IN (@adminLink, @learnerLink)
ORDER BY o.Link;

SELECT 'AFTER: who can see them' AS Stage, o.Link, tr.Name AS RoleName,
       rp.CanView, rp.CanAdd, rp.CanEdit, rp.CanDelete
FROM Core.TenantRolePermission rp
JOIN Core.TenantOperation o ON o.Id = rp.TenantOperationId
JOIN Core.TenantRole tr ON tr.Id = rp.TenantRoleId
WHERE o.Link IN (@adminLink, @learnerLink)
ORDER BY o.Link, tr.Name;
