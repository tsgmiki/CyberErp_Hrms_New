using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class OperationSubSystemIdAndDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropIndex(
                name: "IX_Operation_SubSystemId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropIndex(
                name: "IX_Operation_SubSystemId_ModuleId_DisplayOrder",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropColumn(
                name: "SubSystemId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuleId",
                schema: "Core",
                table: "Operation",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "Module",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "Core",
                table: "Module",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Filter",
                schema: "Core",
                table: "Module",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Core",
                table: "Module",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Operation_ModuleId_DisplayOrder",
                schema: "Core",
                table: "Operation",
                columns: new[] { "ModuleId", "DisplayOrder" });

            // ---- Leftover DEFAULT CONSTRAINTS that EF does not model -------------------------
            // Added by earlier NOT NULL alters (empty-Guid placeholders) and by HasDefaultValue.
            // SRMS has none of them, and EF will not drop what it never declared. Removed by
            // name-agnostic lookup so this works whatever SQL Server auto-named them.
            migrationBuilder.Sql(@"
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(dc.parent_object_id))
            + N'.' + QUOTENAME(OBJECT_NAME(dc.parent_object_id))
            + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE (dc.parent_object_id = OBJECT_ID('Core.Module')
        AND c.name IN ('SubSystemId','Icon','Filter','DisplayOrder','IsActive'))
   OR (dc.parent_object_id = OBJECT_ID('Core.Operation') AND c.name IN ('ModuleId','IsActive'));
IF @sql <> N'' EXEC sp_executesql @sql;");

            // SRMS spells Operation.IsActive's default ((1)); EF emits (CONVERT([bit],(1))). Same
            // value, different text — recreated so the catalogs match exactly.
            migrationBuilder.Sql(
                @"ALTER TABLE Core.Operation ADD CONSTRAINT DF_Operation_IsActive DEFAULT ((1)) FOR IsActive;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Operation_ModuleId_DisplayOrder",
                schema: "Core",
                table: "Operation");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuleId",
                schema: "Core",
                table: "Operation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubSystemId",
                schema: "Core",
                table: "Operation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "Module",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "Core",
                table: "Module",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Filter",
                schema: "Core",
                table: "Module",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Core",
                table: "Module",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

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
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation",
                column: "SubSystemId",
                principalSchema: "Core",
                principalTable: "Subsystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
