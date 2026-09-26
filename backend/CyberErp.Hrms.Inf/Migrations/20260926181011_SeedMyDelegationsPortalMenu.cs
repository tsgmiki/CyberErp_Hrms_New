using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Registers "My Delegations" in the HOME portal's self-service menu.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ A SECOND, SEPARATE menu row — not the one seeded for the HRMS register. The two
    /// subsystems store their links differently: HRMS operations are NAMESPACED
    /// (<c>/hrms/approvalDelegation</c>) while the portal's self-service items are plain
    /// (<c>/annualLeave</c>, <c>/myExit</c>). Reusing the HRMS row would put the item in the wrong
    /// subsystem's sidebar and match nothing in the portal's route table.</para>
    ///
    /// <para>This is the whole point of the feature: the people who most need to delegate —
    /// department heads — have no access to HRMS at all. That subsystem is HR's. A head's only door
    /// into the platform is the portal, so the screen has to be reachable there or the feature
    /// reaches nobody who needs it.</para>
    ///
    /// <para>Placed in the module the other self-service items share, so it lands beside Annual
    /// Leave and My Compensation rather than in a category of its own. The portal additionally
    /// hides it at runtime for anyone with nobody to delegate to
    /// (<c>CONDITIONAL_MENU</c> → <c>ApprovalDelegation/my-scope</c>), so an employee who manages
    /// no one never sees it — permissions decide who MAY, the probe decides who it APPLIES to.</para>
    ///
    /// <para>Idempotent; re-running cannot duplicate rows.</para>
    /// </remarks>
    public partial class SeedMyDelegationsPortalMenu : Migration
    {
        private const string Link = "/myDelegations";
        private const string OperationName = "My Delegations";
        /// <summary>Anchor: an existing portal self-service item, to borrow its module.</summary>
        private const string Sibling = "/annualLeave";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM Core.Operation WHERE Link = '{Link}')
BEGIN
    INSERT INTO Core.Operation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
    SELECT NEWID(), o.ModuleId, N'{OperationName}', '{Link}', N'', 'UserCheck',
           ISNULL((SELECT MAX(x.DisplayOrder) FROM Core.Operation x WHERE x.ModuleId = o.ModuleId), 0) + 1,
           1, SYSUTCDATETIME(), 'migration', 0x0000000000000001
    FROM Core.Operation o
    WHERE o.Link = '{Sibling}';
END;");

            migrationBuilder.Sql($@"
INSERT INTO Core.TenantOperation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), t.ModuleId, N'{OperationName}', '{Link}', N'', 'UserCheck',
       ISNULL((SELECT MAX(x.DisplayOrder) FROM Core.TenantOperation x WHERE x.ModuleId = t.ModuleId), 0) + 1,
       1, SYSUTCDATETIME(), 'migration', 0x0000000000000001
FROM Core.TenantOperation t
WHERE t.Link = '{Sibling}'
  AND NOT EXISTS (SELECT 1 FROM Core.TenantOperation x
                  WHERE x.Link = '{Link}' AND x.ModuleId = t.ModuleId);");

            // Grant it exactly where the sibling self-service screen is granted. Arranging your own
            // cover is an ordinary employee action, so it follows Annual Leave rather than the
            // administrative delegation register — a department head holds the first and not the
            // second, which is the entire reason this screen exists.
            migrationBuilder.Sql($@"
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), p.TenantRoleId, d.Id, 1, 1, 1, 0, 0, 0, SYSUTCDATETIME(), 'migration', 0x0000000000000001
FROM Core.TenantRolePermission p
JOIN Core.TenantOperation src ON src.Id = p.TenantOperationId AND src.Link = '{Sibling}'
JOIN Core.TenantOperation d   ON d.Link = '{Link}' AND d.ModuleId = src.ModuleId
WHERE p.CanView = 1
  AND NOT EXISTS (SELECT 1 FROM Core.TenantRolePermission x
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
