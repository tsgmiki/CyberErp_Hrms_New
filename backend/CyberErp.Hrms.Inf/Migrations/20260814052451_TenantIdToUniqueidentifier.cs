using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Converts every <c>TenantId</c> column from nvarchar to <b>uniqueidentifier</b> — the SRMS
    /// platform type — across 201 tables.
    ///
    /// <para>The CLR property stays a <c>string</c>; a global value converter in
    /// <c>HrmsDbContext.OnModelCreating</c> bridges the two, so no entity, no repository filter and
    /// no handler changed. See logic.md §12.14.</para>
    ///
    /// <para>⚠️ WRITTEN BY HAND, AS DISCOVERY-DRIVEN SQL, NOT AS 400 SCAFFOLDED AlterColumn CALLS.
    /// EF generated the column changes and <b>no index handling whatsoever</b>, but <b>141 indexes
    /// include TenantId</b> and SQL Server refuses to alter a column an index depends on — the
    /// scaffold fails on the first indexed table. The script below drops those indexes, converts the
    /// columns, and rebuilds the indexes from definitions captured beforehand, so it stays correct
    /// even as tables are added.</para>
    ///
    /// <para>⚠️ Nineteen rows carry a BLANK TenantId — the global lookup tables, Organization,
    /// Setting — and an implicit conversion of <c>''</c> to uniqueidentifier fails. They are set to
    /// the empty GUID first, which is exactly what the value converter maps back to
    /// <c>string.Empty</c>, so the <c>IsNullOrEmpty</c> checks in the aggregate handlers keep working.</para>
    ///
    /// <para>Everything runs in ONE transaction with XACT_ABORT: a failure part-way through 201
    /// tables must not leave the schema half-converted.</para>
    /// </summary>
    public partial class TenantIdToUniqueidentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

/* 1. Capture the indexes that touch TenantId on the tables about to change, as CREATE statements,
      BEFORE dropping them. Unique-ness, key order, descending keys, included columns and filters all
      have to survive the round trip. */
IF OBJECT_ID('tempdb..#idx') IS NOT NULL DROP TABLE #idx;
CREATE TABLE #idx (seq int IDENTITY(1,1), drop_sql nvarchar(max), create_sql nvarchar(max));

INSERT INTO #idx (drop_sql, create_sql)
SELECT
    N'DROP INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(sc.name) + N'.' + QUOTENAME(t.name) + N';',
    N'CREATE ' + CASE WHEN i.is_unique = 1 THEN N'UNIQUE ' ELSE N'' END + i.type_desc COLLATE DATABASE_DEFAULT
      + N' INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(sc.name) + N'.' + QUOTENAME(t.name) + N' ('
      + STUFF((SELECT N', ' + QUOTENAME(c2.name) + CASE WHEN ic2.is_descending_key = 1 THEN N' DESC' ELSE N' ASC' END
               FROM sys.index_columns ic2
               JOIN sys.columns c2 ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id
               WHERE ic2.object_id = i.object_id AND ic2.index_id = i.index_id AND ic2.is_included_column = 0
               ORDER BY ic2.key_ordinal
               FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'') + N')'
      + ISNULL(N' INCLUDE (' + STUFF((SELECT N', ' + QUOTENAME(c3.name)
               FROM sys.index_columns ic3
               JOIN sys.columns c3 ON c3.object_id = ic3.object_id AND c3.column_id = ic3.column_id
               WHERE ic3.object_id = i.object_id AND ic3.index_id = i.index_id AND ic3.is_included_column = 1
               ORDER BY ic3.index_column_id
               FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'') + N')', N'')
      + ISNULL(N' WHERE ' + i.filter_definition, N'') + N';'
FROM sys.indexes i
JOIN sys.tables t  ON t.object_id = i.object_id
JOIN sys.schemas sc ON sc.schema_id = t.schema_id
WHERE i.type > 0 AND i.is_primary_key = 0 AND i.is_unique_constraint = 0
  AND EXISTS (SELECT 1 FROM sys.index_columns ic
              JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
              WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND c.name = 'TenantId')
  AND EXISTS (SELECT 1 FROM sys.columns c4
              JOIN sys.types ty ON ty.user_type_id = c4.user_type_id
              WHERE c4.object_id = t.object_id AND c4.name = 'TenantId' AND ty.name IN ('nvarchar','varchar'));

/* 1b. ⚠️ KEY CONSTRAINTS BLOCK THE ALTER TOO, and they are not indexes you can DROP INDEX.
       Hrms.NumberSequence has TenantId in its PRIMARY KEY, which is what the first attempt failed
       on. Captured and rebuilt as constraints, discovery-driven like the indexes so another such
       table is handled automatically. (Verified: no foreign key references these, so dropping is
       safe; if one ever did, this would fail loudly rather than corrupt anything.) */
IF OBJECT_ID('tempdb..#con') IS NOT NULL DROP TABLE #con;
CREATE TABLE #con (seq int IDENTITY(1,1), drop_sql nvarchar(max), create_sql nvarchar(max));

INSERT INTO #con (drop_sql, create_sql)
SELECT
    N'ALTER TABLE ' + QUOTENAME(sc.name) + N'.' + QUOTENAME(t.name) + N' DROP CONSTRAINT ' + QUOTENAME(i.name) + N';',
    N'ALTER TABLE ' + QUOTENAME(sc.name) + N'.' + QUOTENAME(t.name) + N' ADD CONSTRAINT ' + QUOTENAME(i.name)
      + CASE WHEN i.is_primary_key = 1 THEN N' PRIMARY KEY ' ELSE N' UNIQUE ' END
      + CASE WHEN i.type = 1 THEN N'CLUSTERED' ELSE N'NONCLUSTERED' END + N' ('
      + STUFF((SELECT N', ' + QUOTENAME(c2.name) + CASE WHEN ic2.is_descending_key = 1 THEN N' DESC' ELSE N' ASC' END
               FROM sys.index_columns ic2
               JOIN sys.columns c2 ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id
               WHERE ic2.object_id = i.object_id AND ic2.index_id = i.index_id AND ic2.is_included_column = 0
               ORDER BY ic2.key_ordinal
               FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'') + N');'
FROM sys.indexes i
JOIN sys.tables t  ON t.object_id = i.object_id
JOIN sys.schemas sc ON sc.schema_id = t.schema_id
WHERE (i.is_primary_key = 1 OR i.is_unique_constraint = 1)
  AND EXISTS (SELECT 1 FROM sys.index_columns ic
              JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
              WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND c.name = 'TenantId')
  AND EXISTS (SELECT 1 FROM sys.columns c4
              JOIN sys.types ty ON ty.user_type_id = c4.user_type_id
              WHERE c4.object_id = t.object_id AND c4.name = 'TenantId' AND ty.name IN ('nvarchar','varchar'));

/* 2. Drop the indexes, then the key constraints. */
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + drop_sql + NCHAR(10) FROM #idx ORDER BY seq;
IF LEN(ISNULL(@sql, N'')) > 0 EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql = @sql + drop_sql + NCHAR(10) FROM #con ORDER BY seq;
IF LEN(ISNULL(@sql, N'')) > 0 EXEC sp_executesql @sql;

/* 3. A blank TenantId cannot convert. The empty GUID is what the value converter maps back to
      string.Empty, so this is the faithful equivalent, not a fudge. */
SET @sql = N'';
SELECT @sql = @sql + N'UPDATE ' + QUOTENAME(TABLE_SCHEMA) + N'.' + QUOTENAME(TABLE_NAME)
    + N' SET TenantId = ''00000000-0000-0000-0000-000000000000'' WHERE LTRIM(RTRIM(ISNULL(TenantId,''''))) = '''';' + NCHAR(10)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'TenantId' AND DATA_TYPE IN ('nvarchar','varchar');
EXEC sp_executesql @sql;

/* 4. Convert every column. NOT NULL throughout — it always was. */
SET @sql = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(TABLE_SCHEMA) + N'.' + QUOTENAME(TABLE_NAME)
    + N' ALTER COLUMN TenantId uniqueidentifier NOT NULL;' + NCHAR(10)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'TenantId' AND DATA_TYPE IN ('nvarchar','varchar');
EXEC sp_executesql @sql;

/* 5. Rebuild: key constraints first (an index may depend on the clustered key), then the indexes. */
SET @sql = N'';
SELECT @sql = @sql + create_sql + NCHAR(10) FROM #con ORDER BY seq;
IF LEN(ISNULL(@sql, N'')) > 0 EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql = @sql + create_sql + NCHAR(10) FROM #idx ORDER BY seq;
IF LEN(ISNULL(@sql, N'')) > 0 EXEC sp_executesql @sql;

DROP TABLE #idx;
DROP TABLE #con;
COMMIT;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The mirror image: back to nvarchar(450). The empty GUID becomes '' again, so the data
            // reads exactly as it did before Up ran.
            migrationBuilder.Sql(@"
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF OBJECT_ID('tempdb..#idxd') IS NOT NULL DROP TABLE #idxd;
CREATE TABLE #idxd (seq int IDENTITY(1,1), drop_sql nvarchar(max), create_sql nvarchar(max));

INSERT INTO #idxd (drop_sql, create_sql)
SELECT
    N'DROP INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(sc.name) + N'.' + QUOTENAME(t.name) + N';',
    N'CREATE ' + CASE WHEN i.is_unique = 1 THEN N'UNIQUE ' ELSE N'' END + i.type_desc COLLATE DATABASE_DEFAULT
      + N' INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(sc.name) + N'.' + QUOTENAME(t.name) + N' ('
      + STUFF((SELECT N', ' + QUOTENAME(c2.name) + CASE WHEN ic2.is_descending_key = 1 THEN N' DESC' ELSE N' ASC' END
               FROM sys.index_columns ic2
               JOIN sys.columns c2 ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id
               WHERE ic2.object_id = i.object_id AND ic2.index_id = i.index_id AND ic2.is_included_column = 0
               ORDER BY ic2.key_ordinal
               FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'') + N')'
      + ISNULL(N' WHERE ' + i.filter_definition, N'') + N';'
FROM sys.indexes i
JOIN sys.tables t  ON t.object_id = i.object_id
JOIN sys.schemas sc ON sc.schema_id = t.schema_id
WHERE i.type > 0 AND i.is_primary_key = 0 AND i.is_unique_constraint = 0
  AND EXISTS (SELECT 1 FROM sys.index_columns ic
              JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
              WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND c.name = 'TenantId')
  AND EXISTS (SELECT 1 FROM sys.columns c4
              JOIN sys.types ty ON ty.user_type_id = c4.user_type_id
              WHERE c4.object_id = t.object_id AND c4.name = 'TenantId' AND ty.name = 'uniqueidentifier');

DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + drop_sql + NCHAR(10) FROM #idxd ORDER BY seq;
IF LEN(ISNULL(@sql, N'')) > 0 EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(TABLE_SCHEMA) + N'.' + QUOTENAME(TABLE_NAME)
    + N' ALTER COLUMN TenantId nvarchar(450) NOT NULL;' + NCHAR(10)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'TenantId' AND DATA_TYPE = 'uniqueidentifier';
EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql = @sql + N'UPDATE ' + QUOTENAME(TABLE_SCHEMA) + N'.' + QUOTENAME(TABLE_NAME)
    + N' SET TenantId = '''' WHERE TenantId = ''00000000-0000-0000-0000-000000000000'';' + NCHAR(10)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'TenantId' AND DATA_TYPE = 'nvarchar';
EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql = @sql + create_sql + NCHAR(10) FROM #idxd ORDER BY seq;
IF LEN(ISNULL(@sql, N'')) > 0 EXEC sp_executesql @sql;

DROP TABLE #idxd;
COMMIT;
");
        }
    }
}
