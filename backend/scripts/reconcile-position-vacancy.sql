/*
 * Reopens positions that are marked OCCUPIED but that nobody holds.
 *
 * WHY
 *   IsVacant is written in exactly two places, both in EmployeeHandlers: assigning an employee sets
 *   it false, and removing one recomputes it. Nothing else touches it. So a position with
 *   IsVacant = 0 and NO employee row pointing at it is stale data, not a state the application can
 *   produce. In CERP three such rows arrived with the 2026-08-10 NVI import (BA2-02, BA2-03, BA2-04,
 *   all "Assistant Production Technologist II"); their UpdatedAt was still NULL, so nothing had
 *   touched them since.
 *
 *   It matters because IsVacant is read as the establishment gate: hiring requests, job requisitions,
 *   transfer assessment, reinstatement and the transfer form's target-position picker all count
 *   vacancies from it. A seat stuck as occupied is a seat nobody can recruit into.
 *
 * ⚠️ SCOPE — the NOT EXISTS is the whole safety property.
 *   A TERMINATED employee still referencing the position keeps it occupied, and rightly so: the exit
 *   flow reopens the seat when the case settles. This only touches positions with no employee row of
 *   any kind pointing at them, which is why it cannot "free" a seat that someone still holds.
 *
 * Idempotent: a second run updates nothing.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by nvarchar(200) = N'reconcile-position-vacancy.sql';

SELECT 'BEFORE: occupied with no employee' AS Stage, COUNT(*) AS Positions
FROM Hrms.Position p
WHERE p.IsVacant = 0
  AND NOT EXISTS (SELECT 1 FROM Hrms.Employee e WHERE e.PositionId = p.Id);

-- What is about to change, named, so the run is auditable.
SELECT p.Code, ISNULL(pc.Title, '(no class)') AS Title, ISNULL(ou.Name, '(no unit)') AS OrganizationUnit
FROM Hrms.Position p
LEFT JOIN Hrms.PositionClass pc ON pc.Id = p.PositionClassId
LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id = p.OrganizationUnitId
WHERE p.IsVacant = 0
  AND NOT EXISTS (SELECT 1 FROM Hrms.Employee e WHERE e.PositionId = p.Id)
ORDER BY p.Code;

BEGIN TRAN;

UPDATE p
SET p.IsVacant = 1,
    p.UpdatedAt = SYSUTCDATETIME(),
    p.UpdatedBy = @by
FROM Hrms.Position p
WHERE p.IsVacant = 0
  AND NOT EXISTS (SELECT 1 FROM Hrms.Employee e WHERE e.PositionId = p.Id);

COMMIT;

SELECT 'AFTER: occupied with no employee (expect 0)' AS Stage, COUNT(*) AS Positions
FROM Hrms.Position p
WHERE p.IsVacant = 0
  AND NOT EXISTS (SELECT 1 FROM Hrms.Employee e WHERE e.PositionId = p.Id)
UNION ALL
SELECT 'AFTER: vacant positions', COUNT(*) FROM Hrms.Position WHERE IsVacant = 1
UNION ALL
SELECT 'AFTER: occupied positions', COUNT(*) FROM Hrms.Position WHERE IsVacant = 0
UNION ALL
SELECT 'CHECK: occupied positions with an employee', COUNT(*)
FROM Hrms.Position p
WHERE p.IsVacant = 0
  AND EXISTS (SELECT 1 FROM Hrms.Employee e WHERE e.PositionId = p.Id);
