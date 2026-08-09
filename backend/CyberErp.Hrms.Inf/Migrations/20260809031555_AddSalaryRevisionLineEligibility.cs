using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryRevisionLineEligibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthsOfService",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProrationFactor",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 1m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthsOfService",
                schema: "Hrms",
                table: "SalaryRevisionLine");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "Hrms",
                table: "SalaryRevisionLine");

            migrationBuilder.DropColumn(
                name: "ProrationFactor",
                schema: "Hrms",
                table: "SalaryRevisionLine");
        }
    }
}
