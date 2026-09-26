using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddDelegationPolicyRestrictToOwnDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RestrictToOwnDepartment",
                schema: "Hrms",
                table: "DelegationPolicy",
                type: "bit",
                nullable: false,
                // ⚠️ TRUE, not EF's generated false. The column encodes a rule that is ALREADY in
                // force: until now every non-HR delegation was confined to the approver's own
                // department, unconditionally. Taking EF's default would silently flip every
                // existing tenant to "anyone in the organisation may be named" the moment this
                // migration ran — a widening of who can hold approval authority, applied by a
                // schema change nobody read as a policy change.
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestrictToOwnDepartment",
                schema: "Hrms",
                table: "DelegationPolicy");
        }
    }
}
