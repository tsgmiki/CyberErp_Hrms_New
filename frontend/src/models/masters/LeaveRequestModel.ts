import type AbstractModel from "../AbstractModel";

/** One detail line of a leave request: a date range (or single day) of a given leave type. */
export interface LeaveRequestLineModel {
  id?: string;
  leaveTypeId?: string;
  leaveTypeCode?: string;
  leaveTypeName?: string;
  startDate?: string;
  endDate?: string;
  dayPart?: string; // Full | FirstHalf | SecondHalf
  workingDays?: number;
}

/**
 * Leave request header (HC034–HC039). A single request can carry multiple date ranges and
 * multiple leave types via its `lines`; the working-day engine aggregates them into
 * `totalWorkingDays`, honouring the active work-week (weekend) configuration.
 */
export default interface LeaveRequestModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  fiscalYearId?: string;
  submittedDate?: string;
  totalWorkingDays?: number;
  reason?: string;
  status?: string;
  decisionComment?: string;
  cancelReason?: string;
  lines?: LeaveRequestLineModel[];
}
