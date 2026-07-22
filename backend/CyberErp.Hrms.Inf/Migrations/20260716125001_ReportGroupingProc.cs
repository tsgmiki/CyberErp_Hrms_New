using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// A DEDICATED pivot/grouping report stored procedure (Core.hrms_Report_EmployeeDirectoryGrouped).
    /// The reference (APSmart V3) never had a pivot SP — it fetched flat rows and grouped them in the JS
    /// grid (on screen) / ReportGroupedExportBuilder (export). This pushes that grouping LOGIC into SQL:
    /// the SP reads the chosen group-by columns from the criteria (reserved "__groupBy" value), returns
    /// the data pre-grouped (ordered by the group columns, honouring "__groupOrder"), and — when
    /// "__showSummary" is set — computes per-group subtotals server-side (a 3rd result set), the T-SQL
    /// analogue of ReportGroupedExportBuilder's group summaries. Group columns are WHITELISTED, so the
    /// dynamic ORDER BY / GROUP BY is injection-safe (QUOTENAME + IN(known columns)).
    /// </summary>
    public partial class ReportGroupingProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDirectoryGrouped
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Standard filters (same as the flat Employee Directory report).
    DECLARE @unitId  UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.OrganizationUnitId'));
    DECLARE @status  NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @hire1   DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
    DECLARE @hire2   DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
    DECLARE @mgrOnly BIT              = CASE WHEN JSON_VALUE(@Criteria, '$.IsManagerial') IN ('true','1') THEN 1 ELSE 0 END;
    DECLARE @useOutputs BIT           = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    -- PIVOT inputs (reference GridConfig / user grouping payload) travel as reserved criteria values.
    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'UnitName';   -- default grouping when none chosen

    -- Parse the comma list into an ORDERED, WHITELISTED set of group columns (OPENJSON [key] = level).
    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('UnitName', 'EmploymentStatus', 'IsManagerial', 'PositionCode');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'UnitName');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    ----------------------------------------------------------------------------------------------------
    -- Result set 1: column metadata — the GROUP columns lead (in level order), then the remaining
    -- output columns (filtered + re-labelled by the user's @OutputFields selection).
    ----------------------------------------------------------------------------------------------------
    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string',   220, NULL, NULL),
        (3, 'UnitName',         'Unit',       'string',   180, NULL, NULL),
        (4, 'PositionCode',     'Position',   'string',   120, NULL, NULL),
        (5, 'EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
        (6, 'IsManagerial',     'Managerial', 'boolean',   90, NULL, NULL),
        (7, 'HireDate',         'Hire Date',  'date',     110, NULL, NULL),
        (8, 'Salary',           'Salary',     'currency', 120, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    -- The shared detail projection + FROM/WHERE, reused by the data and summary sets.
    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        ou.Name  AS UnitName,
        pos.Code AS PositionCode,
        e.EmploymentStatus,
        e.IsManagerial,
        e.HireDate,
        e.Salary';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM Core.hrms_Employee e
        LEFT JOIN Core.CorePerson p             ON p.Id  = e.PersonId
        LEFT JOIN Core.hrms_Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Core.hrms_OrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitId  IS NULL OR pos.OrganizationUnitId = @unitId)
          AND (@status  IS NULL OR e.EmploymentStatus = @status)
          AND (@hire1   IS NULL OR e.HireDate >= @hire1)
          AND (@hire2   IS NULL OR e.HireDate <  DATEADD(DAY, 1, @hire2))
          AND (@mgrOnly = 0     OR e.IsManagerial = 1)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitId UNIQUEIDENTIFIER, @status NVARCHAR(30), @hire1 DATE, @hire2 DATE, @mgrOnly BIT';

    ----------------------------------------------------------------------------------------------------
    -- Result set 2: the detail rows, PRE-GROUPED server-side (ordered by the group columns + level,
    -- then EmployeeNumber). The grid renders these already grouped.
    ----------------------------------------------------------------------------------------------------
    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitId, @status, @hire1, @hire2, @mgrOnly;

    ----------------------------------------------------------------------------------------------------
    -- Result set 3 (optional): per-group SUBTOTALS — the T-SQL port of ReportGroupedExportBuilder's
    -- group summaries. One row per leaf group: the group column values + GroupCount + SalaryTotal.
    ----------------------------------------------------------------------------------------------------
    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount, SUM(d.Salary) AS SalaryTotal
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitId, @status, @hire1, @hire2, @mgrOnly;
    END
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_EmployeeDirectoryGrouped;");
        }
    }
}
