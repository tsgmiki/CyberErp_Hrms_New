using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class MedicalEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsMedicalEnrollment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrolledOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CoverageStart = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CoverageEnd = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsMedicalEnrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsMedicalEnrollment_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsMedicalEnrollment_hrmsMedicalPlan_MedicalPlanId",
                        column: x => x.MedicalPlanId,
                        principalSchema: "dbo",
                        principalTable: "hrmsMedicalPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsMedicalBeneficiary",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmployeeDependentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsMedicalBeneficiary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsMedicalBeneficiary_hrmsMedicalEnrollment_MedicalEnrollmentId",
                        column: x => x.MedicalEnrollmentId,
                        principalSchema: "dbo",
                        principalTable: "hrmsMedicalEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalBeneficiary_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalBeneficiary",
                column: "MedicalEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalEnrollment_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalEnrollment_MedicalPlanId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment",
                column: "MedicalPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsMedicalBeneficiary",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsMedicalEnrollment",
                schema: "dbo");
        }
    }
}
