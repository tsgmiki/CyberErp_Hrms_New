import type AbstractModel from "../AbstractModel";

// ===== Other (non-annual) Leave — static, position-based, gender-aware entitlements =====

/** Per-fiscal-year policy row (hrmsOtherLeaveSetting). Static — never accrues. */
export interface OtherLeaveSettingModel extends AbstractModel {
  fiscalYearId?: string;
  fiscalYearName?: string;
  /** The LeaveType master relationship (replaces the former free-text name). */
  leaveTypeId?: string;
  /** Display name — projected from the related LeaveType. */
  name?: string;
  /** All | Female | Male */
  gender?: string;
  standardDays?: number;
  managerialDays?: number;
  /** Maternity/paternity/mourning style: the whole entitlement in ONE block. */
  isLumpSum?: boolean;
  /** WorkingDays (skip holidays/weekends) | CalendarDays (count them). */
  dayCounting?: string;
  isActive?: boolean;
  description?: string;
}

/** Supporting document attached to a request (metadata only; bytes are fetched on demand). */
export interface OtherLeaveAttachmentModel {
  id?: string;
  fileName?: string;
  contentType?: string;
  fileSize?: number;
  uploadedAt?: string;
  uploadedBy?: string;
}

export interface OtherLeaveDetailModel {
  id?: string;
  startDate?: string;
  endDate?: string;
  leaveDays?: number;
}

/** Request header (hrmsOtherLeave). Status: Pending | Approved | Rejected | Cancelled. */
export interface OtherLeaveModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  otherLeaveSettingId?: string;
  leaveName?: string;
  fiscalYearName?: string;
  isLumpSum?: boolean;
  requestDate?: string;
  remark?: string;
  totalLeaveDays?: number;
  status?: string;
  details?: OtherLeaveDetailModel[];
  attachments?: OtherLeaveAttachmentModel[];
}

/** One selectable entitlement on the request form (active fiscal year, gender-filtered). */
export interface OtherLeaveBalanceModel {
  otherLeaveSettingId: string;
  name: string;
  fiscalYearName?: string;
  gender: string;
  isLumpSum: boolean;
  /** WorkingDays | CalendarDays — how request blocks are costed. */
  dayCounting: string;
  allocation: number;
  reserved: number;
  remaining: number;
}

/** Server-computed end date of a lump-sum block. */
export interface LumpSumEndModel {
  startDate: string;
  endDate: string;
  leaveDays: number;
}
