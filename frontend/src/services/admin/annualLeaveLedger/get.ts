import type { AnnualLeaveLedgerModel } from "@/models";
import { api } from "@/utils/apiClient";

export default async function getAnnualLeaveLedger(settingId: string) {
  if (!settingId) return null;
  return api.get<AnnualLeaveLedgerModel>(`AnnualLeaveLedger?settingId=${encodeURIComponent(settingId)}`);
}
