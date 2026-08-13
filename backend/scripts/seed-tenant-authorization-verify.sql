/*
  ACCEPTANCE TEST for seed-tenant-authorization.sql.

  Compares EFFECTIVE PERMISSIONS between the live model and the mirrored tenant-scoped one, per user.
  Row counts alone prove nothing — what matters is that no user gains or loses a single grant.

  A user's effective set is the DISTINCT (Link, CanView, CanAdd, CanEdit, CanDelete, CanApprove) they
  reach through any of their roles. It is compared with a full outer join, so a grant present on one
  side and missing on the other shows up whichever direction it goes.

  Read-only. Expected result: every count zero, and MATCH on the verdict line.
*/

SET NOCOUNT ON;

/* The live model: User -> UserRole -> RolePermission -> Operation.Link */
WITH old_eff AS (
    SELECT DISTINCT
        ur.UserId, o.Link,
        CAST(rp.CanView AS int) AS CanView, CAST(rp.CanAdd AS int) AS CanAdd,
        CAST(rp.CanEdit AS int) AS CanEdit, CAST(rp.CanDelete AS int) AS CanDelete,
        CAST(rp.CanApprove AS int) AS CanApprove
    FROM Core.UserRole ur
    JOIN Core.RolePermission rp ON rp.RoleId = ur.RoleId
    JOIN Core.Operation o       ON o.Id = rp.OperationId
    WHERE o.Link IS NOT NULL
),
/* The mirrored model: TenantUser -> TenantUserRole -> TenantRolePermission -> TenantOperation.Link */
new_eff AS (
    SELECT DISTINCT
        tu.UserId, topx.Link,
        CAST(trp.CanView AS int) AS CanView, CAST(trp.CanAdd AS int) AS CanAdd,
        CAST(trp.CanEdit AS int) AS CanEdit, CAST(trp.CanDelete AS int) AS CanDelete,
        CAST(trp.CanApprove AS int) AS CanApprove
    FROM Core.TenantUser tu
    JOIN Core.TenantUserRole tur      ON tur.TenantUserId = tu.Id
    JOIN Core.TenantRolePermission trp ON trp.TenantRoleId = tur.TenantRoleId
    JOIN Core.TenantOperation topx     ON topx.Id = trp.TenantOperationId
    WHERE topx.Link IS NOT NULL
)
SELECT
    (SELECT COUNT(*) FROM old_eff) AS old_grant_rows,
    (SELECT COUNT(*) FROM new_eff) AS new_grant_rows,
    (SELECT COUNT(*) FROM old_eff o
       WHERE NOT EXISTS (SELECT 1 FROM new_eff n
                         WHERE n.UserId = o.UserId AND n.Link = o.Link
                           AND n.CanView = o.CanView AND n.CanAdd = o.CanAdd AND n.CanEdit = o.CanEdit
                           AND n.CanDelete = o.CanDelete AND n.CanApprove = o.CanApprove))
        AS lost_in_new,      /* a grant a user has TODAY that the new model would not give them */
    (SELECT COUNT(*) FROM new_eff n
       WHERE NOT EXISTS (SELECT 1 FROM old_eff o
                         WHERE o.UserId = n.UserId AND o.Link = n.Link
                           AND o.CanView = n.CanView AND o.CanAdd = n.CanAdd AND o.CanEdit = n.CanEdit
                           AND o.CanDelete = n.CanDelete AND o.CanApprove = n.CanApprove))
        AS gained_in_new;    /* a grant the new model would ADD — just as serious */

/* Per-user CanView totals, the number the sidebar and the permission filter actually use. */
WITH old_view AS (
    SELECT ur.UserId, COUNT(DISTINCT o.Link) AS Links
    FROM Core.UserRole ur
    JOIN Core.RolePermission rp ON rp.RoleId = ur.RoleId AND rp.CanView = 1
    JOIN Core.Operation o       ON o.Id = rp.OperationId
    GROUP BY ur.UserId
),
new_view AS (
    SELECT tu.UserId, COUNT(DISTINCT topx.Link) AS Links
    FROM Core.TenantUser tu
    JOIN Core.TenantUserRole tur       ON tur.TenantUserId = tu.Id
    JOIN Core.TenantRolePermission trp ON trp.TenantRoleId = tur.TenantRoleId AND trp.CanView = 1
    JOIN Core.TenantOperation topx     ON topx.Id = trp.TenantOperationId
    GROUP BY tu.UserId
)
SELECT COUNT(*) AS users_whose_viewable_link_count_differs
FROM old_view ov
FULL OUTER JOIN new_view nv ON nv.UserId = ov.UserId
WHERE ISNULL(ov.Links, -1) <> ISNULL(nv.Links, -1);

/* Verdict. */
WITH old_eff AS (
    SELECT DISTINCT ur.UserId, o.Link, CAST(rp.CanView AS int) CanView, CAST(rp.CanAdd AS int) CanAdd,
           CAST(rp.CanEdit AS int) CanEdit, CAST(rp.CanDelete AS int) CanDelete, CAST(rp.CanApprove AS int) CanApprove
    FROM Core.UserRole ur
    JOIN Core.RolePermission rp ON rp.RoleId = ur.RoleId
    JOIN Core.Operation o ON o.Id = rp.OperationId WHERE o.Link IS NOT NULL
),
new_eff AS (
    SELECT DISTINCT tu.UserId, topx.Link, CAST(trp.CanView AS int) CanView, CAST(trp.CanAdd AS int) CanAdd,
           CAST(trp.CanEdit AS int) CanEdit, CAST(trp.CanDelete AS int) CanDelete, CAST(trp.CanApprove AS int) CanApprove
    FROM Core.TenantUser tu
    JOIN Core.TenantUserRole tur ON tur.TenantUserId = tu.Id
    JOIN Core.TenantRolePermission trp ON trp.TenantRoleId = tur.TenantRoleId
    JOIN Core.TenantOperation topx ON topx.Id = trp.TenantOperationId WHERE topx.Link IS NOT NULL
)
SELECT CASE WHEN
    (SELECT COUNT(*) FROM old_eff EXCEPT SELECT COUNT(*) FROM new_eff) IS NULL
    AND NOT EXISTS (SELECT * FROM old_eff EXCEPT SELECT * FROM new_eff)
    AND NOT EXISTS (SELECT * FROM new_eff EXCEPT SELECT * FROM old_eff)
    THEN 'MATCH - effective permissions identical in both models'
    ELSE '*** MISMATCH - do NOT switch the readers over ***' END AS verdict;
