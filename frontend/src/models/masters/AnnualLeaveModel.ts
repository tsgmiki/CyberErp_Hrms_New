import type AbstractModel from "../AbstractModel";

/** One detail row of an Annual-Leave request: a date range (or single half-day). */
export interface AnnualLeaveDetailModel {
  id?: string;
  leaveUsage?: string; // FullDay | HalfDay
  halfDayPart?: string; // Morning | Afternoon — only for a HalfDay row
  startDate?: string;
  endDate?: string;
  leaveDays?: number;
}

/**
 * Annual-Leave request header (Master-Detail, dedicated to annual leave). It carries no LeaveType:
 * the ledger row it references (annualLeaveLedgerId → LeaveBalance) already fixes the employee,
 * fiscal year and annual leave type. Detail rows hold the date ranges; the engine aggregates
 * `totalLeaveDays`, and approval debits the referenced ledger.
 */
export default interface AnnualLeaveModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  annualLeaveLedgerId?: string;
  fiscalYearName?: string;
  ledgerAvailable?: number;
  requestDate?: string;
  remark?: string;
  totalLeaveDays?: number;
  status?: string;
  details?: AnnualLeaveDetailModel[];
}
