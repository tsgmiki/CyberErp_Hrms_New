using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class TripConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsPerDiemRate",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DailyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsPerDiemRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsPerDiemRate_hrmsJobGrade_JobGradeId",
                        column: x => x.JobGradeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsJobGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTripBudget",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTripBudget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTripBudget_hrmsOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "dbo",
                        principalTable: "hrmsOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsPerDiemRate_JobGradeId",
                schema: "dbo",
                table: "hrmsPerDiemRate",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsPerDiemRate_TenantId_JobGradeId_TripType",
                schema: "dbo",
                table: "hrmsPerDiemRate",
                columns: new[] { "TenantId", "JobGradeId", "TripType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTripBudget_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTripBudget",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTripBudget_TenantId_FiscalYear_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTripBudget",
                columns: new[] { "TenantId", "FiscalYear", "OrganizationUnitId" },
                unique: true,
                filter: "[OrganizationUnitId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsPerDiemRate",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTripBudget",
                schema: "dbo");
        }
    }
}
