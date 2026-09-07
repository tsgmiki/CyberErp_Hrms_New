/*
 * Removes the queued Hangfire jobs that still invoke TripSettlementReminder.RunAsync.
 *
 * WHY
 *   RunAsync carries the HR-only guard for the on-demand endpoint. A Hangfire worker has no HTTP
 *   context, so the signed-in user is null, IsAdminAsync returns false at its first line, and the
 *   job threw "Only HR can run the settlement reminders." on every attempt — surfacing in the
 *   console each time the application starts and the worker picks the job back up (logic §12.73).
 *
 *   The code fix repoints the recurring job at RunUnattendedAsync, and RecurringJob.AddOrUpdate
 *   rewrites the stored definition on the next startup. But jobs ALREADY ENQUEUED carry their own
 *   serialised InvocationData naming the old method, so they keep failing and retrying (10 attempts,
 *   backing off ~25 minutes) regardless of the fix. This clears them.
 *
 * NOTHING IS LOST. These jobs are guaranteed to fail, and the recurring schedule re-enqueues a
 *   fresh one — with the corrected method — at 02:00 UTC. No reminder that would otherwise have
 *   been sent is discarded, because none of these attempts ever sent one.
 *
 * SCOPE. Only jobs whose InvocationData names BOTH TripSettlementReminder and RunAsync, and that
 *   are not already in a terminal state. Terminal rows are left as history.
 *
 * Run AFTER deploying the code fix, so the schedule re-creates the job against the new method.
 */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @doomed TABLE (JobId int PRIMARY KEY);

INSERT INTO @doomed (JobId)
SELECT j.Id
FROM HangFire.Job j
WHERE j.InvocationData LIKE '%TripSettlementReminder%'
  AND j.InvocationData LIKE '%"m":"RunAsync"%'
  AND ISNULL(j.StateName, '') NOT IN ('Succeeded', 'Deleted');

SELECT 'BEFORE' AS Stage, j.Id AS JobId, j.StateName, j.CreatedAt
FROM HangFire.Job j JOIN @doomed d ON d.JobId = j.Id;

BEGIN TRAN;

DELETE q FROM HangFire.JobQueue     q JOIN @doomed d ON d.JobId = q.JobId;
DELETE s FROM HangFire.State        s JOIN @doomed d ON d.JobId = s.JobId;
DELETE p FROM HangFire.JobParameter p JOIN @doomed d ON d.JobId = p.JobId;
DELETE j FROM HangFire.Job          j JOIN @doomed d ON d.JobId = j.Id;

COMMIT;

SELECT 'AFTER: stale RunAsync jobs remaining (expect 0)' AS Stage, COUNT(*) AS Jobs
FROM HangFire.Job j
WHERE j.InvocationData LIKE '%TripSettlementReminder%'
  AND j.InvocationData LIKE '%"m":"RunAsync"%'
  AND ISNULL(j.StateName, '') NOT IN ('Succeeded', 'Deleted');

/*
 * The stored recurring definition still names RunAsync until the application restarts and
 * RecurringJob.AddOrUpdate rewrites it. Check it afterwards with:
 *
 *   SELECT [Key], Field, Value FROM HangFire.[Hash]
 *   WHERE [Key] = 'recurring-job:trip-settlement-reminders' AND Field = 'Job';
 *
 * It should read "m":"RunUnattendedAsync".
 */
