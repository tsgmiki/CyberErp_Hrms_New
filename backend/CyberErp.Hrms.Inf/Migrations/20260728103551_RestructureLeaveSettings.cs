using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class RestructureLeaveSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAnnualLeaveSetting_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_Name",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropIndex(
                name: "IX_hrmsAnnualLeaveSetting_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropIndex(
                name: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.AddColumn<string>(
                name: "DayCounting",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveSetting_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                columns: new[] { "TenantId", "FiscalYearId", "LeaveTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                columns: new[] { "TenantId", "FiscalYearId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOtherLeaveSetting_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                column: "LeaveTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOtherLeaveSetting_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropIndex(
                name: "IX_hrmsOtherLeaveSetting_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropIndex(
                name: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropColumn(
                name: "DayCounting",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_Name",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                columns: new[] { "TenantId", "FiscalYearId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsAnnualLeaveSetting_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                columns: new[] { "TenantId", "FiscalYearId", "LeaveTypeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAnnualLeaveSetting_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                column: "LeaveTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
