using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class LeaveRequestMasterDetail_WorkWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveRequest_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropIndex(
                name: "IX_hrms_LeaveRequest_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropColumn(
                name: "DayPart",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.RenameColumn(
                name: "WorkingDays",
                schema: "Core",
                table: "hrms_LeaveRequest",
                newName: "TotalWorkingDays");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "Core",
                table: "hrms_LeaveRequest",
                newName: "SubmittedDate");

            migrationBuilder.CreateTable(
                name: "hrms_LeaveRequestLine",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    DayPart = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkingDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_LeaveRequestLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_LeaveRequestLine_hrms_LeaveRequest_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalSchema: "Core",
                        principalTable: "hrms_LeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrms_LeaveRequestLine_hrms_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "Core",
                        principalTable: "hrms_LeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_WorkWeekConfiguration",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Monday = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Tuesday = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Wednesday = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Thursday = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Friday = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Saturday = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Sunday = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_hrms_WorkWeekConfiguration", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveRequestLine_LeaveRequestId",
                schema: "Core",
                table: "hrms_LeaveRequestLine",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveRequestLine_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequestLine",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkWeekConfiguration_TenantId_IsActive",
                schema: "Core",
                table: "hrms_WorkWeekConfiguration",
                columns: new[] { "TenantId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_LeaveRequestLine",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_WorkWeekConfiguration",
                schema: "Core");

            migrationBuilder.RenameColumn(
                name: "TotalWorkingDays",
                schema: "Core",
                table: "hrms_LeaveRequest",
                newName: "WorkingDays");

            migrationBuilder.RenameColumn(
                name: "SubmittedDate",
                schema: "Core",
                table: "hrms_LeaveRequest",
                newName: "StartDate");

            migrationBuilder.AddColumn<string>(
                name: "DayPart",
                schema: "Core",
                table: "hrms_LeaveRequest",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "Core",
                table: "hrms_LeaveRequest",
                type: "datetime2(3)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveRequest_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                column: "LeaveTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveRequest_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                column: "LeaveTypeId",
                principalSchema: "Core",
                principalTable: "hrms_LeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
