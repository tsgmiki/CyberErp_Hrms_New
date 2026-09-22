import { api } from "@/utils/apiClient";

/** One employee whose approved leave has ended without a recorded return. */
export interface OverdueLeaveReturn {
  requestId: string;
  /** "AnnualLeave" | "OtherLeave" — which module the row came from. */
  source: string;
  employeeId: string;
  fullName: string;
  employeeNumber: string;
  positionTitle?: string;
  /** "Annual Leave", or the other-leave type's name. */
  leaveName: string;
  /** The last approved day — they were due back after this. */
  plannedEndDate: string;
  daysOverdue: number;
  totalLeaveDays: number;
  /**
   * Whether confirming a return can CLEAR this row. True for annual leave only — Other Leave has no
   * return step, so those rows report "this leave has ended" and cannot be acknowledged away
   * (HRMS logic §12.106).
   */
  hasReturnConfirmation: boolean;
}

export default async function getOverdueLeaveReturns() {
  return api.get<OverdueLeaveReturn[]>("Dashboard/overdue-leave-returns");
}
