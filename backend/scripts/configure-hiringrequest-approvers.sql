/*
 * Gives "Hiring Need Approval" real approvers. It shipped with three steps and NONE, which left it
 * routing by the open-step fallback — actionable by anyone entitled to approve, rather than by the
 * designated approver.
 *
 * WHAT THIS CONFIGURES
 *   step 1  Directorate Head Review -> ImmediateManager   (the requester's own manager)
 *   step 2  HR Review               -> UnitManager @ Manpower Development and Administration Dept
 *   step 3  Finance Review          -> LEFT OPEN ON PURPOSE, see below
 *
 * STEP 3 IS DELIBERATELY NOT CONFIGURED — configuring it would BREAK SUBMISSION.
 *   No finance unit in this tenant has a designated manager: Finance Directorate, Finance
 *   Department, Cost and Budget Section, General Finance Section and General Account Team all have
 *   ZERO employees flagged IsManagerial, and the resolver's climb from Finance Directorate runs
 *   Finance Directorate -> General Director -> Bord Of Director -> root without finding one.
 *
 *   That matters more than "the step would be unresolvable". StartIfDefinedAsync pre-validates
 *   EVERY step's dynamic approvers before it starts an instance (preValidateApprovers defaults to
 *   true, and SubmitHiringRequest does not override it), and an unresolvable UnitManager THROWS.
 *   So anchoring step 3 at Finance would not merely strand requests at step 3 — it would make
 *   EVERY hiring-request submission fail outright, which is worse than the fallback it replaces.
 *
 *   TO ENABLE IT LATER: tick "Managerial" on an employee whose position belongs to the Finance
 *   Directorate (or a parent of it), confirm they have a login, then run the commented-out step 3
 *   block at the bottom of this script.
 *
 * WHY step 1 IS ImmediateManager AND NOT UnitManager.
 *   UnitManager anchors at a FIXED unit stored in ApproverId — WorkflowApproverAuth resolves it as
 *   ResolveUnitManagerAsync(a.ApproverId, ...), ignoring the request's own unit. Anchoring step 1
 *   that way would send EVERY hiring request to one department's manager regardless of who raised
 *   it. ImmediateManager resolves against the requester instead, which is what "the directorate head
 *   above the manager who raised it" actually means. Step 2 DOES belong at a fixed unit (HR is the
 *   same office for every request), which is how Annual Leave, Other Leave and Salary Revision
 *   already model their HR step.
 *
 * REQUIRES the code fix that passes the requester's employee id into StartIfDefinedAsync
 *   (HiringRequestHandlers). Manager-type approvers resolve against WorkflowInstance.EmployeeId, and
 *   it was being written as NULL — with that null, step 1 can never resolve and the request stays
 *   invisible no matter what this script configures. Instances created BEFORE that fix still carry
 *   NULL and are reset below.
 *
 * ApproverId convention, copied from the existing rows: the all-zero Guid for types that need no
 * anchor (ImmediateManager), the ORGANIZATION UNIT id for UnitManager.
 *
 * Idempotent: guarded by NOT EXISTS per step, so a second run inserts nothing.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by     nvarchar(200)    = N'configure-hiringrequest-approvers.sql';
DECLARE @rv     varbinary(8)     = 0x0000000000000001;
DECLARE @hrUnit uniqueidentifier = '9259AC39-3D10-4A29-9C54-918CA25CFFE0'; -- Manpower Development and Administration Department
DECLARE @zero   uniqueidentifier = '00000000-0000-0000-0000-000000000000';

SELECT 'BEFORE' AS Stage, s.StepOrder, s.Name,
       ISNULL(a.ApproverType, '(open - no approver)') AS Approver
FROM Hrms.WorkflowStep s
JOIN Hrms.WorkflowDefinition d ON d.Id = s.DefinitionId AND d.EntityType = 'HiringRequest'
LEFT JOIN Hrms.WorkflowStepApprover a ON a.StepId = s.Id
ORDER BY s.StepOrder;

-- Refuse to run if the HR anchor cannot resolve either — same failure mode as Finance.
IF NOT EXISTS (
    SELECT 1 FROM Hrms.Position po
    JOIN Hrms.Employee em ON em.PositionId = po.Id AND em.IsManagerial = 1
    WHERE po.OrganizationUnitId = @hrUnit)
BEGIN
    RAISERROR(N'ABORTED: the HR anchor unit has no managerial employee, so step 2 would be unresolvable and every submission would fail. Designate an HR manager first.', 16, 1);
    RETURN;
END

BEGIN TRAN;

-- Steps 1 and 2 only. Paired to their step by StepOrder within the HiringRequest definition.
WITH target AS (
    SELECT s.Id AS StepId, s.StepOrder, s.TenantId,
           CASE s.StepOrder WHEN 1 THEN 'ImmediateManager' ELSE 'UnitManager' END AS ApproverType,
           CASE s.StepOrder WHEN 1 THEN @zero ELSE @hrUnit END AS ApproverId,
           CASE s.StepOrder
                WHEN 1 THEN N'Immediate Manager'
                ELSE (SELECT N'Manager of ' + Name FROM Hrms.OrganizationUnit WHERE Id = @hrUnit)
           END AS DisplayName
    FROM Hrms.WorkflowStep s
    JOIN Hrms.WorkflowDefinition d ON d.Id = s.DefinitionId AND d.EntityType = 'HiringRequest'
    WHERE s.StepOrder IN (1, 2)
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

SELECT 'REMINDER' AS Stage,
       'Step 3 (Finance Review) is still open - no finance unit has a managerial employee.' AS Note;

/* ---------------------------------------------------------------------------
 * STEP 3 — run this ONLY after designating a manager in the Finance Directorate.
 * Verify first:
 *     SELECT em.Id, pe.FirstName, pe.FatherName
 *     FROM Hrms.Position po
 *     JOIN Hrms.Employee em ON em.PositionId = po.Id AND em.IsManagerial = 1
 *     JOIN Core.Person pe ON pe.Id = em.PersonId
 *     WHERE po.OrganizationUnitId = 'ABD3E163-EF21-4998-BEF7-80B5A060B63B';
 * That must return at least one row WITH a Core.[User] login before you continue.
 *
 * DECLARE @finUnit uniqueidentifier = 'ABD3E163-EF21-4998-BEF7-80B5A060B63B';
 * INSERT INTO Hrms.WorkflowStepApprover
 *     (Id, StepId, ApproverType, ApproverId, DisplayName, TenantId, CreatedAt, CreatedBy, RowVersion)
 * SELECT NEWID(), s.Id, 'UnitManager', @finUnit,
 *        (SELECT N'Manager of ' + Name FROM Hrms.OrganizationUnit WHERE Id = @finUnit),
 *        s.TenantId, SYSUTCDATETIME(), 'configure-hiringrequest-approvers.sql (step 3)', 0x0000000000000001
 * FROM Hrms.WorkflowStep s
 * JOIN Hrms.WorkflowDefinition d ON d.Id = s.DefinitionId AND d.EntityType = 'HiringRequest'
 * WHERE s.StepOrder = 3
 *   AND NOT EXISTS (SELECT 1 FROM Hrms.WorkflowStepApprover a WHERE a.StepId = s.Id);
 * --------------------------------------------------------------------------- */
