/*
 * Makes the Email Templates screen reachable.
 *
 * The screen was invisible for two linked reasons, both data rather than code:
 *   1. the sidebar is built from Core.TenantOperation, so a screen with no operation row
 *      simply is not in the menu;
 *   2. NotificationTemplateController is gated on "notificationTemplate", and gating on a link
 *      nobody holds denies EVERYONE — including an administrator.
 *
 * So this adds the operation to the template catalogue and to the tenant's copy, then grants it
 * to the HR Admin role with every privilege.
 *
 * Idempotent: every statement is guarded by NOT EXISTS, so re-running changes nothing.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rv          varbinary(8)     = 0x0000000000000001;
DECLARE @link        nvarchar(200)    = N'/hrms/notificationTemplate';
DECLARE @name        nvarchar(200)    = N'Email Templates';
DECLARE @icon        nvarchar(100)    = N'MailPlus';       -- resolved by the SPA's lucideIconMap
DECLARE @order       int              = 30;                -- System module, after Workflow Definitions
DECLARE @tplModule   uniqueidentifier = '964232D1-3D2C-4BCC-9079-DA405298C25E'; -- Core.Module 'System'
DECLARE @tenModule   uniqueidentifier = '6BFCDB2C-6F8A-4F78-AB68-414BC728A603'; -- Core.TenantModule 'System'
DECLARE @hrAdminRole uniqueidentifier = '826DC7CE-CCC1-4448-8016-70F73D7F8A1A'; -- Core.TenantRole 'HR Admin'

BEGIN TRAN;

/* 1. Template catalogue -------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM Core.Operation WHERE Link = @link)
    INSERT INTO Core.Operation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, RowVersion)
    VALUES (NEWID(), @tplModule, @name, @link, N'', @icon, @order, 1, SYSUTCDATETIME(), @rv);

/* 2. The tenant's own copy — this is what the sidebar reads --------------------------- */
IF NOT EXISTS (SELECT 1 FROM Core.TenantOperation WHERE Link = @link AND ModuleId = @tenModule)
    INSERT INTO Core.TenantOperation (Id, ModuleId, Name, Link, Filter, Icon, DisplayOrder, IsActive, CreatedAt, RowVersion)
    VALUES (NEWID(), @tenModule, @name, @link, N'', @icon, @order, 1, SYSUTCDATETIME(), @rv);

DECLARE @op uniqueidentifier =
    (SELECT TOP 1 Id FROM Core.TenantOperation WHERE Link = @link AND ModuleId = @tenModule);

/* 3. Grant it to HR Admin, with every privilege --------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM Core.TenantRolePermission
               WHERE TenantRoleId = @hrAdminRole AND TenantOperationId = @op)
    INSERT INTO Core.TenantRolePermission
        (Id, TenantRoleId, TenantOperationId, CanView, CanAdd, CanEdit, CanDelete, CanApprove, CanExport, CreatedAt, RowVersion)
    VALUES (NEWID(), @hrAdminRole, @op, 1, 1, 1, 1, 1, 1, SYSUTCDATETIME(), @rv);

COMMIT;

/* ---- what the change looks like ---------------------------------------------------- */
SELECT o.Name, o.Link, o.Icon, o.DisplayOrder, m.Name AS [Module]
FROM Core.TenantOperation o JOIN Core.TenantModule m ON m.Id = o.ModuleId
WHERE o.Link = @link;

SELECT r.Name AS [Role], p.CanView, p.CanAdd, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport
FROM Core.TenantRolePermission p
JOIN Core.TenantRole r ON r.Id = p.TenantRoleId
WHERE p.TenantOperationId = @op;
