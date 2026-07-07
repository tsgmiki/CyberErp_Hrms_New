export default interface ParameterModel {
  skip: number;
  take: number;
  sortCol: string;
  dir: string;
  searchText: string;
  reportName: string;
  fromDate: string;
  toDate: string;
  customerId: string;
  bankId: string;
  itemId: string;
  salesPersonId: string;
  category: string;
  salesType: string;
  purchaseType: string;
  supplierId: string;
  itemCategoryId: string;
  voucherType: string;
  remainingOnly: boolean;
  isRoot: boolean;
  storeId: string;
  expenseTypeId: string;
  groupId?: string;
  categoryId?: string;
  itemType: string;
  status: string;
  employeeId: string;
  unitId: string;
  pettyCashId: string;
  dateRangeType: string;
  getByFinalStatus: boolean;
  operationTypeId?: string;
  advanceRequestId?: string;
  operationIds?: string;
  operationId?: string;
  reportCategory?: string;
  userId?: string;
  parentId?: string;
  /** Restrict positions to vacant (open) ones — used by the employee placement dropdown. */
  isVacant?: boolean;
  /** Filters the salary-scale grid to a single job grade. */
  jobGradeId?: string;

}
