import type AbstractModel from "../AbstractModel";

export default interface NotificationModel extends AbstractModel {
  voucherType?: string;
  voucherId?: string;
  voucherNumber?: string;
  date?: string;
  isViewd?: boolean;
  message?: string;
  statusId?: string;
  status?: string;
  approverId?: string;
  approver?: string;
  criteria?: string;
  remark?: string;
  isResponded?: boolean;
}
