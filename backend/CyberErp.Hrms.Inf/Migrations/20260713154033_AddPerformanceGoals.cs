using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrms_OrganizationalObjective",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentObjectiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_OrganizationalObjective", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_OrganizationalObjective_hrms_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_OrganizationalObjective_hrms_OrganizationalObjective_ParentObjectiveId",
                        column: x => x.ParentObjectiveId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationalObjective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_OrganizationalObjective_hrms_ReviewCycle_ReviewCycleId",
                        column: x => x.ReviewCycleId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReviewCycle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_EmployeeGoal",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationalObjectiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Measure = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SetByManager = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_EmployeeGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_EmployeeGoal_hrms_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Core",
                        principalTable: "hrms_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_EmployeeGoal_hrms_OrganizationalObjective_OrganizationalObjectiveId",
                        column: x => x.OrganizationalObjectiveId,
                        principalSchema: "Core",
                        principalTable: "hrms_OrganizationalObjective",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrms_EmployeeGoal_hrms_ReviewCycle_ReviewCycleId",
                        column: x => x.ReviewCycleId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReviewCycle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrms_GoalActionItem",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeGoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_hrms_GoalActionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_GoalActionItem_hrms_EmployeeGoal_EmployeeGoalId",
                        column: x => x.EmployeeGoalId,
                        principalSchema: "Core",
                        principalTable: "hrms_EmployeeGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeGoal_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeGoal_OrganizationalObjectiveId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                column: "OrganizationalObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeGoal_ReviewCycleId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                column: "ReviewCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeGoal_TenantId_EmployeeId_ReviewCycleId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                columns: new[] { "TenantId", "EmployeeId", "ReviewCycleId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_GoalActionItem_EmployeeGoalId_SortOrder",
                schema: "Core",
                table: "hrms_GoalActionItem",
                columns: new[] { "EmployeeGoalId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_OrganizationalObjective_OrganizationUnitId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_OrganizationalObjective_ParentObjectiveId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                column: "ParentObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_OrganizationalObjective_ReviewCycleId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                column: "ReviewCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_OrganizationalObjective_TenantId_ReviewCycleId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                columns: new[] { "TenantId", "ReviewCycleId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_OrganizationalObjective_TenantId_ReviewCycleId_Title",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                columns: new[] { "TenantId", "ReviewCycleId", "Title" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_GoalActionItem",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_EmployeeGoal",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_OrganizationalObjective",
                schema: "Core");
        }
    }
}
