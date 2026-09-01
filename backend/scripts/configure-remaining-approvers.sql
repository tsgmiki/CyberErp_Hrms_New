/*
 * Configures the approver steps that can be resolved SAFELY across the remaining workflow
 * definitions. 47 steps had no approver at all; this sets 28 of them and deliberately leaves 19.
 *
 * WHAT IS SET
 *   'HR Review' / 'HR Approval' / 'HRBP Review'          (21) -> UnitManager @ Manpower Development
 *   'Executive Approval' / 'Approving Authority' /
 *   'Department Head Approval' / 'Directorate Review'     (7) -> UnitManager @ Office of the CEO
 *
 * WHAT IS DELIBERATELY LEFT OPEN
 *   'Manager Review' / 'Supervisor Review'               (14+5) -- see below
 *   'Finance Review'                                        (5) -- no finance unit has a manager
 *
 * WHY THE MANAGER STEPS ARE NOT ImmediateManager.
 *   Only 6 of 121 organization units have an employee flagged IsManagerial. 139 NON-managerial
 *   employees cannot resolve a manager anywhere up their chain, and the bypass in
 *   IsBypassableManagerStepAsync applies ONLY to managerial employees — so for those 139,
 *   EnsureDynamicApproversResolvableAsync THROWS and the submit fails outright. Configuring
 *   ImmediateManager here would block 28% of the workforce from raising leave, disciplinary
 *   cases, loans, training needs and trips.
 *
 *   (That is already live for annual leave, whose step 1 is ImmediateManager: those 139 employees
 *   cannot submit annual leave today. Designating unit managers fixes both at once — logic §12.72.)
 *
 * WHY NOT A ROLE APPROVER FOR THOSE STEPS.
 *   Role approvers are never pre-validated, so they never throw — but 'HR Admin' and 'HR Officer'
 *   have ZERO account holders and 'Department Manager' has one. Routing to a role nobody holds
 *   produces a step nobody can action: submission succeeds and the request then sits forever.
 *   An open step at least falls back to the entitled-approver audience (minus the requester).
 *
 * ⚠️ KNOWN EDGE CASE — THE CEO CANNOT SUBMIT A REQUEST THAT HAS AN EXECUTIVE STEP.
 *   ClimbAsync self-excludes the requester. The Office of the CEO is the topmost unit holding a
 *   manager (its ancestors NVI Board Of Director and Bord Of Director have none), so when that
 *   manager is themselves the requester the climb reaches the root and returns null — which throws
 *   at submit. Affects exactly the CEO account, for the definitions carrying one of the 7 executive
 *   steps. FIX: designate a manager on 'NVI Board Of Director'.
 *
 * SAFE FOR NULL-REQUESTER MODULES. CriticalPosition, JobOffer, SuccessionPlan, TalentReview and
 *   WorkforcePlan start their workflow with a null employeeId. UnitManager anchors at a FIXED unit
 *   and does not need the requester, so these steps resolve regardless — unlike ImmediateManager,
 *   which would throw for them on every submission.
 *
 * Idempotent: NOT EXISTS per step, so already-configured steps (Annual Leave, Appraisal, Job
 * Requisition, Other Leave, Salary Revision, Hiring Need 1-2) are never touched, and a second run
 * inserts nothing.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by      nvarchar(200)    = N'configure-remaining-approvers.sql';
DECLARE @rv      varbinary(8)     = 0x0000000000000001;
DECLARE @hrUnit  uniqueidentifier = '9259AC39-3D10-4A29-9C54-918CA25CFFE0'; -- Manpower Development and Administration Department
DECLARE @ceoUnit uniqueidentifier = 'A07859F0-D8F8-4911-BB3A-B858FB5E1564'; -- Office of the CEO

-- Both anchors MUST have a managerial employee, or every submission through them throws.
IF NOT EXISTS (SELECT 1 FROM Hrms.Position po JOIN Hrms.Employee em ON em.PositionId = po.Id AND em.IsManagerial = 1
               WHERE po.OrganizationUnitId = @hrUnit)
BEGIN
    RAISERROR(N'ABORTED: the HR anchor unit has no managerial employee - every HR step would be unresolvable and submissions would fail.', 16, 1);
    RETURN;
END
IF NOT EXISTS (SELECT 1 FROM Hrms.Position po JOIN Hrms.Employee em ON em.PositionId = po.Id AND em.IsManagerial = 1
               WHERE po.OrganizationUnitId = @ceoUnit)
BEGIN
    RAISERROR(N'ABORTED: the executive anchor unit has no managerial employee - every executive step would be unresolvable and submissions would fail.', 16, 1);
    RETURN;
END

SELECT 'BEFORE: steps with no approver' AS Stage, COUNT(*) AS Steps
FROM Hrms.WorkflowStep s
WHERE NOT EXISTS (SELECT 1 FROM Hrms.WorkflowStepApprover a WHERE a.StepId = s.Id);

BEGIN TRAN;

-- Step names are the seeded intent, so they drive the mapping. Listed explicitly rather than by
-- LIKE, so a new step name is silently left open instead of silently matching the wrong anchor.
WITH target AS (
    SELECT s.Id AS StepId, s.TenantId,
           CASE WHEN s.Name IN ('HR Review', 'HR Approval', 'HRBP Review') THEN @hrUnit
                ELSE @ceoUnit END AS ApproverId,
           CASE WHEN s.Name IN ('HR Review', 'HR Approval', 'HRBP Review')
                THEN (SELECT N'Manager of ' + Name FROM Hrms.OrganizationUnit WHERE Id = @hrUnit)
                ELSE (SELECT N'Manager of ' + Name FROM Hrms.OrganizationUnit WHERE Id = @ceoUnit)
           END AS DisplayName
    FROM Hrms.WorkflowStep s
    WHERE s.Name IN ('HR Review', 'HR Approval', 'HRBP Review',
                     'Executive Approval', 'Approving Authority',
                     'Department Head Approval', 'Directorate Review')
      AND NOT EXISTS (SELECT 1 FROM Hrms.WorkflowStepApprover a WHERE a.StepId = s.Id)
)
INSERT INTO Hrms.WorkflowStepApprover
    (Id, StepId, ApproverType, ApproverId, DisplayName, TenantId, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), t.StepId, 'UnitManager', t.ApproverId, t.DisplayName, t.TenantId,
       SYSUTCDATETIME(), @by, @rv
FROM target t;

COMMIT;

SELECT 'AFTER: configured' AS Stage, d.EntityType, s.StepOrder, s.Name, a.DisplayName
FROM Hrms.WorkflowStep s
JOIN Hrms.WorkflowDefinition d ON d.Id = s.DefinitionId
JOIN Hrms.WorkflowStepApprover a ON a.StepId = s.Id
WHERE a.CreatedBy = @by
ORDER BY d.EntityType, s.StepOrder;

SELECT 'AFTER: still open (expected: Manager/Supervisor/Finance)' AS Stage, s.Name, COUNT(*) AS Steps
FROM Hrms.WorkflowStep s
WHERE NOT EXISTS (SELECT 1 FROM Hrms.WorkflowStepApprover a WHERE a.StepId = s.Id)
GROUP BY s.Name
ORDER BY s.Name;
