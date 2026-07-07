using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicClearanceConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hrms_ClearanceDepartment",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_hrms_ClearanceDepartment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ClearanceDepartmentApprover",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApproverType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ClearanceDepartmentApprover", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ClearanceDepartmentApprover_hrms_ClearanceDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "Core",
                        principalTable: "hrms_ClearanceDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_TerminationClearance_DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ClearanceDepartment_TenantId_Name",
                schema: "Core",
                table: "hrms_ClearanceDepartment",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ClearanceDepartmentApprover_ApproverType_ApproverId",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover",
                columns: new[] { "ApproverType", "ApproverId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ClearanceDepartmentApprover_DepartmentId",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_TerminationClearance_hrms_ClearanceDepartment_DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance",
                column: "DepartmentId",
                principalSchema: "Core",
                principalTable: "hrms_ClearanceDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_TerminationClearance_hrms_ClearanceDepartment_DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance");

            migrationBuilder.DropTable(
                name: "hrms_ClearanceDepartmentApprover",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ClearanceDepartment",
                schema: "Core");

            migrationBuilder.DropIndex(
                name: "IX_hrms_TerminationClearance_DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance");
        }
    }
}
