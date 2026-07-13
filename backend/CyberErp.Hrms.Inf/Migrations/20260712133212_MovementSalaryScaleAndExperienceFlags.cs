using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class MovementSalaryScaleAndExperienceFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Job-grade ids are NOT salary-scale ids — DROP the old columns and ADD fresh null
            // scale columns (a rename would carry grade ids into the coreSalaryScale FK and break
            // it). Existing movements keep their salary snapshots; the scale reference starts null.
            migrationBuilder.DropColumn(
                name: "ToJobGradeId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.DropColumn(
                name: "FromJobGradeId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.AddColumn<Guid>(
                name: "ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FromSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                schema: "Core",
                table: "hrms_EmployeeExperience",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGovernmental",
                schema: "Core",
                table: "hrms_EmployeeExperience",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeMovement_ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                column: "ToSalaryScaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeMovement_coreSalaryScale_ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                column: "ToSalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeMovement_coreSalaryScale_ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.DropIndex(
                name: "IX_hrms_EmployeeMovement_ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                schema: "Core",
                table: "hrms_EmployeeExperience");

            migrationBuilder.DropColumn(
                name: "IsGovernmental",
                schema: "Core",
                table: "hrms_EmployeeExperience");

            migrationBuilder.DropColumn(
                name: "ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.DropColumn(
                name: "FromSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.AddColumn<Guid>(
                name: "ToJobGradeId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FromJobGradeId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
