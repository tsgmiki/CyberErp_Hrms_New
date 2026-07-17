using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_AppraisalTemplate",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GoalsWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CompetenciesWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_hrms_AppraisalTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_CompetencyCategory",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_hrms_CompetencyCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_RatingScale",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ScoreType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_RatingScale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_Competency",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompetencyCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_hrms_Competency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_Competency_hrms_CompetencyCategory_CompetencyCategoryId",
                        column: x => x.CompetencyCategoryId,
                        principalSchema: "Core",
                        principalTable: "hrms_CompetencyCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_RatingScaleLevel",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RatingScaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    MaxScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
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
                    table.PrimaryKey("PK_hrms_RatingScaleLevel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_RatingScaleLevel_hrms_RatingScale_RatingScaleId",
                        column: x => x.RatingScaleId,
                        principalSchema: "Core",
                        principalTable: "hrms_RatingScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ReviewCycle",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RatingScaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    SelfReviewDue = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ManagerReviewDue = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    EnableSelfAssessment = table.Column<bool>(type: "bit", nullable: false),
                    EnablePeerAssessment = table.Column<bool>(type: "bit", nullable: false),
                    EnableCalibration = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_hrms_ReviewCycle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReviewCycle_FiscalYear_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalSchema: "Core",
                        principalTable: "FiscalYear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_ReviewCycle_hrms_RatingScale_RatingScaleId",
                        column: x => x.RatingScaleId,
                        principalSchema: "Core",
                        principalTable: "hrms_RatingScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_PositionCompetency",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_hrms_PositionCompetency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_PositionCompetency_hrms_Competency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "Core",
                        principalTable: "hrms_Competency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_PositionCompetency_hrms_Position_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "Core",
                        principalTable: "hrms_Position",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalTemplate_TenantId_Name",
                schema: "Core",
                table: "hrms_AppraisalTemplate",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Competency_CompetencyCategoryId",
                schema: "Core",
                table: "hrms_Competency",
                column: "CompetencyCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Competency_TenantId_Name",
                schema: "Core",
                table: "hrms_Competency",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CompetencyCategory_TenantId_Name",
                schema: "Core",
                table: "hrms_CompetencyCategory",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_PositionCompetency_CompetencyId",
                schema: "Core",
                table: "hrms_PositionCompetency",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_PositionCompetency_PositionId_CompetencyId",
                schema: "Core",
                table: "hrms_PositionCompetency",
                columns: new[] { "PositionId", "CompetencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_RatingScale_TenantId_Name",
                schema: "Core",
                table: "hrms_RatingScale",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_RatingScaleLevel_RatingScaleId_Value",
                schema: "Core",
                table: "hrms_RatingScaleLevel",
                columns: new[] { "RatingScaleId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReviewCycle_FiscalYearId",
                schema: "Core",
                table: "hrms_ReviewCycle",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReviewCycle_RatingScaleId",
                schema: "Core",
                table: "hrms_ReviewCycle",
                column: "RatingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReviewCycle_TenantId_Name",
                schema: "Core",
                table: "hrms_ReviewCycle",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReviewCycle_TenantId_Status",
                schema: "Core",
                table: "hrms_ReviewCycle",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_AppraisalTemplate",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_PositionCompetency",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_RatingScaleLevel",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ReviewCycle",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_Competency",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_RatingScale",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_CompetencyCategory",
                schema: "Core");
        }
    }
}
