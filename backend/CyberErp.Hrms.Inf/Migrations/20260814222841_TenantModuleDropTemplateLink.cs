using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class TenantModuleDropTemplateLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantModule_Module_ModuleId",
                schema: "Core",
                table: "TenantModule");

            migrationBuilder.DropIndex(
                name: "IX_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantModule");

            migrationBuilder.DropIndex(
                name: "IX_TenantModule_TenantId_ModuleId",
                schema: "Core",
                table: "TenantModule");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "Core",
                table: "TenantModule");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_TenantId_SubSystemId_Name",
                schema: "Core",
                table: "TenantModule",
                columns: new[] { "TenantId", "SubSystemId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantModule_TenantId_SubSystemId_Name",
                schema: "Core",
                table: "TenantModule");

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                schema: "Core",
                table: "TenantModule",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantModule",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_TenantId_ModuleId",
                schema: "Core",
                table: "TenantModule",
                columns: new[] { "TenantId", "ModuleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantModule_Module_ModuleId",
                schema: "Core",
                table: "TenantModule",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "Module",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
