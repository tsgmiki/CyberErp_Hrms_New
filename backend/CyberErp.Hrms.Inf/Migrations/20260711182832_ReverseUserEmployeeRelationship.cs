using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class ReverseUserEmployeeRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_Employee_UserId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "IsHeadOffice",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Core",
                table: "hrms_Employee");

            // Drop BranchId outright (it is NOT the employee link) and add a fresh, null EmployeeId
            // — a rename would carry branch-id values into the FK column and break it.
            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Core",
                table: "User");

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                schema: "Core",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_EmployeeId",
                schema: "Core",
                table: "User",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "User",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_EmployeeId",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                schema: "Core",
                table: "User");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Core",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHeadOffice",
                schema: "Core",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "Core",
                table: "hrms_Employee",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Employee_UserId",
                schema: "Core",
                table: "hrms_Employee",
                column: "UserId");
        }
    }
}
