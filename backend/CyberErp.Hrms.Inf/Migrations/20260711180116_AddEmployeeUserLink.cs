using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_Employee_UserId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Core",
                table: "hrms_Employee");
        }
    }
}
