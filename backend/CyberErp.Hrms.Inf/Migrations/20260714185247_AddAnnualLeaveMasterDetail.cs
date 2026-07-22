using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualLeaveMasterDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_AnnualLeaveHeader",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnualLeaveLedgerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "date", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalLeaveDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_AnnualLeaveHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_AnnualLeaveHeader_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_AnnualLeaveHeader_hrms_LeaveBalance_AnnualLeaveLedgerId",
                        column: x => x.AnnualLeaveLedgerId,
                        principalSchema: "Core",
                        principalTable: "hrms_LeaveBalance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_AnnualLeaveDetail",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnualLeaveHeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveUsage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    LeaveDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_AnnualLeaveDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_AnnualLeaveDetail_hrms_AnnualLeaveHeader_AnnualLeaveHeaderId",
                        column: x => x.AnnualLeaveHeaderId,
                        principalSchema: "Core",
                        principalTable: "hrms_AnnualLeaveHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveDetail_AnnualLeaveHeaderId",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail",
                column: "AnnualLeaveHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail",
                columns: new[] { "AnnualLeaveHeaderId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveHeader_AnnualLeaveLedgerId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                column: "AnnualLeaveLedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveHeader_EmployeeId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveHeader_EmployeeId_Status",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                columns: new[] { "EmployeeId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_AnnualLeaveDetail",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_AnnualLeaveHeader",
                schema: "Core");
        }
    }
}
