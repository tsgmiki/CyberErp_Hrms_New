using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeCeilingPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromotedToGradeCode",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromotedToSalaryScaleId",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PromoteOnGradeCeiling",
                schema: "Hrms",
                table: "SalaryIncrementPolicy",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromotedToGradeCode",
                schema: "Hrms",
                table: "SalaryRevisionLine");

            migrationBuilder.DropColumn(
                name: "PromotedToSalaryScaleId",
                schema: "Hrms",
                table: "SalaryRevisionLine");

            migrationBuilder.DropColumn(
                name: "PromoteOnGradeCeiling",
                schema: "Hrms",
                table: "SalaryIncrementPolicy");
        }
    }
}
