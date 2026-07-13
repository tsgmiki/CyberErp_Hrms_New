using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferLetterTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                schema: "Core",
                table: "hrms_CompanyProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactAddress",
                schema: "Core",
                table: "hrms_CompanyProfile",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                schema: "Core",
                table: "hrms_CompanyProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                schema: "Core",
                table: "hrms_CompanyProfile",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hrms_OfferLetterTemplate",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    SignatoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignatoryTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_OfferLetterTemplate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_OfferLetterTemplate_TenantId",
                schema: "Core",
                table: "hrms_OfferLetterTemplate",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrms_OfferLetterTemplate",
                schema: "Core");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                schema: "Core",
                table: "hrms_CompanyProfile");

            migrationBuilder.DropColumn(
                name: "ContactAddress",
                schema: "Core",
                table: "hrms_CompanyProfile");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                schema: "Core",
                table: "hrms_CompanyProfile");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                schema: "Core",
                table: "hrms_CompanyProfile");
        }
    }
}
