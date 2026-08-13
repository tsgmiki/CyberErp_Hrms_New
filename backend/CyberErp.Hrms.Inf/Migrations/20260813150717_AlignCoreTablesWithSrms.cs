using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Brings Core.User, Core.Role and Core.Operation into line with the cybererp_srms platform schema.
    ///
    /// <para>⚠️ THE SCAFFOLD IS NOT ENOUGH ON ITS OWN. EF generates the column changes and the indexes
    /// in dependency order, not in DATA order, so three of them fail or corrupt against real rows:</para>
    /// <list type="number">
    /// <item>the unique index on NormalizedUserName is created while all 506 rows still hold '' —
    ///       every one of them collides;</item>
    /// <item>SubSystemId is added defaulting to an empty Guid and a foreign key to Core.Subsystem is
    ///       put on top of it — no subsystem has that id, so the constraint cannot be created;</item>
    /// <item>Role.Code becomes NOT NULL with a '' default, silently turning five real roles into
    ///       blank-coded ones.</item>
    /// </list>
    /// <para>Each is therefore preceded by an explicit backfill below. The three tables keep their
    /// TenantId column, which SRMS does not have — see logic.md §12.5 for why.</para>
    /// </summary>
    public partial class AlignCoreTablesWithSrms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                schema: "Core",
                table: "User",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                schema: "Core",
                table: "Operation",
                newName: "DisplayOrder");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                schema: "Core",
                table: "User",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                schema: "Core",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlatformAdministrator",
                schema: "Core",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndUtc",
                schema: "Core",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                schema: "Core",
                table: "User",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                schema: "Core",
                table: "User",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePicture",
                schema: "Core",
                table: "User",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureContentType",
                schema: "Core",
                table: "User",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                schema: "Core",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // BACKFILL 1 — the normalised lookup columns, before their unique indexes exist.
            // Without this every row holds '' and IX_User_NormalizedUserName cannot be created.
            migrationBuilder.Sql(@"
                UPDATE Core.[User]
                   SET NormalizedUserName = UPPER(LTRIM(RTRIM(UserName))),
                       NormalizedEmail    = UPPER(LTRIM(RTRIM(ISNULL(Email, ''))));");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Core",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            // BACKFILL 2 — a real code for the five roles that have none, derived from the name the
            // same way Role.DeriveCode does ("HR Officer" -> "HR-OFFICER"). Runs BEFORE the column
            // becomes NOT NULL, otherwise the '' default would erase what those roles are called.
            migrationBuilder.Sql(@"
                UPDATE Core.Role
                   SET Code = LEFT(UPPER(REPLACE(LTRIM(RTRIM(Name)), ' ', '-')), 80)
                 WHERE Code IS NULL OR LTRIM(RTRIM(Code)) = '';");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Core",
                table: "Role",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Core",
                table: "Role",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlatformRole",
                schema: "Core",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Filter",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "Operation",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubSystemId",
                schema: "Core",
                table: "Operation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // BACKFILL 3 — resolve every operation's subsystem THROUGH ITS MODULE, before the foreign
            // key goes on. The scaffolded default is an empty Guid, which matches no subsystem, so
            // FK_Operation_Subsystem_SubSystemId would be rejected outright without this.
            migrationBuilder.Sql(@"
                UPDATE o
                   SET o.SubSystemId = m.SubsystemId
                  FROM Core.Operation o
                  JOIN Core.Module m ON m.Id = o.ModuleId;");

            migrationBuilder.CreateIndex(
                name: "IX_User_NormalizedEmail",
                schema: "Core",
                table: "User",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_User_NormalizedUserName",
                schema: "Core",
                table: "User",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Code",
                schema: "Core",
                table: "Role",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Operation_SubSystemId",
                schema: "Core",
                table: "Operation",
                column: "SubSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Operation_SubSystemId_ModuleId_DisplayOrder",
                schema: "Core",
                table: "Operation",
                columns: new[] { "SubSystemId", "ModuleId", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Operation_Subsystem_SubSystemId",
                schema: "Core",
                table: "Operation",
                column: "SubSystemId",
                principalSchema: "Core",
                principalTable: "Subsystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Subsystem_SubSystemId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropIndex(
                name: "IX_User_NormalizedEmail",
                schema: "Core",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_NormalizedUserName",
                schema: "Core",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Role_Code",
                schema: "Core",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Operation_SubSystemId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropIndex(
                name: "IX_Operation_SubSystemId_ModuleId_DisplayOrder",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsPlatformAdministrator",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ProfilePictureContentType",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                schema: "Core",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Core",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Core",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsPlatformRole",
                schema: "Core",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropColumn(
                name: "SubSystemId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                schema: "Core",
                table: "User",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                schema: "Core",
                table: "Operation",
                newName: "SortOrder");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Core",
                table: "Role",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Core",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Filter",
                schema: "Core",
                table: "Operation",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
