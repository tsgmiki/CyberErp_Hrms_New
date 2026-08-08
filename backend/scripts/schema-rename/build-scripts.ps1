<#
    Builds the reviewable schema-rename SQL for both applications.

    Why the post-processing step: `dotnet ef migrations script` writes migrationBuilder.Sql() content
    verbatim and does NOT insert GO between operations. The 28 report procedures would therefore all
    land in a single batch and fail with "CREATE PROCEDURE must be the first statement in a query
    batch". Each procedure statement in the migration begins with a "-- ===BATCH===" sentinel, which
    this script converts into a real GO separator. (Applying the migration with
    `dotnet ef database update` instead does not need this — there each Sql() is already its own
    command and the sentinel is just a comment.)

    Run from the repository root of the HRMS checkout. Produces:
      01-hrms-module-schema-rename.sql   (this repo)
      02-home-notification-schema.sql    (the Home repo)
#>
param(
    [string]$HrmsRoot = "D:\Workspace\CyberErp\Hrms\backend",
    [string]$HomeRoot = "D:\Workspace\CyberErp\Home\backend"
)

$ErrorActionPreference = "Stop"
$hrmsOut = Join-Path $HrmsRoot "scripts\schema-rename\01-hrms-module-schema-rename.sql"
$homeOut = Join-Path $HomeRoot "scripts\schema-rename\02-home-notification-schema.sql"

# NOT --idempotent, deliberately. Idempotent output wraps each migration in
# `IF NOT EXISTS(...) BEGIN ... END`, and the GO separators the procedures require would split that
# block mid-way ("Incorrect syntax near 'BEGIN'"). Batch separators and idempotent guards cannot
# coexist. A precondition guard is prepended below instead, so running this against a database at
# the wrong migration fails fast rather than half-applying.
Push-Location $HrmsRoot
dotnet ef migrations script SalaryRevisionPerformanceBands ModuleSchemaRename `
    -p CyberErp.Hrms.Inf -s CyberErp.Hrms.Api -o $hrmsOut | Out-Null
Pop-Location

# sentinel -> real batch separator
$sql = Get-Content -Raw -Path $hrmsOut
$sql = $sql -replace '(?m)^\s*--\s*===BATCH===\s*$', 'GO'

# Fail fast if the target is not exactly where this script expects it. Without the idempotent guards
# a mis-targeted run would half-apply, which on 180 table renames is a bad place to stop.
$guard = @'
/* ---------------------------------------------------------------------------
   Precondition. This script is NOT idempotent (see build-scripts.ps1 for why),
   so it refuses to run unless the database is exactly at
   20260808112235_SalaryRevisionPerformanceBands and has not already been renamed.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory]
               WHERE [MigrationId] = N'20260808112235_SalaryRevisionPerformanceBands')
BEGIN
    RAISERROR('ABORTED: database is not at SalaryRevisionPerformanceBands. Apply earlier HRMS migrations first.', 16, 1);
    SET NOEXEC ON;
END
GO
IF EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] LIKE N'%_ModuleSchemaRename')
BEGIN
    RAISERROR('ABORTED: ModuleSchemaRename has already been applied to this database.', 16, 1);
    SET NOEXEC ON;
END
GO

'@
$footer = "`r`nGO`r`nSET NOEXEC OFF;`r`nGO`r`n"
Set-Content -Path $hrmsOut -Value ($guard + $sql + $footer) -Encoding UTF8
"01-hrms: $((Get-Content $hrmsOut).Count) lines, $((Select-String -Path $hrmsOut -Pattern '^GO$').Count) batches"

# Home is generated idempotently over ALL its migrations on purpose: the CERP database was found to
# be missing 20260802082344_AddNotificationUserFeedIndex, so a ranged script would try to rename an
# index that does not exist. Guarded from the start, this applies whatever the target is missing.
Push-Location $HomeRoot
dotnet ef migrations script --idempotent `
    -p CyberErp.Home.Inf -s CyberErp.Home.Api -o $homeOut | Out-Null
Pop-Location
"02-home: $((Get-Content $homeOut).Count) lines"
