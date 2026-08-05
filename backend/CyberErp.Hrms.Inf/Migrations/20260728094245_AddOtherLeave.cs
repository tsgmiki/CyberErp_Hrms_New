using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsOtherLeaveSetting",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StandardDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    ManagerialDays = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    IsLumpSum = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsOtherLeaveSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsOtherLeaveSetting_FiscalYear_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalSchema: "Core",
                        principalTable: "FiscalYear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsOtherLeave",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OtherLeaveSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_hrmsOtherLeave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsOtherLeave_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsOtherLeave_hrmsOtherLeaveSetting_OtherLeaveSettingId",
                        column: x => x.OtherLeaveSettingId,
                        principalSchema: "dbo",
                        principalTable: "hrmsOtherLeaveSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsOtherLeaveDetail",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OtherLeaveHeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_hrmsOtherLeaveDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsOtherLeaveDetail_hrmsOtherLeave_OtherLeaveHeaderId",
                        column: x => x.OtherLeaveHeaderId,
                        principalSchema: "dbo",
                        principalTable: "hrmsOtherLeave",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeave_EmployeeId",
                schema: "dbo",
                table: "hrmsOtherLeave",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeave_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsOtherLeave",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeave_OtherLeaveSettingId",
                schema: "dbo",
                table: "hrmsOtherLeave",
                column: "OtherLeaveSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail",
                column: "OtherLeaveHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId_StartDate_EndDate",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail",
                columns: new[] { "OtherLeaveHeaderId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveSetting_FiscalYearId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_Name",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                columns: new[] { "TenantId", "FiscalYearId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                columns: new[] { "TenantId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsOtherLeaveDetail",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsOtherLeave",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsOtherLeaveSetting",
                schema: "dbo");
        }
    }
}
