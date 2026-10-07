using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceCapture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceDevice",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Protocol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Port = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IngestKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    LastSyncError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastSyncPunchCount = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceDevice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkShift",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakMinutes = table.Column<int>(type: "int", nullable: false),
                    GraceMinutes = table.Column<int>(type: "int", nullable: false),
                    MinimumMinutesForPresent = table.Column<int>(type: "int", nullable: false),
                    HalfDayMinutes = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkShift", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceEnrollment",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceDeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceEnrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceEnrollment_AttendanceDevice_AttendanceDeviceId",
                        column: x => x.AttendanceDeviceId,
                        principalSchema: "Hrms",
                        principalTable: "AttendanceDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceEnrollment_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Hrms",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendancePunch",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AttendanceDeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeviceUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PunchedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RawPayload = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UnresolvedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendancePunch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendancePunch_AttendanceDevice_AttendanceDeviceId",
                        column: x => x.AttendanceDeviceId,
                        principalSchema: "Hrms",
                        principalTable: "AttendanceDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendancePunch_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Hrms",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceDay",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    WorkShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstIn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    LastOut = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    WorkedMinutes = table.Column<int>(type: "int", nullable: false),
                    LateMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyLeaveMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DerivedStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DayValue = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    IsOverridden = table.Column<bool>(type: "bit", nullable: false),
                    OverrideReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OverriddenBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OverriddenAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceDay_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Hrms",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceDay_WorkShift_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "Hrms",
                        principalTable: "WorkShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShiftAssignment",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShiftAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftAssignment_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Hrms",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftAssignment_WorkShift_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalSchema: "Hrms",
                        principalTable: "WorkShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDay_EmployeeId",
                schema: "Hrms",
                table: "AttendanceDay",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDay_TenantId_EmployeeId_WorkDate",
                schema: "Hrms",
                table: "AttendanceDay",
                columns: new[] { "TenantId", "EmployeeId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDay_TenantId_WorkDate_Status",
                schema: "Hrms",
                table: "AttendanceDay",
                columns: new[] { "TenantId", "WorkDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDay_WorkShiftId",
                schema: "Hrms",
                table: "AttendanceDay",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDevice_TenantId_Code",
                schema: "Hrms",
                table: "AttendanceDevice",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEnrollment_AttendanceDeviceId",
                schema: "Hrms",
                table: "AttendanceEnrollment",
                column: "AttendanceDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEnrollment_EmployeeId",
                schema: "Hrms",
                table: "AttendanceEnrollment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEnrollment_TenantId_AttendanceDeviceId_DeviceUserId",
                schema: "Hrms",
                table: "AttendanceEnrollment",
                columns: new[] { "TenantId", "AttendanceDeviceId", "DeviceUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunch_AttendanceDeviceId",
                schema: "Hrms",
                table: "AttendancePunch",
                column: "AttendanceDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunch_EmployeeId",
                schema: "Hrms",
                table: "AttendancePunch",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunch_TenantId_AttendanceDeviceId_ExternalId",
                schema: "Hrms",
                table: "AttendancePunch",
                columns: new[] { "TenantId", "AttendanceDeviceId", "ExternalId" },
                unique: true,
                filter: "[ExternalId] IS NOT NULL AND [AttendanceDeviceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunch_TenantId_EmployeeId_PunchedAt",
                schema: "Hrms",
                table: "AttendancePunch",
                columns: new[] { "TenantId", "EmployeeId", "PunchedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftAssignment_EmployeeId",
                schema: "Hrms",
                table: "EmployeeShiftAssignment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftAssignment_TenantId_EmployeeId_EffectiveFrom",
                schema: "Hrms",
                table: "EmployeeShiftAssignment",
                columns: new[] { "TenantId", "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftAssignment_WorkShiftId",
                schema: "Hrms",
                table: "EmployeeShiftAssignment",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShift_TenantId_Code",
                schema: "Hrms",
                table: "WorkShift",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceDay",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "AttendanceEnrollment",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "AttendancePunch",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "EmployeeShiftAssignment",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "AttendanceDevice",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "WorkShift",
                schema: "Hrms");
        }
    }
}
