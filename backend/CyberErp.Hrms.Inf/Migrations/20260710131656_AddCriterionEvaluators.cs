using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddCriterionEvaluators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // REORDERED BY HAND: the child table is created and the existing single-evaluator data
            // is copied into it BEFORE the old columns drop — the scaffolded order lost the data.
            migrationBuilder.CreateTable(
                name: "hrms_CriterionEvaluator",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatorType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_CriterionEvaluator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_CriterionEvaluator_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_hrms_CriterionEvaluator_hrms_RequisitionScreeningCriterion_CriterionId",
                        column: x => x.CriterionId,
                        principalSchema: "Core",
                        principalTable: "hrms_RequisitionScreeningCriterion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CriterionEvaluator_CriterionId",
                schema: "Core",
                table: "hrms_CriterionEvaluator",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CriterionEvaluator_EmployeeId",
                schema: "Core",
                table: "hrms_CriterionEvaluator",
                column: "EmployeeId");

            // Preserve existing single-evaluator assignments as child rows before the columns drop.
            migrationBuilder.Sql("""
                INSERT INTO [Core].[hrms_CriterionEvaluator]
                    ([Id], [CriterionId], [EvaluatorType], [EmployeeId], [Name],
                     [TenantId], [CreatedAt], [RowVersion])
                SELECT NEWID(), c.[Id], c.[EvaluatorType], c.[EvaluatorEmployeeId],
                       ISNULL(c.[EvaluatorName], N'(unnamed evaluator)'),
                       c.[TenantId], SYSUTCDATETIME(), 0x0000000000000001
                FROM [Core].[hrms_RequisitionScreeningCriterion] c
                WHERE c.[EvaluatorType] <> N'None';
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_RequisitionScreeningCriterion_hrms_Employee_EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropIndex(
                name: "IX_hrms_RequisitionScreeningCriterion_EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropColumn(
                name: "EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropColumn(
                name: "EvaluatorName",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropColumn(
                name: "EvaluatorType",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_CriterionEvaluator",
                schema: "Core");

            migrationBuilder.AddColumn<Guid>(
                name: "EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvaluatorName",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvaluatorType",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_RequisitionScreeningCriterion_EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                column: "EvaluatorEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_RequisitionScreeningCriterion_hrms_Employee_EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                column: "EvaluatorEmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
