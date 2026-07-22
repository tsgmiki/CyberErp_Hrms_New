using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class TrainingDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsTrainingBudget",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTrainingBudget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingBudget_hrmsOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "dbo",
                        principalTable: "hrmsOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTrainingSession",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Venue = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TrainerType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrainerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MeetingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxParticipants = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrainerCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaterialsCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    VenueCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTrainingSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingSession_hrmsTrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTrainingEnrollment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingNeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttendancePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AssessmentScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CompletedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    FeedbackRating = table.Column<int>(type: "int", nullable: true),
                    FeedbackComments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTrainingEnrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingEnrollment_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingEnrollment_hrmsTrainingNeed_TrainingNeedId",
                        column: x => x.TrainingNeedId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingNeed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingEnrollment_hrmsTrainingSession_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingBudget_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTrainingBudget",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingBudget_TenantId_FiscalYear_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTrainingBudget",
                columns: new[] { "TenantId", "FiscalYear", "OrganizationUnitId" },
                unique: true,
                filter: "[OrganizationUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingEnrollment_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingEnrollment_TenantId_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                columns: new[] { "TenantId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingEnrollment_TenantId_TrainingSessionId_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                columns: new[] { "TenantId", "TrainingSessionId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingEnrollment_TrainingNeedId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                column: "TrainingNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingEnrollment_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                column: "TrainingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingSession_TenantId_StartDate",
                schema: "dbo",
                table: "hrmsTrainingSession",
                columns: new[] { "TenantId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingSession_TenantId_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingSession",
                columns: new[] { "TenantId", "TrainingCourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingSession_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingSession",
                column: "TrainingCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsTrainingBudget",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTrainingEnrollment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTrainingSession",
                schema: "dbo");
        }
    }
}
