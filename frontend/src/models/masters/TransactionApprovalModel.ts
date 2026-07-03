import type AbstractModel from "../AbstractModel";

export default interface TransactionApprovalModel extends AbstractModel {
  companyId?: string;
  voucherType?: string;
  voucherNo?:string
  voucherId?: string;
  date?: string;
  isApproved?: string;
  step?: number;
  totalStep?: number;
  status?: string;
  userId?: string;
  criteria?: string;
  itemGroup?: string;
  storeId?: string;
  unitId?: string;
  unit?: string;
  store?: string;
  user?: string;
  approver?:string
}
