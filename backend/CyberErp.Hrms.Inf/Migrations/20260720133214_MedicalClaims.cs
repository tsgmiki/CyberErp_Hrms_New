using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class MedicalClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsMedicalClaim",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalBeneficiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BeneficiaryCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MedicalPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ClaimedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_hrmsMedicalClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsMedicalClaim_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsMedicalClaim_hrmsMedicalEnrollment_MedicalEnrollmentId",
                        column: x => x.MedicalEnrollmentId,
                        principalSchema: "dbo",
                        principalTable: "hrmsMedicalEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsMedicalClaimAttachment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_hrmsMedicalClaimAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsMedicalClaimAttachment_hrmsMedicalClaim_MedicalClaimId",
                        column: x => x.MedicalClaimId,
                        principalSchema: "dbo",
                        principalTable: "hrmsMedicalClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaim_ClaimNumber",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaim_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaim_MedicalBeneficiaryId_Status",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                columns: new[] { "MedicalBeneficiaryId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaim_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "MedicalEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaim_Status",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaimAttachment_MedicalClaimId",
                schema: "dbo",
                table: "hrmsMedicalClaimAttachment",
                column: "MedicalClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsMedicalClaimAttachment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsMedicalClaim",
                schema: "dbo");
        }
    }
}
