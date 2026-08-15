using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// <para>Constrains <c>Core.Tenant.TenantTypeId</c> to <c>Core.LookUpCategoryList</c>, matching
    /// cybererp_srms's <c>FK_Tenant_LookUpCategoryList</c> — which CERP was missing entirely.</para>
    ///
    /// <para>⚠️ RAW SQL, not the EF model. CERP has TWO lookup systems: <c>Core.LookUpCategory/List</c>
    /// mirrors the SRMS platform schema, while <c>Hrms.LookUpCategory/List</c> is the HRMS domain one
    /// that the <c>LookupCategoryList</c> entity maps (education levels, fields of study). A tenant
    /// TYPE is platform data, so the constraint must point at the <b>Core</b> table — and EF cannot
    /// express that while the entity maps the Hrms one. Mapping it through EF silently produced a
    /// foreign key to <c>Hrms.LookUpCategoryList</c>, which is the wrong table.</para>
    ///
    /// <para>Safe: all three tenant rows hold NULL, and NULLs are exempt from a foreign key. Restrict
    /// on delete, so a lookup value in use by a tenant cannot be removed underneath it.</para>
    /// </summary>
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260815000500_TenantTypeIdForeignKey")]
    public partial class TenantTypeIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Core.Tenant') AND name = 'IX_Tenant_TenantTypeId')
    CREATE INDEX IX_Tenant_TenantTypeId ON Core.Tenant (TenantTypeId);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Tenant_LookUpCategoryList')
    ALTER TABLE Core.Tenant ADD CONSTRAINT FK_Tenant_LookUpCategoryList
        FOREIGN KEY (TenantTypeId) REFERENCES Core.LookUpCategoryList (Id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Tenant_LookUpCategoryList')
    ALTER TABLE Core.Tenant DROP CONSTRAINT FK_Tenant_LookUpCategoryList;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Core.Tenant') AND name = 'IX_Tenant_TenantTypeId')
    DROP INDEX IX_Tenant_TenantTypeId ON Core.Tenant;");
        }
    }
}
