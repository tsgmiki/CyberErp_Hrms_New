using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SrmsConstraintNamesAndAlternateKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * ⚠️ sp_rename, NOT DropPrimaryKey/AddPrimaryKey.
             *
             * EF scaffolds a rename as a drop followed by an add, and SQL Server refuses to drop a
             * primary key that foreign keys depend on:
             *   "The constraint 'PK_TenantModule' is being referenced by table 'TenantOperation'".
             * sp_rename changes the constraint's NAME in place, leaving every dependent foreign key
             * intact — which is what a rename actually means.
             *
             * ⚠️ ORDER MATTERS. SRMS calls Core.Subsystem's primary key PK_Module (a leftover from
             * when its SubSystem entity was named Module). Core.Module's own PK must therefore be
             * renamed away FIRST, or the second rename collides on a name already in use.
             */
            migrationBuilder.Sql(@"
EXEC sp_rename 'Core.PK_Module', 'PK_NavigationModule', 'OBJECT';
EXEC sp_rename 'Core.PK_Subsystem', 'PK_Module', 'OBJECT';
EXEC sp_rename 'Core.PK_Role', 'PK_StandardRoleTemplate', 'OBJECT';
EXEC sp_rename 'Core.PK_Setting', 'PK_SystemSetting', 'OBJECT';
EXEC sp_rename 'Core.PK_TenantModule', 'PK_TenantNavigationModule', 'OBJECT';
EXEC sp_rename 'Core.PK_TenantSubSystem', 'PK_TenantModuleEntitlement', 'OBJECT';");

            // The four alternate keys SRMS declares so composite foreign keys can target them.
            // Each is unique trivially, since it leads with the primary key column.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_Tenant_Id_OrganizationId')
    ALTER TABLE Core.Tenant ADD CONSTRAINT AK_Tenant_Id_OrganizationId UNIQUE (Id, OrganizationId);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_OrganizationSubscription_Id_OrganizationId')
    ALTER TABLE Core.OrganizationSubscription ADD CONSTRAINT AK_OrganizationSubscription_Id_OrganizationId UNIQUE (Id, OrganizationId);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_TenantRole_Id_TenantId')
    ALTER TABLE Core.TenantRole ADD CONSTRAINT AK_TenantRole_Id_TenantId UNIQUE (Id, TenantId);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_TenantUser_Id_TenantId')
    ALTER TABLE Core.TenantUser ADD CONSTRAINT AK_TenantUser_Id_TenantId UNIQUE (Id, TenantId);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_Tenant_Id_OrganizationId')
    ALTER TABLE Core.Tenant DROP CONSTRAINT AK_Tenant_Id_OrganizationId;
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_OrganizationSubscription_Id_OrganizationId')
    ALTER TABLE Core.OrganizationSubscription DROP CONSTRAINT AK_OrganizationSubscription_Id_OrganizationId;
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_TenantRole_Id_TenantId')
    ALTER TABLE Core.TenantRole DROP CONSTRAINT AK_TenantRole_Id_TenantId;
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'AK_TenantUser_Id_TenantId')
    ALTER TABLE Core.TenantUser DROP CONSTRAINT AK_TenantUser_Id_TenantId;");

            migrationBuilder.Sql(@"
EXEC sp_rename 'Core.PK_TenantModuleEntitlement', 'PK_TenantSubSystem', 'OBJECT';
EXEC sp_rename 'Core.PK_TenantNavigationModule', 'PK_TenantModule', 'OBJECT';
EXEC sp_rename 'Core.PK_SystemSetting', 'PK_Setting', 'OBJECT';
EXEC sp_rename 'Core.PK_StandardRoleTemplate', 'PK_Role', 'OBJECT';
EXEC sp_rename 'Core.PK_Module', 'PK_Subsystem', 'OBJECT';
EXEC sp_rename 'Core.PK_NavigationModule', 'PK_Module', 'OBJECT';");
        }
    }
}
