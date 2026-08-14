using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class ModuleOperationColumnParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Module_Subsystem_SubsystemId",
                schema: "Core",
                table: "Module");

            migrationBuilder.RenameColumn(
                name: "SubsystemId",
                schema: "Core",
                table: "Module",
                newName: "SubSystemId");

            migrationBuilder.RenameIndex(
                name: "IX_Module_SubsystemId",
                schema: "Core",
                table: "Module",
                newName: "IX_Module_SubSystemId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Operation",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(7)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Module",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(7)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Module_Subsystem_SubSystemId",
                schema: "Core",
                table: "Module",
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
                name: "FK_Module_Subsystem_SubSystemId",
                schema: "Core",
                table: "Module");

            migrationBuilder.RenameColumn(
                name: "SubSystemId",
                schema: "Core",
                table: "Module",
                newName: "SubsystemId");

            migrationBuilder.RenameIndex(
                name: "IX_Module_SubSystemId",
                schema: "Core",
                table: "Module",
                newName: "IX_Module_SubsystemId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Operation",
                type: "datetime2(7)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Module",
                type: "datetime2(7)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Module_Subsystem_SubsystemId",
                schema: "Core",
                table: "Module",
                column: "SubsystemId",
                principalSchema: "Core",
                principalTable: "Subsystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
