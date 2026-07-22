using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class TripRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsTripRequest",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    DailyPerDiemRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PerDiemAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TripBudgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    AdvanceDisbursedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    AdvanceReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SettledAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    SettlementNet = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SettlementReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTripRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTripRequest_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsTripRequest_hrmsTripBudget_TripBudgetId",
                        column: x => x.TripBudgetId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTripBudget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTripExpense",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTripExpense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTripExpense_hrmsTripRequest_TripRequestId",
                        column: x => x.TripRequestId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTripRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTripExpense_TripRequestId",
                schema: "dbo",
                table: "hrmsTripExpense",
                column: "TripRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTripRequest_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsTripRequest",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTripRequest_Status",
                schema: "dbo",
                table: "hrmsTripRequest",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTripRequest_TenantId_TripNumber",
                schema: "dbo",
                table: "hrmsTripRequest",
                columns: new[] { "TenantId", "TripNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTripRequest_TripBudgetId",
                schema: "dbo",
                table: "hrmsTripRequest",
                column: "TripBudgetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsTripExpense",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTripRequest",
                schema: "dbo");
        }
    }
}
