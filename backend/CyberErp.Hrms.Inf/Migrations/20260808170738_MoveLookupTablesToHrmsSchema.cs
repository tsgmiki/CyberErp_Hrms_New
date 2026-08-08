using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class MoveLookupTablesToHrmsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "LookUpCategoryList",
                schema: "Core",
                newName: "LookUpCategoryList",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "LookUpCategory",
                schema: "Core",
                newName: "LookUpCategory",
                newSchema: "Hrms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "LookUpCategoryList",
                schema: "Hrms",
                newName: "LookUpCategoryList",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "LookUpCategory",
                schema: "Hrms",
                newName: "LookUpCategory",
                newSchema: "Core");
        }
    }
}
