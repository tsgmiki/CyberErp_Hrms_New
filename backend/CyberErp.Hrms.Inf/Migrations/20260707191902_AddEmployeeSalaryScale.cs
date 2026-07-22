using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSalaryScale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Employee_SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee",
                column: "SalaryScaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Employee_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Employee_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropIndex(
                name: "IX_hrms_Employee_SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropColumn(
                name: "SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee");
        }
    }
}
