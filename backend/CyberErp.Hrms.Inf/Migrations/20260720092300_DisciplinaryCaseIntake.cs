using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class DisciplinaryCaseIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrmsDisciplinaryMeasure_EmployeeId",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.AddColumn<bool>(
                name: "AffectsPromotion",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsReward",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RaisedByEmployeeId",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidUntil",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsDisciplinaryMeasure_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                columns: new[] { "EmployeeId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrmsDisciplinaryMeasure_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.DropColumn(
                name: "AffectsPromotion",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.DropColumn(
                name: "AffectsReward",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.DropColumn(
                name: "RaisedByEmployeeId",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.DropColumn(
                name: "ValidUntil",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsDisciplinaryMeasure_EmployeeId",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                column: "EmployeeId");
        }
    }
}
