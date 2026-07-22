using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class CompanyAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsCompanyAsset",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedToEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsCompanyAsset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsCompanyAsset_hrmsEmployee_AssignedToEmployeeId",
                        column: x => x.AssignedToEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTerminationAssetRecovery",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResolvedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTerminationAssetRecovery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTerminationAssetRecovery_hrmsCompanyAsset_CompanyAssetId",
                        column: x => x.CompanyAssetId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCompanyAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsTerminationAssetRecovery_hrmsEmployeeTermination_TerminationId",
                        column: x => x.TerminationId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployeeTermination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCompanyAsset_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                column: "AssignedToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCompanyAsset_TenantId_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                columns: new[] { "TenantId", "AssignedToEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCompanyAsset_TenantId_Status",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTerminationAssetRecovery_CompanyAssetId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                column: "CompanyAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTerminationAssetRecovery_TenantId_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                columns: new[] { "TenantId", "TerminationId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTerminationAssetRecovery_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                column: "TerminationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsTerminationAssetRecovery",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsCompanyAsset",
                schema: "dbo");
        }
    }
}
