using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceDevelopmentPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_DevelopmentPlan",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_DevelopmentPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_DevelopmentPlan_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_DevelopmentPlan_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ImprovementPlan",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OutcomeNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OutcomeRecordedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ImprovementPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ImprovementPlan_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_ImprovementPlan_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_DevelopmentAction",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevelopmentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LearningIntervention = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TargetDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_DevelopmentAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_DevelopmentAction_hrms_Competency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "Core",
                        principalTable: "hrms_Competency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_DevelopmentAction_hrms_DevelopmentPlan_DevelopmentPlanId",
                        column: x => x.DevelopmentPlanId,
                        principalSchema: "Core",
                        principalTable: "hrms_DevelopmentPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_PipObjective",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_PipObjective", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_PipObjective_hrms_ImprovementPlan_PipId",
                        column: x => x.PipId,
                        principalSchema: "Core",
                        principalTable: "hrms_ImprovementPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_DevelopmentAction_CompetencyId",
                schema: "Core",
                table: "hrms_DevelopmentAction",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_DevelopmentAction_DevelopmentPlanId_SortOrder",
                schema: "Core",
                table: "hrms_DevelopmentAction",
                columns: new[] { "DevelopmentPlanId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_DevelopmentPlan_AppraisalId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                column: "AppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_DevelopmentPlan_EmployeeId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_DevelopmentPlan_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ImprovementPlan_AppraisalId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                column: "AppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ImprovementPlan_EmployeeId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ImprovementPlan_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_PipObjective_PipId_SortOrder",
                schema: "Core",
                table: "hrms_PipObjective",
                columns: new[] { "PipId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_DevelopmentAction",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_PipObjective",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_DevelopmentPlan",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ImprovementPlan",
                schema: "Core");
        }
    }
}
