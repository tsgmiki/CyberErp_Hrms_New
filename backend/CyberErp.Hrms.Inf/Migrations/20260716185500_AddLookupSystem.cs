using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LookUpCategory",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookUpCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookUpCategoryList",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookUpCategoryList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookUpCategoryList_LookUpCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Core",
                        principalTable: "LookUpCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookUpCategory_Code",
                schema: "Core",
                table: "LookUpCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookUpCategoryList_CategoryId_Code",
                schema: "Core",
                table: "LookUpCategoryList",
                columns: new[] { "CategoryId", "Code" },
                unique: true);

            // Seed the two lookups the Education form uses (GLOBAL: TenantId = ''). Idempotent by code.
            migrationBuilder.Sql(@"
DECLARE @edu UNIQUEIDENTIFIER, @fos UNIQUEIDENTIFIER;

IF NOT EXISTS (SELECT 1 FROM [Core].[LookUpCategory] WHERE [Code] = N'EducationLevel')
BEGIN
    SET @edu = NEWID();
    INSERT INTO [Core].[LookUpCategory] ([Id],[Name],[Code],[TenantId],[CreatedAt],[RowVersion])
    VALUES (@edu, N'Education Level', N'EducationLevel', N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    INSERT INTO [Core].[LookUpCategoryList] ([Id],[CategoryId],[Name],[Code],[TenantId],[CreatedAt],[RowVersion])
    VALUES
        (NEWID(), @edu, N'Primary School (1-8)',      N'PRIMARY', N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @edu, N'High School (9-12)',        N'HIGH',    N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @edu, N'Certificate',               N'CERT',    N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @edu, N'TVET / Diploma',            N'DIP',     N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @edu, N'Bachelor''s Degree',        N'BACH',    N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @edu, N'Master''s Degree',          N'MAST',    N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @edu, N'Doctorate (PhD)',           N'PHD',     N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END

IF NOT EXISTS (SELECT 1 FROM [Core].[LookUpCategory] WHERE [Code] = N'FieldOfStudy')
BEGIN
    SET @fos = NEWID();
    INSERT INTO [Core].[LookUpCategory] ([Id],[Name],[Code],[TenantId],[CreatedAt],[RowVersion])
    VALUES (@fos, N'Field of Study', N'FieldOfStudy', N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    INSERT INTO [Core].[LookUpCategoryList] ([Id],[CategoryId],[Name],[Code],[TenantId],[CreatedAt],[RowVersion])
    VALUES
        (NEWID(), @fos, N'Accounting & Finance',      N'ACCT',  N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Management',                N'MGMT',  N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Economics',                 N'ECON',  N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Human Resource Management', N'HRM',   N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Computer Science / IT',     N'CS',    N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Engineering',               N'ENG',   N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Law',                       N'LAW',   N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Medicine & Health',         N'MED',   N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Education',                 N'EDU',   N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())),
        (NEWID(), @fos, N'Marketing',                 N'MKT',   N'', SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookUpCategoryList",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "LookUpCategory",
                schema: "Core");
        }
    }
}
