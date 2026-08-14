/*
  Clears the SEEDED placeholder SMTP values from Core.Setting.

  WHY THIS IS NEEDED
  ------------------
  Core.Setting's SMTP columns became the source of truth on 2026-08-13 (logic.md §12.12). Until then
  nothing read them, so the row that exists was never entered by an administrator — it was seeded,
  and it carries `smtp.cyber.com` / `noreply@cybererp.com`, which is not a relay anyone has verified.

  Making the stored values win without this would redirect the organisation's mail from the working
  configured relay to a host that probably does not resolve — and the failure would be invisible,
  because sends happen in a background job.

  The resolver falls back to the Email configuration section FIELD BY FIELD, so blanking these
  restores exactly the previous behaviour: configuration supplies the relay until an administrator
  sets one deliberately through PUT /api/v1/Setting.

  Only touches rows that still hold the seeded values, so a real setting entered since is left alone.
  Idempotent. Backup/password/session policy are untouched.
*/

SET NOCOUNT ON;

UPDATE Core.Setting
   SET SmtpHost = '',
       SmtpUser = ''
 WHERE SmtpHost = 'smtp.cyber.com'
    OR SmtpUser = 'noreply@cybererp.com';

SELECT @@ROWCOUNT AS rows_cleared;

SELECT '[' + SmtpHost + ']' AS smtp_host_now,
       '[' + SmtpUser + ']' AS smtp_user_now,
       SmtpPort, SmtpUseTls
  FROM Core.Setting;
