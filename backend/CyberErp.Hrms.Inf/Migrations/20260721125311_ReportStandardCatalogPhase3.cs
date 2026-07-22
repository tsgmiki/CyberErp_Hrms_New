using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Standard report catalog — Phase 3 (Cases &amp; Pipeline): Disciplinary Cases, Training
    /// Completion and the Recruitment Pipeline, cloned from the same report SP templates as
    /// Phases 1–2, plus the TrainingCourseId / EnrollmentStatus / ApplicationStage /
    /// RequisitionStatus / MeasureType / DisciplinaryStatus lookup keys.
    /// Definitions are registered per tenant by <c>ISeedDefaultReports</c>.
    /// </summary>
    public partial class ReportStandardCatalogPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- 11. Disciplinary Cases (flat) -----------------------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_DisciplinaryCases
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

    DECLARE @viol1   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ViolationDate1'));
    DECLARE @viol2   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ViolationDate2'));
    DECLARE @measure NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.MeasureType'), '');
    DECLARE @dstat   NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.DisciplinaryStatus'), '');
    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',     'Employee #',        'string',  120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',           'Full Name',         'string',  220, NULL, NULL),
        (3, 'UnitName',           'Unit',              'string',  180, NULL, NULL),
        (4, 'ViolationDate',      'Violation Date',    'date',    120, NULL, NULL),
        (5, 'ViolationType',      'Violation',         'string',  160, NULL, NULL),
        (6, 'MeasureType',        'Measure',           'string',  140, NULL, NULL),
        (7, 'DisciplinaryStatus', 'Case Status',       'string',  110, NULL, NULL),
        (8, 'EffectiveDate',      'Effective',         'date',    110, NULL, NULL),
        (9, 'ValidUntil',         'Valid Until',       'date',    110, NULL, NULL),
        (10,'AffectsPromotion',   'Blocks Promotion',  'boolean', 120, NULL, NULL),
        (11,'AffectsReward',      'Blocks Reward',     'boolean', 110, NULL, NULL),
        (12,'Resolution',         'Resolution',        'string',  250, NULL, NULL)
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
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','ViolationDate','ViolationType','MeasureType','DisciplinaryStatus','EffectiveDate','ValidUntil','AffectsPromotion','AffectsReward','Resolution')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[ViolationDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name AS UnitName,
               d.ViolationDate,
               d.ViolationType,
               d.MeasureType,
               d.Status AS DisciplinaryStatus,
               d.EffectiveDate,
               d.ValidUntil,
               d.AffectsPromotion,
               d.AffectsReward,
               d.Resolution
        FROM dbo.hrmsDisciplinaryMeasure d
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = d.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        WHERE d.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@viol1   IS NULL OR d.ViolationDate >= @viol1)
          AND (@viol2   IS NULL OR d.ViolationDate <  DATEADD(DAY, 1, @viol2))
          AND (@measure IS NULL OR d.MeasureType = @measure)
          AND (@dstat   IS NULL OR d.Status = @dstat)
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @viol1 DATE, @viol2 DATE, @measure NVARCHAR(30), @dstat NVARCHAR(30), @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @viol1, @viol2, @measure, @dstat, @unitIds;
END");

            // ---- 12. Training Completion (flat — one row per enrollment) ---------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_TrainingCompletion
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

    DECLARE @sess1    DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.SessionStart1'));
    DECLARE @sess2    DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.SessionStart2'));
    DECLARE @courseId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.TrainingCourseId'));
    DECLARE @estat    NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EnrollmentStatus'), '');
    DECLARE @unitIds  NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs BIT            = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #',   'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',    'string', 220, NULL, NULL),
        (3, 'UnitName',         'Unit',         'string', 180, NULL, NULL),
        (4, 'CourseName',       'Course',       'string', 200, NULL, NULL),
        (5, 'SessionStart',     'Session From', 'date',   110, NULL, NULL),
        (6, 'SessionEnd',       'Session To',   'date',   110, NULL, NULL),
        (7, 'DeliveryMode',     'Delivery',     'string', 110, NULL, NULL),
        (8, 'EnrollmentStatus', 'Status',       'string', 110, NULL, NULL),
        (9, 'AttendancePercent','Attendance %', 'number', 110, NULL, NULL),
        (10,'AssessmentScore',  'Score',        'number', 100, NULL, NULL),
        (11,'CompletedOn',      'Completed On', 'date',   110, NULL, NULL),
        (12,'FeedbackRating',   'Feedback',     'number', 100, NULL, NULL)
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
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','CourseName','SessionStart','SessionEnd','DeliveryMode','EnrollmentStatus','AttendancePercent','AssessmentScore','CompletedOn','FeedbackRating')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[SessionStart] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name  AS UnitName,
               tc.Name  AS CourseName,
               ts.StartDate AS SessionStart,
               ts.EndDate   AS SessionEnd,
               tc.DeliveryMode,
               te.Status AS EnrollmentStatus,
               te.AttendancePercent,
               te.AssessmentScore,
               te.CompletedOn,
               te.FeedbackRating
        FROM dbo.hrmsTrainingEnrollment te
        INNER JOIN dbo.hrmsTrainingSession ts ON ts.Id  = te.TrainingSessionId
        INNER JOIN dbo.hrmsTrainingCourse tc  ON tc.Id  = ts.TrainingCourseId
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = te.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        WHERE te.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@sess1    IS NULL OR ts.StartDate >= @sess1)
          AND (@sess2    IS NULL OR ts.StartDate <  DATEADD(DAY, 1, @sess2))
          AND (@courseId IS NULL OR ts.TrainingCourseId = @courseId)
          AND (@estat    IS NULL OR te.Status = @estat)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @sess1 DATE, @sess2 DATE, @courseId UNIQUEIDENTIFIER, @estat NVARCHAR(30), @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @sess1, @sess2, @courseId, @estat, @unitIds;
END");

            // ---- 13. Recruitment Pipeline (grouped by stage) ---------------------------------------
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_RecruitmentPipeline
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

    DECLARE @app1   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.AppliedAt1'));
    DECLARE @app2   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.AppliedAt2'));
    DECLARE @stage  NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.ApplicationStage'), '');
    DECLARE @rstat  NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.RequisitionStatus'), '');
    DECLARE @useOutputs BIT      = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'ApplicationStage';

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('ApplicationStage', 'RequisitionTitle', 'UnitName');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'ApplicationStage');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'CandidateNumber',   'Candidate #',   'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'CandidateName',     'Candidate',     'string', 220, NULL, NULL),
        (3, 'RequisitionNumber', 'Requisition #', 'string', 130, NULL, NULL),
        (4, 'RequisitionTitle',  'Vacancy',       'string', 200, NULL, NULL),
        (5, 'UnitName',          'Unit',          'string', 180, NULL, NULL),
        (6, 'ApplicationStage',  'Stage',         'string', 120, NULL, NULL),
        (7, 'RequisitionStatus', 'Req. Status',   'string', 120, NULL, NULL),
        (8, 'AppliedAt',         'Applied',       'date',   110, NULL, NULL),
        (9, 'ScreeningScore',    'Screening',     'number', 100, NULL, NULL),
        (10,'Source',            'Source',        'string', 110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    DECLARE @selectList NVARCHAR(MAX) = N'
        ca.CandidateNumber,
        LTRIM(RTRIM(CONCAT(ca.FirstName, '' '', ca.FatherName))) AS CandidateName,
        r.RequisitionNumber,
        r.Title  AS RequisitionTitle,
        ou.Name  AS UnitName,
        a.Stage  AS ApplicationStage,
        r.Status AS RequisitionStatus,
        a.AppliedAt,
        a.ScreeningScore,
        ca.Source';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM dbo.hrmsJobApplication a
        INNER JOIN dbo.hrmsCandidate ca       ON ca.Id  = a.CandidateId
        INNER JOIN dbo.hrmsJobRequisition r   ON r.Id   = a.RequisitionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = r.OrganizationUnitId
        WHERE a.TenantId = @TenantId
          AND (@BranchId IS NULL OR ou.BranchId = @BranchId OR ou.BranchId IS NULL)
          AND (@app1  IS NULL OR a.AppliedAt >= @app1)
          AND (@app2  IS NULL OR a.AppliedAt <  DATEADD(DAY, 1, @app2))
          AND (@stage IS NULL OR a.Stage = @stage)
          AND (@rstat IS NULL OR r.Status = @rstat)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @app1 DATE, @app2 DATE, @stage NVARCHAR(30), @rstat NVARCHAR(30)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [AppliedAt] DESC, [CandidateNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @app1, @app2, @stage, @rstat;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @app1, @app2, @stage, @rstat;
    END
END");

            // ---- 14. Extend the master lookup SP with the Phase-3 field keys -----------------------
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
    ELSE IF @Field = 'TrainingCourseId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsTrainingCourse
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'EnrollmentStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Enrolled'),('Completed'),('NoShow'),('Withdrawn')) v(Value);
    ELSE IF @Field = 'ApplicationStage'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Received'),('Screening'),('Shortlisted'),('Interview'),('Selected'),('OfferPending'),('OfferAccepted'),('Hired'),('Rejected'),('Withdrawn')) v(Value);
    ELSE IF @Field = 'RequisitionStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Draft'),('PendingApproval'),('Approved'),('Posted'),('Closed'),('Cancelled'),('Rejected')) v(Value);
    ELSE IF @Field = 'MeasureType'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('VerbalWarning'),('WrittenWarning'),('FinalWarning'),('Suspension'),('SalaryDeduction'),('Demotion'),('Termination')) v(Value);
    ELSE IF @Field = 'DisciplinaryStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Open'),('UnderReview'),('Resolved'),('Cancelled')) v(Value);
    ELSE
        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
        WHERE 1 = 0;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_DisciplinaryCases;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_TrainingCompletion;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_RecruitmentPipeline;");

            // Restore the master lookup SP to its Phase-2 body (drops the Phase-3 keys).
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
    }
}
