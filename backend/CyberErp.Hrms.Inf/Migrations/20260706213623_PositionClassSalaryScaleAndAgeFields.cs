using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class PositionClassSalaryScaleAndAgeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionClass_hrms_JobGrade_JobGradeId",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.RenameColumn(
                name: "JobGradeId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionClass_JobGradeId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "IX_hrms_PositionClass_SalaryScaleId");

            migrationBuilder.AddColumn<int>(
                name: "MaximumAge",
                schema: "Core",
                table: "hrms_PositionClass",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumAge",
                schema: "Core",
                table: "hrms_PositionClass",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyWorkingHours",
                schema: "Core",
                table: "hrms_PositionClass",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionClass_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_PositionClass",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionClass_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropColumn(
                name: "MaximumAge",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropColumn(
                name: "MinimumAge",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropColumn(
                name: "WeeklyWorkingHours",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.RenameColumn(
                name: "SalaryScaleId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "JobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionClass_SalaryScaleId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "IX_hrms_PositionClass_JobGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionClass_hrms_JobGrade_JobGradeId",
                schema: "Core",
                table: "hrms_PositionClass",
                column: "JobGradeId",
                principalSchema: "Core",
                principalTable: "hrms_JobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
