using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeListPerfIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_hrms_Employee_TenantId_PositionId_EmployeeNumber",
                schema: "Core",
                table: "hrms_Employee",
                columns: new[] { "TenantId", "PositionId", "EmployeeNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_Employee_TenantId_PositionId_EmployeeNumber",
                schema: "Core",
                table: "hrms_Employee");
        }
    }
}
