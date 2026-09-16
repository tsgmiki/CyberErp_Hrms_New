import { api } from "@/utils/apiClient";

export interface RecalculateLedgerResult {
  examined: number;
  raised: number;
  lowered: number;
  created: number;
  netChange: number;
  /** Employee numbers now showing a negative balance because entitlement fell below days taken. */
  overTaken: string[];
  message: string;
}

/**
 * Re-applies the policy to entitlements that have ALREADY been generated.
 *
 * Distinct from `calculate`, which skips anyone who already has a balance and therefore cannot
 * repair figures produced by an earlier version of the accrual rules.
 */
export default async function recalculateAnnualLeaveLedger(settingId: string) {
  return api.post<RecalculateLedgerResult>("AnnualLeaveLedger/recalculate", { settingId });
}
