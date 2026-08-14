using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// <para>Points <c>Core.Operation.ModuleId</c> back at <c>Core.Module</c>, matching cybererp_srms
    /// (2026-08-15). On 2026-08-13 it was made self-referencing — a group was an Operation with a null
    /// ModuleId — because that is what SRMS looked like then. SRMS has since been corrected.</para>
    ///
    /// <para><b>No data change is needed.</b> The 2026-08-13 migration copied the 24 modules into
    /// Operation USING THEIR OWN Ids, so every parent operation's Id already equals its module's, and
    /// all 144 child rows were already pointing at a valid Core.Module. Verified before applying:
    /// 144 of 144 ModuleId values exist in Core.Module, 0 missing.</para>
    ///
    /// <para>⚠️ Both constraint NAMES are SRMS's verbatim, per the identical-structure requirement.
    /// <c>FK_Operation_Module_ModuleId</c> constrains <c>SubSystemId</c>, not ModuleId — a misnomer
    /// left over from a rename in SRMS. Its CASCADE is also SRMS's: deleting a subsystem now takes its
    /// menu with it, where CERP previously refused with Restrict.</para>
    ///
    /// <para>The 24 group rows still exist with a null ModuleId; SRMS has none and its column is NOT
    /// NULL. Removing them is a separate step — the sidebar still reads groups from the tenant
    /// copies — so ModuleId stays nullable here.</para>
    /// </summary>
    public partial class OperationModuleForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Operation_ModuleId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Subsystem_SubSystemId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.AddForeignKey(
                name: "FK_NavigationOperation_Module_ModuleId",
                schema: "Core",
                table: "Operation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "Module",
                principalColumn: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NavigationOperation_Module_ModuleId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.AddForeignKey(
                name: "FK_Operation_Operation_ModuleId",
                schema: "Core",
                table: "Operation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "Operation",
                principalColumn: "Id");

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
    }
}
