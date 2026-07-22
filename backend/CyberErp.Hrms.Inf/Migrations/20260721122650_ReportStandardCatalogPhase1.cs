using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Standard report catalog — Phase 1 (Workforce): seven report stored procedures cloned from the
    /// <c>Core.hrms_Report_EmployeeDirectory</c> / <c>…Grouped</c> templates (same contract:
    /// @TenantId/@BranchId/@UserId/@ReportKey/@Criteria JSON/@OutputFields JSON/@Source/@Roles →
    /// RS1 column metadata, RS2 rows via whitelisted dynamic ORDER BY, optional RS3 group subtotals),
    /// plus new lookup keys on <c>Core.hrms_ReportFieldValues</c>. Report definitions are registered
    /// per tenant by <c>ISeedDefaultReports</c> (POST Report/seed-defaults) — not by this migration.
    /// </summary>
    public partial class ReportStandardCatalogPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- 1. Headcount by Unit (grouped: pivot + per-group counts) --------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_HeadcountByUnit
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

    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @status  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @nature  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentNature'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    -- Pivot inputs travel as reserved criteria values (same convention as EmployeeDirectoryGrouped).
    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'UnitName';

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('UnitName', 'BranchName', 'EmploymentStatus', 'EmploymentNature', 'Gender', 'IsManagerial');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'UnitName');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    -- Result set 1: group columns lead (level order), then the remaining selected output columns.
    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string',   220, NULL, NULL),
        (3, 'Gender',           'Gender',     'string',    90, NULL, NULL),
        (4, 'UnitName',         'Unit',       'string',   180, NULL, NULL),
        (5, 'BranchName',       'Branch',     'string',   150, NULL, NULL),
        (6, 'PositionTitle',    'Position',   'string',   200, NULL, NULL),
        (7, 'EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
        (8, 'EmploymentNature', 'Nature',     'string',   110, NULL, NULL),
        (9, 'IsManagerial',     'Managerial', 'boolean',   90, NULL, NULL),
        (10,'HireDate',         'Hire Date',  'date',     110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        p.Gender,
        ou.Name  AS UnitName,
        b.Name   AS BranchName,
        poc.Title AS PositionTitle,
        e.EmploymentStatus,
        e.EmploymentNature,
        e.IsManagerial,
        e.HireDate';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND ((@status IS NULL AND e.IsTerminated = 0) OR e.EmploymentStatus = @status)
          AND (@nature  IS NULL OR e.EmploymentNature = @nature)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @status NVARCHAR(30), @nature NVARCHAR(30)';

    -- Result set 2: detail rows pre-grouped server-side.
    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitIds, @status, @nature;

    -- Result set 3 (optional): headcount per leaf group.
    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitIds, @status, @nature;
    END
END");

            // ---- 2. Employee Demographics (grouped: gender / age bands) ----------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDemographics
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

    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @status  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'Gender';

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('Gender', 'AgeBand', 'UnitName', 'EmploymentStatus');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'Gender');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string', 220, NULL, NULL),
        (3, 'Gender',           'Gender',     'string',  90, NULL, NULL),
        (4, 'Age',              'Age',        'number',  70, NULL, NULL),
        (5, 'AgeBand',          'Age Band',   'string', 100, NULL, NULL),
        (6, 'UnitName',         'Unit',       'string', 180, NULL, NULL),
        (7, 'EmploymentStatus', 'Status',     'string', 110, NULL, NULL),
        (8, 'HireDate',         'Hire Date',  'date',   110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    -- Age computed birthday-accurate; bands follow the common enterprise demographic split.
    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        p.Gender,
        ca.Age,
        CASE WHEN ca.Age IS NULL THEN ''Unknown''
             WHEN ca.Age < 25 THEN ''Under 25''
             WHEN ca.Age < 35 THEN ''25 - 34''
             WHEN ca.Age < 45 THEN ''35 - 44''
             WHEN ca.Age < 55 THEN ''45 - 54''
             ELSE ''55+'' END AS AgeBand,
        ou.Name AS UnitName,
        e.EmploymentStatus,
        e.HireDate';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        CROSS APPLY (SELECT CASE WHEN e.DateOfBirth IS NULL THEN NULL
            ELSE DATEDIFF(YEAR, e.DateOfBirth, GETDATE())
                 - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, e.DateOfBirth, GETDATE()), e.DateOfBirth) > GETDATE() THEN 1 ELSE 0 END
            END AS Age) ca
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND ((@status IS NULL AND e.IsTerminated = 0) OR e.EmploymentStatus = @status)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @status NVARCHAR(30)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitIds, @status;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitIds, @status;
    END
END");

            // ---- 3. New Hires (flat) ---------------------------------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_NewHires
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

    DECLARE @hire1   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
    DECLARE @hire2   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @nature  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentNature'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #',  'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',   'string',   220, NULL, NULL),
        (3, 'Gender',           'Gender',      'string',    90, NULL, NULL),
        (4, 'HireDate',         'Hire Date',   'date',     110, NULL, NULL),
        (5, 'UnitName',         'Unit',        'string',   180, NULL, NULL),
        (6, 'BranchName',       'Branch',      'string',   150, NULL, NULL),
        (7, 'PositionTitle',    'Position',    'string',   200, NULL, NULL),
        (8, 'EmploymentNature', 'Nature',      'string',   110, NULL, NULL),
        (9, 'EmploymentStatus', 'Status',      'string',   110, NULL, NULL),
        (10,'IsProbation',      'On Probation','boolean',   95, NULL, NULL),
        (11,'Salary',           'Salary',      'currency', 120, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','Gender','HireDate','UnitName','BranchName','PositionTitle','EmploymentNature','EmploymentStatus','IsProbation','Salary')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[HireDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               p.Gender,
               e.HireDate,
               ou.Name   AS UnitName,
               b.Name    AS BranchName,
               poc.Title AS PositionTitle,
               e.EmploymentNature,
               e.EmploymentStatus,
               e.IsProbation,
               e.Salary
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@hire1   IS NULL OR e.HireDate >= @hire1)
          AND (@hire2   IS NULL OR e.HireDate <  DATEADD(DAY, 1, @hire2))
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND (@nature  IS NULL OR e.EmploymentNature = @nature)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @hire1 DATE, @hire2 DATE, @unitIds NVARCHAR(MAX), @nature NVARCHAR(30)',
        @TenantId, @BranchId, @hire1, @hire2, @unitIds, @nature;
END");

            // ---- 4. Probation Tracking (flat) ------------------------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_ProbationTracking
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

    DECLARE @end1    DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ProbationEnd1'));
    DECLARE @end2    DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ProbationEnd2'));
    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #',     'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',      'string', 220, NULL, NULL),
        (3, 'HireDate',         'Hire Date',      'date',   110, NULL, NULL),
        (4, 'ProbationEndDate', 'Probation Ends', 'date',   120, NULL, NULL),
        (5, 'DaysRemaining',    'Days Remaining', 'number', 110, NULL, NULL),
        (6, 'UnitName',         'Unit',           'string', 180, NULL, NULL),
        (7, 'PositionTitle',    'Position',       'string', 200, NULL, NULL),
        (8, 'EmploymentStatus', 'Status',         'string', 110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','HireDate','ProbationEndDate','DaysRemaining','UnitName','PositionTitle','EmploymentStatus')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[ProbationEndDate], [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               e.HireDate,
               e.ProbationEndDate,
               DATEDIFF(DAY, CAST(GETDATE() AS DATE), e.ProbationEndDate) AS DaysRemaining,
               ou.Name   AS UnitName,
               poc.Title AS PositionTitle,
               e.EmploymentStatus
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND e.IsProbation = 1
          AND e.IsTerminated = 0
          AND (@end1    IS NULL OR e.ProbationEndDate >= @end1)
          AND (@end2    IS NULL OR e.ProbationEndDate <  DATEADD(DAY, 1, @end2))
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @end1 DATE, @end2 DATE, @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @end1, @end2, @unitIds;
END");

            // ---- 5. Terminations & Attrition (grouped) ---------------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_Terminations
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

    DECLARE @lwd1   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.LastWorkingDate1'));
    DECLARE @lwd2   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.LastWorkingDate2'));
    DECLARE @type   NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.TerminationType'), '');
    DECLARE @tstat  NVARCHAR(40) = NULLIF(JSON_VALUE(@Criteria, '$.TerminationStatus'), '');
    DECLARE @useOutputs BIT      = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'TerminationType';

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('TerminationType', 'TerminationStatus', 'UnitName', 'BranchName');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'TerminationType');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',    'Employee #',       'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',          'Full Name',        'string', 220, NULL, NULL),
        (3, 'UnitName',          'Unit',             'string', 180, NULL, NULL),
        (4, 'BranchName',        'Branch',           'string', 150, NULL, NULL),
        (5, 'PositionTitle',     'Position',         'string', 200, NULL, NULL),
        (6, 'TerminationType',   'Type',             'string', 110, NULL, NULL),
        (7, 'TerminationStatus', 'Case Status',      'string', 150, NULL, NULL),
        (8, 'NoticeDate',        'Notice Date',      'date',   110, NULL, NULL),
        (9, 'LastWorkingDate',   'Last Working Day', 'date',   130, NULL, NULL),
        (10,'TenureYears',       'Tenure (Years)',   'number', 110, NULL, NULL),
        (11,'Reason',            'Reason',           'string', 250, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        ou.Name   AS UnitName,
        b.Name    AS BranchName,
        poc.Title AS PositionTitle,
        t.TerminationType,
        t.Status  AS TerminationStatus,
        t.NoticeDate,
        t.LastWorkingDate,
        CAST(ROUND(DATEDIFF(DAY, e.HireDate, t.LastWorkingDate) / 365.25, 1) AS DECIMAL(6,1)) AS TenureYears,
        t.Reason';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM dbo.hrmsEmployeeTermination t
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = t.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
        WHERE t.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@lwd1  IS NULL OR t.LastWorkingDate >= @lwd1)
          AND (@lwd2  IS NULL OR t.LastWorkingDate <  DATEADD(DAY, 1, @lwd2))
          AND (@type  IS NULL OR t.TerminationType = @type)
          AND (@tstat IS NULL OR t.Status = @tstat)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @lwd1 DATE, @lwd2 DATE, @type NVARCHAR(30), @tstat NVARCHAR(40)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [LastWorkingDate] DESC, [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @lwd1, @lwd2, @type, @tstat;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @lwd1, @lwd2, @type, @tstat;
    END
END");

            // ---- 6. Employee Movements / Transfers (flat) ------------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeMovements
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

    DECLARE @eff1  DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.EffectiveDate1'));
    DECLARE @eff2  DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.EffectiveDate2'));
    DECLARE @mtype NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.MovementType'), '');
    DECLARE @mstat NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.MovementStatus'), '');
    DECLARE @useOutputs BIT     = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber', 'Employee #',     'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',       'Full Name',      'string',   220, NULL, NULL),
        (3, 'MovementType',   'Movement',       'string',   110, NULL, NULL),
        (4, 'TransferKind',   'Transfer Kind',  'string',   110, NULL, NULL),
        (5, 'MovementStatus', 'Status',         'string',   110, NULL, NULL),
        (6, 'EffectiveDate',  'Effective Date', 'date',     120, NULL, NULL),
        (7, 'FromPosition',   'From Position',  'string',   160, NULL, NULL),
        (8, 'ToPosition',     'To Position',    'string',   160, NULL, NULL),
        (9, 'FromSalary',     'From Salary',    'currency', 120, NULL, NULL),
        (10,'ToSalary',       'To Salary',      'currency', 120, NULL, NULL),
        (11,'FromBranchName', 'From Branch',    'string',   150, NULL, NULL),
        (12,'ToBranchName',   'To Branch',      'string',   150, NULL, NULL),
        (13,'ExecutedAt',     'Executed',       'date',     110, NULL, NULL),
        (14,'Reason',         'Reason',         'string',   250, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','MovementType','TransferKind','MovementStatus','EffectiveDate','FromPosition','ToPosition','FromSalary','ToSalary','FromBranchName','ToBranchName','ExecutedAt','Reason')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[EffectiveDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               m.MovementType,
               m.TransferKind,
               m.Status AS MovementStatus,
               m.EffectiveDate,
               fp.Code  AS FromPosition,
               tp.Code  AS ToPosition,
               m.FromSalary,
               m.ToSalary,
               fb.Name  AS FromBranchName,
               tb.Name  AS ToBranchName,
               m.ExecutedAt,
               m.Reason
        FROM dbo.hrmsEmployeeMovement m
        INNER JOIN dbo.hrmsEmployee e  ON e.Id  = m.EmployeeId
        LEFT JOIN Core.CorePerson p    ON p.Id  = e.PersonId
        LEFT JOIN dbo.hrmsPosition fp  ON fp.Id = m.FromPositionId
        LEFT JOIN dbo.hrmsPosition tp  ON tp.Id = m.ToPositionId
        LEFT JOIN dbo.hrmsBranch fb    ON fb.Id = m.FromBranchId
        LEFT JOIN dbo.hrmsBranch tb    ON tb.Id = m.ToBranchId
        WHERE m.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@eff1  IS NULL OR m.EffectiveDate >= @eff1)
          AND (@eff2  IS NULL OR m.EffectiveDate <  DATEADD(DAY, 1, @eff2))
          AND (@mtype IS NULL OR m.MovementType = @mtype)
          AND (@mstat IS NULL OR m.Status = @mstat)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @eff1 DATE, @eff2 DATE, @mtype NVARCHAR(30), @mstat NVARCHAR(30)',
        @TenantId, @BranchId, @eff1, @eff2, @mtype, @mstat;
END");

            // ---- 7. Vacant Positions / Establishment (flat) ----------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_VacantPositions
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

    DECLARE @unitIds NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @classId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.PositionClassId'));
    DECLARE @useOutputs BIT           = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'PositionCode',       'Position Code',   'string', 130, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'PositionTitle',      'Position Title',  'string', 220, NULL, NULL),
        (3, 'UnitName',           'Unit',            'string', 180, NULL, NULL),
        (4, 'BranchName',         'Branch',          'string', 150, NULL, NULL),
        (5, 'MinQualifications',  'Qualifications',  'string', 220, NULL, NULL),
        (6, 'MinExperienceYears', 'Min Exp (Years)', 'number', 110, NULL, NULL),
        (7, 'VacantSince',        'Vacant Since',    'date',   110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('PositionCode','PositionTitle','UnitName','BranchName','MinQualifications','MinExperienceYears','VacantSince')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[UnitName], [PositionCode]';

    -- VacantSince approximates from the position row''s last update (vacancy sync touches it).
    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT pos.Code  AS PositionCode,
               poc.Title AS PositionTitle,
               ou.Name   AS UnitName,
               b.Name    AS BranchName,
               poc.MinQualifications,
               poc.MinExperienceYears,
               CAST(COALESCE(pos.UpdatedAt, pos.CreatedAt) AS DATE) AS VacantSince
        FROM dbo.hrmsPosition pos
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = pos.BranchId
        WHERE pos.TenantId = @TenantId
          AND (@BranchId IS NULL OR pos.BranchId = @BranchId)
          AND pos.IsVacant = 1
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND (@classId IS NULL OR pos.PositionClassId = @classId)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @classId UNIQUEIDENTIFIER',
        @TenantId, @BranchId, @unitIds, @classId;
END");

            // ---- 8. Extend the master lookup SP with the Phase-1 field keys ------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportFieldValues
    @TenantId  NVARCHAR(64),
    @BranchId  UNIQUEIDENTIFIER = NULL,
    @UserId    UNIQUEIDENTIFIER = NULL,
    @ReportKey NVARCHAR(100),
    @Field     NVARCHAR(100),
    @Dependency NVARCHAR(400) = NULL,
    @Search    NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Field = '@DynamicDate'
        SELECT v.Value, v.Label
        FROM (VALUES
            (1,  'Today',            'Today'),
            (2,  'Yesterday',        'Yesterday'),
            (3,  'Tomorrow',         'Tomorrow'),
            (4,  'StartOfWeek',      'Start of this week'),
            (5,  'EndOfWeek',        'End of this week'),
            (6,  'StartOfMonth',     'Start of this month'),
            (7,  'EndOfMonth',       'End of this month'),
            (8,  'StartOfLastMonth', 'Start of last month'),
            (9,  'EndOfLastMonth',   'End of last month'),
            (10, 'StartOfQuarter',   'Start of this quarter'),
            (11, 'EndOfQuarter',     'End of this quarter'),
            (12, 'StartOfYear',      'Start of this year'),
            (13, 'EndOfYear',        'End of this year'),
            (14, 'Last7Days',        '7 days ago'),
            (15, 'Last30Days',       '30 days ago'),
            (16, 'Last90Days',       '90 days ago')
        ) v(Seq, Value, Label)
        WHERE @Search IS NULL OR v.Label LIKE '%' + @Search + '%'
        ORDER BY v.Seq;
    ELSE IF @Field = 'OrganizationUnitId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsOrganizationUnit
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@BranchId IS NULL OR BranchId = @BranchId OR BranchId IS NULL)
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'EmploymentStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Active'),('Probation'),('OnLeave'),('Suspended'),('Terminated'),('Retired')) v(Value);
    ELSE IF @Field = 'LeaveTypeId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsLeaveType
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'BranchId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsBranch
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'PositionClassId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Title AS Label
        FROM dbo.hrmsPositionClass
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Title LIKE '%' + @Search + '%')
        ORDER BY Title;
    ELSE IF @Field = 'EmploymentNature'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Permanent'),('Contract')) v(Value);
    ELSE IF @Field = 'Gender'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Male'),('Female')) v(Value);
    ELSE IF @Field = 'MovementType'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Transfer'),('Promotion'),('Demotion')) v(Value);
    ELSE IF @Field = 'MovementStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Pending'),('Approved'),('Completed'),('Cancelled')) v(Value);
    ELSE IF @Field = 'TerminationType'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Voluntary'),('Involuntary')) v(Value);
    ELSE IF @Field = 'TerminationStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Initiated'),('ClearanceInProgress'),('Settled'),('Cancelled')) v(Value);
    ELSE
        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
        WHERE 1 = 0;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_HeadcountByUnit;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_EmployeeDemographics;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_NewHires;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_ProbationTracking;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_Terminations;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_EmployeeMovements;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_VacantPositions;");

            // Restore the master lookup SP to its pre-Phase-1 body.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportFieldValues
    @TenantId  NVARCHAR(64),
    @BranchId  UNIQUEIDENTIFIER = NULL,
    @UserId    UNIQUEIDENTIFIER = NULL,
    @ReportKey NVARCHAR(100),
    @Field     NVARCHAR(100),
    @Dependency NVARCHAR(400) = NULL,
    @Search    NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Field = '@DynamicDate'
        SELECT v.Value, v.Label
        FROM (VALUES
            (1,  'Today',            'Today'),
            (2,  'Yesterday',        'Yesterday'),
            (3,  'Tomorrow',         'Tomorrow'),
            (4,  'StartOfWeek',      'Start of this week'),
            (5,  'EndOfWeek',        'End of this week'),
            (6,  'StartOfMonth',     'Start of this month'),
            (7,  'EndOfMonth',       'End of this month'),
            (8,  'StartOfLastMonth', 'Start of last month'),
            (9,  'EndOfLastMonth',   'End of last month'),
            (10, 'StartOfQuarter',   'Start of this quarter'),
            (11, 'EndOfQuarter',     'End of this quarter'),
            (12, 'StartOfYear',      'Start of this year'),
            (13, 'EndOfYear',        'End of this year'),
            (14, 'Last7Days',        '7 days ago'),
            (15, 'Last30Days',       '30 days ago'),
            (16, 'Last90Days',       '90 days ago')
        ) v(Seq, Value, Label)
        WHERE @Search IS NULL OR v.Label LIKE '%' + @Search + '%'
        ORDER BY v.Seq;
    ELSE IF @Field = 'OrganizationUnitId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsOrganizationUnit
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@BranchId IS NULL OR BranchId = @BranchId OR BranchId IS NULL)
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'EmploymentStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Active'),('Probation'),('OnLeave'),('Suspended'),('Terminated'),('Retired')) v(Value);
    ELSE IF @Field = 'LeaveTypeId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsLeaveType
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE
        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
        WHERE 1 = 0;
END");
        }
    }
}
