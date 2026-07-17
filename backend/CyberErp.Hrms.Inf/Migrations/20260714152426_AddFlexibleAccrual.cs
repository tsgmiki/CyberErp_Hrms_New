using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddFlexibleAccrual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConsiderExternalExperience",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MilestoneDate",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreMilestoneBaseLeaveDays",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreMilestoneIncrementDays",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreMilestoneIntervalYears",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "RuleType",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ServiceYears");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsiderExternalExperience",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "MilestoneDate",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "PreMilestoneBaseLeaveDays",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "PreMilestoneIncrementDays",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "PreMilestoneIntervalYears",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "RuleType",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");
        }
    }
}
