using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class RenameNavigationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Operation_OperationId",
                schema: "Core",
                table: "RolePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Operation",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Module",
                schema: "Core",
                table: "Module");

            migrationBuilder.RenameTable(
                name: "Operation",
                schema: "Core",
                newName: "coreOperation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Module",
                schema: "Core",
                newName: "coreModule",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_Operation_ModuleId",
                schema: "dbo",
                table: "coreOperation",
                newName: "IX_coreOperation_ModuleId");

            migrationBuilder.AddColumn<Guid>(
                name: "SubsystemId",
                schema: "dbo",
                table: "coreModule",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Data migration: existing modules referenced their subsystem by NAME (old SubSystem
            // column). Create any subsystem rows that don't exist yet (per tenant), then resolve
            // SubsystemId from the name before the old column is dropped and the FK is created.
            migrationBuilder.Sql(@"
INSERT INTO [dbo].[coreSubsystem] ([Id], [Name], [Code], [SortOrder], [TenantId], [CreatedAt], [RowVersion])
SELECT NEWID(), m.[SubSystem], LEFT(m.[SubSystem], 50), 1, m.[TenantId], SYSUTCDATETIME(), 0x0000000000000000
FROM (SELECT DISTINCT [SubSystem], [TenantId] FROM [dbo].[coreModule]) m
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[coreSubsystem] s
    WHERE s.[TenantId] = m.[TenantId] AND s.[Name] = m.[SubSystem]);

UPDATE m SET m.[SubsystemId] = s.[Id]
FROM [dbo].[coreModule] m
JOIN [dbo].[coreSubsystem] s ON s.[TenantId] = m.[TenantId] AND s.[Name] = m.[SubSystem];");

            migrationBuilder.DropColumn(
                name: "SubSystem",
                schema: "dbo",
                table: "coreModule");

            migrationBuilder.AddPrimaryKey(
                name: "PK_coreOperation",
                schema: "dbo",
                table: "coreOperation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_coreModule",
                schema: "dbo",
                table: "coreModule",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_coreModule_SubsystemId",
                schema: "dbo",
                table: "coreModule",
                column: "SubsystemId");

            migrationBuilder.AddForeignKey(
                name: "FK_coreModule_coreSubsystem_SubsystemId",
                schema: "dbo",
                table: "coreModule",
                column: "SubsystemId",
                principalSchema: "dbo",
                principalTable: "coreSubsystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_coreOperation_coreModule_ModuleId",
                schema: "dbo",
                table: "coreOperation",
                column: "ModuleId",
                principalSchema: "dbo",
                principalTable: "coreModule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_coreOperation_OperationId",
                schema: "Core",
                table: "RolePermission",
                column: "OperationId",
                principalSchema: "dbo",
                principalTable: "coreOperation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_coreModule_coreSubsystem_SubsystemId",
                schema: "dbo",
                table: "coreModule");

            migrationBuilder.DropForeignKey(
                name: "FK_coreOperation_coreModule_ModuleId",
                schema: "dbo",
                table: "coreOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_coreOperation_OperationId",
                schema: "Core",
                table: "RolePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_coreOperation",
                schema: "dbo",
                table: "coreOperation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_coreModule",
                schema: "dbo",
                table: "coreModule");

            migrationBuilder.AddColumn<string>(
                name: "SubSystem",
                schema: "dbo",
                table: "coreModule",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Restore the by-name reference from the FK before the id column disappears.
            migrationBuilder.Sql(@"
UPDATE m SET m.[SubSystem] = s.[Name]
FROM [dbo].[coreModule] m
JOIN [dbo].[coreSubsystem] s ON s.[Id] = m.[SubsystemId];");

            migrationBuilder.DropIndex(
                name: "IX_coreModule_SubsystemId",
                schema: "dbo",
                table: "coreModule");

            migrationBuilder.DropColumn(
                name: "SubsystemId",
                schema: "dbo",
                table: "coreModule");

            migrationBuilder.RenameTable(
                name: "coreOperation",
                schema: "dbo",
                newName: "Operation",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "coreModule",
                schema: "dbo",
                newName: "Module",
                newSchema: "Core");

            migrationBuilder.RenameIndex(
                name: "IX_coreOperation_ModuleId",
                schema: "Core",
                table: "Operation",
                newName: "IX_Operation_ModuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Operation",
                schema: "Core",
                table: "Operation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Module",
                schema: "Core",
                table: "Module",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "Module",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Operation_OperationId",
                schema: "Core",
                table: "RolePermission",
                column: "OperationId",
                principalSchema: "Core",
                principalTable: "Operation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
