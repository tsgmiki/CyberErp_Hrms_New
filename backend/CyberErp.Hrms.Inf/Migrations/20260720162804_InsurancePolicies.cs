using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class InsurancePolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsInsurancePolicy",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InsurerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InsuranceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Coverage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CoverageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PolicyYear = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    AnnualPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremiumFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsRenewal = table.Column<bool>(type: "bit", nullable: false),
                    PreviousPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsInsurancePolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsInsurancePremiumSchedule",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsurancePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Installment = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsInsurancePremiumSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsInsurancePremiumSchedule_hrmsInsurancePolicy_InsurancePolicyId",
                        column: x => x.InsurancePolicyId,
                        principalSchema: "dbo",
                        principalTable: "hrmsInsurancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsurancePolicy_Status",
                schema: "dbo",
                table: "hrmsInsurancePolicy",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsurancePolicy_TenantId_PolicyNumber",
                schema: "dbo",
                table: "hrmsInsurancePolicy",
                columns: new[] { "TenantId", "PolicyNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsurancePremiumSchedule_InsurancePolicyId_Installment",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule",
                columns: new[] { "InsurancePolicyId", "Installment" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsurancePremiumSchedule_Status_DueDate",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule",
                columns: new[] { "Status", "DueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsInsurancePremiumSchedule",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsInsurancePolicy",
                schema: "dbo");
        }
    }
}
