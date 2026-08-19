import { api } from "@/utils/apiClient";
import errorMessageParser from "@/components/util/errorMessageParser";
import type { SettingModel, TestEmailResultModel } from "@/models";

/**
 * Deployment operations settings (`Core.Setting`) — a SINGLETON, so there is no getAll/getById/
 * delete here and no paged-query factory: one GET, one PUT, plus the relay test.
 */

export interface ActionResult { ok: boolean; message: string }

/** The one settings row, with the SMTP values the server will ACTUALLY relay through. */
export const getSetting = () => api.get<SettingModel>("Setting");

/**
 * Upserts the row. Only the editable fields are sent — `hasSmtpPassword` and `emailEnabled` are
 * report-only, and posting them back would imply this screen can change deployment configuration.
 */
export async function saveSetting(model: SettingModel): Promise<ActionResult> {
  try {
    const res = await api.put<{ message?: string }>("Setting", {
      smtpHost: model.smtpHost ?? "",
      smtpPort: Number(model.smtpPort ?? 587),
      smtpUser: model.smtpUser ?? "",
      smtpUseTls: model.smtpUseTls ?? true,
      autoBackup: model.autoBackup ?? false,
      backupFrequency: model.backupFrequency || "daily",
      backupRetentionDays: Number(model.backupRetentionDays ?? 0),
    });
    return { ok: true, message: res?.message ?? "Settings saved" };
  } catch (error) {
    return { ok: false, message: parseError(error) };
  }
}

/**
 * Queues one message to prove the relay works, and reports which host and user were resolved.
 *
 * A refusal is a NORMAL outcome here, not an error: the server answers 200 with `queued: false` and
 * the reason (e-mail disabled for the deployment, no host, missing password). Only a transport or
 * permission failure lands in the catch.
 */
export async function sendTestEmail(to: string): Promise<TestEmailResultModel> {
  try {
    return await api.post<TestEmailResultModel>("Setting/test-email", { to });
  } catch (error) {
    return { queued: false, message: parseError(error) };
  }
}

/** apiClient throws with the raw response body; unwrap the server's own message where there is one. */
function parseError(error: unknown): string {
  const text = error instanceof Error ? error.message : String(error);
  try {
    const parsed = JSON.parse(text);
    return errorMessageParser(parsed.errors || parsed) || "Request failed";
  } catch {
    return text || "Request failed";
  }
}
