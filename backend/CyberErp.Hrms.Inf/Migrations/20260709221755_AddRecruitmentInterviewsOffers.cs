using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentInterviewsOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_Interview",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_hrms_Interview", x => x.Id);
                    table.CheckConstraint("CK_hrms_Interview_Window", "[ScheduledEnd] > [ScheduledStart]");
                    table.ForeignKey(
                        name: "FK_hrms_Interview_hrms_JobApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "Core",
                        principalTable: "hrms_JobApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_JobOffer",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HiringManagerEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HiringManagerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalaryScaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalaryJustification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProposedStartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ResponseNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LetterText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    HiredEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_JobOffer", x => x.Id);
                    table.CheckConstraint("CK_hrms_JobOffer_Salary", "[Salary] > 0");
                    table.ForeignKey(
                        name: "FK_hrms_JobOffer_coreSalaryScale_SalaryScaleId",
                        column: x => x.SalaryScaleId,
                        principalSchema: "Core",
                        principalTable: "coreSalaryScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_JobOffer_hrms_Employee_HiringManagerEmployeeId",
                        column: x => x.HiringManagerEmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_hrms_JobOffer_hrms_JobApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "Core",
                        principalTable: "hrms_JobApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_NumberSequence",
                schema: "Core",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_NumberSequence", x => new { x.TenantId, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "hrms_InterviewPanelist",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InterviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PanelistName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsLead = table.Column<bool>(type: "bit", nullable: false),
                    Attendance = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_InterviewPanelist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_InterviewPanelist_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_hrms_InterviewPanelist_hrms_Interview_InterviewId",
                        column: x => x.InterviewId,
                        principalSchema: "Core",
                        principalTable: "hrms_Interview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_InterviewFeedback",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PanelistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CriterionName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_InterviewFeedback", x => x.Id);
                    table.CheckConstraint("CK_hrms_InterviewFeedback_Score", "[Score] >= 0 AND [Score] <= 100");
                    table.ForeignKey(
                        name: "FK_hrms_InterviewFeedback_hrms_InterviewPanelist_PanelistId",
                        column: x => x.PanelistId,
                        principalSchema: "Core",
                        principalTable: "hrms_InterviewPanelist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Interview_ApplicationId",
                schema: "Core",
                table: "hrms_Interview",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Interview_ScheduledStart",
                schema: "Core",
                table: "hrms_Interview",
                column: "ScheduledStart");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Interview_TenantId_Status",
                schema: "Core",
                table: "hrms_Interview",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_InterviewFeedback_PanelistId",
                schema: "Core",
                table: "hrms_InterviewFeedback",
                column: "PanelistId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_InterviewFeedback_PanelistId_CriterionId",
                schema: "Core",
                table: "hrms_InterviewFeedback",
                columns: new[] { "PanelistId", "CriterionId" },
                unique: true,
                filter: "[CriterionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_InterviewPanelist_EmployeeId",
                schema: "Core",
                table: "hrms_InterviewPanelist",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_InterviewPanelist_InterviewId_EmployeeId",
                schema: "Core",
                table: "hrms_InterviewPanelist",
                columns: new[] { "InterviewId", "EmployeeId" },
                unique: true,
                filter: "[EmployeeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobOffer_Application_Active",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "ApplicationId",
                unique: true,
                filter: "[Status] IN ('Draft','PendingApproval','Approved','Sent')");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobOffer_HiredEmployeeId",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "HiredEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobOffer_HiringManagerEmployeeId",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "HiringManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobOffer_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "SalaryScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobOffer_TenantId_OfferNumber",
                schema: "Core",
                table: "hrms_JobOffer",
                columns: new[] { "TenantId", "OfferNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobOffer_TenantId_Status",
                schema: "Core",
                table: "hrms_JobOffer",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_InterviewFeedback",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_JobOffer",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_NumberSequence",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_InterviewPanelist",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_Interview",
                schema: "Core");
        }
    }
}
