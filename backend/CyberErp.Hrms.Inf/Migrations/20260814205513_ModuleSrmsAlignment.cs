using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// <para>Brings <c>Core.Module</c> to the cybererp_srms shape (2026-08-15), now that Operation
    /// points at it again: drops <c>TenantId</c>, renames <c>SortOrder</c> to <c>DisplayOrder</c>,
    /// adds <c>Filter</c> and <c>IsActive</c>, narrows Name/Icon to nvarchar(100) and makes Icon
    /// NOT NULL.</para>
    ///
    /// <para>Checked before applying: all 24 rows belong to a SINGLE tenant, so dropping TenantId
    /// needs none of the deduplication Subsystem will (HOME/HRMS are duplicated per tenant there).
    /// The longest module name is 29 characters, so narrowing loses nothing, and the one row with a
    /// blank Icon holds an empty string rather than NULL — NOT NULL applies without a data fix.</para>
    /// </summary>
    public partial class ModuleSrmsAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "Module");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                schema: "Core",
                table: "Module",
                newName: "DisplayOrder");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Core",
                table: "Module",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "Core",
                table: "Module",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Filter",
                schema: "Core",
                table: "Module",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "Module",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Filter",
                schema: "Core",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Core",
                table: "Module");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                schema: "Core",
                table: "Module",
                newName: "SortOrder");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Core",
                table: "Module",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "Core",
                table: "Module",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Core",
                table: "Module",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
