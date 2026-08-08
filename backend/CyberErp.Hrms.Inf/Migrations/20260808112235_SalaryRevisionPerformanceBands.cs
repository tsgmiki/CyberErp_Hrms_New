using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SalaryRevisionPerformanceBands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TargetReviewCycleId",
                schema: "dbo",
                table: "hrmsSalaryRevision",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hrmsSalaryRevisionBand",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaryRevisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinScore = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSalaryRevisionBand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSalaryRevisionBand_hrmsSalaryRevision_SalaryRevisionId",
                        column: x => x.SalaryRevisionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsSalaryRevision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSalaryRevisionBand_SalaryRevisionId_MinScore",
                schema: "dbo",
                table: "hrmsSalaryRevisionBand",
                columns: new[] { "SalaryRevisionId", "MinScore" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsSalaryRevisionBand",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "TargetReviewCycleId",
                schema: "dbo",
                table: "hrmsSalaryRevision");
        }
    }
}
