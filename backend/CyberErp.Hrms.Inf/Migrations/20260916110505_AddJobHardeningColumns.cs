using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddJobHardeningColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSettlementReminderOn",
                schema: "Hrms",
                table: "TripRequest",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecutionAttemptOn",
                schema: "Hrms",
                table: "EmployeeMovement",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastExecutionError",
                schema: "Hrms",
                table: "EmployeeMovement",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSettlementReminderOn",
                schema: "Hrms",
                table: "TripRequest");

            migrationBuilder.DropColumn(
                name: "LastExecutionAttemptOn",
                schema: "Hrms",
                table: "EmployeeMovement");

            migrationBuilder.DropColumn(
                name: "LastExecutionError",
                schema: "Hrms",
                table: "EmployeeMovement");
        }
    }
}
