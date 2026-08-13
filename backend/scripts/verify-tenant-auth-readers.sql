/*
  READER PARITY TEST for the SRMS phase-2 flip.

  seed-tenant-authorization-verify.sql compares the two MODELS. This one compares the two QUERIES —
  it transcribes, predicate for predicate, what the code actually ran before and after the flip:

    EndpointPermissionService.LoadGrantedLinksAsync   (the [RequirePermission] gate)
    GetModuleWithOperationsRepository.GetAsync        (the sidebar feed)

  The new versions carry two predicates the old ones had no way to express — TenantUser.Status and
  TenantOperation.IsActive. Those are the ONLY way this can differ while the model test still passes,
  so they are checked explicitly first rather than assumed.

  Read-only. Expected: every count zero, MATCH on both verdict lines.
*/

SET NOCOUNT ON;

/* ---- 1. The two new predicates must exclude nothing today ------------------- */
SELECT
    (SELECT COUNT(*) FROM Core.TenantUser      WHERE Status <> 'Active') AS memberships_not_active,
    (SELECT COUNT(*) FROM Core.TenantOperation WHERE IsActive = 0)       AS operations_hidden,
    (SELECT COUNT(*) FROM Core.UserRole ur
       WHERE NOT EXISTS (SELECT 1 FROM Core.TenantUser tu WHERE tu.UserId = ur.UserId))
        AS users_with_a_role_but_no_membership;   /* would lose ALL access after the flip */

/* ---- 2. The permission gate ------------------------------------------------- */
/* OLD: UserRole -> RolePermission(CanView) -> Operation(Link not null), normalised. */
WITH old_gate AS (
    SELECT DISTINCT ur.UserId, LOWER(LTRIM(REPLACE(o.Link, '/', ' '))) AS x, o.Link
    FROM Core.UserRole ur
    JOIN Core.RolePermission rp ON rp.RoleId = ur.RoleId AND rp.CanView = 1
    JOIN Core.Operation o       ON o.Id = rp.OperationId
    WHERE o.Link IS NOT NULL
),
/* NEW: TenantUser(Active) -> TenantUserRole -> TenantRolePermission(CanView)
        -> TenantOperation(IsActive, Link <> ''). */
new_gate AS (
    SELECT DISTINCT tu.UserId, LOWER(LTRIM(REPLACE(topx.Link, '/', ' '))) AS x, topx.Link
    FROM Core.TenantUser tu
    JOIN Core.TenantUserRole tur       ON tur.TenantUserId = tu.Id
    JOIN Core.TenantRolePermission trp ON trp.TenantRoleId = tur.TenantRoleId AND trp.CanView = 1
    JOIN Core.TenantOperation topx     ON topx.Id = trp.TenantOperationId
    WHERE tu.Status = 'Active' AND topx.IsActive = 1 AND topx.Link <> ''
)
SELECT
    (SELECT COUNT(*) FROM old_gate) AS old_gate_rows,
    (SELECT COUNT(*) FROM new_gate) AS new_gate_rows,
    (SELECT COUNT(*) FROM (SELECT UserId, Link FROM old_gate
                           EXCEPT SELECT UserId, Link FROM new_gate) d) AS gate_lost,
    (SELECT COUNT(*) FROM (SELECT UserId, Link FROM new_gate
                           EXCEPT SELECT UserId, Link FROM old_gate) d) AS gate_gained,
    CASE WHEN NOT EXISTS (SELECT UserId, Link FROM old_gate EXCEPT SELECT UserId, Link FROM new_gate)
          AND NOT EXISTS (SELECT UserId, Link FROM new_gate EXCEPT SELECT UserId, Link FROM old_gate)
         THEN 'MATCH - the permission gate grants the same links'
         ELSE '*** MISMATCH - the gate changed ***' END AS gate_verdict;

/* ---- 3. The sidebar feed ---------------------------------------------------- */
/* Both versions drop operations without CanView, then drop modules left empty. The new one reads
   the tenant's own copy of each operation, so Name/Link/Icon/order come from TenantOperation. */
WITH old_menu AS (
    SELECT DISTINCT ur.UserId, m.Id AS ModuleId, o.Id AS OperationId, o.Link, o.Name
    FROM Core.UserRole ur
    JOIN Core.RolePermission rp ON rp.RoleId = ur.RoleId AND rp.CanView = 1
    JOIN Core.Operation o       ON o.Id = rp.OperationId
    JOIN Core.Module m          ON m.Id = o.ModuleId
),
new_menu AS (
    SELECT DISTINCT tu.UserId, g.OperationId AS ModuleId, topx.OperationId, topx.Link, topx.Name
    FROM Core.TenantUser tu
    JOIN Core.TenantUserRole tur       ON tur.TenantUserId = tu.Id
    JOIN Core.TenantRolePermission trp ON trp.TenantRoleId = tur.TenantRoleId AND trp.CanView = 1
    JOIN Core.TenantOperation topx     ON topx.Id = trp.TenantOperationId AND topx.IsActive = 1
    -- The GROUP is the PARENT OPERATION now, not a Core.Module row (2026-08-13). Joining Core.Module
    -- would still pass, because a parent shares its module's Id, but only by relying on that
    -- invariant — a group created since would not be in Core.Module at all.
    JOIN Core.TenantOperation g        ON g.OperationId = topx.ModuleId AND g.ModuleId IS NULL
    WHERE tu.Status = 'Active'
)
SELECT
    (SELECT COUNT(*) FROM old_menu) AS old_menu_rows,
    (SELECT COUNT(*) FROM new_menu) AS new_menu_rows,
    (SELECT COUNT(*) FROM (SELECT * FROM old_menu EXCEPT SELECT * FROM new_menu) d) AS menu_lost,
    (SELECT COUNT(*) FROM (SELECT * FROM new_menu EXCEPT SELECT * FROM old_menu) d) AS menu_gained,
    CASE WHEN NOT EXISTS (SELECT * FROM old_menu EXCEPT SELECT * FROM new_menu)
          AND NOT EXISTS (SELECT * FROM new_menu EXCEPT SELECT * FROM old_menu)
         THEN 'MATCH - the sidebar shows the same entries'
         ELSE '*** MISMATCH - the menu changed ***' END AS menu_verdict;

/* ---- 4. Per-user link totals, the number a signed-in user would actually see -- */
WITH old_n AS (
    SELECT ur.UserId, COUNT(DISTINCT o.Id) n
    FROM Core.UserRole ur
    JOIN Core.RolePermission rp ON rp.RoleId = ur.RoleId AND rp.CanView = 1
    JOIN Core.Operation o ON o.Id = rp.OperationId
    JOIN Core.Module m ON m.Id = o.ModuleId
    GROUP BY ur.UserId
),
new_n AS (
    SELECT tu.UserId, COUNT(DISTINCT topx.OperationId) n
    FROM Core.TenantUser tu
    JOIN Core.TenantUserRole tur ON tur.TenantUserId = tu.Id
    JOIN Core.TenantRolePermission trp ON trp.TenantRoleId = tur.TenantRoleId AND trp.CanView = 1
    JOIN Core.TenantOperation topx ON topx.Id = trp.TenantOperationId AND topx.IsActive = 1
    JOIN Core.TenantOperation g ON g.OperationId = topx.ModuleId AND g.ModuleId IS NULL
    WHERE tu.Status = 'Active'
    GROUP BY tu.UserId
)
SELECT COUNT(*) AS users_whose_sidebar_size_differs
FROM old_n o FULL OUTER JOIN new_n n ON n.UserId = o.UserId
WHERE ISNULL(o.n, -1) <> ISNULL(n.n, -1);
