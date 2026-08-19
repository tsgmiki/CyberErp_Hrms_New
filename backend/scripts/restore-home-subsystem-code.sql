/*
    restore-home-subsystem-code.sql — 2026-08-15

    Restores Code = 'HOME' on the NVI tenant's Home subsystem row, which had been renamed to
    "Self Service Management Sysem" with Code = '003'.

    WHY THIS BROKE SOMETHING. Core.Subsystem.Code is not a label — it is a JOIN KEY that the Home
    portal's frontend matches on literally, in five places:

        useMenuModules.ts    .filter(s => s.code === "HOME")   <- the sidebar itself
        portalLanding.tsx    HOME_CODE = "HOME"                <- local screen vs deep-link
        widgets.tsx          HOME_CODE = "HOME"
        dashboard.tsx        HOME_CODE = "HOME"
        services/portal      subsystem.code === "HOME"

    With the code at '003' the portal still LOADED — login fine, feed returning 200 with all 21
    screens — but the sidebar rendered EMPTY and the launcher treated Home as an external subsystem,
    because nothing matched. A working API and a broken UI at the same time.

    ⚠️ Only the CODE is restored. The Name is left as the user set it: the sidebar and launcher
    display Name (through i18n) and never match on it, so it is safe to rename freely. Code is not.

    Scoped by Id, not by Name or Code, so re-running cannot touch the wrong row. The other tenant's
    Home row (9FC9447D…, tenant 0AF6866E) already carries 'HOME' and is untouched — per-tenant
    duplicates of the subsystem rows are expected, and Core.Subsystem is tenant-filtered, so two rows
    sharing a code never collide at runtime.
*/

SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @HomeSubsystemId uniqueidentifier = 'B7340E07-CF4D-47B2-A91A-85056C701D97';

SELECT 'BEFORE' AS Stage, Id, Name, Code, TenantId FROM Core.Subsystem WHERE Id = @HomeSubsystemId;

UPDATE Core.Subsystem
SET Code = 'HOME'
WHERE Id = @HomeSubsystemId
  AND Code <> 'HOME';

PRINT CONCAT('Rows updated: ', @@ROWCOUNT);

SELECT 'AFTER' AS Stage, Id, Name, Code, TenantId FROM Core.Subsystem WHERE Id = @HomeSubsystemId;

-- Verification: the row must now carry HOME and still own its modules.
SELECT 'Tenant modules under it' AS Check_, COUNT(*) AS N
FROM Core.TenantModule WHERE SubSystemId = @HomeSubsystemId;
