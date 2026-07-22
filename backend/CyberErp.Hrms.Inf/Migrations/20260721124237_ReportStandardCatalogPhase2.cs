using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Standard report catalog — Phase 2 (Leave &amp; Pay): Leave Balances, Leave Taken and the
    /// Salary Register, cloned from the same report SP templates as Phase 1, plus the
    /// FiscalYearId / JobGradeId / LeaveStatus lookup keys on <c>Core.hrms_ReportFieldValues</c>.
    /// Definitions are registered per tenant by <c>ISeedDefaultReports</c>.
    /// </summary>
    public partial class ReportStandardCatalogPhase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- 8. Leave Balances (flat) ----------------------------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_LeaveBalances
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

    DECLARE @fiscalYearId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.FiscalYearId'));
    DECLARE @leaveTypeId  UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.LeaveTypeId'));
    DECLARE @unitIds      NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs   BIT              = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',  'Employee #',      'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',        'Full Name',       'string', 220, NULL, NULL),
        (3, 'UnitName',        'Unit',            'string', 180, NULL, NULL),
        (4, 'LeaveTypeName',   'Leave Type',      'string', 150, NULL, NULL),
        (5, 'FiscalYearName',  'Fiscal Year',     'string', 110, NULL, NULL),
        (6, 'Entitled',        'Entitled',        'number', 100, NULL, NULL),
        (7, 'CarriedForward',  'Carried Forward', 'number', 120, NULL, NULL),
        (8, 'Adjusted',        'Adjusted',        'number', 100, NULL, NULL),
        (9, 'Taken',           'Taken',           'number', 100, NULL, NULL),
        (10,'Remaining',       'Remaining',       'number', 100, NULL, NULL)
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
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','LeaveTypeName','FiscalYearName','Entitled','CarriedForward','Adjusted','Taken','Remaining')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[EmployeeNumber], [LeaveTypeName]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name AS UnitName,
               lt.Name AS LeaveTypeName,
               fy.Name AS FiscalYearName,
               lb.Entitled,
               lb.CarriedForward,
               lb.Adjusted,
               lb.Taken,
               (lb.Entitled + lb.CarriedForward + lb.Adjusted - lb.Taken) AS Remaining
        FROM dbo.hrmsLeaveBalance lb
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = lb.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsLeaveType lt        ON lt.Id  = lb.LeaveTypeId
        LEFT JOIN Core.FiscalYear fy          ON fy.Id  = lb.FiscalYearId
        WHERE lb.TenantId = @TenantId
          AND (@BranchId     IS NULL OR e.BranchId = @BranchId)
          AND (@fiscalYearId IS NULL OR lb.FiscalYearId = @fiscalYearId)
          AND (@leaveTypeId  IS NULL OR lb.LeaveTypeId = @leaveTypeId)
          AND (@unitIds      IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @fiscalYearId UNIQUEIDENTIFIER, @leaveTypeId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @fiscalYearId, @leaveTypeId, @unitIds;
END");

            // ---- 9. Leave Taken (flat — one row per leave request line) ----------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_LeaveTaken
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

    DECLARE @start1      DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.StartDate1'));
    DECLARE @start2      DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.StartDate2'));
    DECLARE @leaveTypeId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.LeaveTypeId'));
    DECLARE @lstat       NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.LeaveStatus'), '');
    DECLARE @unitIds     NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs  BIT              = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber', 'Employee #',   'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',       'Full Name',    'string', 220, NULL, NULL),
        (3, 'UnitName',       'Unit',         'string', 180, NULL, NULL),
        (4, 'LeaveTypeName',  'Leave Type',   'string', 150, NULL, NULL),
        (5, 'StartDate',      'From',         'date',   110, NULL, NULL),
        (6, 'EndDate',        'To',           'date',   110, NULL, NULL),
        (7, 'DayPart',        'Day Part',     'string',  95, NULL, NULL),
        (8, 'WorkingDays',    'Working Days', 'number', 110, NULL, NULL),
        (9, 'RequestStatus',  'Status',       'string', 100, NULL, NULL),
        (10,'SubmittedDate',  'Submitted',    'date',   110, NULL, NULL)
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
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','LeaveTypeName','StartDate','EndDate','DayPart','WorkingDays','RequestStatus','SubmittedDate')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[StartDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name AS UnitName,
               lt.Name AS LeaveTypeName,
               ll.StartDate,
               ll.EndDate,
               ll.DayPart,
               ll.WorkingDays,
               lr.Status AS RequestStatus,
               lr.SubmittedDate
        FROM dbo.hrmsLeaveRequestLine ll
        INNER JOIN dbo.hrmsLeaveRequest lr    ON lr.Id  = ll.LeaveRequestId
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = lr.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsLeaveType lt        ON lt.Id  = ll.LeaveTypeId
        WHERE ll.TenantId = @TenantId
          AND (@BranchId    IS NULL OR e.BranchId = @BranchId)
          AND (@start1      IS NULL OR ll.StartDate >= @start1)
          AND (@start2      IS NULL OR ll.StartDate <  DATEADD(DAY, 1, @start2))
          AND (@leaveTypeId IS NULL OR ll.LeaveTypeId = @leaveTypeId)
          AND (@lstat       IS NULL OR lr.Status = @lstat)
          AND (@unitIds     IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @start1 DATE, @start2 DATE, @leaveTypeId UNIQUEIDENTIFIER, @lstat NVARCHAR(30), @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @start1, @start2, @leaveTypeId, @lstat, @unitIds;
END");

            // ---- 10. Salary Register (grouped: pivot + per-group count & salary subtotals) ---------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_SalaryRegister
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
    DECLARE @gradeId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.JobGradeId'));
    DECLARE @status  NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @useOutputs BIT           = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'UnitName';

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('UnitName', 'BranchName', 'JobGradeName', 'EmploymentStatus');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'UnitName');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string',   220, NULL, NULL),
        (3, 'UnitName',         'Unit',       'string',   180, NULL, NULL),
        (4, 'BranchName',       'Branch',     'string',   150, NULL, NULL),
        (5, 'PositionTitle',    'Position',   'string',   200, NULL, NULL),
        (6, 'JobGradeName',     'Job Grade',  'string',   130, NULL, NULL),
        (7, 'StepName',         'Step',       'string',   100, NULL, NULL),
        (8, 'EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
        (9, 'HireDate',         'Hire Date',  'date',     110, NULL, NULL),
        (10,'Salary',           'Salary',     'currency', 120, NULL, NULL)
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
        jg.Name   AS JobGradeName,
        st.Name   AS StepName,
        e.EmploymentStatus,
        e.HireDate,
        e.Salary';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
        LEFT JOIN Core.coreSalaryScale ss     ON ss.Id  = e.SalaryScaleId
        LEFT JOIN dbo.hrmsJobGrade jg         ON jg.Id  = ss.JobGradeId
        LEFT JOIN Core.lupStep st             ON st.Id  = ss.StepId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND (@gradeId  IS NULL OR ss.JobGradeId = @gradeId)
          AND ((@status IS NULL AND e.IsTerminated = 0) OR e.EmploymentStatus = @status)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @gradeId UNIQUEIDENTIFIER, @status NVARCHAR(30)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitIds, @gradeId, @status;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount, SUM(d.Salary) AS SalaryTotal
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitIds, @gradeId, @status;
    END
END");

            // ---- 11. Extend the master lookup SP with the Phase-2 field keys -----------------------
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
    ELSE IF @Field = 'FiscalYearId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Core.FiscalYear
        WHERE TenantId = @TenantId
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY StartDate DESC;
    ELSE IF @Field = 'JobGradeId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsJobGrade
        WHERE TenantId = @TenantId
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'LeaveStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Draft'),('Pending'),('Approved'),('Rejected'),('Cancelled')) v(Value);
    ELSE
        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
        WHERE 1 = 0;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_LeaveBalances;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_LeaveTaken;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_SalaryRegister;");

            // Restore the master lookup SP to its Phase-1 body (drops the Phase-2 keys).
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
    }
}
