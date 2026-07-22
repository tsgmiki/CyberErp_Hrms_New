using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SalaryRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsSalaryRevision",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RevisionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Basis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TargetJobGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AppliedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSalaryRevision", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSalaryRevisionLine",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaryRevisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProposedSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSalaryRevisionLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSalaryRevisionLine_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsSalaryRevisionLine_hrmsSalaryRevision_SalaryRevisionId",
                        column: x => x.SalaryRevisionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsSalaryRevision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSalaryRevision_Status",
                schema: "dbo",
                table: "hrmsSalaryRevision",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSalaryRevisionLine_EmployeeId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSalaryRevisionLine_SalaryRevisionId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine",
                column: "SalaryRevisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsSalaryRevisionLine",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSalaryRevision",
                schema: "dbo");
        }
    }
}
