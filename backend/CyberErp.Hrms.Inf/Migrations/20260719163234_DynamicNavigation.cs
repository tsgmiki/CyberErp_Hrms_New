using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class DynamicNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "Core",
                table: "Operation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "Core",
                table: "Module",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // dbo.coreSubsystem may already exist in the shared ERP database (created outside the
            // HRMS migration history, empty, with a slightly different column profile). Drop the
            // orphan ONLY when it holds no rows so EF can own the table; a populated table (another
            // subsystem started using it) fails the migration loudly instead of losing data.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[coreSubsystem]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[coreSubsystem])
        THROW 51000, 'dbo.coreSubsystem already contains data - migrate it manually before applying DynamicNavigation.', 1;
    DROP TABLE [dbo].[coreSubsystem];
END");

            migrationBuilder.CreateTable(
                name: "coreSubsystem",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coreSubsystem", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coreSubsystem_TenantId_Name",
                schema: "dbo",
                table: "coreSubsystem",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coreSubsystem",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "Core",
                table: "Module");
        }
    }
}
