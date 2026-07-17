import type AbstractModel from "../AbstractModel";
import type { EmployeePerformanceSummaryModel } from "./PerformanceModel";

// ===== Career Development §3.7.A — Succession Planning (HC148–HC160) =====

export interface CriticalPositionModel extends AbstractModel {
  positionId?: string;
  positionCode?: string;
  positionTitle?: string;
  organizationUnitName?: string;
  riskLevel?: string;
  reason?: string;
  criteria?: string;
  isActive?: boolean;
}

export interface TalentReviewModel extends AbstractModel {
  name?: string;
  cycle?: string;
  organizationUnitId?: string;
  status?: string;
  notes?: string;
}

export interface NineBoxCellModel {
  performanceBand: number;
  potentialBand: number;
  count: number;
}
export interface NineBoxModel {
  talentReviewId: string;
  total: number;
  hiPoCount: number;
  cells: NineBoxCellModel[];
}

export interface TalentRatingModel {
  id?: string;
  raterEmployeeId?: string;
  raterRole?: string;
  performanceScore?: number;
  potentialScore?: number;
  comment?: string;
}
export interface TalentAssessmentModel extends AbstractModel {
  talentReviewId?: string;
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  performanceBand?: number;
  potentialBand?: number;
  isHiPo?: boolean;
  readiness?: string;
  notes?: string;
  ratings?: TalentRatingModel[];
}

export interface SuccessionPlanModel extends AbstractModel {
  criticalPositionId?: string;
  roleTitle?: string;
  name?: string;
  horizon?: string;
  status?: string;
  notes?: string;
}

export interface SuccessionChartNodeModel {
  candidateId: string;
  rank: number;
  employeeId: string;
  employeeName?: string;
  readiness: string;
  readinessScore?: number;
}
export interface SuccessionChartModel {
  successionPlanId: string;
  name: string;
  roleTitle?: string;
  successors: SuccessionChartNodeModel[];
}

export interface SuccessionDevelopmentActionModel {
  id?: string;
  type?: string;
  description?: string;
  dueDate?: string;
  status?: string;
  mentorEmployeeId?: string;
}
export interface KnowledgeTransferModel {
  id?: string;
  topic?: string;
  fromEmployeeId?: string;
  status?: string;
  targetDate?: string;
  completedDate?: string;
}
export interface SuccessionCandidateModel extends AbstractModel {
  successionPlanId?: string;
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  rank?: number;
  readiness?: string;
  readinessScore?: number;
  gapSummary?: string;
  notes?: string;
  developmentActions?: SuccessionDevelopmentActionModel[];
  knowledgeTransfers?: KnowledgeTransferModel[];
}

export interface CompetencyGapItemModel {
  competencyId: string;
  name: string;
  weight: number;
}
export interface CompetencyGapModel {
  successionCandidateId: string;
  requiredCount: number;
  metCount: number;
  gaps: CompetencyGapItemModel[];
}

// ---- Step 4: integration & business logic (HC148, HC153, HC158) ----
export interface ReadinessComputationModel {
  successionCandidateId: string;
  readiness: string;
  readinessScore?: number;
  performanceScore?: number;
  competencyScore?: number;
  competencyMet: number;
  competencyRequired: number;
  hasAppraisal: boolean;
}
export interface SuccessionCandidateProfileModel {
  successionCandidateId: string;
  employeeId: string;
  employeeName?: string;
  readiness: string;
  readinessScore?: number;
  performance?: EmployeePerformanceSummaryModel;
  gap?: CompetencyGapModel;
}
export interface IdentifyHiPosResultModel {
  flagged: number;
  totalHiPo: number;
}

// ===== Career Development §3.7.B — Career Path (HC161–HC169) =====

export interface CareerPathModel extends AbstractModel {
  name?: string;
  code?: string;
  description?: string;
  isActive?: boolean;
}

export interface CareerPathStepCompetencyModel {
  id?: string;
  competencyId?: string;
  competencyName?: string;
  weight?: number;
}
export interface CareerPathStepModel extends AbstractModel {
  careerPathId?: string;
  stepOrder?: number;
  name?: string;
  positionClassId?: string;
  positionClassName?: string;
  jobGradeId?: string;
  requiredExperienceMonths?: number;
  certifications?: string;
  description?: string;
  competencies?: CareerPathStepCompetencyModel[];
}

export interface EmployeeCareerStepProgressModel {
  id?: string;
  careerPathStepId?: string;
  status?: string;
  completedDate?: string;
  notes?: string;
}
export interface EmployeeCareerPathModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  careerPathId?: string;
  careerPathName?: string;
  currentStepId?: string;
  assignedBy?: string;
  assignedDate?: string;
  progressPercent?: number;
  status?: string;
  notes?: string;
  stepProgress?: EmployeeCareerStepProgressModel[];
}

export interface MentorshipModel extends AbstractModel {
  mentorEmployeeId?: string;
  mentorName?: string;
  menteeEmployeeId?: string;
  menteeName?: string;
  context?: string;
  refId?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  notes?: string;
}

export interface CareerPathChangeRequestModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  currentCareerPathId?: string;
  requestedCareerPathId?: string;
  reason?: string;
  status?: string;
  decisionNotes?: string;
  decidedAt?: string;
}

export interface VisualizeCompetencyModel {
  competencyId: string;
  name: string;
  weight: number;
  isMet: boolean;
}
export interface CareerPathVisualizeStepModel {
  stepId: string;
  stepOrder: number;
  name: string;
  positionClassId?: string;
  positionClassName?: string;
  requiredExperienceMonths?: number;
  certifications?: string;
  progressStatus?: string;
  isCurrentStep: boolean;
  requiredCount: number;
  metCount: number;
  competencies: VisualizeCompetencyModel[];
}
export interface CareerPathVisualizeModel {
  careerPathId: string;
  careerPathName: string;
  employeeId?: string;
  employeeName?: string;
  progressPercent?: number;
  steps: CareerPathVisualizeStepModel[];
}

export interface CareerPathUtilizationModel {
  careerPathId: string;
  careerPathName: string;
  code: string;
  assignedCount: number;
  activeCount: number;
  completedCount: number;
  onHoldCount: number;
  avgProgress: number;
}

// ---- Step 4: integration & business logic (HC163/HC164/HC167) ----
export interface CareerPathSuggestionModel {
  careerPathId: string;
  careerPathName: string;
  code: string;
  requiredCount: number;
  metCount: number;
  matchPercent: number;
  performanceScore?: number;
  fitScore: number;
  alreadyAssigned: boolean;
}
export interface DevelopmentRecommendationItemModel {
  competencyId: string;
  name: string;
  weight: number;
  suggestedAction: string;
}
export interface DevelopmentRecommendationModel {
  employeeCareerPathId: string;
  employeeId: string;
  targetStepId?: string;
  targetStepName?: string;
  gapCount: number;
  recommendations: DevelopmentRecommendationItemModel[];
}
export interface CreateDevelopmentGoalsResultModel {
  reviewCycleId: string;
  created: number;
  skipped: number;
  organizationalObjectiveId?: string;
  organizationalObjectiveTitle?: string;
}

export interface CreateDevelopmentPlanResultModel {
  developmentPlanId: string;
  actionCount: number;
}

// ---- Employee 360 development profile (Performance ↔ Career Development bridge, HC158) ----
export interface DevEmployeeCareerPathModel {
  id: string;
  careerPathId: string;
  careerPathName?: string;
  status: string;
  progressPercent: number;
}
export interface DevSuccessionCandidacyModel {
  id: string;
  successionPlanId: string;
  planName?: string;
  roleTitle?: string;
  rank: number;
  readiness: string;
  readinessScore?: number;
}
export interface DevMentorshipModel {
  id: string;
  role: string;
  counterpartName?: string;
  context: string;
  status: string;
}
export interface EmployeeDevelopmentProfileModel {
  employeeId: string;
  employeeName?: string;
  performance?: EmployeePerformanceSummaryModel;
  careerPaths: DevEmployeeCareerPathModel[];
  successionCandidacies: DevSuccessionCandidacyModel[];
  mentorships: DevMentorshipModel[];
  nextStepGap?: DevelopmentRecommendationModel;
}
