import { api } from "@/utils/apiClient";

export interface RolloverResult {
  balancesRolled: number;
  totalCarried: number;
  totalExpired: number;
  message: string;
}

/**
 * Year-end rollover for the fiscal year this setting governs: carries remaining balances forward up
 * to the setting's carry-forward cap, expires over-aged carry, and CLOSES the fiscal year.
 *
 * Takes the SETTING id, not the fiscal year's. Moved here from the Fiscal Year screen — the cap and
 * the expiry rule are both fields of this policy (logic §12.97).
 */
export default async function rolloverLeaveSetting(settingId: string) {
  return api.post<RolloverResult>(`AnnualLeaveSetting/${settingId}/rollover-leave`, {});
}
