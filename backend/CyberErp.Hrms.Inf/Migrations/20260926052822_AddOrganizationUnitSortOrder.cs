using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationUnitSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "Hrms",
                table: "OrganizationUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Seed every existing unit with the position it ALREADY OCCUPIES on screen, so the day
            // this ships the tree looks exactly as it did the day before. The old tree query ordered
            // by UnitType then Name; reproducing that here means nothing appears to jump.
            //
            // ⚠️ UnitType is persisted as a STRING (HasConversion<string>), so both the old LINQ
            // ordering and this ORDER BY sort by the type's NAME, not its enum value — Branch,
            // BusinessUnit, Department, Directorate, Division, Team. Ordering by the enum here
            // would "fix" that into a different order and silently rearrange every level.
            //
            // Numbered in tens to match the resequencer's gap, and partitioned by TenantId as well
            // as ParentId: root units of different tenants are all ParentId NULL and must not be
            // interleaved into one sequence.
            migrationBuilder.Sql(@"
WITH ordered AS (
    SELECT Id,
           ROW_NUMBER() OVER (
               PARTITION BY TenantId, ParentId
               ORDER BY UnitType, Name
           ) * 10 AS NewSortOrder
    FROM Hrms.OrganizationUnit
)
UPDATE ou
   SET ou.SortOrder = o.NewSortOrder
  FROM Hrms.OrganizationUnit ou
 INNER JOIN ordered o ON o.Id = ou.Id;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "Hrms",
                table: "OrganizationUnit");
        }
    }
}
