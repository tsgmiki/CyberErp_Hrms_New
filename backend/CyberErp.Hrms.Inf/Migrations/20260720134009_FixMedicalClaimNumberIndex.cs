using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class FixMedicalClaimNumberIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrmsMedicalClaim_ClaimNumber",
                schema: "dbo",
                table: "hrmsMedicalClaim");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaim_TenantId_ClaimNumber",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                columns: new[] { "TenantId", "ClaimNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrmsMedicalClaim_TenantId_ClaimNumber",
                schema: "dbo",
                table: "hrmsMedicalClaim");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsMedicalClaim_ClaimNumber",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "ClaimNumber",
                unique: true);
        }
    }
}
