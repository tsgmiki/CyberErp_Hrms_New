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
  /** Step basis only — rung today, rung landed on (may be fractional, e.g. 5.5). */
  currentStep?: number | null;
  proposedStep?: number | null;
  /** True when the salary was interpolated between two rungs rather than read off one. */
  interpolated?: boolean;
  /** Why the salary did not move; null when it did. */
  note?: string | null;
  /** Performance type only: the score that selected the band, and the band itself. */
  performanceScore?: number | null;
  bandLabel?: string | null;
  bandValue?: number | null;
  /** The employee's hire date (Hrms.Employee.HireDate) — the input behind `monthsOfService`. */
  hireDate?: string | null;
  /** Completed months of service at the effective date. */
  monthsOfService?: number | null;
  /** Share of the increment earned: 1 = full, <1 = prorated first year, 0 = excluded. */
  prorationFactor?: number;
  /** True when an eligibility rule removed this employee; `note` says which. */
  isExcluded?: boolean;
  /** Grade code the employee moves up into when a step increment clears their ceiling. */
  promotedToGradeCode?: string | null;
  /** True when this line changes the employee's GRADE, not only their pay. */
  promoted?: boolean;
}

/** Salary revision plan (HC228). */
export interface SalaryRevisionModel extends AbstractModel {
  name?: string;
  revisionType?: string; // Merit | Market | CostOfLiving | Performance
  basis?: string; // Percentage | FixedAmount | Step
  rate?: number;
  effectiveDate?: string;
  targetJobGradeId?: string;
  targetOrganizationUnitId?: string;
  /** Performance type: pin the review cycle, or omit for each employee's latest completed appraisal. */
  targetReviewCycleId?: string;
  /** Performance type: the score bands. Ignored by the flat-rate types. */
  bands?: SalaryRevisionBandModel[];
  status?: string; // Draft | PendingApproval | Approved | Applied | Cancelled
  /** A workflow owns the approval, so the direct Approve action is not available. */
  awaitingWorkflow?: boolean;
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
  /** Step basis only: employees the scale could not move (off-scale, no rows, already at ceiling). */
  unresolvedCount?: number;
  /** Step basis only: employees whose salary was interpolated between two rungs. */
  interpolatedCount?: number;
  /** Performance type only: targeted employees with no completed appraisal, so no award. */
  noScoreCount?: number;
  /** Performance type only: the score range actually seen — reveals bands set for the wrong scale. */
  minObservedScore?: number | null;
  maxObservedScore?: number | null;
  /** Employees removed by the tenure gate or an active disciplinary case. */
  excludedCount?: number;
  /** Employees on a reduced increment because they are inside their first year. */
  proratedCount?: number;
  /** The minimum-service gate in force, so the UI can explain the exclusions. */
  minimumServiceMonths?: number;
  /** Employees moved onto the next grade because a step increment cleared their ceiling. */
  promotedCount?: number;
}

/**
 * Increment eligibility rules — one active policy per tenant, so this is a singleton screen rather
 * than a list. Every field is a rule the salary revision applies when it builds its lines.
 */
export interface SalaryIncrementPolicyModel extends AbstractModel {
  name?: string;
  /** Completed months of service required to qualify. 0 = no tenure gate. */
  minimumServiceMonths?: number;
  /** Scale the increment by months worked / 12 for anyone inside their first year. */
  prorateFirstYear?: boolean;
  /** Exclude anyone with an unexpired disciplinary case. */
  excludeActiveDisciplinary?: boolean;
  /** Move an employee onto the next grade up when a step increment clears their ceiling. */
  promoteOnGradeCeiling?: boolean;
  isActive?: boolean;
}

/** One score band of a performance-based revision. */
export interface SalaryRevisionBandModel {
  /** Inclusive lower bound: this band applies when score >= minScore. */
  minScore?: number;
  /** Award in the revision's basis units (steps / percent / amount). */
  value?: number;
  label?: string;
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
