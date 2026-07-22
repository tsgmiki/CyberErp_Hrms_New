using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_Candidate",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GrandFatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InternalEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EducationSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExperienceSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SkillsSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: true),
                    ResumeFileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ConsentGiven = table.Column<bool>(type: "bit", nullable: false),
                    ConsentAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    AnonymizedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    IsInTalentPool = table.Column<bool>(type: "bit", nullable: false),
                    TalentPoolNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_Candidate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_Candidate_hrms_Employee_InternalEmployeeId",
                        column: x => x.InternalEmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hrms_HiringRequest",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfPositions = table.Column<int>(type: "int", nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    JobRequirements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExpectedStartDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TimelineRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WorkforcePlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_HiringRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_HiringRequest_hrms_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_HiringRequest_hrms_PositionClass_PositionClassId",
                        column: x => x.PositionClassId,
                        principalSchema: "Core",
                        principalTable: "hrms_PositionClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_JobRequisition",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HiringRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumberOfPositions = table.Column<int>(type: "int", nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    MinQualifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MinExperienceYears = table.Column<int>(type: "int", nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SalaryScaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostingChannel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PostingText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    OpenFrom = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    OpenUntil = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_JobRequisition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_JobRequisition_coreSalaryScale_SalaryScaleId",
                        column: x => x.SalaryScaleId,
                        principalSchema: "Core",
                        principalTable: "coreSalaryScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_JobRequisition_hrms_HiringRequest_HiringRequestId",
                        column: x => x.HiringRequestId,
                        principalSchema: "Core",
                        principalTable: "hrms_HiringRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_JobRequisition_hrms_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_JobRequisition_hrms_PositionClass_PositionClassId",
                        column: x => x.PositionClassId,
                        principalSchema: "Core",
                        principalTable: "hrms_PositionClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_JobRequisition_hrms_WorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "Core",
                        principalTable: "hrms_WorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_JobApplication",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ScreeningScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ScreeningRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_JobApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_JobApplication_hrms_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "Core",
                        principalTable: "hrms_Candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_JobApplication_hrms_JobRequisition_RequisitionId",
                        column: x => x.RequisitionId,
                        principalSchema: "Core",
                        principalTable: "hrms_JobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_RequisitionScreeningCriterion",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_RequisitionScreeningCriterion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_RequisitionScreeningCriterion_hrms_JobRequisition_RequisitionId",
                        column: x => x.RequisitionId,
                        principalSchema: "Core",
                        principalTable: "hrms_JobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_JobApplicationStageLog",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_JobApplicationStageLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_JobApplicationStageLog_hrms_JobApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "Core",
                        principalTable: "hrms_JobApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Candidate_Email",
                schema: "Core",
                table: "hrms_Candidate",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Candidate_InternalEmployeeId",
                schema: "Core",
                table: "hrms_Candidate",
                column: "InternalEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Candidate_TenantId_CandidateNumber",
                schema: "Core",
                table: "hrms_Candidate",
                columns: new[] { "TenantId", "CandidateNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Candidate_TenantId_IsInTalentPool",
                schema: "Core",
                table: "hrms_Candidate",
                columns: new[] { "TenantId", "IsInTalentPool" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_HiringRequest_OrganizationUnitId",
                schema: "Core",
                table: "hrms_HiringRequest",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_HiringRequest_PositionClassId",
                schema: "Core",
                table: "hrms_HiringRequest",
                column: "PositionClassId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_HiringRequest_TenantId_RequestNumber",
                schema: "Core",
                table: "hrms_HiringRequest",
                columns: new[] { "TenantId", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_HiringRequest_TenantId_Status",
                schema: "Core",
                table: "hrms_HiringRequest",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobApplication_CandidateId_RequisitionId",
                schema: "Core",
                table: "hrms_JobApplication",
                columns: new[] { "CandidateId", "RequisitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobApplication_RequisitionId",
                schema: "Core",
                table: "hrms_JobApplication",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobApplication_TenantId_Stage",
                schema: "Core",
                table: "hrms_JobApplication",
                columns: new[] { "TenantId", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobApplicationStageLog_ApplicationId",
                schema: "Core",
                table: "hrms_JobApplicationStageLog",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobRequisition_HiringRequestId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "HiringRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobRequisition_OrganizationUnitId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobRequisition_PositionClassId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "PositionClassId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobRequisition_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "SalaryScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobRequisition_TenantId_RequisitionNumber",
                schema: "Core",
                table: "hrms_JobRequisition",
                columns: new[] { "TenantId", "RequisitionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobRequisition_TenantId_Status",
                schema: "Core",
                table: "hrms_JobRequisition",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobRequisition_WorkLocationId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_RequisitionScreeningCriterion_RequisitionId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                column: "RequisitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_JobApplicationStageLog",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_RequisitionScreeningCriterion",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_JobApplication",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_Candidate",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_JobRequisition",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_HiringRequest",
                schema: "Core");
        }
    }
}
