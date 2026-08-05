using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardSummaryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsWorkflowInstance_TenantId_Status",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployee_TenantId_BranchId_EmploymentStatus",
                schema: "dbo",
                table: "hrmsEmployee",
                columns: new[] { "TenantId", "BranchId", "EmploymentStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrmsWorkflowInstance_TenantId_Status",
                schema: "dbo",
                table: "hrmsWorkflowInstance");

            migrationBuilder.DropIndex(
                name: "IX_hrmsEmployee_TenantId_BranchId_EmploymentStatus",
                schema: "dbo",
                table: "hrmsEmployee");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
