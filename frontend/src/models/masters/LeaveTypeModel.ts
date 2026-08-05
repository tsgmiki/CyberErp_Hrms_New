import type AbstractModel from "../AbstractModel";

export default interface LeaveTypeModel extends AbstractModel {
  code?: string;
  name?: string;
  nameA?: string;
  isPaid?: boolean;
  requiresApproval?: boolean;
  allowHalfDay?: boolean;
  genderEligibility?: string;
  accrualMethod?: string;
  description?: string;
  isActive?: boolean;
}
