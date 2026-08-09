using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddAffectsSalaryIncrement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AffectsSalaryIncrement",
                schema: "Hrms",
                table: "DisciplinaryMeasure",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffectsSalaryIncrement",
                schema: "Hrms",
                table: "DisciplinaryMeasure");
        }
    }
}
