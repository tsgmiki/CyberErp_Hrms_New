using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceCalibration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCalibrated",
                schema: "Core",
                table: "hrms_Appraisal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "hrms_AppraisalPeerReview",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeerEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_AppraisalPeerReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_AppraisalPeerReview_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrms_AppraisalPeerReview_hrms_Employee_PeerEmployeeId",
                        column: x => x.PeerEmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_CalibrationSession",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReviewCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_CalibrationSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_CalibrationSession_hrms_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_CalibrationSession_hrms_ReviewCycle_ReviewCycleId",
                        column: x => x.ReviewCycleId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReviewCycle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_PerformanceHistory",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_PerformanceHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_CalibrationItem",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalibrationSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    CalibratedScore = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Justification = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsAdjusted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_CalibrationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_CalibrationItem_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_CalibrationItem_hrms_CalibrationSession_CalibrationSessionId",
                        column: x => x.CalibrationSessionId,
                        principalSchema: "Core",
                        principalTable: "hrms_CalibrationSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalPeerReview_AppraisalId_PeerEmployeeId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview",
                columns: new[] { "AppraisalId", "PeerEmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalPeerReview_PeerEmployeeId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview",
                column: "PeerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CalibrationItem_AppraisalId",
                schema: "Core",
                table: "hrms_CalibrationItem",
                column: "AppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CalibrationItem_CalibrationSessionId",
                schema: "Core",
                table: "hrms_CalibrationItem",
                column: "CalibrationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CalibrationSession_OrganizationUnitId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CalibrationSession_ReviewCycleId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                column: "ReviewCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_CalibrationSession_TenantId_ReviewCycleId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                columns: new[] { "TenantId", "ReviewCycleId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_PerformanceHistory_TenantId_EntityType_EntityId",
                schema: "Core",
                table: "hrms_PerformanceHistory",
                columns: new[] { "TenantId", "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_AppraisalPeerReview",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_CalibrationItem",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_PerformanceHistory",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_CalibrationSession",
                schema: "Core");

            migrationBuilder.DropColumn(
                name: "IsCalibrated",
                schema: "Core",
                table: "hrms_Appraisal");
        }
    }
}
