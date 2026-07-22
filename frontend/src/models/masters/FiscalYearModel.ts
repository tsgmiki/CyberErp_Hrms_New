import type AbstractModel from "../AbstractModel";

export default interface FiscalYearModel extends AbstractModel {
  name?: string;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
  isClosed?: boolean;
}
