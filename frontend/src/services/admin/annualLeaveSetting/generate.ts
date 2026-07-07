import { api } from "@/utils/apiClient";

/** Generates service-length-based entitlements for all active employees (idempotent). */
export default async function generateEntitlements(settingId: string) {
  return api.post<{ message: string; count: number }>(`AnnualLeaveSetting/${settingId}/generate-entitlements`, {});
}
