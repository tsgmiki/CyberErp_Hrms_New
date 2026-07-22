using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddReportEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_Report",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReportGrouping = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StoredProc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_Report", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ReportField",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FieldOrder = table.Column<int>(type: "int", nullable: false),
                    DependencyField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReportField_hrms_Report_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "Core",
                        principalTable: "hrms_Report",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Report_TenantId_IsActive",
                schema: "Core",
                table: "hrms_Report",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Report_TenantId_ReportKey",
                schema: "Core",
                table: "hrms_Report",
                columns: new[] { "TenantId", "ReportKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportField_ReportId",
                schema: "Core",
                table: "hrms_ReportField",
                column: "ReportId");

            // ---- Master lookup procedure (ported from the reference `_x_ReportFieldValues`) ----
            // Feeds dropdown/radio options for report parameters. Extend the IF chain per field key.
            migrationBuilder.Sql("""
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

                    IF @Field = 'OrganizationUnitId'
                        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
                        FROM Core.hrms_OrganizationUnit
                        WHERE TenantId = @TenantId AND IsActive = 1
                          AND (@BranchId IS NULL OR BranchId = @BranchId OR BranchId IS NULL)
                          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
                        ORDER BY Name;
                    ELSE IF @Field = 'EmploymentStatus'
                        SELECT v.Value, v.Value AS Label
                        FROM (VALUES ('Active'),('Probation'),('OnLeave'),('Suspended'),('Terminated'),('Retired')) v(Value);
                    ELSE IF @Field = 'LeaveTypeId'
                        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
                        FROM Core.hrms_LeaveType
                        WHERE TenantId = @TenantId AND IsActive = 1
                          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
                        ORDER BY Name;
                    ELSE
                        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
                        WHERE 1 = 0;
                END
                """);

            // ---- Sample report SP: Employee Directory --------------------------------------
            // The generic contract every report SP follows:
            //   result set 1 = column metadata (Field, Label, Type, Width, LinkPage, LinkPageValue)
            //   result set 2 = the data rows; @Criteria is a JSON dict parsed HERE (never web-side SQL).
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDirectory
                    @TenantId  NVARCHAR(64),
                    @BranchId  UNIQUEIDENTIFIER = NULL,
                    @UserId    UNIQUEIDENTIFIER = NULL,
                    @ReportKey NVARCHAR(100),
                    @Criteria  NVARCHAR(MAX) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @unitId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.OrganizationUnitId'));
                    DECLARE @status NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
                    DECLARE @hire1  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
                    DECLARE @hire2  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
                    DECLARE @mgrOnly BIT             = CASE WHEN JSON_VALUE(@Criteria, '$.IsManagerial') IN ('true','1') THEN 1 ELSE 0 END;

                    -- Result set 1: the report declares its OWN columns.
                    SELECT Field, Label, [Type], Width, LinkPage, LinkPageValue
                    FROM (VALUES
                        ('EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
                        ('FullName',         'Full Name',  'string',   220, NULL, NULL),
                        ('UnitName',         'Unit',       'string',   180, NULL, NULL),
                        ('PositionCode',     'Position',   'string',   120, NULL, NULL),
                        ('EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
                        ('IsManagerial',     'Managerial', 'boolean',   90, NULL, NULL),
                        ('HireDate',         'Hire Date',  'date',     110, NULL, NULL),
                        ('Salary',           'Salary',     'currency', 120, NULL, NULL)
                    ) c(Field, Label, [Type], Width, LinkPage, LinkPageValue);

                    -- Result set 2: the data.
                    SELECT e.EmployeeNumber,
                           LTRIM(RTRIM(CONCAT(p.FirstName, ' ', p.GrandFatherName))) AS FullName,
                           ou.Name  AS UnitName,
                           pos.Code AS PositionCode,
                           e.EmploymentStatus,
                           e.IsManagerial,
                           e.HireDate,
                           e.Salary
                    FROM Core.hrms_Employee e
                    LEFT JOIN Core.CorePerson p            ON p.Id  = e.PersonId
                    LEFT JOIN Core.hrms_Position pos       ON pos.Id = e.PositionId
                    LEFT JOIN Core.hrms_OrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
                    WHERE e.TenantId = @TenantId
                      AND (@BranchId IS NULL OR e.BranchId = @BranchId)
                      AND (@unitId  IS NULL OR pos.OrganizationUnitId = @unitId)
                      AND (@status  IS NULL OR e.EmploymentStatus = @status)
                      AND (@hire1   IS NULL OR e.HireDate >= @hire1)
                      AND (@hire2   IS NULL OR e.HireDate <  DATEADD(DAY, 1, @hire2))
                      AND (@mgrOnly = 0     OR e.IsManagerial = 1)
                    ORDER BY e.EmployeeNumber;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_Report_EmployeeDirectory;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS Core.hrms_ReportFieldValues;");

            migrationBuilder.DropTable(
                name: "hrms_ReportField",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_Report",
                schema: "Core");
        }
    }
}
