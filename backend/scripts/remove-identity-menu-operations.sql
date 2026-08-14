/*
    remove-identity-menu-operations.sql — 2026-08-14

    Removes the seven identity/navigation ADMIN SCREENS from the HRMS menu, because SRMS manages them
    now and runs against this same CERP database:

        /user  /role  /userRole  /rolePermission  /subsystem  /module  /operation

    WHAT THIS DOES NOT TOUCH — deliberately. The tables behind those screens (Core.User, Core.Role,
    Core.UserRole, Core.SubSystem, Core.Module, Core.Operation) all stay. HRMS logs in against
    Core.User, renders its sidebar from Core.Module/Operation, and gates every [RequirePermission]
    on TenantRolePermission; Home reads the same catalogue. Only the MENU ENTRIES for the removed
    screens go, so the pages become unreachable rather than broken.

    Scope: the rows deleted are the ones parented to the HRMS "System" group. SRMS's own menu has no
    operations in this tree, so it is unaffected. The group keeps its six other children (Workflow
    Tracking, Workflow Definitions, Clearance Departments, Form Builder, Audit Trail, Settings).

    Delete order follows the FKs: TenantRolePermission -> TenantOperation -> Operation. Core.Operation
    is GLOBAL (no TenantId since 2026-08-13); the per-tenant copies are the TenantOperation rows, and
    every tenant's copy goes, not just the current one.

    Safe to re-run: each DELETE is a no-op once the rows are gone.
*/

SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @Retired TABLE (Id uniqueidentifier PRIMARY KEY, Link nvarchar(400));

INSERT INTO @Retired (Id, Link)
SELECT o.Id, o.Link
FROM Core.Operation o
JOIN Core.Module m ON m.Id = o.ModuleId
JOIN Core.SubSystem s ON s.Id = m.SubsystemId
WHERE s.Name = 'HRMS'
  AND o.Link IN ('/user', '/role', '/userRole', '/rolePermission', '/subsystem', '/module', '/operation');

DECLARE @Matched int = (SELECT COUNT(*) FROM @Retired);
PRINT CONCAT('Operations matched: ', @Matched);

-- 1. The grants. Without this the TenantOperation delete fails on the FK.
DELETE p
FROM Core.TenantRolePermission p
JOIN Core.TenantOperation t ON t.Id = p.TenantOperationId
WHERE t.OperationId IN (SELECT Id FROM @Retired);
PRINT CONCAT('TenantRolePermission rows deleted: ', @@ROWCOUNT);

-- 2. The per-tenant copies the sidebar and the permission gate actually read.
DELETE t
FROM Core.TenantOperation t
WHERE t.OperationId IN (SELECT Id FROM @Retired);
PRINT CONCAT('TenantOperation rows deleted: ', @@ROWCOUNT);

-- 3. The global templates.
DELETE o
FROM Core.Operation o
WHERE o.Id IN (SELECT Id FROM @Retired);
PRINT CONCAT('Operation rows deleted: ', @@ROWCOUNT);

COMMIT TRANSACTION;

-- Verification: both selects must come back empty.
SELECT 'Operation left behind' AS Check_, o.Name, o.Link
FROM Core.Operation o
WHERE o.Link IN ('/user', '/role', '/userRole', '/rolePermission', '/subsystem', '/module', '/operation');

SELECT 'TenantOperation left behind' AS Check_, t.Name, t.Link
FROM Core.TenantOperation t
WHERE t.Link IN ('/user', '/role', '/userRole', '/rolePermission', '/subsystem', '/module', '/operation');
