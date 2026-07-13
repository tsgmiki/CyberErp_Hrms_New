using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobOffer_ApplicationId_CreatedAt",
                schema: "Core",
                table: "hrms_JobOffer",
                columns: new[] { "ApplicationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_JobApplication_TenantId_AppliedAt",
                schema: "Core",
                table: "hrms_JobApplication",
                columns: new[] { "TenantId", "AppliedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_JobOffer_ApplicationId_CreatedAt",
                schema: "Core",
                table: "hrms_JobOffer");

            migrationBuilder.DropIndex(
                name: "IX_hrms_JobApplication_TenantId_AppliedAt",
                schema: "Core",
                table: "hrms_JobApplication");
        }
    }
}
