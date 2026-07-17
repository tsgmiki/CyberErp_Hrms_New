using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsCareerPath",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_hrmsCareerPath", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsMentorship",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenteeEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RefId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
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
                    table.PrimaryKey("PK_hrmsMentorship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsMentorship_hrmsEmployee_MenteeEmployeeId",
                        column: x => x.MenteeEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsMentorship_hrmsEmployee_MentorEmployeeId",
                        column: x => x.MentorEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsCareerPathChangeRequest",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentCareerPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestedCareerPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsCareerPathChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathChangeRequest_hrmsCareerPath_CurrentCareerPathId",
                        column: x => x.CurrentCareerPathId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCareerPath",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathChangeRequest_hrmsCareerPath_RequestedCareerPathId",
                        column: x => x.RequestedCareerPathId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCareerPath",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathChangeRequest_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsCareerPathStep",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareerPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PositionClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequiredExperienceMonths = table.Column<int>(type: "int", nullable: true),
                    Certifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsCareerPathStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathStep_hrmsCareerPath_CareerPathId",
                        column: x => x.CareerPathId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCareerPath",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathStep_hrmsJobGrade_JobGradeId",
                        column: x => x.JobGradeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsJobGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathStep_hrmsPositionClass_PositionClassId",
                        column: x => x.PositionClassId,
                        principalSchema: "dbo",
                        principalTable: "hrmsPositionClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsEmployeeCareerPath",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareerPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ProgressPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_hrmsEmployeeCareerPath", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeCareerPath_hrmsCareerPath_CareerPathId",
                        column: x => x.CareerPathId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCareerPath",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeCareerPath_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsCareerPathStepCompetency",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareerPathStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsCareerPathStepCompetency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathStepCompetency_hrmsCareerPathStep_CareerPathStepId",
                        column: x => x.CareerPathStepId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCareerPathStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsCareerPathStepCompetency_hrmsCompetency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCompetency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsEmployeeCareerPathStepProgress",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCareerPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareerPathStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsEmployeeCareerPathStepProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeCareerPathStepProgress_hrmsEmployeeCareerPath_EmployeeCareerPathId",
                        column: x => x.EmployeeCareerPathId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployeeCareerPath",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPath_TenantId_Code",
                schema: "dbo",
                table: "hrmsCareerPath",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathChangeRequest_CurrentCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                column: "CurrentCareerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathChangeRequest_EmployeeId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathChangeRequest_RequestedCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                column: "RequestedCareerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathChangeRequest_TenantId_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                columns: new[] { "TenantId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathStep_CareerPathId_StepOrder",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                columns: new[] { "CareerPathId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathStep_JobGradeId",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathStep_PositionClassId",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                column: "PositionClassId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathStepCompetency_CareerPathStepId_CompetencyId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency",
                columns: new[] { "CareerPathStepId", "CompetencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCareerPathStepCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeCareerPath_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                column: "CareerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeCareerPath_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeCareerPath_TenantId_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                columns: new[] { "TenantId", "CareerPathId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeCareerPath_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeCareerPath_TenantId_EmployeeId_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                columns: new[] { "TenantId", "EmployeeId", "CareerPathId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeCareerPathStepProgress_EmployeeCareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPathStepProgress",
                column: "EmployeeCareerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMentorship_MenteeEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                column: "MenteeEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMentorship_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                column: "MentorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMentorship_TenantId_MenteeEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                columns: new[] { "TenantId", "MenteeEmployeeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsCareerPathChangeRequest",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsCareerPathStepCompetency",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsEmployeeCareerPathStepProgress",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsMentorship",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsCareerPathStep",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsEmployeeCareerPath",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsCareerPath",
                schema: "dbo");
        }
    }
}
