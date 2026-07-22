using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class IndexDynamicFormRecordCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId",
                schema: "Core",
                table: "hrms_DynamicFormRecord");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt",
                schema: "Core",
                table: "hrms_DynamicFormRecord",
                columns: new[] { "DynamicFormId", "OwnerType", "OwnerId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt",
                schema: "Core",
                table: "hrms_DynamicFormRecord");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId",
                schema: "Core",
                table: "hrms_DynamicFormRecord",
                columns: new[] { "DynamicFormId", "OwnerType", "OwnerId" });
        }
    }
}
