using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkforcePlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_WorkforcePlan",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Horizon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Scenario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartFiscalYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodCount = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    RootPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BudgetThresholdPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EscalationJustification = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProjectedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_WorkforcePlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_WorkforcePlan_FiscalYear_StartFiscalYearId",
                        column: x => x.StartFiscalYearId,
                        principalSchema: "Core",
                        principalTable: "FiscalYear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_WorkforcePlan_hrms_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_WorkforcePlanLine",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeriodIndex = table.Column<int>(type: "int", nullable: false),
                    AuthorizedHeadcount = table.Column<int>(type: "int", nullable: false),
                    FilledCount = table.Column<int>(type: "int", nullable: false),
                    VacantCount = table.Column<int>(type: "int", nullable: false),
                    NewHires = table.Column<int>(type: "int", nullable: false),
                    Replacements = table.Column<int>(type: "int", nullable: false),
                    TemporaryStaff = table.Column<int>(type: "int", nullable: false),
                    MobilityIn = table.Column<int>(type: "int", nullable: false),
                    Promotions = table.Column<int>(type: "int", nullable: false),
                    ActingAssignments = table.Column<int>(type: "int", nullable: false),
                    Retirements = table.Column<int>(type: "int", nullable: false),
                    Resignations = table.Column<int>(type: "int", nullable: false),
                    ContractExpiries = table.Column<int>(type: "int", nullable: false),
                    IsCriticalRole = table.Column<bool>(type: "bit", nullable: false),
                    RequiredCompetencies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AnnualSalaryCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualBenefits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_WorkforcePlanLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_WorkforcePlanLine_hrms_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_WorkforcePlanLine_hrms_PositionClass_PositionClassId",
                        column: x => x.PositionClassId,
                        principalSchema: "Core",
                        principalTable: "hrms_PositionClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_WorkforcePlanLine_hrms_WorkforcePlan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "Core",
                        principalTable: "hrms_WorkforcePlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlan_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlan_RootPlanId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                column: "RootPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlan_StartFiscalYearId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                column: "StartFiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlan_TenantId_Status",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlanLine_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlanLine_PlanId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                columns: new[] { "PlanId", "OrganizationUnitId", "PositionClassId", "EmploymentType", "PeriodIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_WorkforcePlanLine_PositionClassId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                column: "PositionClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_WorkforcePlanLine",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_WorkforcePlan",
                schema: "Core");
        }
    }
}
