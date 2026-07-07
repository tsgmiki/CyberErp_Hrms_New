import type AbstractModel from "../AbstractModel";

export default interface LeaveBalanceModel extends AbstractModel {
  employeeId?: string;
  leaveTypeId?: string;
  leaveTypeCode?: string;
  leaveTypeName?: string;
  fiscalYearId?: string;
  fiscalYearName?: string;
  entitled?: number;
  carriedForward?: number;
  adjusted?: number;
  taken?: number;
  available?: number;
}
