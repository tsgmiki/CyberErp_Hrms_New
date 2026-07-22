using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class InsuranceClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsInsuranceClaim",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsurancePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ClaimedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsInsuranceClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsInsuranceClaim_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsInsuranceClaim_hrmsInsurancePolicy_InsurancePolicyId",
                        column: x => x.InsurancePolicyId,
                        principalSchema: "dbo",
                        principalTable: "hrmsInsurancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsInsuranceClaimAttachment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsuranceClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsInsuranceClaimAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsInsuranceClaimAttachment_hrmsInsuranceClaim_InsuranceClaimId",
                        column: x => x.InsuranceClaimId,
                        principalSchema: "dbo",
                        principalTable: "hrmsInsuranceClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsuranceClaim_EmployeeId",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsuranceClaim_InsurancePolicyId",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                column: "InsurancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsuranceClaim_Status",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsuranceClaim_TenantId_ClaimNumber",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                columns: new[] { "TenantId", "ClaimNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsInsuranceClaimAttachment_InsuranceClaimId",
                schema: "dbo",
                table: "hrmsInsuranceClaimAttachment",
                column: "InsuranceClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsInsuranceClaimAttachment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsInsuranceClaim",
                schema: "dbo");
        }
    }
}
