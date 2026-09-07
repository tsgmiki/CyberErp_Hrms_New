/*
 * Adds the Home portal's "My Evaluations" menu entry and grants it, so an assigned examiner can
 * reach their own screen.
 *
 * WHY
 *   An assigned criterion evaluator is usually an ordinary employee who holds none of the
 *   recruitment operations, so every recruitment screen answered 403 and a non-HR examiner could
 *   not submit results at all (logic §12.78). The API side is solved by MyEvaluationController,
 *   which is not gated on those operations and authorises on evaluator standing instead. This adds
 *   the SIDEBAR entry that takes them there.
 *
 * WHERE
 *   The SSMS ("Self Service") subsystem's "My Requests" module — the same module that holds
 *   /myPeerReviews, /myTrips, /myLoans. Home's sidebar is built from these rows.
 *
 * ⚠️ LINK IS BARE, NOT NAMESPACED. Every SSMS operation is stored without a prefix
 *   (/myPeerReviews, /myProfile, /transferRequest) while HRMS's carry /hrms/. That is the
 *   convention, not an oversight — and the HRMS permission gate strips only its OWN namespace, so a
 *   bare link is what lets a Home screen authorise against the HRMS API (logic §12.69).
 *
 * GRANTS — mirrors /myPeerReviews exactly: UserRole, Department Manager and HR Admin, View+Add+Edit.
 *   Granting UserRole means every employee sees the item; that is deliberate and matches the other
 *   self-service screens, because who is an examiner changes per vacancy and cannot be a role. The
 *   screen shows an explicit empty state ("You have no applicants to evaluate right now") for anyone
 *   with no assignment, and the SERVER returns an empty list to them regardless of this grant — the
 *   menu row controls visibility of the link, never access to the data.
 *
 * Idempotent: guarded by NOT EXISTS on both the operation and each grant.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by   nvarchar(200)    = N'add-myevaluations-menu.sql';
DECLARE @rv   varbinary(8)     = 0x0000000000000001;
DECLARE @link nvarchar(200)    = N'/myEvaluations';

-- Anchor to the module that already owns the other self-service screens, so the entry cannot land
-- in the wrong subsystem's sidebar.
DECLARE @module uniqueidentifier =
    (SELECT TOP 1 o.ModuleId FROM Core.TenantOperation o WHERE o.Link = N'/myPeerReviews');

IF @module IS NULL
BEGIN
    RAISERROR(N'ABORTED: could not locate the "My Requests" module via /myPeerReviews. Check the operation catalogue before running.', 16, 1);
    RETURN;
END

-- NOTE: Core.TenantOperation and Core.TenantRolePermission carry NO TenantId column. The operation
-- catalogue is SHARED across tenants (the EF model maps and then ignores that property, which the
-- build even warns about); tenant scoping happens through TenantRole, not through these rows.
SELECT 'BEFORE' AS Stage, COUNT(*) AS ExistingRows
FROM Core.TenantOperation WHERE Link = @link;

BEGIN TRAN;

INSERT INTO Core.TenantOperation
    (Id, ModuleId, Name, Link, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), @module, N'My Evaluations', @link, N'ClipboardCheck', 101, 1,
       SYSUTCDATETIME(), @by, @rv
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantOperation WHERE Link = @link);

-- Same audience and privileges as /myPeerReviews, resolved from that row rather than hardcoded, so
-- the two stay in step if the client re-scopes their self-service menu.
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), src.TenantRoleId, tgt.Id, src.CanView, src.CanAdd, src.CanEdit, 0, 0, 0,
       SYSUTCDATETIME(), @by, @rv
FROM Core.TenantRolePermission src
JOIN Core.TenantOperation srcOp ON srcOp.Id = src.TenantOperationId AND srcOp.Link = N'/myPeerReviews'
CROSS JOIN (SELECT TOP 1 Id FROM Core.TenantOperation WHERE Link = @link) tgt
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantOperationId = tgt.Id AND x.TenantRoleId = src.TenantRoleId);

COMMIT;

SELECT 'AFTER: the operation' AS Stage, o.Link, o.Name, o.Icon, o.DisplayOrder, o.IsActive
FROM Core.TenantOperation o WHERE o.Link = @link;

SELECT 'AFTER: who can see it' AS Stage, tr.Name AS RoleName, rp.CanView, rp.CanAdd, rp.CanEdit
FROM Core.TenantRolePermission rp
JOIN Core.TenantOperation o ON o.Id = rp.TenantOperationId
JOIN Core.TenantRole tr ON tr.Id = rp.TenantRoleId
WHERE o.Link = @link
ORDER BY tr.Name;
