<#
    compare-schemas.ps1 -- 2026-08-15

    Compares the SHARED surface of CERP against cybererp_srms across three dimensions:
    COLUMNS, FOREIGN KEYS and INDEXES/KEYS. Prints one verified difference count per dimension.

    !! WHY THIS EXISTS AS A SCRIPT.

    Three separate ad-hoc comparisons during this alignment reported "0 differences" and were WRONG:

      * the index harness built its column list with FOR XML PATH, returned nothing, and the
        `Where-Object { $_ -match '~' }` filter swallowed the empty result;
      * the foreign-key harness hit a COLLATION CONFLICT (sys columns carry mixed collations, so
        `fk.name + fk.delete_referential_action_desc` is rejected outright) and returned only an
        error, which the same filter swallowed;
      * both left two EMPTY dictionaries, and two empty dictionaries compare as identical.

    A comparison that returns nothing is indistinguishable from one that passes. This script fixes
    both root causes and, more importantly, ASSERTS ITS LOAD COUNTS before reporting anything -- a
    zero is only trustworthy when you know the query actually returned rows.

    Every concatenation of a system column is wrapped in COLLATE DATABASE_DEFAULT. Do not remove it.

    Usage:  pwsh -File compare-schemas.ps1 [-Server "..."] [-Left CERP] [-Right cybererp_srms]
#>

param(
    [string]$Server = "CLOUDX-SICS2\SQLEXPRESS",
    [string]$Left   = "CERP",
    [string]$Right  = "cybererp_srms"
)

$ErrorActionPreference = "Stop"

# The 30 tables both databases have. CERP's ~190 HRMS-domain tables are out of scope by design.
$Shared = @(
    'FiscalYear','LoginTrail','LookUpCategory','LookUpCategoryList','Module','Notification',
    'Operation','Organization','OrganizationSubscription','Person','Role','SalaryScale','Setting',
    'Step','SubscriptionPlan','SubscriptionPlanModule','Subsystem','Tenant',
    'TenantModule','TenantOperation','TenantRole','TenantRolePermission','TenantSubscription',
    'TenantSubscriptionAddOn','TenantSubSystem','TenantUser','TenantUserRole','User',
    'UserPreference','UserRole'
)

function Invoke-Pairs {
    param([string]$Db, [string]$Query)
    $h = @{}
    $raw = sqlcmd -S $Server -d $Db -h -1 -W -s"~" -Q $Query
    foreach ($line in $raw) {
        if ($line -notmatch '~') { continue }
        $p = $line.Split('~')
        $h[$p[0].Trim()] = ($p[1..($p.Count-1)] -join '~').Trim()
    }
    return $h
}

function Compare-Dimension {
    param([string]$Name, [string]$Query, [int]$MinRows = 1)

    $l = Invoke-Pairs -Db $Left  -Query $Query
    $r = Invoke-Pairs -Db $Right -Query $Query

    # !! THE ASSERTION. Without it an errored or empty query reports a perfect score.
    if ($l.Count -lt $MinRows -or $r.Count -lt $MinRows) {
        Write-Host ""
        Write-Host "  !! $Name HARNESS FAILED -- loaded $Left=$($l.Count), $Right=$($r.Count)." -ForegroundColor Red
        Write-Host "     A zero here would be meaningless. Run the query by hand to see the error." -ForegroundColor Red
        return $null
    }

    $diffs = @()
    foreach ($k in ($l.Keys + $r.Keys | Sort-Object -Unique)) {
        $table = ($k -split '\|')[0]
        if ($Shared -notcontains $table) { continue }
        $lv = if ($l.ContainsKey($k)) { $l[$k] } else { '<absent>' }
        $rv = if ($r.ContainsKey($k)) { $r[$k] } else { '<absent>' }
        if ($lv -ne $rv) { $diffs += [pscustomobject]@{ Key = $k; Left = $lv; Right = $rv } }
    }

    Write-Host ""
    Write-Host "  $Name -- loaded $Left=$($l.Count), $Right=$($r.Count)  ->  $($diffs.Count) difference(s)" -ForegroundColor Cyan
    foreach ($d in $diffs) {
        Write-Host ("    {0}" -f $d.Key)
        Write-Host ("        {0,-14} {1}" -f $Left, $d.Left)
        Write-Host ("        {0,-14} {1}" -f $Right, $d.Right)
    }
    return $diffs.Count
}

# ---- COLUMNS: name, type, size, nullability, default ---------------------------------------
$qColumns = @"
SET NOCOUNT ON;
SELECT CAST(t.name AS nvarchar(200)) COLLATE DATABASE_DEFAULT + '|'
     + CAST(c.name AS nvarchar(200)) COLLATE DATABASE_DEFAULT AS K,
       CAST(ty.name AS nvarchar(100)) COLLATE DATABASE_DEFAULT
     + ' size=' + CAST(CASE WHEN ty.name LIKE '%char%' AND c.max_length > 0 THEN c.max_length/2
                            WHEN ty.name IN ('datetime2','time','datetimeoffset') THEN c.scale
                            WHEN ty.name IN ('decimal','numeric') THEN c.precision*100 + c.scale
                            ELSE 0 END AS varchar(12)) COLLATE DATABASE_DEFAULT
     + ' null=' + CAST(c.is_nullable AS varchar(1)) COLLATE DATABASE_DEFAULT
     + ' default=' + ISNULL(CAST(dc.definition AS nvarchar(400)) COLLATE DATABASE_DEFAULT, '-') AS V
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE s.name = 'Core';
"@

# ---- FOREIGN KEYS: column -> referenced table, plus name and delete action ------------------
$qForeignKeys = @"
SET NOCOUNT ON;
SELECT CAST(OBJECT_NAME(fk.parent_object_id) AS nvarchar(200)) COLLATE DATABASE_DEFAULT + '|'
     + CAST(c.name AS nvarchar(200)) COLLATE DATABASE_DEFAULT + '->'
     + CAST(OBJECT_NAME(fk.referenced_object_id) AS nvarchar(200)) COLLATE DATABASE_DEFAULT AS K,
       CAST(fk.name AS nvarchar(300)) COLLATE DATABASE_DEFAULT
     + ' ondelete=' + CAST(fk.delete_referential_action_desc AS nvarchar(60)) COLLATE DATABASE_DEFAULT AS V
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) = 'Core';
"@

# ---- INDEXES AND KEYS ----------------------------------------------------------------------
# Column lists are aggregated with STRING_AGG rather than FOR XML PATH: the latter is what silently
# returned nothing the first time this was attempted.
$qIndexes = @"
SET NOCOUNT ON;
SELECT CAST(OBJECT_NAME(i.object_id) AS nvarchar(200)) COLLATE DATABASE_DEFAULT + '|'
     + CAST(i.name AS nvarchar(300)) COLLATE DATABASE_DEFAULT AS K,
       CAST(i.type_desc AS nvarchar(60)) COLLATE DATABASE_DEFAULT
     + ' uniq=' + CAST(i.is_unique AS varchar(1)) COLLATE DATABASE_DEFAULT
     + ' pk=' + CAST(i.is_primary_key AS varchar(1)) COLLATE DATABASE_DEFAULT
     + ' cols=' + STRING_AGG(CAST(c.name AS nvarchar(200)) COLLATE DATABASE_DEFAULT
                             + CASE WHEN ic.is_included_column = 1 THEN '(incl)' ELSE '' END, ',')
                  WITHIN GROUP (ORDER BY ic.is_included_column, ic.key_ordinal) AS V
FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE OBJECT_SCHEMA_NAME(i.object_id) = 'Core'
  AND i.name IS NOT NULL
  AND OBJECTPROPERTY(i.object_id, 'IsUserTable') = 1
GROUP BY i.object_id, i.name, i.type_desc, i.is_unique, i.is_primary_key;
"@

Write-Host ""
Write-Host "Comparing $Left  vs  $Right   (Core schema, $($Shared.Count) shared tables)" -ForegroundColor White

$results = [ordered]@{
    'COLUMNS'      = Compare-Dimension -Name 'COLUMNS'      -Query $qColumns     -MinRows 100
    'FOREIGN KEYS' = Compare-Dimension -Name 'FOREIGN KEYS' -Query $qForeignKeys -MinRows 10
    'INDEXES/KEYS' = Compare-Dimension -Name 'INDEXES/KEYS' -Query $qIndexes     -MinRows 20
}

Write-Host ""
Write-Host "---- SUMMARY ----" -ForegroundColor White
$failed = $false
foreach ($k in $results.Keys) {
    if ($null -eq $results[$k]) { Write-Host ("  {0,-14} HARNESS FAILED" -f $k) -ForegroundColor Red; $failed = $true }
    else { Write-Host ("  {0,-14} {1}" -f $k, $results[$k]) }
}
if ($failed) { Write-Host "  (a failed dimension is NOT a pass -- see above)" -ForegroundColor Red }
Write-Host ""
