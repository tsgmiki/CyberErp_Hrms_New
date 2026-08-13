using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Retires Hrms.CompanyProfile; Core.Organization owns the letterhead now.
    ///
    /// <para>Organization was added as an additive layer and, until this change, had NO reader at
    /// all, while the profile fed the company logo, the offer letter and the movement letters. The
    /// overlap was always meant to end this way round — the profile's four letterhead fields are a
    /// subset of what Organization already carries.</para>
    ///
    /// <para>⚠️ The copy below is defensive rather than decorative. This database has ZERO profile
    /// rows, so the drop is free here — but another environment may not, and a migration that only
    /// works against the database it was written on is not a migration. It moves the name, contact
    /// details and logo across, and only fills fields Organization has not already got, so a real
    /// organization record is never overwritten by a thinner profile.</para>
    /// </summary>
    public partial class ConsolidateCompanyProfileIntoOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Update the organization from the profile of the SAME tenant, filling only the gaps.
            migrationBuilder.Sql(@"
                UPDATE o
                   SET o.LegalName  = CASE WHEN NULLIF(LTRIM(RTRIM(o.LegalName)), '') IS NULL
                                           THEN LEFT(ISNULL(p.CompanyName, o.LegalName), 300) ELSE o.LegalName END,
                       o.Address    = ISNULL(o.Address,    LEFT(p.ContactAddress, 500)),
                       o.PhoneNumber= ISNULL(o.PhoneNumber,LEFT(p.ContactPhone, 100)),
                       o.Email      = ISNULL(o.Email,      LEFT(p.ContactEmail, 200)),
                       o.Logo       = ISNULL(o.Logo,       p.LogoContent),
                       o.LogoContentType = ISNULL(o.LogoContentType, LEFT(p.LogoContentType, 150))
                  FROM Core.Organization o
                  JOIN Hrms.CompanyProfile p ON p.TenantId = o.TenantId;");

            // 2. A tenant with a profile but no organization gets one built from it. Code and
            //    LegalName are required, so both fall back to something an administrator can correct
            //    rather than blocking the migration.
            migrationBuilder.Sql(@"
                INSERT INTO Core.Organization
                    (Id, Code, LegalName, Address, PhoneNumber, Email, Logo, LogoContentType,
                     FiscalYearStartMonth, IsActive, TenantId, CreatedAt, RowVersion)
                SELECT NEWID(), 'DEFAULT',
                       LEFT(ISNULL(NULLIF(LTRIM(RTRIM(p.CompanyName)), ''), 'Organization'), 300),
                       LEFT(p.ContactAddress, 500), LEFT(p.ContactPhone, 100), LEFT(p.ContactEmail, 200),
                       p.LogoContent, LEFT(p.LogoContentType, 150),
                       0, 1, p.TenantId, SYSUTCDATETIME(), 0x0000000000000001
                  FROM Hrms.CompanyProfile p
                 WHERE NOT EXISTS (SELECT 1 FROM Core.Organization o WHERE o.TenantId = p.TenantId);");

            migrationBuilder.DropTable(
                name: "CompanyProfile",
                schema: "Hrms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyProfile",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    LogoContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProfile", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProfile_TenantId",
                schema: "Hrms",
                table: "CompanyProfile",
                column: "TenantId",
                unique: true);
        }
    }
}
