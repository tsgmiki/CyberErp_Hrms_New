using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AppraisalCollaborativeWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableHrSignOff",
                schema: "dbo",
                table: "hrmsReviewCycle",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSecondLevelReview",
                schema: "dbo",
                table: "hrmsReviewCycle",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Stage",
                schema: "dbo",
                table: "hrmsAppraisal",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "HrSignature",
                schema: "dbo",
                table: "hrmsAppraisal",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HrSignedAt",
                schema: "dbo",
                table: "hrmsAppraisal",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerComments",
                schema: "dbo",
                table: "hrmsAppraisal",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerSignature",
                schema: "dbo",
                table: "hrmsAppraisal",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewerSignedAt",
                schema: "dbo",
                table: "hrmsAppraisal",
                type: "datetime2(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableHrSignOff",
                schema: "dbo",
                table: "hrmsReviewCycle");

            migrationBuilder.DropColumn(
                name: "EnableSecondLevelReview",
                schema: "dbo",
                table: "hrmsReviewCycle");

            migrationBuilder.DropColumn(
                name: "HrSignature",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.DropColumn(
                name: "HrSignedAt",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.DropColumn(
                name: "ReviewerComments",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.DropColumn(
                name: "ReviewerSignature",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.DropColumn(
                name: "ReviewerSignedAt",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.AlterColumn<string>(
                name: "Stage",
                schema: "dbo",
                table: "hrmsAppraisal",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
