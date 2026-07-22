using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class GeneralizeCustomFieldsToChildForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeFieldValue_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue");

            migrationBuilder.DropIndex(
                name: "IX_hrms_EmployeeFieldValue_EmployeeId_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue");

            migrationBuilder.DropIndex(
                name: "IX_hrms_EmployeeFieldDefinition_TenantId_Name",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                newName: "OwnerId");

            // Existing rows are all Employee-scoped (the only owner type before this migration) — the
            // default backfills them so they keep matching EmployeeFieldOwnerType.Employee ("Employee").
            migrationBuilder.AddColumn<string>(
                name: "OwnerType",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Employee");

            migrationBuilder.AddColumn<string>(
                name: "OwnerType",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Employee");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                columns: new[] { "OwnerType", "OwnerId", "FieldDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeFieldDefinition_TenantId_OwnerType_Name",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition",
                columns: new[] { "TenantId", "OwnerType", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrms_EmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue");

            migrationBuilder.DropIndex(
                name: "IX_hrms_EmployeeFieldDefinition_TenantId_OwnerType_Name",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                schema: "Core",
                table: "hrms_EmployeeFieldValue");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                newName: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeFieldValue_EmployeeId_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                columns: new[] { "EmployeeId", "FieldDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrms_EmployeeFieldDefinition_TenantId_Name",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeFieldValue_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
