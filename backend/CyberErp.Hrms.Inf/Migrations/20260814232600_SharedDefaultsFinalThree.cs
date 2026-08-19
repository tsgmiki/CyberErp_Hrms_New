using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// The last three default constraints, none of which EF can express (2026-08-15).
    ///
    /// <para><c>Role.Code</c> carries an <c>(N'')</c> default left by an older migration that the
    /// model never declared, so EF neither knows about it nor drops it. SRMS has none.</para>
    ///
    /// <para><c>Subsystem.Code</c> is a spelling difference: EF emits <c>N''</c> for a string default,
    /// SRMS stores <c>''</c>.</para>
    ///
    /// <para><c>TenantRolePermission.CanExport</c> is the opposite problem — <c>HasDefaultValue(false)</c>
    /// produces nothing, because <c>false</c> is the CLR default and EF optimises it away. SRMS has an
    /// explicit one.</para>
    /// </summary>
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260814232600_SharedDefaultsFinalThree")]
    public partial class SharedDefaultsFinalThree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @drop nvarchar(max) = N'';
SELECT @drop = @drop + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(dc.parent_object_id))
             + N'.' + QUOTENAME(OBJECT_NAME(dc.parent_object_id))
             + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE (dc.parent_object_id = OBJECT_ID('Core.Role') AND c.name = 'Code')
   OR (dc.parent_object_id = OBJECT_ID('Core.Subsystem') AND c.name = 'Code')
   OR (dc.parent_object_id = OBJECT_ID('Core.TenantRolePermission') AND c.name = 'CanExport');
IF @drop <> N'' EXEC sp_executesql @drop;

ALTER TABLE Core.Subsystem ADD CONSTRAINT DF_Subsystem_Code DEFAULT ('') FOR Code;
ALTER TABLE Core.TenantRolePermission ADD CONSTRAINT DF_TenantRolePermission_CanExport
    DEFAULT (CONVERT([bit],(0))) FOR CanExport;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @drop nvarchar(max) = N'';
SELECT @drop = @drop + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(dc.parent_object_id))
             + N'.' + QUOTENAME(OBJECT_NAME(dc.parent_object_id))
             + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE (dc.parent_object_id = OBJECT_ID('Core.Subsystem') AND c.name = 'Code')
   OR (dc.parent_object_id = OBJECT_ID('Core.TenantRolePermission') AND c.name = 'CanExport');
IF @drop <> N'' EXEC sp_executesql @drop;

ALTER TABLE Core.Role ADD CONSTRAINT DF_Role_Code DEFAULT (N'') FOR Code;
ALTER TABLE Core.Subsystem ADD CONSTRAINT DF_Subsystem_Code2 DEFAULT (N'') FOR Code;");
        }
    }
}
