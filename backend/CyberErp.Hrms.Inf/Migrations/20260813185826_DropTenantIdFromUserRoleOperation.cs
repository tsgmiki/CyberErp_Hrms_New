using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class DropTenantIdFromUserRoleOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⚠️ CARRY THE MEMBERSHIP ACROSS BEFORE DESTROYING THE EVIDENCE.
            //
            // TenantId IS the membership for these rows; once the column is gone there is no way to
            // work out which tenant a user belonged to. The seed built Core.TenantUser from UserRole,
            // so it only covers users who hold a ROLE — six accounts do not, and one of them
            // (`dagmawi`) is a live headoffice user. Dropping the column without this would leave
            // them as global identities attached to no tenant: gone from the Users screen, which
            // scopes through TenantUser, and unable to reach anything at all.
            migrationBuilder.Sql(@"
                INSERT INTO Core.TenantUser
                    (Id, OwningTenantId, UserId, Status, IsDefaultTenant, TenantId, CreatedAt, RowVersion)
                SELECT NEWID(), t.Id, u.Id, 'Active', 1, u.TenantId, SYSUTCDATETIME(), 0x0000000000000001
                  FROM Core.[User] u
                  JOIN Core.Tenant t ON CAST(t.Id AS nvarchar(64)) = u.TenantId
                 WHERE NOT EXISTS (SELECT 1 FROM Core.TenantUser tu WHERE tu.UserId = u.Id);");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "User");

            // Same for roles: every role must have an instance in the tenant that owned it, or the
            // Roles screen (which scopes through TenantRole) loses it. The seed covered all 8, but a
            // role created since would not be, and this migration has to stand on its own.
            migrationBuilder.Sql(@"
                INSERT INTO Core.TenantRole
                    (Id, OwningTenantId, SourceTemplateId, Code, Name, Description, IsCustomized,
                     TenantId, CreatedAt, RowVersion)
                SELECT NEWID(), t.Id, r.Id, LEFT(r.Code, 100), LEFT(r.Name, 200), '', 0,
                       r.TenantId, SYSUTCDATETIME(), 0x0000000000000001
                  FROM Core.Role r
                  JOIN Core.Tenant t ON CAST(t.Id AS nvarchar(64)) = r.TenantId
                 WHERE NOT EXISTS (SELECT 1 FROM Core.TenantRole tr WHERE tr.SourceTemplateId = r.Id);");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "Role");

            // Operations need no backfill: TenantOperation already mirrors all 174 one-to-one.
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "Operation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "Core",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "Core",
                table: "Role",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
