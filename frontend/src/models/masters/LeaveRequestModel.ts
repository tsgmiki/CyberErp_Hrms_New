import type AbstractModel from "../AbstractModel";

export default interface LeaveRequestModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  leaveTypeId?: string;
  leaveTypeCode?: string;
  leaveTypeName?: string;
  startDate?: string;
  endDate?: string;
  dayPart?: string;
  workingDays?: number;
  reason?: string;
  status?: string;
  decisionComment?: string;
  cancelReason?: string;
}
