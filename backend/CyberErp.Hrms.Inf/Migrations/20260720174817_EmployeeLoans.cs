using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsLoanType",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxSalaryMultiple = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
                    MaxTermMonths = table.Column<int>(type: "int", nullable: false),
                    InterestRatePct = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    RequiresGuarantor = table.Column<bool>(type: "bit", nullable: false),
                    MinGuarantors = table.Column<int>(type: "int", nullable: false),
                    ServiceCommitmentMonths = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLoanType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsLoan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    InterestRatePct = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    MonthlyInstallment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalRepayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ServiceCommitmentMonths = table.Column<int>(type: "int", nullable: false),
                    DisbursedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    DisbursementReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SettledAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ServiceCommitmentConsentAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLoan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLoan_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsLoan_hrmsLoanType_LoanTypeId",
                        column: x => x.LoanTypeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsLoanType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsLoanGuarantor",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuarantorEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdentificationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuaranteedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLoanGuarantor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLoanGuarantor_hrmsLoan_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "dbo",
                        principalTable: "hrmsLoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsLoanRepaymentSchedule",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstallmentNo = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    PrincipalPortion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestPortion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLoanRepaymentSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLoanRepaymentSchedule_hrmsLoan_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "dbo",
                        principalTable: "hrmsLoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoan_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsLoan",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoan_LoanTypeId",
                schema: "dbo",
                table: "hrmsLoan",
                column: "LoanTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoan_Status",
                schema: "dbo",
                table: "hrmsLoan",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoan_TenantId_LoanNumber",
                schema: "dbo",
                table: "hrmsLoan",
                columns: new[] { "TenantId", "LoanNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoanGuarantor_LoanId",
                schema: "dbo",
                table: "hrmsLoanGuarantor",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoanRepaymentSchedule_LoanId_InstallmentNo",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule",
                columns: new[] { "LoanId", "InstallmentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoanRepaymentSchedule_Status_DueDate",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLoanType_TenantId_Name",
                schema: "dbo",
                table: "hrmsLoanType",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsLoanGuarantor",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsLoanRepaymentSchedule",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsLoan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsLoanType",
                schema: "dbo");
        }
    }
}
