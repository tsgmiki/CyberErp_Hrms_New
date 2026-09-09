/*
 * Adds the Home portal's learner Learning entries: "Course Catalogue" and "My Learning".
 *
 * WHY
 *   All ten Learning operations are registered under the hrms/ namespace, so to see their own training an
 *   employee had to be given access to the HR admin console. These two put the learner half of the
 *   module in the portal where the rest of self-service already lives (logic §12.85).
 *
 * WHERE
 *   The SSMS ("Self Service") subsystem's "My Requests" module — beside /myPeerReviews,
 *   /myEvaluations, /myTrips. Home's sidebar is built from these rows.
 *
 * ⚠️ LINKS ARE BARE, NOT NAMESPACED, like every other SSMS operation (/myProfile, /myEvaluations).
 *   That is the convention, and it is load-bearing: the HRMS permission gate strips only its OWN
 *   hrms/ prefix, so a bare link is what lets a Home screen authorise against the HRMS API
 *   (logic §12.69).
 *
 * ⚠️ THESE MENU ROWS GRANT NOTHING BY THEMSELVES. Both screens call HRMS endpoints gated on
 *   `myTraining`, which staff already hold, and the catalogue is served by a separate READ-ONLY
 *   controller precisely so that permission cannot become course creation. The rows below control
 *   whether the link is visible, never what the data allows.
 *
 * GRANTS — mirrors /myEvaluations exactly, which in turn mirrors /myPeerReviews: every role that
 *   sees the self-service menu sees these too. Appropriate because everyone is a learner, unlike an
 *   examiner or an approver.
 *
 * Idempotent: guarded by NOT EXISTS on both the operations and each grant.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by nvarchar(200) = N'add-learning-portal-menu.sql';
DECLARE @rv varbinary(8)  = 0x0000000000000001;

-- Anchor to the module that already owns the other self-service screens, so these cannot land in
-- the wrong subsystem's sidebar.
DECLARE @module uniqueidentifier =
    (SELECT TOP 1 o.ModuleId FROM Core.TenantOperation o WHERE o.Link = N'/myEvaluations');

IF @module IS NULL
BEGIN
    RAISERROR(N'ABORTED: could not locate the self-service module via /myEvaluations. Check the operation catalogue before running.', 16, 1);
    RETURN;
END

-- NOTE: Core.TenantOperation and Core.TenantRolePermission carry NO TenantId column — the operation
-- catalogue is SHARED across tenants; scoping happens through TenantRole.
DECLARE @new TABLE (Link nvarchar(200), Name nvarchar(200), Icon nvarchar(100), DisplayOrder int);
INSERT INTO @new (Link, Name, Icon, DisplayOrder) VALUES
    (N'/courseCatalog', N'Course Catalogue', N'BookOpen',      102),
    (N'/myLearning',    N'My Learning',      N'GraduationCap', 103);

SELECT 'BEFORE' AS Stage, n.Link,
       CASE WHEN EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = n.Link)
            THEN 'already present' ELSE 'to add' END AS State
FROM @new n;

BEGIN TRAN;

INSERT INTO Core.TenantOperation
    (Id, ModuleId, Name, Link, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), @module, n.Name, n.Link, n.Icon, n.DisplayOrder, 1, SYSUTCDATETIME(), @by, @rv
FROM @new n
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = n.Link);

-- Same audience and privileges as /myEvaluations, read from that row rather than hardcoded, so the
-- self-service menu stays internally consistent if the client re-scopes it.
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), src.TenantRoleId, tgt.Id, src.CanView, src.CanAdd, src.CanEdit, 0, 0, 0,
       SYSUTCDATETIME(), @by, @rv
FROM Core.TenantRolePermission src
JOIN Core.TenantOperation srcOp ON srcOp.Id = src.TenantOperationId AND srcOp.Link = N'/myEvaluations'
JOIN Core.TenantOperation tgt ON tgt.Link IN (N'/courseCatalog', N'/myLearning')
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantOperationId = tgt.Id AND x.TenantRoleId = src.TenantRoleId);

COMMIT;

SELECT 'AFTER: operations' AS Stage, o.Link, o.Name, o.Icon, o.DisplayOrder, o.IsActive
FROM Core.TenantOperation o WHERE o.Link IN (N'/courseCatalog', N'/myLearning')
ORDER BY o.DisplayOrder;

SELECT 'AFTER: who can see them' AS Stage, o.Link, tr.Name AS RoleName, rp.CanView, rp.CanAdd
FROM Core.TenantRolePermission rp
JOIN Core.TenantOperation o ON o.Id = rp.TenantOperationId
JOIN Core.TenantRole tr ON tr.Id = rp.TenantRoleId
WHERE o.Link IN (N'/courseCatalog', N'/myLearning')
ORDER BY o.Link, tr.Name;
