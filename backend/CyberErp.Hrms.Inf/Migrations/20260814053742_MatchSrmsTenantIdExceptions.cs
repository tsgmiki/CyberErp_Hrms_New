using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Reverts TenantId to nvarchar on Core.LoginTrail and Core.UserPreference — the two tables where
    /// SRMS itself uses a string.
    ///
    /// <para>⚠️ THIS MIRRORS AN INCONSISTENCY IN THE SOURCE SCHEMA, DELIBERATELY. SRMS types TenantId
    /// as uniqueidentifier on seven tables and nvarchar on these two. The previous migration converted
    /// every column in CERP, which left these two "more consistent than the thing they are supposed to
    /// match". Matching exactly is the goal, so they come back — but the oddity is SRMS's, and if it
    /// is ever tidied up there, this migration and the exclusion in HrmsDbContext should both go.</para>
    ///
    /// <para>⚠️ EF scaffolded the ALTERs with no index handling, and UserPreference has a UNIQUE index
    /// on (UserId, TenantId). SQL Server will not alter a column an index depends on, so it is dropped
    /// and rebuilt here.</para>
    /// </summary>
    public partial class MatchSrmsTenantIdExceptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserPreference_UserId_TenantId'
           AND object_id = OBJECT_ID('Core.UserPreference'))
    DROP INDEX IX_UserPreference_UserId_TenantId ON Core.UserPreference;

ALTER TABLE Core.UserPreference ALTER COLUMN TenantId nvarchar(450) NOT NULL;
ALTER TABLE Core.LoginTrail     ALTER COLUMN TenantId nvarchar(max) NOT NULL;

-- The empty GUID means 'no tenant' and read back as string.Empty through the value converter.
-- As text it would become the literal '00000000-...', which no IsNullOrEmpty check would catch.
UPDATE Core.UserPreference SET TenantId = '' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Core.LoginTrail     SET TenantId = '' WHERE TenantId = '00000000-0000-0000-0000-000000000000';

CREATE UNIQUE INDEX IX_UserPreference_UserId_TenantId ON Core.UserPreference (UserId, TenantId);

COMMIT;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserPreference_UserId_TenantId'
           AND object_id = OBJECT_ID('Core.UserPreference'))
    DROP INDEX IX_UserPreference_UserId_TenantId ON Core.UserPreference;

UPDATE Core.UserPreference SET TenantId = '00000000-0000-0000-0000-000000000000' WHERE LTRIM(RTRIM(ISNULL(TenantId,''))) = '';
UPDATE Core.LoginTrail     SET TenantId = '00000000-0000-0000-0000-000000000000' WHERE LTRIM(RTRIM(ISNULL(TenantId,''))) = '';

ALTER TABLE Core.UserPreference ALTER COLUMN TenantId uniqueidentifier NOT NULL;
ALTER TABLE Core.LoginTrail     ALTER COLUMN TenantId uniqueidentifier NOT NULL;

CREATE UNIQUE INDEX IX_UserPreference_UserId_TenantId ON Core.UserPreference (UserId, TenantId);

COMMIT;
");
        }
    }
}
