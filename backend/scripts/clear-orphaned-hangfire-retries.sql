/*
 * Removes entries from HangFire's "retries" set whose job row no longer exists.
 *
 * WHY
 *   The retries set holds job ids awaiting another attempt. Succeeded/failed Job rows are expired
 *   and removed by HangFire's own ExpirationManager, but these set entries carry ExpireAt = NULL,
 *   so when their job row goes the entry stays — permanently. Nothing can run from one (HangFire
 *   loads the job, finds nothing, discards it), but they accumulate and inflate the Retries count
 *   on the dashboard, which makes a real backlog harder to notice.
 *
 *   Found while auditing for jobs stranded by the 2026-08-10 purge (logic §12.74). These are
 *   accumulated failure history rather than purge fallout specifically — the oldest ids (3, 9, 12,
 *   13) predate it.
 *
 * SAFETY — the NOT EXISTS is the whole property.
 *   Only entries with NO corresponding HangFire.Job row are touched. An entry whose job still
 *   exists is a live retry and is left completely alone, whatever its state. A row whose Value is
 *   not an integer is also left alone rather than guessed at.
 *
 *   Run it any time; it does not need the application stopped. Worst case a job is created between
 *   the check and the delete, and that job's entry simply is not matched by NOT EXISTS.
 *
 * Idempotent: a second run deletes nothing.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- LEFT JOIN rather than NOT EXISTS inside SUM: SQL Server cannot aggregate over a subquery
-- ("Cannot perform an aggregate function on an expression containing an aggregate or a subquery"),
-- and because that is a COMPILE-time error the whole batch would fail before the DELETE ran.
SELECT 'BEFORE' AS Stage,
       COUNT(*) AS TotalRetryEntries,
       SUM(CASE WHEN j.Id IS NULL THEN 1 ELSE 0 END) AS OrphanEntries
FROM HangFire.[Set] s
LEFT JOIN HangFire.Job j ON j.Id = TRY_CAST(s.Value AS int)
WHERE s.[Key] = 'retries';

-- Named, so the run is auditable.
SELECT 'TO DELETE' AS Stage, s.Value AS JobId, s.Score
FROM HangFire.[Set] s
WHERE s.[Key] = 'retries'
  AND TRY_CAST(s.Value AS int) IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM HangFire.Job j WHERE j.Id = TRY_CAST(s.Value AS int))
ORDER BY TRY_CAST(s.Value AS int);

BEGIN TRAN;

DELETE s
FROM HangFire.[Set] s
WHERE s.[Key] = 'retries'
  AND TRY_CAST(s.Value AS int) IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM HangFire.Job j WHERE j.Id = TRY_CAST(s.Value AS int));

COMMIT;

SELECT 'AFTER' AS Stage,
       COUNT(*) AS TotalRetryEntries,
       SUM(CASE WHEN j.Id IS NULL THEN 1 ELSE 0 END) AS OrphanEntries
FROM HangFire.[Set] s
LEFT JOIN HangFire.Job j ON j.Id = TRY_CAST(s.Value AS int)
WHERE s.[Key] = 'retries';

/* Live retries that were preserved (expect: any entry whose job row still exists). */
SELECT 'KEPT (live retries)' AS Stage, s.Value AS JobId, ISNULL(j.StateName, '?') AS JobState
FROM HangFire.[Set] s
JOIN HangFire.Job j ON j.Id = TRY_CAST(s.Value AS int)
WHERE s.[Key] = 'retries';
