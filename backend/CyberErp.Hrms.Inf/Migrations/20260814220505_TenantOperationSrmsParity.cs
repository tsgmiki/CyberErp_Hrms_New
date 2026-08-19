using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// <para>Brings <c>Core.TenantOperation</c> to the cybererp_srms shape (2026-08-15). SRMS
    /// NORMALISED the table onto its group: a screen's tenant and subsystem are its module's, so
    /// <c>TenantId</c>, <c>SubSystemId</c> and the template link <c>OperationId</c> all go.</para>
    ///
    /// <para>⚠️ Losing <c>TenantId</c> means this table can no longer be filtered by tenant at all.
    /// Every read must reach it through <c>TenantModule</c> or through a tenant-scoped grant — see
    /// the warning on <c>Repository.IsGlobalEntity</c>, which now lists it for that reason alone.</para>
    ///
    /// <para>⚠️ Losing <c>OperationId</c> means the projector re-keys on <b>(module, link)</b>. That
    /// is safe here — verified 0 duplicate pairs across the 144 rows — and link is what every
    /// permission check already matches on. Both SPAs report ids only as React keys; nothing joins
    /// on them (permissionGate, formPermissions, gridAction and useListPermissions all match links).</para>
    ///
    /// <para>No data moves. The columns are simply dropped.</para>
    /// </summary>
    public partial class TenantOperationSrmsParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantOperation_Operation_OperationId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_ModuleId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_OperationId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_TenantId_Link",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_TenantId_OperationId",
                schema: "Core",
                table: "TenantOperation");

            // ⚠️ Two constraints here were added in RAW SQL during the OwningTenantId work, so EF
            // does not model them and did not scaffold their removal: the FK on TenantId, and the
            // default on ModuleId left by the NOT NULL alter. Both block the DROP COLUMN below.
            // Looked up by name so this works whatever SQL Server auto-named them.
            migrationBuilder.Sql(@"
DECLARE @fk sysname;
SELECT @fk = fk.name FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fk.parent_object_id = OBJECT_ID('Core.TenantOperation') AND c.name = 'TenantId';
IF @fk IS NOT NULL EXEC('ALTER TABLE Core.TenantOperation DROP CONSTRAINT [' + @fk + ']');");

            // The NOT NULL alter in the previous migration left a default constraint EF does not
            // model and SRMS does not have. Dropped by name-agnostic lookup.
            migrationBuilder.Sql(@"
DECLARE @df sysname;
SELECT @df = dc.name FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('Core.TenantOperation') AND c.name = 'ModuleId';
IF @df IS NOT NULL EXEC('ALTER TABLE Core.TenantOperation DROP CONSTRAINT [' + @df + ']');");

            migrationBuilder.DropColumn(
                name: "OperationId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropColumn(
                name: "SubSystemId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.AlterColumn<string>(
                name: "Filter",
                schema: "Core",
                table: "TenantOperation",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_Link",
                schema: "Core",
                table: "TenantOperation",
                column: "Link");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_ModuleId_Link",
                schema: "Core",
                table: "TenantOperation",
                columns: new[] { "ModuleId", "Link" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_Link",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_ModuleId_Link",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.AlterColumn<string>(
                name: "Filter",
                schema: "Core",
                table: "TenantOperation",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                schema: "Core",
                table: "TenantOperation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubSystemId",
                schema: "Core",
                table: "TenantOperation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Core",
                table: "TenantOperation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_ModuleId",
                schema: "Core",
                table: "TenantOperation",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_OperationId",
                schema: "Core",
                table: "TenantOperation",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_TenantId_Link",
                schema: "Core",
                table: "TenantOperation",
                columns: new[] { "TenantId", "Link" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_TenantId_OperationId",
                schema: "Core",
                table: "TenantOperation",
                columns: new[] { "TenantId", "OperationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantOperation_Operation_OperationId",
                schema: "Core",
                table: "TenantOperation",
                column: "OperationId",
                principalSchema: "Core",
                principalTable: "Operation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
