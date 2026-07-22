import type { LeaveBalanceModel } from "@/models";
import { api } from "@/utils/apiClient";

export default async function getLeaveBalances(employeeId: string, fiscalYearId?: string) {
  if (!employeeId) return [] as LeaveBalanceModel[];
  const q = new URLSearchParams({ employeeId });
  if (fiscalYearId) q.append("fiscalYearId", fiscalYearId);
  return api.get<LeaveBalanceModel[]>(`LeaveBalance?${q.toString()}`);
}
