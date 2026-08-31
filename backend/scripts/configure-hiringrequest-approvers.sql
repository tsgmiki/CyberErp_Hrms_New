/*
 * Gives "Hiring Need Approval" real approvers. It shipped with three steps and NONE, which left it
 * routing by the open-step fallback — actionable by anyone entitled to approve, rather than by the
 * designated approver.
 *
 * THE CHAIN (HC078: Directorate Head -> HR -> Finance)
 *   step 1  Directorate Head Review -> ImmediateManager        (the requester's own manager)
 *   step 2  HR Review               -> UnitManager @ Manpower Development and Administration Dept
 *   step 3  Finance Review          -> UnitManager @ Finance Directorate
 *
 * ⚠️ WHY step 1 IS ImmediateManager AND NOT UnitManager.
 *   UnitManager anchors at a FIXED unit stored in ApproverId — WorkflowApproverAuth resolves it as
 *   ResolveUnitManagerAsync(a.ApproverId, ...), ignoring the request's own unit. Anchoring step 1
 *   that way would send EVERY hiring request to one department's manager regardless of who raised
 *   it. ImmediateManager resolves against the requester instead, which is what "the directorate head
 *   above the manager who raised it" actually means. Steps 2 and 3 DO belong at fixed units (HR,
 *   Finance are the same offices for every request), which is exactly how Annual Leave, Other Leave
 *   and Salary Revision already model their HR step.
 *
 * ⚠️ REQUIRES the code fix that passes the requester's employee id into StartIfDefinedAsync
 *   (HiringRequestHandlers). Manager-type approvers resolve against WorkflowInstance.EmployeeId, and
 *   it was being written as NULL — with that null, step 1 can never resolve and the request stays
 *   invisible no matter what this script configures. Instances created BEFORE that fix still carry
 *   NULL and are reset below.
 *
 * ApproverId convention, copied from the existing rows: the all-zero Guid for types that need no
 * anchor (ImmediateManager), the ORGANIZATION UNIT id for UnitManager.
 *
 * Idempotent: guarded by NOT EXISTS per (step, type), so a second run inserts nothing.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by      nvarchar(200)    = N'configure-hiringrequest-approvers.sql';
DECLARE @rv      varbinary(8)     = 0x0000000000000001;
DECLARE @hrUnit  uniqueidentifier = '9259AC39-3D10-4A29-9C54-918CA25CFFE0'; -- Manpower Development and Administration Department
DECLARE @finUnit uniqueidentifier = 'ABD3E163-EF21-4998-BEF7-80B5A060B63B'; -- Finance Directorate
DECLARE @zero    uniqueidentifier = '00000000-0000-0000-0000-000000000000';

SELECT 'BEFORE' AS Stage, s.StepOrder, s.Name,
       ISNULL(a.ApproverType, '(open - no approver)') AS Approver
FROM Hrms.WorkflowStep s
JOIN Hrms.WorkflowDefinition d ON d.Id = s.DefinitionId AND d.EntityType = 'HiringRequest'
LEFT JOIN Hrms.WorkflowStepApprover a ON a.StepId = s.Id
ORDER BY s.StepOrder;

BEGIN TRAN;

-- The three rows, paired to their step by StepOrder within the HiringRequest definition.
;WITH target AS (
    SELECT s.Id AS StepId, s.StepOrder, s.TenantId,
           CASE s.StepOrder WHEN 1 THEN 'ImmediateManager' ELSE 'UnitManager' END AS ApproverType,
           CASE s.StepOrder WHEN 1 THEN @zero WHEN 2 THEN @hrUnit ELSE @finUnit END AS ApproverId,
           CASE s.StepOrder
                WHEN 1 THEN N'Immediate Manager'
                WHEN 2 THEN (SELECT N'Manager of ' + Name FROM Hrms.OrganizationUnit WHERE Id = @hrUnit)
                ELSE          (SELECT N'Manager of ' + Name FROM Hrms.OrganizationUnit WHERE Id = @finUnit)
           END AS DisplayName
    FROM Hrms.WorkflowStep s
    JOIN Hrms.WorkflowDefinition d ON d.Id = s.DefinitionId AND d.EntityType = 'HiringRequest'
    WHERE s.StepOrder BETWEEN 1 AND 3
)
INSERT INTO Hrms.WorkflowStepApprover
    (Id, StepId, ApproverType, ApproverId, DisplayName, TenantId, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), t.StepId, t.ApproverType, t.ApproverId, t.DisplayName, t.TenantId,
       SYSUTCDATETIME(), @by, @rv
FROM target t
WHERE NOT EXISTS (SELECT 1 FROM Hrms.WorkflowStepApprover a WHERE a.StepId = t.StepId);

/*
 * Re-anchor instances started before the code fix: they carry EmployeeId = NULL, so step 1 would
 * resolve to nobody and stay stuck. Recovered from the requesting employee behind RequestedBy —
 * matched by user name, since that is what the column holds.
 */
UPDATE i
SET i.EmployeeId = u.EmployeeId,
    i.UpdatedAt  = SYSUTCDATETIME(),
    i.UpdatedBy  = @by
FROM Hrms.WorkflowInstance i
JOIN Core.[User] u ON u.UserName = i.RequestedBy
WHERE i.EntityType = 'HiringRequest'
  AND i.EmployeeId IS NULL
  AND u.EmployeeId IS NOT NULL;

COMMIT;

SELECT 'AFTER' AS Stage, s.StepOrder, s.Name,
       ISNULL(a.ApproverType, '(open - no approver)') AS Approver, a.DisplayName
FROM Hrms.WorkflowStep s
JOIN Hrms.WorkflowDefinition d ON d.Id = s.DefinitionId AND d.EntityType = 'HiringRequest'
LEFT JOIN Hrms.WorkflowStepApprover a ON a.StepId = s.Id
ORDER BY s.StepOrder;

SELECT 'AFTER: instances still missing a requester (expect 0)' AS Stage, COUNT(*) AS Instances
FROM Hrms.WorkflowInstance WHERE EntityType = 'HiringRequest' AND EmployeeId IS NULL;
