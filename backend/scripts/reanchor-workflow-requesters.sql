/*
 * Backfills WorkflowInstance.EmployeeId on recruitment instances started before the routing-key fix.
 *
 * WHY
 *   EmployeeId is the ROUTING KEY every manager-type approver resolves against, and the approval
 *   inbox pre-filters dynamic steps on `EmployeeId != null` before it evaluates them. Both
 *   SubmitHiringRequest and SubmitJobRequisition used to pass null (logic §12.67), so instances
 *   created before the fix cannot route to an Immediate/Second-Level/Unit Manager step and are
 *   skipped by the inbox's dynamic branch.
 *
 *   It is recoverable because RequestedBy stores the submitting USER NAME, and Core.[User] carries
 *   the EmployeeId link.
 *
 * SCOPE — recruitment only, on purpose.
 *   HiringRequest and JobRequisition have a genuine subject: the manager who raised the request,
 *   whose own management chain the first step routes to.
 *
 *   SalaryRevision is DELIBERATELY EXCLUDED even though it also carries nulls. It has no single
 *   subject — one revision covers many employees — and its only step is UnitManager anchored at the
 *   HR department. Worse, ClimbAsync SELF-EXCLUDES the requester, and only HR may submit a salary
 *   revision: stamping the HR submitter here would exclude them from resolving their own unit's
 *   manager step and push the approval up to the CEO. Null is the correct value there.
 *
 * Idempotent: only touches rows that are still NULL.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by nvarchar(200) = N'reanchor-workflow-requesters.sql';

SELECT 'BEFORE' AS Stage, i.EntityType, i.Status, i.RequestedBy,
       ISNULL(CAST(i.EmployeeId AS varchar(40)), 'NULL') AS EmployeeId
FROM Hrms.WorkflowInstance i
WHERE i.EntityType IN ('HiringRequest', 'JobRequisition')
ORDER BY i.EntityType;

BEGIN TRAN;

UPDATE i
SET i.EmployeeId = u.EmployeeId,
    i.UpdatedAt  = SYSUTCDATETIME(),
    i.UpdatedBy  = @by
FROM Hrms.WorkflowInstance i
JOIN Core.[User] u ON u.UserName = i.RequestedBy
WHERE i.EntityType IN ('HiringRequest', 'JobRequisition')
  AND i.EmployeeId IS NULL
  AND u.EmployeeId IS NOT NULL;

COMMIT;

SELECT 'AFTER' AS Stage, i.EntityType, i.Status, i.RequestedBy,
       ISNULL(CAST(i.EmployeeId AS varchar(40)), 'NULL') AS EmployeeId
FROM Hrms.WorkflowInstance i
WHERE i.EntityType IN ('HiringRequest', 'JobRequisition')
ORDER BY i.EntityType;

SELECT 'AFTER: recruitment instances still unanchored (expect 0)' AS Stage, COUNT(*) AS Instances
FROM Hrms.WorkflowInstance
WHERE EntityType IN ('HiringRequest', 'JobRequisition') AND EmployeeId IS NULL;
