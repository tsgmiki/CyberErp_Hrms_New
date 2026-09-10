/*
 * Adds the HRMS admin entry for "Question Banks" — the reusable quiz-question library (logic 12.87).
 *
 * WHY
 *   Phase 4 gives a course version graded quizzes. The quiz itself is authored inside the course
 *   form, where the work happens, but a BANK is a shared library across courses and needs a home of
 *   its own: managing an organisation-wide compliance question set from inside one course's editor
 *   is the wrong place for it.
 *
 * WHERE
 *   The Learning module — beside /hrms/trainingCourse and /hrms/trainingCategory, which is where an
 *   author already goes to build a course.
 *
 * NAMESPACED LINK, unlike the self-service rows: this is an HR admin screen in the HRMS sidebar, so
 *   it follows the hrms/ convention that every other admin operation uses. The permission gate
 *   strips that prefix when comparing, so QuestionBankController's [RequirePermission("questionBank")]
 *   matches this row.
 *
 * GRANTS — mirrors /hrms/trainingCourse exactly, read from that row rather than hardcoded. Anyone
 *   who can build a course can write the questions that go in it; nobody else gains anything, and in
 *   particular UserRole (ordinary staff) is not in that set, so learners never see the library that
 *   holds the answer keys.
 *
 * Idempotent: guarded by NOT EXISTS on the operation and on each grant.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @by nvarchar(200) = N'add-question-bank-menu.sql';
DECLARE @rv varbinary(8)  = 0x0000000000000001;
DECLARE @link nvarchar(200) = N'/hrms/questionBank';
DECLARE @source nvarchar(200) = N'/hrms/trainingCourse';

-- Anchor to the module that already owns the course catalogue, so this cannot land in the wrong
-- subsystem's sidebar.
DECLARE @module uniqueidentifier =
    (SELECT TOP 1 o.ModuleId FROM Core.TenantOperation o WHERE o.Link = @source);

IF @module IS NULL
BEGIN
    RAISERROR(N'ABORTED: could not locate the Learning module via the course catalogue operation. Check the operation catalogue before running.', 16, 1);
    RETURN;
END

-- Sits directly after the course catalogue in the sidebar.
DECLARE @order int =
    (SELECT TOP 1 o.DisplayOrder + 1 FROM Core.TenantOperation o WHERE o.Link = @source);

SELECT 'BEFORE' AS Stage, @link AS Link,
       CASE WHEN EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = @link)
            THEN 'already present' ELSE 'to add' END AS State;

BEGIN TRAN;

-- NOTE: Core.TenantOperation and Core.TenantRolePermission carry NO TenantId column — the operation
-- catalogue is SHARED across tenants; scoping happens through TenantRole.
INSERT INTO Core.TenantOperation
    (Id, ModuleId, Name, Link, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), @module, N'Question Banks', @link, N'ListChecks', ISNULL(@order, 100), 1,
       SYSUTCDATETIME(), @by, @rv
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantOperation o WHERE o.Link = @link);

INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport,
     CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), src.TenantRoleId, tgt.Id, src.CanView, src.CanAdd, src.CanEdit, src.CanDelete, 0, 0,
       SYSUTCDATETIME(), @by, @rv
FROM Core.TenantRolePermission src
JOIN Core.TenantOperation srcOp ON srcOp.Id = src.TenantOperationId AND srcOp.Link = @source
JOIN Core.TenantOperation tgt ON tgt.Link = @link
WHERE NOT EXISTS (
    SELECT 1 FROM Core.TenantRolePermission x
    WHERE x.TenantOperationId = tgt.Id AND x.TenantRoleId = src.TenantRoleId);

COMMIT;

SELECT 'AFTER: operation' AS Stage, o.Link, o.Name, o.Icon, o.DisplayOrder, o.IsActive
FROM Core.TenantOperation o WHERE o.Link = @link;

SELECT 'AFTER: who can see it' AS Stage, tr.Name AS RoleName, rp.CanView, rp.CanAdd, rp.CanEdit, rp.CanDelete
FROM Core.TenantRolePermission rp
JOIN Core.TenantOperation o ON o.Id = rp.TenantOperationId
JOIN Core.TenantRole tr ON tr.Id = rp.TenantRoleId
WHERE o.Link = @link
ORDER BY tr.Name;
