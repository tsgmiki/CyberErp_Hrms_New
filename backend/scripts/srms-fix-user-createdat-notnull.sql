/*
  Core.User.CreatedAt -> NOT NULL.

  WHY THIS IS A CORRECTION, NOT A CHANGE
  --------------------------------------
  The SRMS model already says this column is required, in two independent places:

    * BaseEntity.CreatedAt is a non-nullable NodaTime Instant, assigned in the constructor.
    * SrmsDbContextModelSnapshot declares  b.Property<DateTime>("CreatedAt")  — non-nullable —
      and the initial migration created it as  nullable: false.

  No migration in the project ever made it nullable. The DATABASE drifted away from the model
  outside of migrations; this puts it back. Twenty of the twenty-three CreatedAt columns in this
  database are already NOT NULL, so the drift is the exception, not the convention.

  It was found while aligning the CERP database against this one: after closing every other
  difference across the 22 shared tables, User.CreatedAt was the only one left — and it turned out
  the stricter side was CERP, which matches what SRMS itself intends.

  ⚠️ WHY A SCRIPT AND NOT A MIGRATION
  `dotnet ef migrations add` currently FAILS in this project, before reaching any of this:

      The property 'OperationId' cannot be added to the type
      'CyberErp.Srms.Dom.Entities.Core.TenantOperation (Dictionary<string, object>)' because no
      property type was specified and there is no corresponding CLR property or field.

  That is a pre-existing model error unrelated to this fix, and it blocks the EF tooling entirely.
  Once it is resolved, this belongs in a migration; hand-forging snapshot files against a model that
  will not load would be worse than a script.

  Safe to re-run. Refuses rather than inventing timestamps if any row is NULL.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM Core.[User] WHERE CreatedAt IS NULL)
BEGIN
    -- Deliberately fails instead of guessing. A fabricated creation date is worse than a NULL one:
    -- it is indistinguishable from a real one afterwards. Decide what those rows should say first.
    THROW 50000, 'Core.User has rows with a NULL CreatedAt. Set them deliberately before running this.', 1;
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'User'
             AND COLUMN_NAME = 'CreatedAt' AND IS_NULLABLE = 'YES')
BEGIN
    ALTER TABLE Core.[User] ALTER COLUMN CreatedAt datetime2(3) NOT NULL;
    PRINT 'Core.User.CreatedAt is now NOT NULL.';
END
ELSE
BEGIN
    PRINT 'Core.User.CreatedAt is already NOT NULL — nothing to do.';
END

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
  FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'User' AND COLUMN_NAME = 'CreatedAt';

/*
  ⚠️ TWO MORE TABLES CARRY THE SAME DRIFT, left alone deliberately because they were not the ask and
  are not part of the CERP comparison: Core.LookUpCategory and Core.LookUpCategoryList. If you want
  this database consistent with its own model throughout, they need the same treatment:

      ALTER TABLE Core.LookUpCategory     ALTER COLUMN CreatedAt datetime2(3) NOT NULL;
      ALTER TABLE Core.LookUpCategoryList ALTER COLUMN CreatedAt datetime2(3) NOT NULL;

  Check for NULLs there first, exactly as above.
*/
