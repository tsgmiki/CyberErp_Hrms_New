using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Turns Core.Operation into the self-referencing menu tree SRMS uses:
    /// <c>ModuleId IS NULL</c> marks a PARENT (a group), and any other value names the parent that
    /// row hangs off. The 24 Core.Module rows are copied in as those parents.
    ///
    /// <para>⚠️ THE SCAFFOLD IS NOT ENOUGH. EF drops the old foreign key, widens the column and adds
    /// the self-reference — but adds it BEFORE the parent rows exist, so all 150 children would point
    /// at keys that are not in the table yet and the constraint cannot be created. The copy therefore
    /// has to happen in between.</para>
    ///
    /// <para>⚠️ Each parent REUSES ITS MODULE'S Id (verified beforehand: zero collisions with existing
    /// operation ids). That is what makes this migration cheap — the 150 children already hold those
    /// values in ModuleId, so not one of them needs repointing. It also establishes the invariant the
    /// entity and SeedDefaultMenu rely on: a parent operation and its module share an Id.</para>
    ///
    /// <para>Core.Module is NOT dropped: SubscriptionPlanModule and TenantSubscriptionAddOn have
    /// foreign keys into it. It simply stops being what navigation reads.</para>
    /// </summary>
    public partial class OperationParentChildHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Module_ModuleId",
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

            // Copy Core.Module in as the parent rows, keeping each module's own Id.
            // A parent carries NO Link: the permission gate skips rows with an empty link, which is
            // exactly what stops a group from being treated as a screen and granted access.
            migrationBuilder.Sql(@"
                INSERT INTO Core.Operation
                    (Id, ModuleId, SubSystemId, Name, Link, Filter, Icon, DisplayOrder, IsActive,
                     TenantId, CreatedAt, CreatedBy, RowVersion)
                SELECT m.Id,
                       NULL,                                   -- null parent => this row IS a parent
                       m.SubsystemId,
                       LEFT(m.Name, 100),
                       '', '',                                 -- no route, no filter
                       LEFT(ISNULL(m.Icon, ''), 100),
                       m.SortOrder,
                       1,
                       m.TenantId,
                       SYSUTCDATETIME(),
                       'OperationParentChildHierarchy',
                       0x0000000000000001
                  FROM Core.Module m
                 WHERE NOT EXISTS (SELECT 1 FROM Core.Operation o WHERE o.Id = m.Id);");

            // Only now can the self-reference be created: every child's ModuleId resolves to a row
            // that exists. NoAction rather than Cascade because SQL Server rejects a cascading
            // self-referencing foreign key outright.
            migrationBuilder.AddForeignKey(
                name: "FK_Operation_Operation_ModuleId",
                schema: "Core",
                table: "Operation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "Operation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Operation_ModuleId",
                schema: "Core",
                table: "Operation");

            // The parent rows have to go before ModuleId can be NOT NULL again — they are precisely
            // the rows holding null — and before the Module foreign key is restored, since their ids
            // are module ids and would otherwise be fine, but their own null ModuleId would not be.
            // Their tenant copies and grants go first: TenantOperation -> Operation is Restrict.
            migrationBuilder.Sql(@"
                DELETE trp
                  FROM Core.TenantRolePermission trp
                  JOIN Core.TenantOperation topx ON topx.Id = trp.TenantOperationId
                  JOIN Core.Operation o         ON o.Id = topx.OperationId
                 WHERE o.ModuleId IS NULL;

                DELETE topx
                  FROM Core.TenantOperation topx
                  JOIN Core.Operation o ON o.Id = topx.OperationId
                 WHERE o.ModuleId IS NULL;

                DELETE FROM Core.Operation WHERE ModuleId IS NULL;");

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
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "Module",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
