using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Registers the organisation-wide "Delegation Rules" settings screen in the HRMS menu.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ Without a menu row the screen is unreachable —
    /// <c>ApprovalDelegationController</c>'s policy endpoints resolve
    /// <c>[RequirePermission("approvalDelegation")]</c> against <c>TenantOperation.Link</c>, and the
    /// SPA route is permission-gated too. This is the same trap the delegation register hit; it is
    /// recorded here because it bites once per screen, not once per feature.</para>
    ///
    /// <para>Granted exactly where the delegation REGISTER is granted, and nowhere else. Setting the
    /// rules for the whole organisation is strictly more powerful than arranging one stand-in: it
    /// decides who may ever hold anybody's approval authority. Seeding it from the register keeps
    /// the two together, so a role that may administer delegations may also set their rules — and a
    /// department head, who holds neither, gains nothing.</para>
    ///
    /// <para>⚠️ NOT granted from the portal's self-service "My Delegations". That grant belongs to
    /// every employee who can arrange their own cover; attaching organisation-wide policy to it
    /// would hand the rules to the entire workforce.</para>
    ///
    /// <para>Idempotent; re-running cannot duplicate rows.</para>
    /// </remarks>
    public partial class SeedDelegationPolicyMenu : Migration
    {
        private const string Link = "/hrms/delegationPolicy";
        private const string OperationName = "Delegation Rules";
        /// <summary>The register — this screen's permission peer.</summary>
        private const string Sibling = "/hrms/approvalDelegation";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM Core.Operation WHERE Link = '{Link}')
BEGIN
    INSERT INTO Core.Operation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
    SELECT NEWID(), o.ModuleId, N'{OperationName}', '{Link}', N'', 'SlidersHorizontal',
           ISNULL((SELECT MAX(x.DisplayOrder) FROM Core.Operation x WHERE x.ModuleId = o.ModuleId), 0) + 1,
           1, SYSUTCDATETIME(), 'migration', 0x0000000000000001
    FROM Core.Operation o
    WHERE o.Link = '{Sibling}';
END;");

            migrationBuilder.Sql($@"
INSERT INTO Core.TenantOperation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), t.ModuleId, N'{OperationName}', '{Link}', N'', 'SlidersHorizontal',
       ISNULL((SELECT MAX(x.DisplayOrder) FROM Core.TenantOperation x WHERE x.ModuleId = t.ModuleId), 0) + 1,
       1, SYSUTCDATETIME(), 'migration', 0x0000000000000001
FROM Core.TenantOperation t
WHERE t.Link = '{Sibling}'
  AND NOT EXISTS (SELECT 1 FROM Core.TenantOperation x
                  WHERE x.Link = '{Link}' AND x.ModuleId = t.ModuleId);");

            migrationBuilder.Sql($@"
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), p.TenantRoleId, d.Id, p.CanView, p.CanAdd, p.CanEdit, 0, 0, 0,
       SYSUTCDATETIME(), 'migration', 0x0000000000000001
FROM Core.TenantRolePermission p
JOIN Core.TenantOperation src ON src.Id = p.TenantOperationId AND src.Link = '{Sibling}'
JOIN Core.TenantOperation d   ON d.Link = '{Link}' AND d.ModuleId = src.ModuleId
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantRolePermission x
                  WHERE x.TenantRoleId = p.TenantRoleId AND x.TenantOperationId = d.Id);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE p FROM Core.TenantRolePermission p
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId
WHERE o.Link = '{Link}';");
            migrationBuilder.Sql($"DELETE FROM Core.TenantOperation WHERE Link = '{Link}';");
            migrationBuilder.Sql($"DELETE FROM Core.Operation WHERE Link = '{Link}';");
        }
    }
}
