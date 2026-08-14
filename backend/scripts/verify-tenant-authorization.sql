/*
  INTEGRITY CHECK for the tenant-scoped authorization model.

  Replaces seed-tenant-authorization-verify.sql and verify-tenant-auth-readers.sql, both of which
  compared the tenant tables against Core.RolePermission. That table was RETIRED on 2026-08-13
  (logic.md §12.7), so there is nothing left to compare against — those scripts could only fail with
  "invalid object name" and have been removed rather than left to mislead.

  What is checkable now is the model's own consistency: every grant reaches a live role and a live
  operation, every membership reaches a live user, and nobody has been left holding nothing.

  Read-only. Expected: every count zero except the last block, which is informational.
*/

SET NOCOUNT ON;

PRINT '--- 1. Dangling references (each must be 0) ---';
SELECT
    (SELECT COUNT(*) FROM Core.TenantRolePermission p
       WHERE NOT EXISTS (SELECT 1 FROM Core.TenantRole r WHERE r.Id = p.TenantRoleId))
        AS grants_without_a_role,
    (SELECT COUNT(*) FROM Core.TenantRolePermission p
       WHERE NOT EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Id = p.TenantOperationId))
        AS grants_without_an_operation,
    (SELECT COUNT(*) FROM Core.TenantUserRole tur
       WHERE NOT EXISTS (SELECT 1 FROM Core.TenantUser tu WHERE tu.Id = tur.TenantUserId))
        AS roles_held_by_nobody,
    (SELECT COUNT(*) FROM Core.TenantUser tu
       WHERE NOT EXISTS (SELECT 1 FROM Core.[User] u WHERE u.Id = tu.UserId))
        AS memberships_without_a_user;

PRINT '--- 2. Cross-tenant leakage (each must be 0) ---';
/* A grant must join a role and an operation belonging to the SAME tenant. Nothing enforces this at
   the database level — the two foreign keys are independent — so it is worth asserting. */
SELECT
    (SELECT COUNT(*) FROM Core.TenantRolePermission p
     JOIN Core.TenantRole r      ON r.Id = p.TenantRoleId
     JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId
     WHERE r.TenantId <> o.TenantId)
        AS grants_spanning_two_tenants,
    (SELECT COUNT(*) FROM Core.TenantUserRole tur
     JOIN Core.TenantUser tu ON tu.Id = tur.TenantUserId
     JOIN Core.TenantRole r  ON r.Id = tur.TenantRoleId
     WHERE tu.TenantId <> r.TenantId)
        AS assignments_spanning_two_tenants;

PRINT '--- 3. The menu tree (each must be 0) ---';
SELECT
    (SELECT COUNT(*) FROM Core.TenantOperation c
       WHERE c.ModuleId IS NOT NULL
         AND NOT EXISTS (SELECT 1 FROM Core.TenantOperation p
                         WHERE p.OperationId = c.ModuleId AND p.ModuleId IS NULL
                           AND p.TenantId = c.TenantId))
        AS screens_whose_group_is_missing,   /* they would vanish from the sidebar */
    (SELECT COUNT(*) FROM Core.TenantOperation WHERE ModuleId IS NULL AND Link <> '')
        AS groups_carrying_a_link;           /* a group must grant nothing */

PRINT '--- 4. Reachability, informational ---';
SELECT
    (SELECT COUNT(*) FROM Core.TenantUser)                                       AS memberships,
    (SELECT COUNT(*) FROM Core.TenantUser tu
       WHERE NOT EXISTS (SELECT 1 FROM Core.TenantUserRole r WHERE r.TenantUserId = tu.Id))
                                                                                 AS memberships_with_no_role,
    (SELECT COUNT(*) FROM Core.TenantRolePermission)                             AS grants,
    (SELECT COUNT(*) FROM Core.TenantOperation WHERE ModuleId IS NULL)           AS menu_groups,
    (SELECT COUNT(*) FROM Core.TenantOperation WHERE ModuleId IS NOT NULL)       AS screens,
    (SELECT COUNT(*) FROM (
        SELECT DISTINCT tu.UserId, o.Link
        FROM Core.TenantUser tu
        JOIN Core.TenantUserRole tur       ON tur.TenantUserId = tu.Id
        JOIN Core.TenantRolePermission p   ON p.TenantRoleId = tur.TenantRoleId AND p.CanView = 1
        JOIN Core.TenantOperation o        ON o.Id = p.TenantOperationId AND o.IsActive = 1
        WHERE tu.Status = 'Active' AND o.Link <> '') x)                          AS viewable_user_link_pairs;
