using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Core.TenantSubSystem: Status becomes a bit — 1 = the subsystem entitlement is live (2026-08-19).
    /// </summary>
    /// <remarks>
    /// <para>⚠️ It collapses a FIVE-value subscription state — Trial / Active / Suspended / Cancelled
    /// / Expired — into two. An entitlement that is merely on trial is now indistinguishable from a
    /// fully paid one, and a cancelled entitlement from an expired one. The dates carry some of that
    /// meaning (<c>TrialEndDate</c>, <c>EndDate</c>) but nothing records WHY an entitlement stopped.
    /// No data is lost today: all 3 rows are 'Active'.</para>
    ///
    /// <para>⚠️ The <c>SubscriptionStatuses</c> constants are NOT removed. They are shared with
    /// <c>OrganizationSubscription.Status</c> and <c>TenantSubscriptionAddOn.Status</c>, which are
    /// different tables and keep the five-value string. Only this column changes.</para>
    ///
    /// <para>⚠️ SRMS models the same concept as a six-value enum
    /// <c>SubscriptionStatus { Pending, Trial, Active, Suspended, Expired, Cancelled }</c>. The SRMS
    /// copy under active development points at <c>CERP_Latest</c>, a different database, so it is
    /// unaffected; the two older copies point at CERP and would be.</para>
    /// </remarks>
    // Hand-written, so it carries its own [Migration] attribute: EF discovers migrations by that
    // attribute, which the scaffolder normally emits into the generated .Designer.cs. Without it the
    // file compiles, sits in the folder, and is silently never applied.
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260819160000_TenantSubSystemStatusBit")]
    public partial class TenantSubSystemStatusBit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⚠️ SPLIT INTO SEPARATE Sql() CALLS ON PURPOSE. A plain AlterColumn to bit fails —
            // SQL Server cannot convert 'Active' — so the column is rebuilt; and the rebuild cannot
            // be one call, because SQL Server compiles a whole batch before running any of it, so an
            // UPDATE naming a column added by an earlier ALTER in the SAME batch fails to parse
            // ("Invalid column name 'Status_bit'"). GO is a client directive, not T-SQL, and EF does
            // not accept it — one Sql() call per batch is how you get the same effect.
            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantSubSystem] ADD [Status_bit] bit NOT NULL CONSTRAINT [DF_TenantSubSystem_Status_bit] DEFAULT 1;");

            // Trial counts as live: a trial entitlement grants access, which is what this column now
            // means. Suspended, Cancelled and Expired do not.
            migrationBuilder.Sql(
                "UPDATE [Core].[TenantSubSystem] SET [Status_bit] = CASE WHEN LTRIM(RTRIM([Status])) IN ('Active', 'Trial') THEN 1 ELSE 0 END;");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantSubSystem] DROP COLUMN [Status];");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantSubSystem] DROP CONSTRAINT [DF_TenantSubSystem_Status_bit];");

            migrationBuilder.Sql(
                "EXEC sp_rename N'[Core].[TenantSubSystem].[Status_bit]', N'Status', N'COLUMN';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The reverse cannot tell Trial from Active, or Cancelled from Expired. Everything live
            // comes back as 'Active' and everything else as 'Suspended' — the reversible states,
            // rather than inventing a terminal one the row may never have been in.
            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantSubSystem] ADD [Status_text] nvarchar(30) NOT NULL CONSTRAINT [DF_TenantSubSystem_Status_text] DEFAULT N'Active';");

            migrationBuilder.Sql(
                "UPDATE [Core].[TenantSubSystem] SET [Status_text] = CASE WHEN [Status] = 1 THEN N'Active' ELSE N'Suspended' END;");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantSubSystem] DROP COLUMN [Status];");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantSubSystem] DROP CONSTRAINT [DF_TenantSubSystem_Status_text];");

            migrationBuilder.Sql(
                "EXEC sp_rename N'[Core].[TenantSubSystem].[Status_text]', N'Status', N'COLUMN';");
        }
    }
}
