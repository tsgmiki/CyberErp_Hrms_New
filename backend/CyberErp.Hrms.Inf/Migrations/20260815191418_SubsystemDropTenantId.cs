using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SubsystemDropTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subsystem_TenantId_Name",
                schema: "Core",
                table: "Subsystem");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "Subsystem");

            migrationBuilder.CreateIndex(
                name: "IX_Subsystem_Name",
                schema: "Core",
                table: "Subsystem",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subsystem_Name",
                schema: "Core",
                table: "Subsystem");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Core",
                table: "Subsystem",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Subsystem_TenantId_Name",
                schema: "Core",
                table: "Subsystem",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }
    }
}
