using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SrmsSubsystemDropSortOrderAndUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subsystem_Name",
                schema: "Core",
                table: "Subsystem");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "Core",
                table: "Subsystem");

            migrationBuilder.DropColumn(
                name: "Url",
                schema: "Core",
                table: "Subsystem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "Core",
                table: "Subsystem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                schema: "Core",
                table: "Subsystem",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subsystem_Name",
                schema: "Core",
                table: "Subsystem",
                column: "Name",
                unique: true);
        }
    }
}
