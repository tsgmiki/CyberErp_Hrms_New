using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class JobGradeRefactorAndSalaryScale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Core",
                table: "hrms_JobGrade");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Core",
                table: "hrms_JobGrade");

            migrationBuilder.DropColumn(
                name: "Level",
                schema: "Core",
                table: "hrms_JobGrade");

            migrationBuilder.DropColumn(
                name: "MaxSalary",
                schema: "Core",
                table: "hrms_JobGrade");

            migrationBuilder.DropColumn(
                name: "MinSalary",
                schema: "Core",
                table: "hrms_JobGrade");

            migrationBuilder.AddColumn<string>(
                name: "NameA",
                schema: "Core",
                table: "hrms_JobGrade",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "lupStep",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lupStep", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "coreSalaryScale",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coreSalaryScale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coreSalaryScale_hrms_JobGrade_JobGradeId",
                        column: x => x.JobGradeId,
                        principalSchema: "Core",
                        principalTable: "hrms_JobGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_coreSalaryScale_lupStep_StepId",
                        column: x => x.StepId,
                        principalSchema: "Core",
                        principalTable: "lupStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coreSalaryScale_JobGradeId",
                schema: "Core",
                table: "coreSalaryScale",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_coreSalaryScale_StepId",
                schema: "Core",
                table: "coreSalaryScale",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_coreSalaryScale_TenantId_JobGradeId_StepId",
                schema: "Core",
                table: "coreSalaryScale",
                columns: new[] { "TenantId", "JobGradeId", "StepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lupStep_TenantId_Code",
                schema: "Core",
                table: "lupStep",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coreSalaryScale",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "lupStep",
                schema: "Core");

            migrationBuilder.DropColumn(
                name: "NameA",
                schema: "Core",
                table: "hrms_JobGrade");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Core",
                table: "hrms_JobGrade",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "hrms_JobGrade",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                schema: "Core",
                table: "hrms_JobGrade",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxSalary",
                schema: "Core",
                table: "hrms_JobGrade",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinSalary",
                schema: "Core",
                table: "hrms_JobGrade",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
