using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class MoveLeavePolicyFieldsToSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarryForwardMaxDays",
                schema: "dbo",
                table: "hrmsLeaveType");

            migrationBuilder.DropColumn(
                name: "DefaultAnnualEntitlement",
                schema: "dbo",
                table: "hrmsLeaveType");

            migrationBuilder.DropColumn(
                name: "MaxConsecutiveDays",
                schema: "dbo",
                table: "hrmsLeaveType");

            migrationBuilder.AddColumn<decimal>(
                name: "CarryForwardMaxDays",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultAnnualEntitlement",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxConsecutiveDays",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarryForwardMaxDays",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "DefaultAnnualEntitlement",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "MaxConsecutiveDays",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.AddColumn<decimal>(
                name: "CarryForwardMaxDays",
                schema: "dbo",
                table: "hrmsLeaveType",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultAnnualEntitlement",
                schema: "dbo",
                table: "hrmsLeaveType",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxConsecutiveDays",
                schema: "dbo",
                table: "hrmsLeaveType",
                type: "int",
                nullable: true);
        }
    }
}
