using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceRecognition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_Achievement",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AchievementDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_Achievement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_Achievement_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_Achievement_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_RecognitionBadge",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_hrms_RecognitionBadge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_EmployeeRecognition",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecognitionBadgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Citation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RecognizedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_EmployeeRecognition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_EmployeeRecognition_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_EmployeeRecognition_hrms_RecognitionBadge_RecognitionBadgeId",
                        column: x => x.RecognitionBadgeId,
                        principalSchema: "Core",
                        principalTable: "hrms_RecognitionBadge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Achievement_AppraisalId",
                schema: "Core",
                table: "hrms_Achievement",
                column: "AppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Achievement_EmployeeId",
                schema: "Core",
                table: "hrms_Achievement",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_Achievement_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_Achievement",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeRecognition_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeRecognition_RecognitionBadgeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                column: "RecognitionBadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeRecognition_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeRecognition_TenantId_IsPublic",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                columns: new[] { "TenantId", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_RecognitionBadge_TenantId_Name",
                schema: "Core",
                table: "hrms_RecognitionBadge",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_Achievement",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_EmployeeRecognition",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_RecognitionBadge",
                schema: "Core");
        }
    }
}
