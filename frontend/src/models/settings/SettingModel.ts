/**
 * Deployment operations settings — the single `Core.Setting` row (GET/PUT `Setting`).
 *
 * A SINGLETON: no id, no list. `hasSmtpPassword` / `emailEnabled` are REPORTED ONLY and are not
 * sent back on save.
 *
 * ⚠️ There is no smtpPassword field, in either direction, and there must not be. The credential
 * lives in configuration (user-secrets locally, environment variables elsewhere) and is never
 * returned to a client; `hasSmtpPassword` says only whether one exists, so the screen can warn.
 */
export default interface SettingModel {
  smtpHost?: string;
  smtpPort?: number | string;
  smtpUser?: string;
  smtpUseTls?: boolean;
  autoBackup?: boolean;
  /** "daily" | "weekly" | "monthly" — free text on the wire, chosen from a fixed list here. */
  backupFrequency?: string;
  backupRetentionDays?: number | string;

  /** Read-only: true when Email:Password is configured. Mail cannot authenticate without it. */
  hasSmtpPassword?: boolean;
  /** Read-only: the Email:Enabled master switch — a deployment concern, not editable here. */
  emailEnabled?: boolean;
}

/** What `Setting/test-email` reports back: whether it was queued, and through which relay. */
export interface TestEmailResultModel {
  queued: boolean;
  message: string;
  /** The host the message will ACTUALLY be relayed through — the point of the test. */
  resolvedHost?: string | null;
  resolvedUser?: string | null;
}
