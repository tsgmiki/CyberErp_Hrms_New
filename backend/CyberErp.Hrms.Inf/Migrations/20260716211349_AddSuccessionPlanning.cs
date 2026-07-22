using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddSuccessionPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsCriticalPosition",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Criteria = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsCriticalPosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsCriticalPosition_hrmsPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTalentReview",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cycle = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTalentReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTalentReview_hrmsOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "dbo",
                        principalTable: "hrmsOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSuccessionPlan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriticalPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Horizon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSuccessionPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSuccessionPlan_hrmsCriticalPosition_CriticalPositionId",
                        column: x => x.CriticalPositionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCriticalPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTalentAssessment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceBand = table.Column<int>(type: "int", nullable: false),
                    PotentialBand = table.Column<int>(type: "int", nullable: false),
                    IsHiPo = table.Column<bool>(type: "bit", nullable: false),
                    Readiness = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTalentAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTalentAssessment_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsTalentAssessment_hrmsTalentReview_TalentReviewId",
                        column: x => x.TalentReviewId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTalentReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSuccessionCandidate",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuccessionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Readiness = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReadinessScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    GapSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSuccessionCandidate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSuccessionCandidate_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsSuccessionCandidate_hrmsSuccessionPlan_SuccessionPlanId",
                        column: x => x.SuccessionPlanId,
                        principalSchema: "dbo",
                        principalTable: "hrmsSuccessionPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTalentRating",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaterEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaterRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PerformanceScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    PotentialScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTalentRating", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTalentRating_hrmsEmployee_RaterEmployeeId",
                        column: x => x.RaterEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsTalentRating_hrmsTalentAssessment_TalentAssessmentId",
                        column: x => x.TalentAssessmentId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTalentAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsKnowledgeTransfer",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuccessionCandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FromEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsKnowledgeTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsKnowledgeTransfer_hrmsEmployee_FromEmployeeId",
                        column: x => x.FromEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsKnowledgeTransfer_hrmsSuccessionCandidate_SuccessionCandidateId",
                        column: x => x.SuccessionCandidateId,
                        principalSchema: "dbo",
                        principalTable: "hrmsSuccessionCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSuccessionDevelopmentAction",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuccessionCandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MentorEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSuccessionDevelopmentAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSuccessionDevelopmentAction_hrmsEmployee_MentorEmployeeId",
                        column: x => x.MentorEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsSuccessionDevelopmentAction_hrmsSuccessionCandidate_SuccessionCandidateId",
                        column: x => x.SuccessionCandidateId,
                        principalSchema: "dbo",
                        principalTable: "hrmsSuccessionCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCriticalPosition_PositionId",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCriticalPosition_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCriticalPosition_TenantId_PositionId",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                columns: new[] { "TenantId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsKnowledgeTransfer_FromEmployeeId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer",
                column: "FromEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsKnowledgeTransfer_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer",
                column: "SuccessionCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionCandidate_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionCandidate_SuccessionPlanId_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                columns: new[] { "SuccessionPlanId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionCandidate_SuccessionPlanId_Rank",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                columns: new[] { "SuccessionPlanId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionCandidate_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionDevelopmentAction_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction",
                column: "MentorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionDevelopmentAction_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction",
                column: "SuccessionCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionPlan_CriticalPositionId",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                column: "CriticalPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionPlan_TenantId_CriticalPositionId",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                columns: new[] { "TenantId", "CriticalPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuccessionPlan_TenantId_Status",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTalentAssessment_EmployeeId",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTalentAssessment_TalentReviewId_EmployeeId",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                columns: new[] { "TalentReviewId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTalentAssessment_TenantId_TalentReviewId_PerformanceBand_PotentialBand",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                columns: new[] { "TenantId", "TalentReviewId", "PerformanceBand", "PotentialBand" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTalentRating_RaterEmployeeId",
                schema: "dbo",
                table: "hrmsTalentRating",
                column: "RaterEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTalentRating_TalentAssessmentId",
                schema: "dbo",
                table: "hrmsTalentRating",
                column: "TalentAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTalentReview_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTalentReview",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTalentReview_TenantId_Status",
                schema: "dbo",
                table: "hrmsTalentReview",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsKnowledgeTransfer",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSuccessionDevelopmentAction",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTalentRating",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSuccessionCandidate",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTalentAssessment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSuccessionPlan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTalentReview",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsCriticalPosition",
                schema: "dbo");
        }
    }
}
