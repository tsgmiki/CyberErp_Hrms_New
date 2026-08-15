using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SrmsForeignKeyNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantOperation_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantRolePermission_TenantOperation_TenantOperationId",
                schema: "Core",
                table: "TenantRolePermission");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantNavigationOperation_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantOperation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "TenantModule",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantRolePermission_Operation_OperationId",
                schema: "Core",
                table: "TenantRolePermission",
                column: "TenantOperationId",
                principalSchema: "Core",
                principalTable: "TenantOperation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantNavigationOperation_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantRolePermission_Operation_OperationId",
                schema: "Core",
                table: "TenantRolePermission");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantOperation_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantOperation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "TenantModule",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantRolePermission_TenantOperation_TenantOperationId",
                schema: "Core",
                table: "TenantRolePermission",
                column: "TenantOperationId",
                principalSchema: "Core",
                principalTable: "TenantOperation",
                principalColumn: "Id");
        }
    }
}
