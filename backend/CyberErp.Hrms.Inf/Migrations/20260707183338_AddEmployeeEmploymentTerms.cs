using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeEmploymentTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractPeriod",
                schema: "Core",
                table: "hrms_Employee",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentNature",
                schema: "Core",
                table: "hrms_Employee",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsProbation",
                schema: "Core",
                table: "hrms_Employee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTerminated",
                schema: "Core",
                table: "hrms_Employee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProbationEndDate",
                schema: "Core",
                table: "hrms_Employee",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Employee_DateOfBirth",
                schema: "Core",
                table: "hrms_Employee",
                column: "DateOfBirth");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Employee_EmploymentStatus_IsProbation",
                schema: "Core",
                table: "hrms_Employee",
                columns: new[] { "EmploymentStatus", "IsProbation" });

            // Keep the new denormalized flag consistent with existing terminated records.
            migrationBuilder.Sql("UPDATE [Core].[hrms_Employee] SET [IsTerminated] = 1 WHERE [EmploymentStatus] = 'Terminated';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_Employee_DateOfBirth",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropIndex(
                name: "IX_hrms_Employee_EmploymentStatus_IsProbation",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "ContractPeriod",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "EmploymentNature",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "IsProbation",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "IsTerminated",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "ProbationEndDate",
                schema: "Core",
                table: "hrms_Employee");
        }
    }
}
