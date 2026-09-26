using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Registers the Approval Delegation screen in the menu catalogue and grants it wherever
    /// Workflow Definitions is already granted.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ Without this the feature is unreachable. <c>ApprovalDelegationController</c> carries
    /// <c>[RequirePermission("approvalDelegation")]</c>, and the gate resolves that against
    /// <c>TenantOperation.Link</c> — an operation nobody has defined matches nothing, so every call
    /// answers 403 for every user including administrators. Shipping the code without the menu row
    /// is shipping a feature that is switched off in a way no setting can switch on.</para>
    ///
    /// <para>⚠️ Links are stored NAMESPACED (<c>/hrms/approvalDelegation</c>). The bare form matches
    /// nothing — the trap documented in <c>WorkflowApproverAuth.WorkflowOperationLinks</c>, which
    /// silently emptied the approval inbox for 22 of 27 workflows.</para>
    ///
    /// <para>Delegation rights are seeded from <b>Workflow Definitions</b> rather than from Workflow
    /// Tracking: arranging who may approve is an administrative act like editing the chain itself,
    /// not the everyday act of approving. Anyone who may not edit the chain does not silently gain
    /// the ability to re-route it.</para>
    ///
    /// <para>Idempotent, and written so re-running it cannot duplicate rows.</para>
    /// </remarks>
    public partial class SeedApprovalDelegationMenu : Migration
    {
        private const string Link = "/hrms/approvalDelegation";
        private const string OperationName = "Approval Delegation";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Catalogue row, in the same module as the workflow screens.
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM Core.Operation WHERE Link = '{Link}')
BEGIN
    INSERT INTO Core.Operation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
    SELECT NEWID(), o.ModuleId, N'{OperationName}', '{Link}', N'', 'user-check',
           ISNULL(MAX(o2.DisplayOrder), 0) + 1, 1, SYSUTCDATETIME(), 'migration', 0x0000000000000001
    FROM Core.Operation o
    LEFT JOIN Core.Operation o2 ON o2.ModuleId = o.ModuleId
    WHERE o.Link = '/hrms/workflowDefinition'
    GROUP BY o.ModuleId;
END;");

            // 2. Per-tenant projection, one row per tenant that already has the workflow screens.
            migrationBuilder.Sql($@"
INSERT INTO Core.TenantOperation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), t.ModuleId, N'{OperationName}', '{Link}', N'', 'user-check', t.DisplayOrder + 1, 1,
       SYSUTCDATETIME(), 'migration', 0x0000000000000001
FROM Core.TenantOperation t
WHERE t.Link = '/hrms/workflowDefinition'
  AND NOT EXISTS (SELECT 1 FROM Core.TenantOperation x
                  WHERE x.Link = '{Link}' AND x.ModuleId = t.ModuleId);");

            // 3. Grant it to every role that can already edit workflow definitions. A role that may
            //    not re-route an approval chain must not gain the right to delegate it away either.
            migrationBuilder.Sql($@"
INSERT INTO Core.TenantRolePermission
    (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport, CreatedAt, CreatedBy, RowVersion)
SELECT NEWID(), p.TenantRoleId, d.Id, p.CanView, p.CanAdd, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport,
       SYSUTCDATETIME(), 'migration', 0x0000000000000001
FROM Core.TenantRolePermission p
JOIN Core.TenantOperation src ON src.Id = p.TenantOperationId AND src.Link = '/hrms/workflowDefinition'
JOIN Core.TenantOperation d   ON d.Link = '{Link}' AND d.ModuleId = src.ModuleId
WHERE NOT EXISTS (SELECT 1 FROM Core.TenantRolePermission x
                  WHERE x.TenantRoleId = p.TenantRoleId AND x.TenantOperationId = d.Id);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Permissions first — they reference the operation rows.
            migrationBuilder.Sql($@"
DELETE p FROM Core.TenantRolePermission p
JOIN Core.TenantOperation o ON o.Id = p.TenantOperationId
WHERE o.Link = '{Link}';");
            migrationBuilder.Sql($"DELETE FROM Core.TenantOperation WHERE Link = '{Link}';");
            migrationBuilder.Sql($"DELETE FROM Core.Operation WHERE Link = '{Link}';");
        }
    }
}
