import type AbstractModel from "../AbstractModel";

/** One row of a workforce plan: a role at a unit for one employment type and horizon period. */
export interface WorkforcePlanLineModel {
  id?: string;
  organizationUnitId: string;
  organizationUnitName?: string;
  positionClassId: string;
  positionClassTitle?: string;
  jobGradeName?: string;
  jobCategoryName?: string;
  employmentType: string; // Permanent | Contract | Intern | Consultant
  periodIndex: number;
  authorizedHeadcount: number;
  filledCount: number;
  vacantCount: number;
  newHires: number;
  replacements: number;
  temporaryStaff: number;
  mobilityIn: number;
  promotions: number;
  actingAssignments: number;
  retirements: number;
  resignations: number;
  contractExpiries: number;
  isCriticalRole: boolean;
  requiredCompetencies?: string;
  annualSalaryCost: number;
  annualAllowances: number;
  annualBenefits: number;
  // Computed server-side (also derivable client-side while editing)
  endHeadcount?: number;
  headcountGap?: number;
  lineCost?: number;
  remark?: string;
}

/** Versioned, scenario-tagged workforce plan (HC053–HC076). */
export default interface WorkforcePlanModel extends AbstractModel {
  name?: string;
  description?: string;
  horizon?: string; // Annual | MediumTerm | MultiYear
  scenario?: string; // Baseline | Growth | Contraction | Restructuring
  status?: string; // Draft | Submitted | Approved | Rejected | Archived
  organizationUnitId?: string;
  organizationUnitName?: string;
  startFiscalYearId?: string;
  startFiscalYearName?: string;
  periodCount?: number;
  version?: number;
  rootPlanId?: string;
  totalBudget?: number;
  budgetThresholdPercent?: number;
  escalationJustification?: string;
  projectedCost?: number;
  budgetVariance?: number;
  excessBeyondThreshold?: number;
  submittedAt?: string;
  approvedAt?: string;
  awaitingWorkflow?: boolean;
  lines?: WorkforcePlanLineModel[];
}

/** Live establishment row: authorized / filled / vacant per unit × role (HC056). */
export interface EstablishmentRowModel {
  organizationUnitId: string;
  organizationUnitName?: string;
  positionClassId: string;
  positionClassTitle?: string;
  jobGradeName?: string;
  jobCategoryName?: string;
  authorized: number;
  filled: number;
  vacant: number;
  avgVacantDays?: number;
}

export interface SeparationSuggestionModel {
  organizationUnitId: string;
  positionClassId: string;
  retirements: number;
}

export interface WorkforcePlanPeriodSummaryModel {
  periodIndex: number;
  endHeadcount: number;
  demand: number;
  supply: number;
  separations: number;
  cost: number;
}

export interface WorkforcePlanSummaryModel {
  planId: string;
  totalBudget: number;
  projectedCost: number;
  budgetVariance: number;
  excessBeyondThreshold: number;
  totalEndHeadcount: number;
  totalGap: number;
  criticalRoles: number;
  periods: WorkforcePlanPeriodSummaryModel[];
}

/** One plan's figures in a scenario comparison (HC068). */
export interface WorkforcePlanComparisonModel {
  planId: string;
  name: string;
  scenario: string;
  status: string;
  version: number;
  totalEndHeadcount: number;
  totalDemand: number;
  totalSeparations: number;
  totalGap: number;
  criticalRoles: number;
  projectedCost: number;
  totalBudget: number;
  budgetVariance: number;
}

/** Outstanding approved hiring demand — the recruitment feed (HC075). */
export interface ApprovedDemandRowModel {
  planId: string;
  planName: string;
  organizationUnitId: string;
  organizationUnitName?: string;
  positionClassId: string;
  positionClassTitle?: string;
  employmentType: string;
  periodIndex: number;
  newHires: number;
  replacements: number;
  temporaryStaff: number;
  isCriticalRole: boolean;
  requiredCompetencies?: string;
}
