import type AbstractModel from "../AbstractModel";

/** Training & Development §3.8 (HC187–HC202). */

/** Groups catalog courses by kind (HC191). */
export interface TrainingCategoryModel extends AbstractModel {
  name?: string;
  description?: string;
  isActive?: boolean;
  sortOrder?: number;
}

/** A catalog course / program (HC191/HC196; external providers per HC194). */
export interface TrainingCourseModel extends AbstractModel {
  name?: string;
  code?: string;
  trainingCategoryId?: string;
  categoryName?: string;
  description?: string;
  objectives?: string;
  targetAudience?: string;
  prerequisites?: string;
  durationHours?: number;
  /** InPerson | Online | Hybrid. */
  deliveryMode?: string;
  cpdHours?: number;
  isExternal?: boolean;
  providerName?: string;
  externalUrl?: string;
  isActive?: boolean;
}

/** A training/development need (HC187), workflow-routed per type (HC188/HC201). */
export interface TrainingNeedModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  trainingCourseId?: string;
  courseName?: string;
  topic?: string;
  /** Local | Abroad. */
  needType?: string;
  justification?: string;
  /** Low | Medium | High | Critical. */
  priority?: string;
  /** Manual | CompetencyGap | Appraisal | Goal. */
  source?: string;
  /** Pending | Approved | Rejected | Fulfilled | Cancelled. */
  status?: string;
  competencyId?: string;
  competencyName?: string;
  estimatedCost?: number;
  neededBy?: string;
  requestedByEmployeeId?: string;
  requestedByName?: string;
  decidedOn?: string;
  fulfilledOn?: string;
}

/** A performance-driven training suggestion (HC189). */
export interface TrainingNeedSuggestionModel {
  /** CompetencyGap | Appraisal | Goal. */
  source?: string;
  title?: string;
  rationale?: string;
  competencyId?: string;
  competencyName?: string;
  goalId?: string;
}

/** A scheduled delivery of a course (HC197). */
export interface TrainingSessionModel extends AbstractModel {
  trainingCourseId?: string;
  courseName?: string;
  deliveryMode?: string;
  startDate?: string;
  endDate?: string;
  venue?: string;
  /** Internal | External. */
  trainerType?: string;
  trainerName?: string;
  providerName?: string;
  meetingUrl?: string;
  maxParticipants?: number;
  enrolledCount?: number;
  /** Scheduled | Completed | Cancelled. */
  status?: string;
  trainerCost?: number;
  materialsCost?: number;
  venueCost?: number;
  notes?: string;
}

/** One employee's participation in a session (HC198/HC199). */
export interface TrainingEnrollmentModel extends AbstractModel {
  trainingSessionId?: string;
  courseName?: string;
  sessionStartDate?: string;
  sessionEndDate?: string;
  sessionStatus?: string;
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  trainingNeedId?: string;
  /** Enrolled | Completed | NoShow | Withdrawn. */
  status?: string;
  attendancePercent?: number;
  assessmentScore?: number;
  completedOn?: string;
  feedbackRating?: number;
  feedbackComments?: string;
  cpdHours?: number;
}

/** A budget envelope (HC190). */
export interface TrainingBudgetModel extends AbstractModel {
  fiscalYear?: number;
  organizationUnitId?: string;
  organizationUnitName?: string;
  amount?: number;
  notes?: string;
}

export interface TrainingBudgetUtilizationModel {
  fiscalYear?: number;
  organizationUnitId?: string;
  budgetAmount: number;
  sessionCosts: number;
  committedNeedEstimates: number;
  utilized: number;
  remaining: number;
}

/** A structured learning path (HC193). */
export interface LearningPathStepModel {
  trainingCourseId?: string;
  courseName?: string;
  deliveryMode?: string;
  cpdHours?: number;
  sortOrder?: number;
  isRequired?: boolean;
}

export interface LearningPathModel extends AbstractModel {
  name?: string;
  description?: string;
  targetPositionId?: string;
  targetPositionName?: string;
  isActive?: boolean;
  stepCount?: number;
  steps?: LearningPathStepModel[];
}

export interface LearningPathProgressStepModel {
  trainingCourseId?: string;
  courseName?: string;
  sortOrder?: number;
  isRequired?: boolean;
  completed?: boolean;
}

export interface LearningPathProgressModel {
  learningPathId?: string;
  employeeId?: string;
  totalSteps: number;
  completedSteps: number;
  requiredSteps: number;
  completedRequiredSteps: number;
  progressPercent: number;
  steps: LearningPathProgressStepModel[];
}

/** A certification held by an employee (HC200). */
export interface TrainingCertificateModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  trainingCourseId?: string;
  courseName?: string;
  trainingEnrollmentId?: string;
  certificateNo?: string;
  title?: string;
  issuedOn?: string;
  expiresOn?: string;
  notes?: string;
}

/** CPD rollup (HC200). */
export interface CpdEntryModel {
  trainingCourseId?: string;
  courseName?: string;
  completedOn?: string;
  cpdHours?: number;
  assessmentScore?: number;
}

export interface CpdSummaryModel {
  employeeId?: string;
  year?: number;
  totalCpdHours: number;
  completedTrainings: number;
  certificates: number;
  entries: CpdEntryModel[];
}

/** Provider-payment hand-off row (HC202). */
export interface TrainingProviderPaymentModel extends AbstractModel {
  trainingSessionId?: string;
  courseName?: string;
  sessionStartDate?: string;
  providerName?: string;
  amount?: number;
  /** Pending | Paid | Cancelled. */
  status?: string;
  paidAt?: string;
  reference?: string;
  notes?: string;
}

/** A learning community (HC199). */
export interface LearningCommunityModel extends AbstractModel {
  name?: string;
  description?: string;
  trainingCourseId?: string;
  courseName?: string;
  isActive?: boolean;
  createdByEmployeeId?: string;
  createdByName?: string;
  memberCount?: number;
  postCount?: number;
  /** Whether the CALLER belongs to / moderates the community. */
  isMember?: boolean;
  isModerator?: boolean;
}

/** A discussion post — topic or single-level reply (HC198). */
export interface CommunityPostModel {
  id?: string;
  learningCommunityId?: string;
  employeeId?: string;
  authorName?: string;
  parentPostId?: string;
  content?: string;
  postedAt?: string;
  replies?: CommunityPostModel[];
}

/** A trackable company asset (HC214). */
export interface CompanyAssetModel extends AbstractModel {
  name?: string;
  /** ITEquipment | AccessCard | Key | Vehicle | Tool | Other. */
  category?: string;
  serialNo?: string;
  description?: string;
  /** Available | Assigned | Retired. */
  status?: string;
  assignedToEmployeeId?: string;
  assignedToName?: string;
  assignedToNumber?: string;
  assignedOn?: string;
}

/** One line of an exit's asset-recovery checklist (HC215). */
export interface AssetRecoveryModel {
  id?: string;
  terminationId?: string;
  companyAssetId?: string;
  assetName?: string;
  category?: string;
  serialNo?: string;
  /** Outstanding | Recovered | Waived. */
  status?: string;
  resolvedOn?: string;
  note?: string;
}

/** One configurable question (survey-JSON shape) used by exit interviews (HC219). */
export interface ExitQuestionModel {
  key?: string;
  text?: string;
  /** Rating | Choice | Text. */
  type?: string;
  options?: string[];
  required?: boolean;
}

/** An exit interview launched against a termination case (HC219). */
export interface ExitInterviewModel {
  id?: string;
  terminationId?: string;
  /** Pending | Completed. */
  status?: string;
  completedOn?: string;
  questions?: ExitQuestionModel[];
  answers?: Record<string, string>;
}

/** One final-settlement worksheet line (HC216). */
export interface SettlementLineModel {
  id?: string;
  /** Earning | Deduction. */
  kind?: string;
  label?: string;
  amount?: number;
  isAutoSuggested?: boolean;
}

/** The final settlement of an exit case (HC216/HC217). */
export interface TerminationSettlementModel {
  id?: string;
  terminationId?: string;
  /** Draft | Approved | Paid. */
  status?: string;
  approvedOn?: string;
  paidOn?: string;
  paidReference?: string;
  notes?: string;
  totalEarnings?: number;
  totalDeductions?: number;
  netAmount?: number;
  lines?: SettlementLineModel[];
}
