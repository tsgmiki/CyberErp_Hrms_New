import type AbstractModel from "../AbstractModel";

// Performance Management (HC118–HC147) — Phase A configuration models.

export interface RatingScaleLevelModel {
  id?: string;
  value?: number;
  label?: string;
  description?: string;
  minScore?: number | null;
  maxScore?: number | null;
  sortOrder?: number;
}

/** Rating scale / scoring framework (HC138) with its level bands. */
export default interface RatingScaleModel extends AbstractModel {
  name?: string;
  description?: string;
  /** "Numeric" | "Percentage" */
  scoreType?: string;
  isActive?: boolean;
  sortOrder?: number;
  levels?: RatingScaleLevelModel[];
}

/** Configurable competency category (HC125). */
export interface CompetencyCategoryModel extends AbstractModel {
  name?: string;
  description?: string;
  sortOrder?: number;
  isActive?: boolean;
}

/** Competency library entry (HC123–HC125). */
export interface CompetencyModel extends AbstractModel {
  name?: string;
  competencyCategoryId?: string;
  competencyCategoryName?: string;
  description?: string;
  isActive?: boolean;
}

/** Configurable appraisal cycle (HC126–HC128). */
export interface ReviewCycleModel extends AbstractModel {
  name?: string;
  /** Annual | BiAnnual | Quarterly | Probation | Custom */
  periodType?: string;
  fiscalYearId?: string;
  fiscalYearName?: string;
  ratingScaleId?: string;
  ratingScaleName?: string;
  startDate?: string;
  endDate?: string;
  selfReviewDue?: string;
  managerReviewDue?: string;
  enableSelfAssessment?: boolean;
  enablePeerAssessment?: boolean;
  enableCalibration?: boolean;
  enableSecondLevelReview?: boolean;
  enableHrSignOff?: boolean;
  /** Probation length in months (probation cycles only). */
  probationDurationMonths?: number;
  /** Draft | Active | Closed */
  status?: string;
}

/** Appraisal form template — goals/competencies weight split (HC138). */
export interface AppraisalTemplateModel extends AbstractModel {
  name?: string;
  description?: string;
  goalsWeight?: number;
  competenciesWeight?: number;
  isActive?: boolean;
}

/** One competency assigned to a position with a weight (HC123/HC124). */
export interface PositionCompetencyModel {
  id?: string;
  competencyId?: string;
  competencyName?: string;
  competencyCategoryName?: string;
  weight?: number;
}

/** Organizational objective — cascade org→directorate→team (HC118/HC120/HC122). */
export interface OrganizationalObjectiveModel extends AbstractModel {
  title?: string;
  description?: string;
  reviewCycleId?: string;
  reviewCycleName?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  parentObjectiveId?: string;
  parentObjectiveTitle?: string;
  weight?: number;
  /** Draft | Active | Closed */
  status?: string;
}

export interface GoalActionItemModel {
  id?: string;
  description?: string;
  dueDate?: string;
  isCompleted?: boolean;
  sortOrder?: number;
}

/** Individual employee goal — SMART + action plan (HC119–HC122). */
export interface EmployeeGoalModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  reviewCycleId?: string;
  reviewCycleName?: string;
  organizationalObjectiveId?: string;
  objectiveTitle?: string;
  title?: string;
  description?: string;
  measure?: string;
  targetValue?: number | null;
  startDate?: string;
  dueDate?: string;
  weight?: number;
  progressPercent?: number;
  /** Draft | Active | Completed | Cancelled */
  status?: string;
  setByManager?: boolean;
  actionItems?: GoalActionItemModel[];
}

/** One scored line (goal or competency) within an appraisal (HC127/HC138). */
export interface AppraisalLineModel {
  id?: string;
  title?: string;
  weight?: number;
  selfScore?: number | null;
  selfComments?: string;
  managerScore?: number | null;
  managerComments?: string;
  sortOrder?: number;
  /** "Goal" | "Competency" */
  lineType?: string;
  referenceId?: string;
}

/** A scored appraisal for one employee in one review cycle (HC127/HC138). */
export interface AppraisalModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  reviewCycleId?: string;
  reviewCycleName?: string;
  appraisalTemplateId?: string;
  /** Period this appraisal covers (per-employee for probation cycles). */
  periodStart?: string;
  periodEnd?: string;
  goalsWeight?: number;
  competenciesWeight?: number;
  /** SelfAssessment | ManagerReview | Completed */
  stage?: string;
  selfComments?: string;
  managerComments?: string;
  overallScore?: number | null;
  finalRatingLevelId?: string;
  finalRatingLabel?: string;
  selfSubmittedAt?: string;
  completedAt?: string;
  isCalibrated?: boolean;
  /** Pending | Accepted | Appealed */
  acknowledgmentStatus?: string;
  employeeSignature?: string;
  employeeSignedAt?: string;
  managerSignature?: string;
  managerSignedAt?: string;
  reviewerComments?: string;
  reviewerSignature?: string;
  reviewerSignedAt?: string;
  hrSignature?: string;
  hrSignedAt?: string;
  /** Workflow-routing fields (single-record read): whose turn it is + whether the caller may act now. */
  currentUserRole?: string;
  canActCurrentStage?: boolean;
  currentStageActorName?: string;
  goals?: AppraisalLineModel[];
  competencies?: AppraisalLineModel[];
  peerReviews?: AppraisalPeerReviewModel[];
  peerAverageScore?: number | null;
}

/** An appraisal appeal reviewed by HR / management (HC143-144). */
export interface AppraisalAppealModel extends AbstractModel {
  appraisalId?: string;
  employeeId?: string;
  employeeName?: string;
  comments?: string;
  requestFollowUp?: boolean;
  /** Open | UnderReview | Resolved | Rejected */
  status?: string;
  resolution?: string;
  resolvedAt?: string;
}

/** A peer's assessment of an appraisal (HC127). */
export interface AppraisalPeerReviewModel {
  id?: string;
  appraisalId?: string;
  peerEmployeeId?: string;
  peerEmployeeName?: string;
  /** Invited | Submitted | Declined */
  status?: string;
  score?: number | null;
  comments?: string;
  submittedAt?: string;
}

/** One appraisal under calibration — original vs. calibrated score (HC128/HC129). */
export interface CalibrationItemModel {
  id?: string;
  appraisalId?: string;
  employeeId?: string;
  employeeName?: string;
  originalScore?: number | null;
  calibratedScore?: number | null;
  justification?: string;
  isAdjusted?: boolean;
}

/** A calibration & moderation session (HC128/HC129). */
export interface CalibrationSessionModel extends AbstractModel {
  name?: string;
  reviewCycleId?: string;
  reviewCycleName?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  /** Draft | Finalized */
  status?: string;
  notes?: string;
  finalizedAt?: string;
  items?: CalibrationItemModel[];
}

/** One append-only audit/version-history entry (HC132). */
export interface PerformanceHistoryModel {
  id?: string;
  entityType?: string;
  entityId?: string;
  action?: string;
  summary?: string;
  snapshotJson?: string;
  createdBy?: string;
  createdAt?: string;
}

/** One development action within an IDP (HC130/131). */
export interface DevelopmentActionModel {
  id?: string;
  description?: string;
  competencyId?: string;
  competencyName?: string;
  learningIntervention?: string;
  targetDate?: string;
  /** Planned | InProgress | Completed */
  status?: string;
  progressPercent?: number;
  sortOrder?: number;
}

/** An Individual Development Plan (HC130/131). */
export interface DevelopmentPlanModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  appraisalId?: string;
  reviewCycleId?: string;
  title?: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  /** Draft | Active | Completed | Cancelled */
  status?: string;
  actions?: DevelopmentActionModel[];
}

/** One objective within a PIP (HC135). */
export interface PipObjectiveModel {
  id?: string;
  description?: string;
  targetDate?: string;
  /** NotStarted | InProgress | Met | NotMet */
  status?: string;
  progressPercent?: number;
  sortOrder?: number;
}

/** One row of the rating distribution (HC134). */
export interface RatingDistributionRowModel {
  levelId?: string;
  label?: string;
  count?: number;
}

/** Manager / HR performance dashboard aggregates (HC134). */
export interface PerformanceDashboardModel {
  totalAppraisals?: number;
  selfAssessmentCount?: number;
  managerReviewCount?: number;
  completedCount?: number;
  overdueReviews?: number;
  pendingAcknowledgment?: number;
  ratingDistribution?: RatingDistributionRowModel[];
  totalGoals?: number;
  completedGoals?: number;
  averageGoalProgress?: number;
  activePips?: number;
  openAppeals?: number;
}

/** Unified per-employee performance summary (HC147). */
export interface EmployeePerformanceSummaryModel {
  employeeId?: string;
  employeeName?: string;
  latestAppraisal?: {
    id?: string;
    reviewCycleName?: string;
    stage?: string;
    overallScore?: number | null;
    finalRatingLabel?: string;
    acknowledgmentStatus?: string;
  } | null;
  totalGoals?: number;
  activeGoals?: number;
  completedGoals?: number;
  averageGoalProgress?: number;
  activePip?: { id?: string; title?: string; status?: string; endDate?: string } | null;
  openAppeals?: number;
  achievementsCount?: number;
  recognitionsCount?: number;
}

/** An employee achievement / milestone (HC139-140). */
export interface AchievementModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  title?: string;
  description?: string;
  achievementDate?: string;
  /** Milestone | Award | Project | Certification | Other */
  category?: string;
  appraisalId?: string;
}

/** A configurable recognition badge / award (HC141). */
export interface RecognitionBadgeModel extends AbstractModel {
  name?: string;
  description?: string;
  color?: string;
  icon?: string;
  isActive?: boolean;
  sortOrder?: number;
}

/** A recognition granted to an employee (HC141). */
export interface EmployeeRecognitionModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  recognitionBadgeId?: string;
  badgeName?: string;
  badgeColor?: string;
  badgeIcon?: string;
  citation?: string;
  recognizedOn?: string;
  isPublic?: boolean;
}

/** A Performance Improvement Plan (HC135). */
export interface ImprovementPlanModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  appraisalId?: string;
  title?: string;
  reason?: string;
  startDate?: string;
  endDate?: string;
  /** Draft | Active | UnderReview | Completed */
  status?: string;
  /** Pending | Successful | Unsuccessful | Extended */
  outcome?: string;
  outcomeNotes?: string;
  outcomeRecordedAt?: string;
  objectives?: PipObjectiveModel[];
}
