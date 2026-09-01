/*
 * DIAGNOSTIC (read-only). Lists the organization units whose staff cannot resolve a manager.
 *
 * WHY THIS MATTERS
 *   ImmediateManager / SecondLevelManager steps resolve by climbing from the employee's unit to its
 *   parent, grandparent and so on, stopping at the first unit holding an employee flagged
 *   IsManagerial. If that climb reaches the root without finding one, the step is unresolvable —
 *   and because StartIfDefinedAsync PRE-VALIDATES every step, that is not a stalled approval, it is
 *   a FAILED SUBMIT (EnsureDynamicApproversResolvableAsync throws). The bypass that forgives this
 *   applies only to MANAGERIAL employees, so ordinary staff get a hard error.
 *
 *   Live consequence today: Annual Leave step 1 is ImmediateManager, so every employee listed by
 *   the second query below is currently unable to submit an annual leave request. The same is why
 *   the 19 'Manager Review' / 'Supervisor Review' / 'Finance Review' steps were left OPEN rather
 *   than anchored (logic §12.72).
 *
 * HOW TO FIX
 *   For each unit below, tick "Managerial" on an employee whose position belongs to that unit — or
 *   to any ancestor of it, which covers all of its descendants at once. Designating managers for the
 *   units nearest the top of the tree therefore clears the most rows for the least work.
 *   Confirm the person has a login (Core.[User].EmployeeId), otherwise they resolve but cannot act.
 *
 *   Once no unit remains, the Manager/Supervisor steps can be switched to ImmediateManager and the
 *   annual-leave breakage disappears at the same time.
 */
SET NOCOUNT ON;

WITH anc AS (
    SELECT ou.Id AS UnitId, ou.Id AS AncId, ou.ParentId FROM Hrms.OrganizationUnit ou
    UNION ALL
    SELECT a.UnitId, o.Id, o.ParentId
    FROM anc a JOIN Hrms.OrganizationUnit o ON o.Id = a.ParentId
),
mgrUnits AS (
    SELECT DISTINCT po.OrganizationUnitId AS UnitId
    FROM Hrms.Position po
    JOIN Hrms.Employee em ON em.PositionId = po.Id AND em.IsManagerial = 1
),
staffed AS (
    SELECT p.OrganizationUnitId AS UnitId, COUNT(*) AS Staff
    FROM Hrms.Employee e JOIN Hrms.Position p ON p.Id = e.PositionId
    GROUP BY p.OrganizationUnitId
)
SELECT ou.Name AS OrganizationUnit,
       ISNULL(pr.Name, '(root)') AS ParentUnit,
       s.Staff AS StaffAffected,
       CAST(ou.Id AS varchar(40)) AS UnitId
FROM staffed s
JOIN Hrms.OrganizationUnit ou ON ou.Id = s.UnitId
LEFT JOIN Hrms.OrganizationUnit pr ON pr.Id = ou.ParentId
WHERE NOT EXISTS (
    SELECT 1 FROM anc JOIN mgrUnits m ON m.UnitId = anc.AncId
    WHERE anc.UnitId = s.UnitId)
ORDER BY s.Staff DESC, ou.Name
OPTION (MAXRECURSION 50);

/* The individual employees currently blocked (non-managerial, no manager anywhere up the chain). */
WITH anc AS (
    SELECT ou.Id AS UnitId, ou.Id AS AncId, ou.ParentId FROM Hrms.OrganizationUnit ou
    UNION ALL
    SELECT a.UnitId, o.Id, o.ParentId
    FROM anc a JOIN Hrms.OrganizationUnit o ON o.Id = a.ParentId
),
mgrUnits AS (
    SELECT DISTINCT po.OrganizationUnitId AS UnitId, em.Id AS MgrEmpId
    FROM Hrms.Position po
    JOIN Hrms.Employee em ON em.PositionId = po.Id AND em.IsManagerial = 1
)
SELECT 'Employees blocked from ImmediateManager steps (e.g. Annual Leave)' AS Note,
       COUNT(*) AS Employees
FROM Hrms.Employee e
JOIN Hrms.Position p ON p.Id = e.PositionId
WHERE e.IsManagerial = 0
  AND NOT EXISTS (
      SELECT 1 FROM anc JOIN mgrUnits m ON m.UnitId = anc.AncId
      WHERE anc.UnitId = p.OrganizationUnitId AND m.MgrEmpId <> e.Id)
OPTION (MAXRECURSION 50);
