/*
 * Designates the directorate heads as MANAGERIAL, so manager-routed workflow steps can resolve.
 *
 * WHY
 *   Manager steps (ImmediateManager / SecondLevelManager) resolve by climbing from the employee's
 *   unit until they find an employee flagged IsManagerial. Only 6 of 121 units held one, so 139
 *   non-managerial employees could not resolve a manager anywhere up their chain — and because
 *   StartIfDefinedAsync pre-validates every step, that is a FAILED SUBMIT, not a stalled approval
 *   (logic §12.72). Annual Leave step 1 is already ImmediateManager, so those 139 could not request
 *   leave at all.
 *
 *   Verified by simulation before running: these seven take the blocked count from 139 to ZERO.
 *   The climb inherits downward, so heads high in the tree cover every unit beneath them:
 *
 *     Bord Of Director (no staff, root)
 *     └── General Director                          <- Martha (Dr.) Yami
 *         ├── Finance Directorate            (no staff)
 *         ├── Vaccine Sales ... Directorate  (no staff)
 *         ├── HR Devt & Admin Directorate           <- Kehase Berhe + Aberash Teklu
 *         ├── Procurement & Property Directorate    <- Lemlem Hagos
 *         └── Vice General Director                 <- Esayas (Dr.) Gelaye
 *             ├── Quality Control & Assurance   (no staff)
 *             ├── Engineering & Maintenance     (no staff)
 *             ├── Vaccine R&D Directorate           <- Belayneh (Dr.) Getachew
 *             └── Vaccine Production Directorate    <- Gelagay (Dr.) Ayelet
 *
 *   That is why this designates SEVEN PEOPLE rather than "11 unit managers": five of the eleven
 *   units that needed one have NOBODY positioned in them, and they do not need one — an ancestor's
 *   head already covers them. Designating the intermediate directors as well is what keeps routing
 *   PRECISE; without them every "Supervisor Review" in the organisation would climb to the General
 *   Manager.
 *
 * WHO — each was chosen because their own position title names the unit they sit in. No judgement
 *   was applied beyond that:
 *     Martha (Dr.) Yami        "General Manager"                                    @ General Director
 *     Esayas (Dr.) Gelaye      "Operation Vice General Director"                    @ Vice General Director
 *     Kehase Berhe             "...Development and Administration Directorate Directer"  @ HR Devt & Admin
 *     Aberash Teklu            "...Development and Administration Directorate Directer"  @ HR Devt & Admin
 *     Lemlem Hagos             "Procurement,Property Administration & General service Directorate Directer"
 *     Gelagay (Dr.) Ayelet     "Vaccine Production and Drug Formulation Directorate Director"
 *     Belayneh (Dr.) Getachew  "Research and development Directorate director"
 *
 *   TWO people hold the identical director title in HR Devt & Admin. Both are designated on purpose:
 *   ResolvedManager carries a LIST of managers per unit, so either may act, and the data gives no
 *   basis for preferring one. All seven have a login, which matters — a manager who resolves but
 *   cannot sign in leaves the step stuck.
 *
 * WHAT ELSE THIS CHANGES. IsManagerial is not only workflow routing: it also makes the person a
 *   MANAGER for visibility (VisibilityScope.IsManager, UnitIds = their subtree), so each will now
 *   see employee data for their own directorate — and the General Manager for the whole organisation.
 *   That is appropriate for these roles but is a real widening, so it is stated rather than implied.
 *
 * NOT FIXED BY THIS. Bord Of Director is the root and has no staff, so the people at the TOP of each
 *   branch (the General Manager here, the CEO in the parallel branch) still cannot resolve a manager
 *   above themselves — ClimbAsync self-excludes the requester. They therefore cannot submit a request
 *   whose chain contains a manager step. Positioning someone in Bord Of Director is the remedy.
 *
 * Idempotent: only flips rows that are currently 0.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by nvarchar(200) = N'designate-unit-managers.sql';

DECLARE @heads TABLE (EmpId uniqueidentifier PRIMARY KEY);
INSERT INTO @heads (EmpId) VALUES
    ('C64E657A-D2B1-4538-A204-3686530301C4'),  -- Martha (Dr.) Yami        @ General Director
    ('2FCB4728-10A8-49CD-910B-96F6091E36F4'),  -- Esayas (Dr.) Gelaye      @ Vice General Director
    ('6169AB67-8071-4038-9FE0-9D1CA9172377'),  -- Kehase Berhe             @ HR Devt & Admin Directorate
    ('A142FA70-3583-4A6C-9A32-C9A984F425E2'),  -- Aberash Teklu            @ HR Devt & Admin Directorate
    ('3B0F9FE2-8AF2-4082-BA90-46BF26ECA67A'),  -- Lemlem Hagos             @ Procurement & Property Directorate
    ('A133123F-CAFB-4A7D-AB5B-7F047F3D090F'),  -- Gelagay (Dr.) Ayelet     @ Vaccine Production Directorate
    ('274C0575-ED4D-4B20-B4F0-F7754A5BE458');  -- Belayneh (Dr.) Getachew  @ Vaccine R&D Directorate

-- Refuse if any id does not resolve to an employee, rather than silently designating fewer.
IF (SELECT COUNT(*) FROM @heads h JOIN Hrms.Employee e ON e.Id = h.EmpId) <> (SELECT COUNT(*) FROM @heads)
BEGIN
    RAISERROR(N'ABORTED: one or more of the seven employee ids does not exist. Re-check before running.', 16, 1);
    RETURN;
END

SELECT 'BEFORE' AS Stage, pe.FirstName + ' ' + pe.FatherName AS Person, ou.Name AS Unit,
       em.IsManagerial, ISNULL(us.UserName, 'NO LOGIN') AS Login
FROM @heads h
JOIN Hrms.Employee em ON em.Id = h.EmpId
JOIN Core.Person pe ON pe.Id = em.PersonId
JOIN Hrms.Position po ON po.Id = em.PositionId
JOIN Hrms.OrganizationUnit ou ON ou.Id = po.OrganizationUnitId
LEFT JOIN Core.[User] us ON us.EmployeeId = em.Id
ORDER BY ou.Name;

BEGIN TRAN;

UPDATE em
SET em.IsManagerial = 1,
    em.UpdatedAt    = SYSUTCDATETIME(),
    em.UpdatedBy    = @by
FROM Hrms.Employee em
JOIN @heads h ON h.EmpId = em.Id
WHERE em.IsManagerial = 0;

COMMIT;

SELECT 'AFTER' AS Stage, pe.FirstName + ' ' + pe.FatherName AS Person, ou.Name AS Unit, em.IsManagerial
FROM @heads h
JOIN Hrms.Employee em ON em.Id = h.EmpId
JOIN Core.Person pe ON pe.Id = em.PersonId
JOIN Hrms.Position po ON po.Id = em.PositionId
JOIN Hrms.OrganizationUnit ou ON ou.Id = po.OrganizationUnitId
ORDER BY ou.Name;

/* The whole point: non-managerial employees who still cannot resolve a manager. Expect 0. */
WITH anc AS (
    SELECT ou.Id AS UnitId, ou.Id AS AncId, ou.ParentId FROM Hrms.OrganizationUnit ou
    UNION ALL
    SELECT a.UnitId, o.Id, o.ParentId FROM anc a JOIN Hrms.OrganizationUnit o ON o.Id = a.ParentId
),
mgrUnits AS (
    SELECT DISTINCT po.OrganizationUnitId AS UnitId, em.Id AS MgrEmpId
    FROM Hrms.Position po JOIN Hrms.Employee em ON em.PositionId = po.Id AND em.IsManagerial = 1
)
SELECT 'AFTER: employees still unable to resolve a manager (expect 0)' AS Stage, COUNT(*) AS Employees
FROM Hrms.Employee e
JOIN Hrms.Position p ON p.Id = e.PositionId
WHERE e.IsManagerial = 0
  AND NOT EXISTS (SELECT 1 FROM anc JOIN mgrUnits m ON m.UnitId = anc.AncId
                  WHERE anc.UnitId = p.OrganizationUnitId AND m.MgrEmpId <> e.Id)
OPTION (MAXRECURSION 50);
