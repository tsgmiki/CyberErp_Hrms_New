import type { LeaveBalanceModel } from "@/models";
import { api } from "@/utils/apiClient";

/**
 * The employee's leave ledgers.
 *
 * @param selectableOnly pass true from a REQUEST FORM: the dropdown then offers only fiscal years
 * that may still be charged (active policy, year not closed). The HR balance screen leaves it off,
 * since adjusting a historical year is what that screen is for (HRMS logic §12.102).
 */
export default async function getLeaveBalances(
  employeeId: string,
  fiscalYearId?: string,
  selectableOnly = false,
) {
  if (!employeeId) return [] as LeaveBalanceModel[];
  const q = new URLSearchParams({ employeeId });
  if (fiscalYearId) q.append("fiscalYearId", fiscalYearId);
  if (selectableOnly) q.append("selectableOnly", "true");
  return api.get<LeaveBalanceModel[]>(`LeaveBalance?${q.toString()}`);
}
