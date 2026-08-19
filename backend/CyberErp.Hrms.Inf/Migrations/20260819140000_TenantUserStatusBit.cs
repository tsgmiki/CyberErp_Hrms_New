using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Core.TenantUser: Status becomes a bit — 1 = an active membership (2026-08-19).
    /// </summary>
    /// <remarks>
    /// <para>⚠️ THIS DIVERGES FROM SRMS, WHICH SHARES THIS DATABASE AND WRITES THIS TABLE.
    /// Memberships are created directly in Core.TenantUser / Core.TenantUserRole by SRMS (see
    /// TenantAuthorizationProjector), and SRMS stores the string 'Active'. After this migration an
    /// SRMS insert of 'Active' fails on the bit column, and its reads compare a bit to a string.
    /// SRMS must be updated to match. Applied deliberately on that understanding, exactly as the
    /// Core.User.AccountStatus change was (handoff 0133).</para>
    ///
    /// <para>⚠️ It also collapses a THREE-value state — Active / Suspended / Invited — into two.
    /// 'Suspended' and 'Invited' become indistinguishable from any other non-active value. No data
    /// is lost today: all 499 rows are 'Active' at the time of writing.</para>
    /// </remarks>
    // Hand-written, so it carries its own [Migration] attribute: EF discovers migrations by that
    // attribute, which the scaffolder normally puts in the generated .Designer.cs file. Without it
    // the file compiles, sits in the folder, and is silently never applied.
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260819140000_TenantUserStatusBit")]
    public partial class TenantUserStatusBit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⚠️ HAND-WRITTEN, AND DELIBERATELY SPLIT INTO SEPARATE Sql() CALLS.
            //
            // A plain AlterColumn to bit fails: SQL Server cannot convert the string 'Active' to bit.
            // So the column is rebuilt with an explicit mapping.
            //
            // The rebuild CANNOT be one Sql() call. SQL Server compiles a whole batch before running
            // any of it, so an UPDATE naming a column that an earlier ALTER in the SAME batch adds
            // fails to parse: "Invalid column name 'Status_bit'". Normally you would separate them
            // with GO, but GO is a client directive, not T-SQL, and EF does not accept it. One
            // Sql() call per batch is the way to get the same effect.
            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantUser] ADD [Status_bit] bit NOT NULL CONSTRAINT [DF_TenantUser_Status_bit] DEFAULT 1;");

            // Anything that is not exactly 'Active' becomes 0. Suspended and Invited memberships are
            // inactive as far as a boolean can say.
            migrationBuilder.Sql(
                "UPDATE [Core].[TenantUser] SET [Status_bit] = CASE WHEN LTRIM(RTRIM([Status])) = 'Active' THEN 1 ELSE 0 END;");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantUser] DROP COLUMN [Status];");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantUser] DROP CONSTRAINT [DF_TenantUser_Status_bit];");

            migrationBuilder.Sql(
                "EXEC sp_rename N'[Core].[TenantUser].[Status_bit]', N'Status', N'COLUMN';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The reverse cannot restore Suspended vs Invited — that distinction is gone. Every
            // inactive membership comes back as 'Suspended', which is the safer of the two to
            // assume: 'Invited' would imply the user may still accept and become active.
            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantUser] ADD [Status_text] nvarchar(30) NOT NULL CONSTRAINT [DF_TenantUser_Status_text] DEFAULT N'Active';");

            migrationBuilder.Sql(
                "UPDATE [Core].[TenantUser] SET [Status_text] = CASE WHEN [Status] = 1 THEN N'Active' ELSE N'Suspended' END;");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantUser] DROP COLUMN [Status];");

            migrationBuilder.Sql(
                "ALTER TABLE [Core].[TenantUser] DROP CONSTRAINT [DF_TenantUser_Status_text];");

            migrationBuilder.Sql(
                "EXEC sp_rename N'[Core].[TenantUser].[Status_text]', N'Status', N'COLUMN';");
        }
    }
}
