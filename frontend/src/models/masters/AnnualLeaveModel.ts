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
  /** Pending | Approved | Rejected | Cancelled | ReturnPending | Closed */
  status?: string;
  /** Days actually taken; null until the return is settled. */
  actualLeaveDays?: number | null;
  /** The employee may confirm their return — drives the action on the row. */
  canConfirmReturn?: boolean;
  /** Last approved day, so the return form can default to it. */
  plannedEndDate?: string | null;
  details?: AnnualLeaveDetailModel[];
}

/** What confirming a given return date would do, without committing to it. */
export interface AnnualLeaveReturnPreviewModel {
  plannedEndDate?: string;
  approvedDays?: number;
  actualDays?: number;
  /** Negative = returned early (credit due), positive = late (extra days), 0 = on time. */
  adjustmentDays?: number;
  returnType?: string; // OnTime | Early | Late
  commentRequired?: boolean;
  requiresApproval?: boolean;
  /** Balance available for a LATE return's extra days; null when not applicable. */
  availableForExtension?: number | null;
  warning?: string | null;
}

/** Outcome of confirming a return. */
export interface AnnualLeaveReturnResultModel {
  returnId?: string;
  returnType?: string;
  approvedDays?: number;
  actualDays?: number;
  adjustmentDays?: number;
  requiresApproval?: boolean;
  headerStatus?: string;
  message?: string;
}

/** One entry in a request's lifecycle, whatever produced it. */
export interface AnnualLeaveHistoryEntryModel {
  at?: string;
  /** Submitted | Workflow | Return | Settled */
  kind?: string;
  title?: string;
  detail?: string | null;
  actor?: string | null;
  action?: string | null;
  comment?: string | null;
  stepOrder?: number | null;
  stepName?: string | null;
}

/** The whole story of one request — what the approver's history popup renders. */
export interface AnnualLeaveHistoryModel {
  id?: string;
  employeeName?: string;
  employeeNumber?: string;
  requestDate?: string;
  status?: string;
  approvedDays?: number;
  actualDays?: number | null;
  plannedEndDate?: string | null;
  actualEndDate?: string | null;
  adjustmentDays?: number | null;
  returnType?: string | null;
  entries?: AnnualLeaveHistoryEntryModel[];
}
