using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddAppraisalAppeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcknowledgmentStatus",
                schema: "Core",
                table: "hrms_Appraisal",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeSignature",
                schema: "Core",
                table: "hrms_Appraisal",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmployeeSignedAt",
                schema: "Core",
                table: "hrms_Appraisal",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerSignature",
                schema: "Core",
                table: "hrms_Appraisal",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerSignedAt",
                schema: "Core",
                table: "hrms_Appraisal",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hrms_AppraisalAppeal",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequestFollowUp = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_AppraisalAppeal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_AppraisalAppeal_hrms_Appraisal_AppraisalId",
                        column: x => x.AppraisalId,
                        principalSchema: "Core",
                        principalTable: "hrms_Appraisal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrms_AppraisalAppeal_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalAppeal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                column: "AppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalAppeal_EmployeeId",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AppraisalAppeal_TenantId_Status",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_AppraisalAppeal",
                schema: "Core");

            migrationBuilder.DropColumn(
                name: "AcknowledgmentStatus",
                schema: "Core",
                table: "hrms_Appraisal");

            migrationBuilder.DropColumn(
                name: "EmployeeSignature",
                schema: "Core",
                table: "hrms_Appraisal");

            migrationBuilder.DropColumn(
                name: "EmployeeSignedAt",
                schema: "Core",
                table: "hrms_Appraisal");

            migrationBuilder.DropColumn(
                name: "ManagerSignature",
                schema: "Core",
                table: "hrms_Appraisal");

            migrationBuilder.DropColumn(
                name: "ManagerSignedAt",
                schema: "Core",
                table: "hrms_Appraisal");
        }
    }
}
