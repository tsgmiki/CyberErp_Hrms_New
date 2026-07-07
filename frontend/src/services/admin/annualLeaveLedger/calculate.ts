import { api } from "@/utils/apiClient";

/** Generates (persists) the ledger entitlements for all eligible employees under a setting. */
export default async function calculateAnnualLeaveLedger(settingId: string) {
  return api.post<{ count: number; message: string }>("AnnualLeaveLedger/calculate", { settingId });
}
