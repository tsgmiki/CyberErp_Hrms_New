using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class ReportOutputFieldOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The sample report SP now consumes the STRUCTURED @OutputFields JSON produced by the Fields
            // popup — [{ "Field", "Label", "Order", "SortOrder" }] (SQL Server analog of the reference's
            // Field■Label■order■sortOrder packed string): RS1 columns are filtered to the checked fields,
            // ordered by Order, and re-labelled; RS2 data is ORDER BY'd by the fields whose SortOrder>0
            // (priority ascending) via a whitelisted dynamic clause. Empty @OutputFields = full default set.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDirectory
    @TenantId  NVARCHAR(64),
    @BranchId  UNIQUEIDENTIFIER = NULL,
    @UserId    UNIQUEIDENTIFIER = NULL,
    @ReportKey NVARCHAR(100),
    @Criteria  NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source NVARCHAR(20) = NULL,
    @Roles NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @unitId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.OrganizationUnitId'));
    DECLARE @status NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @hire1  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
    DECLARE @hire2  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
    DECLARE @mgrOnly BIT             = CASE WHEN JSON_VALUE(@Criteria, '$.IsManagerial') IN ('true','1') THEN 1 ELSE 0 END;
    DECLARE @useOutputs BIT          = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    -- Result set 1: the report's columns, filtered + ordered + re-labelled by the user's selection.
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
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    -- Result set 2: the data, ORDER BY'd by the chosen sort fields (SortOrder>0, priority ascending).
    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','PositionCode','EmploymentStatus','IsManagerial','HireDate','Salary')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name  AS UnitName,
               pos.Code AS PositionCode,
               e.EmploymentStatus,
               e.IsManagerial,
               e.HireDate,
               e.Salary
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
          AND (@mgrOnly = 0     OR e.IsManagerial = 1)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitId UNIQUEIDENTIFIER, @status NVARCHAR(30), @hire1 DATE, @hire2 DATE, @mgrOnly BIT',
        @TenantId, @BranchId, @unitId, @status, @hire1, @hire2, @mgrOnly;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the string-array @OutputFields form.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDirectory
    @TenantId  NVARCHAR(64),
    @BranchId  UNIQUEIDENTIFIER = NULL,
    @UserId    UNIQUEIDENTIFIER = NULL,
    @ReportKey NVARCHAR(100),
    @Criteria  NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source NVARCHAR(20) = NULL,
    @Roles NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @unitId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.OrganizationUnitId'));
    DECLARE @status NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @hire1  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
    DECLARE @hire2  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
    DECLARE @mgrOnly BIT             = CASE WHEN JSON_VALUE(@Criteria, '$.IsManagerial') IN ('true','1') THEN 1 ELSE 0 END;
    DECLARE @useOutputs BIT          = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;
    SELECT c.Field, c.Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
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
    LEFT JOIN OPENJSON(@OutputFields) o ON o.[value] = c.Field
    WHERE @useOutputs = 0 OR o.[value] IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN CAST(o.[key] AS INT) ELSE c.Seq END;
    SELECT e.EmployeeNumber,
           LTRIM(RTRIM(CONCAT(p.FirstName, ' ', p.GrandFatherName))) AS FullName,
           ou.Name AS UnitName, pos.Code AS PositionCode, e.EmploymentStatus,
           e.IsManagerial, e.HireDate, e.Salary
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
      AND (@mgrOnly = 0     OR e.IsManagerial = 1)
    ORDER BY e.EmployeeNumber;
END");
        }
    }
}
