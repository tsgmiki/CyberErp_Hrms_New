using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class TenantModuleAndOperationGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantModule",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Filter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantModule_Module_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "Core",
                        principalTable: "Module",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantModule_Subsystem_SubSystemId",
                        column: x => x.SubSystemId,
                        principalSchema: "Core",
                        principalTable: "Subsystem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_ModuleId",
                schema: "Core",
                table: "TenantOperation",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantModule",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_SubSystemId",
                schema: "Core",
                table: "TenantModule",
                column: "SubSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_TenantId_ModuleId",
                schema: "Core",
                table: "TenantModule",
                columns: new[] { "TenantId", "ModuleId" },
                unique: true);


            // ---- DATA MIGRATION -------------------------------------------------------------
            // Order matters and the scaffold got it wrong: it made ModuleId NOT NULL before the
            // rows to point at existed. Groups have to be MOVED first, then the column tightened.

            // 1. Every tenant group row (null ModuleId) becomes a TenantModule. The group keeps
            //    its own Id, so nothing else is re-keyed, and ModuleId is its OperationId - which
            //    IS the template module id, because a parent operation and its module have shared
            //    an Id since 2026-08-13.
            migrationBuilder.Sql(@"
INSERT INTO Core.TenantModule
    (Id, SubSystemId, ModuleId, Name, Icon, DisplayOrder, IsActive, Filter, TenantId,
     CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, RowVersion)
SELECT t.Id, t.SubSystemId, t.OperationId, t.Name, t.Icon, t.DisplayOrder, t.IsActive, t.Filter,
       t.TenantId, t.CreatedAt, t.UpdatedAt, t.CreatedBy, t.UpdatedBy, t.RowVersion
FROM Core.TenantOperation t
WHERE t.ModuleId IS NULL
  AND EXISTS (SELECT 1 FROM Core.Module m WHERE m.Id = t.OperationId);");

            // 2. Screens repoint from the TEMPLATE module id to this tenant's own group row.
            migrationBuilder.Sql(@"
UPDATE o
SET o.ModuleId = tm.Id
FROM Core.TenantOperation o
JOIN Core.TenantModule tm
  ON tm.ModuleId = o.ModuleId AND tm.TenantId = o.TenantId
WHERE o.ModuleId IS NOT NULL;");

            // 3. The group rows have served their purpose in TenantOperation. They hold no grants
            //    (verified: 0), so nothing references them.
            migrationBuilder.Sql(@"DELETE FROM Core.TenantOperation WHERE ModuleId IS NULL;");

            // 4. Same for the 24 template group rows, now that no tenant copy points at them.
            //    Must follow step 3: TenantOperation.OperationId has an FK to these rows.
            migrationBuilder.Sql(@"DELETE FROM Core.Operation WHERE ModuleId IS NULL;");

            // 5. A screen whose group did not survive would now be an orphan. Fail loudly rather
            //    than let the NOT NULL below silently turn it into an empty-Guid FK violation.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM Core.TenantOperation WHERE ModuleId IS NULL)
    THROW 51000, 'TenantOperation rows still have a null ModuleId - aborting.', 1;
IF EXISTS (SELECT 1 FROM Core.Operation WHERE ModuleId IS NULL)
    THROW 51000, 'Operation rows still have a null ModuleId - aborting.', 1;");

            // ---- Only now can the columns be tightened --------------------------------------
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "TenantOperation",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(7)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuleId",
                schema: "Core",
                table: "TenantOperation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_TenantOperation_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantOperation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "TenantModule",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantOperation_TenantModule_ModuleId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropTable(
                name: "TenantModule",
                schema: "Core");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_ModuleId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "TenantOperation",
                type: "datetime2(7)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuleId",
                schema: "Core",
                table: "TenantOperation",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuleId",
                schema: "Core",
                table: "Operation",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
