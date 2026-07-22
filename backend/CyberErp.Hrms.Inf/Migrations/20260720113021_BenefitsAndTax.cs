using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class BenefitsAndTax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsBenefitPlan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmployeeContributionMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeContributionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmployerContributionMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployerContributionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EnrollmentOpenFrom = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    EnrollmentOpenTo = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
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
                    table.PrimaryKey("PK_hrmsBenefitPlan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTaxBracket",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LowerBound = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpperBound = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RatePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTaxBracket", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsEmployeeBenefitEnrollment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BenefitPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnrolledOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CoverageStart = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CoverageEnd = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ElectedEmployeeContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_hrmsEmployeeBenefitEnrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeBenefitEnrollment_hrmsBenefitPlan_BenefitPlanId",
                        column: x => x.BenefitPlanId,
                        principalSchema: "dbo",
                        principalTable: "hrmsBenefitPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeBenefitEnrollment_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsBenefitPlan_TenantId_Name",
                schema: "dbo",
                table: "hrmsBenefitPlan",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeBenefitEnrollment_BenefitPlanId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment",
                column: "BenefitPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeBenefitEnrollment_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsEmployeeBenefitEnrollment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTaxBracket",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsBenefitPlan",
                schema: "dbo");
        }
    }
}
