using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrgUnitManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_Employee_ManagerEmployeeId",
                schema: "Core",
                table: "hrms_OrganizationUnit");

            migrationBuilder.DropIndex(
                name: "IX_hrms_OrganizationUnit_ManagerEmployeeId",
                schema: "Core",
                table: "hrms_OrganizationUnit");

            migrationBuilder.DropColumn(
                name: "ManagerEmployeeId",
                schema: "Core",
                table: "hrms_OrganizationUnit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManagerEmployeeId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_OrganizationUnit_ManagerEmployeeId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                column: "ManagerEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_Employee_ManagerEmployeeId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                column: "ManagerEmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
