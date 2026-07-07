export interface AnnualLeaveLedgerRow {
  employeeId: string;
  employeeName?: string;
  employeeNumber?: string;
  hireDate?: string;
  serviceYears: number;
  isManagerial: boolean;
  calculatedEntitlement: number;
  isGenerated: boolean;
  entitled: number;
  carriedForward: number;
  adjusted: number;
  taken: number;
  available: number;
}

export default interface AnnualLeaveLedgerModel {
  settingId: string;
  fiscalYearId: string;
  fiscalYearName?: string;
  fiscalYearStart: string;
  fiscalYearEnd: string;
  leaveTypeId: string;
  leaveTypeName?: string;
  fiscalYearClosed: boolean;
  totalEmployees: number;
  generatedCount: number;
  rows: AnnualLeaveLedgerRow[];
}
