using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddAppraisals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_Appraisal",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GoalsWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CompetenciesWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SelfComments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    OverallScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    FinalRatingLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelfSubmittedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_Appraisal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_Appraisal_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_Appraisal_hrms_ReviewCycle_ReviewCycleId",
                        column: x => x.ReviewCycleId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReviewCycle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_AppraisalCompetency",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetencyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SelfScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    SelfComments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ManagerScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_hrms_AppraisalCompetency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_AppraisalCompetency_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_AppraisalGoal",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeGoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SelfScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    SelfComments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ManagerScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_hrms_AppraisalGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_AppraisalGoal_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Appraisal_EmployeeId",
                schema: "Core",
                table: "hrms_Appraisal",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Appraisal_ReviewCycleId",
                schema: "Core",
                table: "hrms_Appraisal",
                column: "ReviewCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Appraisal_TenantId_EmployeeId_ReviewCycleId",
                schema: "Core",
                table: "hrms_Appraisal",
                columns: new[] { "TenantId", "EmployeeId", "ReviewCycleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Appraisal_TenantId_ReviewCycleId_Stage",
                schema: "Core",
                table: "hrms_Appraisal",
                columns: new[] { "TenantId", "ReviewCycleId", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalCompetency_AppraisalId_SortOrder",
                schema: "Core",
                table: "hrms_AppraisalCompetency",
                columns: new[] { "AppraisalId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalGoal_AppraisalId_SortOrder",
                schema: "Core",
                table: "hrms_AppraisalGoal",
                columns: new[] { "AppraisalId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_AppraisalCompetency",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_AppraisalGoal",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_Appraisal",
                schema: "Core");
        }
    }
}
