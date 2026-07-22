using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminationReinstatement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReinstatedAt",
                schema: "Core",
                table: "hrms_EmployeeTermination",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VacatedPositionId",
                schema: "Core",
                table: "hrms_EmployeeTermination",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReinstatedAt",
                schema: "Core",
                table: "hrms_EmployeeTermination");

            migrationBuilder.DropColumn(
                name: "VacatedPositionId",
                schema: "Core",
                table: "hrms_EmployeeTermination");
        }
    }
}
