using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeJobGradeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Employee_hrms_JobGrade_JobGradeId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropIndex(
                name: "IX_hrms_Employee_JobGradeId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "JobGradeId",
                schema: "Core",
                table: "hrms_Employee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobGradeId",
                schema: "Core",
                table: "hrms_Employee",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Employee_JobGradeId",
                schema: "Core",
                table: "hrms_Employee",
                column: "JobGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Employee_hrms_JobGrade_JobGradeId",
                schema: "Core",
                table: "hrms_Employee",
                column: "JobGradeId",
                principalSchema: "Core",
                principalTable: "hrms_JobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
