/*
  Populate the tenant-scoped authorization model FROM CERP's existing data.

  WHY FROM CERP AND NOT FROM SRMS
  -------------------------------
  cybererp_srms is a different product: its 326 operations share ZERO links with CERP's 150, and its
  data is an empty template (1 role, 3 users, 6 permissions). Seeding from it would hand every user a
  permission set matching none of our screens. CERP's 8 roles, 150 operations, 598 permissions and
  503 user-role rows are the real ones, so they are the source (logic.md section 12.2).

  MAPPING
  -------
    Role           -> TenantRole            (SourceTemplateId = the template's Id)
    Operation      -> TenantOperation       (a per-tenant copy, carrying Name/Link/Icon/DisplayOrder)
    RolePermission -> TenantRolePermission  (CanExport seeded false: the old model has no such column,
                                             so granting it here would invent access nobody assigned)
    User + UserRole-> TenantUser + TenantUserRole
    Subsystem      -> TenantSubSystem       (every tenant keeps the access it has today)

  Idempotent: every insert is guarded by NOT EXISTS, so re-running adds only what is missing.
  Writes ONLY to the six new tables — the live model is untouched, so this changes no behaviour.

  ACCEPTANCE TEST (run seed-tenant-authorization-verify.sql afterwards): every user's effective
  (link, CanView) set must be IDENTICAL in the old and new models.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rv varbinary(8) = 0x0000000000000001;

BEGIN TRAN;

/* ---- 1. Roles ---------------------------------------------------------------
   The existing model is ALREADY tenant-scoped through the TenantId discriminator
   (3 roles belong to demo, 5 to headoffice; all 150 operations and 598 permissions
   to headoffice). So the mirror is 1:1 — joining each row to ITS OWN tenant.

   ⚠ Do NOT cross join Role x Tenant here. That replicates every role into every
   tenant, and the membership insert downstream then makes each user a member of
   ALL tenants: 506 users became 1500 memberships on the first attempt. */
INSERT INTO Core.TenantRole
    (Id, OwningTenantId, SourceTemplateId, Code, Name, Description, IsCustomized, TenantId, CreatedAt, RowVersion)
SELECT NEWID(), t.Id, r.Id, LEFT(r.Name, 100), LEFT(r.Name, 200), NULL, 0,
       CAST(t.Id AS nvarchar(64)), SYSUTCDATETIME(), @rv
FROM Core.Role r
JOIN Core.Tenant t ON CAST(t.Id AS nvarchar(64)) = r.TenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRole tr WHERE tr.OwningTenantId = t.Id AND tr.SourceTemplateId = r.Id);

/* ---- 2. Operations ----------------------------------------------------------
   A per-tenant COPY: the tenant may later rename or hide a screen without
   affecting anyone else. Link is what the permission check matches on. */
INSERT INTO Core.TenantOperation
    (Id, OwningTenantId, SubSystemId, OperationId, ModuleId, Name, Link, Icon, DisplayOrder, IsActive,
     Filter, TenantId, CreatedAt, RowVersion)
-- Since the 2026-08 SRMS alignment the template carries DisplayOrder, IsActive and its own
-- SubSystemId, so all three copy straight across instead of being derived or defaulted.
SELECT NEWID(), t.Id, ISNULL(NULLIF(o.SubSystemId, '00000000-0000-0000-0000-000000000000'),
                             ISNULL(m.SubsystemId, '00000000-0000-0000-0000-000000000000')),
       o.Id, o.ModuleId,
       LEFT(ISNULL(o.Name, ''), 200), LEFT(ISNULL(o.Link, ''), 300), LEFT(ISNULL(o.Icon, ''), 100),
       ISNULL(o.DisplayOrder, 0), o.IsActive, LEFT(ISNULL(o.Filter, ''), 500),
       CAST(t.Id AS nvarchar(64)), SYSUTCDATETIME(), @rv
FROM Core.Operation o
LEFT JOIN Core.Module m ON m.Id = o.ModuleId
JOIN Core.Tenant t ON CAST(t.Id AS nvarchar(64)) = o.TenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantOperation topx WHERE topx.OwningTenantId = t.Id AND topx.OperationId = o.Id);

/* ---- 3. Permissions ---------------------------------------------------------
   Joined through the template ids, so each tenant's permissions land on that
   tenant's own role and operation rows. CanExport starts false everywhere. */
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     TenantId, CreatedAt, RowVersion)
SELECT NEWID(), tr.Id, topx.Id, rp.CanView, rp.CanAdd, rp.CanEdit, rp.CanDelete, rp.CanApprove, 0,
       tr.TenantId, SYSUTCDATETIME(), @rv
FROM Core.RolePermission rp
JOIN Core.TenantRole tr      ON tr.SourceTemplateId = rp.RoleId
JOIN Core.TenantOperation topx ON topx.OperationId  = rp.OperationId
                              AND topx.OwningTenantId = tr.OwningTenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantRoleId = tr.Id AND x.TenantOperationId = topx.Id);

/* ---- 4. Memberships ---------------------------------------------------------
   One row per (tenant, user) for users that actually hold a role in it, so the
   membership table reflects real access rather than a cross join of everybody. */
-- The distinct (tenant, user) pairs are resolved in a subquery FIRST. Selecting
-- `DISTINCT NEWID(), …` would not dedupe at all — NEWID() makes every row unique, so a user
-- holding two roles in one tenant would produce two membership rows and hit the unique index.
INSERT INTO Core.TenantUser
    (Id, OwningTenantId, UserId, Status, IsDefaultTenant, TenantId, CreatedAt, RowVersion)
SELECT NEWID(), m.OwningTenantId, m.UserId, 'Active', 1, m.TenantId, SYSUTCDATETIME(), @rv
FROM (
    SELECT DISTINCT tr.OwningTenantId, ur.UserId, tr.TenantId
    FROM Core.UserRole ur
    JOIN Core.TenantRole tr ON tr.SourceTemplateId = ur.RoleId
) m
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantUser tu WHERE tu.OwningTenantId = m.OwningTenantId AND tu.UserId = m.UserId);

INSERT INTO Core.TenantUserRole
    (Id, TenantUserId, TenantRoleId, AssignedAt, AssignedBy, TenantId, CreatedAt, RowVersion)
SELECT NEWID(), tu.Id, tr.Id, SYSUTCDATETIME(), 'seed-tenant-authorization',
       tu.TenantId, SYSUTCDATETIME(), @rv
FROM Core.UserRole ur
JOIN Core.TenantRole tr ON tr.SourceTemplateId = ur.RoleId
JOIN Core.TenantUser tu ON tu.UserId = ur.UserId AND tu.OwningTenantId = tr.OwningTenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantUserRole x WHERE x.TenantUserId = tu.Id AND x.TenantRoleId = tr.Id);

/* ---- 5. Subsystem licensing -------------------------------------------------
   Every tenant keeps exactly the subsystems it can reach today; the term fields
   exist so a trial or lapse can be expressed later, not to restrict anything now. */
INSERT INTO Core.TenantSubSystem
    (Id, OwningTenantId, SubSystemId, SourceType, Status, StartDate, EndDate, TrialEndDate,
     TenantId, CreatedAt, RowVersion)
SELECT NEWID(), t.Id, ss.Id, 'Plan', 'Active', CAST(SYSUTCDATETIME() AS date), NULL, NULL,
       CAST(t.Id AS nvarchar(64)), SYSUTCDATETIME(), @rv
FROM Core.Subsystem ss
JOIN Core.Tenant t ON CAST(t.Id AS nvarchar(64)) = ss.TenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantSubSystem ts WHERE ts.OwningTenantId = t.Id AND ts.SubSystemId = ss.Id);

COMMIT;

/* ---- Result ---- */
SELECT
    (SELECT COUNT(*) FROM Core.TenantRole)           AS tenant_roles,
    (SELECT COUNT(*) FROM Core.TenantOperation)      AS tenant_operations,
    (SELECT COUNT(*) FROM Core.TenantRolePermission) AS tenant_permissions,
    (SELECT COUNT(*) FROM Core.TenantUser)           AS tenant_users,
    (SELECT COUNT(*) FROM Core.TenantUserRole)       AS tenant_user_roles,
    (SELECT COUNT(*) FROM Core.TenantSubSystem)      AS tenant_subsystems;
