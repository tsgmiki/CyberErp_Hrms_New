using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class ReportEngineParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_ReportFieldOutput",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FieldOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportFieldOutput", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReportFieldOutput_hrms_Report_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "Core",
                        principalTable: "hrms_Report",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ReportRun",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    RanBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportRun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ReportSavedFilter",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportSavedFilter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReportSavedFilter_hrms_Report_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "Core",
                        principalTable: "hrms_Report",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportFieldOutput_ReportId",
                schema: "Core",
                table: "hrms_ReportFieldOutput",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportRun_TenantId_ReportKey",
                schema: "Core",
                table: "hrms_ReportRun",
                columns: new[] { "TenantId", "ReportKey" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportSavedFilter_ReportId",
                schema: "Core",
                table: "hrms_ReportSavedFilter",
                column: "ReportId");

            // Contract extension (reference pReportFieldOutput): every report SP now also receives
            // @OutputFields — a JSON array of the user's chosen output columns — and filters/orders
            // its OWN column-metadata result set by it. NULL/empty = full default set.
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDirectory
                    @TenantId  NVARCHAR(64),
                    @BranchId  UNIQUEIDENTIFIER = NULL,
                    @UserId    UNIQUEIDENTIFIER = NULL,
                    @ReportKey NVARCHAR(100),
                    @Criteria  NVARCHAR(MAX) = NULL,
                    @OutputFields NVARCHAR(MAX) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @unitId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.OrganizationUnitId'));
                    DECLARE @status NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
                    DECLARE @hire1  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
                    DECLARE @hire2  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
                    DECLARE @mgrOnly BIT             = CASE WHEN JSON_VALUE(@Criteria, '$.IsManagerial') IN ('true','1') THEN 1 ELSE 0 END;
                    DECLARE @useOutputs BIT          = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

                    -- Result set 1: the report's columns, limited + ordered by the user's selection.
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

                    -- Result set 2: the data (the grid binds columns via result set 1).
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
            migrationBuilder.DropTable(
                name: "hrms_ReportFieldOutput",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ReportRun",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ReportSavedFilter",
                schema: "Core");
        }
    }
}
