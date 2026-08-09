using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualLeaveReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualLeaveDays",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnnualLeaveReturn",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnualLeaveHeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "date", nullable: false),
                    ActualEndDate = table.Column<DateTime>(type: "date", nullable: false),
                    ApprovedDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    ActualDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    AdjustmentDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    ReturnType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnualLeaveReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnualLeaveReturn_AnnualLeaveHeader_AnnualLeaveHeaderId",
                        column: x => x.AnnualLeaveHeaderId,
                        principalSchema: "Hrms",
                        principalTable: "AnnualLeaveHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnualLeaveReturn_AnnualLeaveHeaderId_ConfirmedAt",
                schema: "Hrms",
                table: "AnnualLeaveReturn",
                columns: new[] { "AnnualLeaveHeaderId", "ConfirmedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnualLeaveReturn",
                schema: "Hrms");

            migrationBuilder.DropColumn(
                name: "ActualLeaveDays",
                schema: "Hrms",
                table: "AnnualLeaveHeader");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
