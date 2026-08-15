using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class LoginTrailAndTenantModuleForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_LoginTrail_User_UserId",
                schema: "Core",
                table: "LoginTrail",
                column: "UserId",
                principalSchema: "Core",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            /*
             * TenantModule.TenantId -> Core.Tenant, matching SRMS's FK_TenantModule_Tenant_TenantId.
             *
             * ⚠️ RAW SQL because EF cannot model a relationship on a VALUE-CONVERTED property, and
             * TenantId is exactly that: a string in the CLR stored as uniqueidentifier (logic.md
             * §12.14). Same reason the OwningTenantId foreign keys went in by hand in §12.16.
             *
             * Verified before adding: 28 rows, 0 orphans.
             */
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TenantModule_Tenant_TenantId')
    ALTER TABLE Core.TenantModule ADD CONSTRAINT FK_TenantModule_Tenant_TenantId
        FOREIGN KEY (TenantId) REFERENCES Core.Tenant (Id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TenantModule_Tenant_TenantId')
    ALTER TABLE Core.TenantModule DROP CONSTRAINT FK_TenantModule_Tenant_TenantId;");

            migrationBuilder.DropForeignKey(
                name: "FK_LoginTrail_User_UserId",
                schema: "Core",
                table: "LoginTrail");
        }
    }
}
