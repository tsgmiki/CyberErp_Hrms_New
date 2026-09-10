/*
 * Converts Hrms.AuditLog into an APPEND-ONLY LEDGER TABLE — GxP option B (logic 12.90).
 *
 * WHY
 *   The audit trail was already unreachable from the application: AuditLogController is read-only and
 *   no handler writes it. But nothing stopped the app's own SQL login from deleting rows directly,
 *   and "we do not delete audit rows" is a policy statement, not a control. An append-only ledger
 *   table makes UPDATE and DELETE fail at the engine, and SQL Server keeps a cryptographic digest an
 *   inspector can be walked through: not "we did not tamper" but "tampering is detectable, here is
 *   the verification".
 *
 * WHY A SCRIPT AND NOT AN EF MIGRATION
 *   An existing table cannot be converted in place -- ALTER TABLE ... SET (LEDGER = ON) is not
 *   supported. The only path is create-copy-swap, which is a deliberate, verifiable deployment step,
 *   which is what a validated change should be anyway.
 *
 * !! READ BEFORE RUNNING !!
 *   1. This is close to ONE-WAY. Ledger tables cannot be un-ledgered; reversing it means another
 *      create-copy-swap, and the ledger's own history is then lost.
 *   2. FUTURE EF MIGRATIONS THAT ALTER Hrms.AuditLog WILL FAIL. The schema of a ledger table is
 *      heavily restricted. AuditLog has been stable for the life of the product, but adding a column
 *      to it after this point is a create-copy-swap of its own.
 *   3. Take a database backup first. This script keeps the original rows in
 *      Hrms.AuditLog_PreLedger rather than dropping them, so the swap is reversible until you drop
 *      that table yourself -- but a backup is still the right precaution.
 *   4. Run it with the application STOPPED. It renames the table the audit interceptor writes to.
 *
 * VERIFICATION AFTERWARDS
 *   EXEC sys.sp_verify_database_ledger;                       -- verifies the whole database ledger
 *   SELECT * FROM sys.database_ledger_transactions;           -- the transaction history
 *   SELECT * FROM Hrms.AuditLog_Ledger;                       -- the ledger view over the table
 *
 * Idempotent: refuses to run twice.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditLog'
           AND SCHEMA_NAME(schema_id) = 'Hrms' AND ledger_type_desc <> 'NON_LEDGER_TABLE')
BEGIN
    SELECT 'ALREADY A LEDGER TABLE — nothing to do.' AS Result;
    RETURN;
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditLog_PreLedger' AND SCHEMA_NAME(schema_id) = 'Hrms')
BEGIN
    RAISERROR(N'ABORTED: Hrms.AuditLog_PreLedger already exists — a previous run may have been interrupted. Inspect it before continuing.', 16, 1);
    RETURN;
END

DECLARE @before int = (SELECT COUNT(*) FROM Hrms.AuditLog);
SELECT 'BEFORE' AS Stage, @before AS AuditRows;

BEGIN TRAN;

-- The ledger twin. Column definitions mirror the EF model exactly; a mismatch here would break the
-- interceptor's inserts, which is why they are spelled out rather than generated.
CREATE TABLE Hrms.AuditLog_New
(
    Id                uniqueidentifier NOT NULL,
    EntityType        nvarchar(100)    NOT NULL,
    EntityId          uniqueidentifier NOT NULL,
    EntityName        nvarchar(300)    NULL,
    Action            nvarchar(30)     NOT NULL,
    Changes           nvarchar(max)    NULL,
    Reason            nvarchar(500)    NULL,
    PerformedByUserId uniqueidentifier NULL,
    PerformedBy       nvarchar(200)    NULL,
    BranchId          uniqueidentifier NULL,
    TenantId          uniqueidentifier NOT NULL,
    CreatedAt         datetime2(3)     NOT NULL,
    UpdatedAt         datetime2(7)     NULL,
    CreatedBy         nvarchar(max)    NULL,
    UpdatedBy         nvarchar(max)    NULL,
    RowVersion        varbinary(8)     NOT NULL,
    CONSTRAINT PK_AuditLog_New PRIMARY KEY (Id)
)
WITH (LEDGER = ON (APPEND_ONLY = ON));

-- Carry the history across. Insert is the only DML an append-only ledger table accepts, which is
-- exactly why the copy has to happen at creation time rather than by renaming into place.
INSERT INTO Hrms.AuditLog_New
    (Id, EntityType, EntityId, EntityName, Action, Changes, Reason,
     PerformedByUserId, PerformedBy, BranchId, TenantId,
     CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion)
SELECT Id, EntityType, EntityId, EntityName, Action, Changes, Reason,
       PerformedByUserId, PerformedBy, BranchId, TenantId,
       CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion
FROM Hrms.AuditLog;

DECLARE @copied int = @@ROWCOUNT;
IF @copied <> @before
BEGIN
    ROLLBACK TRAN;
    RAISERROR(N'ABORTED: copied %d rows but expected %d. Nothing has changed.', 16, 1, @copied, @before);
    RETURN;
END

-- The original is KEPT, renamed, so the swap can be undone until it is dropped deliberately.
--
-- ⚠️ The PRIMARY KEY is renamed BEFORE the tables. Renaming a table leaves its constraints named as
-- they were, so the preserved copy would still be holding the name PK_AuditLog when the new table
-- tries to claim it — and the swap would finish with a primary key called PK_AuditLog_New.
EXEC sp_rename 'Hrms.PK_AuditLog', 'PK_AuditLog_PreLedger', 'OBJECT';
EXEC sp_rename 'Hrms.AuditLog', 'AuditLog_PreLedger';
EXEC sp_rename 'Hrms.AuditLog_New', 'AuditLog';
EXEC sp_rename 'Hrms.PK_AuditLog_New', 'PK_AuditLog', 'OBJECT';

COMMIT;

SELECT 'AFTER' AS Stage,
       (SELECT COUNT(*) FROM Hrms.AuditLog)            AS LedgerRows,
       (SELECT COUNT(*) FROM Hrms.AuditLog_PreLedger)  AS PreservedRows,
       (SELECT ledger_type_desc FROM sys.tables
         WHERE name = 'AuditLog' AND SCHEMA_NAME(schema_id) = 'Hrms') AS LedgerType;

SELECT 'NEXT' AS Stage,
       'Run EXEC sys.sp_verify_database_ledger to confirm, then drop Hrms.AuditLog_PreLedger once satisfied.' AS Step;
