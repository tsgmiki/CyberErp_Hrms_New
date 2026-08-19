using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SrmsDropTenantRoleForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantRole_Role_RoleId",
                schema: "Core",
                table: "TenantRole");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantRolePermission_TenantRole_TenantRoleId",
                schema: "Core",
                table: "TenantRolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantUserRole_TenantRole_TenantRoleId",
                schema: "Core",
                table: "TenantUserRole");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantRole_Role_SourceTemplateId",
                schema: "Core",
                table: "TenantRole",
                column: "RoleId",
                principalSchema: "Core",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantRole_Role_SourceTemplateId",
                schema: "Core",
                table: "TenantRole");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantRole_Role_RoleId",
                schema: "Core",
                table: "TenantRole",
                column: "RoleId",
                principalSchema: "Core",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantRolePermission_TenantRole_TenantRoleId",
                schema: "Core",
                table: "TenantRolePermission",
                column: "TenantRoleId",
                principalSchema: "Core",
                principalTable: "TenantRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantUserRole_TenantRole_TenantRoleId",
                schema: "Core",
                table: "TenantUserRole",
                column: "TenantRoleId",
                principalSchema: "Core",
                principalTable: "TenantRole",
                principalColumn: "Id");
        }
    }
}
