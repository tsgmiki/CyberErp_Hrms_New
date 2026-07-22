using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class EngagementCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsAnnouncement",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublishFrom = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    PublishUntil = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_hrmsAnnouncement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsAnnouncement_hrmsBranch_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "dbo",
                        principalTable: "hrmsBranch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsAnnouncement_hrmsOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "dbo",
                        principalTable: "hrmsOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsGrievance",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsConfidential = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedToEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ResolvedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsGrievance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsGrievance_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSuggestion",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ManagementResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    RespondedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSuggestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsGrievanceNote",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrievanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsGrievanceNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsGrievanceNote_hrmsGrievance_GrievanceId",
                        column: x => x.GrievanceId,
                        principalSchema: "dbo",
                        principalTable: "hrmsGrievance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsAnnouncement_BranchId",
                schema: "dbo",
                table: "hrmsAnnouncement",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsAnnouncement_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsAnnouncement",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsAnnouncement_TenantId_IsActive_PublishFrom",
                schema: "dbo",
                table: "hrmsAnnouncement",
                columns: new[] { "TenantId", "IsActive", "PublishFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsGrievance_EmployeeId",
                schema: "dbo",
                table: "hrmsGrievance",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsGrievance_TenantId_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsGrievance",
                columns: new[] { "TenantId", "AssignedToEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsGrievance_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsGrievance",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsGrievance_TenantId_Status",
                schema: "dbo",
                table: "hrmsGrievance",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsGrievanceNote_GrievanceId",
                schema: "dbo",
                table: "hrmsGrievanceNote",
                column: "GrievanceId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsGrievanceNote_TenantId_GrievanceId",
                schema: "dbo",
                table: "hrmsGrievanceNote",
                columns: new[] { "TenantId", "GrievanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSuggestion_TenantId_Status",
                schema: "dbo",
                table: "hrmsSuggestion",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsAnnouncement",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsGrievanceNote",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSuggestion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsGrievance",
                schema: "dbo");
        }
    }
}
