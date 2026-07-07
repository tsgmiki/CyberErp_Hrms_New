using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class IntegrateFiscalYearLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rekeying balances/ledger/requests from calendar year to fiscal year. The existing
            // rows are development fixtures only (no production data) — cleared so the new
            // NOT NULL FiscalYearId FK columns can be added.
            migrationBuilder.Sql("DELETE FROM [Core].[hrms_LeaveBalanceTransaction];");
            migrationBuilder.Sql("DELETE FROM [Core].[hrms_LeaveBalance];");
            migrationBuilder.Sql("DELETE FROM [Core].[hrms_LeaveRequest];");

            migrationBuilder.DropIndex(
                name: "IX_hrms_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_Year",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_hrms_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_Year",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropColumn(
                name: "Year",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "Year",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.AddColumn<Guid>(
                name: "FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsManagerial",
                schema: "Core",
                table: "hrms_Employee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Core.FiscalYear already exists (created outside EF) — adopted, not recreated.
            // Only the new IsClosed period-control column is added, and TenantId is aligned to the
            // standard indexable nvarchar(450) (it was created as nvarchar(max)).
            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                schema: "Core",
                table: "FiscalYear",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("ALTER TABLE [Core].[FiscalYear] ALTER COLUMN [TenantId] nvarchar(450) NOT NULL;");

            migrationBuilder.CreateTable(
                name: "hrms_AnnualLeaveSetting",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinExperienceMonths = table.Column<int>(type: "int", nullable: false),
                    NewEmployeeLeaveDays = table.Column<int>(type: "int", nullable: false),
                    BaseLeaveDays = table.Column<int>(type: "int", nullable: false),
                    ManagerialLeaveDays = table.Column<int>(type: "int", nullable: false),
                    IncrementDays = table.Column<int>(type: "int", nullable: false),
                    IncrementIntervalYears = table.Column<int>(type: "int", nullable: false),
                    MaxLeaveDays = table.Column<int>(type: "int", nullable: false),
                    ExpiryYears = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_hrms_AnnualLeaveSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_AnnualLeaveSetting_FiscalYear_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalSchema: "Core",
                        principalTable: "FiscalYear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_AnnualLeaveSetting_hrms_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "Core",
                        principalTable: "hrms_LeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveRequest_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction",
                columns: new[] { "EmployeeId", "LeaveTypeId", "FiscalYearId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveBalance_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                columns: new[] { "TenantId", "EmployeeId", "LeaveTypeId", "FiscalYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYear_TenantId_Name",
                schema: "Core",
                table: "FiscalYear",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveSetting_FiscalYearId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveSetting_LeaveTypeId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_AnnualLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                columns: new[] { "TenantId", "FiscalYearId", "LeaveTypeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveBalance_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveRequest_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveBalance_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveRequest_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropTable(
                name: "hrms_AnnualLeaveSetting",
                schema: "Core");

            // Core.FiscalYear pre-dates EF — on rollback we only remove the column we added.
            migrationBuilder.DropIndex(
                name: "IX_FiscalYear_TenantId_Name",
                schema: "Core",
                table: "FiscalYear");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                schema: "Core",
                table: "FiscalYear");

            migrationBuilder.DropIndex(
                name: "IX_hrms_LeaveRequest_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropIndex(
                name: "IX_hrms_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_hrms_LeaveBalance_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_hrms_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropColumn(
                name: "IsManagerial",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                schema: "Core",
                table: "hrms_LeaveBalance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_Year",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction",
                columns: new[] { "EmployeeId", "LeaveTypeId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_Year",
                schema: "Core",
                table: "hrms_LeaveBalance",
                columns: new[] { "TenantId", "EmployeeId", "LeaveTypeId", "Year" },
                unique: true);
        }
    }
}
