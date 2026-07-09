using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentCandidateLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<Guid>(
                name: "HiredEmployeeId",
                schema: "Core",
                table: "hrms_Candidate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                schema: "Core",
                table: "hrms_Candidate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hrms_ApplicationCriterionScore",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ScoredBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ScoredAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ApplicationCriterionScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ApplicationCriterionScore_hrms_JobApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "Core",
                        principalTable: "hrms_JobApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_CandidateDocument",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_CandidateDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_CandidateDocument_hrms_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "Core",
                        principalTable: "hrms_Candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_RequisitionScreeningCriterion_EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                column: "EvaluatorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Candidate_HiredEmployeeId",
                schema: "Core",
                table: "hrms_Candidate",
                column: "HiredEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Candidate_PersonId",
                schema: "Core",
                table: "hrms_Candidate",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ApplicationCriterionScore_ApplicationId_CriterionId",
                schema: "Core",
                table: "hrms_ApplicationCriterionScore",
                columns: new[] { "ApplicationId", "CriterionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CandidateDocument_CandidateId_DocumentType",
                schema: "Core",
                table: "hrms_CandidateDocument",
                columns: new[] { "CandidateId", "DocumentType" });

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Candidate_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_Candidate",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Candidate_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_RequisitionScreeningCriterion_hrms_Employee_EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropTable(
                name: "hrms_ApplicationCriterionScore",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_CandidateDocument",
                schema: "Core");

            migrationBuilder.DropIndex(
                name: "IX_hrms_RequisitionScreeningCriterion_EvaluatorEmployeeId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropIndex(
                name: "IX_hrms_Candidate_HiredEmployeeId",
                schema: "Core",
                table: "hrms_Candidate");

            migrationBuilder.DropIndex(
                name: "IX_hrms_Candidate_PersonId",
                schema: "Core",
                table: "hrms_Candidate");

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

            migrationBuilder.DropColumn(
                name: "HiredEmployeeId",
                schema: "Core",
                table: "hrms_Candidate");

            migrationBuilder.DropColumn(
                name: "PersonId",
                schema: "Core",
                table: "hrms_Candidate");
        }
    }
}
