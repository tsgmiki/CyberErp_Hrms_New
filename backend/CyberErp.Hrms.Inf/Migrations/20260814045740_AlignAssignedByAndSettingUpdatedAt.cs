using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AlignAssignedByAndSettingUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⚠️ SQL Server cannot cast 'seed-tenant-authorization' to a uniqueidentifier, so the
            // ALTER below fails outright without this. The column only ever held provenance markers
            // — never a user id — so anything that is not already a Guid is dropped to null rather
            // than invented. TRY_CAST keeps a genuine id if one was ever written.
            migrationBuilder.Sql(@"
                UPDATE Core.TenantUserRole
                   SET AssignedBy = NULL
                 WHERE AssignedBy IS NOT NULL
                   AND TRY_CAST(AssignedBy AS uniqueidentifier) IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedBy",
                schema: "Core",
                table: "TenantUserRole",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            // ⚠️ Seed from CreatedAt before the column becomes NOT NULL. EF's scaffolded default is
            // 0001-01-01, which would stamp every existing row with a date that never happened —
            // "last updated when it was created" is at least true.
            migrationBuilder.Sql(@"
                UPDATE Core.Setting SET UpdatedAt = CreatedAt WHERE UpdatedAt IS NULL;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Setting",
                type: "datetime2(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2(7)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AssignedBy",
                schema: "Core",
                table: "TenantUserRole",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Setting",
                type: "datetime2(7)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(7)");
        }
    }
}
