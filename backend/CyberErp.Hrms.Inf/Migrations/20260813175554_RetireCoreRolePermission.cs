using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Drops Core.RolePermission. It has had no reader since the phase-2 flip (logic.md §12.4) — the
    /// runtime resolves permissions through Core.TenantRolePermission — and as of this change the Role
    /// Permissions screen writes that table directly, so nothing projects into it either.
    ///
    /// <para>⚠️ THIS DESTROYS 598 ROWS. They were proved redundant immediately beforehand: every
    /// user's effective (link, CanView, CanAdd, CanEdit, CanDelete, CanApprove) set was compared
    /// across both models in both directions — 70,852 grant rows each side, none lost, none gained.
    /// The Down() below rebuilds the table but CANNOT repopulate it; recovery is the backup
    /// CERP_before-retire-rolepermission, or re-deriving it from TenantRolePermission.</para>
    ///
    /// <para>Core.Role, Core.Operation and Core.UserRole all remain. Only the grant table moves.</para>
    /// </summary>
    public partial class RetireCoreRolePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guard, not decoration: if the tenant-scoped table were somehow empty while this one had
            // rows, dropping would delete the only copy of every permission in the system.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Core.RolePermission)
                   AND NOT EXISTS (SELECT 1 FROM Core.TenantRolePermission)
                    THROW 50000, 'Refusing to drop Core.RolePermission: Core.TenantRolePermission is empty, so this is the only copy of the permission data. Run seed-tenant-authorization.sql first.', 1;");

            migrationBuilder.DropTable(
                name: "RolePermission",
                schema: "Core");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePermission",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermission_Operation_OperationId",
                        column: x => x.OperationId,
                        principalSchema: "Core",
                        principalTable: "Operation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Core",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_OperationId",
                schema: "Core",
                table: "RolePermission",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleId",
                schema: "Core",
                table: "RolePermission",
                column: "RoleId");
        }
    }
}
