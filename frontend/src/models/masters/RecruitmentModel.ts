import type AbstractModel from "../AbstractModel";

/** Hiring-need assessment (HC077–HC083). */
export interface HiringRequestModel extends AbstractModel {
  requestNumber?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  positionClassId?: string;
  positionClassTitle?: string;
  jobGradeName?: string;
  numberOfPositions?: number;
  employmentType?: string;
  justification?: string;
  jobRequirements?: string;
  expectedStartDate?: string;
  timelineRemarks?: string;
  estimatedBudget?: number;
  workforcePlanId?: string;
  workforcePlanName?: string;
  status?: string; // Draft | Submitted | Approved | Rejected | Closed
  submittedAt?: string;
  approvedAt?: string;
  awaitingWorkflow?: boolean;
  vacantSeats?: number;
  requisitionedPositions?: number;
}

/** Per-unit recruitment budget/headcount monitor row (HC083). */
export interface RecruitmentBudgetRowModel {
  organizationUnitId: string;
  organizationUnitName?: string;
  approvedRequests: number;
  requestedPositions: number;
  estimatedBudget: number;
  openRequisitions: number;
}

export interface ScreeningCriterionModel {
  id?: string;
  name: string;
  isMandatory: boolean;
  weight: number;
  /** None | Employee | ExternalPerson | Organization — who scores this criterion. */
  evaluatorType?: string;
  evaluatorEmployeeId?: string;
  evaluatorName?: string;
}

/** One evaluator's score of one criterion (score sheet row). */
export interface CriterionScoreModel {
  criterionId: string;
  criterionName?: string;
  isMandatory: boolean;
  weight: number;
  evaluatorType?: string;
  evaluatorName?: string;
  score?: number;
  remarks?: string;
  scoredBy?: string;
  scoredAt?: string;
}

/** One row of a vacancy's auto-calculated candidate ranking. */
export interface ApplicationRankingRowModel {
  applicationId: string;
  candidateId: string;
  candidateNumber?: string;
  candidateName?: string;
  stage: string;
  totalScore?: number;
  scoredCriteria: number;
  totalCriteria: number;
  failsMandatory: boolean;
  breakdown: CriterionScoreModel[];
}

/** A file attached to a candidate (credentials + mandatory compliance set). */
export interface CandidateDocumentModel {
  id: string;
  documentType: string;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
}

/** Job requisition (HC084–HC088, HC091, HC095). */
export interface JobRequisitionModel extends AbstractModel {
  requisitionNumber?: string;
  hiringRequestId?: string;
  hiringRequestNumber?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  positionClassId?: string;
  positionClassTitle?: string;
  jobGradeName?: string;
  workLocationId?: string;
  workLocationName?: string;
  numberOfPositions?: number;
  employmentType?: string;
  title?: string;
  description?: string;
  minQualifications?: string;
  minExperienceYears?: number;
  skills?: string;
  salaryScaleId?: string;
  salaryScaleAmount?: number;
  postingChannel?: string; // Internal | External | Both
  postingText?: string;
  openFrom?: string;
  openUntil?: string;
  status?: string; // Draft | PendingApproval | Approved | Posted | Closed | Cancelled | Rejected
  submittedAt?: string;
  approvedAt?: string;
  postedAt?: string;
  closedAt?: string;
  awaitingWorkflow?: boolean;
  applicationCount?: number;
  screeningCriteria?: ScreeningCriterionModel[];
}

/** Centralized applicant record (HC092–HC097). */
export interface CandidateModel extends AbstractModel {
  candidateNumber?: string;
  firstName?: string;
  fatherName?: string;
  grandFatherName?: string;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  gender?: string;
  source?: string; // External | Internal | JobBoard | SocialMedia | Referral | WalkIn
  internalEmployeeId?: string;
  internalEmployeeName?: string;
  educationSummary?: string;
  experienceSummary?: string;
  skillsSummary?: string;
  yearsOfExperience?: number;
  resumeFileName?: string;
  consentGiven?: boolean;
  consentAt?: string;
  isArchived?: boolean;
  anonymizedAt?: string;
  isInTalentPool?: boolean;
  talentPoolNotes?: string;
  applicationCount?: number;
  /** The shared CorePerson record backing this candidate (hire-conversion anchor). */
  personId?: string;
  /** The employee this candidate became when hired. */
  hiredEmployeeId?: string;
  missingComplianceDocuments?: string[];
  complianceComplete?: boolean;
}

/** One ranked internal-matching result for a vacancy (HC090). */
export interface CandidateMatchModel {
  candidateId: string;
  candidateNumber: string;
  fullName: string;
  source: string;
  isInTalentPool: boolean;
  yearsOfExperience?: number;
  matchScore: number;
  matchedSkills: string[];
  meetsExperience: boolean;
}

export interface ApplicationStageLogModel {
  stage: string;
  note?: string;
  actedBy?: string;
  actedAt: string;
}

/** One candidate's application to one requisition (HC098–HC099). */
export interface JobApplicationModel extends AbstractModel {
  candidateId?: string;
  candidateNumber?: string;
  candidateName?: string;
  requisitionId?: string;
  requisitionNumber?: string;
  requisitionTitle?: string;
  stage?: string;
  appliedAt?: string;
  screeningScore?: number;
  screeningRemarks?: string;
  stageLog?: ApplicationStageLogModel[];
  /** The requisition's criteria merged with this application's scores (score sheet). */
  criterionScores?: CriterionScoreModel[];
}
