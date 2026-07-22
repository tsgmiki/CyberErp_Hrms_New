import type AbstractModel from "../AbstractModel";

/** §3.10.1 Compensation & Benefit (HC226–234). */

/** Allowance/earning catalogue entry (HC226). */
export interface AllowanceTypeModel extends AbstractModel {
  name?: string;
  code?: string;
  calcMethod?: string; // Fixed | PercentOfBase
  defaultRate?: number;
  isTaxable?: boolean;
  isActive?: boolean;
  sortOrder?: number;
}

/** Per-employee allowance assignment (HC226). */
export interface EmployeeAllowanceModel extends AbstractModel {
  employeeId?: string;
  allowanceTypeId?: string;
  allowanceTypeName?: string;
  calcMethod?: string;
  isTaxable?: boolean;
  value?: number;
  resolvedAmount?: number;
  effectiveFrom?: string;
  effectiveTo?: string;
  isCurrentlyActive?: boolean;
  remark?: string;
}

/** Resolved compensation snapshot (HC226/HC233). */
export interface CompensationSummaryModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  baseSalary?: number;
  jobGradeName?: string;
  stepName?: string;
  allowances?: EmployeeAllowanceModel[];
  totalAllowances?: number;
  taxableAllowances?: number;
  nonTaxableAllowances?: number;
  grossPay?: number;
  taxableGross?: number;
}

/** Salary revision line (HC228). */
export interface SalaryRevisionLineModel {
  id?: string;
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  currentSalary?: number;
  proposedSalary?: number;
  increase?: number;
  increasePercent?: number;
}

/** Salary revision plan (HC228). */
export interface SalaryRevisionModel extends AbstractModel {
  name?: string;
  revisionType?: string; // Merit | Market | CostOfLiving
  basis?: string; // Percentage | FixedAmount
  rate?: number;
  effectiveDate?: string;
  targetJobGradeId?: string;
  targetOrganizationUnitId?: string;
  status?: string; // Draft | PendingApproval | Approved | Applied | Cancelled
  appliedOn?: string;
  notes?: string;
  employeeCount?: number;
  totalCurrent?: number;
  totalProposed?: number;
  totalIncrease?: number;
  averagePercent?: number;
  lines?: SalaryRevisionLineModel[];
}

/** Stateless simulation result (HC228). */
export interface SalarySimulationModel {
  employeeCount?: number;
  totalCurrent?: number;
  totalProposed?: number;
  totalIncrease?: number;
  averagePercent?: number;
  lines?: SalaryRevisionLineModel[];
  linesTruncated?: boolean;
}

/** Benefit plan (HC230). */
export interface BenefitPlanModel extends AbstractModel {
  name?: string;
  category?: string; // Health | Life | Disability | Pension | Other
  description?: string;
  employeeContributionMethod?: string;
  employeeContributionRate?: number;
  employerContributionMethod?: string;
  employerContributionRate?: number;
  enrollmentOpenFrom?: string;
  enrollmentOpenTo?: string;
  isActive?: boolean;
  isEnrollmentOpen?: boolean;
}

/** Employee benefit enrollment (HC230). */
export interface BenefitEnrollmentModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  benefitPlanId?: string;
  benefitPlanName?: string;
  category?: string;
  status?: string; // Enrolled | Waived | Terminated
  enrolledOn?: string;
  coverageStart?: string;
  coverageEnd?: string;
  electedEmployeeContribution?: number;
  employeeContribution?: number;
  employerContribution?: number;
  remark?: string;
}

/** Progressive tax bracket (HC231). */
export interface TaxBracketModel extends AbstractModel {
  lowerBound?: number;
  upperBound?: number | null;
  ratePercent?: number;
  sortOrder?: number;
}

export interface DeductionLineModel {
  label?: string;
  kind?: string; // Tax | BenefitContribution
  amount?: number;
}

/** Automated deductions preview (HC232). */
export interface PayrollDeductionsModel {
  employeeId?: string;
  employeeName?: string;
  baseSalary?: number;
  grossPay?: number;
  taxableGross?: number;
  incomeTax?: number;
  employeeBenefitContributions?: number;
  employerBenefitContributions?: number;
  totalDeductions?: number;
  netPay?: number;
  lines?: DeductionLineModel[];
}

/** Consolidated self-service compensation (HC233). */
export interface MyCompensationModel {
  employeeId?: string;
  summary?: CompensationSummaryModel;
  benefits?: BenefitEnrollmentModel[];
  deductions?: PayrollDeductionsModel;
}

/** Employee self-service compensation request (HC234). */
export interface CompensationRequestModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  requestType?: string; // BenefitChange | PayrollDiscrepancy
  subject?: string;
  details?: string;
  benefitPlanId?: string;
  benefitPlanName?: string;
  referencePeriod?: string;
  disputedAmount?: number;
  status?: string; // Submitted | UnderReview | Resolved | Rejected
  resolution?: string;
  submittedOn?: string;
  resolvedOn?: string;
}
