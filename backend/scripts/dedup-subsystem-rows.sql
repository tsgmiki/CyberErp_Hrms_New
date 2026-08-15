/*
    dedup-subsystem-rows.sql -- 2026-08-15

    Removes the duplicate Core.Subsystem row for the Home portal, so the table can lose its TenantId
    and become the single global catalogue SRMS models.

    THE DUPLICATE. Subsystem rows were created PER TENANT, so 'HOME' exists twice:

        9FC9447D...  "Home"                            tenant 0AF6866E (demo)
                     0 modules, 0 tenant modules, 1 tenant entitlement
        B7340E07...  "Self Service Management Sysem"   tenant AADB4E82 (NVI)
                     4 modules, 8 tenant modules, 2 tenant entitlements

    Both carry Code = 'HOME'. That matters more than it looks: the Home portal's frontend matches
    `code === "HOME"` literally in five places, so once TenantId is gone and both rows are visible,
    which one wins is undefined. Names are already unique across all 8 rows -- only the code collides.

    WHICH ROW SURVIVES. B7340E07, the one that owns every module and operation. The empty row is
    deleted and its single entitlement is repointed, so the demo tenant keeps its access to Home
    through the surviving row rather than losing it.

    Safe to re-run: the UPDATE is a no-op once repointed, and the DELETE matches nothing.

    !! Run this BEFORE the migration that drops Subsystem.TenantId. With the duplicate still present,
    the replacement unique index on Name would still build (names differ) but the application would
    face two rows coded HOME with no tenant to tell them apart.
*/

SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @Redundant uniqueidentifier = '9FC9447D-E698-4B5B-99DA-699110CAC440';  -- "Home", empty
DECLARE @Survivor  uniqueidentifier = 'B7340E07-CF4D-47B2-A91A-85056C701D97';  -- owns the content

SELECT 'BEFORE' AS Stage, Id, Name, Code, TenantId FROM Core.Subsystem WHERE Id IN (@Redundant, @Survivor);

-- 1. Move the redundant row's entitlements onto the survivor. Skipped where the tenant already has
--    one, which would violate IX_TenantSubSystem_TenantId_SubSystemId.
UPDATE ts
SET ts.SubSystemId = @Survivor
FROM Core.TenantSubSystem ts
WHERE ts.SubSystemId = @Redundant
  AND NOT EXISTS (SELECT 1 FROM Core.TenantSubSystem x
                  WHERE x.SubSystemId = @Survivor AND x.TenantId = ts.TenantId);
PRINT CONCAT('TenantSubSystem rows repointed: ', @@ROWCOUNT);

-- 2. Anything left pointing at the redundant row would have been a duplicate entitlement; drop it.
DELETE FROM Core.TenantSubSystem WHERE SubSystemId = @Redundant;
PRINT CONCAT('Duplicate entitlements removed: ', @@ROWCOUNT);

-- 3. The other two foreign keys into Core.Subsystem. Both are expected to match nothing -- the
--    redundant row owns no modules -- but repointing rather than assuming keeps this honest.
UPDATE Core.Module      SET SubSystemId = @Survivor WHERE SubSystemId = @Redundant;
PRINT CONCAT('Modules repointed: ', @@ROWCOUNT);
UPDATE Core.TenantModule SET SubSystemId = @Survivor WHERE SubSystemId = @Redundant;
PRINT CONCAT('TenantModules repointed: ', @@ROWCOUNT);

-- 4. The row itself.
DELETE FROM Core.Subsystem WHERE Id = @Redundant;
PRINT CONCAT('Subsystem rows deleted: ', @@ROWCOUNT);

COMMIT TRANSACTION;

-- Verification: no code may appear twice, and nothing may still reference the deleted row.
SELECT 'Duplicate codes remaining' AS Check_, Code, COUNT(*) AS N
FROM Core.Subsystem GROUP BY Code HAVING COUNT(*) > 1;

SELECT 'Orphaned references' AS Check_, COUNT(*) AS N
FROM (
    SELECT SubSystemId FROM Core.TenantSubSystem
    UNION ALL SELECT SubSystemId FROM Core.Module
    UNION ALL SELECT SubSystemId FROM Core.TenantModule
) r
WHERE NOT EXISTS (SELECT 1 FROM Core.Subsystem s WHERE s.Id = r.SubSystemId);

SELECT 'AFTER' AS Stage, Id, Name, Code, TenantId FROM Core.Subsystem ORDER BY Code;
