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
    (Id, SourceTemplateId, Code, Name, Description, IsCustomized, TenantId, CreatedAt, RowVersion)
SELECT NEWID(), r.Id, LEFT(r.Name, 100), LEFT(r.Name, 200), NULL, 0,
       t.Id, SYSUTCDATETIME(), @rv
FROM Core.Role r
JOIN Core.Tenant t ON t.Id = r.TenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRole tr WHERE tr.TenantId = t.Id AND tr.SourceTemplateId = r.Id);

/* ---- 2. Operations ----------------------------------------------------------
   A per-tenant COPY: the tenant may later rename or hide a screen without
   affecting anyone else. Link is what the permission check matches on. */
INSERT INTO Core.TenantOperation
    (Id, SubSystemId, OperationId, ModuleId, Name, Link, Icon, DisplayOrder, IsActive,
     Filter, TenantId, CreatedAt, RowVersion)
-- Since the 2026-08 SRMS alignment the template carries DisplayOrder, IsActive and its own
-- SubSystemId, so all three copy straight across instead of being derived or defaulted.
SELECT NEWID(), t.Id, ISNULL(NULLIF(o.SubSystemId, '00000000-0000-0000-0000-000000000000'),
                             ISNULL(m.SubsystemId, '00000000-0000-0000-0000-000000000000')),
       o.Id, o.ModuleId,
       LEFT(ISNULL(o.Name, ''), 200), LEFT(ISNULL(o.Link, ''), 300), LEFT(ISNULL(o.Icon, ''), 100),
       ISNULL(o.DisplayOrder, 0), o.IsActive, LEFT(ISNULL(o.Filter, ''), 500),
       t.Id, SYSUTCDATETIME(), @rv
FROM Core.Operation o
LEFT JOIN Core.Module m ON m.Id = o.ModuleId
JOIN Core.Tenant t ON t.Id = o.TenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantOperation topx WHERE topx.TenantId = t.Id AND topx.OperationId = o.Id);

/* ---- 3. Permissions ----------------------------------------------------------
   ⚠️ NOTHING TO DO. Core.RolePermission was RETIRED on 2026-08-13 (logic.md §12.7):
   Core.TenantRolePermission is now the ONLY grant table, written directly by the
   Role Permissions screen. There is no longer a source to seed it from, and this
   script must never delete or overwrite what is there — it holds the real data. */

/* ---- 4. Memberships ---------------------------------------------------------
   One row per (tenant, user) for users that actually hold a role in it, so the
   membership table reflects real access rather than a cross join of everybody. */
-- The distinct (tenant, user) pairs are resolved in a subquery FIRST. Selecting
-- `DISTINCT NEWID(), …` would not dedupe at all — NEWID() makes every row unique, so a user
-- holding two roles in one tenant would produce two membership rows and hit the unique index.
INSERT INTO Core.TenantUser
    (Id, UserId, Status, IsDefaultTenant, TenantId, CreatedAt, RowVersion)
-- Status is a bit since 2026-08-19: 1 = an active membership.
SELECT NEWID(), m.UserId, 1, 1, m.TenantId, SYSUTCDATETIME(), @rv
FROM (
    SELECT DISTINCT ur.UserId, tr.TenantId
    FROM Core.UserRole ur
    JOIN Core.TenantRole tr ON tr.SourceTemplateId = ur.RoleId
) m
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantUser tu WHERE tu.TenantId = m.TenantId AND tu.UserId = m.UserId);

INSERT INTO Core.TenantUserRole
    (Id, TenantUserId, TenantRoleId, AssignedAt, AssignedBy, TenantId, CreatedAt, RowVersion)
SELECT NEWID(), tu.Id, tr.Id, SYSUTCDATETIME(), 'seed-tenant-authorization',
       tu.TenantId, SYSUTCDATETIME(), @rv
FROM Core.UserRole ur
JOIN Core.TenantRole tr ON tr.SourceTemplateId = ur.RoleId
JOIN Core.TenantUser tu ON tu.UserId = ur.UserId AND tu.TenantId = tr.TenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantUserRole x WHERE x.TenantUserId = tu.Id AND x.TenantRoleId = tr.Id);

/* ---- 5. Subsystem licensing -------------------------------------------------
   Every tenant keeps exactly the subsystems it can reach today; the term fields
   exist so a trial or lapse can be expressed later, not to restrict anything now. */
INSERT INTO Core.TenantSubSystem
    (Id, SubSystemId, SourceType, Status, StartDate, EndDate, TrialEndDate,
     TenantId, CreatedAt, RowVersion)
-- Status is a bit since 2026-08-19: 1 = the entitlement is live.
SELECT NEWID(), ss.Id, 'Plan', 1, CAST(SYSUTCDATETIME() AS date), NULL, NULL,
       t.Id, SYSUTCDATETIME(), @rv
FROM Core.Subsystem ss
JOIN Core.Tenant t ON t.Id = ss.TenantId
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantSubSystem ts WHERE ts.TenantId = t.Id AND ts.SubSystemId = ss.Id);

COMMIT;

/* ---- Result ---- */
SELECT
    (SELECT COUNT(*) FROM Core.TenantRole)           AS tenant_roles,
    (SELECT COUNT(*) FROM Core.TenantOperation)      AS tenant_operations,
    (SELECT COUNT(*) FROM Core.TenantRolePermission) AS tenant_permissions,
    (SELECT COUNT(*) FROM Core.TenantUser)           AS tenant_users,
    (SELECT COUNT(*) FROM Core.TenantUserRole)       AS tenant_user_roles,
    (SELECT COUNT(*) FROM Core.TenantSubSystem)      AS tenant_subsystems;
